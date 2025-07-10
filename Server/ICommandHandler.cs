using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using SharedPacketLib;

namespace Server
{
    public interface ICommandHandler<T> where T : PacketBase
    {
        void Execute(T packet, TcpClient client, AsyncServer server);
    }
}
