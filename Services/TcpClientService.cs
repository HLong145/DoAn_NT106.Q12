using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DoAn_NT106.Services
{
    /// <summary>
    /// Service để kết nối TCP Server (async/await + sync wrapper)
    /// </summary>
    public class TcpClientService
    {
        private readonly string serverAddress;
        private readonly int serverPort;

        public TcpClientService(string address = "127.0.0.1", int port = 8080)
        {
            serverAddress = address;
            serverPort = port;
        }

        // ==========================
        // 🟢 ASYNC API
        // ==========================
        public Task<ServerResponse> LogoutAsync(string token, string logoutType = "normal")
        {
            var requestData = new Dictionary<string, object>
    {
        { "token", token },
        { "logoutType", logoutType }
    };
            return SendRequestAsync("LOGOUT", requestData);
        }

      
        public Task<ServerResponse> LoginAsync(string username, string password)
        {
            var requestData = new Dictionary<string, object>
            {
                { "username", username },
                { "password", password }
            };
            return SendRequestAsync("LOGIN", requestData);
        }

        public Task<ServerResponse> RegisterAsync(string username, string email, string phone, string password)
        {
            var requestData = new Dictionary<string, object>
            {
                { "username", username },
                { "email", email ?? "" },
                { "phone", phone ?? "" },
                { "password", password }
            };
            return SendRequestAsync("REGISTER", requestData);
        }

        public Task<ServerResponse> VerifyTokenAsync(string token)
        {
            var requestData = new Dictionary<string, object>
            {
                { "token", token }
            };
            return SendRequestAsync("VERIFY_TOKEN", requestData);
        }

        public Task<ServerResponse> GenerateOtpAsync(string username)
        {
            var requestData = new Dictionary<string, object>
            {
                { "username", username }
            };
            return SendRequestAsync("GENERATE_OTP", requestData);
        }

        public Task<ServerResponse> VerifyOtpAsync(string username, string otp)
        {
            var requestData = new Dictionary<string, object>
            {
                { "username", username },
                { "otp", otp }
            };
            return SendRequestAsync("VERIFY_OTP", requestData);
        }

        public Task<ServerResponse> ResetPasswordAsync(string username, string newPassword)
        {
            var requestData = new Dictionary<string, object>
            {
                { "username", username },
                { "newPassword", newPassword }
            };
            return SendRequestAsync("RESET_PASSWORD", requestData);
        }

        public Task<ServerResponse> GetUserByContactAsync(string contact, bool isEmail)
        {
            var requestData = new Dictionary<string, object>
            {
                { "contact", contact },
                { "isEmail", isEmail }
            };
            return SendRequestAsync("GET_USER_BY_CONTACT", requestData);
        }

        // ==========================
        // ⚙️ CORE ASYNC METHOD
        // ==========================
        private async Task<ServerResponse> SendRequestAsync(string action, Dictionary<string, object> data)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    // Timeout kết nối 5 giây
                    var connectTask = client.ConnectAsync(serverAddress, serverPort);
                    var timeoutTask = Task.Delay(5000);

                    if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
                    {
                        return new ServerResponse
                        {
                            Success = false,
                            Message = $"⏱ Timeout: Cannot connect to server {serverAddress}:{serverPort}"
                        };
                    }

                    using (NetworkStream stream = client.GetStream())
                    {
                        var request = new
                        {
                            Action = action,
                            Data = data
                        };

                        string requestJson = JsonSerializer.Serialize(request);
                        byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);

                        // Gửi request
                        await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                        // Đọc response (có timeout)
                        var buffer = new byte[8192];
                        using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(10)))
                        {
                            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                            string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                            return JsonSerializer.Deserialize<ServerResponse>(responseJson);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return new ServerResponse
                {
                    Success = false,
                    Message = "⏱ Timeout waiting for server response."
                };
            }
            catch (SocketException)
            {
                return new ServerResponse
                {
                    Success = false,
                    Message = $"❌ Cannot connect to server {serverAddress}:{serverPort}. Please check if server is running."
                };
            }
            catch (Exception ex)
            {
                return new ServerResponse
                {
                    Success = false,
                    Message = $"⚠️ Connection error: {ex.Message}"
                };
            }
        }

        // ==========================
        // 🟠 SYNC WRAPPER (cho form cũ)
        // ==========================
        public ServerResponse Register(string username, string email, string phone, string password)
            => RegisterAsync(username, email, phone, password).GetAwaiter().GetResult();

        public ServerResponse Login(string username, string password)
            => LoginAsync(username, password).GetAwaiter().GetResult();

        public ServerResponse ResetPassword(string username, string newPassword)
            => ResetPasswordAsync(username, newPassword).GetAwaiter().GetResult();

        public ServerResponse VerifyToken(string token)
            => VerifyTokenAsync(token).GetAwaiter().GetResult();

        public ServerResponse GenerateOtp(string username)
            => GenerateOtpAsync(username).GetAwaiter().GetResult();

        public ServerResponse VerifyOtp(string username, string otp)
            => VerifyOtpAsync(username, otp).GetAwaiter().GetResult();

        public ServerResponse GetUserByContact(string contact, bool isEmail)
            => GetUserByContactAsync(contact, isEmail).GetAwaiter().GetResult();
        // Sync wrapper
        public ServerResponse Logout(string token, string logoutType = "normal")
            => LogoutAsync(token, logoutType).GetAwaiter().GetResult();

        // ==========================
        // 🟢 ROOM MANAGEMENT - ASYNC
        // ==========================
        public Task<ServerResponse> CreateRoomAsync(string roomName, string password, string username)
        {
            var requestData = new Dictionary<string, object>
            {
                { "roomName", roomName },
                { "password", password ?? "" },
                { "username", username }
            };
            return SendRequestAsync("CREATE_ROOM", requestData);
        }

        public Task<ServerResponse> JoinRoomAsync(string roomCode, string password, string username)
        {
            var requestData = new Dictionary<string, object>
            {
                { "roomCode", roomCode },
                { "password", password ?? "" },
                { "username", username }
            };
            return SendRequestAsync("JOIN_ROOM", requestData);
        }

        public Task<ServerResponse> GetRoomsAsync()
        {
            return SendRequestAsync("GET_ROOMS", new Dictionary<string, object>());
        }

        public Task<ServerResponse> StartGameAsync(string roomCode)
        {
            var requestData = new Dictionary<string, object>
            {
                { "roomCode", roomCode }
            };
            return SendRequestAsync("START_GAME", requestData);
        }

        public Task<ServerResponse> SendGameActionAsync(
            string roomCode,
            string username,
            string actionType,
            int x = 0,
            int y = 0,
            string actionName = null)
        {
            var requestData = new Dictionary<string, object>
            {
                { "roomCode", roomCode },
                { "username", username },
                { "type", actionType },
                { "x", x },
                { "y", y }
            };

            if (!string.IsNullOrEmpty(actionName))
            {
                requestData["actionName"] = actionName;
            }

            return SendRequestAsync("GAME_ACTION", requestData);
        }

        public Task<ServerResponse> LeaveRoomAsync(string roomCode, string username)
        {
            var requestData = new Dictionary<string, object>
            {
                { "roomCode", roomCode },
                { "username", username }
            };
            return SendRequestAsync("LEAVE_ROOM", requestData);
        }

        // ==========================
        // 🟠 ROOM MANAGEMENT - SYNC WRAPPER
        // ==========================
        public ServerResponse CreateRoom(string roomName, string password, string username)
            => CreateRoomAsync(roomName, password, username).GetAwaiter().GetResult();

        public ServerResponse JoinRoom(string roomCode, string password, string username)
            => JoinRoomAsync(roomCode, password, username).GetAwaiter().GetResult();

        public ServerResponse GetRooms()
            => GetRoomsAsync().GetAwaiter().GetResult();

        public ServerResponse StartGame(string roomCode)
            => StartGameAsync(roomCode).GetAwaiter().GetResult();

        public ServerResponse SendGameAction(
            string roomCode,
            string username,
            string actionType,
            int x = 0,
            int y = 0,
            string actionName = null)
            => SendGameActionAsync(roomCode, username, actionType, x, y, actionName).GetAwaiter().GetResult();

        public ServerResponse LeaveRoom(string roomCode, string username)
            => LeaveRoomAsync(roomCode, username).GetAwaiter().GetResult();
    }

    // ==========================
    // DTO: Response
    // ==========================
    public class ServerResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Dictionary<string, object> Data { get; set; }

        public string GetDataValue(string key)
        {
            if (Data != null && Data.ContainsKey(key))
                return Data[key]?.ToString();
            return null;
        }
    }
}
