using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;

namespace DoAn_NT106.Server
{
    #region Class definition
    public class RoomListBroadcaster
    {
        #endregion

        #region Fields

        // Danh sách các client đang lắng nghe broadcast danh sách phòng, key là username
        private ConcurrentDictionary<string, ClientHandler> listeners 
            = new ConcurrentDictionary<string, ClientHandler>();

        private RoomManager roomManager;

        #endregion

        #region Events

        public event Action<string> OnLog;

        #endregion

        #region JSON options

        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        #endregion

        #region Constructor

        public RoomListBroadcaster(RoomManager roomManager)
        {
            this.roomManager = roomManager;
        }

        #endregion

        #region Subscribe

        public (bool Success, string Message, List<RoomInfo> Rooms) Subscribe
            (string username, ClientHandler client)
        {
            try
            {
                // Lưu client theo username để  broadcast
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

        #endregion

        #region Unsubscribe

        public void Unsubscribe(string username)
        {
            // Xóa client khỏi danh sách  nếu tồn tại
            if (listeners.TryRemove(username, out _))
            {
                Log($"👋 {username} unsubscribed from room list");
            }
        }

        #endregion

        #region Broadcast

        public void BroadcastRoomList()
        {
            try
            {
                // Lấy danh sách phòng khả dụng từ RoomManager
                var rooms = roomManager.GetAvailableRooms();

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

                // Gửi cho từng client đang lắng nghe
                foreach (var kvp in listeners.ToArray())
                {
                    try
                    {
                        kvp.Value.SendMessage(json);
                        Log($" → Sent to {kvp.Key}");
                    }
                    catch (Exception ex)
                    {
                        Log($" ❌ Failed to send to {kvp.Key}: {ex.Message}");
                        // Nếu gửi thất bại thì remove listener đó
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

        #endregion

        #region Logging

        // Hàm helper để log, thêm prefix tên class
        private void Log(string message) => OnLog?.Invoke($"[RoomListBroadcaster] {message}");

        #endregion
    }
}
