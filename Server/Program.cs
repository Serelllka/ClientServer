using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Server;

namespace ConsoleServer
{
    class Program
    {
        const int Port = 8888;
        private const string Addr = "127.0.0.1";
        static TcpListener listener;
        static void Main(string[] args)
        {
            try
            {
                listener = new TcpListener(IPAddress.Parse(Addr), Port);
                listener.Start();

                var config = new ServerConfig
                {
                    MessageSuffix = "\\a\\b",
                    ReceiveTimeout = 100000,
                    RechargingTimeout = 5000,
                    MaxId = 4,
                    MinId = 0
                };
                config.SetMaxLenght(Types.Codes.ClientUsername, 20);
                config.SetMaxLenght(Types.Codes.ClientKeyId, 5);
                config.SetMaxLenght(Types.Codes.ClientConfirmation, 9);
                config.SetMaxLenght(Types.Codes.ClientFullPower, 20);
                config.SetMaxLenght(Types.Codes.ClientRecharging, 20);
                config.SetMaxLenght(Types.Codes.ClientOk, 12);
                
                
                while(true)
                {
                    var client = listener.AcceptTcpClient();
                    client.ReceiveTimeout = config.ReceiveTimeout;
                    var clientObject = new ClientObject(client, config);
                    var clientThread = new Thread(clientObject.Process);
                    clientThread.Start();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}