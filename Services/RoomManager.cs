using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using DoAn_NT106.Services;

namespace DoAn_NT106.Server
{
    #region Class definition

    public class RoomManager
    {
        #endregion

        #region Fields

        private ConcurrentDictionary<string, GameRoom> activeRooms = new ConcurrentDictionary<string, GameRoom>();

        private Random random = new Random();
        private DatabaseService dbService;
        private System.Timers.Timer cleanupTimer;

        private const int CLEANUP_INTERVAL_MS = 10000;
        private const int EMPTY_ROOM_TIMEOUT_SECONDS = 30;

        #endregion

        #region Events and dependencies

        public event Action<string> OnLog;

        public RoomListBroadcaster RoomListBroadcaster { get; set; }

        #endregion

        #region Constructor

        public RoomManager()
        {
            dbService = new DatabaseService();

            // Load rooms từ database khi khởi động
            LoadRoomsFromDatabase();

            //Timer cleanup mỗi 10 giây thay vì 1 giờ
            cleanupTimer = new System.Timers.Timer(CLEANUP_INTERVAL_MS);
            cleanupTimer.Elapsed += CleanupEmptyRooms;
            cleanupTimer.AutoReset = true;
            cleanupTimer.Start();

            Log($"✅ RoomManager initialized (cleanup interval: {CLEANUP_INTERVAL_MS}ms, timeout: {EMPTY_ROOM_TIMEOUT_SECONDS}s)");
        }

        #endregion

        #region Cleanup timers
        private void CleanupEmptyRooms(object sender, ElapsedEventArgs e)
        {
            try
            {
                var now = DateTime.Now;
                var roomsToDelete = new List<string>();

                foreach (var kvp in activeRooms)
                {
                    var room = kvp.Value;

                    // Check nếu room trống và đã quá timeout
                    if (string.IsNullOrEmpty(room.Player1Username) &&
                        string.IsNullOrEmpty(room.Player2Username))
                    {
                        var emptyDuration = (now - room.LastActivity).TotalSeconds;
                        if (emptyDuration >= EMPTY_ROOM_TIMEOUT_SECONDS)
                        {
                            roomsToDelete.Add(kvp.Key);
                        }
                    }
                }

                // Xóa các room đã timeout
                foreach (var roomCode in roomsToDelete)
                {
                    if (activeRooms.TryRemove(roomCode, out _))
                    {
                        dbService.DeleteRoom(roomCode);
                        Log($"🗑️ Auto-deleted empty room {roomCode} (timeout {EMPTY_ROOM_TIMEOUT_SECONDS}s)");
                    }
                }

                // Broadcast nếu có room bị xóa
                if (roomsToDelete.Count > 0)
                {
                    RoomListBroadcaster?.BroadcastRoomList();
                }
            }
            catch (Exception ex)
            {
                Log($"❌ CleanupEmptyRooms error: {ex.Message}");
            }
        }

        #endregion

        #region Load rooms from database

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
                            Log($" ✅ Loaded room: {dbRoom.RoomCode} ({dbRoom.RoomName})");
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

        #endregion

        #region Room creation

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

                // Gọi CreateRoomEmpty 
                var dbResult = dbService.CreateRoomEmpty(roomCode, roomName, password);
                if (!dbResult.Success)
                {
                    return (false, dbResult.Message, null);
                }

                // Tạo room TRỐNG trong memory
                var room = new GameRoom
                {
                    RoomCode = roomCode,
                    RoomName = roomName,
                    Password = password,
                    Status = "waiting",
                    Player1Username = null,
                    Player1Client = null,
                    Player2Username = null,
                    Player2Client = null,
                    CreatedAt = DateTime.Now,
                    LastActivity = DateTime.Now
                };

