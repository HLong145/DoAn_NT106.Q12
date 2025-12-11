using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Data;
using DoAn_NT106.Server;
using MySql.Data.MySqlClient;

namespace DoAn_NT106.Services
{
    public class DatabaseService
    {
        private readonly string connectionString =
            "Server=mainline.proxy.rlwy.net;Port=4330;Database=railway;Uid=root;Pwd=gjtaqRQddaGAkFWFTgWiDMPbyrFqzAql;";

        // =============================
        // 1. TEST CONNECTION
        // =============================
        public bool TestConnection()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    return conn.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        // =============================
        // 2. CHECK TABLE EXISTS
        // =============================
        public bool TableExists(string tableName)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT COUNT(*) 
                    FROM information_schema.tables 
                    WHERE table_schema = DATABASE() 
                      AND table_name = @TableName";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TableName", tableName);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        // =============================
        // 3. SAVE USER
        // =============================
        public bool SaveUser(string username, string email, string passwordHash)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"INSERT INTO USERS (USERNAME, EMAIL, PASSWORD_HASH)
                                     VALUES (@Username, @Email, @PasswordHash)";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                    Console.WriteLine("❌ Duplicate user");
                return false;
            }
        }

        // =============================
        // 4. CREATE ROOM EMPTY
        // =============================
        public bool CreateRoomEmpty(string roomCode, string roomName, string password)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "CALL SP_CREATE_ROOM_EMPTY(@RoomCode, @RoomName, @RoomPassword);";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoomCode", roomCode);
                        cmd.Parameters.AddWithValue("@RoomName", roomName);

                        if (string.IsNullOrEmpty(password))
                            cmd.Parameters.AddWithValue("@RoomPassword", DBNull.Value);
                        else
                            cmd.Parameters.AddWithValue("@RoomPassword", password);

                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ CreateRoomEmpty: " + ex.Message);
                return false;
            }
        }

        // =============================
        // 5. CHECK ROOM CODE EXISTS
        // =============================
        public bool IsRoomCodeExists(string roomCode)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = "CALL SP_CHECK_ROOM_CODE_EXISTS(@RoomCode);";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RoomCode", roomCode);
                    var result = cmd.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) > 0;
                }
            }
        }

        // =============================
        // 6. CREATE ROOM
        // =============================
        public bool CreateRoom(string roomCode, string roomName, string password, string player1)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string query =
                        "CALL SP_CREATE_ROOM(@RoomCode, @RoomName, @RoomPassword, @Player1Username);";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoomCode", roomCode);
                        cmd.Parameters.AddWithValue("@RoomName", roomName);
                        cmd.Parameters.AddWithValue("@RoomPassword",
                            string.IsNullOrEmpty(password) ? DBNull.Value : password);
                        cmd.Parameters.AddWithValue("@Player1Username", player1);

                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ CreateRoom: " + ex.Message);
                return false;
            }
        }

        // =============================
        // 7. JOIN ROOM
        // =============================
        public bool JoinRoom(string roomCode, string password, string username)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "CALL SP_JOIN_ROOM(@RoomCode, @Password, @Username);";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoomCode", roomCode);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ JoinRoom: " + ex.Message);
                return false;
            }
        }

        // =============================
        // 8. LEAVE ROOM
        // =============================
        public void LeaveRoom(string roomCode, string username)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = "CALL SP_LEAVE_ROOM(@RoomCode, @Username);";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RoomCode", roomCode);
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // =============================
        // 9. GET AVAILABLE ROOMS
        // =============================
        public DataTable GetAvailableRooms()
        {
            var table = new DataTable();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = "CALL SP_GET_AVAILABLE_ROOMS();";

                using (var adapter = new MySqlDataAdapter(query, conn))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }

        // =============================
        // 10. CLEANUP INACTIVE ROOMS
        // =============================
        public void CleanupInactiveRooms()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = "CALL SP_CLEANUP_INACTIVE_ROOMS();";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // =============================
        // 11. UPDATE ROOM ACTIVITY
        // =============================
        public void UpdateRoomActivity(string roomCode)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = @"UPDATE ROOMS 
                                 SET LAST_ACTIVITY = NOW()
                                 WHERE ROOM_CODE = @RoomCode;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RoomCode", roomCode);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // =============================
        // 12. UPDATE ROOM STATUS
        // =============================
        public void UpdateRoomStatus(string roomCode, string status)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = @"UPDATE ROOMS 
                                 SET ROOM_STATUS = @Status,
                                     LAST_ACTIVITY = NOW()
                                 WHERE ROOM_CODE = @RoomCode;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@RoomCode", roomCode);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }


}