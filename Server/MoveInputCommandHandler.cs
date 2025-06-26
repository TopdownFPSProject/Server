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
            string[] posData = data.Split(';', StringSplitOptions.RemoveEmptyEntries);
            string id = posData[1];

            float x = Convert.ToSingle(posData[2]);
            float y = Convert.ToSingle(posData[3]);
            float z = Convert.ToSingle(posData[4]);

            // 서버 플레이어 위치 갱신(옵션)
            if (server.players.TryGetValue(id, out var player))
            {
                player.x = x;
                player.y = y;
                player.z = z;
            }
            //await server.PlayerMoveAsync();

            //string syncMsg = $"syncPosition;{id},{x},{y},{z}";
            //await server.SendExceptTargetAsync(syncMsg, id);
        }
    }
}
