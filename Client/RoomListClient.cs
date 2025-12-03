using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DoAn_NT106.Client
{
    public class RoomListClient
    {
        private TcpClient client;
        private NetworkStream stream;
        private CancellationTokenSource cts;
        private string serverAddress;
        private int serverPort;
        private string username;
        private string token;
        private bool isConnected;

        // Events
        public event Action<List<RoomListInfo>> OnRoomListUpdated;
        public event Action<string> OnError;

        public RoomListClient(string address = "127.0.0.1", int port = 8080)
        {
            serverAddress = address;
            serverPort = port;
        }

        // ===========================
        // KẾT NỐI VÀ SUBSCRIBE
        // ===========================
        public async Task<bool> ConnectAndSubscribeAsync(string username, string token)
        {
            try
            {
                this.username = username;
                this.token = token;

                client = new TcpClient();
                await client.ConnectAsync(serverAddress, serverPort);
                stream = client.GetStream();

                // Gửi request subscribe
                var request = new
                {
                    Action = "ROOM_LIST_SUBSCRIBE",
                    Data = new Dictionary<string, object>
                    {
                        { "username", username },
                        { "token", token }
                    }
                };

                string requestJson = JsonSerializer.Serialize(request);
                byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);
                await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                // Đọc response
                byte[] buffer = new byte[65536];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"[RoomListClient] Response: {responseJson.Substring(0, Math.Min(300, responseJson.Length))}...");

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                bool success = root.GetProperty("Success").GetBoolean();

                if (success)
                {
                    isConnected = true;

                    // Parse danh sách phòng từ response
                    if (root.TryGetProperty("Data", out var data) &&
                        data.TryGetProperty("rooms", out var roomsArray))
                    {
                        var rooms = ParseRoomsFromJson(roomsArray);

                        Console.WriteLine($"[RoomListClient] Initial rooms: {rooms.Count}");

                        // Invoke event với danh sách phòng ban đầu
                        OnRoomListUpdated?.Invoke(rooms);
                    }

                    // Bắt đầu listen cho broadcasts tiếp theo
                    cts = new CancellationTokenSource();
                    _ = Task.Run(() => ListenForBroadcasts(cts.Token));

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Connect error: {ex.Message}");
                return false;
            }
        }

        // ===========================
        // LẮNG NGHE BROADCAST
        // ===========================
        private async Task ListenForBroadcasts(CancellationToken token)
        {
            byte[] buffer = new byte[65536];

            try
            {
                while (!token.IsCancellationRequested && stream != null && client?.Connected == true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0) break;

                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessBroadcast(json);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Listen error: {ex.Message}");
            }
        }

        // ===========================
        // XỬ LÝ BROADCAST
        // ===========================
        private void ProcessBroadcast(string json)
        {
            try
            {
                Console.WriteLine($"[RoomListClient] Broadcast received: {json.Substring(0, Math.Min(300, json.Length))}...");

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("Action", out var actionElement))
                    return;

                string action = actionElement.GetString();

                if (action == "ROOM_LIST_UPDATE" && root.TryGetProperty("Data", out var data))
                {
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
        // DISCONNECT
        // ===========================
        public void Disconnect()
        {
            try
            {
                isConnected = false;
                cts?.Cancel();
                stream?.Close();
                client?.Close();
            }
            catch { }
        }
    }

    // ===========================
    // DATA CLASS (Đổi tên để tránh conflict)
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