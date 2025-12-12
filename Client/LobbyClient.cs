using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DoAn_NT106.Services;

namespace DoAn_NT106.Client
{
    /// <summary>
    /// Lobby Client - Sử dụng PersistentTcpClient singleton
    /// </summary>
    public class LobbyClient : IDisposable
    {
        #region Fields and properties

        // Sử dụng PersistentTcpClient singleton
        private PersistentTcpClient TcpClient => PersistentTcpClient.Instance;

        private string roomCode;
        private string username;
        private string token;
        private bool isJoined = false;
        private bool isDisposed = false;

        // Events để UI subscribe
        public event Action OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string> OnError;

        // Lobby events
        public event Action<LobbyPlayerData> OnPlayerJoined;
        public event Action<string> OnPlayerLeft;
        public event Action<string, bool> OnPlayerReadyChanged;
        public event Action<LobbyChatMessage> OnChatMessage;
        public event Action OnAllPlayersReady;
        // udpPort, myPlayerNumber, serverIp, opponent
        public event Action<int, int, string, string> OnStartGame;
        public event Action<LobbyStateData> OnLobbyStateUpdate;

        public bool IsConnected => TcpClient.IsConnected;
        public bool IsJoined => isJoined;

        #endregion

        #region Constructors

    
        public LobbyClient()
        {
            // Subscribe vào broadcast của PersistentTcpClient
            TcpClient.OnBroadcast += HandleBroadcast;
            TcpClient.OnDisconnected += HandleDisconnected;
        }

        public LobbyClient(string address, int port) : this()
        {
        }

        // ✅ THÊM: Initialize method để set roomCode, username, token
        public void Initialize(string roomCode, string username, string token)
        {
            this.roomCode = roomCode;
            this.username = username;
            this.token = token;
            isJoined = true;
            Console.WriteLine($"[LobbyClient] Initialized: roomCode={roomCode}, username={username}");
        }

        #endregion

        #region Connect and join lobby
        public async Task<(bool Success, LobbyStateData State)> ConnectAndJoinAsync(
            string roomCode, string username, string token)
        {
            try
            {
                this.roomCode = roomCode;
                this.username = username;
                this.token = token;

                // Đảm bảo đã connect
                if (!TcpClient.IsConnected)
                {
                    bool connected = await TcpClient.ConnectAsync();
                    if (!connected)
                    {
                        OnError?.Invoke("Cannot connect to server");
                        return (false, null);
                    }
                }

                // Gửi request join lobby
                var response = await TcpClient.LobbyJoinAsync(roomCode, username, token);
                Console.WriteLine($"[LobbyClient] Join response: {response.Success} - {response.Message}");

                if (!response.Success)
                {
                    OnError?.Invoke(response.Message);
                    return (false, null);
                }

                isJoined = true;
                LobbyStateData state = null;

                // Parse response data
                if (response.RawData.ValueKind != JsonValueKind.Undefined)
                {
                    state = new LobbyStateData
                    {
                        RoomCode = roomCode,
                        RoomName = GetStringProp(response.RawData, "roomName"),
                        Player1 = GetStringProp(response.RawData, "player1"),
                        Player2 = GetStringProp(response.RawData, "player2"),
                        Player1Ready = GetBoolProp(response.RawData, "player1Ready"),
                        Player2Ready = GetBoolProp(response.RawData, "player2Ready"),
                        ChatHistory = new List<LobbyChatMessage>()
                    };

                    // Parse chat history nếu có
                    if (response.RawData.TryGetProperty("chatHistory", out var historyEl))
                    {
                        foreach (var item in historyEl.EnumerateArray())
                        {
                            state.ChatHistory.Add(new LobbyChatMessage
                            {
                                Username = GetStringProp(item, "username"),
                                Message = GetStringProp(item, "message"),
                                Timestamp = GetStringProp(item, "timestamp"),
                                Type = GetStringProp(item, "type")
                            });
                        }
                    }
                }

                OnConnected?.Invoke();
                return (true, state);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Connection error: {ex.Message}");
                return (false, null);
            }
        }

        #endregion

        #region Handle broadcasts

