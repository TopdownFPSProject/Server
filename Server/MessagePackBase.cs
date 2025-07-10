using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [MessagePackObject]
    public class MessagePackBase
    {
        [Key(0)]
        public string Command { get; set; }
    }

    [MessagePackObject]
    [Command("connected")]
    public class  ConnectPacket : MessagePackBase
    {
        [Key(1)]
        public string Id { get; set; }
    }

    [MessagePackObject]
    [Command("disconnected")]
    public class DisConnectPacket : MessagePackBase
    {
        [Key(1)]
        public string Id { get; set; }
    }

    [Command("fire")]
    [MessagePackObject]
    public class FirePacket : MessagePackBase
    {

    }

    //[MessagePackObject]
    //[Command("moveinput")]
    //public class PositionPacket : MessagePackBase
    //{
    //    [Key(1)]
    //    public string Id { get; set; }
    //    [Key(2)]
    //    public float X { get; set; }
    //    [Key(3)]
    //    public float Y { get; set; }
    //    [Key(4)]
    //    public float Z { get; set; }
    //}
}
