using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;

namespace DoAn_NT106.Server
{
    public class RoomListBroadcaster
    {
        private ConcurrentDictionary<string, ClientHandler> listeners = new ConcurrentDictionary<string, ClientHandler>();
        private RoomManager roomManager;
        public event Action<string> OnLog;

        // JSON options với camelCase
        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public RoomListBroadcaster(RoomManager roomManager)
        {
            this.roomManager = roomManager;
        }

        // ===========================
        // ĐĂNG KÝ LẮNG NGHE
        // ===========================
        public (bool Success, string Message, List<RoomInfo> Rooms) Subscribe(string username, ClientHandler client)
        {
            try
            {
                listeners[username] = client;
                Log($"✅ {username} subscribed to room list broadcasts");

                // Trả về danh sách phòng ngay lập tức
                var rooms = roomManager.GetAvailableRooms();
                Log($"📋 Returning {rooms.Count} rooms to {username}");

                return (true, "Subscribed successfully", rooms);
            }
            catch (Exception ex)
            {
                Log($"❌ Subscribe error: {ex.Message}");
                return (false, ex.Message, null);
            }
        }

        // ===========================
        // HỦY ĐĂNG KÝ
        // ===========================
        public void Unsubscribe(string username)
        {
            if (listeners.TryRemove(username, out _))
            {
                Log($"👋 {username} unsubscribed from room list");
            }
        }

        // ===========================
        // BROADCAST DANH SÁCH PHÒNG
        // ===========================
        public void BroadcastRoomList()
        {
            try
            {
                var rooms = roomManager.GetAvailableRooms();

                // Tạo data với camelCase
                var roomsData = new List<object>();
                foreach (var r in rooms)
                {
                    roomsData.Add(new Dictionary<string, object>
                    {
                        { "roomCode", r.RoomCode },
                        { "roomName", r.RoomName },
                        { "hasPassword", r.HasPassword },
                        { "playerCount", r.PlayerCount },
                        { "status", r.Status }
                    });
                }

                var broadcast = new
                {
                    Action = "ROOM_LIST_UPDATE",
                    Data = new
                    {
                        rooms = roomsData,
                        timestamp = DateTime.Now
                    }
                };

                string json = JsonSerializer.Serialize(broadcast);

                Log($"📤 Broadcasting: {json.Substring(0, Math.Min(200, json.Length))}...");

                foreach (var kvp in listeners.ToArray())
                {
                    try
                    {
                        kvp.Value.SendMessage(json);
                        Log($"   → Sent to {kvp.Key}");
                    }
                    catch (Exception ex)
                    {
                        Log($"   ❌ Failed to send to {kvp.Key}: {ex.Message}");
                        listeners.TryRemove(kvp.Key, out _);
                    }
                }

                Log($"📢 Broadcasted room list to {listeners.Count} listeners ({rooms.Count} rooms)");
            }
            catch (Exception ex)
            {
                Log($"❌ BroadcastRoomList error: {ex.Message}");
            }
        }

        private void Log(string message) => OnLog?.Invoke($"[RoomListBroadcaster] {message}");
    }
}
