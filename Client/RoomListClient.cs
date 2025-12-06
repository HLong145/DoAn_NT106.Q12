using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DoAn_NT106.Services;

namespace DoAn_NT106.Client
{
    /// <summary>
    /// Room List Client - Sử dụng PersistentTcpClient singleton
    /// /// </summary>
    public class RoomListClient
    {
        // Sử dụng PersistentTcpClient singleton
        private PersistentTcpClient TcpClient => PersistentTcpClient.Instance;

        private string username;
        private string token;
        private bool isConnected = false;
        private bool isDisposed = false;

        // Events
        public event Action<List<RoomListInfo>> OnRoomListUpdated;
        public event Action<string> OnError;

        public bool IsConnected => isConnected && TcpClient.IsConnected;

        // ===========================
        // CONSTRUCTOR
        // ===========================
        public RoomListClient()
        {
            // Subscribe vào broadcast của PersistentTcpClient
            TcpClient.OnBroadcast += HandleBroadcast;
        }

        public RoomListClient(string address, int port) : this()
        {
            // Ignore address/port - dùng PersistentTcpClient singleton
        }

        // ===========================
        // CONNECT AND SUBSCRIBE
        // ===========================
        public async Task<bool> ConnectAndSubscribeAsync(string username, string token)
        {
            try
            {
                this.username = username;
                this.token = token;

                // Đảm bảo đã connect
                if (!TcpClient.IsConnected)
                {
                    bool connected = await TcpClient.ConnectAsync();
                    if (!connected)
                    {
                        OnError?.Invoke("Cannot connect to server");
                        return false;
                    }
                }

                // Gửi request subscribe
                var response = await TcpClient.SendRequestAsync("ROOM_LIST_SUBSCRIBE",
                    new Dictionary<string, object>
                    {
                        { "username", username },
                        { "token", token }
                    });

                Console.WriteLine($"[RoomListClient] Subscribe response: {response.Success} - {response.Message}");

                if (response.Success)
                {
                    isConnected = true;

                    // Parse danh sách phòng từ response
                    if (response.RawData.ValueKind != JsonValueKind.Undefined &&
                        response.RawData.TryGetProperty("rooms", out var roomsArray))
                    {
                        var rooms = ParseRoomsFromJson(roomsArray);
                        Console.WriteLine($"[RoomListClient] Initial rooms: {rooms.Count}");
                        OnRoomListUpdated?.Invoke(rooms);
                    }

                    return true;
                }

                OnError?.Invoke(response.Message);
                return false;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Connect error: {ex.Message}");
                return false;
            }
        }

        // ===========================
        // HANDLE BROADCASTS
        // ===========================
        private void HandleBroadcast(string action, JsonElement data)
        {
            if (!isConnected || isDisposed) return;

            try
            {
                if (action == "ROOM_LIST_UPDATE")
                {
                    Console.WriteLine($"[RoomListClient] Room list update received");

                    if (data.TryGetProperty("rooms", out var roomsArray))
                    {
                        var rooms = ParseRoomsFromJson(roomsArray);
                        Console.WriteLine($"[RoomListClient] Parsed {rooms.Count} rooms from broadcast");
                        OnRoomListUpdated?.Invoke(rooms);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoomListClient] HandleBroadcast error: {ex.Message}");
                OnError?.Invoke($"Process broadcast error: {ex.Message}");
            }
        }

        // ===========================
        // PARSE ROOMS FROM JSON
        // ===========================
        private List<RoomListInfo> ParseRoomsFromJson(JsonElement roomsArray)
        {
            var rooms = new List<RoomListInfo>();

            try
            {
                foreach (var roomEl in roomsArray.EnumerateArray())
                {
                    rooms.Add(new RoomListInfo
                    {
                        RoomCode = GetStringProperty(roomEl, "roomCode"),
                        RoomName = GetStringProperty(roomEl, "roomName"),
                        HasPassword = GetBoolProperty(roomEl, "hasPassword"),
                        PlayerCount = GetIntProperty(roomEl, "playerCount"),
                        Status = GetStringProperty(roomEl, "status")
                    });
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"ParseRoomsFromJson error: {ex.Message}");
            }

            return rooms;
        }

        // ===========================
        // HELPER METHODS
        // ===========================
        private string GetStringProperty(JsonElement el, string name)
        {
            return el.TryGetProperty(name, out var prop) ? prop.GetString() ?? "" : "";
        }

        private bool GetBoolProperty(JsonElement el, string name)
        {
            return el.TryGetProperty(name, out var prop) && prop.GetBoolean();
        }

        private int GetIntProperty(JsonElement el, string name)
        {
            return el.TryGetProperty(name, out var prop) ? prop.GetInt32() : 0;
        }

        // ===========================
        // DISCONNECT / UNSUBSCRIBE
        // ===========================
        public void Disconnect()
        {
            if (isDisposed) return;
            isDisposed = true;

            try
            {
                // Unsubscribe từ events
                TcpClient.OnBroadcast -= HandleBroadcast;

                // Gửi unsubscribe request nếu đang connected
                if (isConnected && TcpClient.IsConnected)
                {
                    _ = TcpClient.SendRequestAsync("ROOM_LIST_UNSUBSCRIBE",
                        new Dictionary<string, object> { { "username", username } });
                }

                isConnected = false;
            }
            catch { }

            // KHÔNG disconnect PersistentTcpClient vì nó dùng chung
        }
    }

    // ===========================
    // DATA CLASS
    // ===========================
    public class RoomListInfo
    {
        public string RoomCode { get; set; }
        public string RoomName { get; set; }
        public bool HasPassword { get; set; }
        public int PlayerCount { get; set; }
        public string Status { get; set; }
    }
}