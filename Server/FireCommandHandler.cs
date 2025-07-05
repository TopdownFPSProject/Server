using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{

    internal class FireCommandHandler : ICommandHandler<FirePacket>
    {
        public void Execute(FirePacket packet, TcpClient clinet, AsyncServer server)
        {
            //string id = parts[1];
            //string x = parts[2];
            //string y = parts[3];
            //string z = parts[4];
            //string dirX = parts[5];
            //string dirY = parts[6];
            //string dirZ = parts[7];
            //string time = parts[8];

            //string command = $"fire;{id};{x};{y};{z};{dirX};{dirY};{dirZ};{time}";
            ////Console.WriteLine(command);
            //_ = server.SendAllClientAsync(command);
        }
    }
}
