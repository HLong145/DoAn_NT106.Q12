using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DoAn_NT106.Server
{
    /// <summary>
    /// Quản lý Lobby cho mỗi Room - Real-time sync giữa 2 players
    /// </summary>
    public class LobbyManager
    {
        #region Fields and constructor

        // Room Code -> Lobby Data
        private ConcurrentDictionary<string, LobbyData> lobbies = new ConcurrentDictionary<string, LobbyData>();

        // Room Code -> Character selection state (P1/P2 picks)
        private ConcurrentDictionary<string, CharacterSelectState> characterSelectByRoom =
            new ConcurrentDictionary<string, CharacterSelectState>();

        // Reference đến RoomManager để gọi LeaveRoom
        private RoomManager roomManager;

        private DatabaseService dbService;

        public event Action<string> OnLog;

        private void Log(string message) => OnLog?.Invoke($"[Lobby] {message}");

        public LobbyManager(DatabaseService dbService = null)
        {
            // Khởi tạo DatabaseService dùng để lưu chat lobby
            this.dbService = dbService ?? new DatabaseService();
        }

        // Set RoomManager sau khi khởi tạo
        public void SetRoomManager(RoomManager manager)
        {
            this.roomManager = manager;
            Log("✅ RoomManager reference set");
        }

        #endregion

        // SET MAP
        public (bool Success, string Message) SetLobbyMap(string roomCode, string username, string selectedMap)
        {
            try
            {
                if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(selectedMap))
                    return (false, "Missing parameters");

                if (!lobbies.TryGetValue(roomCode, out var lobby))
                    return (false, "Lobby not found");

                lock (lobby.Lock)
                {
                    // Only host can change map (player1)
                    if (lobby.Player1Username != username)
                    {
                        return (false, "Only host can change map");
                    }

                    lobby.SelectedMap = selectedMap;
                    Log($"🗺 Lobby {roomCode} map changed to {selectedMap} by {username}");

                    // Broadcast map change to both players
                    BroadcastMapChanged(lobby);
                }

                return (true, "Map updated");
            }
            catch (Exception ex)
            {
                Log($"❌ SetLobbyMap error: {ex.Message}");
                return (false, ex.Message);
            }
        }

        private void BroadcastMapChanged(LobbyData lobby)
        {
            var broadcast = new
            {
                Action = "LOBBY_MAP_CHANGED",
                Data = new
                {
                    roomCode = lobby.RoomCode,
                    selectedMap = lobby.SelectedMap
                }
            };

            string json = JsonSerializer.Serialize(broadcast);

            if (lobby.Player1Client != null)
                SafeSend(lobby.Player1Client, json);

            if (lobby.Player2Client != null)
                SafeSend(lobby.Player2Client, json);
        }

        #region Join and leave lobby
        public (bool Success, string Message, LobbyData Lobby) JoinLobby(
            string roomCode,
            string username,
            ClientHandler client,
            RoomManager roomMgr)
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
                    // Lấy thông tin room từ RoomManager
                    var room = roomMgr?.GetRoom(roomCode);
                    if (room != null)
                    {
                        lobby.RoomName = room.RoomName;
                    }

                    // Gán slot cho player
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

                    // Broadcast trạng thái lobby cho 2 bên
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

        // LEAVE LOBBY - Gọi RoomManager.LeaveRoom
        public (bool Success, string Message) LeaveLobby(string roomCode, string username)
        {
            try
            {
                Log($"📤 LeaveLobby called: {username} from {roomCode}");

                if (!lobbies.TryGetValue(roomCode, out var lobby))
                {
                    Log($"⚠️ Lobby not found for {roomCode}, but will still leave room");
                    
                    //Vẫn gọi LeaveRoom dù lobby không tồn tại
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

                    // Broadcast TRƯỚC khi xóa lobby
                    BroadcastPlayerLeft(lobby, username);

                    // Gọi RoomManager.LeaveRoom để xóa username khỏi room
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

        #endregion

        #region Ready and start game
        // SET READY STATUS
        public (bool Success, string Message, bool BothReady) SetReady(
            string roomCode,
            string username,
            bool isReady)
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

                    // Broadcast trạng thái ready tới cả 2 bên
                    BroadcastLobbyState(lobby, excludeUsername: null);

                    // Check if both ready
                    bool bothReady = lobby.Player1Ready && lobby.Player2Ready &&
                                     !string.IsNullOrEmpty(lobby.Player1Username) &&
                                     !string.IsNullOrEmpty(lobby.Player2Username);

                    if (bothReady)
                    {
                        Log($"🚀 Both players ready in lobby {roomCode}!");
                        BroadcastBothReady(lobby);
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
        // START GAME (by host)
        public (bool Success, string Message) StartGame(string roomCode, string username)
        {
            try
            {
                if (!lobbies.TryGetValue(roomCode, out var lobby))
                    return (false, "Lobby not found");

                lock (lobby.Lock)
                {
                    // Chỉ Player 1 (host) mới được start
                    if (lobby.Player1Username != username)
                    {
                        return (false, "Only the host can start the game");
                    }

                    // Kiểm tra cả 2 đã ready
                    bool bothReady = lobby.Player1Ready && lobby.Player2Ready &&
                                     !string.IsNullOrEmpty(lobby.Player1Username) &&
                                     !string.IsNullOrEmpty(lobby.Player2Username);

                    if (!bothReady)
                    {
                        return (false, "Both players must be ready");
                    }

                    Log($"🎮 Host {username} starting game in lobby {roomCode}");

                    // Ensure RoomManager starts the game state and UDP match is created BEFORE broadcasting
                    try
                    {
                        if (roomManager != null)
                        {
                            // Initialize game state in RoomManager first
                            bool started = roomManager.StartGame(roomCode);
                            Log(started ? "✅ RoomManager.StartGame succeeded" : "⚠️ RoomManager.StartGame failed or returned false");

                            // Try to create UDP match via RoomManager's UDPGameServer reference
                            var udp = roomManager.UdpGameServer;
                            if (udp != null)
                            {
                                var p1 = lobby.Player1Username;
                                var p2 = lobby.Player2Username;
                                var res = udp.CreateMatch(roomCode, p1, p2);
                                if (res.Success)
                                {
                                    Log($"✅ UDP Match created for room {roomCode} (via LobbyManager)");
                                }
                                else
                                {
                                    Log($"⚠️ UDP CreateMatch failed for room {roomCode}: {res.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"❌ Error starting room/udp from LobbyManager: {ex.Message}");
                    }

                    // Now broadcast START (clients will receive after UDP match exists)
                    BroadcastStartGame(lobby);

                    return (true, "Game started");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ StartGame error: {ex.Message}");
                return (false, ex.Message);
            }
        }

        #endregion`

        #region Chat and data access

        // SEND CHAT MESSAGE
        public (bool Success, string Message) SendChatMessage(
            string roomCode,
            string username,
            string message)
        {
            try
            {
                if (!lobbies.TryGetValue(roomCode, out var lobby))
                    return (false, "Lobby not found");

                if (string.IsNullOrWhiteSpace(message))
                    return (false, "Message cannot be empty");

                // Tạo message chat mới
                var chatMessage = new LobbyChatMessage
                {
                    Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                    Username = username,
                    Message = message,
                    Timestamp = DateTime.Now
                };

                try
                {
                    // Lưu chat xuống DB  nếu fail chỉ log warning
                    var dbResult = dbService.SaveLobbyChatMessage(roomCode, username, message);
                    if (!dbResult.Success)
                    {
                        Log($"⚠️ Failed to save to DB: {dbResult.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Log($"⚠️ DB save error: {ex.Message}");
                }

                lock (lobby.Lock)
                {
                    // Lưu vào history trong bộ nhớ
                    lobby.ChatHistory.Add(chatMessage);
                    if (lobby.ChatHistory.Count > 50)
                        lobby.ChatHistory.RemoveAt(0);

                    // Broadcast chat message cho cả 2 players
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
        
        
        // GET LOBBY DATA
        public LobbyData GetLobby(string roomCode)
        {
            // Lấy thông tin lobby hiện tại
            lobbies.TryGetValue(roomCode, out var lobby);
            return lobby;
        }

        #endregion

        #region Broadcast helpers

        private void BroadcastLobbyState(LobbyData lobby, string excludeUsername)
        {
            // Tính số lượng player hiện tại trong lobby
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
                    selectedMap = lobby.SelectedMap,
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
                    roomCode = lobby.RoomCode,
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
                    ,
                    selectedMap = lobby.SelectedMap
                }
            };

            string json = JsonSerializer.Serialize(broadcast);

            if (lobby.Player1Client != null)
                SafeSend(lobby.Player1Client, json);

            if (lobby.Player2Client != null)
                SafeSend(lobby.Player2Client, json);
        }

        private void BroadcastBothReady(LobbyData lobby)
        {
            var broadcast = new
            {
                Action = "LOBBY_BOTH_READY",
                Data = new
                {
                    roomCode = lobby.RoomCode,
                    bothReady = true
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

        #endregion

        #region Data classes

        public class LobbyData
        {
            public string RoomCode { get; set; }
            public string RoomName { get; set; }

            // Selected map for this lobby (e.g. "battleground1")
            public string SelectedMap { get; set; } = "battleground1";

            public string Player1Username { get; set; }
            public ClientHandler Player1Client { get; set; }
            public bool Player1Ready { get; set; }

            public string Player2Username { get; set; }
            public ClientHandler Player2Client { get; set; }
            public bool Player2Ready { get; set; }

            // Lịch sử chat trong lobby, giữ với số lượng nhất định
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

        private class CharacterSelectState
        {
            public string Player1Character { get; set; }
            public string Player2Character { get; set; }
            public bool Player1Selected { get; set; }
            public bool Player2Selected { get; set; }
        }

        #endregion

        public void HandleSelectCharacter(string roomCode, string username, string character)
        {
            if (!lobbies.TryGetValue(roomCode, out var lobby))
            {
                Log($"❌ Lobby not found: {roomCode}");
                return;
            }

            lock (lobby.Lock)
            {
                var state = characterSelectByRoom.GetOrAdd(roomCode, _ => new CharacterSelectState());

                if (lobby.Player1Username == username)
                {
                    state.Player1Character = character;
                    state.Player1Selected = true;
                    Log($"✅ Player1 ({username}) selected {character}");
                }
                else if (lobby.Player2Username == username)
                {
                    state.Player2Character = character;
                    state.Player2Selected = true;
                    Log($"✅ Player2 ({username}) selected {character}");
                }
                else
                {
                    Log($"❌ Unknown user {username} in lobby {roomCode}");
                    return; // unknown user in this lobby
                }

                Log($"[Lobby] Character selections - P1: {state.Player1Selected} ({state.Player1Character ?? "null"}), P2: {state.Player2Selected} ({state.Player2Character ?? "null"})");

                // Khi cả 2 đã chọn xong, gửi START_GAME chung với role + character mapping
                if (state.Player1Selected && state.Player2Selected &&
                    !string.IsNullOrEmpty(lobby.Player1Username) &&
                    !string.IsNullOrEmpty(lobby.Player2Username))
                {
                    var payload = new
                    {
                        Action = "START_GAME",
                        Data = new
                        {
                            roomCode = lobby.RoomCode,
                            player1 = lobby.Player1Username,
                            player2 = lobby.Player2Username,
                            player1Number = 1,  //  Explicit player number
                            player2Number = 2,  //  Explicit player number
                            player1Character = state.Player1Character,
                            player2Character = state.Player2Character
                            ,
                            selectedMap = lobby.SelectedMap
                        }
                    };

                    string json = JsonSerializer.Serialize(payload);
                    
                    Log($"📢 Broadcasting START_GAME to Player1: {lobby.Player1Username}");
                    SafeSend(lobby.Player1Client, json);
                    
                    Log($"📢 Broadcasting START_GAME to Player2: {lobby.Player2Username}");
                    SafeSend(lobby.Player2Client, json);

                    Log($"🚀 START_GAME sent for room {roomCode}: P1={lobby.Player1Username} ({state.Player1Character}), P2={lobby.Player2Username} ({state.Player2Character})");
                }
                else
                {
                    Log($"⏳ Waiting for other player: P1_Selected={state.Player1Selected}, P2_Selected={state.Player2Selected}");
                }
            }
        }

        // Reset lobby sau khi game kết thúc (rematch hoặc return to lobby)
        public (bool Success, string Message) ResetLobbyForRematch(string roomCode)
        {
            try
            {
                if (!lobbies.TryGetValue(roomCode, out var lobby))
                    return (false, "Lobby not found");

                lock (lobby.Lock)
                {
                    // Reset ready status
                    lobby.Player1Ready = false;
                    lobby.Player2Ready = false;
                    Log($"✅ Reset ready status for lobby {roomCode}");

                    // Reset character selections (xóa dữ liệu cũ)
                    if (characterSelectByRoom.TryGetValue(roomCode, out var charState))
                    {
                        charState.Player1Character = null;
                        charState.Player2Character = null;
                        charState.Player1Selected = false;
                        charState.Player2Selected = false;
                        Log($"✅ Reset character selections for lobby {roomCode}");
                    }

                    // Broadcast trạng thái mới (cả 2 đều NOT READY)
                    BroadcastLobbyState(lobby, excludeUsername: null);

                    Log($"🔄 Lobby {roomCode} reset to initial state (ready for rematch or return)");
                    return (true, "Lobby reset for rematch");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ ResetLobbyForRematch error: {ex.Message}");
                return (false, ex.Message);
            }
        }
    }
}
