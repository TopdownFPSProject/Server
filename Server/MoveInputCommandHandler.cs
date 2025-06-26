using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class MoveInputCommandHandler : ICommandHandler
    {
        private const float moveSpeed = 5f;
        public void Execute(string data, TcpClient clinet, AsyncServer server)
        {
            string[] parts = data.Split(';', StringSplitOptions.RemoveEmptyEntries);
            string id = parts[1];

            if (!server.players.TryGetValue(id, out PlayerData player)) return;

            float dirX = float.Parse(parts[2]);
            float dirY = float.Parse(parts[3]); 
            float dirZ = float.Parse(parts[4]);
            Console.WriteLine("ddd");

            Vector3 direction = new Vector3(dirX, dirY, dirZ);

            float deltaTime = 1f / 30;
            Console.WriteLine("move");
            player.x += direction.X * moveSpeed * deltaTime;
            player.y += direction.Y * moveSpeed * deltaTime;
            player.z += direction.Z * moveSpeed * deltaTime;
            Console.WriteLine($"x :{player.x}, y : {player.y}, z : {player.z}");

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
