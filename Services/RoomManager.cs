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
        public RoomListBroadcaster RoomListBroadcaster { get; set; }

        public RoomManager()
        {
            dbService = new DatabaseService();

            // ✅ THÊM MỚI: Load rooms từ database khi khởi động
            LoadRoomsFromDatabase();

            cleanupTimer = new System.Timers.Timer(3600000);
            cleanupTimer.Elapsed += (s, e) => CleanupInactiveRooms();
            cleanupTimer.AutoReset = true;
            cleanupTimer.Start();

            Log("✅ RoomManager initialized with database support");
        }

        private void LoadRoomsFromDatabase()
        {
            try
            {
                Log($"🔄 Loading rooms from database...");

                var rooms = dbService.GetAvailableRooms();
                Log($"📊 Database returned {rooms?.Count ?? 0} rooms");

                if (rooms == null || rooms.Count == 0)
                {
                    Log($"ℹ️ No rooms in database to load");
                    return;
                }

                foreach (var roomInfo in rooms)
                {
                    // Load chi tiết room từ database
                    var dbRoom = dbService.GetRoomByCode(roomInfo.RoomCode);
                    if (dbRoom != null)
                    {
                        var room = new GameRoom
                        {
                            RoomCode = dbRoom.RoomCode,
                            RoomName = dbRoom.RoomName,
                            Password = dbRoom.Password,
                            Status = dbRoom.Status?.ToLower() ?? "waiting",
                            Player1Username = dbRoom.Player1Username,
                            Player2Username = dbRoom.Player2Username,
                            CreatedAt = dbRoom.CreatedAt,
                            LastActivity = DateTime.Now,
                            Player1Client = null,
                            Player2Client = null
                        };

                        if (activeRooms.TryAdd(dbRoom.RoomCode, room))
                        {
                            Log($"   ✅ Loaded room: {dbRoom.RoomCode} ({dbRoom.RoomName})");
                        }
                    }
                }

                Log($"✅ Loaded {activeRooms.Count} rooms from database into memory");
            }
            catch (Exception ex)
            {
                Log($"❌ LoadRoomsFromDatabase error: {ex.Message}");
            }
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
                string roomCode = GenerateUniqueRoomCode();
                if (string.IsNullOrEmpty(roomCode))
                {
                    return (false, "Failed to generate unique room code", null);
                }

                var dbResult = dbService.CreateRoom(roomCode, roomName, password, creatorUsername);
                if (!dbResult.Success)
                {
                    return (false, dbResult.Message, null);
                }

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

                    // ✅ THÊM MỚI: Broadcast room list update
                    RoomListBroadcaster?.BroadcastRoomList();

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
                var dbResult = dbService.JoinRoom(roomCode, password, username);
                if (!dbResult.Success)
                {
                    return (false, dbResult.Message, null);
                }

                if (!activeRooms.TryGetValue(roomCode, out GameRoom room))
                {
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

                room.Player2Username = username;
                room.Player2Client = client;
                room.LastActivity = DateTime.Now;

                Log($"✅ {username} joined room {roomCode}");

                BroadcastToRoom(roomCode, new
                {
                    Action = "PLAYER_JOINED",
                    Data = new { username = username }
                });

                // ✅ THÊM MỚI: Broadcast room list (room đã đầy)
                RoomListBroadcaster?.BroadcastRoomList();

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
                {
                    Log($"⚠️ Room {roomCode} not found in memory");
                    return;
                }

                bool wasPlayer1 = room.Player1Username == username;
                bool wasPlayer2 = room.Player2Username == username;

                if (!wasPlayer1 && !wasPlayer2)
                {
                    Log($"⚠️ {username} is not in room {roomCode}");
                    return;
                }

                room.LastActivity = DateTime.Now;

                if (wasPlayer1)
                {
                    room.Player1Username = null;
                    room.Player1Client = null;
                    Log($"👋 {username} left room {roomCode} (was Player 1)");
                }
                else if (wasPlayer2)
                {
                    room.Player2Username = null;
                    room.Player2Client = null;
                    Log($"👋 {username} left room {roomCode} (was Player 2)");
                }

                // ✅ FIX: Kiểm tra và xóa room nếu trống
                bool roomEmpty = string.IsNullOrEmpty(room.Player1Username) &&
                                 string.IsNullOrEmpty(room.Player2Username);

                if (roomEmpty)
                {
                    // Xóa khỏi memory
                    activeRooms.TryRemove(roomCode, out _);

                    // Xóa khỏi database
                    dbService.DeleteRoom(roomCode);

                    Log($"🗑️ Room {roomCode} deleted (empty)");
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

                // ✅ Broadcast room list update
                RoomListBroadcaster?.BroadcastRoomList();
            }
            catch (Exception ex)
            {
                Log($"❌ LeaveRoom error: {ex.Message}");
            }
        }

        // ===========================
        // LẤY DANH SÁCH PHÒNG (từ memory cache, không phải database)
        // ===========================
        public List<RoomInfo> GetAvailableRooms()
        {
            try
            {
                var rooms = new List<RoomInfo>();

                foreach (var kvp in activeRooms)
                {
                    var room = kvp.Value;

                    // Chỉ lấy các phòng đang waiting và chưa đầy
                    if (room.Status?.ToLower() == "waiting")
                    {
                        int playerCount = 0;
                        if (!string.IsNullOrEmpty(room.Player1Username)) playerCount++;
                        if (!string.IsNullOrEmpty(room.Player2Username)) playerCount++;

                        // Chỉ hiển thị phòng chưa đầy
                        if (playerCount < 2)
                        {
                            rooms.Add(new RoomInfo
                            {
                                RoomCode = room.RoomCode,
                                RoomName = room.RoomName,
                                HasPassword = !string.IsNullOrEmpty(room.Password),
                                PlayerCount = playerCount,
                                Status = room.Status
                            });
                        }
                    }
                }

                Log($"📋 GetAvailableRooms: Found {rooms.Count} rooms in memory (Total in cache: {activeRooms.Count})");

                return rooms;
            }
            catch (Exception ex)
            {
                Log($"❌ GetAvailableRooms error: {ex.Message}");
                return new List<RoomInfo>();
            }
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