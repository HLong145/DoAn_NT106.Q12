using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DoAn_NT106.Server
{
    /// <summary>
    /// Quản lý real-time connections trong mỗi room lobby
    /// </summary>
    public class LobbyManager
    {
        // Dictionary: roomCode -> LobbyRoom
        private ConcurrentDictionary<string, LobbyRoom> lobbies = new ConcurrentDictionary<string, LobbyRoom>();

        public event Action<string> OnLog;

        // ===========================
        // JOIN LOBBY
        // ===========================
        public (bool Success, string Message, LobbyRoom Lobby) JoinLobby(
            string roomCode, string username, ClientHandler client, RoomManager roomManager)
        {
            try
            {
                // Lấy room info từ RoomManager
                var room = roomManager.GetRoom(roomCode);
                if (room == null)
                {
                    return (false, "Room not found", null);
                }

                // Tạo hoặc lấy lobby
                var lobby = lobbies.GetOrAdd(roomCode, _ => new LobbyRoom
                {
                    RoomCode = roomCode,
                    RoomName = room.RoomName
                });

                // Xác định player slot
                bool isPlayer1 = room.Player1Username == username;
                bool isPlayer2 = room.Player2Username == username;

                if (!isPlayer1 && !isPlayer2)
                {
                    return (false, "You are not in this room", null);
                }

                // Thêm connection
                if (isPlayer1)
                {
                    lobby.Player1Username = username;
                    lobby.Player1Client = client;
                }
                else
                {
                    lobby.Player2Username = username;
                    lobby.Player2Client = client;
                }

                Log($"✅ {username} joined lobby {roomCode}");

                // Broadcast cho người còn lại
                BroadcastToLobby(roomCode, new
                {
                    Action = "LOBBY_PLAYER_JOINED",
                    Data = new
                    {
                        username = username,
                        isPlayer1 = isPlayer1
                    }
                }, excludeUser: username);

                // Gửi system message
                AddChatMessage(roomCode, "System", $"{username} đã vào phòng!", "system");

                return (true, "Joined lobby", lobby);
            }
            catch (Exception ex)
            {
                Log($"❌ JoinLobby error: {ex.Message}");
                return (false, ex.Message, null);
            }
        }

        // ===========================
        // LEAVE LOBBY
        // ===========================
        public void LeaveLobby(string roomCode, string username)
        {
            try
            {
                if (!lobbies.TryGetValue(roomCode, out var lobby))
                    return;

                bool wasPlayer1 = lobby.Player1Username == username;

                if (wasPlayer1)
                {
                    lobby.Player1Username = null;
                    lobby.Player1Client = null;
                    lobby.Player1Ready = false;
                }
                else if (lobby.Player2Username == username)
                {
                    lobby.Player2Username = null;
                    lobby.Player2Client = null;
                    lobby.Player2Ready = false;
                }

                Log($"👋 {username} left lobby {roomCode}");

                // Broadcast cho người còn lại
                BroadcastToLobby(roomCode, new
                {
                    Action = "LOBBY_PLAYER_LEFT",
                    Data = new { username = username }
                });

                AddChatMessage(roomCode, "System", $"{username} đã rời phòng!", "system");

                // Nếu lobby trống, xóa
                if (lobby.Player1Client == null && lobby.Player2Client == null)
                {
                    lobbies.TryRemove(roomCode, out _);
                    Log($"🗑️ Lobby {roomCode} removed (empty)");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ LeaveLobby error: {ex.Message}");
            }
        }

        // ===========================
        // SET READY STATUS
        // ===========================
        public (bool Success, bool AllReady) SetReady(string roomCode, string username, bool isReady)
        {
            try
            {
                if (!lobbies.TryGetValue(roomCode, out var lobby))
                    return (false, false);

                bool isPlayer1 = lobby.Player1Username == username;

                if (isPlayer1)
                    lobby.Player1Ready = isReady;
                else if (lobby.Player2Username == username)
                    lobby.Player2Ready = isReady;
                else
                    return (false, false);

                Log($"🎮 {username} is {(isReady ? "READY" : "NOT READY")} in room {roomCode}");

                // Broadcast ready status
                BroadcastToLobby(roomCode, new
                {
                    Action = "LOBBY_READY_CHANGED",
                    Data = new
                    {
                        username = username,
                        isReady = isReady
                    }
                });

                // Kiểm tra cả 2 đã ready chưa
                bool allReady = !string.IsNullOrEmpty(lobby.Player1Username) &&
                               !string.IsNullOrEmpty(lobby.Player2Username) &&
                               lobby.Player1Ready && lobby.Player2Ready;

                if (allReady)
                {
                    Log($"🚀 All players ready in room {roomCode}! Starting game...");

                    // Broadcast ALL_READY
                    BroadcastToLobby(roomCode, new
                    {
                        Action = "LOBBY_ALL_READY",
                        Data = new
                        {
                            roomCode = roomCode,
                            player1 = lobby.Player1Username,
                            player2 = lobby.Player2Username
                        }
                    });
                }

                return (true, allReady);
            }
            catch (Exception ex)
            {
                Log($"❌ SetReady error: {ex.Message}");
                return (false, false);
            }
        }

        // ===========================
        // CHAT
        // ===========================
        public bool SendChat(string roomCode, string username, string message)
        {
            try
            {
                if (!lobbies.TryGetValue(roomCode, out var lobby))
                    return false;

                AddChatMessage(roomCode, username, message, "user");
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ SendChat error: {ex.Message}");
                return false;
            }
        }

        private void AddChatMessage(string roomCode, string username, string message, string type)
        {
            if (!lobbies.TryGetValue(roomCode, out var lobby))
                return;

            var chatMsg = new LobbyChatMsg
            {
                Username = username,
                Message = message,
                Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                Type = type
            };

            // Lưu vào history (giới hạn 50)
            lobby.ChatHistory.Add(chatMsg);
            if (lobby.ChatHistory.Count > 50)
                lobby.ChatHistory.RemoveAt(0);

            // Broadcast
            BroadcastToLobby(roomCode, new
            {
                Action = "LOBBY_CHAT",
                Data = new
                {
                    username = chatMsg.Username,
                    message = chatMsg.Message,
                    timestamp = chatMsg.Timestamp,
                    type = chatMsg.Type
                }
            });

            if (type == "user")
                Log($"💬 [Lobby {roomCode}] {username}: {message.Substring(0, Math.Min(30, message.Length))}...");
        }

        // ===========================
        // GET LOBBY STATE
        // ===========================
        public LobbyRoom GetLobby(string roomCode)
        {
            lobbies.TryGetValue(roomCode, out var lobby);
            return lobby;
        }

        // ===========================
        // BROADCAST
        // ===========================
        private void BroadcastToLobby(string roomCode, object message, string excludeUser = null)
        {
            if (!lobbies.TryGetValue(roomCode, out var lobby))
                return;

            string json = JsonSerializer.Serialize(message);

            if (lobby.Player1Client != null && lobby.Player1Username != excludeUser)
            {
                try { lobby.Player1Client.SendMessage(json); } catch { }
            }

            if (lobby.Player2Client != null && lobby.Player2Username != excludeUser)
            {
                try { lobby.Player2Client.SendMessage(json); } catch { }
            }
        }

        // ===========================
        // CLEANUP
        // ===========================
        public void RemoveDisconnectedClient(string roomCode, string username)
        {
            LeaveLobby(roomCode, username);
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"[Lobby] {message}");
        }
    }

    // ===========================
    // DATA CLASSES
    // ===========================
    public class LobbyRoom
    {
        public string RoomCode { get; set; }
        public string RoomName { get; set; }

        public string Player1Username { get; set; }
        public ClientHandler Player1Client { get; set; }
        public bool Player1Ready { get; set; }

        public string Player2Username { get; set; }
        public ClientHandler Player2Client { get; set; }
        public bool Player2Ready { get; set; }

        public List<LobbyChatMsg> ChatHistory { get; set; } = new List<LobbyChatMsg>();
    }

    public class LobbyChatMsg
    {
        public string Username { get; set; }
        public string Message { get; set; }
        public string Timestamp { get; set; }
        public string Type { get; set; }
    }
}