        private void HandleBroadcast(string action, JsonElement data)
        {
            if (!isJoined || isDisposed) return;

            // Kiểm tra roomCode nếu có 
            if (data.TryGetProperty("roomCode", out var rcEl))
            {
                string broadcastRoomCode = rcEl.GetString();
                if (broadcastRoomCode != roomCode) return; 
            }

            try
            {
                switch (action)
                {
                    case "LOBBY_STATE_UPDATE":
                        ProcessLobbyStateUpdate(data);
                        break;

                    case "LOBBY_PLAYER_JOINED":
                        ProcessPlayerJoined(data);
                        break;

                    case "LOBBY_PLAYER_LEFT":
                        ProcessPlayerLeft(data);
                        break;

                    case "LOBBY_READY_CHANGED":
                        ProcessReadyChanged(data);
                        break;

                    case "LOBBY_CHAT_MESSAGE":
                        ProcessChatMessage(data);
                        break;

                    case "LOBBY_ALL_READY":
                        OnAllPlayersReady?.Invoke();
                        break;

                    case "LOBBY_START_GAME":
                        OnAllPlayersReady?.Invoke();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LobbyClient] HandleBroadcast error: {ex.Message}");
            }
        }

        private void ProcessLobbyStateUpdate(JsonElement data)
        {
            var state = new LobbyStateData
            {
                RoomCode = GetStringProp(data, "roomCode"),
                RoomName = GetStringProp(data, "roomName"),
                Player1 = GetStringProp(data, "player1"),
                Player2 = GetStringProp(data, "player2"),
                Player1Ready = GetBoolProp(data, "player1Ready"),
                Player2Ready = GetBoolProp(data, "player2Ready")
            };

            Console.WriteLine($"[LobbyClient] State update: P1={state.Player1}, P2={state.Player2}");
            OnLobbyStateUpdate?.Invoke(state);
        }

        private void ProcessPlayerJoined(JsonElement data)
        {
            var player = new LobbyPlayerData
            {
                Username = GetStringProp(data, "username"),
                IsPlayer1 = GetBoolProp(data, "isPlayer1")
            };

            Console.WriteLine($"[LobbyClient] Player joined: {player.Username}");
            OnPlayerJoined?.Invoke(player);
        }

        private void ProcessPlayerLeft(JsonElement data)
        {
            string leftUsername = GetStringProp(data, "username");
            Console.WriteLine($"[LobbyClient] Player left: {leftUsername}");
            OnPlayerLeft?.Invoke(leftUsername);
        }

        private void ProcessReadyChanged(JsonElement data)
        {
            string changedUsername = GetStringProp(data, "username");
            bool isReady = GetBoolProp(data, "isReady");
            Console.WriteLine($"[LobbyClient] Ready changed: {changedUsername} = {isReady}");
            OnPlayerReadyChanged?.Invoke(changedUsername, isReady);
        }

        private void ProcessChatMessage(JsonElement data)
        {
            var msg = new LobbyChatMessage
            {
                Username = GetStringProp(data, "username"),
                Message = GetStringProp(data, "message"),
                Timestamp = GetStringProp(data, "timestamp"),
                Type = GetStringProp(data, "type")
            };

            Console.WriteLine($"[LobbyClient] Chat: {msg.Username}: {msg.Message}");
            OnChatMessage?.Invoke(msg);
        }

        private void HandleDisconnected(string reason)
        {
            if (isJoined && !isDisposed)
            {
                isJoined = false;
                OnDisconnected?.Invoke(reason);
            }
        }

        #endregion

        #region Send actions

