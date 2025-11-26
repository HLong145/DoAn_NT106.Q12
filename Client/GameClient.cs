using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DoAn_NT106.Services
{
    /// <summary>
    /// Client dùng để lắng nghe broadcast từ server trong trận đấu
    /// </summary>
    public class GameClient : IDisposable
    {
        private TcpClient client;
        private NetworkStream stream;
        private CancellationTokenSource cts;
        private Task listenTask;
        private readonly string serverAddress;
        private readonly int serverPort;

        // Events để UI có thể subscribe
        public event Action<GameUpdateData> OnGameUpdate;
        public event Action<PlayerJoinedData> OnPlayerJoined;
        public event Action<PlayerLeftData> OnPlayerLeft;
        public event Action<StartGameData> OnStartGame;
        public event Action<string> OnError;

        public bool IsConnected => client?.Connected ?? false;

        public GameClient(string address = "127.0.0.1", int port = 8080)
        {
            serverAddress = address;
            serverPort = port;
        }

        // ===========================
        // KẾT NỐI VÀ BẮT ĐẦU LẮNG NGHE
        // ===========================
        public async Task<bool> ConnectAsync()
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(serverAddress, serverPort);
                stream = client.GetStream();

                cts = new CancellationTokenSource();
                listenTask = Task.Run(() => ListenForBroadcasts(cts.Token));

                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Connection error: {ex.Message}");
                return false;
            }
        }

        // ===========================
        // LẮNG NGHE BROADCAST TỪ SERVER
        // ===========================
        private async Task ListenForBroadcasts(CancellationToken token)
        {
            byte[] buffer = new byte[8192];

            try
            {
                while (!token.IsCancellationRequested && client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);

                    if (bytesRead == 0)
                        break;

                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessBroadcast(json);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Listen error: {ex.Message}");
            }
        }

        // ===========================
        // XỬ LÝ BROADCAST
        // ===========================
        private void ProcessBroadcast(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("Action", out var actionElement))
                    return;

                string action = actionElement.GetString();

                switch (action)
                {
                    case "GAME_UPDATE":
                        HandleGameUpdate(root);
                        break;

                    case "PLAYER_JOINED":
                        HandlePlayerJoined(root);
                        break;

                    case "PLAYER_LEFT":
                        HandlePlayerLeft(root);
                        break;

                    case "START_GAME":
                        HandleStartGame(root);
                        break;
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Process broadcast error: {ex.Message}");
            }
        }

        // ===========================
        // XỬ LÝ CÁC LOẠI BROADCAST
        // ===========================
        private void HandleGameUpdate(JsonElement root)
        {
            try
            {
                if (!root.TryGetProperty("Data", out var data))
                    return;

                var gameState = data.GetProperty("gameState");

                var updateData = new GameUpdateData
                {
                    Player1Health = gameState.GetProperty("Player1Health").GetInt32(),
                    Player2Health = gameState.GetProperty("Player2Health").GetInt32(),
                    Player1Stamina = gameState.GetProperty("Player1Stamina").GetInt32(),
                    Player2Stamina = gameState.GetProperty("Player2Stamina").GetInt32(),
                    Player1Mana = gameState.GetProperty("Player1Mana").GetInt32(),
                    Player2Mana = gameState.GetProperty("Player2Mana").GetInt32(),
                    Player1X = gameState.GetProperty("Player1X").GetInt32(),
                    Player1Y = gameState.GetProperty("Player1Y").GetInt32(),
                    Player2X = gameState.GetProperty("Player2X").GetInt32(),
                    Player2Y = gameState.GetProperty("Player2Y").GetInt32(),
                    Player1Action = gameState.TryGetProperty("Player1Action", out var p1Action)
                        ? p1Action.GetString()
                        : null,
                    Player2Action = gameState.TryGetProperty("Player2Action", out var p2Action)
                        ? p2Action.GetString()
                        : null,
                    FromPlayer = data.TryGetProperty("fromPlayer", out var from)
                        ? from.GetString()
                        : null
                };

                OnGameUpdate?.Invoke(updateData);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"HandleGameUpdate error: {ex.Message}");
            }
        }

        private void HandlePlayerJoined(JsonElement root)
        {
            try
            {
                if (!root.TryGetProperty("Data", out var data))
                    return;

                var joinedData = new PlayerJoinedData
                {
                    Username = data.GetProperty("username").GetString(),
                    RoomCode = data.GetProperty("roomCode").GetString(),
                    Player1 = data.GetProperty("player1").GetString(),
                    Player2 = data.GetProperty("player2").GetString()
                };

                OnPlayerJoined?.Invoke(joinedData);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"HandlePlayerJoined error: {ex.Message}");
            }
        }

        private void HandlePlayerLeft(JsonElement root)
        {
            try
            {
                if (!root.TryGetProperty("Data", out var data))
                    return;

                var leftData = new PlayerLeftData
                {
                    Username = data.GetProperty("username").GetString()
                };

                OnPlayerLeft?.Invoke(leftData);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"HandlePlayerLeft error: {ex.Message}");
            }
        }

        private void HandleStartGame(JsonElement root)
        {
            try
            {
                if (!root.TryGetProperty("Data", out var data))
                    return;

                var startData = new StartGameData
                {
                    RoomCode = data.GetProperty("roomCode").GetString(),
                    Player1 = data.GetProperty("player1").GetString(),
                    Player2 = data.GetProperty("player2").GetString()
                };

                OnStartGame?.Invoke(startData);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"HandleStartGame error: {ex.Message}");
            }
        }

        // ===========================
        // GỬI DỮ LIỆU TỚI SERVER
        // ===========================
        public async Task<bool> SendActionAsync(string json)
        {
            try
            {
                if (!IsConnected)
                    return false;

                byte[] data = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(data, 0, data.Length);
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Send error: {ex.Message}");
                return false;
            }
        }

        // ===========================
        // CLEANUP
        // ===========================
        public void Dispose()
        {
            try
            {
                cts?.Cancel();
                listenTask?.Wait(1000);
                stream?.Close();
                client?.Close();
            }
            catch { }
        }
    }

    // ===========================
    // DATA CLASSES
    // ===========================
    public class GameUpdateData
    {
        public int Player1Health { get; set; }
        public int Player2Health { get; set; }
        public int Player1Stamina { get; set; }
        public int Player2Stamina { get; set; }
        public int Player1Mana { get; set; }
        public int Player2Mana { get; set; }
        public int Player1X { get; set; }
        public int Player1Y { get; set; }
        public int Player2X { get; set; }
        public int Player2Y { get; set; }
        public string Player1Action { get; set; }
        public string Player2Action { get; set; }
        public string FromPlayer { get; set; }
    }

    public class PlayerJoinedData
    {
        public string Username { get; set; }
        public string RoomCode { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
    }

    public class PlayerLeftData
    {
        public string Username { get; set; }
    }

    public class StartGameData
    {
        public string RoomCode { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
    }
}