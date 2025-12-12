using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DoAn_NT106.Client
{
    /// <summary>
    /// UDP Client g?i/nh?n game state binary 30-50ms/l?n
    /// ? ENHANCED: Now supports combat events (attack, damage, parry)
    /// </summary>
    public class UDPGameClient : IDisposable
    {
        #region Fields

        private UdpClient udpSocket;
        private string serverIp;
        private int serverPort;
        private string roomCode;
        private string username;
        private int playerNumber; // 1 or 2

        private bool isConnected = false;
        private CancellationTokenSource cts;
        private Task sendTask;
        private Task receiveTask;

        // Game state to send
        private int currentX, currentY;
        private int currentHealth, currentStamina, currentMana;
        private string currentAction = "idle";
        private string currentFacing = "right"; // ? ADD: Facing direction
        private readonly object stateLock = new object();

        // ? ADD: Packet types
        private enum PacketType : byte
        {
            StateUpdate = 0,    // Position, health, animation
            CombatEvent = 1,    // Attack, damage, parry
            RoundEvent = 2      // Round start/end
        }

        // Events
        public event Action<string> OnLog;
        public event Action<string> OnAck; // ACK from server (optional)
        public event Action<byte[]> OnOpponentState; // Receive opponent's state
        public event Action<string, Dictionary<string, object>> OnCombatEvent; // ? ADD: Combat events

        public bool IsConnected => isConnected;

        #endregion

        #region Constructor

        public UDPGameClient(string serverIp, int serverPort, string roomCode, string username)
        {
            this.serverIp = serverIp;
            this.serverPort = serverPort;
            this.roomCode = roomCode;
            this.username = username;
        }

        #endregion

        #region Connect/Disconnect

        public void Connect()
        {
            if (isConnected)
            {
                Log("?? Already connected");
                return;
            }

            try
            {
                udpSocket = new UdpClient();
                udpSocket.Connect(serverIp, serverPort);

                isConnected = true;
                cts = new CancellationTokenSource();

                // Start send loop (30-50ms interval)
                sendTask = Task.Run(() => SendLoop(cts.Token));

                // Start receive loop
                receiveTask = Task.Run(() => ReceiveLoop(cts.Token));

                Log($"? UDP connected to {serverIp}:{serverPort}");
            }
            catch (Exception ex)
            {
                Log($"? UDP connect error: {ex.Message}");
                throw;
            }
        }

        public void Disconnect()
        {
            if (!isConnected) return;

            try
            {
                isConnected = false;
                cts?.Cancel();

                udpSocket?.Close();

                sendTask?.Wait(500);
                receiveTask?.Wait(500);

                Log("?? UDP disconnected");
            }
            catch (Exception ex)
            {
                Log($"? UDP disconnect error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Disconnect();
            udpSocket?.Dispose();
        }

        #endregion

        #region Update State

    /// <summary>
    /// Update current game state (called from BattleForm timer)
    /// ? ENHANCED: Now includes Facing direction for proper animation sync
    /// </summary>
    public void UpdateState(int x, int y, int health, int stamina, int mana, string action, string facing = "right")
    {
        lock (stateLock)
        {
            currentX = x;
            currentY = y;
            currentHealth = health;
            currentStamina = stamina;
            currentMana = mana;
            currentAction = action ?? "idle";
            currentFacing = facing ?? "right"; // ? ADD: Store facing direction
        }
    }

    /// <summary>
    /// ? NEW: Send combat event (attack, damage, parry) to opponent
    /// </summary>
    public void SendCombatEvent(string eventType, Dictionary<string, object> eventData)
    {
        if (!isConnected || udpSocket == null) return;

        try
        {
            byte[] packet = BuildCombatEventPacket(eventType, eventData);
            udpSocket.Send(packet, packet.Length);
            Log($"? Sent combat event: {eventType}");
        }
        catch (Exception ex)
        {
            Log($"? SendCombatEvent error: {ex.Message}");
        }
    }

        // Convenience: send state update immediately (one-shot) to reduce latency for key events
        public void SendImmediateState(int x, int y, int health, int stamina, int mana, string action, string facing = "right")
        {
            lock (stateLock)
            {
                currentX = x; currentY = y; currentHealth = health; currentStamina = stamina; currentMana = mana; currentAction = action; currentFacing = facing;
            }
            try
            {
                byte[] packet = BuildPacket();
                udpSocket.Send(packet, packet.Length);
            }
            catch { }
        }

        #endregion

        #region Send Loop

        private async Task SendLoop(CancellationToken token)
        {
            Log("?? UDP send loop started");

            try
            {
                while (!token.IsCancellationRequested && isConnected)
                {
                    byte[] packet = BuildPacket();
                    await udpSocket.SendAsync(packet, packet.Length);

                    // ? FIX: Reduce delay from 40ms to 16ms for ~60 FPS update rate
                    // This provides much smoother gameplay with lower perceived latency
                    await Task.Delay(16, token); // 16ms ? 60 FPS
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                    Log($"? Send loop error: {ex.Message}");
            }

            Log("?? UDP send loop stopped");
        }

        private byte[] BuildPacket()
        {
            lock (stateLock)
            {
                // Binary packet structure:
                // [PacketType(1)] [RoomCode(6)] [PlayerNum(1)] [X(2)] [Y(2)] [Health(1)] [Stamina(1)] [Mana(1)] [FacingLen(1)] [Facing(var)] [ActionLen(1)] [Action(var)]

                byte[] facingBytes = System.Text.Encoding.UTF8.GetBytes(currentFacing);
                int facingLen = Math.Min(facingBytes.Length, 10); // Max 10 chars

                byte[] actionBytes = System.Text.Encoding.UTF8.GetBytes(currentAction);
                int actionLen = Math.Min(actionBytes.Length, 20); // Max 20 chars

                // Need +1 for ActionLen byte and +1 since last written index is 16+facingLen+actionLen
                byte[] packet = new byte[17 + facingLen + actionLen];

                // ? ADD: Packet type
                packet[0] = (byte)PacketType.StateUpdate;

                // RoomCode (6 bytes, padded with nulls)
                byte[] roomCodeBytes = System.Text.Encoding.UTF8.GetBytes(roomCode.PadRight(6, '\0'));
                Array.Copy(roomCodeBytes, 0, packet, 1, 6);

                // PlayerNum (1 byte)
                packet[7] = (byte)playerNumber;

                // X (2 bytes, little-endian)
                packet[8] = (byte)(currentX & 0xFF);
                packet[9] = (byte)((currentX >> 8) & 0xFF);

                // Y (2 bytes)
                packet[10] = (byte)(currentY & 0xFF);
                packet[11] = (byte)((currentY >> 8) & 0xFF);

                // Health (1 byte)
                packet[12] = (byte)Math.Clamp(currentHealth, 0, 255);

                // Stamina (1 byte)
                packet[13] = (byte)Math.Clamp(currentStamina, 0, 255);

                // Mana (1 byte)
                packet[14] = (byte)Math.Clamp(currentMana, 0, 255);

                // FacingLen (1 byte)
                packet[15] = (byte)facingLen;

                // Facing (variable length)
                if (facingLen > 0)
                    Array.Copy(facingBytes, 0, packet, 16, facingLen);

                // ActionLen (1 byte)
                packet[16 + facingLen] = (byte)actionLen;

                // Action (variable length)
                if (actionLen > 0)
                    Array.Copy(actionBytes, 0, packet, 17 + facingLen, actionLen);

                return packet;
            }
        }

        /// <summary>
        /// ? NEW: Build combat event packet
        /// </summary>
        private byte[] BuildCombatEventPacket(string eventType, Dictionary<string, object> eventData)
        {
            // Packet structure:
            // [PacketType(1)] [RoomCode(6)] [PlayerNum(1)] [EventTypeLen(1)] [EventType(var)] [DataLen(2)] [Data(JSON)]

            byte[] eventTypeBytes = System.Text.Encoding.UTF8.GetBytes(eventType);
            int eventTypeLen = Math.Min(eventTypeBytes.Length, 20);

            string jsonData = System.Text.Json.JsonSerializer.Serialize(eventData);
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
            int jsonLen = Math.Min(jsonBytes.Length, 1024); // Max 1KB

            byte[] packet = new byte[11 + eventTypeLen + jsonLen];

            // Packet type
            packet[0] = (byte)PacketType.CombatEvent;

            // RoomCode (6 bytes)
            byte[] roomCodeBytes = System.Text.Encoding.UTF8.GetBytes(roomCode.PadRight(6, '\0'));
            Array.Copy(roomCodeBytes, 0, packet, 1, 6);

            // PlayerNum (1 byte)
            packet[7] = (byte)playerNumber;

            // EventTypeLen (1 byte)
            packet[8] = (byte)eventTypeLen;

            // EventType (variable)
            if (eventTypeLen > 0)
                Array.Copy(eventTypeBytes, 0, packet, 9, eventTypeLen);

            // DataLen (2 bytes)
            packet[9 + eventTypeLen] = (byte)(jsonLen & 0xFF);
            packet[10 + eventTypeLen] = (byte)((jsonLen >> 8) & 0xFF);

            // Data (JSON)
            if (jsonLen > 0)
                Array.Copy(jsonBytes, 0, packet, 11 + eventTypeLen, jsonLen);

            return packet;
        }

        #endregion

        #region Receive Loop

        private async Task ReceiveLoop(CancellationToken token)
        {
            Log("?? UDP receive loop started");

            try
            {
                while (!token.IsCancellationRequested && isConnected)
                {
                    var result = await udpSocket.ReceiveAsync();
                    ProcessReceivedPacket(result.Buffer);
                }
            }
            catch (ObjectDisposedException)
            {
                // Socket closed
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                    Log($"? Receive loop error: {ex.Message}");
            }

            Log("?? UDP receive loop stopped");
        }

        private void ProcessReceivedPacket(byte[] data)
        {
            try
            {
                if (data == null || data.Length < 1)
                    return;

                // ? NEW: Check packet type
                PacketType packetType = (PacketType)data[0];

                switch (packetType)
                {
                    case PacketType.StateUpdate:
                        // Invoke event for BattleForm to handle opponent's state
                        OnOpponentState?.Invoke(data);
                        break;

                    case PacketType.CombatEvent:
                        ProcessCombatEvent(data);
                        break;

                    default:
                        Log($"?? Unknown packet type: {packetType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log($"? Process packet error: {ex.Message}");
            }
        }

        /// <summary>
        /// ? NEW: Process combat event packet
        /// </summary>
        private void ProcessCombatEvent(byte[] data)
        {
            try
            {
                if (data.Length < 11) return;

                // [PacketType(1)] [RoomCode(6)] [PlayerNum(1)] [EventTypeLen(1)] [EventType(var)] [DataLen(2)] [Data(JSON)]

                int senderPlayerNum = data[7];
                int eventTypeLen = data[8];

                if (data.Length < 11 + eventTypeLen) return;

                string eventType = System.Text.Encoding.UTF8.GetString(data, 9, eventTypeLen);

                int jsonLen = data[9 + eventTypeLen] | (data[10 + eventTypeLen] << 8);

                if (data.Length < 11 + eventTypeLen + jsonLen) return;

                string jsonData = System.Text.Encoding.UTF8.GetString(data, 11 + eventTypeLen, jsonLen);

                // Deserialize into JsonElement dictionary then convert to primitive types
                try
                {
                    var raw = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(jsonData);
                    var eventData = new Dictionary<string, object>();
                    if (raw != null)
                    {
                        foreach (var kv in raw)
                        {
                            var ve = kv.Value;
                            switch (ve.ValueKind)
                            {
                                case System.Text.Json.JsonValueKind.Number:
                                    if (ve.TryGetInt32(out int vi)) eventData[kv.Key] = vi;
                                    else if (ve.TryGetInt64(out long vl)) eventData[kv.Key] = vl;
                                    else if (ve.TryGetDouble(out double vd)) eventData[kv.Key] = vd;
                                    else eventData[kv.Key] = ve.GetRawText();
                                    break;
                                case System.Text.Json.JsonValueKind.String:
                                    eventData[kv.Key] = ve.GetString();
                                    break;
                                case System.Text.Json.JsonValueKind.True:
                                case System.Text.Json.JsonValueKind.False:
                                    eventData[kv.Key] = ve.GetBoolean();
                                    break;
                                case System.Text.Json.JsonValueKind.Object:
                                case System.Text.Json.JsonValueKind.Array:
                                    eventData[kv.Key] = ve.GetRawText();
                                    break;
                                default:
                                    eventData[kv.Key] = ve.GetRawText();
                                    break;
                            }
                        }
                    }

                    OnCombatEvent?.Invoke(eventType, eventData);
                    Log($"? Received combat event: {eventType}");
                }
                catch (Exception ex)
                {
                    Log($"? CombatEvent deserialize error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Log($"? ProcessCombatEvent error: {ex.Message}");
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Set player number (1 or 2) - called after joining lobby
        /// </summary>
        public void SetPlayerNumber(int playerNum)
        {
            this.playerNumber = playerNum;
            Log($"?? Player number set: {playerNum}");
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"[UDP] {message}");
        }

        #endregion
    }
}
