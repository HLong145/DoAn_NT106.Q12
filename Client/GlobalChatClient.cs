using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace DoAn_NT106.Services
{
    #region Global chat client

    /// <summary>
    /// Global Chat Client - Sử dụng PersistentTcpClient singleton
    /// </summary>
    public class GlobalChatClient : IDisposable
    {
        #region Fields and properties

        private PersistentTcpClient TcpClient => PersistentTcpClient.Instance;

        private string username;
        private string token;
        private bool isJoined = false;
        private bool isDisposed = false;

        // Events
        public event Action<ChatMessageData> OnChatMessage;

        // Events when server reports the number of online users
        public event Action<int> OnOnlineCountUpdate;

        // Events when server returns chat history upon successful join
        public event Action<List<ChatMessageData>> OnHistoryReceived;

        public event Action<string> OnError;
        public event Action OnConnected;
        public event Action OnDisconnected;

        // Properties
        public bool IsConnected => TcpClient.IsConnected;
        public bool IsJoined => isJoined;

        #endregion

        #region Constructors

        public GlobalChatClient()
        {
            // Subscribe vào broadcast của PersistentTcpClient
            TcpClient.OnBroadcast += HandleBroadcast;
            TcpClient.OnDisconnected += HandleDisconnected;
        }

        public GlobalChatClient(string address, int port) : this()
        {
        }

        #endregion

        #region Connect and join

        public async Task<(bool Success, int OnlineCount, List<ChatMessageData> History)> ConnectAndJoinAsync(string username, string token)
        {
            try
            {
                this.username = username;
                this.token = token;

                // Đảm bảo đã connect
                if (!TcpClient.IsConnected)
                {
                    bool connected = await TcpClient.ConnectAsync();
                    if (!connected)
                    {
                        OnError?.Invoke("Cannot connect to server");
                        return (false, 0, null);
                    }
                }

                // Gửi request qua PersistentTcpClient
                var response = await TcpClient.GlobalChatJoinAsync(username, token);
                Console.WriteLine($"[GlobalChatClient] Join response: {response.Success} - {response.Message}");

                if (!response.Success)
                {
                    OnError?.Invoke(response.Message);
                    return (false, 0, null);
                }

                isJoined = true;

                // Parse response data
                int onlineCount = 0;
                var history = new List<ChatMessageData>();

                if (response.RawData.ValueKind != JsonValueKind.Undefined)
                {
                    if (response.RawData.TryGetProperty("onlineCount", out var countEl))
                    {
                        onlineCount = countEl.GetInt32();
                        Console.WriteLine($"[GlobalChatClient] ✅ Parsed onlineCount: {onlineCount}");
                    }

                    if (response.RawData.TryGetProperty("history", out var historyEl))
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

                OnHistoryReceived?.Invoke(history);
                OnConnected?.Invoke();

                return (true, onlineCount, history);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GlobalChatClient] Exception: {ex.Message}");
                OnError?.Invoke($"Connection error: {ex.Message}");
                return (false, 0, null);
            }
        }

        #endregion

        #region Handle broadcasts
        private void HandleBroadcast(string action, JsonElement data)
        {
            if (!isJoined || isDisposed) return;

            try
            {
                switch (action)
                {
                    case "GLOBAL_CHAT_MESSAGE":
                        ProcessChatMessage(data);
                        break;

                    case "GLOBAL_CHAT_ONLINE_UPDATE":
                        ProcessOnlineUpdate(data);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GlobalChatClient] HandleBroadcast error: {ex.Message}");
            }
        }

        private void ProcessChatMessage(JsonElement data)
        {
            var msg = new ChatMessageData
            {
                Id = GetStringProp(data, "id"),
                Username = GetStringProp(data, "username"),
                Message = GetStringProp(data, "message"),
                Timestamp = GetStringProp(data, "timestamp"),
                Type = GetStringProp(data, "type")
            };

            Console.WriteLine($"[GlobalChatClient] Chat message from {msg.Username}: {msg.Message}");

            // Cập nhật online count nếu có trong message
            if (data.TryGetProperty("onlineCount", out var countEl))
            {
                OnOnlineCountUpdate?.Invoke(countEl.GetInt32());
            }

            OnChatMessage?.Invoke(msg);
        }

        private void ProcessOnlineUpdate(JsonElement data)
        {
            if (data.TryGetProperty("onlineCount", out var countEl))
            {
                int count = countEl.GetInt32();
                Console.WriteLine($"[GlobalChatClient] ✅ Online update: {count}");
                OnOnlineCountUpdate?.Invoke(count);
            }
        }

        private void HandleDisconnected(string reason)
        {
            if (isJoined && !isDisposed)
            {
                isJoined = false;
                OnDisconnected?.Invoke();
            }
        }

        #endregion

        #region Send message

        public async Task<bool> SendMessageAsync(string message)
        {
            try
            {
                if (!IsConnected || !isJoined)
                {
                    OnError?.Invoke("Not connected to Global Chat");
                    return false;
                }

                if (string.IsNullOrEmpty(message)) return false;

                // Giới hạn độ dài message để tránh spam quá dài
                if (message.Length > 1000)
                    message = message.Substring(0, 1000);

                var response = await TcpClient.GlobalChatSendAsync(username, message, token);
                Console.WriteLine($"[GlobalChatClient] Send result: {response.Success}");
                return response.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GlobalChatClient] Send error: {ex.Message}");
                OnError?.Invoke($"Send error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Leave and dispose

        public async Task LeaveAsync()
        {
            try
            {
                if (IsConnected && isJoined)
                {
                    await TcpClient.GlobalChatLeaveAsync(username);
                    Console.WriteLine($"[GlobalChatClient] Left global chat");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GlobalChatClient] Leave error: {ex.Message}");
            }
            finally
            {
                isJoined = false;
            }
        }


        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            try
            {
                // Unsubscribe từ events
                TcpClient.OnBroadcast -= HandleBroadcast;
                TcpClient.OnDisconnected -= HandleDisconnected;

                // Leave chat nếu đang joined
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

        #region Helpers
        private string GetStringProp(JsonElement el, string name)
        {
            return el.TryGetProperty(name, out var prop) ? prop.GetString() ?? "" : "";
        }

        #endregion

        }

        #region Data class

    public class ChatMessageData
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public string Timestamp { get; set; }
        public string Type { get; set; }
        #endregion
    }

    #endregion
}