                if (activeRooms.TryAdd(roomCode, room))
                {
                    Log($"✅ Room created: {roomCode} ({roomName}) by {creatorUsername} [EMPTY - needs manual join]");

                    // Broadcast room list update
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

        private string GenerateUniqueRoomCode()
        {
            // Thử sinh tối đa 100 lần để tránh trùng mã phòng
            for (int i = 0; i < 100; i++)
            {
                string code = random.Next(100000, 999999).ToString();
                if (!activeRooms.ContainsKey(code) && !dbService.RoomCodeExists(code))
                {
                    return code;
                }
            }

            return null;
        }

        #endregion

        #region Join room

        public (bool Success, string Message, GameRoom Room) JoinRoom(
            string roomCode,
            string password,
            string username,
            ClientHandler client)
        {
            try
            {
                // Kiểm tra room tồn tại trong memory
                if (!activeRooms.TryGetValue(roomCode, out GameRoom room))
                {
                    // Thử load từ database nếu chưa có trong memory
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
                        Status = dbRoom.Status?.ToLower() ?? "waiting",
                        Player1Username = dbRoom.Player1Username,
                        Player2Username = dbRoom.Player2Username,
                        CreatedAt = dbRoom.CreatedAt,
                        LastActivity = DateTime.Now
                    };

                    activeRooms.TryAdd(roomCode, room);
                }

                if (!string.IsNullOrEmpty(room.Password) && room.Password != password)
                {
                    return (false, "Incorrect password", null);
                }

                if (room.Player1Username == username || room.Player2Username == username)
                {
                    return (false, "You are already in this room", null);
                }

                if (!string.IsNullOrEmpty(room.Player1Username) && !string.IsNullOrEmpty(room.Player2Username))
                {
                    return (false, "Room is full", null);
                }

                // Join vào database
                var dbResult = dbService.JoinRoom(roomCode, password, username);
                if (!dbResult.Success)
                {
                    return (false, dbResult.Message, null);
                }

                // Update memory
                if (string.IsNullOrEmpty(room.Player1Username))
                {
                    room.Player1Username = username;
                    room.Player1Client = client;
                    Log($"✅ {username} joined room {roomCode} as Player 1");
                }
                else
                {
                    room.Player2Username = username;
                    room.Player2Client = client;
                    room.Status = "ready";
                    Log($"✅ {username} joined room {roomCode} as Player 2");
                }

                room.LastActivity = DateTime.Now;

                // Broadcast cho player còn lại
                BroadcastToRoom(roomCode, new
                {
                    Action = "PLAYER_JOINED",
                    Data = new { username = username }
                });

                // Broadcast room list 
                RoomListBroadcaster?.BroadcastRoomList();

                return (true, "Joined room successfully", room);
            }
            catch (Exception ex)
            {
                Log($"❌ JoinRoom error: {ex.Message}");
                return (false, $"Error: {ex.Message}", null);
            }
        }

        #endregion

        #region Leave room
        public void LeaveRoom(string roomCode, string username)
        {
            try
            {
                Log($"📤 LeaveRoom called: {username} from {roomCode}");

                // Cập nhật database 
                dbService.LeaveRoom(roomCode, username);

                if (!activeRooms.TryGetValue(roomCode, out GameRoom room))
                {
                    Log($"⚠️ Room {roomCode} not found in memory");
                    return;
                }

                bool wasPlayer1 = room.Player1Username == username;
                bool wasPlayer2 = room.Player2Username == username;

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

                room.LastActivity = DateTime.Now;
                room.Status = "waiting";

                // Nếu phòng trống, KHÔNG xóa ngay, để timer cleanup sau 30s
                if (string.IsNullOrEmpty(room.Player1Username) &&
                    string.IsNullOrEmpty(room.Player2Username))
                {
                    Log($"⏳ Room {roomCode} is now empty, will be deleted in {EMPTY_ROOM_TIMEOUT_SECONDS}s if no one joins");
                }
                else
                {
                    // Thông báo cho người còn lại trong room
                    BroadcastToRoom(roomCode, new
                    {
                        Action = "PLAYER_LEFT",
                        Data = new { username = username }
                    });
                }

                // Broadcast room list (room có slot trống)
                RoomListBroadcaster?.BroadcastRoomList();
            }
            catch (Exception ex)
            {
                Log($"❌ LeaveRoom error: {ex.Message}");
            }
        }