        public async Task<bool> SetReadyAsync(bool isReady)
        {
            try
            {
                if (!IsConnected || !isJoined) return false;

                var response = await TcpClient.LobbySetReadyAsync(roomCode, username, isReady);
                Console.WriteLine($"[LobbyClient] SetReady({isReady}): {response.Success}");
                return response.Success;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"SetReady error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendChatAsync(string message)
        {
            try
            {
                if (!IsConnected || !isJoined) return false;
                if (string.IsNullOrEmpty(message)) return false;

                var response = await TcpClient.LobbySendChatAsync(roomCode, username, message);
                Console.WriteLine($"[LobbyClient] SendChat: {response.Success}");
                return response.Success;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"SendChat error: {ex.Message}");
                return false;
            }
        }

        // ✅ THÊM: Start game method
        public async Task<bool> StartGameAsync()
        {
            try
            {
                if (!IsConnected || !isJoined)
                {
                    Console.WriteLine($"[LobbyClient] StartGame failed: not connected or joined");
                    OnError?.Invoke("Not connected or joined to lobby");
                    return false;
                }

                Console.WriteLine($"[LobbyClient] Sending StartGameAsync for room={roomCode}, user={username}");
                var response = await TcpClient.LobbyStartGameAsync(roomCode, username);
                
                Console.WriteLine($"[LobbyClient] StartGame response: Success={response.Success}, Message={response.Message}");

                if (!response.Success)
                {
                    Console.WriteLine($"[LobbyClient] StartGame failed: {response.Message}");
                    OnError?.Invoke($"Start game failed: {response.Message}");
                    return false;
                }

                // Parse response data
                if (response.RawData.ValueKind == JsonValueKind.Undefined)
                {
                    Console.WriteLine($"[LobbyClient] StartGame response has no data");
                    OnError?.Invoke("No UDP data in response");
                    return false;
                }

                // Extract UDP info from response
                int udpPort = GetIntProp(response.RawData, "udpPort");
                string serverIp = GetStringProp(response.RawData, "serverIp");
                string player1 = GetStringProp(response.RawData, "player1");
                string player2 = GetStringProp(response.RawData, "player2");

                // Determine player number and opponent
                int myPlayerNumber = 2; // default
                string opponent = player1;
                
                if (player1 == username)
                {
                    myPlayerNumber = 1;
                    opponent = player2;
                }

                Console.WriteLine($"[LobbyClient] StartGame success: udpPort={udpPort}, playerNum={myPlayerNumber}, serverIp={serverIp}, opponent={opponent}");

                // Fire event for UI to handle
                OnStartGame?.Invoke(udpPort, myPlayerNumber, serverIp, opponent ?? "Opponent");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LobbyClient] StartGame exception: {ex.Message}");
                OnError?.Invoke($"StartGame error: {ex.Message}");
                return false;
            }
        }

        public async Task LeaveAsync()
        {
            try
            {
                if (IsConnected && isJoined)
                {
                    var response = await TcpClient.LobbyLeaveAsync(roomCode, username);
                    Console.WriteLine($"[LobbyClient] Leave: {response.Success}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LobbyClient] Leave error: {ex.Message}");
            }
            finally
            {
                isJoined = false;
            }
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            try
            {
                // Unsubscribe từ events
                TcpClient.OnBroadcast -= HandleBroadcast;
                TcpClient.OnDisconnected -= HandleDisconnected;

                // Leave lobby nếu đang joined
                if (isJoined)
                {
                    _ = LeaveAsync();
                }
            }
            catch
            {
            }
        }

        #endregion

        #region Helper methods

        private string GetStringProp(JsonElement el, string name)
        {
            return el.TryGetProperty(name, out var prop) ? prop.GetString() ?? "" : "";
        }

        private bool GetBoolProp(JsonElement el, string name)
        {
            return el.TryGetProperty(name, out var prop) && prop.GetBoolean();
        }

        // ✅ THÊM: Get int property helper
        private int GetIntProp(JsonElement el, string name)
        {
            return el.TryGetProperty(name, out var prop) && prop.TryGetInt32(out int value) ? value : 0;
        }

        #endregion

        #region Data classes
        public class LobbyStateData
        {
            public string RoomCode { get; set; }
            public string RoomName { get; set; }
            public string Player1 { get; set; }
            public string Player2 { get; set; }
            public bool Player1Ready { get; set; }
            public bool Player2Ready { get; set; }
            public List<LobbyChatMessage> ChatHistory { get; set; }
        }

        public class LobbyPlayerData
        {
            public string Username { get; set; }
            public bool IsPlayer1 { get; set; }
        }

        public class LobbyChatMessage
        {
            public string Username { get; set; }
            public string Message { get; set; }
            public string Timestamp { get; set; }
            public string Type { get; set; }
        }

        #endregion
    }
}
