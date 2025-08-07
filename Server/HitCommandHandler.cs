using SharedPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class HitCommandHandler : ICommandHandler<C_HitPacket>
    {
        public void Execute(C_HitPacket packet, TcpClient client, AsyncServer server)
        {
            // 쏜 사람이 연결이 되어있어야함
            if (client.Connected)
            {
                S_HitInfoPacket hitInfo = new S_HitInfoPacket()
                {
                    shooter = packet.shooter,
                    target = packet.target,
                    damage = 10 // 테스트
                };

                foreach (BulletData bullet in server.bullets)
                {
                    if (bullet.spawnTime == packet.spawnedTime)
                    {
                        Console.WriteLine("총알 삭제 됨");
                        server.bullets.Remove(bullet);
                        break;
                    }
                }

                _ = server.SendAllClientAsync(hitInfo);
            }
        }
    }
}
