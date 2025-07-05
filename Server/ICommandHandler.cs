using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Server
{
    public interface ICommandHandler<T> where T : MessagePackBase
    {
        void Execute(T packet, TcpClient client, AsyncServer server);
    }
}
