using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DoAn_NT106.Services;

namespace DoAn_NT106.Client
{
    /// <summary>
    /// UDP Client g?i/nh?n game state binary 30-50ms/l?n
    /// ? Packet structure: [RoomCode(6)] [PlayerNum(1)] [X(2)] [Y(2)] [Health(1)] [Stamina(1)] [Mana(1)] 
    ///                     [Facing(1)] [IsAttacking(1)] [IsParrying(1)] [ActionLen(1)] [Action(var)]
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
        private string currentFacing = "right";        // ? NEW
        private bool currentIsAttacking = false;       // ? NEW
        private bool currentIsParrying = false;        // ? NEW
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
        /// ? NEW: Include Facing, IsAttacking, IsParrying
        /// </summary>
        public void UpdateState(int x, int y, int health, int stamina, int mana, string action, 
            string facing = "right", bool isAttacking = false, bool isParrying = false)
        {
            lock (stateLock)
            {
                currentX = x;
                currentY = y;
                currentHealth = health;
                currentStamina = stamina;
                currentMana = mana;
                currentAction = action ?? "stand";
                currentFacing = facing ?? "right";      // ? NEW
                currentIsAttacking = isAttacking;       // ? NEW
                currentIsParrying = isParrying;         // ? NEW
            }
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

                    // 30-50ms interval (~ 20-33 FPS)
                    await Task.Delay(40, token); // 40ms = 25 FPS
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
                // ? NEW PACKET STRUCTURE:
                // [RoomCode(6)] [PlayerNum(1)] [X(2)] [Y(2)] [Health(1)] [Stamina(1)] [Mana(1)] 
                // [Facing(1)] [IsAttacking(1)] [IsParrying(1)] [ActionLen(1)] [Action(var)]

                byte[] actionBytes = System.Text.Encoding.UTF8.GetBytes(currentAction);
                int actionLen = Math.Min(actionBytes.Length, 20); // Max 20 chars

                byte[] packet = new byte[17 + actionLen];

                // RoomCode (6 bytes, padded with nulls)
                byte[] roomCodeBytes = System.Text.Encoding.UTF8.GetBytes(roomCode.PadRight(6, '\0'));
                Array.Copy(roomCodeBytes, 0, packet, 0, 6);

                // PlayerNum (1 byte)
                packet[6] = (byte)playerNumber;

                // X (2 bytes, little-endian)
                packet[7] = (byte)(currentX & 0xFF);
                packet[8] = (byte)((currentX >> 8) & 0xFF);

                // Y (2 bytes)
                packet[9] = (byte)(currentY & 0xFF);
                packet[10] = (byte)((currentY >> 8) & 0xFF);

                // Health (1 byte)
                packet[11] = (byte)Math.Clamp(currentHealth, 0, 255);

                // Stamina (1 byte)
                packet[12] = (byte)Math.Clamp(currentStamina, 0, 255);

                // Mana (1 byte)
                packet[13] = (byte)Math.Clamp(currentMana, 0, 255);

                // ? NEW: Facing (1 byte) - 'L' = left, 'R' = right
                packet[14] = (byte)(currentFacing == "left" ? 'L' : 'R');

                // ? NEW: IsAttacking (1 byte) - 1 = true, 0 = false
                packet[15] = currentIsAttacking ? (byte)1 : (byte)0;

                // ? NEW: IsParrying (1 byte) - 1 = true, 0 = false
                packet[16] = currentIsParrying ? (byte)1 : (byte)0;

                // ActionLen (1 byte)
                packet[17] = (byte)actionLen;

                // Action (variable length)
                if (actionLen > 0)
                    Array.Copy(actionBytes, 0, packet, 18, actionLen);

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
                // Socket ?ã ?óng
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
                if (data == null || data.Length < 13)
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
            this.playerNumber = playerNum;
            Log($"? Player number set: {playerNum}");
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"[UDP] {message}");
        }

        #endregion
    }
}
