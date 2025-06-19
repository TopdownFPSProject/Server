using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class FireCommandHandler //: ICommandHandler
    {
        public void Execute(string[] args, TcpClient clinet, AsyncServer server)
        {
            string id = args[1];
            //위치
            string x = args[2];
            string y = args[3];
            string z = args[4];
            //바라보는 방향
            string dirX = args[5];
            string dirY = args[6];
            string dirZ = args[7];
            //보낸 시간
            string time = args[8];

            //쏘는 방향은 다르게 해야하긴 함(ex 플레이어가 보는 방향)
            //일단 쏜 시간을 기준으로 총알 오브젝트 만들고 더 최적화 하는 방법을 생각해보자
            string command = $"fire;{id};{x};{y};{z};{dirX};{dirY};{dirZ};{time};";
            //_ = server.SendAllClientAsync(command);
        }
    }
}
