using SharedPacketLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class MoveInputCommandHandler : ICommandHandler<C_InputPacket>
    {
        private const float moveSpeed = 5f;
        public void Execute(C_InputPacket packet, TcpClient clinet, AsyncServer server)
        {
            string id = packet.Id;

            if (!server.players.TryGetValue(id, out PlayerData player)) return;

            Vector3 direction = new Vector3(packet.X, packet.Y, packet.Z);

            float deltaTime = 1f / 30;
            player.x += direction.X * moveSpeed * deltaTime;
            player.y += direction.Y * moveSpeed * deltaTime;
            player.z += direction.Z * moveSpeed * deltaTime;

            // 서버 플레이어 위치 갱신(옵션)
            //if (server.players.TryGetValue(id, out var player))
            //{
            //    player.x = x;
            //    player.y = y;
            //    player.z = z;
            //}
            //await server.PlayerMoveAsync();

            //string syncMsg = $"syncPosition;{id},{x},{y},{z}";
            //await server.SendExceptTargetAsync(syncMsg, id);
        }
    }
}
