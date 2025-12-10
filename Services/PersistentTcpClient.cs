using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DoAn_NT106.Services
{
    /// <summary>
    /// Response từ server - THAY THẾ class cũ trong TcpClientService.cs
    /// </summary>
    public class ServerResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public JsonElement RawData { get; set; }  // Thêm mới

        public string GetDataValue(string key)
        {
            if (Data != null && Data.ContainsKey(key))
                return Data[key]?.ToString();
            return null;
        }
    }

    /// <summary>
    /// Client TCP persistent - giữ 1 connection cho toàn bộ session
    /// </summary>
    public class PersistentTcpClient : IDisposable
    {
        private readonly string serverAddress;
        private readonly int serverPort;

        private TcpClient client;
        private NetworkStream stream;
        private CancellationTokenSource cts;
        private Task listenTask;

        private bool isConnected = false;
        private readonly object sendLock = new object();

        // Pending requests: RequestId -> TaskCompletionSource
        private ConcurrentDictionary<string, TaskCompletionSource<ServerResponse>> pendingRequests
            = new ConcurrentDictionary<string, TaskCompletionSource<ServerResponse>>();

        // Events
        public event Action<string, JsonElement> OnBroadcast;
        public event Action OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string> OnError;

        public bool IsConnected => isConnected && client?.Connected == true;

        // Singleton
        private static PersistentTcpClient _instance;
        private static readonly object _lock = new object();

        public static PersistentTcpClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new PersistentTcpClient();
                    }
                }
                return _instance;
            }
        }

        public PersistentTcpClient(string address = "127.0.0.1", int port = 8080)
        {
            serverAddress = address;
            serverPort = port;
        }

        // ===========================
        // CONNECT
        // ===========================
        public async Task<bool> ConnectAsync()
        {
            if (isConnected) return true;

            try
            {
                client = new TcpClient { NoDelay = true };

                var connectTask = client.ConnectAsync(serverAddress, serverPort);
                if (await Task.WhenAny(connectTask, Task.Delay(5000)) != connectTask)
                {
                    OnError?.Invoke("Connection timeout");
                    return false;
                }

                stream = client.GetStream();
                isConnected = true;

                cts = new CancellationTokenSource();
                listenTask = Task.Run(() => ListenLoop(cts.Token));

                OnConnected?.Invoke();
                Console.WriteLine($"[TCP] Connected to {serverAddress}:{serverPort}");
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Connect error: {ex.Message}");
                return false;
            }
        }

        // ===========================
        // SEND REQUEST
        // ===========================
        public async Task<ServerResponse> SendRequestAsync(string action, Dictionary<string, object> data = null, int timeoutMs = 10000)
        {
            if (!IsConnected && !await ConnectAsync())
                return new ServerResponse { Success = false, Message = "Cannot connect" };

            string requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var tcs = new TaskCompletionSource<ServerResponse>();

            if (!pendingRequests.TryAdd(requestId, tcs))
                return new ServerResponse { Success = false, Message = "Request creation failed" };

            try
            {
                var request = new { Action = action, RequestId = requestId, Data = data ?? new Dictionary<string, object>() };
                string json = JsonSerializer.Serialize(request);

                string encrypted = EncryptionService.Encrypt(json);
                byte[] bytes = Encoding.UTF8.GetBytes(encrypted);

                Console.WriteLine($"[TCP] Sending: {action} (ID: {requestId})");

                lock (sendLock)
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }

                using var timeoutCts = new CancellationTokenSource(timeoutMs);
                var completed = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs, timeoutCts.Token));

                if (completed == tcs.Task)
                {
                    timeoutCts.Cancel();
                    return await tcs.Task;
                }

                pendingRequests.TryRemove(requestId, out _);
                return new ServerResponse { Success = false, Message = "Timeout" };
            }
            catch (Exception ex)
            {
                pendingRequests.TryRemove(requestId, out _);
                return new ServerResponse { Success = false, Message = ex.Message };
            }
        }

        // ===========================
        // LISTEN LOOP
        // ===========================
        private async Task ListenLoop(CancellationToken token)
        {
            byte[] buffer = new byte[65536];
            StringBuilder msgBuffer = new StringBuilder();

            try
            {
                while (!token.IsCancellationRequested && client?.Connected == true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0) break;

                    msgBuffer.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                    ProcessMessages(msgBuffer);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                    OnError?.Invoke($"Listen error: {ex.Message}");
            }
            finally
            {
                isConnected = false;
                foreach (var kvp in pendingRequests)
                    kvp.Value.TrySetResult(new ServerResponse { Success = false, Message = "Disconnected" });
                pendingRequests.Clear();
                OnDisconnected?.Invoke("Connection closed");
            }
        }

        private void ProcessMessages(StringBuilder buffer)
        {
            string content = buffer.ToString();

            int startIndex = 0;
            int newlineIndex;

            while ((newlineIndex = content.IndexOf('\n', startIndex)) != -1)
            {
                string encryptedMessage = content.Substring(startIndex, newlineIndex - startIndex);
                startIndex = newlineIndex + 1;

                if (!string.IsNullOrWhiteSpace(encryptedMessage))
                {
                    try
                    {
                        ProcessSingleMessage(encryptedMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[TCP] ProcessSingleMessage error: {ex.Message}");
                    }
                }
            }

            buffer.Clear();
            if (startIndex < content.Length)
                buffer.Append(content.Substring(startIndex));
        }
        private void ProcessSingleMessage(string encryptedJson)
        {
            string json;
            try
            {
                json = EncryptionService.Decrypt(encryptedJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP] Decryption failed: {ex.Message}");
                return;
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Response to pending request
            if (root.TryGetProperty("RequestId", out var reqIdEl))
            {
                string requestId = reqIdEl.GetString();
                if (pendingRequests.TryRemove(requestId, out var tcs))
                {
                    var response = new ServerResponse
                    {
                        Success = root.TryGetProperty("Success", out var s) && s.GetBoolean(),
                        Message = root.TryGetProperty("Message", out var m) ? m.GetString() : ""
                    };

                    if (root.TryGetProperty("Data", out var dataEl))
                    {
                        response.Data = JsonSerializer.Deserialize<Dictionary<string, object>>(dataEl.GetRawText());
                        response.RawData = dataEl.Clone();
                    }

                    tcs.TrySetResult(response);
                    return;
                }
            }

            // Broadcast
            if (root.TryGetProperty("Action", out var actionEl))
            {
                string action = actionEl.GetString();
                JsonElement data = root.TryGetProperty("Data", out var d) ? d.Clone() : default;
                Console.WriteLine($"[TCP] Broadcast: {action}");
                OnBroadcast?.Invoke(action, data);
            }
        }

        // ===========================
        // DISCONNECT
        // ===========================
        public void Disconnect()
        {
            try
            {
                cts?.Cancel();
                stream?.Close();
                client?.Close();
            }
            catch { }
            finally
            {
                isConnected = false;
                client = null;
                stream = null;
            }
        }

        public void Dispose() => Disconnect();

        // ===========================
        // API METHODS - AUTH
        // ===========================
        public Task<ServerResponse> LoginAsync(string username, string password)
            => SendRequestAsync("LOGIN", new Dictionary<string, object> { { "username", username }, { "password", password } });

        public Task<ServerResponse> RegisterAsync(string username, string email, string phone, string password)
            => SendRequestAsync("REGISTER", new Dictionary<string, object> { { "username", username }, { "email", email ?? "" }, { "phone", phone ?? "" }, { "password", password } });

        public Task<ServerResponse> LogoutAsync(string token, string logoutType = "normal")
            => SendRequestAsync("LOGOUT", new Dictionary<string, object> { { "token", token }, { "logoutType", logoutType } });

        public Task<ServerResponse> VerifyTokenAsync(string token)
            => SendRequestAsync("VERIFY_TOKEN", new Dictionary<string, object> { { "token", token } });

        public Task<ServerResponse> GenerateOtpAsync(string username)
            => SendRequestAsync("GENERATE_OTP", new Dictionary<string, object> { { "username", username } });

        public Task<ServerResponse> VerifyOtpAsync(string username, string otp)
            => SendRequestAsync("VERIFY_OTP", new Dictionary<string, object> { { "username", username }, { "otp", otp } });

        public Task<ServerResponse> ResetPasswordAsync(string username, string newPassword)
            => SendRequestAsync("RESET_PASSWORD", new Dictionary<string, object> { { "username", username }, { "newPassword", newPassword } });

        public Task<ServerResponse> GetUserByContactAsync(string contact, bool isEmail)
            => SendRequestAsync("GET_USER_BY_CONTACT", new Dictionary<string, object> { { "contact", contact }, { "isEmail", isEmail } });


        // ===========================
        // API METHODS - ROOM
        // ===========================
        public Task<ServerResponse> CreateRoomAsync(string roomName, string password, string username)
            => SendRequestAsync("CREATE_ROOM", new Dictionary<string, object> { { "roomName", roomName }, { "password", password ?? "" }, { "username", username } });

        public Task<ServerResponse> JoinRoomAsync(string roomCode, string password, string username)
            => SendRequestAsync("JOIN_ROOM", new Dictionary<string, object> { { "roomCode", roomCode }, { "password", password ?? "" }, { "username", username } });

        public Task<ServerResponse> LeaveRoomAsync(string roomCode, string username)
            => SendRequestAsync("LEAVE_ROOM", new Dictionary<string, object> { { "roomCode", roomCode }, { "username", username } });

        public Task<ServerResponse> GetRoomListAsync()
            => SendRequestAsync("GET_ROOMS");

        // ===========================
        // API METHODS - LOBBY
        // ===========================
        public Task<ServerResponse> LobbyJoinAsync(string roomCode, string username, string token)
            => SendRequestAsync("LOBBY_JOIN", new Dictionary<string, object> { { "roomCode", roomCode }, { "username", username }, { "token", token } });

        public Task<ServerResponse> LobbyLeaveAsync(string roomCode, string username)
            => SendRequestAsync("LOBBY_LEAVE", new Dictionary<string, object> { { "roomCode", roomCode }, { "username", username } });

        public Task<ServerResponse> LobbySetReadyAsync(string roomCode, string username, bool isReady)
            => SendRequestAsync("LOBBY_SET_READY", new Dictionary<string, object> { { "roomCode", roomCode }, { "username", username }, { "isReady", isReady } });

        public Task<ServerResponse> LobbySendChatAsync(string roomCode, string username, string message)
            => SendRequestAsync("LOBBY_CHAT_SEND", new Dictionary<string, object> { { "roomCode", roomCode }, { "username", username }, { "message", message } });
        public Task<ServerResponse> LobbyStartGameAsync(string roomCode, string username)
    => SendRequestAsync("LOBBY_START_GAME", new Dictionary<string, object> { { "roomCode", roomCode }, { "username", username } });

        // ===========================
        // API METHODS - GLOBAL CHAT
        // ===========================
        public Task<ServerResponse> GlobalChatJoinAsync(string username, string token)
            => SendRequestAsync("GLOBAL_CHAT_JOIN", new Dictionary<string, object> { { "username", username }, { "token", token } });

        public Task<ServerResponse> GlobalChatSendAsync(string username, string message, string token)
            => SendRequestAsync("GLOBAL_CHAT_SEND", new Dictionary<string, object> { { "username", username }, { "message", message }, { "token", token } });

        public Task<ServerResponse> GlobalChatLeaveAsync(string username)
            => SendRequestAsync("GLOBAL_CHAT_LEAVE", new Dictionary<string, object> { { "username", username } });

        // ===========================
        // SYNC WRAPPERS (backward compatibility)
        // ===========================
        public ServerResponse Login(string username, string password)
            => LoginAsync(username, password).GetAwaiter().GetResult();

        public ServerResponse Register(string username, string email, string phone, string password)
            => RegisterAsync(username, email, phone, password).GetAwaiter().GetResult();

        public ServerResponse Logout(string token, string logoutType = "normal")
            => LogoutAsync(token, logoutType).GetAwaiter().GetResult();

        public ServerResponse VerifyToken(string token)
            => VerifyTokenAsync(token).GetAwaiter().GetResult();

        public ServerResponse CreateRoom(string roomName, string password, string username)
            => CreateRoomAsync(roomName, password, username).GetAwaiter().GetResult();

        public ServerResponse JoinRoom(string roomCode, string password, string username)
            => JoinRoomAsync(roomCode, password, username).GetAwaiter().GetResult();

        public ServerResponse LeaveRoom(string roomCode, string username)
            => LeaveRoomAsync(roomCode, username).GetAwaiter().GetResult();

        public ServerResponse GetRoomList()
            => GetRoomListAsync().GetAwaiter().GetResult();
    }
}