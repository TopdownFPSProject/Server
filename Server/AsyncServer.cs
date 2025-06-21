using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class Message
    {
        [JsonPropertyName("command")]
        public string Command { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("target")]
        public string Target { get; set; } = "all";
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; } = new();
    }

    public class PlayerData
    {
        public string id;

        public TcpClient client;
        public float x = 0, y = 0, z = 0;
        //public bool isShot = false;

        //public float prevX = 0, prevY = 0, prevZ = 0;

        //public bool HasMoved()
        //{
        //    if (x != prevX || y != prevY || z != prevZ) return true;
        //    return false;
        //}
    }

    public class BulletData
    {
        public string ownerId;
        public float x, y, z;
        public float dirX, dirY, dirZ;
    }

    public class AsyncServer
    {
        public static int index = 0;
        private TcpListener listener;

        //레이스 컨디셔닝을 피하기 위해 자료구조를 Concurrent로 변경
        public readonly ConcurrentDictionary<string, PlayerData> players = new();

        private readonly ConcurrentQueue<(string id, string input)> inputQueue = new();
        private readonly ConcurrentBag<BulletData> bullets = new();

        private readonly Dictionary<string, ICommandHandler> commandHandlers = new();

        private readonly object bulletLock = new();

        private const double tickRate = 30.0;
        private const double tickInterval = 1000.0 / tickRate;

        public AsyncServer()
        {
            commandHandlers = new()
            {
                ["connected"] = new ConnectCommandHandler(),
                ["disconnected"] = new DisconnectCommandHandler(),
                ["position"] = new MoveInputCommandHandler(),
                //["fire"] = new FireCommandHandler(),
            };

            //게임 별도 쓰레드 -> 임시로 주석
            //_ = Task.Run(PlayerMoveAsync);
        }

        public async Task StartAsync(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"[서버 시작] 포트 {port}");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);

                Console.WriteLine($"현재 플레이어 수 : {players.Count}");
            }
        }

        //입력 = 헤더 + 본문
        private async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096];

            try
            {
                while (true)
                {
                    int headerRead = 0;

                    //혹시 네트워크 상황에 따라 4바이트가 한번에 넘어오지 않을 수 있으므로 while로 작성
                    while (headerRead < 4)
                    {
                        int read = await stream.ReadAsync(buffer, headerRead, 4 - headerRead);
                        if (read == 0) return; //연결 끊김
                        headerRead += read;
                    }

                    int bodyLength = BitConverter.ToInt32(buffer, 0);
                    if (bodyLength <= 0 || bodyLength > buffer.Length - 4)
                    {
                        Console.WriteLine("잘못된 패킷 크기");
                        return;
                    }

                    int bodyRead = 0;
                    byte[] body = new byte[bodyLength];
                    while (bodyRead < body.Length)
                    {
                        int read = await stream.ReadAsync(body, bodyRead, bodyLength - bodyRead);
                        if (read == 0) return;
                        bodyRead += read;
                    }

                    string json = Encoding.UTF8.GetString(body);
                    Message msg = JsonSerializer.Deserialize<Message>(json);

                    if (msg != null && !string.IsNullOrEmpty(msg.Command))
                    {
                        if (commandHandlers.TryGetValue(msg.Command, out ICommandHandler handler))
                        {
                            handler.Execute(msg, client, this);
                            Console.WriteLine($"[넘어온 JSON] : {json}");
                        }
                        else
                        {
                            Console.WriteLine($"[알 수 없는 명령] : {msg.Command}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[예외] : {e.Message}");
            }
        }

        private void HandleClientMessage(string msg, TcpClient client)
        {
            Message message;
            try
            {
                message = JsonSerializer.Deserialize<Message>(msg);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[JSON] 파싱 오류 : {e.Message}");
                return;
            }

            if (message == null || string.IsNullOrEmpty(message.Command)) return;

            if (commandHandlers.TryGetValue(message.Command, out ICommandHandler handler))
            {
                handler.Execute(message, client, this);
            }
            else
            {
                Console.WriteLine($"[알 수 없는 명령] {message.Command}");
            }
        }

        //실제 움직임 제어
        //private async Task PlayerMoveAsync()
        //{
        //    while (true)
        //    {
        //        foreach (PlayerData player in players.Values)
        //        {
        //            if (player.client.Connected == false) continue;

        //            if (player.HasMoved() == false) continue;

        //            await Task.Delay(50); //0.25초 대기(1000 = 1초)

        //            string msg = $"position;{player.id};{player.x};{player.y};{player.z}";

        //            _ = SendAllClientAsync(msg);

        //            player.prevX = player.x;
        //            player.prevY = player.y;
        //            player.prevZ = player.z;
        //        }
        //    }
        //}

        #region broadcast (전체, 타겟, 제외)
        //전체에게 broadcast
        public async Task SendAllClientAsync(Message msg)
        {
            string json = JsonSerializer.Serialize(msg);
            byte[] body = Encoding.UTF8.GetBytes(json);
            byte[] header = BitConverter.GetBytes(body.Length);
            byte[] packet = new byte[header.Length + body.Length];

            Buffer.BlockCopy(header, 0, packet, 0, header.Length);
            Buffer.BlockCopy(body, 0, packet, header.Length, body.Length);

            List<PlayerData> snapshot = players.Values.ToList();
            List<Task> sendTasks = new List<Task>();

            foreach (PlayerData player in snapshot)
            {
                try
                {
                    if (player.client.Connected == false) continue;
                    NetworkStream stream = player.client.GetStream();
                    if (!stream.CanWrite) continue;
                    sendTasks.Add(stream.WriteAsync(packet, 0, packet.Length));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[전송오류] {e.Message}");
                }
            }

            await Task.WhenAll(sendTasks);
            Console.WriteLine($"[전송 완료] {msg}");
        }

        //타겟 broadcast
        public async Task SendTargetClientAsync(Message msg, TcpClient client)
        {
            string json = JsonSerializer.Serialize(msg);
            byte[] body = Encoding.UTF8.GetBytes(json);
            byte[] header = BitConverter.GetBytes(body.Length);
            byte[] packet = new byte[header.Length + body.Length];

            Buffer.BlockCopy(header, 0, packet, 0, header.Length);
            Buffer.BlockCopy(body, 0, packet, header.Length, body.Length);

            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(packet, 0, packet.Length);
        }

        //특정 id 제외한 broadcast
        public async Task SendExceptTargetAsync(Message msg, string exceptId)
        {
            string json = JsonSerializer.Serialize(msg);
            byte[] body = Encoding.UTF8.GetBytes(json);
            byte[] header = BitConverter.GetBytes(body.Length);
            byte[] packet = new byte[header.Length + body.Length];

            Buffer.BlockCopy(header, 0, packet, 0, header.Length);
            Buffer.BlockCopy(body, 0, packet, header.Length, body.Length);

            var snapshot = players.Values.Where(p => p.id != exceptId).ToList();
            List<Task> sendTasks = new List<Task>();

            foreach (var player in snapshot)
            {
                try
                {
                    if (player.client.Connected == false) continue;
                    var stream = player.client.GetStream();
                    if (!stream.CanWrite) continue;
                    sendTasks.Add(stream.WriteAsync(packet, 0, packet.Length));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[전송오류] {e.Message}");
                }
            }

            await Task.WhenAll(sendTasks);
        }
    }
    #endregion

    class Program
    {
        static async Task Main()
        {
            AsyncServer server = new AsyncServer();
            await server.StartAsync(7777);
        }
    }
}
