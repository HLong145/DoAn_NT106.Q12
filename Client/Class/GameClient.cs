using System;
using System.Text.Json;

namespace DoAn_NT106.Client.Class
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
        //   Event when server signals game end (winner or draw)
        public event Action<GameEndData> OnGameEnded;
        //   Event khi có damage
        public event Action<DamageEventData> OnDamageEvent;
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
        public async Task<bool> ConnectAsync()
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
                    case "GAME_ENDED":
                    case "GAME_END":
                        HandleGameEnd(data);
                        break;
                    
                    //   Handle damage event (server may use GAME_DAMAGE or DAMAGE_EVENT)
                    case "DAMAGE_EVENT":
                    case "GAME_DAMAGE":
                    case "GAME_DAMAGE_EVENT":
                        HandleDamageEvent(data);
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
                    Player2 = GetStringProp(data, "player2"),
                    Player1Character = GetStringProp(data, "player1Character"),
                    Player2Character = GetStringProp(data, "player2Character")
                };

                OnStartGame?.Invoke(startData);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"HandleStartGame error: {ex.Message}");
            }
        }

        private void HandleGameEnd(JsonElement data)
        {
            try
            {
                var endData = new GameEndData
                {
                    RoomCode = GetStringProp(data, "roomCode"),
                    Winner = GetStringProp(data, "winner"),
                    Reason = GetStringProp(data, "reason")
                };

                OnGameEnded?.Invoke(endData);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"HandleGameEnd error: {ex.Message}");
            }
        }

        //   Handle damage event
        private void HandleDamageEvent(JsonElement data)
        {
            try
            {
                var damageData = new DamageEventData
                {
                    TargetPlayerNum = GetIntProp(data, "targetPlayerNum"),
                    Damage = GetIntProp(data, "damage"),
                    IsParried = GetBoolProp(data, "isParried")
                };

                OnDamageEvent?.Invoke(damageData);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"HandleDamageEvent error: {ex.Message}");
            }
        }

        // ===========================
        // SEND ACTION (cho backward compatible)
        // ===========================
        public async Task<bool> SendActionAsync(string json)
        {
            Console.WriteLine($"[GameClient] SendActionAsync called - consider using TcpClient.SendRequestAsync instead");
            return TcpClient.IsConnected;
        }

        //   Broadcast damage event to server (reliable)
        public async Task BroadcastDamageEvent(string roomCode, string username, int targetPlayerNum, int damage, bool isParried)
        {
            try
            {
                if (!TcpClient.IsConnected)
                {
                    Console.WriteLine("[GameClient] Not connected to server, cannot broadcast damage event");
                    return;
                }

                var data = new Dictionary<string, object>
                {
                    { "roomCode", roomCode },
                    { "username", username },
                    { "targetPlayerNum", targetPlayerNum },
                    { "damage", damage },
                    { "isParried", isParried }
                };

                Console.WriteLine($"[GameClient] Sending GAME_DAMAGE to server: room={roomCode} target={targetPlayerNum} dmg={damage}");
                var resp = await TcpClient.SendRequestAsync("GAME_DAMAGE", data, 3000).ConfigureAwait(false);
                if (resp != null && !resp.Success)
                {
                    Console.WriteLine($"[GameClient] GAME_DAMAGE response failure: {resp.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameClient] BroadcastDamageEvent error: {ex.Message}");
            }
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

        //  NEW: character mapping for each player
        public string Player1Character { get; set; }
        public string Player2Character { get; set; }
    }

    //   Damage event data
    public class DamageEventData
    {
        public int TargetPlayerNum { get; set; }
        public int Damage { get; set; }
        public bool IsParried { get; set; }
    }

    public class GameEndData
    {
        public string RoomCode { get; set; }
        public string Winner { get; set; }
        public string Reason { get; set; }
    }
}