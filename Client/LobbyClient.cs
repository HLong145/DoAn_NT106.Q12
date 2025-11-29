using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DoAn_NT106.Client
{
    /// <summary>
    /// Client để kết nối real-time với Lobby của room
    /// Tương tự GlobalChatClient
    /// </summary>
    public class LobbyClient : IDisposable
    {
        private TcpClient client;
        private NetworkStream stream;
        private CancellationTokenSource cts;
        private Task listenTask;

        private readonly string serverAddress;
        private readonly int serverPort;

        private string roomCode;
        private string username;
        private string token;
        private bool isJoined = false;

        // Events để UI subscribe
        public event Action OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string> OnError;

        // Lobby events
        public event Action<LobbyPlayerData> OnPlayerJoined;
        public event Action<string> OnPlayerLeft;
        public event Action<string, bool> OnPlayerReadyChanged; // username, isReady
        public event Action<LobbyChatMessage> OnChatMessage;
        public event Action OnAllPlayersReady; // Khi cả 2 ready
        public event Action<LobbyStateData> OnLobbyStateUpdate;

        public bool IsConnected => client?.Connected ?? false;
        public bool IsJoined => isJoined;

        public LobbyClient(string address = "127.0.0.1", int port = 8080)
        {
            serverAddress = address;
            serverPort = port;
        }

        // ===========================
        // KẾT NỐI VÀ JOIN LOBBY
        // ===========================
        public async Task<(bool Success, LobbyStateData State)> ConnectAndJoinAsync(
            string roomCode, string username, string token)
        {
            try
            {
                this.roomCode = roomCode;
                this.username = username;
                this.token = token;

                // Kết nối TCP
                client = new TcpClient();
                await client.ConnectAsync(serverAddress, serverPort);
                stream = client.GetStream();

                // Gửi request join lobby
                var request = new
                {
                    Action = "LOBBY_JOIN",
                    Data = new Dictionary<string, object>
                    {
                        { "roomCode", roomCode },
                        { "username", username },
                        { "token", token }
                    }
                };

                string requestJson = JsonSerializer.Serialize(request);
                byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);
                await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                // Đọc response
                byte[] buffer = new byte[16384];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                bool success = root.GetProperty("Success").GetBoolean();

                if (success)
                {
                    isJoined = true;
                    LobbyStateData state = null;

                    if (root.TryGetProperty("Data", out var data))
                    {
                        state = new LobbyStateData
                        {
                            RoomCode = roomCode,
                            RoomName = data.TryGetProperty("roomName", out var rn) ? rn.GetString() : "",
                            Player1 = data.TryGetProperty("player1", out var p1) ? p1.GetString() : null,
                            Player2 = data.TryGetProperty("player2", out var p2) ? p2.GetString() : null,
                            Player1Ready = data.TryGetProperty("player1Ready", out var p1r) && p1r.GetBoolean(),
                            Player2Ready = data.TryGetProperty("player2Ready", out var p2r) && p2r.GetBoolean()
                        };

                        // Parse chat history nếu có
                        if (data.TryGetProperty("chatHistory", out var historyEl))
                        {
                            state.ChatHistory = new List<LobbyChatMessage>();
                            foreach (var item in historyEl.EnumerateArray())
                            {
                                state.ChatHistory.Add(new LobbyChatMessage
                                {
                                    Username = item.GetProperty("username").GetString(),
                                    Message = item.GetProperty("message").GetString(),
                                    Timestamp = item.GetProperty("timestamp").GetString(),
                                    Type = item.TryGetProperty("type", out var t) ? t.GetString() : "user"
                                });
                            }
                        }
                    }

                    // Bắt đầu lắng nghe broadcasts
                    cts = new CancellationTokenSource();
                    listenTask = Task.Run(() => ListenForBroadcasts(cts.Token));

                    OnConnected?.Invoke();
                    return (true, state);
                }

                string message = root.GetProperty("Message").GetString();
                OnError?.Invoke(message);
                return (false, null);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Connection error: {ex.Message}");
                return (false, null);
            }
        }

        // ===========================
        // LẮNG NGHE BROADCASTS
        // ===========================
        private async Task ListenForBroadcasts(CancellationToken token)
        {
            byte[] buffer = new byte[16384];

            try
            {
                while (!token.IsCancellationRequested && client?.Connected == true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);

                    if (bytesRead == 0) break;

                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessBroadcast(json);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                OnError?.Invoke($"Listen error: {ex.Message}");
            }
            finally
            {
                OnDisconnected?.Invoke("Connection closed");
            }
        }

        private void ProcessBroadcast(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("Action", out var actionElement))
                    return;

                string action = actionElement.GetString();
                var data = root.TryGetProperty("Data", out var d) ? d : default;

                switch (action)
                {
                    case "LOBBY_PLAYER_JOINED":
                        HandlePlayerJoined(data);
                        break;

                    case "LOBBY_PLAYER_LEFT":
                        HandlePlayerLeft(data);
                        break;

                    case "LOBBY_READY_CHANGED":
                        HandleReadyChanged(data);
                        break;

                    case "LOBBY_CHAT":
                        HandleChatMessage(data);
                        break;

                    case "LOBBY_ALL_READY":
                        OnAllPlayersReady?.Invoke();
                        break;

                    case "LOBBY_STATE_UPDATE":
                        HandleStateUpdate(data);
                        break;
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Process broadcast error: {ex.Message}");
            }
        }

        private void HandlePlayerJoined(JsonElement data)
        {
            var playerData = new LobbyPlayerData
            {
                Username = data.GetProperty("username").GetString(),
                IsPlayer1 = data.TryGetProperty("isPlayer1", out var ip1) && ip1.GetBoolean()
            };
            OnPlayerJoined?.Invoke(playerData);
        }

        private void HandlePlayerLeft(JsonElement data)
        {
            string username = data.GetProperty("username").GetString();
            OnPlayerLeft?.Invoke(username);
        }

        private void HandleReadyChanged(JsonElement data)
        {
            string username = data.GetProperty("username").GetString();
            bool isReady = data.GetProperty("isReady").GetBoolean();
            OnPlayerReadyChanged?.Invoke(username, isReady);
        }

        private void HandleChatMessage(JsonElement data)
        {
            var msg = new LobbyChatMessage
            {
                Username = data.GetProperty("username").GetString(),
                Message = data.GetProperty("message").GetString(),
                Timestamp = data.GetProperty("timestamp").GetString(),
                Type = data.TryGetProperty("type", out var t) ? t.GetString() : "user"
            };
            OnChatMessage?.Invoke(msg);
        }

        private void HandleStateUpdate(JsonElement data)
        {
            var state = new LobbyStateData
            {
                Player1 = data.TryGetProperty("player1", out var p1) ? p1.GetString() : null,
                Player2 = data.TryGetProperty("player2", out var p2) ? p2.GetString() : null,
                Player1Ready = data.TryGetProperty("player1Ready", out var p1r) && p1r.GetBoolean(),
                Player2Ready = data.TryGetProperty("player2Ready", out var p2r) && p2r.GetBoolean()
            };
            OnLobbyStateUpdate?.Invoke(state);
        }

        // ===========================
        // GỬI ACTIONS
        // ===========================
        public async Task<bool> SetReadyAsync(bool isReady)
        {
            try
            {
                if (!IsConnected || !isJoined) return false;

                var request = new
                {
                    Action = "LOBBY_SET_READY",
                    Data = new Dictionary<string, object>
                    {
                        { "roomCode", roomCode },
                        { "username", username },
                        { "isReady", isReady }
                    }
                };

                string json = JsonSerializer.Serialize(request);
                byte[] data = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(data, 0, data.Length);
                return true;
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

                var request = new
                {
                    Action = "LOBBY_CHAT_SEND",
                    Data = new Dictionary<string, object>
                    {
                        { "roomCode", roomCode },
                        { "username", username },
                        { "message", message }
                    }
                };

                string json = JsonSerializer.Serialize(request);
                byte[] data = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(data, 0, data.Length);
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"SendChat error: {ex.Message}");
                return false;
            }
        }

        public async Task LeaveAsync()
        {
            try
            {
                if (IsConnected && isJoined)
                {
                    var request = new
                    {
                        Action = "LOBBY_LEAVE",
                        Data = new Dictionary<string, object>
                        {
                            { "roomCode", roomCode },
                            { "username", username }
                        }
                    };

                    string json = JsonSerializer.Serialize(request);
                    byte[] data = Encoding.UTF8.GetBytes(json);
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
            catch { }
            finally
            {
                isJoined = false;
            }
        }

        public void Dispose()
        {
            try
            {
                cts?.Cancel();
                if (isJoined) _ = LeaveAsync();
                stream?.Close();
                client?.Close();
            }
            catch { }
            finally
            {
                cts?.Dispose();
                stream?.Dispose();
                client?.Dispose();
            }
        }
    }

    // ===========================
    // DATA CLASSES
    // ===========================
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
}