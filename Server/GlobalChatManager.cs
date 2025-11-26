using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DoAn_NT106.Server
{
    /// <summary>
    /// Quản lý Global Chat - Chat toàn cục cho tất cả người chơi online
    /// </summary>
    public class GlobalChatManager
    {
        // Dictionary lưu user online: username -> ClientHandler
        private ConcurrentDictionary<string, ClientHandler> onlineUsers = new ConcurrentDictionary<string, ClientHandler>();

        // Lưu lịch sử chat (giới hạn 100 tin nhắn gần nhất)
        private List<GlobalChatMessage> chatHistory = new List<GlobalChatMessage>();
        private readonly object historyLock = new object();
        private const int MAX_HISTORY = 100;

        public event Action<string> OnLog;

        // ===========================
        // QUẢN LÝ USER ONLINE
        // ===========================

        /// <summary>
        /// User tham gia Global Chat
        /// </summary>
        public (bool Success, string Message, int OnlineCount) JoinGlobalChat(string username, ClientHandler client)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                    return (false, "Username is required", 0);

                // Nếu user đã online, cập nhật client mới
                onlineUsers.AddOrUpdate(username, client, (key, oldClient) => client);

                int onlineCount = onlineUsers.Count;
                Log($"✅ {username} joined Global Chat. Online: {onlineCount}");

                // Broadcast thông báo user join
                BroadcastSystemMessage($"{username} đã tham gia chat", username);

                return (true, "Joined Global Chat", onlineCount);
            }
            catch (Exception ex)
            {
                Log($"❌ JoinGlobalChat error: {ex.Message}");
                return (false, ex.Message, 0);
            }
        }

        /// <summary>
        /// User rời Global Chat
        /// </summary>
        public (bool Success, int OnlineCount) LeaveGlobalChat(string username)
        {
            try
            {
                if (onlineUsers.TryRemove(username, out _))
                {
                    int onlineCount = onlineUsers.Count;
                    Log($"👋 {username} left Global Chat. Online: {onlineCount}");

                    // Broadcast thông báo user leave
                    BroadcastSystemMessage($"{username} đã rời chat", username);

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

        /// <summary>
        /// Lấy số người online
        /// </summary>
        public int GetOnlineCount() => onlineUsers.Count;

        /// <summary>
        /// Lấy danh sách user online
        /// </summary>
        public List<string> GetOnlineUsers() => onlineUsers.Keys.ToList();

        // ===========================
        // XỬ LÝ CHAT MESSAGE
        // ===========================

        /// <summary>
        /// Gửi tin nhắn chat
        /// </summary>
        public (bool Success, string Message) SendChatMessage(string username, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                    return (false, "Username is required");

                if (string.IsNullOrEmpty(message))
                    return (false, "Message is required");

                // Giới hạn 1000 ký tự
                if (message.Length > 1000)
                    message = message.Substring(0, 1000);

                // Kiểm tra user có online không
                if (!onlineUsers.ContainsKey(username))
                    return (false, "User not in Global Chat");

                // Tạo chat message
                var chatMessage = new GlobalChatMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = username,
                    Message = message,
                    Timestamp = DateTime.Now,
                    Type = "user"
                };

                // Lưu vào history
                AddToHistory(chatMessage);

                // Broadcast đến tất cả online users
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

        /// <summary>
        /// Broadcast tin nhắn hệ thống
        /// </summary>
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

        /// <summary>
        /// Broadcast chat message đến tất cả users
        /// </summary>
        private void BroadcastChatMessage(GlobalChatMessage chatMessage, string excludeUser = null)
        {
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
                    onlineCount = onlineUsers.Count
                }
            };

            string json = JsonSerializer.Serialize(broadcast);

            foreach (var kvp in onlineUsers)
            {
                // Có thể exclude user (VD: không gửi thông báo join cho chính user đó)
                if (excludeUser != null && kvp.Key == excludeUser)
                    continue;

                try
                {
                    kvp.Value.SendMessage(json);
                }
                catch (Exception ex)
                {
                    Log($"⚠️ Failed to send to {kvp.Key}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Broadcast cập nhật số online (khi có người join/leave)
        /// </summary>
        public void BroadcastOnlineCount()
        {
            var broadcast = new
            {
                Action = "GLOBAL_CHAT_ONLINE_UPDATE",
                Data = new
                {
                    onlineCount = onlineUsers.Count
                }
            };

            string json = JsonSerializer.Serialize(broadcast);

            foreach (var kvp in onlineUsers)
            {
                try
                {
                    kvp.Value.SendMessage(json);
                }
                catch { }
            }
        }

        // ===========================
        // CHAT HISTORY
        // ===========================

        /// <summary>
        /// Thêm tin nhắn vào history
        /// </summary>
        private void AddToHistory(GlobalChatMessage message)
        {
            lock (historyLock)
            {
                chatHistory.Add(message);

                // Giữ tối đa MAX_HISTORY tin nhắn
                if (chatHistory.Count > MAX_HISTORY)
                {
                    chatHistory.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Lấy lịch sử chat (cho user mới join)
        /// </summary>
        public List<GlobalChatMessage> GetChatHistory(int count = 50)
        {
            lock (historyLock)
            {
                return chatHistory.TakeLast(count).ToList();
            }
        }

        // ===========================
        // CLEANUP
        // ===========================

        /// <summary>
        /// Xóa user khỏi online khi disconnect
        /// </summary>
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

    // ===========================
    // DATA MODEL
    // ===========================
    public class GlobalChatMessage
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } // "user" hoặc "system"
    }
}