using DoAn_NT106.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DoAn_NT106.Server
{
    public class GlobalChatManager
    {
        private ConcurrentDictionary<string, ClientHandler> onlineUsers = new ConcurrentDictionary<string, ClientHandler>();
        private List<GlobalChatMessage> chatHistory = new List<GlobalChatMessage>();
        private readonly object historyLock = new object();
        private DatabaseService dbService;
        private const int MAX_HISTORY = 100;

        public event Action<string> OnLog;

        public GlobalChatManager(DatabaseService dbService = null)
        {
            this.dbService = dbService ?? new DatabaseService();
        }

        // ===========================
        // QUẢN LÝ USER ONLINE
        // ===========================

        public (bool Success, string Message, int OnlineCount) JoinGlobalChat(string username, ClientHandler client)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                    return (false, "Username is required", 0);

                onlineUsers.AddOrUpdate(username, client, (key, oldClient) => client);

                int onlineCount = onlineUsers.Count;
                Log($"✅ {username} joined Global Chat. Online: {onlineCount}");

                // ✅ Broadcast thông báo user join
                BroadcastSystemMessage($"{username} joined chat", excludeUser: username);

                return (true, "Joined Global Chat", onlineCount);
            }
            catch (Exception ex)
            {
                Log($"❌ JoinGlobalChat error: {ex.Message}");
                return (false, ex.Message, 0);
            }
        }

        public (bool Success, int OnlineCount) LeaveGlobalChat(string username)
        {
            try
            {
                if (onlineUsers.TryRemove(username, out _))
                {
                    int onlineCount = onlineUsers.Count;
                    Log($"👋 {username} left Global Chat. Online: {onlineCount}");

                    // Broadcast thông báo user leave
                    BroadcastSystemMessage($"{username} left chat", excludeUser: null);

                    return (true, onlineCount);
                }
                return (false, onlineUsers.Count);
            }
            catch (Exception ex)
            {
                Log($"❌ LeaveGlobalChat error: {ex.Message}");
                return (false, onlineUsers.Count);
            }
        }

        public int GetOnlineCount() => onlineUsers.Count;
        public List<string> GetOnlineUsers() => onlineUsers.Keys.ToList();

        // ===========================
        // XỬ LÝ CHAT MESSAGE
        // ===========================

        public (bool Success, string Message) SendChatMessage(string username, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                    return (false, "Username is required");

                if (string.IsNullOrEmpty(message))
                    return (false, "Message is required");

                if (message.Length > 1000)
                    message = message.Substring(0, 1000);

                if (!onlineUsers.ContainsKey(username))
                {
                    Log($"⚠️ {username} not in online users, cannot send message");
                    return (false, "User not in Global Chat");
                }

                try
                {
                    var dbResult = dbService.SaveGlobalChatMessage(username, message);
                    if (!dbResult.Success)
                    {
                        Log($"⚠️ Failed to save to DB: {dbResult.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Log($"⚠️ DB save error: {ex.Message}");
                }

                var chatMessage = new GlobalChatMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = username,
                    Message = message,
                    Timestamp = DateTime.Now,
                    Type = "user"
                };

                AddToHistory(chatMessage);
                BroadcastChatMessage(chatMessage);

                Log($"💬 [{username}]: {message.Substring(0, Math.Min(50, message.Length))}...");

                return (true, "Message sent");
            }
            catch (Exception ex)
            {
                Log($"❌ SendChatMessage error: {ex.Message}");
                return (false, ex.Message);
            }
        }

        // ===========================
        // BROADCAST METHODS
        // ===========================

        private void BroadcastSystemMessage(string message, string excludeUser = null)
        {
            var chatMessage = new GlobalChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                Username = "System",
                Message = message,
                Timestamp = DateTime.Now,
                Type = "system"
            };

            AddToHistory(chatMessage);
            BroadcastChatMessage(chatMessage, excludeUser);
        }

        private void BroadcastChatMessage(GlobalChatMessage chatMessage, string excludeUser = null)
        {
            int onlineCount = onlineUsers.Count;

            var broadcast = new
            {
                Action = "GLOBAL_CHAT_MESSAGE",
                Data = new
                {
                    id = chatMessage.Id,
                    username = chatMessage.Username,
                    message = chatMessage.Message,
                    timestamp = chatMessage.Timestamp.ToString("HH:mm:ss"),
                    type = chatMessage.Type,
                    onlineCount = onlineCount  // ✅ Luôn kèm online count
                }
            };

            string json = JsonSerializer.Serialize(broadcast);
            Log($"📤 Broadcasting chat to {onlineUsers.Count} users: {json.Substring(0, Math.Min(100, json.Length))}...");

            int sentCount = 0;
            foreach (var kvp in onlineUsers.ToArray())
            {
                if (excludeUser != null && kvp.Key == excludeUser)
                    continue;

                try
                {
                    kvp.Value.SendMessage(json);
                    sentCount++;
                }
                catch (Exception ex)
                {
                    Log($"⚠️ Failed to send to {kvp.Key}: {ex.Message}");
                    // Remove disconnected user
                    onlineUsers.TryRemove(kvp.Key, out _);
                }
            }

            Log($"📤 Sent chat message to {sentCount}/{onlineUsers.Count} users");
        }

        public void BroadcastOnlineCount()
        {
            int count = onlineUsers.Count;

            var broadcast = new
            {
                Action = "GLOBAL_CHAT_ONLINE_UPDATE",
                Data = new
                {
                    onlineCount = count
                }
            };

            string json = JsonSerializer.Serialize(broadcast);
            Log($"📤 Broadcasting online count ({count}) to {onlineUsers.Count} users");

            int sentCount = 0;
            foreach (var kvp in onlineUsers.ToArray())
            {
                try
                {
                    kvp.Value.SendMessage(json);
                    sentCount++;
                }
                catch (Exception ex)
                {
                    Log($"⚠️ Failed to send online count to {kvp.Key}: {ex.Message}");
                }
            }

            Log($"📤 Sent online count to {sentCount} users");
        }

        // ===========================
        // CHAT HISTORY
        // ===========================

        private void AddToHistory(GlobalChatMessage message)
        {
            lock (historyLock)
            {
                chatHistory.Add(message);
                if (chatHistory.Count > MAX_HISTORY)
                {
                    chatHistory.RemoveAt(0);
                }
            }
        }

        public List<GlobalChatMessage> GetChatHistory(int count = 50)
        {
            lock (historyLock)
            {
                return chatHistory.TakeLast(count).ToList();
            }
        }

        public void RemoveDisconnectedUser(ClientHandler client)
        {
            var username = onlineUsers.FirstOrDefault(x => x.Value == client).Key;
            if (!string.IsNullOrEmpty(username))
            {
                LeaveGlobalChat(username);
            }
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"[GlobalChat] {message}");
        }
    }

    public class GlobalChatMessage
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string Type { get; set; }
    }
}