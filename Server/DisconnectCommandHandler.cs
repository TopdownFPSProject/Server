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
        public void Execute(Message msg, TcpClient client, AsyncServer server)
        {
            string id = msg.Id;

            if (server.players.TryRemove(id, out var removed))
            {
                Console.WriteLine($"[정상 종료] {id}");

                // disconnected 메시지 생성 (JSON)
                Message disconnectMsg = new Message
                {
                    Command = "disconnected",
                    Id = id
                };

                // 모든 클라이언트에게 알림 후 소켓 닫기
                _ = server.SendAllClientAsync(disconnectMsg).ContinueWith(_ =>
                {
                    try { removed.client.Close(); }
                    catch { }
                });
            }
        }
    }
}
