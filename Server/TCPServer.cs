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

        private Task _acceptTask;                     // 💡 Giữ task AcceptClients
        private CancellationTokenSource cts;          // 💡 Dùng để hủy mềm

        public event Action<string> OnLog;
        public bool IsRunning => isRunning;

        public TcpServer()
        {
            dbService = new DatabaseService();
            tokenManager = new TokenManager();
            validationService = new ValidationService();
            securityService = new SecurityService();
        }

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

                _acceptTask = Task.Run(() => AcceptClients(cts.Token)); // 💡 truyền token hủy
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
                cts.Cancel(); // 💡 yêu cầu dừng AcceptClients()
                listener?.Stop();

                // 💡 Đóng tất cả client an toàn
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

                // 💡 Đợi vòng AcceptClients kết thúc
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
                        // 💡 Listener bị dừng khi Stop() gọi -> thoát vòng
                        break;
                    }
                    catch (InvalidOperationException)
                    {
                        // 💡 Listener đã bị dispose
                        break;
                    }

                    if (client == null) break;

                    var clientHandler = new ClientHandler(client, this, dbService, tokenManager,
                                                          validationService, securityService);

                    lock (connectedClients)
                    {
                        connectedClients.Add(clientHandler);
                    }

                    Log($"📱 New client connected. Total: {connectedClients.Count}");

                    // 💡 Xử lý client trong luồng riêng
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
    }

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

        private bool isNormalLogout = false;
        public ClientHandler(TcpClient client, TcpServer server, DatabaseService dbService,
                      TokenManager tokenManager, ValidationService validationService,
                      SecurityService securityService)
        {
            tcpClient = client;
            this.server = server;
            this.dbService = dbService;
            this.tokenManager = tokenManager;
            stream = client.GetStream();
            this.validationService = validationService;
            this.securityService = securityService;
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
                Close();
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
                            writer.WriteString(property.Name, "***HIDDEN***"); // Ẩn password
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
                // Nếu có lỗi parse JSON, trả về string đã được xử lý đơn giản
                return Regex.Replace(json, @"""password""\s*:\s*""[^""]*""", @"""password"":""***HIDDEN***""", RegexOptions.IgnoreCase);
            }
        }
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

        private string HandleRegister(Request request)
        {
            try
            {
                var username = request.Data?["username"]?.ToString();
                var email = request.Data.ContainsKey("email") ? request.Data["email"]?.ToString() : null;
                var phone = request.Data.ContainsKey("phone") ? request.Data["phone"]?.ToString() : null;
                var password = request.Data?["password"]?.ToString();

                // ✅ THÊM LOG CHI TIẾT
                server.Log($"🔍 Register attempt: Username='{username}', Email='{email}', Phone='{phone}'");

                // ✅ VALIDATE INPUT
                var validationResult = validationService.ValidateRegistration(username, email, phone, password);
                if (!validationResult.IsValid)
                {
                    server.Log($"❌ Register validation failed: {validationResult.Message}");
                    return CreateResponse(false, validationResult.Message);
                }

                // ✅ CHECK EXISTING USER
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

                // ✅ CHECK BRUTE-FORCE
                if (!securityService.CheckLoginAttempts(username))
                {
                    int remainingMinutes = securityService.GetLockoutMinutes(username);
                    return CreateResponse(false, $"Account locked. Try again in {remainingMinutes} minutes.");
                }

                bool loginSuccess = dbService.VerifyUserLogin(username, password);

                // ✅ RECORD ATTEMPT
                securityService.RecordLoginAttempt(username, loginSuccess);

                if (loginSuccess)
                {
                    string token = tokenManager.GenerateToken(username); // ✅ Server tạo token
                    currentToken = token;

                    return CreateResponse(true, "Login successful", new Dictionary<string, object>
                    {
                          { "token", token }, // ✅ Trả token về client
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

        public void Close()
        {
            try
            {
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
        private string HandleLogout(Request request)
        {
            try
            {
                var token = request.Data?["token"]?.ToString();
                var logoutType = request.Data?["logoutType"]?.ToString(); // "normal" hoặc "complete"

                if (string.IsNullOrEmpty(token))
                {
                    return CreateResponse(false, "Token is required for logout");
                }

                // ✅ Nếu là logout bình thường (giữ remember me), không revoke token
                if (logoutType == "normal")
                {
                    SetNormalLogout(); // Đánh dấu không revoke token khi đóng kết nối
                    return CreateResponse(true, "Logout successful (token preserved)");
                }
                else
                {
                    // ✅ Logout hoàn toàn - revoke token
                    tokenManager.RevokeToken(token);
                    return CreateResponse(true, "Logout successful (token revoked)");
                }
            }
            catch (Exception ex)
            {
                return CreateResponse(false, $"Logout error: {ex.Message}");
            }
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