using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using SharedPacketLib;

namespace Server
{
    internal class DisconnectCommandHandler : ICommandHandler<DisConnectPacket>
    {
        public void Execute(DisConnectPacket packet, TcpClient client, AsyncServer server)
        {
            string id = packet.Id;

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
