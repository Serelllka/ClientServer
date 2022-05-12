using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using static System.Console;

namespace Server
{
    public class ClientObject
    {
        private readonly TcpClient _client;
        private User _authUser;
        private NetworkStream _stream;
        private readonly ServerConfig _config;
        private bool _checked;
        private readonly Validator _validator;
        private string _prevString;
        
        public ClientObject(TcpClient tcpClient, ServerConfig config)
        {
            _prevString = "";
            _stream = null;
            _authUser = null;
            _validator = new Validator(config);
            _config = config;
            _client = tcpClient;
            _checked = false;
        }
 
        public void Process()
        {
            WriteLine("Starting new session!... ");
            try
            {
                _stream = _client.GetStream();
                var message = ReceiveNotRechargingMessage(Types.Codes.ClientUsername);
                Auth(message);
                
                message = ReceiveNotRechargingMessage(Types.Codes.ClientKeyId);
                ServerConfirm(message);
                
                message = ReceiveNotRechargingMessage(Types.Codes.ClientConfirmation);
                ServerCheck(message);

                RunAlgo();
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
            _authUser = new User(userName);
            SendMessage(Types.GetRequest(Types.Codes.ServerKeyRequest));
        }

        private bool IsClientRecharging(Types.Codes code, string input) 
        {
            if (input != Types.GetRequest(Types.Codes.ClientRecharging))
            {
                Validate(code, input);
                return false;
            }

            _client.ReceiveTimeout = _config.RechargingTimeout;
            var message = ReceiveMessage(code);
            Validate(Types.Codes.ClientFullPower, message);
            
            _client.ReceiveTimeout = _config.ReceiveTimeout;
            return true;
        }

        private string ReceiveNotRechargingMessage(Types.Codes code)
        {
            var message = ReceiveMessage(code);
            while (IsClientRecharging(code, message))
            {
                message = ReceiveMessage(code);
            }

            return Types.ToMessage(message, _config);
        }
        private string ReceiveMessage(Types.Codes code)
        {
            var builder = new StringBuilder(_prevString);
            var data = new byte[128];
            if (!_prevString.Contains(_config.MessageSuffix))
            {
                do
                {
                    var bytes = _stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                } while (!builder.ToString().Contains(_config.MessageSuffix) &&
                         builder.ToString().Length < _config.MaxLength[code]);
            }

            var message = builder.ToString();
            
            WriteLine("MESSAGE RECEIVED: " + message);
            return GetLastMessage(message);
        }

        private string GetLastMessage(string message)
        {
            if (!message.Contains(_config.MessageSuffix))
            {
                SendMessage(Types.GetRequest(Types.Codes.ServerSyntaxError));
                throw new Exception(Types.GetMessage(Types.Codes.ServerSyntaxError, _config));
            }
            var val = message.IndexOf(_config.MessageSuffix, StringComparison.Ordinal);
            _prevString = message[(val + _config.MessageSuffix.Length)..];
            return message[..val] + _config.MessageSuffix;
        }

        private void SendMessage(string message)
        {
            WriteLine("MESSAGE SENT: " + message);
            var data = Encoding.UTF8.GetBytes(message);
            _stream.Write(data, 0, data.Length);
        }

        private void ServerConfirm(string message)
        {
            _authUser.ClientKeyId = Convert.ToInt32(message);
            
            var sum = _authUser.Name.Sum(t => t * 1000);
            _authUser.Hash = sum;
            sum += Types.Ids[_authUser.ClientKeyId].Key;
            sum %= 65536;
            SendMessage(Types.ToRequest(sum.ToString(), _config));
        }

        private void ServerCheck(string message)
        {
            var sum = _authUser.Hash;
            sum += Types.Ids[_authUser.ClientKeyId].Value;
            sum %= 65536;
            if (sum == Convert.ToInt32(message))
            {
                SendMessage(Types.GetRequest(Types.Codes.ServerOk));
                return;
            }
            
            SendMessage(Types.GetRequest(Types.Codes.ServerLoginFailed));
            throw new Exception(Types.GetMessage(Types.Codes.ServerLoginFailed, _config));
        }

        private Types.Position ServerMove(Types.Codes code)
        {
            SendMessage(Types.GetRequest(code));
            var message = ReceiveNotRechargingMessage(Types.Codes.ClientOk);

            return new Types.Position(
                Convert.ToInt32(message.Split(" ").ToList()[1]),
                Convert.ToInt32(message.Split(" ").ToList()[2])
            );
        }

        private void Validate(Types.Codes code, string message)
        {
            var value = _validator.Validate(code, message);

            if (value is Types.Codes.ValidMessage) return;
            SendMessage(Types.GetRequest(value));
            throw new Exception(Types.GetMessage(value, _config));
        }

        private void ChangeDirection(Types.Direction from, Types.Direction to)
        {
            if (from == to)
                return;

            var delta = to - from;
            switch (delta)
            {
                case -3 or 1:
                    ServerMove(Types.Codes.ServerTurnRight);
                    return;
                case -2 or 2:
                    ServerMove(Types.Codes.ServerTurnRight);
                    ServerMove(Types.Codes.ServerTurnRight);
                    return;
                case -1 or 3:
                    ServerMove(Types.Codes.ServerTurnLeft);
                    break;
            }
        }

        private void RunAlgo()
        {
            var prevPosition = ServerMove(Types.Codes.ServerMove);
            var currPosition = ServerMove(Types.Codes.ServerMove);
            if (currPosition == prevPosition)
            {
                ServerMove(Types.Codes.ServerTurnRight);
                currPosition = ServerMove(Types.Codes.ServerMove);
            }
            
            var currDirection = Algorithm.GetMotionVector(prevPosition, currPosition);
            while (currPosition.Y != 0 || currPosition.X != 0)
            {
                var directions = Algorithm.GetDirections(currPosition);
                while (currPosition.X != 0)
                {
                    ChangeDirection(currDirection, directions.Key);
                    currDirection = directions.Key;
                    
                    prevPosition = currPosition;
                    currPosition = ServerMove(Types.Codes.ServerMove);
                    if (prevPosition == currPosition)
                    {
                        ChangeDirection(currDirection, directions.Value);
                        currDirection = directions.Value;
                        currPosition = ServerMove(Types.Codes.ServerMove);
                    }
                    
                    directions = Algorithm.GetDirections(currPosition);
                }
                
                directions = Algorithm.GetDirections(currPosition);
                while (currPosition.Y != 0)
                {
                    ChangeDirection(currDirection, directions.Value);
                    currDirection = directions.Value;
                    
                    prevPosition = currPosition;
                    currPosition = ServerMove(Types.Codes.ServerMove);
                    if (prevPosition == currPosition)
                    {
                        ChangeDirection(currDirection, directions.Key);
                        currDirection = directions.Key;
                        currPosition = ServerMove(Types.Codes.ServerMove);
                    }
                    
                    directions = Algorithm.GetDirections(currPosition);
                }
            }
            
            SendMessage(Types.GetRequest(Types.Codes.ServerPickUp));
            ReceiveNotRechargingMessage(Types.Codes.ClientMessage);
            SendMessage(Types.GetRequest(Types.Codes.ServerLogout));
        }
    }
}