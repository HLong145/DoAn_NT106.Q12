using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DoAn_NT106.Server
{
    /// <summary>
    /// Quản lý Lobby cho mỗi Room - Real-time sync giữa 2 players
    /// </summary>
    public class LobbyManager
    {
        // Room Code -> Lobby Data
        private ConcurrentDictionary<string, LobbyData> lobbies = new ConcurrentDictionary<string, LobbyData>();

        // ✅ THÊM MỚI: Reference đến RoomManager để gọi LeaveRoom
        private RoomManager roomManager;

        public event Action<string> OnLog;

        private void Log(string message) => OnLog?.Invoke($"[Lobby] {message}");

        // ✅ THÊM MỚI: Set RoomManager sau khi khởi tạo
        public void SetRoomManager(RoomManager manager)
        {
            this.roomManager = manager;
            Log("✅ RoomManager reference set");
        }

        // ===========================
        // JOIN LOBBY
        // ===========================
        public (bool Success, string Message, LobbyData Lobby) JoinLobby(
            string roomCode, string username, ClientHandler client, RoomManager roomMgr)
        {
            try
            {
                // Cập nhật reference nếu được truyền vào
                if (roomMgr != null)
                {
                    this.roomManager = roomMgr;
                }

                if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(username))
                    return (false, "Room code and username are required", null);

                // Get or create lobby
                var lobby = lobbies.GetOrAdd(roomCode, code => new LobbyData { RoomCode = code });

                lock (lobby.Lock)
                {
                    // Get room info from RoomManager
                    var room = roomMgr?.GetRoom(roomCode);
                    if (room != null)
                    {
                        lobby.RoomName = room.RoomName;
                    }

                    // Determine player slot
                    if (string.IsNullOrEmpty(lobby.Player1Username))
                    {
                        lobby.Player1Username = username;
                        lobby.Player1Client = client;
                        lobby.Player1Ready = false;
                        Log($"✅ {username} joined lobby {roomCode} as Player 1");
                    }
                    else if (lobby.Player1Username == username)
                    {
                        // Reconnecting as Player 1
                        lobby.Player1Client = client;
                        Log($"🔄 {username} reconnected to lobby {roomCode} as Player 1");
                    }
                    else if (string.IsNullOrEmpty(lobby.Player2Username))
                    {
                        lobby.Player2Username = username;
                        lobby.Player2Client = client;
                        lobby.Player2Ready = false;
                        Log($"✅ {username} joined lobby {roomCode} as Player 2");
                    }
                    else if (lobby.Player2Username == username)
                    {
                        // Reconnecting as Player 2
                        lobby.Player2Client = client;
                        Log($"🔄 {username} reconnected to lobby {roomCode} as Player 2");
                    }
                    else
                    {
                        return (false, "Lobby is full", null);
                    }

                    // Broadcast to other player
                    BroadcastLobbyState(lobby, excludeUsername: null);
                }

                return (true, "Joined lobby", lobby);
            }
            catch (Exception ex)
            {
                Log($"❌ JoinLobby error: {ex.Message}");
                return (false, ex.Message, null);
            }
        }

        // ===========================
        // ✅ SỬA: LEAVE LOBBY - Gọi RoomManager.LeaveRoom
        // ===========================
        public (bool Success, string Message) LeaveLobby(string roomCode, string username)
        {
            try
            {
                Log($"📤 LeaveLobby called: {username} from {roomCode}");

                if (!lobbies.TryGetValue(roomCode, out var lobby))
                {
                    Log($"⚠️ Lobby not found for {roomCode}, but will still leave room");

                    // ✅ QUAN TRỌNG: Vẫn gọi LeaveRoom dù lobby không tồn tại
                    roomManager?.LeaveRoom(roomCode, username);
                    return (true, "Lobby not found but room left");
                }

                lock (lobby.Lock)
                {
                    bool wasPlayer1 = lobby.Player1Username == username;
                    bool wasPlayer2 = lobby.Player2Username == username;

                    if (wasPlayer1)
                    {
                        lobby.Player1Username = null;
                        lobby.Player1Client = null;
                        lobby.Player1Ready = false;
                        Log($"👋 {username} left lobby {roomCode} (was Player 1)");
                    }
                    else if (wasPlayer2)
                    {
                        lobby.Player2Username = null;
                        lobby.Player2Client = null;
                        lobby.Player2Ready = false;
                        Log($"👋 {username} left lobby {roomCode} (was Player 2)");
                    }

                    // ✅ QUAN TRỌNG: Broadcast TRƯỚC khi xóa lobby
                    BroadcastPlayerLeft(lobby, username);

                    // ✅ QUAN TRỌNG: Gọi RoomManager.LeaveRoom để xóa username khỏi room
                    roomManager?.LeaveRoom(roomCode, username);
                    Log($"✅ Called RoomManager.LeaveRoom for {username}");

                    // Remove lobby if empty
                    if (string.IsNullOrEmpty(lobby.Player1Username) &&
                        string.IsNullOrEmpty(lobby.Player2Username))
                    {
                        lobbies.TryRemove(roomCode, out _);
                        Log($"🗑 Lobby {roomCode} removed (empty)");
                    }
                }

                return (true, "Left lobby");
            }
            catch (Exception ex)
            {
                Log($"❌ LeaveLobby error: {ex.Message}");
                return (false, ex.Message);
            }
        }

        // ===========================
        // SET READY STATUS
        // ===========================
        public (bool Success, string Message, bool BothReady) SetReady(string roomCode, string username, bool isReady)
        {
            try
            {
                if (!lobbies.TryGetValue(roomCode, out var lobby))
                    return (false, "Lobby not found", false);

                lock (lobby.Lock)
                {
                    if (lobby.Player1Username == username)
                    {
                        lobby.Player1Ready = isReady;
                        Log($"🎮 {username} is {(isReady ? "READY" : "NOT READY")} in lobby {roomCode}");
                    }
                    else if (lobby.Player2Username == username)
                    {
                        lobby.Player2Ready = isReady;
                        Log($"🎮 {username} is {(isReady ? "READY" : "NOT READY")} in lobby {roomCode}");
                    }
                    else
                    {
                        return (false, "Player not in lobby", false);
                    }

                    // Broadcast updated state
                    BroadcastLobbyState(lobby, excludeUsername: null);

                    // Check if both ready
                    bool bothReady = lobby.Player1Ready && lobby.Player2Ready &&
                                    !string.IsNullOrEmpty(lobby.Player1Username) &&
                                    !string.IsNullOrEmpty(lobby.Player2Username);

                    if (bothReady)
                    {
                        Log($"🚀 Both players ready in lobby {roomCode}!");
                        BroadcastStartGame(lobby);
                    }

                    return (true, "Ready status updated", bothReady);
                }
            }
            catch (Exception ex)
            {
                Log($"❌ SetReady error: {ex.Message}");
                return (false, ex.Message, false);
            }
        }

        // ===========================
        // SEND CHAT MESSAGE
        // ===========================
        public (bool Success, string Message) SendChatMessage(string roomCode, string username, string message)
        {
            try
            {
                if (!lobbies.TryGetValue(roomCode, out var lobby))
                    return (false, "Lobby not found");

                if (string.IsNullOrWhiteSpace(message))
                    return (false, "Message cannot be empty");

                var chatMessage = new LobbyChatMessage
                {
                    Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                    Username = username,
                    Message = message,
                    Timestamp = DateTime.Now
                };

                lock (lobby.Lock)
                {
                    // Store in history
                    lobby.ChatHistory.Add(chatMessage);
                    if (lobby.ChatHistory.Count > 50)
                        lobby.ChatHistory.RemoveAt(0);

                    // Broadcast to both players
                    BroadcastChatMessage(lobby, chatMessage);
                }

                Log($"💬 [{roomCode}] {username}: {message.Substring(0, Math.Min(50, message.Length))}...");
                return (true, "Message sent");
            }
            catch (Exception ex)
            {
                Log($"❌ SendChatMessage error: {ex.Message}");
                return (false, ex.Message);
            }
        }

        // ===========================
        // GET LOBBY DATA
        // ===========================
        public LobbyData GetLobby(string roomCode)
        {
            lobbies.TryGetValue(roomCode, out var lobby);
            return lobby;
        }

        // ===========================
        // BROADCAST METHODS
        // ===========================
        private void BroadcastLobbyState(LobbyData lobby, string excludeUsername)
        {
            // Tính player count
            int playerCount = 0;
            if (!string.IsNullOrEmpty(lobby.Player1Username)) playerCount++;
            if (!string.IsNullOrEmpty(lobby.Player2Username)) playerCount++;

            var broadcast = new
            {
                Action = "LOBBY_STATE_UPDATE",
                Data = new
                {
                    roomCode = lobby.RoomCode,
                    roomName = lobby.RoomName,
                    player1 = lobby.Player1Username,
                    player2 = lobby.Player2Username,
                    player1Ready = lobby.Player1Ready,
                    player2Ready = lobby.Player2Ready,
                    playerCount = playerCount
                }
            };

            string json = JsonSerializer.Serialize(broadcast);

            if (lobby.Player1Client != null && lobby.Player1Username != excludeUsername)
                SafeSend(lobby.Player1Client, json);

            if (lobby.Player2Client != null && lobby.Player2Username != excludeUsername)
                SafeSend(lobby.Player2Client, json);
        }

        private void BroadcastPlayerLeft(LobbyData lobby, string leftUsername)
        {
            var broadcast = new
            {
                Action = "LOBBY_PLAYER_LEFT",
                Data = new
                {
                    roomCode = lobby.RoomCode,
                    username = leftUsername,
                    player1 = lobby.Player1Username,
                    player2 = lobby.Player2Username
                }
            };

            string json = JsonSerializer.Serialize(broadcast);
            Log($"📢 Broadcasting LOBBY_PLAYER_LEFT: {leftUsername}");

            // Gửi cho TẤT CẢ client trong lobby
            if (lobby.Player1Client != null)
                SafeSend(lobby.Player1Client, json);

            if (lobby.Player2Client != null)
                SafeSend(lobby.Player2Client, json);
        }

        private void BroadcastChatMessage(LobbyData lobby, LobbyChatMessage chatMessage)
        {
            var broadcast = new
            {
                Action = "LOBBY_CHAT_MESSAGE",
                Data = new
                {
                    id = chatMessage.Id,
                    username = chatMessage.Username,
                    message = chatMessage.Message,
                    timestamp = chatMessage.Timestamp.ToString("HH:mm:ss")
                }
            };

            string json = JsonSerializer.Serialize(broadcast);

            if (lobby.Player1Client != null)
                SafeSend(lobby.Player1Client, json);

            if (lobby.Player2Client != null)
                SafeSend(lobby.Player2Client, json);
        }

        private void BroadcastStartGame(LobbyData lobby)
        {
            var broadcast = new
            {
                Action = "LOBBY_START_GAME",
                Data = new
                {
                    roomCode = lobby.RoomCode,
                    player1 = lobby.Player1Username,
                    player2 = lobby.Player2Username
                }
            };

            string json = JsonSerializer.Serialize(broadcast);

            if (lobby.Player1Client != null)
                SafeSend(lobby.Player1Client, json);

            if (lobby.Player2Client != null)
                SafeSend(lobby.Player2Client, json);
        }

        private void SafeSend(ClientHandler client, string json)
        {
            try
            {
                client?.SendMessage(json);
            }
            catch (Exception ex)
            {
                Log($"⚠️ SafeSend error: {ex.Message}");
            }
        }
    }

    // ===========================
    // DATA CLASSES
    // ===========================
    public class LobbyData
    {
        public string RoomCode { get; set; }
        public string RoomName { get; set; }

        public string Player1Username { get; set; }
        public ClientHandler Player1Client { get; set; }
        public bool Player1Ready { get; set; }

        public string Player2Username { get; set; }
        public ClientHandler Player2Client { get; set; }
        public bool Player2Ready { get; set; }

        public List<LobbyChatMessage> ChatHistory { get; } = new List<LobbyChatMessage>();
        public object Lock { get; } = new object();
    }

    public class LobbyChatMessage
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}