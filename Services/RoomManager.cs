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
        // Memory cache để quản lý real-time connections
        private ConcurrentDictionary<string, GameRoom> activeRooms = new ConcurrentDictionary<string, GameRoom>();
        private Random random = new Random();
        private DatabaseService dbService;
        private System.Timers.Timer cleanupTimer;

        public event Action<string> OnLog;

        public RoomManager()
        {
            dbService = new DatabaseService();

            // Timer để dọn dẹp room không hoạt động (chạy mỗi 1 giờ)
            cleanupTimer = new System.Timers.Timer(3600000); // 1 hour
            cleanupTimer.Elapsed += (s, e) => CleanupInactiveRooms();
            cleanupTimer.AutoReset = true;
            cleanupTimer.Start();

            Log("✅ RoomManager initialized with database support");
        }

        // ===========================
        // TẠO PHÒNG MỚI (với database)
        // ===========================
        public (bool Success, string Message, string RoomCode) CreateRoom(
            string roomName,
            string password,
            string creatorUsername,
            ClientHandler creatorClient)
        {
            try
            {
                // Generate room code (6 chữ số, tự động retry nếu trùng)
                string roomCode = GenerateUniqueRoomCode();

                if (string.IsNullOrEmpty(roomCode))
                {
                    return (false, "Failed to generate unique room code", null);
                }

                // Lưu vào database
                var dbResult = dbService.CreateRoom(roomCode, roomName, password, creatorUsername);

                if (!dbResult.Success)
                {
                    return (false, dbResult.Message, null);
                }

                // Tạo room trong memory để quản lý real-time
                var room = new GameRoom
                {
                    RoomCode = roomCode,
                    RoomName = roomName,
                    Password = password,
                    Status = "waiting",
                    Player1Username = creatorUsername,
                    Player1Client = creatorClient,
                    CreatedAt = DateTime.Now,
                    LastActivity = DateTime.Now
                };

                if (activeRooms.TryAdd(roomCode, room))
                {
                    Log($"✅ Room created: {roomCode} ({roomName}) by {creatorUsername}");
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

        // ===========================
        // THAM GIA PHÒNG (với database)
        // ===========================
        public (bool Success, string Message, GameRoom Room) JoinRoom(
            string roomCode,
            string password,
            string username,
            ClientHandler client)
        {
            try
            {
                // Kiểm tra trong database
                var dbResult = dbService.JoinRoom(roomCode, password, username);

                if (!dbResult.Success)
                {
                    return (false, dbResult.Message, null);
                }

                // Cập nhật memory cache
                if (!activeRooms.TryGetValue(roomCode, out GameRoom room))
                {
                    // Room chưa có trong memory, load từ database
                    var dbRoom = dbService.GetRoomByCode(roomCode);
                    if (dbRoom == null)
                    {
                        return (false, "Room not found", null);
                    }

                    room = new GameRoom
                    {
                        RoomCode = dbRoom.RoomCode,
                        RoomName = dbRoom.RoomName,
                        Password = dbRoom.Password,
                        Status = dbRoom.Status.ToLower(),
                        Player1Username = dbRoom.Player1Username,
                        CreatedAt = dbRoom.CreatedAt,
                        LastActivity = DateTime.Now
                    };
                    activeRooms.TryAdd(roomCode, room);
                }

                // Thêm player 2
                room.Player2Username = username;
                room.Player2Client = client;
                room.LastActivity = DateTime.Now;

                Log($"✅ {username} joined room {roomCode}");

                // Thông báo cho player 1
                BroadcastToRoom(roomCode, new
                {
                    Action = "PLAYER_JOINED",
                    Data = new { username = username }
                });

                return (true, "Joined room successfully", room);
            }
            catch (Exception ex)
            {
                Log($"❌ JoinRoom error: {ex.Message}");
                return (false, $"Error: {ex.Message}", null);
            }
        }

        // ===========================
        // RỜI PHÒNG (với database)
        // ===========================
        public void LeaveRoom(string roomCode, string username)
        {
            try
            {
                // Cập nhật database
                dbService.LeaveRoom(roomCode, username);

                if (!activeRooms.TryGetValue(roomCode, out GameRoom room))
                    return;

                bool wasPlayer1 = room.Player1Username == username;
                room.LastActivity = DateTime.Now;

                if (wasPlayer1)
                {
                    room.Player1Username = null;
                    room.Player1Client = null;
                }
                else
                {
                    room.Player2Username = null;
                    room.Player2Client = null;
                }

                Log($"👋 {username} left room {roomCode}");

                // Nếu phòng trống -> xóa khỏi memory
                if (string.IsNullOrEmpty(room.Player1Username) &&
                    string.IsNullOrEmpty(room.Player2Username))
                {
                    activeRooms.TryRemove(roomCode, out _);
                    Log($"🗑️ Room {roomCode} removed from memory (empty)");
                }
                else
                {
                    // Thông báo cho người còn lại
                    BroadcastToRoom(roomCode, new
                    {
                        Action = "PLAYER_LEFT",
                        Data = new { username = username }
                    });
                }
            }
            catch (Exception ex)
            {
                Log($"❌ LeaveRoom error: {ex.Message}");
            }
        }

        // ===========================
        // LẤY DANH SÁCH PHÒNG (từ database)
        // ===========================
        public List<RoomInfo> GetAvailableRooms()
        {
            return dbService.GetAvailableRooms();
        }

        // ===========================
        // BẮT ĐẦU GAME
        // ===========================
        public bool StartGame(string roomCode)
        {
            if (!activeRooms.TryGetValue(roomCode, out GameRoom room))
                return false;

            if (string.IsNullOrEmpty(room.Player2Username))
            {
                Log($"⚠️ Cannot start game: room {roomCode} needs 2 players");
                return false;
            }

            room.Status = "playing";
            room.LastActivity = DateTime.Now;

            // Cập nhật database
            dbService.UpdateRoomStatus(roomCode, "PLAYING");

            // Khởi tạo game state
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

            // Broadcast START_GAME đến cả 2 client
            BroadcastToRoom(roomCode, new
            {
                Action = "START_GAME",
                Data = new
                {
                    roomCode = roomCode,
                    player1 = room.Player1Username,
                    player2 = room.Player2Username,
                    gameState = room.GameState
                }
            });

            return true;
        }

        // ===========================
        // CẬP NHẬT VỊ TRÍ/HÀNH ĐỘNG
        // ===========================
        public void UpdateGameState(string roomCode, string username, GameAction action)
        {
            if (!activeRooms.TryGetValue(roomCode, out GameRoom room))
                return;

            if (room.Status != "playing")
                return;

            var state = room.GameState;
            bool isPlayer1 = room.Player1Username == username;

            if (action.Type == "MOVE")
            {
                if (isPlayer1)
                {
                    state.Player1X = action.X;
                    state.Player1Y = action.Y;
                }
                else
                {
                    state.Player2X = action.X;
                    state.Player2Y = action.Y;
                }
            }
            else if (action.Type == "ACTION")
            {
                if (isPlayer1)
                    state.Player1Action = action.ActionName;
                else
                    state.Player2Action = action.ActionName;
            }

            state.LastUpdate = DateTime.Now;
            room.LastActivity = DateTime.Now;

            // Broadcast state mới
            BroadcastToRoom(roomCode, new
            {
                Action = "GAME_UPDATE",
                Data = new
                {
                    roomCode = roomCode,
                    gameState = state,
                    fromPlayer = username
                }
            });
        }

        // ===========================
        // BROADCAST TỚI PHÒNG
        // ===========================
        public void BroadcastToRoom(string roomCode, object message)
        {
            if (!activeRooms.TryGetValue(roomCode, out GameRoom room))
                return;

            string json = System.Text.Json.JsonSerializer.Serialize(message);

            if (room.Player1Client != null)
            {
                room.Player1Client.SendMessage(json);
            }

            if (room.Player2Client != null)
            {
                room.Player2Client.SendMessage(json);
            }
        }

        // ===========================
        // GENERATE UNIQUE ROOM CODE (6 chữ số)
        // ===========================
        private string GenerateUniqueRoomCode()
        {
            const int maxAttempts = 100;

            for (int i = 0; i < maxAttempts; i++)
            {
                // Generate số từ 000000 đến 999999
                int code = random.Next(0, 1000000);
                string roomCode = code.ToString("D6"); // Format 6 chữ số (có leading zeros)

                // Kiểm tra trong memory
                if (activeRooms.ContainsKey(roomCode))
                    continue;

                // Kiểm tra trong database
                if (dbService.IsRoomCodeExists(roomCode))
                    continue;

                return roomCode;
            }

            Log($"❌ Failed to generate unique room code after {maxAttempts} attempts");
            return null;
        }

        // ===========================
        // DỌN DẸP ROOM KHÔNG HOẠT ĐỘNG
        // ===========================
        private void CleanupInactiveRooms()
        {
            try
            {
                int deleted = dbService.CleanupInactiveRooms();
                if (deleted > 0)
                {
                    Log($"🗑️ Cleaned up {deleted} inactive rooms from database");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ CleanupInactiveRooms error: {ex.Message}");
            }
        }

        // ===========================
        // HELPERS
        // ===========================
        private void Log(string message)
        {
            OnLog?.Invoke(message);
        }

        public GameRoom GetRoom(string roomCode)
        {
            activeRooms.TryGetValue(roomCode, out GameRoom room);
            return room;
        }
    }

    // ===========================
    // DATA CLASSES
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
    }
}