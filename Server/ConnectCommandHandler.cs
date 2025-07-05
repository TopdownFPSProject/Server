using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MessagePack;

namespace Server
{
    internal class ConnectCommandHandler : ICommandHandler<ConnectPacket>
    {
        [MessagePackObject]
        [Command("playerList")]
        public class PlayerListPacket : MessagePackBase
        {
            [Key(1)]
            public List<PlayerInfo> Players { get; set; }
        }

        [MessagePackObject]
        [Command("playerList")]
        public class PlayerInfo
        {
            [Key(0)] 
            public string Id { get; set; }
            [Key(1)] 
            public float X { get; set; }
            [Key(2)] 
            public float Y { get; set; }
            [Key(3)] 
            public float Z { get; set; }
        }

        [MessagePackObject]
        [Command("playerJoined")]
        public class PlayerJoinedPacket : MessagePackBase
        {
            [Key(1)] public string Id { get; set; }
            [Key(2)] public float X { get; set; }
            [Key(3)] public float Y { get; set; }
            [Key(4)] public float Z { get; set; }
        }

        //스폰 포지션
        private float x, y, z = 0;

        public async void Execute(ConnectPacket packet, TcpClient client, AsyncServer server)
        {
            string id = packet.Id;

            //연결시 PlayerData객체 생성
            PlayerData playerData = new PlayerData { id = id, client = client };
            server.players[id] = playerData;

            // 1. playerList 패킷 생성 및 전송
            List<PlayerInfo> playerList = server.players.Values
                .Select(p => new PlayerInfo { Id = p.id, X = p.x, Y = p.y, Z = p.z })
                .ToList();

            PlayerListPacket listPacket = new PlayerListPacket
            {
                Command = "playerList",
                Players = playerList
            };
            await server.SendTargetClientAsync(listPacket, client);

            // 2. playerJoined 패킷 생성 및 전송
            PlayerJoinedPacket joinedPacket = new PlayerJoinedPacket
            {
                Command = "playerJoined",
                Id = id,
                X = playerData.x,
                Y = playerData.y,
                Z = playerData.z
            };
            await server.SendExceptTargetAsync(joinedPacket, id);
        }
    }
}
