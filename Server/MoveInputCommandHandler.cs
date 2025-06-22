using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class MoveInputCommandHandler : ICommandHandler
    {
        public async void Execute(string data, TcpClient clinet, AsyncServer server)
        {
            string body = data.Substring("moveInput;".Length);
            string[] posData = body.Split(',', StringSplitOptions.RemoveEmptyEntries);
            string id = posData[0];

            if (!server.players.TryGetValue(id, out var player)) return;

            float dirX = Convert.ToSingle(posData[1]);
            float dirZ = Convert.ToSingle(posData[2]);
            bool isMoving = Convert.ToBoolean(posData[3]);

            float speed = 5f; //초당 이동 속도
            float deltaTime = 0.05f; //50ms마다 처리

            if (isMoving)
            {
                player.x += dirX * speed * deltaTime;
                player.z += dirZ * speed * deltaTime;
            }

            string syncMsg = $"syncPosition;{id},{player.x},{player.z}";
            
            await server.SendAllClientAsync(syncMsg);
        }
    }
}
