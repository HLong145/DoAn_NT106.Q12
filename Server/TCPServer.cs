using DoAn_NT106.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DoAn_NT106.Server
{
    public class TcpServer
    {
        private TcpListener listener;
        private bool isRunning;
        private List<ClientHandler> connectedClients = new List<ClientHandler>();
        private DatabaseService dbService;
        private TokenManager tokenManager;
        private ValidationService validationService;
        private SecurityService securityService;

        //Roomanager quản lý tạo phòng
        private RoomManager roomManager;
        //Quản lý chat global ở lobby form
        private GlobalChatManager globalChatManager;

        private Task _acceptTask;
        private CancellationTokenSource cts;

        public event Action<string> OnLog;
        public bool IsRunning => isRunning;

        public TcpServer()
        {
            dbService = new DatabaseService();
            tokenManager = new TokenManager();
            validationService = new ValidationService();
            securityService = new SecurityService();

            // ✅ KHỞI TẠO ROOM MANAGER
            roomManager = new RoomManager();
            roomManager.OnLog += LogMessage;
            globalChatManager = new GlobalChatManager();
            globalChatManager.OnLog += LogMessage;
        }

        // ... (Giữ nguyên Start, Stop, AcceptClients từ code cũ) ...

        public void Start(int port)
        {
            try
            {
                if (isRunning)
                {
                    Log("⚠️ Server is already running");
                    return;
                }

                cts = new CancellationTokenSource();
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                isRunning = true;

                Log($"✅ Server started on port {port}");

                _acceptTask = Task.Run(() => AcceptClients(cts.Token));
            }
            catch (Exception ex)
            {
                Log($"❌ Error starting server: {ex.Message}");
                throw;
            }
        }

        public async Task Stop()
        {
            if (!isRunning)
            {
                Log("⚠️ Server is not running");
                return;
            }

            try
            {
                isRunning = false;
                cts.Cancel();
                listener?.Stop();

                foreach (var client in connectedClients.ToArray())
                {
                    try
                    {
                        client.Close();
                    }
                    catch (Exception ex)
                    {
                        Log($"⚠️ Error closing client: {ex.Message}");
                    }
                }
                connectedClients.Clear();

                if (_acceptTask != null)
                {
                    await _acceptTask;
                }

                Log("🛑 Server stopped safely");
            }
            catch (Exception ex)
            {
                Log($"❌ Error stopping server: {ex.Message}");
            }
            finally
            {
                listener = null;
                cts = null;
                isRunning = false;
            }
        }

        private async Task AcceptClients(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    TcpClient client = null;
                    try
                    {
                        client = await listener.AcceptTcpClientAsync();
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }

                    if (client == null) break;

                    var clientHandler = new ClientHandler(
                        client,
                        this,
                        dbService,
                        tokenManager,
                        validationService,
                        securityService,
                        roomManager,
                        globalChatManager);

                    lock (connectedClients)
                    {
                        connectedClients.Add(clientHandler);
                    }

                    Log($"📱 New client connected. Total: {connectedClients.Count}");

                    _ = Task.Run(() => clientHandler.Handle());
                }
            }
            catch (Exception ex)
            {
                if (isRunning)
                    Log($"❌ Error in AcceptClients: {ex.Message}");
            }
            finally
            {
                Log("🧩 AcceptClients stopped safely");
            }
        }

        public void RemoveClient(ClientHandler client)
        {
            lock (connectedClients)
            {
                connectedClients.Remove(client);
            }
            Log($"📴 Client disconnected. Remaining: {connectedClients.Count}");
        }

        public void Log(string message)
        {
            string logMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
            OnLog?.Invoke(logMessage);
        }

        private void LogMessage(string message)
        {
            Log(message);
        }
    }

    // ===========================
    // CLIENT HANDLER - ĐÃ CẬP NHẬT
    // ===========================
    public class ClientHandler
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private TcpServer server;
        private DatabaseService dbService;
        private TokenManager tokenManager;
        private string currentToken;
        private ValidationService validationService;
        private SecurityService securityService;

        // ✅ THÊM ROOM MANAGER
        private RoomManager roomManager;
        private string currentUsername;
        private string currentRoomCode;

        private GlobalChatManager globalChatManager;
        private string globalChatUsername;

        private bool isNormalLogout = false;

        public ClientHandler(
            TcpClient client,
            TcpServer server,
            DatabaseService dbService,
            TokenManager tokenManager,
            ValidationService validationService,
            SecurityService securityService,
            RoomManager roomManager,
            GlobalChatManager globalChatManager)
        {
            tcpClient = client;
            this.server = server;
            this.dbService = dbService;
            this.tokenManager = tokenManager;
            stream = client.GetStream();
            this.validationService = validationService;
            this.securityService = securityService;
            this.roomManager = roomManager;
            this.globalChatManager = globalChatManager;
        }

        public void SetNormalLogout()
        {
            isNormalLogout = true;
        }

        public async Task Handle()
        {
            try
            {
                byte[] buffer = new byte[8192];

                while (tcpClient.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0) break;

                    string requestJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string safeLog = HideSensitiveData(requestJson);
                    server.Log($"📨 Received: {safeLog.Substring(0, Math.Min(100, safeLog.Length))}...");

                    var response = ProcessRequest(requestJson);

                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);

                    server.Log($"📤 Sent response");
                }
            }
            catch (Exception ex)
            {
                server.Log($"❌ Client handler error: {ex.Message}");
            }
            finally
            {
                if (!string.IsNullOrEmpty(globalChatUsername))
                {
                    globalChatManager.LeaveGlobalChat(globalChatUsername);
                }

                Close();
            }
        }

        // ✅ GỬI MESSAGE TỚI CLIENT (PUBLIC METHOD)
        public void SendMessage(string json)
        {
            try
            {
                if (tcpClient == null || !tcpClient.Connected)
                    return;

                byte[] data = Encoding.UTF8.GetBytes(json);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                server.Log($"❌ SendMessage error: {ex.Message}");
            }
        }

        private string HideSensitiveData(string json)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                using var stream = new MemoryStream();
                using var writer = new Utf8JsonWriter(stream);

                writer.WriteStartObject();

                if (doc.RootElement.TryGetProperty("Action", out JsonElement action))
                {
                    writer.WriteString("Action", action.GetString());
                }

                if (doc.RootElement.TryGetProperty("Data", out JsonElement data))
                {
                    writer.WritePropertyName("Data");
                    writer.WriteStartObject();

                    foreach (var property in data.EnumerateObject())
                    {
                        if (property.Name.ToLower().Contains("password"))
                        {
                            writer.WriteString(property.Name, "***HIDDEN***");
                        }
                        else
                        {
                            property.WriteTo(writer);
                        }
                    }

                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
                writer.Flush();

                return Encoding.UTF8.GetString(stream.ToArray());
            }
            catch
            {
                return Regex.Replace(json, @"""password""\s*:\s*""[^""]*""", @"""password"":""***HIDDEN***""", RegexOptions.IgnoreCase);
            }
        }

        // ===========================
        // ✅ PROCESS REQUEST - ĐÃ THÊM CÁC ACTION PHÒNG
        // ===========================
        private string ProcessRequest(string requestJson)
        {
            try
            {
                var request = JsonSerializer.Deserialize<Request>(requestJson);

                if (request == null)
                {
                    return CreateResponse(false, "Invalid request format");
                }

                switch (request.Action?.ToUpper())
                {
                    // ===== CÁC ACTION CŨ =====
                    case "REGISTER":
                        return HandleRegister(request);
                    case "LOGIN":
                        return HandleLogin(request);
                    case "VERIFY_TOKEN":
                        return HandleVerifyToken(request);
                    case "GENERATE_OTP":
                        return HandleGenerateOTP(request);
                    case "VERIFY_OTP":
                        return HandleVerifyOTP(request);
                    case "RESET_PASSWORD":
                        return HandleResetPassword(request);
                    case "GET_USER_BY_CONTACT":
                        return HandleGetUserByContact(request);
                    case "LOGOUT":
                        return HandleLogout(request);

                    // ===== ✅ CÁC ACTION MỚI - PHÒNG CHƠI =====
                    case "CREATE_ROOM":
                        return HandleCreateRoom(request);
                    case "JOIN_ROOM":
                        return HandleJoinRoom(request);
                    case "GET_ROOMS":
                        return HandleGetRooms(request);
                    case "START_GAME":
                        return HandleStartGame(request);
                    case "GAME_ACTION":
                        return HandleGameAction(request);
                    case "LEAVE_ROOM":
                        return HandleLeaveRoom(request);

                    //Các action global chat
                    case "GLOBAL_CHAT_JOIN":
                        return HandleGlobalChatJoin(request);

                    case "GLOBAL_CHAT_LEAVE":
                        return HandleGlobalChatLeave(request);

                    case "GLOBAL_CHAT_SEND":
                        return HandleGlobalChatSend(request);

                    case "GLOBAL_CHAT_GET_HISTORY":
                        return HandleGlobalChatGetHistory(request);

                    case "GLOBAL_CHAT_GET_ONLINE":
                        return HandleGlobalChatGetOnline(request);
                    default:
                        return CreateResponse(false, "Unknown action");
                }
            }
            catch (Exception ex)
            {
                server.Log($"❌ Process error: {ex.Message}");
                return CreateResponse(false, $"Server error: {ex.Message}");
            }
        }

        // ===========================
        // ✅ XỬ LÝ TẠO PHÒNG
        // ===========================
        private string HandleCreateRoom(Request request)
        {
            try
            {
                var roomName = request.Data?["roomName"]?.ToString();
                var password = request.Data?.ContainsKey("password") == true
                    ? request.Data["password"]?.ToString()
                    : null;
                var username = request.Data?["username"]?.ToString();

                if (string.IsNullOrEmpty(roomName) || string.IsNullOrEmpty(username))
                {
                    return CreateResponse(false, "Room name and username are required");
                }

                currentUsername = username;

                var result = roomManager.CreateRoom(roomName, password, username, this);

                if (result.Success)
                {
                    currentRoomCode = result.RoomCode;

                    return CreateResponse(true, result.Message, new Dictionary<string, object>
                    {
                        { "roomCode", result.RoomCode },
                        { "roomName", roomName }
                    });
                }

                return CreateResponse(false, result.Message);
            }
            catch (Exception ex)
            {
                return CreateResponse(false, $"Create room error: {ex.Message}");
            }
        }

        // ===========================
        // ✅ XỬ LÝ THAM GIA PHÒNG
        // ===========================
        private string HandleJoinRoom(Request request)
        {
            try
            {
                var roomCode = request.Data?["roomCode"]?.ToString();
                var password = request.Data?.ContainsKey("password") == true
                    ? request.Data["password"]?.ToString()
                    : null;
                var username = request.Data?["username"]?.ToString();

                if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(username))
                {
                    return CreateResponse(false, "Room code and username are required");
                }

                currentUsername = username;

                var result = roomManager.JoinRoom(roomCode, password, username, this);

                if (result.Success)
                {
                    currentRoomCode = roomCode;

                    return CreateResponse(true, result.Message, new Dictionary<string, object>
                    {
                        { "roomCode", roomCode },
                        { "player1", result.Room.Player1Username },
                        { "player2", result.Room.Player2Username }
                    });
                }

                return CreateResponse(false, result.Message);
            }
            catch (Exception ex)
            {
                return CreateResponse(false, $"Join room error: {ex.Message}");
            }
        }

        // ===========================
        // ✅ LẤY DANH SÁCH PHÒNG
        // ===========================

        private string HandleGetRooms(Request request)
        {
            try
            {
                var rooms = roomManager.GetAvailableRooms();

                server.Log($"📋 GetRooms: Found {rooms.Count} available rooms");
                foreach (var room in rooms)
                {
                    server.Log($"   - {room.RoomCode}: {room.RoomName} ({room.PlayerCount}/2)");
                }

                return CreateResponse(true, "Rooms retrieved", new Dictionary<string, object>
        {
            { "rooms", rooms }
        });
            }
            catch (Exception ex)
            {
                server.Log($"❌ GetRooms error: {ex.Message}");
                return CreateResponse(false, $"Get rooms error: {ex.Message}");
            }
        }


        // ===========================
        // ✅ BẮT ĐẦU GAME
        // ===========================
        private string HandleStartGame(Request request)
        {
            try
            {
                var roomCode = request.Data?["roomCode"]?.ToString();

                if (string.IsNullOrEmpty(roomCode))
                {
                    return CreateResponse(false, "Room code is required");
                }

                bool success = roomManager.StartGame(roomCode);

                if (success)
                {
                    return CreateResponse(true, "Game started");
                }

                return CreateResponse(false, "Cannot start game");
            }
            catch (Exception ex)
            {
                return CreateResponse(false, $"Start game error: {ex.Message}");
            }
        }

        // ===========================
        // ✅ XỬ LÝ HÀNH ĐỘNG GAME (VỊ TRÍ/ACTION)
        // ===========================
        private string HandleGameAction(Request request)
        {
            try
            {
                var roomCode = request.Data?["roomCode"]?.ToString();
                var username = request.Data?["username"]?.ToString();
                var actionType = request.Data?["type"]?.ToString();

                if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(username))
                {
                    return CreateResponse(false, "Missing required data");
                }

                var action = new GameAction
                {
                    Type = actionType,
                    X = request.Data.ContainsKey("x") ? Convert.ToInt32(request.Data["x"]) : 0,
                    Y = request.Data.ContainsKey("y") ? Convert.ToInt32(request.Data["y"]) : 0,
                    ActionName = request.Data.ContainsKey("actionName")
                        ? request.Data["actionName"]?.ToString()
                        : null
                };

                // Cập nhật state và broadcast
                roomManager.UpdateGameState(roomCode, username, action);

                return CreateResponse(true, "Action processed");
            }
            catch (Exception ex)
            {
                return CreateResponse(false, $"Game action error: {ex.Message}");
            }
        }

        // ===========================
        // ✅ RỜI PHÒNG
        // ===========================
        private string HandleLeaveRoom(Request request)
        {
            try
            {
                var roomCode = request.Data?["roomCode"]?.ToString();
                var username = request.Data?["username"]?.ToString();

                if (!string.IsNullOrEmpty(roomCode) && !string.IsNullOrEmpty(username))
                {
                    roomManager.LeaveRoom(roomCode, username);
                }

                return CreateResponse(true, "Left room");
            }
            catch (Exception ex)
            {
                return CreateResponse(false, $"Leave room error: {ex.Message}");
            }
        }

        // ... (Giữ nguyên các HandleXXX khác từ code cũ) ...

        private string HandleRegister(Request request)
        {
            try
            {
                var username = request.Data?["username"]?.ToString();
                var email = request.Data.ContainsKey("email") ? request.Data["email"]?.ToString() : null;
                var phone = request.Data.ContainsKey("phone") ? request.Data["phone"]?.ToString() : null;
                var password = request.Data?["password"]?.ToString();

                server.Log($"🔍 Register attempt: Username='{username}', Email='{email}', Phone='{phone}'");

                var validationResult = validationService.ValidateRegistration(username, email, phone, password);
                if (!validationResult.IsValid)
                {
                    server.Log($"❌ Register validation failed: {validationResult.Message}");
                    return CreateResponse(false, validationResult.Message);
                }

                server.Log($"🔍 Checking if user exists: {username}");
                bool userExists = dbService.IsUserExists(username, email, phone);
                server.Log($"🔍 User exists result: {userExists}");

                if (userExists)
                {
                    server.Log($"❌ Register failed: User already exists - {username}");
                    return CreateResponse(false, "Username, email or phone already exists");
                }

                server.Log($"🔍 Creating salt and hash for: {username}");
                string salt = dbService.CreateSalt();
                string hash = dbService.HashPassword_Sha256(password, salt);

                server.Log($"🔍 Saving user to database: {username}");
                bool success = dbService.SaveUserToDatabase(username, email, phone, hash, salt);

                server.Log($"✅ Register result: {success} for user {username}");

                return CreateResponse(success, success ? "Registration successful" : "Registration failed");
            }
            catch (Exception ex)
            {
                server.Log($"❌ Register ERROR: {ex.ToString()}");
                return CreateResponse(false, $"Registration error: {ex.Message}");
            }
        }

        private string HandleLogin(Request request)
        {
            try
            {
                var username = request.Data?["username"]?.ToString();
                var password = request.Data?["password"]?.ToString();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return CreateResponse(false, "Username and password are required");
                }

                if (!securityService.CheckLoginAttempts(username))
                {
                    int remainingMinutes = securityService.GetLockoutMinutes(username);
                    return CreateResponse(false, $"Account locked. Try again in {remainingMinutes} minutes.");
                }

                bool loginSuccess = dbService.VerifyUserLogin(username, password);

                securityService.RecordLoginAttempt(username, loginSuccess);

                if (loginSuccess)
                {
                    string token = tokenManager.GenerateToken(username);
                    currentToken = token;
                    currentUsername = username;

                    return CreateResponse(true, "Login successful", new Dictionary<string, object>
                    {
                          { "token", token },
                           { "username", username }
                      });
                }
                else
                {
                    int remaining = securityService.GetRemainingAttempts(username);
                    return CreateResponse(false, $"Invalid credentials. {remaining} attempts remaining.");
                }
            }
            catch (Exception ex)
            {
                return CreateResponse(false, $"Login error: {ex.Message}");
            }
        }

        private string HandleVerifyToken(Request request)
        {
            try
            {
                var token = request.Data?["token"]?.ToString();

                if (string.IsNullOrEmpty(token))
                {
                    return CreateResponse(false, "Token is required");
                }

                bool isValid = tokenManager.ValidateToken(token);

                if (isValid)
                {
                    var username = tokenManager.GetUsernameFromToken(token);
                    return CreateResponse(true, "Token valid", new Dictionary<string, object>
                    {
                        { "username", username }
                    });
                }
                else
                {
                    return CreateResponse(false, "Invalid or expired token");
                }
            }
            catch (Exception ex)
            {
                return CreateResponse(false, $"Token verification error: {ex.Message}");
            }
        }

        private string HandleGenerateOTP(Request request)
        {
            try
            {
                var username = request.Data?["username"]?.ToString();

                if (string.IsNullOrEmpty(username))
                {
                    return CreateResponse(false, "Username is required");
                }

                string otp = dbService.GenerateOtp(username);

                return CreateResponse(true, "OTP generated", new Dictionary<string, object>
                {
                    { "otp", otp }
                });
            }
            catch (Exception ex)
            {
                return CreateResponse(false, $"OTP generation error: {ex.Message}");
            }
        }

        private string HandleVerifyOTP(Request request)
        {
            try
            {
                var username = request.Data?["username"]?.ToString();
                var otp = request.Data?["otp"]?.ToString();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(otp))
                {
                    return CreateResponse(false, "Username and OTP are required");
                }

                var result = dbService.VerifyOtp(username, otp);

                return CreateResponse(result.IsValid, result.Message);
            }
            catch (Exception ex)
            {
                return CreateResponse(false, $"OTP verification error: {ex.Message}");
            }
        }

        private string HandleResetPassword(Request request)
        {
            try
            {
                var username = request.Data?["username"]?.ToString();
                var newPassword = request.Data?["newPassword"]?.ToString();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(newPassword))
                {
                    return CreateResponse(false, "Username and new password are required");
                }

                bool success = dbService.ResetPassword(username, newPassword);

                return CreateResponse(success, success ? "Password reset successful" : "Password reset failed");
            }
            catch (Exception ex)
            {
                return CreateResponse(false, $"Password reset error: {ex.Message}");
            }
        }

        private string HandleGetUserByContact(Request request)
        {
            try
            {
                var contact = request.Data?["contact"]?.ToString();
                var isEmail = request.Data.ContainsKey("isEmail") &&
                             bool.Parse(request.Data["isEmail"].ToString());

                if (string.IsNullOrEmpty(contact))
                {
                    return CreateResponse(false, "Contact is required");
                }

                string username = dbService.GetUsernameByContact(contact, isEmail);

                if (!string.IsNullOrEmpty(username))
                {
                    return CreateResponse(true, "User found", new Dictionary<string, object>
                    {
                        { "username", username }
                    });
                }
                else
                {
                    return CreateResponse(false, "User not found");
                }
            }
            catch (Exception ex)
            {
                return CreateResponse(false, $"Get user error: {ex.Message}");
            }
        }

        private string HandleLogout(Request request)
        {
            try
            {
                var token = request.Data?["token"]?.ToString();
                var logoutType = request.Data?["logoutType"]?.ToString();

                if (string.IsNullOrEmpty(token))
                {
                    return CreateResponse(false, "Token is required for logout");
                }

                if (logoutType == "normal")
                {
                    SetNormalLogout();
                    return CreateResponse(true, "Logout successful (token preserved)");
                }
                else
                {
                    tokenManager.RevokeToken(token);
                    return CreateResponse(true, "Logout successful (token revoked)");
                }
            }
            catch (Exception ex)
            {
                return CreateResponse(false, $"Logout error: {ex.Message}");
            }
        }

        private string CreateResponse(bool success, string message, Dictionary<string, object> data = null)
        {
            var response = new Response
            {
                Success = success,
                Message = message,
                Data = data ?? new Dictionary<string, object>()
            };

            return JsonSerializer.Serialize(response);
        }

        private string HandleGlobalChatJoin(Request request)
        {
            try
            {
                var username = request.Data?["username"]?.ToString();
                var token = request.Data?["token"]?.ToString();

                if (string.IsNullOrEmpty(username))
                {
                    return CreateResponse(false, "Username is required");
                }

                // Lưu username để cleanup khi disconnect
                globalChatUsername = username;

                var result = globalChatManager.JoinGlobalChat(username, this);

                if (result.Success)
                {
                    // Lấy chat history để gửi cho user mới
                    var history = globalChatManager.GetChatHistory(30);
                    var historyData = history.Select(h => new
                    {
                        id = h.Id,
                        username = h.Username,
                        message = h.Message,
                        timestamp = h.Timestamp.ToString("HH:mm:ss"),
                        type = h.Type
                    }).ToList();

                    return CreateResponseWithData(true, result.Message, new Dictionary<string, object>
            {
                { "onlineCount", result.OnlineCount },
                { "history", historyData }
            });
                }

                return CreateResponse(false, result.Message);
            }
            catch (Exception ex)
            {
                server.Log($"❌ HandleGlobalChatJoin error: {ex.Message}");
                return CreateResponse(false, $"Error: {ex.Message}");
            }
        }

        private string HandleGlobalChatLeave(Request request)
        {
            try
            {
                var username = request.Data?["username"]?.ToString();

                if (string.IsNullOrEmpty(username))
                {
                    return CreateResponse(false, "Username is required");
                }

                var result = globalChatManager.LeaveGlobalChat(username);
                globalChatUsername = null;

                return CreateResponseWithData(true, "Left Global Chat", new Dictionary<string, object>
        {
            { "onlineCount", result.OnlineCount }
        });
            }
            catch (Exception ex)
            {
                server.Log($"❌ HandleGlobalChatLeave error: {ex.Message}");
                return CreateResponse(false, $"Error: {ex.Message}");
            }
        }

        private string HandleGlobalChatSend(Request request)
        {
            try
            {
                var username = request.Data?["username"]?.ToString();
                var message = request.Data?["message"]?.ToString();
                var token = request.Data?["token"]?.ToString();


                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(message))
                {
                    return CreateResponse(false, "Username and message are required");
                }

                var result = globalChatManager.SendChatMessage(username, message);

                return CreateResponse(result.Success, result.Message);
            }
            catch (Exception ex)
            {
                server.Log($"❌ HandleGlobalChatSend error: {ex.Message}");
                return CreateResponse(false, $"Error: {ex.Message}");
            }
        }

        private string HandleGlobalChatGetHistory(Request request)
        {
            try
            {
                int count = 30;
                if (request.Data?.ContainsKey("count") == true)
                {
                    count = Convert.ToInt32(request.Data["count"]);
                }

                var history = globalChatManager.GetChatHistory(count);
                var historyData = history.Select(h => new
                {
                    id = h.Id,
                    username = h.Username,
                    message = h.Message,
                    timestamp = h.Timestamp.ToString("HH:mm:ss"),
                    type = h.Type
                }).ToList();

                return CreateResponseWithData(true, "OK", new Dictionary<string, object>
        {
            { "history", historyData }
        });
            }
            catch (Exception ex)
            {
                return CreateResponse(false, $"Error: {ex.Message}");
            }
        }

        private string HandleGlobalChatGetOnline(Request request)
        {
            try
            {
                var onlineCount = globalChatManager.GetOnlineCount();
                var onlineUsers = globalChatManager.GetOnlineUsers();

                return CreateResponseWithData(true, "OK", new Dictionary<string, object>
        {
            { "onlineCount", onlineCount },
            { "onlineUsers", onlineUsers }
        });
            }
            catch (Exception ex)
            {
                return CreateResponse(false, $"Error: {ex.Message}");
            }
        }

        // ✅ BƯỚC 8: Helper method để tạo response với data
        // ------------------------------------------------------------
        private string CreateResponseWithData(bool success, string message, Dictionary<string, object> data)
        {
            var response = new
            {
                Success = success,
                Message = message,
                Data = data
            };
            return JsonSerializer.Serialize(response);
        }

        public void Close()
        {
            try
            {
                // ✅ Rời phòng nếu đang trong phòng
                if (!string.IsNullOrEmpty(currentRoomCode) && !string.IsNullOrEmpty(currentUsername))
                {
                    roomManager.LeaveRoom(currentRoomCode, currentUsername);
                }

                if (!string.IsNullOrEmpty(currentToken))
                {
                    tokenManager.RevokeToken(currentToken);
                }

                stream?.Close();
                tcpClient?.Close();
                server.RemoveClient(this);
            }
            catch { }
        }
    }

    public class Request
    {
        public string Action { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }

    public class Response
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }
}