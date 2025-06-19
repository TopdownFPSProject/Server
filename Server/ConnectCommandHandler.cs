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
        public void Execute(Message msg, TcpClient client, AsyncServer server)
        {
            string id = msg.Id;

            //연결시 PlayerData객체 생성
            PlayerData data = new PlayerData { id = id, client = client };
            server.players[id] = data;

            //playerList를 먼저 접속한 플레이어에게 넘겨줌
            Message playerListMsg = new Message
            {
                Command = "playerList",
                Data = new Dictionary<string, object>
                {
                    ["players"] = server.players.Values.Select(p => new Dictionary<string, object>
                    {
                        ["id"] = p.id,
                        ["x"] = 0,
                        ["y"] = 0,
                        ["z"] = 0,    
                    }).ToList()
                }
            };
            server.SendMessageAsync(playerListMsg, client).Wait();

            // 먼저 접속한 플레이어들에게 새 플레이어 알림
            Message joinedMsg = new Message
            {
                Command = "playerJoined",
                Id = id,
                Data = new Dictionary<string, object>
                {
                    ["x"] = data.x,
                    ["y"] = data.y,
                    ["z"] = data.z
                }
            };
            server.SendAllExceptAsync(joinedMsg, id).Wait();
        }
    }
}
