// =============================================
// FILE: Services/RoomManager.cs
// VIẾT LẠI HOÀN CHỈNH
// =============================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using DoAn_NT106.Services;

namespace DoAn_NT106.Server
{
    public class RoomManager
    {
        // Memory cache để quản lý rooms
        private ConcurrentDictionary<string, GameRoom> activeRooms = new ConcurrentDictionary<string, GameRoom>();
        private Random random = new Random();
        private DatabaseService dbService;
        private System.Timers.Timer cleanupTimer;

        public event Action<string> OnLog;

        public RoomManager()
        {
            dbService = new DatabaseService();

            // ✅ Load tất cả rooms từ database khi khởi chạy
            LoadRoomsFromDatabase();

            // Timer để dọn dẹp room không hoạt động (chạy mỗi 1 giờ)
            cleanupTimer = new System.Timers.Timer(3600000); // 1 hour
            cleanupTimer.Elapsed += (s, e) => CleanupInactiveRooms();
            cleanupTimer.AutoReset = true;
            cleanupTimer.Start();

            Log("✅ RoomManager initialized with database support");
        }

        // =============================================
        // LOAD ROOMS TỪ DATABASE KHI SERVER KHỞI CHẠY
        // =============================================
        private void LoadRoomsFromDatabase()
        {
            try
            {
                var dbRooms = dbService.GetAllRooms();
                int loadedCount = 0;

                foreach (var dbRoom in dbRooms)
                {
                    if (!activeRooms.ContainsKey(dbRoom.RoomCode))
                    {
                        var room = new GameRoom
                        {
                            RoomCode = dbRoom.RoomCode,
                            RoomName = dbRoom.RoomName,
                            Password = dbRoom.Password,
                            Status = dbRoom.Status?.ToLower() ?? "waiting",
                            Player1Username = dbRoom.Player1Username,
                            Player2Username = dbRoom.Player2Username,
                            Player1Client = null, // Không có client reference
                            Player2Client = null,
                            CreatedAt = dbRoom.CreatedAt,
                            LastActivity = dbRoom.LastActivity
                        };

                        if (activeRooms.TryAdd(dbRoom.RoomCode, room))
                        {
                            loadedCount++;
                        }
                    }
                }

                Log($"📦 Loaded {loadedCount} rooms from database into memory");
                Log($"📊 Total rooms in memory: {activeRooms.Count}");
            }
            catch (Exception ex)
            {
                Log($"❌ LoadRoomsFromDatabase error: {ex.Message}");
            }
        }

        // =============================================
        // TẠO PHÒNG MỚI
        // ✅ SỬA: Không cần ClientHandler parameter
        // =============================================
        public (bool Success, string Message, string RoomCode) CreateRoom(
            string roomName,
            string password,
            string creatorUsername,
            ClientHandler creatorClient = null) // Optional, không dùng
        {
            try
            {
                // Generate room code (6 chữ số)
                string roomCode = GenerateUniqueRoomCode();

                if (string.IsNullOrEmpty(roomCode))
                {
                    return (false, "Failed to generate unique room code", null);
                }

                // Lưu vào database TRƯỚC
                var dbResult = dbService.CreateRoom(roomCode, roomName, password, creatorUsername);

                if (!dbResult.Success)
                {
                    return (false, dbResult.Message, null);
                }

                // Tạo room trong memory
                var room = new GameRoom
                {
                    RoomCode = roomCode,
                    RoomName = roomName,
                    Password = password,
                    Status = "waiting",
                    Player1Username = creatorUsername, // Creator là Player 1
                    Player1Client = null, // Không track client reference nữa
                    Player2Username = null,
                    Player2Client = null,
                    CreatedAt = DateTime.Now,
                    LastActivity = DateTime.Now
                };

                if (activeRooms.TryAdd(roomCode, room))
                {
                    Log($"✅ Room created: {roomCode} ({roomName}) by {creatorUsername}");
                    Log($"📊 Total rooms in memory: {activeRooms.Count}");
                    return (true, "Room created successfully", roomCode);
                }

                return (false, "Failed to create room in memory", null);
            }
            catch (Exception ex)
            {
                Log($"❌ CreateRoom error: {ex.Message}");
                return (false, $"Error: {ex.Message}", null);
            }
        }