        #endregion

        #region Room listing
        public List<RoomInfo> GetAvailableRooms()
        {
            try
            {
                var rooms = new List<RoomInfo>();

                foreach (var kvp in activeRooms)
                {
                    var room = kvp.Value;

                    // Hiển thị cả room trống (0 player) và room có 1 người
                    if (room.Status?.ToLower() == "waiting" || room.Status?.ToLower() == "ready")
                    {
                        int playerCount = 0;
                        if (!string.IsNullOrEmpty(room.Player1Username)) playerCount++;
                        if (!string.IsNullOrEmpty(room.Player2Username)) playerCount++;

                        // Hiển thị phòng chưa đầy (0 hoặc 1 người)
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

                Log($"📋 GetAvailableRooms: Found {rooms.Count} rooms (Total in cache: {activeRooms.Count})");
                return rooms;
            }
            catch (Exception ex)
            {
                Log($"❌ GetAvailableRooms error: {ex.Message}");
                return new List<RoomInfo>();
            }
        }

        #endregion

        #region Gameplay control
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

            // Cập nhật database trạng thái phòng
            dbService.UpdateRoomStatus(roomCode, "PLAYING");

            // Khởi tạo game state ban đầu
            room.GameState = new GameState
            {
                Player1Health = 100,
                Player2Health = 100,
                Player1Stamina = 100,
                Player2Stamina = 100,
                Player1Mana = 0,
                Player2Mana = 0,
                Player1X = 100,
                Player1Y = 300,
                Player2X = 500,
                Player2Y = 300,
                CurrentRound = 1,
                LastUpdate = DateTime.Now
            };

            Log($"🎮 Game started in room {roomCode}");
            return true;
        }

        public void UpdateGameState(string roomCode, string username, GameAction action)
        {
            if (!activeRooms.TryGetValue(roomCode, out GameRoom room))
                return;

            if (room.GameState == null)
                return;

            // Update position và action cho đúng player
            if (username == room.Player1Username)
            {
                room.GameState.Player1X = action.X;
                room.GameState.Player1Y = action.Y;
                room.GameState.Player1Action = action.ActionName;
            }
            else if (username == room.Player2Username)
            {
                room.GameState.Player2X = action.X;
                room.GameState.Player2Y = action.Y;
                room.GameState.Player2Action = action.ActionName;
            }

            room.GameState.LastUpdate = DateTime.Now;

            // Broadcast game state cho cả 2 client
            BroadcastGameState(roomCode);
        }

        private void BroadcastGameState(string roomCode)
        {
            if (!activeRooms.TryGetValue(roomCode, out GameRoom room))
                return;

            var broadcast = new
            {
                Action = "GAME_STATE_UPDATE",
                Data = room.GameState
            };

            string json = System.Text.Json.JsonSerializer.Serialize(broadcast);
            room.Player1Client?.SendMessage(json);
            room.Player2Client?.SendMessage(json);
        }

        #endregion

        #region Broadcast helpers
        public void BroadcastToRoom(string roomCode, object message)
        {
            if (!activeRooms.TryGetValue(roomCode, out GameRoom room))
                return;

            string json = System.Text.Json.JsonSerializer.Serialize(message);
            room.Player1Client?.SendMessage(json);
            room.Player2Client?.SendMessage(json);
        }

        public GameRoom GetRoom(string roomCode)
        {
            activeRooms.TryGetValue(roomCode, out GameRoom room);
            return room;
        }

        #endregion

        #region Logging

        private void Log(string message)
        {
            OnLog?.Invoke(message);
        }

        #endregion
    }
    #region Data classes
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

    #endregion
}
