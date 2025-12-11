using Azure;
using DoAn_NT106.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DoAn_NT106.Server
{
    #region TCPServer
    public class TcpServer
    {

        #region Fields

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
        LobbyManager lobbyManager;
        private RoomListBroadcaster roomListBroadcaster;

        // ✅ THÊM: UDP Game Server
        private UDPGameServer udpGameServer;
        private const int UDP_PORT = 5000;

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

            roomManager = new RoomManager();
            roomManager.OnLog += LogMessage;

            // Khởi tạo RoomListBroadcaster
            roomListBroadcaster = new RoomListBroadcaster(roomManager);
            roomListBroadcaster.OnLog += LogMessage;

            // Link broadcaster vào RoomManager
            roomManager.RoomListBroadcaster = roomListBroadcaster;

            globalChatManager = new GlobalChatManager(dbService);
            globalChatManager.OnLog += LogMessage;

            lobbyManager = new LobbyManager(dbService);
            lobbyManager.OnLog += LogMessage;

            // ✅ THÊM: Khởi tạo UDP Game Server
            udpGameServer = new UDPGameServer(UDP_PORT);
            udpGameServer.OnLog += LogMessage;
        }

        #endregion


        #region Server 
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

                // ✅ THÊM: Start UDP Server
                udpGameServer.Start();

                Log($"✅ Server started on port {port}");
                Log($"✅ UDP Game Server ready on port {UDP_PORT}");

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

                // ✅ THÊM: Stop UDP Server
                udpGameServer?.Stop();

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

        #endregion


        #region Client Management

        private async Task AcceptClients(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    if (client == null) break;

                    var clientHandler = new ClientHandler(
                        client,
                        this,
                        dbService,
                        tokenManager,
                        validationService,
                        securityService,
                        roomManager,
                        globalChatManager,
                        lobbyManager,
                        roomListBroadcaster,
                        udpGameServer);  // ✅ THÊM: Pass UDP server to client handler

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
        #endregion
    }
    #endregion

    #region ClientHandler
    public class ClientHandler
    {
        #region Fields
        private TcpClient tcpClient;
        private NetworkStream stream;
        private TcpServer server;
        private DatabaseService dbService;
        private TokenManager tokenManager;
        private string currentToken;
        private ValidationService validationService;
        private SecurityService securityService;

        //Lobby management
        private LobbyManager lobbyManager;
        private string lobbyRoomCode;
        private string lobbyUsername;

        private string currentRequestId;


        //Room management
        private RoomManager roomManager;
        private string currentUsername;
        private string currentRoomCode;
        private RoomListBroadcaster roomListBroadcaster;
        private string roomListUsername;

        private GlobalChatManager globalChatManager;
        private string globalChatUsername;

        private bool isNormalLogout = false;

        // ✅ THÊM: UDP Game Server reference
        private UDPGameServer udpGameServer;

        #endregion

        #region Constructor
        public ClientHandler(
            TcpClient client,
            TcpServer server,
            DatabaseService dbService,
            TokenManager tokenManager,
            ValidationService validationService,
            SecurityService securityService,
            RoomManager roomManager,
            GlobalChatManager globalChatManager,
            LobbyManager lobbyManager,
            RoomListBroadcaster roomListBroadcaster,
            UDPGameServer udpGameServer)  // ✅ THÊM parameter
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
            this.lobbyManager = lobbyManager;
            this.roomListBroadcaster = roomListBroadcaster;
            this.udpGameServer = udpGameServer;  // ✅ THÊM
        }
        #endregion 
        public void SetNormalLogout()
        {
            isNormalLogout = true;
        }

        #endregion


        #region Handle
        public async Task Handle()
        {
            try
            {
                byte[] buffer = new byte[8192];
                StringBuilder msgBuffer = new StringBuilder();

                while (tcpClient.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0) break;

                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    msgBuffer.Append(receivedData);

                    string bufferContent = msgBuffer.ToString();
                    int newlineIndex;

                    while ((newlineIndex = bufferContent.IndexOf('\n')) != -1)
                    {
                        string encryptedRequest = bufferContent.Substring(0, newlineIndex);
                        bufferContent = bufferContent.Substring(newlineIndex + 1);

                        if (string.IsNullOrWhiteSpace(encryptedRequest))
                            continue;


                        string requestJson;
                        try
                        {
                            requestJson = DoAn_NT106.Services.EncryptionService.Decrypt(encryptedRequest);
                        }
                        catch (Exception ex)
                        {
                            server.Log($"❌ Decryption failed: {ex.Message}");
                            continue;
                        }

                        string safeLog = HideSensitiveData(requestJson);
                        server.Log($"📨 Received: {safeLog.Substring(0, Math.Min(100, safeLog.Length))}...");

                        var response = ProcessRequest(requestJson);

                        // Mã hóa response trước khi gửi
                        string encryptedResponse = DoAn_NT106.Services.EncryptionService.Encrypt(response);

                        byte[] responseBytes = Encoding.UTF8.GetBytes(encryptedResponse);
                        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);

                        server.Log($"📤 Sent response {encryptedRequest}");
                    }

                    // Giữ lại phần chưa đầy đủ
                    msgBuffer.Clear();
                    msgBuffer.Append(bufferContent);
                }
            }
            catch (Exception ex)
            {
                server.Log($"❌ Client handler error: {ex.Message}");
            }
            finally
            {
                // Cleanup code...
                if (!string.IsNullOrEmpty(globalChatUsername))
                {
                    globalChatManager.LeaveGlobalChat(globalChatUsername);
                }

                if (!string.IsNullOrEmpty(lobbyRoomCode) && !string.IsNullOrEmpty(lobbyUsername))
                {
                    lobbyManager?.LeaveLobby(lobbyRoomCode, lobbyUsername);
                }

                if (!string.IsNullOrEmpty(roomListUsername))
                {
                    roomListBroadcaster?.Unsubscribe(roomListUsername);
                    roomListUsername = null;
                }

                Close();
            }
        }
        #endregion

        #region Send Message

        public void SendMessage(string json)
        {
            try
            {
                if (tcpClient == null || !tcpClient.Connected)
                    return;

                string encrypted = DoAn_NT106.Services.EncryptionService.Encrypt(json);

                byte[] data = Encoding.UTF8.GetBytes(encrypted);
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

        #endregion

        #region Handle Request
        private string ProcessRequest(string requestJson)
        {
            try
            {
                var request = JsonSerializer.Deserialize<Request>(requestJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // LƯU REQUEST ID
                currentRequestId = request?.RequestId;

                if (request == null || string.IsNullOrEmpty(request.Action))
                {
                    return CreateResponse(false, "Invalid request");
                }

                switch (request.Action?.ToUpper())
                {
                    //Action liên quan đến user
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

                    // Action liên quan đến room management
                    case "CREATE_ROOM":
                        return HandleCreateRoom(request);
                    case "JOIN_ROOM":
                        return HandleJoinRoom(request);
                    case "GET_ROOMS":
                        return HandleGetRooms(request);

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

                    //Action liên quan đến lobby form
                    case "LOBBY_JOIN":
                        return HandleLobbyJoin(request);

                    case "LOBBY_LEAVE":
                        return HandleLobbyLeave(request);

                    case "LOBBY_SET_READY":
                        return HandleLobbySetReady(request);

                    case "LOBBY_CHAT_SEND":
                        return HandleLobbyChatSend(request);

                    case "LOBBY_START_GAME":
                        return HandleLobbyStartGame(request);

                    // ✅ NEW: character selection in lobby
                    case "SELECT_CHARACTER":
                        return HandleSelectCharacter(request);

                    case "START_GAME":
                        return HandleStartGame(request);
                    case "GAME_ACTION":
                        return HandleGameAction(request);
                    
                    // ✅ THÊM: Handle game end
                    case "GAME_END":
                        return HandleGameEnd(request);

                    //Các case liên quan đến broadcast danh sách phòng
                    case "ROOM_LIST_SUBSCRIBE":
                        return HandleRoomListSubscribe(request);

                    case "ROOM_LIST_UNSUBSCRIBE":
                        return HandleRoomListUnsubscribe(request);


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

        // ... existing handlers ...

        // NEW: route SELECT_CHARACTER to LobbyManager
        private string HandleSelectCharacter(Request request)
        {
            try
            {
                var roomCode = request.Data?["roomCode"]?.ToString();
                var username = request.Data?["username"]?.ToString();
                var character = request.Data?["character"]?.ToString();

                if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(character))
                {
                    return CreateResponse(false, "Room code, username and character are required");
                }

                server.Log($"🎯 SELECT_CHARACTER: {username} -> {character} in room {roomCode}");
                lobbyManager.HandleSelectCharacter(roomCode, username, character);

                // Không cần trả nhiều data, START_GAME sẽ được broadcast riêng
                return CreateResponse(true, "Character selected");
            }
            catch (Exception ex)
            {
                server.Log($"❌ HandleSelectCharacter error: {ex.Message}");
                return CreateResponse(false, $"Error: {ex.Message}");
            }
        }
        #endregion


        #region User Handling

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

                // 1. Generate OTP
                string otp = dbService.GenerateOtp(username);
                if (string.IsNullOrEmpty(otp))
                {
                    return CreateResponse(false, "Unable to generate OTP");
                }

                // 2. Try to get recipient email from database
                string recipientEmail = dbService.GetEmailByUsername(username);
                if (string.IsNullOrEmpty(recipientEmail))
                {
                    // fallback to fixed recipient for debugging
                    recipientEmail = "linhquangcbl@gmail.com";
                }

                // 3. SMTP sender configuration (replace with real sender/app password)
                string senderEmail = "linhquangcbl@gmail.com"; // sender email
                string senderAppPassword = "gpwf gvor avuo pmjp"; // app password (example)

                var emailService = new EmailService(senderEmail, senderAppPassword);

                try
                {
                    emailService.SendOtp(recipientEmail, otp);
                }
                catch (Exception ex)
                {
                    server.Log($"❌ Send OTP email failed: {ex.Message}");
                    return CreateResponse(false, "Failed to send OTP email");
                }

                // 4. Return success (do NOT include OTP in response)
                return CreateResponse(true, "OTP has been sent to your email");
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

        #endregion


        #region Room Management
        //Xử lý tạo phòng
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


        //Xử lý tham gia phòng
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


        //Lấy danh sách phòng
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

        private string HandleLeaveRoom(Request request)
        {
            try
            {
                var roomCode = request.Data?["roomCode"]?.ToString();
                var username = request.Data?["username"]?.ToString();

                server.Log($"📤 HandleLeaveRoom: {username} leaving room {roomCode}");

                if (!string.IsNullOrEmpty(roomCode) && !string.IsNullOrEmpty(username))
                {
                    roomManager.LeaveRoom(roomCode, username);
                    server.Log($"✅ HandleLeaveRoom completed for {username}");
                }

                return CreateResponse(true, "Left room");
            }
            catch (Exception ex)
            {
                server.Log($"❌ HandleLeaveRoom error: {ex.Message}");
                return CreateResponse(false, $"Leave room error: {ex.Message}");
            }
        }
        #endregion


        #region Global Chat

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

                globalChatUsername = username;

                var result = globalChatManager.JoinGlobalChat(username, this);

                if (result.Success)
                {
                    var history = globalChatManager.GetChatHistory(30);
                    var historyData = history.Select(h => new
                    {
                        id = h.Id,
                        username = h.Username,
                        message = h.Message,
                        timestamp = h.Timestamp.ToString("HH:mm:ss"),
                        type = h.Type
                    }).ToList();

                    var responseData = new Dictionary<string, object>
            {
                { "onlineCount", result.OnlineCount },
                { "history", historyData }
            };

                    string response = CreateResponseWithData(true, result.Message, responseData);

                    // ✅ Log response để debug
                    server.Log($"📤 GlobalChatJoin response: onlineCount={result.OnlineCount}, historyCount={historyData.Count}");

                    return response;
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

                // Broadcast online count cho những người còn lại
                globalChatManager.BroadcastOnlineCount();

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
        #endregion


        #region  Lobby Management
        private string HandleLobbyJoin(Request request)
        {
            try
            {
                var roomCode = request.Data?["roomCode"]?.ToString();
                var username = request.Data?["username"]?.ToString();
                var token = request.Data?["token"]?.ToString();

                if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(username))
                {
                    return CreateResponse(false, "Room code and username are required");
                }

                // Save for cleanup on disconnect
                this.lobbyRoomCode = roomCode;
                this.lobbyUsername = username;

                var result = lobbyManager.JoinLobby(roomCode, username, this, roomManager);

                if (result.Success && result.Lobby != null)
                {
                    var lobby = result.Lobby;

                    // Prepare chat history
                    var chatHistory = lobby.ChatHistory.Select(c => new
                    {
                        id = c.Id,
                        username = c.Username,
                        message = c.Message,
                        timestamp = c.Timestamp.ToString("HH:mm:ss")
                    }).ToList();

                    return CreateResponseWithData(true, "Joined lobby", new Dictionary<string, object>
            {
                { "roomCode", roomCode },
                { "roomName", lobby.RoomName ?? "Game Room" },
                { "player1", lobby.Player1Username },
                { "player2", lobby.Player2Username },
                { "player1Ready", lobby.Player1Ready },
                { "player2Ready", lobby.Player2Ready },
                { "chatHistory", chatHistory }
            });
                }

                return CreateResponse(false, result.Message);
            }
            catch (Exception ex)
            {
                server.Log($"❌ HandleLobbyJoin error: {ex.Message}");
                return CreateResponse(false, $"Error: {ex.Message}");
            }
        }

        private string HandleLobbyLeave(Request request)
        {
            try
            {
                var roomCode = request.Data?["roomCode"]?.ToString();
                var username = request.Data?["username"]?.ToString();

                if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(username))
                {
                    return CreateResponse(false, "Room code and username are required");
                }

                server.Log($"📤 HandleLobbyLeave: {username} leaving {roomCode}");

                var result = lobbyManager.LeaveLobby(roomCode, username);

                // Clear saved data
                if (this.lobbyRoomCode == roomCode)
                {
                    this.lobbyRoomCode = null;
                    this.lobbyUsername = null;
                }

                server.Log($"✅ HandleLobbyLeave completed: {result.Success} - {result.Message}");

                return CreateResponse(result.Success, result.Message);
            }
            catch (Exception ex)
            {
                server.Log($"❌ HandleLobbyLeave error: {ex.Message}");
                return CreateResponse(false, $"Error: {ex.Message}");
            }
        }


        private string HandleLobbySetReady(Request request)
        {
            try
            {
                var roomCode = request.Data?["roomCode"]?.ToString();
                var username = request.Data?["username"]?.ToString();

                bool isReady = false;
                if (request.Data?.ContainsKey("isReady") == true)
                {
                    var isReadyObj = request.Data["isReady"];
                    if (isReadyObj is bool b)
                        isReady = b;
                    else if (isReadyObj is JsonElement je)
                        isReady = je.GetBoolean();
                    else
                        isReady = Convert.ToBoolean(isReadyObj);
                }

                if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(username))
                {
                    return CreateResponse(false, "Room code and username are required");
                }

                var result = lobbyManager.SetReady(roomCode, username, isReady);

                return CreateResponseWithData(result.Success, result.Message, new Dictionary<string, object>
        {
            { "isReady", isReady },
            { "bothReady", result.BothReady }
        });
            }
            catch (Exception ex)
            {
                server.Log($"❌ HandleLobbySetReady error: {ex.Message}");
                return CreateResponse(false, $"Error: {ex.Message}");
            }
        }

        private string HandleLobbyChatSend(Request request)
        {
            try
            {
                var roomCode = request.Data?["roomCode"]?.ToString();
                var username = request.Data?["username"]?.ToString();
                var message = request.Data?["message"]?.ToString();

                if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(message))
                {
                    return CreateResponse(false, "Room code, username, and message are required");
                }

                var result = lobbyManager.SendChatMessage(roomCode, username, message);

                return CreateResponse(result.Success, result.Message);
            }
            catch (Exception ex)
            {
                server.Log($"❌ HandleLobbyChatSend error: {ex.Message}");
                return CreateResponse(false, $"Error: {ex.Message}");
            }
        }
        
        private string HandleLobbyStartGame(Request request)
        {
            try
            {
                var roomCode = request.Data?["roomCode"]?.ToString();
                var username = request.Data?["username"]?.ToString();

                if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(username))
                {
                    return CreateResponse(false, "Room code and username are required");
                }

                var result = lobbyManager.StartGame(roomCode, username);

                if (result.Success)
                {
                    // ✅ THÊM: Lấy thông tin room để tạo UDP match
                    var room = roomManager.GetRoom(roomCode);
                    if (room != null)
                    {
                        // Tạo UDP match session
                        var udpResult = udpGameServer.CreateMatch(
                            roomCode, 
                            room.Player1Username, 
                            room.Player2Username
                        );

                        if (udpResult.Success)
                        {
                            server.Log($"✅ UDP Match created for room {roomCode}");
                            
                            // Trả về thông tin UDP port cho client
                            return CreateResponseWithData(true, result.Message, new Dictionary<string, object>
                            {
                                { "udpPort", 5000 },
                                { "serverIp", "127.0.0.1" }  // TODO: Get actual server IP
                            });
                        }
                        else
                        {
                            server.Log($"⚠️ Failed to create UDP match: {udpResult.Message}");
                        }
                    }
                }

                return CreateResponse(result.Success, result.Message);
            }
            catch (Exception ex)
            {
                server.Log($"❌ HandleLobbyStartGame error: {ex.Message}");
                return CreateResponse(false, $"Error: {ex.Message}");
            }
        }


        //Start game
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


        //Xử lý hành động game
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

        // ✅ THÊM: Handle game end - đóng UDP match và trả client về lobby
        private string HandleGameEnd(Request request)
        {
            try
            {
                var roomCode = request.Data?["roomCode"]?.ToString();
                var username = request.Data?["username"]?.ToString();

                if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(username))
                {
                    return CreateResponse(false, "Room code and username are required");
                }

                // Đóng UDP match
                var udpResult = udpGameServer.EndMatch(roomCode);
                
                if (udpResult.Success)
                {
                    server.Log($"✅ UDP Match ended for room {roomCode}");
                }
                else
                {
                    server.Log($"⚠️ Failed to end UDP match: {udpResult.Message}");
                }

                // Update room status về WAITING (keep room alive for rematch)
                var room = roomManager.GetRoom(roomCode);
                if (room != null)
                {
                    room.Status = "waiting";
                    server.Log($"✅ Room {roomCode} reset to WAITING status");
                }

                return CreateResponse(true, "Game ended, return to lobby");
            }
            catch (Exception ex)
            {
                server.Log($"❌ HandleGameEnd error: {ex.Message}");
                return CreateResponse(false, $"Error: {ex.Message}");
            }
        }

        #endregion


        #region Broadcast List Room
        private string HandleRoomListSubscribe(Request request)
        {
            try
            {
                var username = request.Data?["username"]?.ToString();

                if (string.IsNullOrEmpty(username))
                {
                    return CreateResponse(false, "Username is required");
                }

                roomListUsername = username;
                var result = roomListBroadcaster.Subscribe(username, this);

                if (result.Success)
                {
                    // Tạo danh sách rooms với camelCase property names
                    var roomsData = new List<object>();

                    if (result.Rooms != null)
                    {
                        foreach (var r in result.Rooms)
                        {
                            roomsData.Add(new Dictionary<string, object>
                    {
                        { "roomCode", r.RoomCode },
                        { "roomName", r.RoomName },
                        { "hasPassword", r.HasPassword },
                        { "playerCount", r.PlayerCount },
                        { "status", r.Status }
                    });
                        }
                    }

                    return CreateResponseWithData(true, result.Message, new Dictionary<string, object>
            {
                { "rooms", roomsData }
            });
                }

                return CreateResponse(false, result.Message);
            }
            catch (Exception ex)
            {
                server.Log($"❌ HandleRoomListSubscribe error: {ex.Message}");
                return CreateResponse(false, $"Subscribe error: {ex.Message}");
            }
        }
        
        private string HandleRoomListUnsubscribe(Request request)
        {
            try
            {
                if (!string.IsNullOrEmpty(roomListUsername))
                {
                    roomListBroadcaster.Unsubscribe(roomListUsername);
                    roomListUsername = null;
                }

                return CreateResponse(true, "Unsubscribed");
            }
            catch (Exception ex)
            {
                return CreateResponse(false, $"Unsubscribe error: {ex.Message}");
            }
        }
        #endregion


        #region Response
        private string CreateResponse(bool success, string message, Dictionary<string, object> data = null)
        {
            var response = new Response
            {
                Success = success,
                Message = message,
                RequestId = currentRequestId,
                Data = data ?? new Dictionary<string, object>()
            };
            return JsonSerializer.Serialize(response);
        }

        private string CreateResponseWithData(bool success, string message, Dictionary<string, object> data)
        {
            var response = new
            {
                Success = success,
                Message = message,
                RequestId = currentRequestId,
                Data = data
            };
            return JsonSerializer.Serialize(response);
        }
        #endregion

        #region Close
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
        
        private void CleanupOnDisconnect()
        {
            try
            {
                // Cleanup lobby
                if (!string.IsNullOrEmpty(lobbyRoomCode) && !string.IsNullOrEmpty(lobbyUsername))
                {
                    server.Log($"🧹 Cleanup: {lobbyUsername} from lobby {lobbyRoomCode}");
                    lobbyManager.LeaveLobby(lobbyRoomCode, lobbyUsername);
                }

                // Cleanup room (nếu chưa được cleanup bởi LobbyManager)
                if (!string.IsNullOrEmpty(currentRoomCode) && !string.IsNullOrEmpty(currentUsername))
                {
                    server.Log($"🧹 Cleanup: {currentUsername} from room {currentRoomCode}");
                    roomManager.LeaveRoom(currentRoomCode, currentUsername);
                }
            }
            catch (Exception ex)
            {
                server.Log($"❌ CleanupOnDisconnect error: {ex.Message}");
            }
        }
        #endregion
    }



    #region Class
    public class Request
    {
        public string Action { get; set; }
        public string RequestId { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }

    public class Response
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string RequestId { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }
    #endregion
}