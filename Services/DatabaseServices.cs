using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Data;
using DoAn_NT106.Server;

namespace DoAn_NT106.Services
{
    public class DatabaseService
    {
        // ✅ CONNECTION STRING
        private readonly string connectionString = "Server=localhost;Database=USERDB;Trusted_Connection=True;TrustServerCertificate=True;";
        private ConcurrentDictionary<string, (string Otp, DateTime ExpireTime)> otps = new();

        // ✅ KIỂM TRA USER TỒN TẠI - BẢN ĐÃ SỬA
        public bool IsUserExists(string username, string email, string phone)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // ✅ FIX: Sử dụng parameterized query an toàn
                    string query = @"
                        SELECT COUNT(*) 
                        FROM PLAYERS 
                        WHERE USERNAME = @Username 
                           OR (@Email IS NOT NULL AND EMAIL = @Email)
                           OR (@Phone IS NOT NULL AND PHONE = @Phone)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? DBNull.Value : (object)email);
                        command.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone);

                        int count = Convert.ToInt32(command.ExecuteScalar());
                        Console.WriteLine($"🔍 IsUserExists: {username} - Count: {count}");
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ IsUserExists ERROR: {ex.Message}");
                // ✅ Trả về true để ngăn tạo user trùng lặp khi có lỗi
                return true;
            }
        }

        // ✅ LƯU USER VÀO DATABASE 
        public bool SaveUserToDatabase(string username, string email, string phone, string hash, string salt)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // ✅ FIX: Sử dụng transaction để đảm bảo tính toàn vẹn
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string query = @"
                                INSERT INTO PLAYERS (USERNAME, EMAIL, PHONE, PASSWORDHASH, SALT) 
                                VALUES (@Username, @Email, @Phone, @Hash, @Salt)";

                            using (var command = new SqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@Username", username);
                                command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? DBNull.Value : (object)email);
                                command.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone);
                                command.Parameters.AddWithValue("@Hash", hash);
                                command.Parameters.AddWithValue("@Salt", salt);

                                int rowsAffected = command.ExecuteNonQuery();

                                transaction.Commit(); // ✅ Commit transaction

                                Console.WriteLine($"✅ SaveUserToDatabase SUCCESS: {username}, Rows: {rowsAffected}");
                                return rowsAffected > 0;
                            }
                        }
                        catch (Exception)
                        {
                            transaction.Rollback(); // ✅ Rollback nếu có lỗi
                            throw;
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"❌ SQL Error saving user {username}: {sqlEx.Message}");
                Console.WriteLine($"❌ SQL Number: {sqlEx.Number}");

                // ✅ Xử lý các lỗi SQL phổ biến
                if (sqlEx.Number == 2627) // Violation of PRIMARY KEY constraint
                {
                    Console.WriteLine("❌ User already exists (primary key violation)");
                }
                else if (sqlEx.Number == 2601) // Violation of UNIQUE constraint
                {
                    Console.WriteLine("❌ Duplicate user data (unique constraint violation)");
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ General Error saving user {username}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Inner Exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        // ✅ XÁC THỰC ĐĂNG NHẬP
        public bool VerifyUserLogin(string username, string password)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT PASSWORDHASH, SALT FROM PLAYERS WHERE USERNAME = @Username";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader["PASSWORDHASH"]?.ToString();
                                string salt = reader["SALT"]?.ToString();

                                if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(salt))
                                    return false;

                                string verifyHash = HashPassword_Sha256(password, salt);
                                return verifyHash == storedHash;
                            }
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Login error: {ex.Message}");
                return false;
            }
        }

        // ✅ TÌM USERNAME BẰNG EMAIL/PHONE
        public string GetUsernameByContact(string contact, bool isEmail)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = isEmail
                        ? "SELECT USERNAME FROM PLAYERS WHERE EMAIL = @Contact"
                        : "SELECT USERNAME FROM PLAYERS WHERE PHONE = @Contact";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Contact", contact);
                        var result = command.ExecuteScalar();
                        return result?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Get username error: {ex.Message}");
                return null;
            }
        }

        // Lấy email theo username
        public string GetEmailByUsername(string username)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT EMAIL FROM PLAYERS WHERE USERNAME = @Username";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        var result = command.ExecuteScalar();
                        return result?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetEmailByUsername error: {ex.Message}");
                return null;
            }
        }

        // ✅ RESET PASSWORD - BẢN ĐÃ SỬA
        public bool ResetPassword(string username, string newPassword)
        {
            try
            {
                string salt = CreateSalt();
                string hash = HashPassword_Sha256(newPassword, salt);

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // ✅ FIX: Sử dụng transaction
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string query = @"
                                UPDATE PLAYERS 
                                SET PASSWORDHASH = @Hash, SALT = @Salt 
                                WHERE USERNAME = @Username";

                            using (var command = new SqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@Hash", hash);
                                command.Parameters.AddWithValue("@Salt", salt);
                                command.Parameters.AddWithValue("@Username", username);

                                int rowsAffected = command.ExecuteNonQuery();
                                transaction.Commit();

                                Console.WriteLine($"✅ ResetPassword: {username}, Rows affected: {rowsAffected}");
                                return rowsAffected > 0;
                            }
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ResetPassword ERROR for {username}: {ex.Message}");
                return false;
            }
        }

        // ✅ KIỂM TRA VÀ SỬA CẤU TRÚC BẢNG
        public bool CheckAndFixTableStructure()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Kiểm tra xem bảng có tồn tại không
                    string checkTableQuery = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_NAME = 'PLAYERS'";

                    using (var command = new SqlCommand(checkTableQuery, connection))
                    {
                        int tableExists = Convert.ToInt32(command.ExecuteScalar());
                        if (tableExists == 0)
                        {
                            Console.WriteLine("❌ Table PLAYERS does not exist!");
                            return false;
                        }
                    }

                    // Kiểm tra cấu trúc cột
                    string checkColumnsQuery = @"
                        SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'PLAYERS'
                        ORDER BY ORDINAL_POSITION";

                    using (var command = new SqlCommand(checkColumnsQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        Console.WriteLine("📋 Table PLAYERS structure:");
                        while (reader.Read())
                        {
                            Console.WriteLine($"  {reader["COLUMN_NAME"]} - {reader["DATA_TYPE"]} - {reader["IS_NULLABLE"]}");
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CheckTableStructure error: {ex.Message}");
                return false;
            }
        }

        // ✅ TEST TOÀN DIỆN
        public void RunDiagnostics()
        {
            Console.WriteLine("🔧 Running Database Diagnostics...");

            // Test kết nối
            bool connectionOk = TestConnection();
            Console.WriteLine($"📡 Database Connection: {connectionOk}");

            if (connectionOk)
            {
                // Test cấu trúc bảng
                bool tableOk = CheckAndFixTableStructure();
                Console.WriteLine($"📊 Table Structure: {tableOk}");

                // Test tạo user giả
                string testUser = "testuser_" + DateTime.Now.Ticks;
                bool userExists = IsUserExists(testUser, null, null);
                Console.WriteLine($"👤 Test User Check: {userExists} (should be false)");

                if (!userExists)
                {
                    string salt = CreateSalt();
                    string hash = HashPassword_Sha256("testpassword", salt);
                    bool saveOk = SaveUserToDatabase(testUser, "test@test.com", "0123456789", hash, salt);
                    Console.WriteLine($"💾 Test User Save: {saveOk}");
                }
            }

            Console.WriteLine("🔧 Diagnostics Complete");
        }

        public string CreateSalt()
        {
            var bytes = new byte[16];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        public string HashPassword_Sha256(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var combined = Encoding.UTF8.GetBytes(password + salt);
            var hash = sha256.ComputeHash(combined);
            return Convert.ToBase64String(hash);
        }

        public string GenerateOtp(string username)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM PLAYERS WHERE USERNAME = @Username";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        if ((int)command.ExecuteScalar() == 0)
                            return null;
                    }
                }

                var bytes = new byte[4];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(bytes);
                }
                int randomNumber = Math.Abs(BitConverter.ToInt32(bytes, 0));
                string otp = (randomNumber % 900000 + 100000).ToString();

                otps[username] = (otp, DateTime.Now.AddMinutes(5));
                return otp;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Generate OTP error: {ex.Message}");
                return null;
            }
        }

        public (bool IsValid, string Message) VerifyOtp(string username, string otp)
        {
            if (!otps.ContainsKey(username))
                return (false, "OTP not found!");

            var (storedOtp, expireTime) = otps[username];

            if (DateTime.Now > expireTime)
            {
                otps.TryRemove(username, out _);
                return (false, "OTP expired!");
            }

            if (storedOtp != otp)
                return (false, "Wrong OTP, try again!");

            otps.TryRemove(username, out _);
            return (true, "Verify OTP successfully");
        }

        public bool TestConnection()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return connection.State == ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection failed: {ex.Message}");
                return false;
            }
        }

        public void CleanExpiredOtps()
        {
            var now = DateTime.Now;
            var expiredKeys = otps
                .Where(kvp => kvp.Value.ExpireTime < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                otps.TryRemove(key, out _);
            }
        }

       

        #region ROOM MANAGEMENT
        /// <summary>
        /// ✅ THÊM MỚI: Tạo room TRỐNG (không có player nào)
        /// </summary>
        public (bool Success, string Message, int? RoomId) CreateRoomEmpty(
            string roomCode,
            string roomName,
            string password)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "EXEC SP_CREATE_ROOM_EMPTY @RoomCode, @RoomName, @RoomPassword";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoomCode", roomCode);
                        command.Parameters.AddWithValue("@RoomName", roomName);
                        command.Parameters.AddWithValue("@RoomPassword",
                            string.IsNullOrEmpty(password) ? DBNull.Value : (object)password);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool success = Convert.ToInt32(reader["Success"]) == 1;
                                string message = reader["Message"].ToString();
                                int? roomId = reader["RoomId"] != DBNull.Value
                                    ? Convert.ToInt32(reader["RoomId"])
                                    : (int?)null;

                                Console.WriteLine($"✅ CreateRoomEmpty: {roomCode} - {message}");
                                return (success, message, roomId);
                            }
                        }
                    }
                }
                return (false, "Unknown error", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CreateRoomEmpty error: {ex.Message}");
                return (false, ex.Message, null);
            }
        }

        /// <summary>
        /// ✅ THÊM MỚI: Xóa room theo code
        /// (Chỉ thêm nếu chưa có method này)
        /// </summary>
        public bool DeleteRoom(string roomCode)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM ROOMS WHERE ROOM_CODE = @RoomCode";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoomCode", roomCode);
                        int rowsAffected = command.ExecuteNonQuery();

                        Console.WriteLine($"✅ DeleteRoom: {roomCode} - {rowsAffected} rows deleted");
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DeleteRoom error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ✅ THÊM MỚI: Kiểm tra room code đã tồn tại
        /// </summary>
        public bool RoomCodeExists(string roomCode)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM ROOMS WHERE ROOM_CODE = @RoomCode";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoomCode", roomCode);
                        int count = (int)command.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RoomCodeExists error: {ex.Message}");
                return true; // Return true để tránh tạo trùng code
            }
        }

        /// <summary>
        /// Kiểm tra room code đã tồn tại trong database chưa
        /// </summary>
        public bool IsRoomCodeExists(string roomCode)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "EXEC SP_CHECK_ROOM_CODE_EXISTS @RoomCode";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoomCode", roomCode);
                        var result = command.ExecuteScalar();
                        return Convert.ToInt32(result) == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ IsRoomCodeExists error: {ex.Message}");
                return true; // Trả về true để tránh tạo room trùng khi có lỗi
            }
        }

        /// <summary>
        /// Tạo room mới trong database
        /// </summary>
        public (bool Success, string Message, int? RoomId) CreateRoom(
            string roomCode,
            string roomName,
            string password,
            string player1Username)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "EXEC SP_CREATE_ROOM @RoomCode, @RoomName, @RoomPassword, @Player1Username";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoomCode", roomCode);
                        command.Parameters.AddWithValue("@RoomName", roomName);
                        command.Parameters.AddWithValue("@RoomPassword",
                            string.IsNullOrEmpty(password) ? DBNull.Value : (object)password);
                        command.Parameters.AddWithValue("@Player1Username", player1Username);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool success = Convert.ToInt32(reader["Success"]) == 1;
                                string message = reader["Message"].ToString();
                                int? roomId = reader["RoomId"] != DBNull.Value
                                    ? Convert.ToInt32(reader["RoomId"])
                                    : (int?)null;

                                Console.WriteLine($"✅ CreateRoom: {roomCode} - {message}");
                                return (success, message, roomId);
                            }
                        }
                    }
                }
                return (false, "Unknown error", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CreateRoom error: {ex.Message}");
                return (false, ex.Message, null);
            }
        }

        /// <summary>
        /// Tham gia room
        /// </summary>
        public (bool Success, string Message) JoinRoom(string roomCode, string password, string username)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "EXEC SP_JOIN_ROOM @RoomCode, @Password, @Username";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoomCode", roomCode);
                        command.Parameters.AddWithValue("@Password",
                            string.IsNullOrEmpty(password) ? DBNull.Value : (object)password);
                        command.Parameters.AddWithValue("@Username", username);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool success = Convert.ToInt32(reader["Success"]) == 1;
                                string message = reader["Message"].ToString();
                                return (success, message);
                            }
                        }
                    }
                }
                return (false, "Unknown error");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ JoinRoom error: {ex.Message}");
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Rời khỏi room
        /// </summary>
        public (bool Success, string Message) LeaveRoom(string roomCode, string username)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "EXEC SP_LEAVE_ROOM @RoomCode, @Username";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoomCode", roomCode);
                        command.Parameters.AddWithValue("@Username", username);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool success = Convert.ToInt32(reader["Success"]) == 1;
                                string message = reader["Message"].ToString();
                                return (success, message);
                            }
                        }
                    }
                }
                return (false, "Unknown error");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LeaveRoom error: {ex.Message}");
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách room đang chờ người chơi
        /// </summary>
        public List<RoomInfo> GetAvailableRooms()
        {
            var rooms = new List<RoomInfo>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "EXEC SP_GET_AVAILABLE_ROOMS";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rooms.Add(new RoomInfo
                            {
                                RoomCode = reader["RoomCode"].ToString(),
                                RoomName = reader["RoomName"].ToString(),
                                HasPassword = Convert.ToInt32(reader["HasPassword"]) == 1,
                                PlayerCount = Convert.ToInt32(reader["PlayerCount"]),
                                Status = reader["Status"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetAvailableRooms error: {ex.Message}");
            }

            return rooms;
        }

        /// <summary>
        /// Lấy thông tin room theo code
        /// </summary>
        public RoomDbInfo GetRoomByCode(string roomCode)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                SELECT ROOM_ID, ROOM_CODE, ROOM_NAME, ROOM_PASSWORD,
                       PLAYER1_USERNAME, PLAYER2_USERNAME,
                       PLAYER1_CHARACTER, PLAYER2_CHARACTER,
                       ROOM_STATUS, CREATED_AT, LAST_ACTIVITY
                FROM ROOMS 
                WHERE ROOM_CODE = @RoomCode";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoomCode", roomCode);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new RoomDbInfo
                                {
                                    RoomId = Convert.ToInt32(reader["ROOM_ID"]),
                                    RoomCode = reader["ROOM_CODE"].ToString(),
                                    RoomName = reader["ROOM_NAME"].ToString(),
                                    Password = reader["ROOM_PASSWORD"]?.ToString(),
                                    Player1Username = reader["PLAYER1_USERNAME"]?.ToString(),
                                    Player2Username = reader["PLAYER2_USERNAME"]?.ToString(),
                                    Player1Character = reader["PLAYER1_CHARACTER"]?.ToString(),
                                    Player2Character = reader["PLAYER2_CHARACTER"]?.ToString(),
                                    Status = reader["ROOM_STATUS"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["CREATED_AT"]),
                                    LastActivity = reader["LAST_ACTIVITY"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["LAST_ACTIVITY"])
                                        : DateTime.Now
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetRoomByCode error: {ex.Message}");
            }

            return null;
        }
       
        /// <summary>
        /// Cập nhật thời gian hoạt động cuối cùng của room
        /// </summary>
        public bool UpdateRoomActivity(string roomCode)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE ROOMS SET LAST_ACTIVITY = GETDATE() WHERE ROOM_CODE = @RoomCode";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoomCode", roomCode);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ UpdateRoomActivity error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cập nhật trạng thái room
        /// </summary>
        public bool UpdateRoomStatus(string roomCode, string status)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                UPDATE ROOMS 
                SET ROOM_STATUS = @Status, LAST_ACTIVITY = GETDATE() 
                WHERE ROOM_CODE = @RoomCode";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoomCode", roomCode);
                        command.Parameters.AddWithValue("@Status", status);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ UpdateRoomStatus error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Dọn dẹp các room không hoạt động (gọi định kỳ)
        /// </summary>
        public int CleanupInactiveRooms()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "EXEC SP_CLEANUP_INACTIVE_ROOMS";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int deleted = Convert.ToInt32(reader["DeletedRooms"]);
                                Console.WriteLine($"🗑️ Cleaned up {deleted} inactive rooms");
                                return deleted;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CleanupInactiveRooms error: {ex.Message}");
            }
            return 0;
        }

        #endregion

        #region SAVECHAT

        // <summary>
        /// Lưu tin nhắn Global Chat vào database
        /// </summary>
        public (bool Success, string Message) SaveGlobalChatMessage(string username, string messageText)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand("SP_SAVE_GLOBAL_CHAT_MESSAGE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@MessageText", messageText);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool success = reader.GetInt32(0) == 1;
                            string message = reader.GetString(1);
                            return (success, message);
                        }
                    }
                }
                return (false, "No response from database");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saving global chat message: {ex.Message}");
                return (false, $"Database error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy lịch sử Global Chat từ database
        /// </summary>
        public List<ChatMessage> GetGlobalChatHistory(int limit = 50)
        {
            var messages = new List<ChatMessage>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand("SP_GET_GLOBAL_CHAT_HISTORY", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Limit", limit);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(new ChatMessage
                            {
                                MessageId = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                MessageText = reader.GetString(2),
                                SentAt = reader.GetDateTime(3)
                            });
                        }
                    }
                }

                // Đảo ngược để có thứ tự từ cũ đến mới
                messages.Reverse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting global chat history: {ex.Message}");
            }

            return messages;
        }

        /// <summary>
        /// Lưu tin nhắn Lobby Chat vào database
        /// </summary>
        public (bool Success, string Message) SaveLobbyChatMessage(string roomCode, string username, string messageText)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand("SP_SAVE_LOBBY_CHAT_MESSAGE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@RoomCode", roomCode);
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@MessageText", messageText);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool success = reader.GetInt32(0) == 1;
                            string message = reader.GetString(1);
                            return (success, message);
                        }
                    }
                }
                return (false, "No response from database");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saving lobby chat message: {ex.Message}");
                return (false, $"Database error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy lịch sử Lobby Chat từ database
        /// </summary>
        public List<LobbyChatMessage> GetLobbyChatHistory(string roomCode, int limit = 50)
        {
            var messages = new List<LobbyChatMessage>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand("SP_GET_LOBBY_CHAT_HISTORY", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@RoomCode", roomCode);
                    command.Parameters.AddWithValue("@Limit", limit);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        // Kiểm tra nếu có cột Success (nghĩa là room không tồn tại)
                        if (reader.FieldCount == 2 && reader.GetName(0) == "Success")
                        {
                            // Room không tồn tại, trả về list rỗng
                            return messages;
                        }

                        while (reader.Read())
                        {
                            messages.Add(new LobbyChatMessage
                            {
                                MessageId = reader.GetInt32(0),
                                RoomCode = reader.GetString(1),
                                Username = reader.GetString(2),
                                MessageText = reader.GetString(3),
                                SentAt = reader.GetDateTime(4)
                            });
                        }
                    }
                }

                // Đảo ngược để có thứ tự từ cũ đến mới
                messages.Reverse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting lobby chat history: {ex.Message}");
            }

            return messages;
        }


        #endregion

        #region CLASSES
        public class RoomDbInfo
        {
            public int RoomId { get; set; }
            public string RoomCode { get; set; }
            public string RoomName { get; set; }
            public string Password { get; set; }
            public string Player1Username { get; set; }
            public string Player2Username { get; set; }
            public string Player1Character { get; set; }
            public string Player2Character { get; set; }
            public string Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastActivity { get; set; }
        }
        public class ChatMessage
        {
            public int MessageId { get; set; }
            public string Username { get; set; }
            public string MessageText { get; set; }
            public DateTime SentAt { get; set; }
        }

        public class LobbyChatMessage : ChatMessage
        {
            public string RoomCode { get; set; }
        }

        #endregion
    }


}