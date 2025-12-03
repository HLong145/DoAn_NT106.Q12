using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DoAn_NT106.Services
{
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

        public async Task<(bool Success, int OnlineCount, List<ChatMessageData> History)> ConnectAndJoinAsync(string username, string token)
        {
            try
            {
                this.username = username;
                this.token = token;

                client = new TcpClient();
                await client.ConnectAsync(serverAddress, serverPort);
                stream = client.GetStream();

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

                byte[] buffer = new byte[65536];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // ✅ Log response để debug
                Console.WriteLine($"[GlobalChatClient] Raw response: {responseJson}");

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                bool success = root.GetProperty("Success").GetBoolean();
                Console.WriteLine($"[GlobalChatClient] Success: {success}");

                if (success)
                {
                    isJoined = true;
                    int onlineCount = 0;
                    var history = new List<ChatMessageData>();

                    if (root.TryGetProperty("Data", out var data))
                    {
                        Console.WriteLine($"[GlobalChatClient] Data found");

                        if (data.TryGetProperty("onlineCount", out var countEl))
                        {
                            onlineCount = countEl.GetInt32();
                            Console.WriteLine($"[GlobalChatClient] ✅ Parsed onlineCount: {onlineCount}");
                        }
                        else
                        {
                            Console.WriteLine($"[GlobalChatClient] ⚠️ 'onlineCount' NOT found in Data");
                        }

                        if (data.TryGetProperty("history", out var historyEl))
                        {
                            foreach (var item in historyEl.EnumerateArray())
                            {
                                history.Add(new ChatMessageData
                                {
                                    Id = GetStringProp(item, "id"),
                                    Username = GetStringProp(item, "username"),
                                    Message = GetStringProp(item, "message"),
                                    Timestamp = GetStringProp(item, "timestamp"),
                                    Type = GetStringProp(item, "type")
                                });
                            }
                            Console.WriteLine($"[GlobalChatClient] Loaded {history.Count} history messages");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[GlobalChatClient] ⚠️ 'Data' NOT found in response");
                    }

                    // Bắt đầu listen broadcasts
                    cts = new CancellationTokenSource();
                    listenTask = Task.Run(() => ListenForBroadcasts(cts.Token));

                    OnConnected?.Invoke();

                    Console.WriteLine($"[GlobalChatClient] Returning: Success=true, OnlineCount={onlineCount}");
                    return (true, onlineCount, history);
                }

                string message = root.TryGetProperty("Message", out var msgEl) ? msgEl.GetString() : "Unknown error";
                OnError?.Invoke(message);
                return (false, 0, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GlobalChatClient] Exception: {ex.Message}");
                OnError?.Invoke($"Connection error: {ex.Message}");
                return (false, 0, null);
            }
        }

        private string GetStringProp(JsonElement el, string name)
        {
            return el.TryGetProperty(name, out var prop) ? prop.GetString() ?? "" : "";
        }

        private async Task ListenForBroadcasts(CancellationToken token)
        {
            byte[] buffer = new byte[65536];

            try
            {
                while (!token.IsCancellationRequested && client?.Connected == true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("[GlobalChatClient] Server disconnected (0 bytes)");
                        break;
                    }

                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"[GlobalChatClient] Broadcast received: {json.Substring(0, Math.Min(200, json.Length))}...");

                    ProcessBroadcast(json);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[GlobalChatClient] Listen cancelled");
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    Console.WriteLine($"[GlobalChatClient] Listen error: {ex.Message}");
                    OnError?.Invoke($"Listen error: {ex.Message}");
                }
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
                {
                    Console.WriteLine("[GlobalChatClient] No 'Action' in broadcast");
                    return;
                }

                string action = actionEl.GetString();
                Console.WriteLine($"[GlobalChatClient] Processing action: {action}");

                switch (action)
                {
                    case "GLOBAL_CHAT_MESSAGE":
                        HandleChatMessage(root);
                        break;

                    case "GLOBAL_CHAT_ONLINE_UPDATE":
                        HandleOnlineUpdate(root);
                        break;

                    default:
                        Console.WriteLine($"[GlobalChatClient] Unknown action: {action}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GlobalChatClient] Process error: {ex.Message}");
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
                    Id = GetStringProp(data, "id"),
                    Username = GetStringProp(data, "username"),
                    Message = GetStringProp(data, "message"),
                    Timestamp = GetStringProp(data, "timestamp"),
                    Type = GetStringProp(data, "type")
                };

                Console.WriteLine($"[GlobalChatClient] Chat message from {chatMessage.Username}: {chatMessage.Message}");

                // Cập nhật online count nếu có
                if (data.TryGetProperty("onlineCount", out var countEl))
                {
                    int count = countEl.GetInt32();
                    Console.WriteLine($"[GlobalChatClient] Online count in chat message: {count}");
                    OnOnlineCountUpdate?.Invoke(count);
                }

                OnChatMessage?.Invoke(chatMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GlobalChatClient] HandleChatMessage error: {ex.Message}");
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
                    int count = countEl.GetInt32();
                    Console.WriteLine($"[GlobalChatClient] ✅ Online update received: {count}");
                    OnOnlineCountUpdate?.Invoke(count);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GlobalChatClient] HandleOnlineUpdate error: {ex.Message}");
                OnError?.Invoke($"HandleOnlineUpdate error: {ex.Message}");
            }
        }

        public async Task<bool> SendMessageAsync(string message)
        {
            try
            {
                if (!IsConnected || !isJoined)
                {
                    OnError?.Invoke("Not connected to Global Chat");
                    return false;
                }

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

                Console.WriteLine($"[GlobalChatClient] Sent message: {message}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GlobalChatClient] Send error: {ex.Message}");
                OnError?.Invoke($"Send error: {ex.Message}");
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

    public class ChatMessageData
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public string Timestamp { get; set; }
        public string Type { get; set; }
    }
}