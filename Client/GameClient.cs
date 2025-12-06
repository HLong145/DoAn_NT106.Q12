using System;
using System.Text.Json;

namespace DoAn_NT106.Services
{
    /// <summary>
    /// Game Client - Sử dụng PersistentTcpClient singleton
    /// KHÔNG tạo connection riêng
    /// </summary>
    public class GameClient : IDisposable
    {
        // Sử dụng PersistentTcpClient singleton
        private PersistentTcpClient TcpClient => PersistentTcpClient.Instance;

        private bool isDisposed = false;

        // Events để UI có thể subscribe
        public event Action<GameUpdateData> OnGameUpdate;
        public event Action<PlayerJoinedData> OnPlayerJoined;
        public event Action<PlayerLeftData> OnPlayerLeft;
        public event Action<StartGameData> OnStartGame;
        public event Action<string> OnError;

        public bool IsConnected => TcpClient.IsConnected;

        // ===========================
        // CONSTRUCTOR
        // ===========================
        public GameClient()
        {
            // Subscribe vào broadcast của PersistentTcpClient
            TcpClient.OnBroadcast += HandleBroadcast;
        }

        public GameClient(string address, int port) : this()
        {
            // Ignore address/port - dùng PersistentTcpClient singleton
        }

        // ===========================
        // CONNECT (cho backward compatible)
        // ===========================
        public async System.Threading.Tasks.Task<bool> ConnectAsync()
        {
            // Đảm bảo PersistentTcpClient đã connect
            if (!TcpClient.IsConnected)
            {
                return await TcpClient.ConnectAsync();
            }
            return true;
        }

        // ===========================
        // HANDLE BROADCASTS
        // ===========================
        private void HandleBroadcast(string action, JsonElement data)
        {
            if (isDisposed) return;

            try
            {
                switch (action)
                {
                    case "GAME_UPDATE":
                        HandleGameUpdate(data);
                        break;

                    case "PLAYER_JOINED":
                        HandlePlayerJoined(data);
                        break;

                    case "PLAYER_LEFT":
                        HandlePlayerLeft(data);
                        break;

                    case "START_GAME":
                        HandleStartGame(data);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameClient] HandleBroadcast error: {ex.Message}");
                OnError?.Invoke($"Process broadcast error: {ex.Message}");
            }
        }

        // ===========================
        // HANDLE SPECIFIC BROADCASTS
        // ===========================
        private void HandleGameUpdate(JsonElement data)
        {
            try
            {
                if (data.TryGetProperty("gameState", out var gameState))
                {
                    var updateData = new GameUpdateData
                    {
                        Player1Health = GetIntProp(gameState, "Player1Health"),
                        Player2Health = GetIntProp(gameState, "Player2Health"),
                        Player1Stamina = GetIntProp(gameState, "Player1Stamina"),
                        Player2Stamina = GetIntProp(gameState, "Player2Stamina"),
                        Player1Mana = GetIntProp(gameState, "Player1Mana"),
                        Player2Mana = GetIntProp(gameState, "Player2Mana"),
                        Player1X = GetIntProp(gameState, "Player1X"),
                        Player1Y = GetIntProp(gameState, "Player1Y"),
                        Player2X = GetIntProp(gameState, "Player2X"),
                        Player2Y = GetIntProp(gameState, "Player2Y"),
                        Player1Action = GetStringProp(gameState, "Player1Action"),
                        Player2Action = GetStringProp(gameState, "Player2Action")
                    };

                    OnGameUpdate?.Invoke(updateData);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"HandleGameUpdate error: {ex.Message}");
            }
        }

        private void HandlePlayerJoined(JsonElement data)
        {
            try
            {
                var joinData = new PlayerJoinedData
                {
                    Username = GetStringProp(data, "username"),
                    RoomCode = GetStringProp(data, "roomCode"),
                    IsPlayer1 = GetBoolProp(data, "isPlayer1")
                };

                OnPlayerJoined?.Invoke(joinData);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"HandlePlayerJoined error: {ex.Message}");
            }
        }

        private void HandlePlayerLeft(JsonElement data)
        {
            try
            {
                var leftData = new PlayerLeftData
                {
                    Username = GetStringProp(data, "username"),
                    RoomCode = GetStringProp(data, "roomCode")
                };

                OnPlayerLeft?.Invoke(leftData);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"HandlePlayerLeft error: {ex.Message}");
            }
        }

        private void HandleStartGame(JsonElement data)
        {
            try
            {
                var startData = new StartGameData
                {
                    RoomCode = GetStringProp(data, "roomCode"),
                    Player1 = GetStringProp(data, "player1"),
                    Player2 = GetStringProp(data, "player2")
                };

                OnStartGame?.Invoke(startData);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"HandleStartGame error: {ex.Message}");
            }
        }

        // ===========================
        // SEND ACTION (cho backward compatible)
        // ===========================
        public async System.Threading.Tasks.Task<bool> SendActionAsync(string json)
        {
            Console.WriteLine($"[GameClient] SendActionAsync called - consider using TcpClient.SendRequestAsync instead");
            return TcpClient.IsConnected;
        }

        // ===========================
        // DISPOSE
        // ===========================
        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            try
            {
                // Unsubscribe từ events
                TcpClient.OnBroadcast -= HandleBroadcast;
            }
            catch { }

        }

        // ===========================
        // HELPER METHODS
        // ===========================
        private string GetStringProp(JsonElement el, string name)
        {
            return el.TryGetProperty(name, out var prop) ? prop.GetString() ?? "" : "";
        }

        private int GetIntProp(JsonElement el, string name)
        {
            return el.TryGetProperty(name, out var prop) ? prop.GetInt32() : 0;
        }

        private bool GetBoolProp(JsonElement el, string name)
        {
            return el.TryGetProperty(name, out var prop) && prop.GetBoolean();
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
    }

    public class PlayerJoinedData
    {
        public string Username { get; set; }
        public string RoomCode { get; set; }
        public bool IsPlayer1 { get; set; }
    }

    public class PlayerLeftData
    {
        public string Username { get; set; }
        public string RoomCode { get; set; }
    }

    public class StartGameData
    {
        public string RoomCode { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
    }
}