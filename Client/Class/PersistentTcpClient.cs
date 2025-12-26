using DoAn_NT106.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DoAn_NT106.Client.Class
{
    #region Server response model

    public class ServerResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public Dictionary<string, object> Data { get; set; }

        public JsonElement RawData { get; set; }

        // Helper lấy giá trị string theo key trong Data
        public string GetDataValue(string key)
        {
            if (Data != null && Data.ContainsKey(key))
                return Data[key]?.ToString();

            return null;
        }
    }

    #endregion

    #region Persistent TCP client

    /// <summary>
    /// Client TCP persistent - giữ 1 connection cho toàn bộ session
    /// </summary>
    public class PersistentTcpClient : IDisposable
    {
        #region Fields

        private readonly string serverAddress;
        private readonly int serverPort;

        private TcpClient client;
        private NetworkStream stream;
        private CancellationTokenSource cts;
        private Task listenTask;
        private bool isConnected = false;

        private readonly object sendLock = new object();

        // Pending requests: RequestId => TaskCompletionSource
        private ConcurrentDictionary<string, TaskCompletionSource<ServerResponse>> pendingRequests
            = new ConcurrentDictionary<string, TaskCompletionSource<ServerResponse>>();

        #endregion

        #region Events

        // Event broadcast từ server
        public event Action<string, JsonElement> OnBroadcast;

        public event Action OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string> OnError;

        public bool IsConnected => isConnected && client?.Connected == true;

        #endregion

        #region Singleton

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

        public static void ResetInstance()
        {
            lock (_lock)
            {
                if (_instance != null)
                {
                    try
                    {
                        // Disconnect và dispose instance cũ
                        _instance.Disconnect();
                        Console.WriteLine("[PersistentTcpClient] 🔄 Old instance disconnected");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[PersistentTcpClient] ⚠️ Disconnect error: {ex.Message}");
                    }

                    _instance = null;
                    Console.WriteLine("[PersistentTcpClient] Instance reset - next access will create new connection");
                }
            }
        }

        #endregion

        #region Constructor

        public PersistentTcpClient(string address = null, int port = 0)
        {
            // Use AppConfig as default
            serverAddress = string.IsNullOrEmpty(address) ? AppConfig.SERVER_IP : address;
            serverPort = port <= 0 ? AppConfig.TCP_PORT : port;
        }

        #endregion

        #region Connect

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

                // Cấu hình TCP Keep-Alive để phát hiện mất kết nối nhanh hơn
                ConfigureKeepAlive(client.Client);

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

        /// <summary>
        /// Cấu hình TCP Keep-Alive để phát hiện mất kết nối nhanh
        /// </summary>
        private void ConfigureKeepAlive(Socket socket)
        {
            try
            {
                // Bật Keep-Alive
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                // Trên Windows, cấu hình chi tiết Keep-Alive timing
                // Keep-Alive time: 5 giây (thời gian idle trước khi gửi probe đầu tiên)
                // Keep-Alive interval: 1 giây (khoảng cách giữa các probe)
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    byte[] keepAliveValues = new byte[12];
                    BitConverter.GetBytes((uint)1).CopyTo(keepAliveValues, 0);      
                    BitConverter.GetBytes((uint)5000).CopyTo(keepAliveValues, 4);   
                    BitConverter.GetBytes((uint)1000).CopyTo(keepAliveValues, 8);   

                    socket.IOControl(IOControlCode.KeepAliveValues, keepAliveValues, null);
                    Console.WriteLine("[TCP] ✅ TCP Keep-Alive configured (5s idle, 1s interval)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP] ⚠️ Keep-Alive config failed: {ex.Message}");
            }
        }
        #endregion

        #region Send request

        public async Task<ServerResponse> SendRequestAsync(string action, Dictionary<string, object> data = null, int timeoutMs = 10000)
        {
            // Đảm bảo đã kết nối trước khi gửi
            if (!IsConnected && !await ConnectAsync())
                return new ServerResponse { Success = false, Message = "Cannot connect" };

            string requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var tcs = new TaskCompletionSource<ServerResponse>();

            if (!pendingRequests.TryAdd(requestId, tcs))
                return new ServerResponse { Success = false, Message = "Request creation failed" };

            try
            {
                var request = new
                {
                    Action = action,
                    RequestId = requestId,
                    Data = data ?? new Dictionary<string, object>()
                };

                string json = JsonSerializer.Serialize(request);
                string encrypted = EncryptionService.Encrypt(json);
                // IMPORTANT: append newline so server can split messages reliably
                byte[] bytes = Encoding.UTF8.GetBytes(encrypted + "\n");

                Console.WriteLine($"[TCP] Sending: {action} (ID: {requestId})");

                // Lock để tránh ghi chồng dữ liệu trên stream
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

        #endregion

        #region Send broadcast

        /// <summary>
        ///  Send broadcast to server
        /// </summary>
        public void SendBroadcast(string action, string jsonData)
        {
            try
            {
                if (!IsConnected)
                {
                    Console.WriteLine($"[TCP] Not connected, cannot send broadcast: {action}");
                    return;
                }

                var broadcast = new
                {
                    Action = action,
                    Data = jsonData,
                    Timestamp = DateTime.UtcNow
                };

                string json = JsonSerializer.Serialize(broadcast);
                string encrypted = EncryptionService.Encrypt(json);
                byte[] bytes = Encoding.UTF8.GetBytes(encrypted + "\n");

                lock (sendLock)
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }

                Console.WriteLine($"[TCP] Broadcast sent: {action}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP] SendBroadcast error: {ex.Message}");
            }
        }

        #endregion

        #region Listen loop

        private async Task ListenLoop(CancellationToken token)
        {
            byte[] buffer = new byte[65536];
            StringBuilder msgBuffer = new StringBuilder();

            try
            {
                // Set ReceiveTimeout trên socket để ReadAsync không block mãi mãi
                if (client?.Client != null)
                {
                    client.Client.ReceiveTimeout = 10000; 
                }

                while (!token.IsCancellationRequested && client?.Connected == true)
                {
                    int bytesRead;

                    try
                    {
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    }
                    catch (IOException ex) when (ex.InnerException is SocketException)
                    {
                        // Socket error = mất kết nối thực sự
                        Console.WriteLine($"[TCP] ❌ Socket error: {ex.InnerException.Message}");
                        break;
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"[TCP] ❌ Socket exception: {ex.Message}");
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("[TCP] Server closed connection (bytesRead = 0)");
                        break;
                    }

                    msgBuffer.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                    ProcessMessages(msgBuffer);
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                Console.WriteLine("[TCP] Listen cancelled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP] ❌ Listen error: {ex.Message}");
                if (!token.IsCancellationRequested)
                    OnError?.Invoke($"Listen error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("[TCP] 🔴 ListenLoop ended, firing OnDisconnected...");
                isConnected = false;

                foreach (var kvp in pendingRequests)
                    kvp.Value.TrySetResult(new ServerResponse { Success = false, Message = "Disconnected" });

                pendingRequests.Clear();
                OnDisconnected?.Invoke("Connection closed");
            }
        }
        // Cắt chuỗi buffer theo từng dòng (mỗi message 1 dòng)
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

        // Xử lý 1 message (sau khi decrypt)
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

            // Response thuộc về 1 request đang chờ (có RequestId)
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
                }

                return;
            }

            // Broadcast  chỉ có Action
            if (root.TryGetProperty("Action", out var actionEl))
            {
                string action = actionEl.GetString();
                JsonElement data = root.TryGetProperty("Data", out var d) ? d.Clone() : default;
                Console.WriteLine($"[TCP] Broadcast: {action}");
                OnBroadcast?.Invoke(action, data);
            }
        }

        #endregion

        #region Disconnect & dispose

        public void Disconnect()
        {
            try
            {
                cts?.Cancel();
                stream?.Close();
                client?.Close();
            }
            catch
            {
            }
            finally
            {
                isConnected = false;
                client = null;
                stream = null;
            }
        }

        public void Dispose() => Disconnect();

        #endregion

        #region API methods - auth

        public Task<ServerResponse> LoginAsync(string username, string password)
            => SendRequestAsync("LOGIN", new Dictionary<string, object>
            {
                { "username", username },
                { "password", password }
            });

        public Task<ServerResponse> RegisterAsync(string username, string email, string phone, string password)
            => SendRequestAsync("REGISTER", new Dictionary<string, object>
            {
                { "username", username },
                { "email", email ?? "" },
                { "phone", phone ?? "" },
                { "password", password }
            });

        public Task<ServerResponse> LogoutAsync(string token, string logoutType = "normal")
            => SendRequestAsync("LOGOUT", new Dictionary<string, object>
            {
                { "token", token },
                { "logoutType", logoutType }
            });

        public Task<ServerResponse> VerifyTokenAsync(string token)
            => SendRequestAsync("VERIFY_TOKEN", new Dictionary<string, object>
            {
                { "token", token }
            });

        public Task<ServerResponse> GenerateOtpAsync(string username)
            => SendRequestAsync("GENERATE_OTP", new Dictionary<string, object>
            {
                { "username", username }
            });

        public Task<ServerResponse> VerifyOtpAsync(string username, string otp)
            => SendRequestAsync("VERIFY_OTP", new Dictionary<string, object>
            {
                { "username", username },
                { "otp", otp }
            });

        public Task<ServerResponse> ResetPasswordAsync(string username, string newPassword)
            => SendRequestAsync("RESET_PASSWORD", new Dictionary<string, object>
            {
                { "username", username },
                { "newPassword", newPassword }
            });

        public Task<ServerResponse> GetUserByContactAsync(string contact, bool isEmail)
            => SendRequestAsync("GET_USER_BY_CONTACT", new Dictionary<string, object>
            {
                { "contact", contact },
                { "isEmail", isEmail }
            });

        #endregion

        #region API methods - room


        public Task<ServerResponse> CreateRoomAsync(string roomName, string password, string username)
            => SendRequestAsync("CREATE_ROOM", new Dictionary<string, object>
            {
                { "roomName", roomName },
                { "password", password ?? "" },
                { "username", username }
            });

        public Task<ServerResponse> JoinRoomAsync(string roomCode, string password, string username)
            => SendRequestAsync("JOIN_ROOM", new Dictionary<string, object>
            {
                { "roomCode", roomCode },
                { "password", password ?? "" },
                { "username", username }
            });

        public Task<ServerResponse> LeaveRoomAsync(string roomCode, string username)
            => SendRequestAsync("LEAVE_ROOM", new Dictionary<string, object>
            {
                { "roomCode", roomCode },
                { "username", username }
            });

        public Task<ServerResponse> GetRoomListAsync()
            => SendRequestAsync("GET_ROOMS");

        #endregion

        #region API methods - lobby

        public Task<ServerResponse> LobbyJoinAsync(string roomCode, string username, string token)
            => SendRequestAsync("LOBBY_JOIN", new Dictionary<string, object>
            {
                { "roomCode", roomCode },
                { "username", username },
                { "token", token }
            });

        public Task<ServerResponse> LobbyLeaveAsync(string roomCode, string username)
            => SendRequestAsync("LOBBY_LEAVE", new Dictionary<string, object>
            {
                { "roomCode", roomCode },
                { "username", username }
            });

        public Task<ServerResponse> LobbySetReadyAsync(string roomCode, string username, bool isReady)
            => SendRequestAsync("LOBBY_SET_READY", new Dictionary<string, object>
            {
                { "roomCode", roomCode },
                { "username", username },
                { "isReady", isReady }
            });

        public Task<ServerResponse> LobbySendChatAsync(string roomCode, string username, string message)
            => SendRequestAsync("LOBBY_CHAT_SEND", new Dictionary<string, object>
            {
                { "roomCode", roomCode },
                { "username", username },
                { "message", message }
            });

        public Task<ServerResponse> LobbyStartGameAsync(string roomCode, string username)
            => SendRequestAsync("LOBBY_START_GAME", new Dictionary<string, object>
            {
                { "roomCode", roomCode },
                { "username", username }
            });

        public Task<ServerResponse> LobbySetMapAsync(string roomCode, string username, string selectedMap)
            => SendRequestAsync("LOBBY_SET_MAP", new Dictionary<string, object>
            {
                { "roomCode", roomCode },
                { "username", username },
                { "selectedMap", selectedMap }
            });

        #endregion

        #region API methods - game

        public Task<ServerResponse> GameEndAsync(string roomCode, string username)
            => SendRequestAsync("GAME_END", new Dictionary<string, object>
            {
                { "roomCode", roomCode },
                { "username", username }
            });
        
        //   Send game damage event
        public Task<ServerResponse> SendGameDamageAsync(string roomCode, string username, int targetPlayerNum, int damage, bool isParried)
            => SendRequestAsync("GAME_DAMAGE", new Dictionary<string, object>
            {
                { "roomCode", roomCode },
                { "username", username },
                { "targetPlayerNum", targetPlayerNum },
                { "damage", damage },
                { "isParried", isParried }
            });

        #endregion

        #region API methods - global chat

        public Task<ServerResponse> GlobalChatJoinAsync(string username, string token)
            => SendRequestAsync("GLOBAL_CHAT_JOIN", new Dictionary<string, object>
            {
                { "username", username },
                { "token", token }
            });

        public Task<ServerResponse> GlobalChatSendAsync(string username, string message, string token)
            => SendRequestAsync("GLOBAL_CHAT_SEND", new Dictionary<string, object>
            {
                { "username", username },
                { "message", message },
                { "token", token }
            });

        public Task<ServerResponse> GlobalChatLeaveAsync(string username)
            => SendRequestAsync("GLOBAL_CHAT_LEAVE", new Dictionary<string, object>
            {
                { "username", username }
            });

        #endregion

        #region Sync wrappers

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

        #endregion
    }

    #endregion
}
