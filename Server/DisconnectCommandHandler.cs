using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class DisconnectCommandHandler : ICommandHandler
    {
        public void Execute(string[] args, TcpClient clinet, AsyncServer server)
        {
            string id = args[1];

            if (server.players.TryRemove(id, out var removed))
            {
                Console.WriteLine($"[정상 종료] {id}");

                string command = $"disconnected;{id};";
                //_ = server.SendAllClientAsync(command);

                //혹시 stream 제거가 먼저되면 이걸 키면 됨
                _ = server.SendAllClientAsync(command).ContinueWith(_ =>
                {
                    try 
                    { 
                        removed.client.Close(); 
                    } 
                    catch { }
                });
            }
        }
    }
}
