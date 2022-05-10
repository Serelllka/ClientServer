using System.Net.Sockets;
using System.Text;
using static System.Console;

namespace Server
{
    public class ClientObject
    {
        private TcpClient _client;
        private User _authUser;
        private NetworkStream _stream;
        private ClientConfig _config;
        private bool _checked; 
        public ClientObject(TcpClient tcpClient)
        {
            _stream = null;
            _authUser = null;
            _config = new ClientConfig(20);
            _client = tcpClient;
            _checked = false;
        }
 
        public void Process()
        {
            WriteLine("new session!");
            try
            {
                _stream = _client.GetStream();
                while (true)
                {
                    var message = ReceiveMessage();
                    WriteLine(message);
                    
                    if (message == Types.GetRequest(Types.Codes.ClientRecharging))
                    {
                        WriteLine("ClientRecharging...");
                        ClientRecharging();
                        continue;
                    }
                    
                    if (_authUser is null)
                    {
                        WriteLine("Auth...");
                        Auth(message);
                        continue;
                    }

                    if (_authUser.ClientKeyId == -1)
                    {
                        WriteLine("ServerConfirm...");
                        ServerConfirm(message);
                        continue;
                    }

                    if (!_checked)
                    {
                        WriteLine("ServerCheck...");
                        ServerCheck(message);
                        _checked = true;
                        continue;
                    }
                    
                    if (message == Types.GetRequest(Types.Codes.ClientFullPower))
                    {
                        WriteLine("Error...");
                        SendMessage(Types.GetRequest(Types.Codes.ServerLogicError));
                        throw new Exception(Types.GetMessage(Types.Codes.ServerLogicError));
                    }
                }
            }
            catch(Exception ex)
            {
                WriteLine(ex.Message);
            }
            finally
            {
                WriteLine("Closing connection...");
                _stream.Close();
                _client.Close();
            }
        }

        private void Auth(string userName)
        {
            Validate(Types.Codes.ClientUsername, userName, _config.MaxNameLen);
            
            WriteLine("User: " + userName);
            userName = Types.ToMessage(userName);
            _authUser = new User(userName);
            SendMessage(Types.GetRequest(Types.Codes.ServerKeyRequest));
        }

        private void ClientRecharging() 
        {
            _client.ReceiveTimeout = 5000;
            string message = ReceiveMessage();
            if (message != Types.GetRequest(Types.Codes.ClientFullPower))
            {
                SendMessage(Types.GetRequest(Types.Codes.ServerLogicError));
                throw new Exception(Types.GetMessage(Types.Codes.ServerLogicError));
            }
            _client.ReceiveTimeout = 1000000;
        }

        private string ReceiveMessage()
        {
            var builder = new StringBuilder();
            var data = new byte[64];
            var bytes = 0;
            do
            {
                // getting user name
                bytes = _stream.Read(data, 0, data.Length);
                builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
            }
            while (_stream.DataAvailable);
            string message = builder.ToString();
            return message;
        }

        private void SendMessage(string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            _stream.Write(data, 0, data.Length);
        }

        private void ServerConfirm(string message)
        {
            Validate(Types.Codes.ClientKeyId, message, 5);
            _authUser.ClientKeyId = Convert.ToInt32(message);
            if (_authUser.ClientKeyId is < 0 or > 4)
            {
                SendMessage(Types.GetRequest(Types.Codes.ServerKeyOutOfRangeError));
                throw new Exception(Types.GetMessage(Types.Codes.ServerKeyOutOfRangeError));
            }
            
            var sum = _authUser.Name.Sum(t => t * 1000);
            _authUser.Hash = sum;
            sum += Types.Ids[_authUser.ClientKeyId].Key;
            sum %= 65536;
            SendMessage(Types.ToRequest(sum.ToString()));
        }

        private void ServerCheck(string message)
        {
            Validate(Types.Codes.ClientConfirmation, message, 9);
            var sum = _authUser.Hash;
            sum += Types.Ids[_authUser.ClientKeyId].Value;
            sum %= 65536;
            if (sum == Convert.ToInt32(message))
            {
                SendMessage(Types.GetRequest(Types.Codes.ServerOk));
                return;
            }
            
            SendMessage(Types.GetRequest(Types.Codes.ServerLoginFailed));
            throw new Exception(Types.GetMessage(Types.Codes.ServerLoginFailed));
        }

        void Validate(Types.Codes code, string message, int len)
        {
            if (Types.IsValid(code, message, len))
            {
                return;
            }
            SendMessage(Types.GetRequest(Types.Codes.ServerSyntaxError));
            throw new Exception(Types.GetMessage(Types.Codes.ServerSyntaxError));
        }
    }
}