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

                while(true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    client.ReceiveTimeout = 100000;
                    ClientObject clientObject = new ClientObject(client);
                    Thread clientThread = new Thread(clientObject.Process);
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