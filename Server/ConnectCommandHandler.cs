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
        public void Execute(string[] args, TcpClient client, AsyncServer server)
        {
            string id = args[1];

            //연결시 PlayerData객체 생성
            PlayerData data = new PlayerData { id = id, client = client };
            server.players[id] = data;

            StringBuilder sb = new StringBuilder("connected;");
            List<PlayerData> snapshot = server.players.Values.ToList();
            
            foreach (var player in snapshot)
            {
                sb.Append($"{player.id};");
            }

            _ = server.SendAllClientAsync(sb.ToString());
        }
    }
}
