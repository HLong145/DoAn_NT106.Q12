using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DoAn_NT106.Services;

namespace DoAn_NT106.Client.Class
{
    /// <summary>
    /// UDP Client gửi/nhận game state binary 30-50ms/lần
    ///  UPDATED Packet structure: [RoomCode(6)] [PlayerNum(1)] [X(2)] [Y(2)] [Health(1)] [Stamina(1)] [Mana(1)] 
    ///                     [Facing(1)] [IsAttacking(1)] [IsParrying(1)] [IsStunned(1)] [IsSkillActive(1)] [IsCharging(1)] [IsDashing(1)] [ActionLen(1)] [Action(var)]
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
        private string currentAction = "stand";
        private string currentFacing = "right";       
        private bool currentIsAttacking = false;       
        private bool currentIsParrying = false;       
        private bool currentIsStunned = false;       
        private bool currentIsSkillActive = false;    
        private bool currentIsCharging = false;        
        private bool currentIsDashing = false;         
        private int currentLastDamagingAttackId = 0;   //   last damaging attack id
        private readonly object stateLock = new object();

        // Events
        public event Action<string> OnLog;
        public event Action<string> OnAck; // ACK from server (optional)
        public event Action<byte[]> OnOpponentState; // Receive opponent's state

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

        /// <summary>
        /// Send a damage notification via UDP. Format placed into action string so server will relay to opponent.
        /// action format: "DAMAGE:{targetPlayerNum}:{damage}:{resultingHealth}"
        /// </summary>
        public void SendDamageNotification(int targetPlayerNum, int damage, int resultingHealth)
        {
            try
            {
                lock (stateLock)
                {
                    // Keep current health/stamina/mana as-is for sender; embed damage info into action
                    currentLastDamagingAttackId++;
                    currentAction = $"DAMAGE:{targetPlayerNum}:{damage}:{resultingHealth}";
                }

                byte[] packet = BuildPacket();
                udpSocket?.Send(packet, packet.Length);
                Log($"Sent DAMAGE notification: target={targetPlayerNum} dmg={damage} resHP={resultingHealth}");
            }
            catch (Exception ex)
            {
                Log($"SendDamageNotification error: {ex.Message}");
            }
        }

        /// <summary>
        /// Send immediate health update (used after local damage applied) to speed up sync
        /// </summary>
        public void SendImmediateHealthUpdate(int health, int attackId)
        {
            try
            {
                lock (stateLock)
                {
                    currentHealth = health;
                    currentLastDamagingAttackId = attackId;
                }

                byte[] packet = BuildPacket();
                udpSocket?.Send(packet, packet.Length);
                Log($"Sent immediate health update: HP={health} attackId={attackId}");
            }
            catch (Exception ex)
            {
                Log($"SendImmediateHealthUpdate error: {ex.Message}");
            }
        }

        #endregion

        #region Connect/Disconnect

