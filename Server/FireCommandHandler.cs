using SharedPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server
{

    internal class FireCommandHandler : ICommandHandler<C_FirePacket>
    {
        public void Execute(C_FirePacket packet, TcpClient clinet, AsyncServer server)
        {
            string id = packet.Id;
            Vector3 spawnPos = new Vector3(packet.X, packet.Y, packet.Z);
            float angle = packet.Angle;
            long time = packet.SpawnTime;

            BulletData bullet = new BulletData()
            {
                ownerId = id,
                spawnPos = spawnPos,
                angle = angle,
                spawnTime = time,
            };

            server.bullets.Add(bullet);

            S_bulletPacket bulletData = new S_bulletPacket()
            {
                Id = id,
                X = packet.X,
                Y = packet.Y,
                Z = packet.Z,
                Angle = angle,
                SpawnTime = time,
            };

            //string command = $"fire;{id};{x};{y};{z};{dirX};{dirY};{dirZ};{time}";
            ////Console.WriteLine(command);
            _ = server.SendAllClientAsync(bulletData);
        }
    }
}
