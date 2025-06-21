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
        public void Execute(string data, TcpClient client, AsyncServer server)
        {
            string[] parts = data.Split(';', StringSplitOptions.RemoveEmptyEntries);
            string id = parts[1];

            if (server.players.TryRemove(id, out PlayerData removed))
            {
                Console.WriteLine($"[정상 종료] {id}");

                string msg = $"disconnected;{id};";

                _ = server.SendExceptTargetAsync(msg, id).ContinueWith(_ =>
                {
                    //소켓이 완전히 닫힐때까지 대기
                    try { removed.client.Close(); }
                    catch { }
                });
            }
        }
    }
}
