using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Program
    {
        const int port = 8888;
        const string address = "127.0.0.1";
        static bool _flag = true;
        static private NetworkStream stream;
        static void Main(string[] args)
        {
            TcpClient client = null;
            try
            {
                client = new TcpClient(address, port);
                stream = client.GetStream();

                while (true)
                {
                    SendCock();
                    if (_flag)
                        ReceiveCock();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }
        }

        static private void SendCock()
        {
            string message = Console.ReadLine();
            if (message == "r")
            {
                _flag = false;
                message = "RECHARGING\\a\\b";
                byte[] data1 = Encoding.UTF8.GetBytes(message);
                stream.Write(data1, 0, data1.Length);
                return;
            }

            if (message == "f")
            {
                _flag = false;
                message = "FULL POWER\\a\\b";
                byte[] data1 = Encoding.UTF8.GetBytes(message);
                stream.Write(data1, 0, data1.Length);
                return;
            }
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            _flag = true;
        }

        static private void ReceiveCock()
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);
 
            string message = builder.ToString();
            Console.WriteLine("Сервер: {0}", message.Replace("\\a\\b", ""));
        }
    }
}