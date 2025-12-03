using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DoAn_NT106.Services
{
    /// <summary>
    /// Client để kết nối Global Chat - lắng nghe tin nhắn broadcast
    /// </summary>
    public class GlobalChatClient : IDisposable
    {
        private TcpClient client;
        private NetworkStream stream;
        private CancellationTokenSource cts;
        private Task listenTask;
        private readonly string serverAddress;
        private readonly int serverPort;
        private string username;
        private string token;
        private bool isJoined = false;

        // Events
        public event Action<ChatMessageData> OnChatMessage;
        public event Action<int> OnOnlineCountUpdate;
        public event Action<List<ChatMessageData>> OnHistoryReceived;
        public event Action<string> OnError;
        public event Action OnConnected;
        public event Action OnDisconnected;

        public bool IsConnected => client?.Connected ?? false;
        public bool IsJoined => isJoined;

        public GlobalChatClient(string address = "127.0.0.1", int port = 8080)
        {
            serverAddress = address;
            serverPort = port;
        }

        // ===========================
        // KẾT NỐI VÀ JOIN GLOBAL CHAT
        // ===========================

        public async Task<(bool Success, int OnlineCount, List<ChatMessageData> History)> ConnectAndJoinAsync(string username, string token)
        {
            try
            {
                this.username = username;
                this.token = token;

                // Kết nối TCP
                client = new TcpClient();
                await client.ConnectAsync(serverAddress, serverPort);
                stream = client.GetStream();

                // Gửi request join Global Chat
                var request = new
                {
                    Action = "GLOBAL_CHAT_JOIN",
                    Data = new Dictionary<string, object>
            {
                { "username", username },
                { "token", token }
            }
                };

                string requestJson = JsonSerializer.Serialize(request);
                byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);
                await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                // Đọc response
                byte[] buffer = new byte[65536]; // Tăng buffer
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // ✅ THÊM: Log response để debug
                Console.WriteLine($"[GlobalChatClient] Response: {responseJson.Substring(0, Math.Min(500, responseJson.Length))}");

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                bool success = root.GetProperty("Success").GetBoolean();

                if (success)
                {
                    isJoined = true;
                    int onlineCount = 0;
                    var history = new List<ChatMessageData>();

                    if (root.TryGetProperty("Data", out var data))
                    {
                        // ✅ FIX: Parse onlineCount an toàn
                        if (data.TryGetProperty("onlineCount", out var countEl))
                        {
                            onlineCount = countEl.GetInt32();
                            Console.WriteLine($"[GlobalChatClient] Parsed onlineCount: {onlineCount}");
                        }
                        else
                        {
                            Console.WriteLine($"[GlobalChatClient] WARNING: 'onlineCount' not found in response");
                        }

                        if (data.TryGetProperty("history", out var historyEl))
                        {
                            foreach (var item in historyEl.EnumerateArray())
                            {
                                history.Add(new ChatMessageData
                                {
                                    Id = GetStringProperty(item, "id"),
                                    Username = GetStringProperty(item, "username"),
                                    Message = GetStringProperty(item, "message"),
                                    Timestamp = GetStringProperty(item, "timestamp"),
                                    Type = GetStringProperty(item, "type")
                                });
                            }
                            Console.WriteLine($"[GlobalChatClient] Loaded {history.Count} history messages");
                        }
                    }

                    // Bắt đầu lắng nghe broadcast
                    cts = new CancellationTokenSource();
                    listenTask = Task.Run(() => ListenForBroadcasts(cts.Token));

                    OnConnected?.Invoke();

                    Console.WriteLine($"[GlobalChatClient] Returning onlineCount: {onlineCount}");
                    return (true, onlineCount, history);
                }

                string message = root.GetProperty("Message").GetString();
                OnError?.Invoke(message);
                return (false, 0, null);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Connection error: {ex.Message}");
                Console.WriteLine($"[GlobalChatClient] Exception: {ex.Message}");
                return (false, 0, null);
            }
        }

        // ✅ THÊM: Helper method an toàn
        private string GetStringProperty(JsonElement el, string name)
        {
            return el.TryGetProperty(name, out var prop) ? prop.GetString() ?? "" : "";
        }

        // ===========================
        // LẮNG NGHE BROADCAST
        // ===========================

        private async Task ListenForBroadcasts(CancellationToken token)
        {
            byte[] buffer = new byte[16384];

            try
            {
                while (!token.IsCancellationRequested && client?.Connected == true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);

                    if (bytesRead == 0)
                        break;

                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Có thể nhận nhiều JSON messages cùng lúc
                    ProcessBroadcast(json);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                    OnError?.Invoke($"Listen error: {ex.Message}");
            }
            finally
            {
                isJoined = false;
                OnDisconnected?.Invoke();
            }
        }

        private void ProcessBroadcast(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("Action", out var actionEl))
                    return;

                string action = actionEl.GetString();

                switch (action)
                {
                    case "GLOBAL_CHAT_MESSAGE":
                        HandleChatMessage(root);
                        break;

                    case "GLOBAL_CHAT_ONLINE_UPDATE":
                        HandleOnlineUpdate(root);
                        break;
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Process error: {ex.Message}");
            }
        }

        private void HandleChatMessage(JsonElement root)
        {
            try
            {
                if (!root.TryGetProperty("Data", out var data))
                    return;

                var chatMessage = new ChatMessageData
                {
                    Id = data.GetProperty("id").GetString(),
                    Username = data.GetProperty("username").GetString(),
                    Message = data.GetProperty("message").GetString(),
                    Timestamp = data.GetProperty("timestamp").GetString(),
                    Type = data.GetProperty("type").GetString()
                };

                // Cập nhật online count nếu có
                if (data.TryGetProperty("onlineCount", out var countEl))
                {
                    OnOnlineCountUpdate?.Invoke(countEl.GetInt32());
                }

                OnChatMessage?.Invoke(chatMessage);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"HandleChatMessage error: {ex.Message}");
            }
        }

        private void HandleOnlineUpdate(JsonElement root)
        {
            try
            {
                if (!root.TryGetProperty("Data", out var data))
                    return;

                if (data.TryGetProperty("onlineCount", out var countEl))
                {
                    OnOnlineCountUpdate?.Invoke(countEl.GetInt32());
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"HandleOnlineUpdate error: {ex.Message}");
            }
        }

        // ===========================
        // GỬI TIN NHẮN
        // ===========================

        public async Task<bool> SendMessageAsync(string message)
        {
            try
            {
                if (!IsConnected || !isJoined)
                {
                    OnError?.Invoke("Not connected to Global Chat");
                    return false;
                }

                // Giới hạn 1000 ký tự
                if (message.Length > 1000)
                    message = message.Substring(0, 1000);

                var request = new
                {
                    Action = "GLOBAL_CHAT_SEND",
                    Data = new Dictionary<string, object>
                    {
                        { "username", username },
                        { "message", message },
                        { "token", token }
                    }
                };

                string json = JsonSerializer.Serialize(request);
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
        // LEAVE & CLEANUP
        // ===========================

        public async Task LeaveAsync()
        {
            try
            {
                if (IsConnected && isJoined)
                {
                    var request = new
                    {
                        Action = "GLOBAL_CHAT_LEAVE",
                        Data = new Dictionary<string, object>
                        {
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

                if (isJoined)
                {
                    // Fire and forget leave
                    _ = LeaveAsync();
                }

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
    // DATA MODEL
    // ===========================
    public class ChatMessageData
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public string Timestamp { get; set; }
        public string Type { get; set; } // "user" hoặc "system"
    }
}