        // =============================================
        // THAM GIA PHÒNG
        // ✅ SỬA: Không cần ClientHandler parameter
        // =============================================
        public (bool Success, string Message, GameRoom Room) JoinRoom(
            string roomCode,
            string password,
            string username,
            ClientHandler client = null) // Optional, không dùng
        {
            try
            {
                // Kiểm tra trong memory trước
                if (!activeRooms.TryGetValue(roomCode, out GameRoom room))
                {
                    // Không có trong memory, thử load từ database
                    var dbRoom = dbService.GetRoomByCode(roomCode);
                    if (dbRoom == null)
                    {
                        return (false, "Room not found", null);
                    }

                    // Tạo room trong memory
                    room = new GameRoom
                    {
                        RoomCode = dbRoom.RoomCode,
                        RoomName = dbRoom.RoomName,
                        Password = dbRoom.Password,
                        Status = dbRoom.Status?.ToLower() ?? "waiting",
                        Player1Username = dbRoom.Player1Username,
                        Player2Username = dbRoom.Player2Username,
                        Player1Client = null,
                        Player2Client = null,
                        CreatedAt = dbRoom.CreatedAt,
                        LastActivity = DateTime.Now
                    };
                    activeRooms.TryAdd(roomCode, room);
                }

                // Kiểm tra password
                if (!string.IsNullOrEmpty(room.Password))
                {
                    if (string.IsNullOrEmpty(password) || password != room.Password)
                    {
                        return (false, "Incorrect password", null);
                    }
                }

                // Kiểm tra user đã trong room chưa
                if (room.Player1Username == username || room.Player2Username == username)
                {
                    // User đã trong room, cho phép "rejoin"
                    room.LastActivity = DateTime.Now;
                    Log($"🔄 {username} rejoined room {roomCode}");
                    return (true, "Rejoined room successfully", room);
                }

                // Kiểm tra slot trống
                if (!string.IsNullOrEmpty(room.Player1Username) && !string.IsNullOrEmpty(room.Player2Username))
                {
                    return (false, "Room is full", null);
                }

                // Thêm player vào slot trống
                if (string.IsNullOrEmpty(room.Player1Username))
                {
                    room.Player1Username = username;
                }
                else
                {
                    room.Player2Username = username;
                }

                room.LastActivity = DateTime.Now;

                // Cập nhật database
                dbService.JoinRoom(roomCode, password, username);

                Log($"✅ {username} joined room {roomCode}");
                Log($"   Players: {room.Player1Username ?? "empty"} vs {room.Player2Username ?? "empty"}");

                return (true, "Joined room successfully", room);
            }
            catch (Exception ex)
            {
                Log($"❌ JoinRoom error: {ex.Message}");
                return (false, $"Error: {ex.Message}", null);
            }
        }

        // =============================================
        // RỜI PHÒNG
        // ✅ SỬA: Xóa room khỏi cả memory VÀ database khi trống
        // =============================================
        public void LeaveRoom(string roomCode, string username)
        {
            try
            {
                if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(username))
                    return;

                // Cập nhật database
                dbService.LeaveRoom(roomCode, username);

                if (!activeRooms.TryGetValue(roomCode, out GameRoom room))
                    return;

                // Xóa player khỏi room
                if (room.Player1Username == username)
                {
                    room.Player1Username = null;
                    room.Player1Client = null;
                }
                else if (room.Player2Username == username)
                {
                    room.Player2Username = null;
                    room.Player2Client = null;
                }

                room.LastActivity = DateTime.Now;

                Log($"👋 {username} left room {roomCode}");

                // Nếu room trống -> xóa
                if (string.IsNullOrEmpty(room.Player1Username) &&
                    string.IsNullOrEmpty(room.Player2Username))
                {
                    // Xóa khỏi memory
                    activeRooms.TryRemove(roomCode, out _);

                    // Xóa khỏi database
                    var deleteResult = dbService.DeleteRoom(roomCode);

                    Log($"🗑️ Room {roomCode} deleted (empty) - DB: {deleteResult.Message}");
                    Log($"📊 Total rooms in memory: {activeRooms.Count}");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ LeaveRoom error: {ex.Message}");
            }
        }

        // =============================================
        // LẤY DANH SÁCH PHÒNG KHẢ DỤNG
        // ✅ SỬA: Lấy từ MEMORY thay vì database
        // =============================================
        public List<RoomInfo> GetAvailableRooms()
        {
            try
            {
                var rooms = activeRooms.Values
                    .Where(r => r.Status == "waiting" || r.Status == "ready")
                    .Where(r => string.IsNullOrEmpty(r.Player1Username) ||
                                string.IsNullOrEmpty(r.Player2Username)) // Còn slot
                    .Select(r => new RoomInfo
                    {
                        RoomCode = r.RoomCode,
                        RoomName = r.RoomName,
                        HasPassword = !string.IsNullOrEmpty(r.Password),
                        PlayerCount = (string.IsNullOrEmpty(r.Player1Username) ? 0 : 1) +
                                     (string.IsNullOrEmpty(r.Player2Username) ? 0 : 1),
                        Status = r.Status,
                        Player1 = r.Player1Username,
                        Player2 = r.Player2Username,
                        CreatedAt = r.CreatedAt
                    })
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList();

                Log($"📋 GetAvailableRooms: Found {rooms.Count} rooms in memory");
                return rooms;
            }
            catch (Exception ex)
            {
                Log($"❌ GetAvailableRooms error: {ex.Message}");
                return new List<RoomInfo>();
            }
        }

        // =============================================
        // CÁC METHOD KHÁC - GIỮ NGUYÊN
        // =============================================

        public GameRoom GetRoom(string roomCode)
        {
            activeRooms.TryGetValue(roomCode, out GameRoom room);
            return room;
        }

