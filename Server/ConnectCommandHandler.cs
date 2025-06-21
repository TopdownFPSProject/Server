using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class ConnectCommandHandler : ICommandHandler
    {
        //스폰 포지션
        private float x, y, z = 0;

        public async void Execute(string data, TcpClient client, AsyncServer server)
        {
            string[] parts = data.Split(';');
            string id = parts[1];

            //연결시 PlayerData객체 생성
            PlayerData playerData = new PlayerData { id = id, client = client };
            server.players[id] = playerData;

            StringBuilder sb = new StringBuilder();
            sb.Append("playerList;");

            foreach (PlayerData p in server.players.Values)
            {
                sb.Append($"{p.id},{p.x},{p.y},{p.z}|");
            }

            string msg = sb.ToString();

            await server.SendTargetClientAsync(msg, client);

            StringBuilder sb2 = new StringBuilder();
            sb.Append($"playerJoined;{id},{playerData.x},{playerData.y},{playerData.z}");

            string msg2 = sb.ToString();

            await server.SendExceptTargetAsync(msg, id);

        }
    }
}