        public void Connect()
        {
            if (isConnected)
            {
                Log("? Already connected");
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
        ///  UPDATED: Include all combat state flags
        /// </summary>
        public void UpdateState(int x, int y, int health, int stamina, int mana, string action, 
            string facing = "right", bool isAttacking = false, bool isParrying = false,
            bool isStunned = false, bool isSkillActive = false, bool isCharging = false, bool isDashing = false, int lastDamagingAttackId = 0)
        {
            lock (stateLock)
            {
                currentX = x;
                currentY = y;
                currentHealth = health;
                currentStamina = stamina;
                currentMana = mana;
                currentAction = action ?? "stand";
                currentFacing = facing ?? "right";
                currentIsAttacking = isAttacking;
                currentIsParrying = isParrying;
                currentIsStunned = isStunned;          
                currentIsSkillActive = isSkillActive;   
                currentIsCharging = isCharging;        
                currentIsDashing = isDashing;           
                currentLastDamagingAttackId = lastDamagingAttackId;
            }
        }

        #endregion

        #region Send Loop

        private async Task SendLoop(CancellationToken token)
        {
            Log("🚀 UDP send loop started");

            try
            {
                while (!token.IsCancellationRequested && isConnected)
                {
                    byte[] packet = BuildPacket();
                    
                    try
                    {
                        await udpSocket.SendAsync(packet, packet.Length);

                        //  SPARSE LOG - mỗi 100 packets (roughly 4 seconds at 25 FPS)
                        if (DateTime.Now.Millisecond % 4000 < 100)
                        {
                            Console.WriteLine($"[UDP] P{playerNumber} sent packet: {packet.Length} bytes");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"⚠️ Send error: {ex.Message}");
                    }

                    //  SỬA: 50ms interval (20 lần/giây) - tần suất cao hơn để giảm giật
                    await Task.Delay(16, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                    Log($"❌ Send loop error: {ex.Message}");
            }

            Log("🛑 UDP send loop stopped");
        }

        private byte[] BuildPacket()
        {
            lock (stateLock)
            {
                //  UPDATED PACKET STRUCTURE:
                // [RoomCode(6)] [PlayerNum(1)] [X(2)] [Y(2)] [Health(1)] [Stamina(1)] [Mana(1)] 
                // [Facing(1)] [IsAttacking(1)] [IsParrying(1)] [IsStunned(1)] [IsSkillActive(1)] [IsCharging(1)] [IsDashing(1)] [LastDamagingAttackId(1)] [ActionLen(1)] [Action(var)]

                byte[] actionBytes = System.Text.Encoding.UTF8.GetBytes(currentAction);
                int actionLen = Math.Min(actionBytes.Length, 20); // Max 20 chars

                // packet length = header (22 bytes) + actionLen
                byte[] packet = new byte[22 + actionLen];

                // RoomCode (6 bytes, padded with nulls)
                byte[] roomCodeBytes = System.Text.Encoding.UTF8.GetBytes(roomCode.PadRight(6, '\0'));
                Array.Copy(roomCodeBytes, 0, packet, 0, 6);

                // PlayerNum (1 byte)
                packet[6] = (byte)playerNumber;

                // X (2 bytes, little-endian)
                packet[7] = (byte)(currentX & 0xFF);
                packet[8] = (byte)(currentX >> 8 & 0xFF);

                // Y (2 bytes)
                packet[9] = (byte)(currentY & 0xFF);
                packet[10] = (byte)(currentY >> 8 & 0xFF);

                // Health (1 byte)
                packet[11] = (byte)Math.Clamp(currentHealth, 0, 255);

                // Stamina (1 byte)
                packet[12] = (byte)Math.Clamp(currentStamina, 0, 255);

                // Mana (1 byte)
                packet[13] = (byte)Math.Clamp(currentMana, 0, 255);

                // Facing (1 byte) - 'L' = left, 'R' = right
                packet[14] = (byte)(currentFacing == "left" ? 'L' : 'R');

                // IsAttacking (1 byte) - 1 = true, 0 = false
                packet[15] = currentIsAttacking ? (byte)1 : (byte)0;

                // IsParrying (1 byte) - 1 = true, 0 = false
                packet[16] = currentIsParrying ? (byte)1 : (byte)0;

                //   IsStunned (1 byte)
                packet[17] = currentIsStunned ? (byte)1 : (byte)0;

                //   IsSkillActive (1 byte)
                packet[18] = currentIsSkillActive ? (byte)1 : (byte)0;

                //   IsCharging (1 byte)
                packet[19] = currentIsCharging ? (byte)1 : (byte)0;

                //   IsDashing (1 byte)
                packet[20] = currentIsDashing ? (byte)1 : (byte)0;

                // ActionLen (1 byte)
                packet[21] = (byte)actionLen;

                // Action (variable length)
                if (actionLen > 0)
                    Array.Copy(actionBytes, 0, packet, 22, actionLen);

                return packet;
            }
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
                // Socket đã đóng
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
                if (data == null || data.Length < 22)
                    return;

                // Invoke event for BattleForm to handle opponent's state
                OnOpponentState?.Invoke(data);
            }
            catch (Exception ex)
            {
                Log($"? Process packet error: {ex.Message}");
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Set player number (1 or 2) - called after joining lobby
        /// </summary>
        public void SetPlayerNumber(int playerNum)
        {
            playerNumber = playerNum;
            Log($"? Player number set: {playerNum}");
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"[UDP] {message}");
        }

        #endregion
    }
}
