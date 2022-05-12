using System.Net.Sockets;
using System.Text;
using Server;

namespace Client
{
    class Program
    {
        const int Port = 8888;
        const string Address = "127.0.0.1";
        static private NetworkStream? stream;
        static void Main(string[] args)
        {
            TcpClient client = null;
            try
            {
                client = new TcpClient(Address, Port);
                stream = client.GetStream();
                while (true)
                {
                    var tmp = Console.ReadLine();
                    if (tmp == "r")
                    {
                        SendCock("RECHARGING\\a\\b");
                        continue;
                    }

                    if (tmp == "f")
                    {
                        SendCock("FULL POWER\\a\\b");
                        continue;
                    }

                    SendCock(tmp);
                }
                List<KeyValuePair<int, int>> walls = new()
                {
                    new KeyValuePair<int, int>(-5, -9)
                };
                var x = -8;
                var y = -9;
                var dx = 1;
                var dy = 0;

                SendCock("Abobus\\a\\b");
                Console.WriteLine(ReceiveCock());
                SendCock("2\\a\\b");
                Console.WriteLine(ReceiveCock(), "32965\\a\\b");
                SendCock("27779\\a\\b");
                Console.WriteLine(ReceiveCock());


                while (x != 0 || y != 0)
                {
                    string message = ReceiveCock();
                    Console.WriteLine(message);
                    if (message == Types.GetRequest(Types.Codes.ServerMove))
                    {
                        x += dx;
                        y += dy;
                        var tmp = new KeyValuePair<int, int>(x, y);
                        if (walls.Contains(tmp))
                        {
                            x -= dx;
                            y -= dy;
                        }
                    }
                    if (message == Types.GetRequest(Types.Codes.ServerTurnRight))
                    {
                        var tmp = RotateRight(dx, dy);
                        dx = tmp.Key;
                        dy = tmp.Value;
                    }
                    if (message == Types.GetRequest(Types.Codes.ServerTurnLeft))
                    {
                        var tmp = RotateLeft(dx, dy);
                        dx = tmp.Key;
                        dy = tmp.Value;
                    }
                    Console.WriteLine("OK " + x + " " + y);
                    SendCock("OK " + x + " " + y + "\\a\\b");
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

        static private void SendCock(string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        static private string ReceiveCock()
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
            return message;
        }

        static private KeyValuePair<int, int> RotateRight(int x, int y)
        {
            if (x == -1)
                return new KeyValuePair<int, int>(0, 1);
            if (y == 1)
                return new KeyValuePair<int, int>(1, 0);
            if (x == 1)
                return new KeyValuePair<int, int>(0, -1);
            if (y == -1)
                return new KeyValuePair<int, int>(-1, 0);
            throw new NotImplementedException("fuck!");
        }

        static private KeyValuePair<int, int> RotateLeft(int x, int y)
        {
            var tmp = RotateRight(x, y);
            tmp = RotateRight(tmp.Key, tmp.Value);
            tmp = RotateRight(tmp.Key, tmp.Value);
            return tmp;
        }
    }
}