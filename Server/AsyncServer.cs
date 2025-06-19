using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class PlayerData
    {
        public string id;


        public TcpClient client;
        public float x = 0, y = 0, z = 0;
        public bool isShot = false;

        public float prevX = 0, prevY = 0, prevZ = 0;

        public bool HasMoved()
        {
            if (x != prevX || y != prevY || z != prevZ) return true;
            return false;
        }
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
                ["position"] = new PositionCommandHandler(),
                ["fire"] = new FireCommandHandler(),
            };

            //게임 별도 쓰레드
            _ = Task.Run(PlayerMoveAsync);
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
            byte[] buffer = new byte[1024];

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

                    string message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"[받음]{message}");

                    HandleClientMessage(message, client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[예외] : {ex.Message}");
            }
        }

        private void HandleClientMessage(string msg, TcpClient client)
        {
            string[] parts = msg.Split(';', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) return;

            string command = parts[0];

            if (commandHandlers.TryGetValue(command, out var handler))
            {
                handler.Execute(parts, client, this);
            }
            else
            {
                Console.WriteLine($"[알 수 없는 명령] {command}");
            }
        }

        //실제 움직임 제어
        private async Task PlayerMoveAsync()
        {
            while (true)
            {
                foreach (PlayerData player in players.Values)
                {
                    if (player.client.Connected == false) continue;

                    if (player.HasMoved() == false) continue;

                    await Task.Delay(50); //0.25초 대기(1000 = 1초)

                    string msg = $"position;{player.id};{player.x};{player.y};{player.z}";
                    
                    _ = SendAllClientAsync(msg);

                    player.prevX = player.x;
                    player.prevY = player.y;
                    player.prevZ = player.z;
                }
            }
        }

        //메시지 보는 부분은 비동기로 처리
        public async Task SendAllClientAsync(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;

            byte[] body = Encoding.UTF8.GetBytes(msg);
            byte[] header = BitConverter.GetBytes(body.Length);
            byte[] packet = new byte[header.Length + body.Length];

            Buffer.BlockCopy(header, 0, packet, 0, header.Length);
            Buffer.BlockCopy(body, 0, packet, header.Length, body.Length);

            //플레이어가 중간에 없어지는 경우도 있을 수 있어서 보낼때의 List를 파악
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
                    //Console.WriteLine($"클라이언트에게 보낸 메시지 : {msg}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[전송오류] {e.Message}");
                }
            }

            await Task.WhenAll(sendTasks);
            Console.WriteLine($"[전송 완료] {msg}");
        }
    }

    class Program
    {
        static async Task Main()
        {
            AsyncServer server = new AsyncServer();
            await server.StartAsync(7777);
        }
    }
}
