using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DoAn_NT106.Server
{
    public class RoomManager
    {
        private ConcurrentDictionary<string, GameRoom> rooms = new ConcurrentDictionary<string, GameRoom>();
        private Random random = new Random();

        public event Action<string> OnLog;

        // ===========================
        // TẠO PHÒNG MỚI
        // ===========================
        public (bool Success, string Message, string RoomCode) CreateRoom(
            string roomName,
            string password,
            string creatorUsername,
            ClientHandler creatorClient)
        {
            try
            {
                string roomCode = GenerateRoomCode();

                var room = new GameRoom
                {
                    RoomCode = roomCode,
                    RoomName = roomName,
                    Password = password,
                    Status = "waiting",
                    Player1Username = creatorUsername,
                    Player1Client = creatorClient,
                    CreatedAt = DateTime.Now
                };

                if (rooms.TryAdd(roomCode, room))
                {
                    Log($"✅ Room created: {roomCode} by {creatorUsername}");
                    return (true, "Room created successfully", roomCode);
                }

                return (false, "Failed to create room", null);
            }
            catch (Exception ex)
            {
                Log($"❌ CreateRoom error: {ex.Message}");
                return (false, $"Error: {ex.Message}", null);
            }
        }

        // ===========================
        // THAM GIA PHÒNG
        // ===========================
        public (bool Success, string Message, GameRoom Room) JoinRoom(
            string roomCode,
            string password,
            string username,
            ClientHandler client)
        {
            try
            {
                if (!rooms.TryGetValue(roomCode, out GameRoom room))
                {
                    return (false, "Room not found", null);
                }

                // Kiểm tra mật khẩu
                if (!string.IsNullOrEmpty(room.Password) && room.Password != password)
                {
                    return (false, "Incorrect password", null);
                }

                // Kiểm tra phòng đã đầy chưa
                if (!string.IsNullOrEmpty(room.Player2Username))
                {
                    return (false, "Room is full", null);
                }

                // Kiểm tra trùng tên
                if (room.Player1Username == username)
                {
                    return (false, "Username already in room", null);
                }

                // Thêm player 2
                room.Player2Username = username;
                room.Player2Client = client;
                room.Status = "ready"; // Đủ 2 người

                Log($"✅ {username} joined room {roomCode}");

                // Broadcast thông báo có người tham gia
                BroadcastToRoom(roomCode, new
                {
                    Action = "PLAYER_JOINED",
                    Data = new
                    {
                        username = username,
                        roomCode = roomCode,
                        player1 = room.Player1Username,
                        player2 = room.Player2Username
                    }
                });

                return (true, "Joined successfully", room);
            }
            catch (Exception ex)
            {
                Log($"❌ JoinRoom error: {ex.Message}");
                return (false, $"Error: {ex.Message}", null);
            }
        }

        // ===========================
        // LẤY DANH SÁCH PHÒNG
        // ===========================
        public List<RoomInfo> GetAvailableRooms()
        {
            return rooms.Values
                .Where(r => r.Status == "waiting") // Chỉ lấy phòng đang chờ
                .Select(r => new RoomInfo
                {
                    RoomCode = r.RoomCode,
                    RoomName = r.RoomName,
                    HasPassword = !string.IsNullOrEmpty(r.Password),
                    PlayerCount = string.IsNullOrEmpty(r.Player2Username) ? 1 : 2,
                    Status = r.Status
                })
                .ToList();
        }

        // ===========================
        // BẮT ĐẦU GAME
        // ===========================
        public bool StartGame(string roomCode)
        {
            if (!rooms.TryGetValue(roomCode, out GameRoom room))
                return false;

            if (string.IsNullOrEmpty(room.Player2Username))
            {
                Log($"⚠️ Cannot start game: room {roomCode} needs 2 players");
                return false;
            }

            room.Status = "playing";

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
            if (!rooms.TryGetValue(roomCode, out GameRoom room))
                return;

            if (room.Status != "playing")
                return;

            var state = room.GameState;
            bool isPlayer1 = room.Player1Username == username;

            // Cập nhật vị trí
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
            // Cập nhật hành động (attack, jump, skill...)
            else if (action.Type == "ACTION")
            {
                if (isPlayer1)
                    state.Player1Action = action.ActionName;
                else
                    state.Player2Action = action.ActionName;
            }

            state.LastUpdate = DateTime.Now;

            // Broadcast state mới đến CẢ 2 client
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
        // RỜI PHÒNG
        // ===========================
        public void LeaveRoom(string roomCode, string username)
        {
            if (!rooms.TryGetValue(roomCode, out GameRoom room))
                return;

            bool wasPlayer1 = room.Player1Username == username;

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

            // Nếu phòng trống -> xóa
            if (string.IsNullOrEmpty(room.Player1Username) &&
                string.IsNullOrEmpty(room.Player2Username))
            {
                rooms.TryRemove(roomCode, out _);
                Log($"🗑️ Room {roomCode} removed (empty)");
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

        // ===========================
        // BROADCAST TỚI PHÒNG
        // ===========================
        public void BroadcastToRoom(string roomCode, object message)
        {
            if (!rooms.TryGetValue(roomCode, out GameRoom room))
                return;

            string json = System.Text.Json.JsonSerializer.Serialize(message);

            // Gửi cho player 1
            if (room.Player1Client != null)
            {
                room.Player1Client.SendMessage(json);
            }

            // Gửi cho player 2
            if (room.Player2Client != null)
            {
                room.Player2Client.SendMessage(json);
            }
        }

        // ===========================
        // HELPER
        // ===========================
        private string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void Log(string message)
        {
            OnLog?.Invoke(message);
        }

        public GameRoom GetRoom(string roomCode)
        {
            rooms.TryGetValue(roomCode, out GameRoom room);
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
        public string Status { get; set; } // "waiting", "ready", "playing", "finished"

        public string Player1Username { get; set; }
        public ClientHandler Player1Client { get; set; }
        public string Player1Character { get; set; }

        public string Player2Username { get; set; }
        public ClientHandler Player2Client { get; set; }
        public string Player2Character { get; set; }

        public GameState GameState { get; set; }
        public DateTime CreatedAt { get; set; }
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
        public string Type { get; set; } // "MOVE", "ACTION"
        public int X { get; set; }
        public int Y { get; set; }
        public string ActionName { get; set; } // "punch", "kick", "jump"
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