        public bool StartGame(string roomCode)
        {
            if (!activeRooms.TryGetValue(roomCode, out GameRoom room))
                return false;

            if (string.IsNullOrEmpty(room.Player1Username) ||
                string.IsNullOrEmpty(room.Player2Username))
            {
                Log($"⚠️ Cannot start game: room {roomCode} needs 2 players");
                return false;
            }

            room.Status = "playing";
            room.LastActivity = DateTime.Now;

            dbService.UpdateRoomStatus(roomCode, "PLAYING");

            room.GameState = new GameState
            {
                Player1Health = 100,
                Player2Health = 100,
                Player1Stamina = 100,
                Player2Stamina = 100,
                Player1Mana = 100,
                Player2Mana = 100,
                Player1X = 300,
                Player1Y = 400,
                Player2X = 600,
                Player2Y = 400,
                CurrentRound = 1,
                Player1Wins = 0,
                Player2Wins = 0
            };

            Log($"🎮 Game started in room {roomCode}");
            return true;
        }

        public void UpdateGameState(string roomCode, string username, GameAction action)
        {
            if (!activeRooms.TryGetValue(roomCode, out GameRoom room))
                return;

            if (room.GameState == null) return;

            bool isPlayer1 = room.Player1Username == username;

            if (action.Type == "POSITION")
            {
                if (isPlayer1)
                {
                    room.GameState.Player1X = action.X;
                    room.GameState.Player1Y = action.Y;
                }
                else
                {
                    room.GameState.Player2X = action.X;
                    room.GameState.Player2Y = action.Y;
                }
            }

            room.LastActivity = DateTime.Now;

            BroadcastToRoom(roomCode, new
            {
                Action = "GAME_STATE_UPDATE",
                Data = new
                {
                    gameState = room.GameState,
                    lastAction = new
                    {
                        player = username,
                        type = action.Type,
                        actionName = action.ActionName
                    }
                }
            });
        }

        public void BroadcastToRoom(string roomCode, object message)
        {
            if (!activeRooms.TryGetValue(roomCode, out GameRoom room))
                return;

            string json = System.Text.Json.JsonSerializer.Serialize(message);

            // Lưu ý: Vì không track client reference, broadcast sẽ không hoạt động
            // Cần implement qua cơ chế khác (WebSocket hoặc polling)
            if (room.Player1Client != null)
            {
                room.Player1Client.SendMessage(json);
            }

            if (room.Player2Client != null)
            {
                room.Player2Client.SendMessage(json);
            }
        }

        private string GenerateUniqueRoomCode()
        {
            const int maxAttempts = 100;

            for (int i = 0; i < maxAttempts; i++)
            {
                int code = random.Next(0, 1000000);
                string roomCode = code.ToString("D6");

                if (activeRooms.ContainsKey(roomCode))
                    continue;

                if (dbService.IsRoomCodeExists(roomCode))
                    continue;

                return roomCode;
            }

            Log($"❌ Failed to generate unique room code after {maxAttempts} attempts");
            return null;
        }

        private void CleanupInactiveRooms()
        {
            try
            {
                int deleted = dbService.CleanupInactiveRooms();
                if (deleted > 0)
                {
                    Log($"🗑️ Cleaned up {deleted} inactive rooms from database");

                    // Reload rooms từ database để sync
                    LoadRoomsFromDatabase();
                }
            }
            catch (Exception ex)
            {
                Log($"❌ CleanupInactiveRooms error: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            OnLog?.Invoke(message);
        }
    }

    // ===========================
    // DATA CLASSES - GIỮ NGUYÊN
    // ===========================
    public class GameRoom
    {
        public string RoomCode { get; set; }
        public string RoomName { get; set; }
        public string Password { get; set; }
        public string Status { get; set; }

        public string Player1Username { get; set; }
        public ClientHandler Player1Client { get; set; }
        public string Player1Character { get; set; }

        public string Player2Username { get; set; }
        public ClientHandler Player2Client { get; set; }
        public string Player2Character { get; set; }

        public GameState GameState { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivity { get; set; }
    }

    public class GameState
    {
        public int Player1Health { get; set; }
        public int Player2Health { get; set; }
        public int Player1Stamina { get; set; }
        public int Player2Stamina { get; set; }
        public int Player1Mana { get; set; }
        public int Player2Mana { get; set; }
        public int Player1X { get; set; }
        public int Player1Y { get; set; }
        public int Player2X { get; set; }
        public int Player2Y { get; set; }
        public string Player1Action { get; set; }
        public string Player2Action { get; set; }
        public int CurrentRound { get; set; }
        public int Player1Wins { get; set; }
        public int Player2Wins { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public class GameAction
    {
        public string Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string ActionName { get; set; }
    }

    public class RoomInfo
    {
        public string RoomCode { get; set; }
        public string RoomName { get; set; }
        public bool HasPassword { get; set; }
        public int PlayerCount { get; set; }
        public string Status { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}