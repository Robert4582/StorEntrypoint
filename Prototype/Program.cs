using System;
using System.Net.Sockets;
using System.Threading;
using Common;

namespace Prototype
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.ReadKey();
            TcpClient client = new TcpClient("127.0.0.1", 13000);

            byte[] data = Json.SerializeToBytes(new NetworkFile<string[]>()
            {
                CorrelationID="",
                Timeout=1000,
                Service = Services.Create,
                Info = new string[] {"Hi!", "Please"}
            });

            NetworkStream stream = client.GetStream();
            for (int i = 0; i < 40; i++)
            {
                Thread.Sleep(500);
                stream.Write(data, 0, data.Length);
            }
        }
    }
}
