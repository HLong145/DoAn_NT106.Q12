using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Data;

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
                        FROM NGUOIDUNG 
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

        // ✅ LƯU USER VÀO DATABASE - BẢN ĐÃ SỬA
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
                                INSERT INTO NGUOIDUNG (USERNAME, EMAIL, PHONE, PASSWORDHASH, SALT) 
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
                    string query = "SELECT PASSWORDHASH, SALT FROM NGUOIDUNG WHERE USERNAME = @Username";

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
                        ? "SELECT USERNAME FROM NGUOIDUNG WHERE EMAIL = @Contact"
                        : "SELECT USERNAME FROM NGUOIDUNG WHERE PHONE = @Contact";

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
                                UPDATE NGUOIDUNG 
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
                        WHERE TABLE_NAME = 'NGUOIDUNG'";

                    using (var command = new SqlCommand(checkTableQuery, connection))
                    {
                        int tableExists = Convert.ToInt32(command.ExecuteScalar());
                        if (tableExists == 0)
                        {
                            Console.WriteLine("❌ Table NGUOIDUNG does not exist!");
                            return false;
                        }
                    }

                    // Kiểm tra cấu trúc cột
                    string checkColumnsQuery = @"
                        SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'NGUOIDUNG'
                        ORDER BY ORDINAL_POSITION";

                    using (var command = new SqlCommand(checkColumnsQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        Console.WriteLine("📋 Table NGUOIDUNG structure:");
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

        // ✅ CÁC PHƯƠNG THỨC KHÁC GIỮ NGUYÊN
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
                    string query = "SELECT COUNT(*) FROM NGUOIDUNG WHERE USERNAME = @Username";
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
    }
}