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
        public async void Execute(Message msg, TcpClient clinet, AsyncServer server)
        {
            if (!server.players.TryGetValue(msg.Id, out var player)) return;

            float dirX = Convert.ToSingle(msg.Data["dirX"]);
            float dirY = Convert.ToSingle(msg.Data["dirY"]);
            float dirZ = Convert.ToSingle(msg.Data["dirZ"]);
            bool isMoving = Convert.ToBoolean(msg.Data["isMoving"]);

            float speed = 5f; //초당 이동 속도
            float deltaTime = 0.05f; //50ms마다 처리

            if (isMoving)
            {
                player.x += dirX * speed * deltaTime;
                player.y += dirY * speed * deltaTime;
                player.z += dirZ * speed * deltaTime;
            }

            var syncMsg = new Message
            {
                Command = "syncPosition",
                Id = player.id,
                Data = new Dictionary<string, object>
                {
                    { "x", player.x },
                    { "y", player.y },
                    { "z", player.z }
                }
            };

            await server.SendAllClientAsync(syncMsg);
        }
    }
}
