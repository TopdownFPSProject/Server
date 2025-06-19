using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class PositionCommandHandler : ICommandHandler
    {
        public void Execute(string[] args, TcpClient clinet, AsyncServer server)
        {
            string id = args[1];
            float x = float.Parse(args[2]);
            float y = float.Parse(args[3]);
            float z = float.Parse(args[4]);

            if (server.players.TryGetValue(id, out var p))
            {
                p.x = x;
                p.y = y;
                p.z = z;
            }
        }
    }
}
