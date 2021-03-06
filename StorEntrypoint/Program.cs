using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Extensions;

namespace StorEntrypoint
{
    class Program
    {
        static MessageQueue queue = new MessageQueue();
        static void Main(string[] args)
        {
            queue.CreateExchange(RabbitMQExchangeTypes.Direct, "stor");
            queue.BindServices("stor", Services.Response);

            try
            {
                int port = 13000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                using IMemoryOwner<byte> memory = MemoryPool<byte>.Shared.Rent(1024 * 4);
                

                TcpListener listener = new TcpListener(localAddr, port);
                listener.Start();

                while (true)
                {
                    Console.Write("Waiting for a connection... ");
                    var client = listener.AcceptSocket();
                    Task t = new Task(() => HandleClient(client, memory));
                    t.Start();
                    Console.WriteLine("Connected!");
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        static void HandleClient(Socket client, IMemoryOwner<byte> memory)
        {
            while (client.Connected)
            {
                try
                {
                    NetworkFile<string[]> data = null;

                    ReadOnlyMemory<byte> request = memory.Memory.Slice(0, client.Receive(memory.Memory.Span));
                    data = Json.DeserializeFromMemory<NetworkFile<string[]>>(request);
                    Console.WriteLine("Received data for: {0}", data.Service);

                    var response = queue.SendAsRpc<NetworkFile<string[]>, NetworkFile<string[]>>(data, true);
                    foreach (var item in response.Info)
                    {
                        Console.WriteLine(item);
                    }
                    Console.WriteLine($"{response.Info.ToString()}");
                    client.Send(Json.SerializeToBytes(response));
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ERROR OCCURED");
                    Console.ForegroundColor = ConsoleColor.White;
                    client.Close();
                }
            }
        }
    }
}
