using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DoAn_NT106.Server
{
    /// <summary>
    /// UDP Server qu?n lý relay game state gi?a players trong match
    /// </summary>
    public class UDPGameServer
    {
        #region Fields

        private UdpClient udpSocket;
        private int udpPort;
        private bool isRunning = false;
        private CancellationTokenSource cts;
        private Task receiveTask;

        // Match management: RoomCode -> MatchSession
        private ConcurrentDictionary<string, MatchSession> activeMatches;

        // Player endpoint tracking: EndPoint -> (RoomCode, PlayerNumber)
        private ConcurrentDictionary<string, (string RoomCode, int PlayerNum)> playerEndpoints;

        public event Action<string> OnLog;

        #endregion

        #region Constructor

        public UDPGameServer(int port = 5000)
        {
            this.udpPort = port;
            this.activeMatches = new ConcurrentDictionary<string, MatchSession>();
            this.playerEndpoints = new ConcurrentDictionary<string, (string, int)>();
        }

        #endregion

        #region Start/Stop

        public void Start()
        {
            if (isRunning)
            {
                Log("?? UDP Server already running");
                return;
            }

            try
            {
                udpSocket = new UdpClient(udpPort);
                isRunning = true;
                cts = new CancellationTokenSource();

                receiveTask = Task.Run(() => ReceiveLoop(cts.Token));

                Log($"? UDP Game Server started on port {udpPort}");
            }
            catch (Exception ex)
            {
                Log($"? UDP Server start error: {ex.Message}");
                throw;
            }
        }

        public void Stop()
        {
            if (!isRunning) return;

            try
            {
                isRunning = false;
                cts?.Cancel();
                udpSocket?.Close();

                receiveTask?.Wait(1000);

                activeMatches.Clear();
                playerEndpoints.Clear();

                Log("?? UDP Game Server stopped");
            }
            catch (Exception ex)
            {
                Log($"? UDP Server stop error: {ex.Message}");
            }
        }

        #endregion

        #region Match Management

        /// <summary>
        /// T?o match session khi game b?t ??u
        /// </summary>
        public (bool Success, string Message) CreateMatch(string roomCode, string player1, string player2)
        {
            try
            {
                var match = new MatchSession
                {
                    RoomCode = roomCode,
                    Player1Username = player1,
                    Player2Username = player2,
                    CreatedAt = DateTime.Now,
                    LastActivity = DateTime.Now
                };

                if (activeMatches.TryAdd(roomCode, match))
                {
                    Log($"?? Match created: {roomCode} ({player1} vs {player2})");
                    return (true, $"UDP ready on port {udpPort}");
                }

                return (false, "Failed to create match");
            }
            catch (Exception ex)
            {
                Log($"? CreateMatch error: {ex.Message}");
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// K?t thúc match và d?n d?p
        /// </summary>
        public (bool Success, string Message) EndMatch(string roomCode)
        {
            try
            {
                if (activeMatches.TryRemove(roomCode, out var match))
                {
                    // Remove player endpoints
                    var toRemove = new System.Collections.Generic.List<string>();
                    foreach (var kvp in playerEndpoints)
                    {
                        if (kvp.Value.RoomCode == roomCode)
                            toRemove.Add(kvp.Key);
                    }

                    foreach (var key in toRemove)
                        playerEndpoints.TryRemove(key, out _);

                    Log($"? Match ended: {roomCode}");
                    return (true, "Match ended");
                }

                return (false, "Match not found");
            }
            catch (Exception ex)
            {
                Log($"? EndMatch error: {ex.Message}");
                return (false, ex.Message);
            }
        }

        #endregion

        #region UDP Receive & Relay

        private async Task ReceiveLoop(CancellationToken token)
        {
            Log("?? UDP receive loop started");

            try
            {
                while (!token.IsCancellationRequested && isRunning)
                {
                    var result = await udpSocket.ReceiveAsync();
                    _ = Task.Run(() => ProcessPacket(result.Buffer, result.RemoteEndPoint), token);
                }
            }
            catch (ObjectDisposedException)
            {
                // Socket ?ã ?óng
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                    Log($"? UDP receive error: {ex.Message}");
            }

            Log("?? UDP receive loop stopped");
        }

        private void ProcessPacket(byte[] data, IPEndPoint senderEndpoint)
        {
            try
            {
                if (data == null || data.Length < 10)
                {
                    Log($"?? Invalid packet size: {data?.Length ?? 0}");
                    return;
                }

                // Parse binary packet: [RoomCode(6)] [PlayerNum(1)] [X(2)] [Y(2)] [Health(1)] [Stamina(1)] [Mana(1)] [ActionLen(1)] [Action(var)]
                string roomCode = System.Text.Encoding.UTF8.GetString(data, 0, 6).TrimEnd('\0');
                int playerNum = data[6];

                // Validate match exists
                if (!activeMatches.TryGetValue(roomCode, out var match))
                {
                    Log($"?? Packet for unknown match: {roomCode}");
                    return;
                }

                // Register player endpoint if not registered
                string endpointKey = senderEndpoint.ToString();
                playerEndpoints.TryAdd(endpointKey, (roomCode, playerNum));

                // Update last activity
                match.LastActivity = DateTime.Now;

                // Store sender endpoint for this player
                if (playerNum == 1)
                    match.Player1Endpoint = senderEndpoint;
                else if (playerNum == 2)
                    match.Player2Endpoint = senderEndpoint;

                // Relay to opponent
                IPEndPoint opponentEndpoint = playerNum == 1 ? match.Player2Endpoint : match.Player1Endpoint;

                if (opponentEndpoint != null)
                {
                    udpSocket.Send(data, data.Length, opponentEndpoint);
                }
            }
            catch (Exception ex)
            {
                Log($"? ProcessPacket error: {ex.Message}");
            }
        }

        #endregion

        #region Logging

        private void Log(string message)
        {
            OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        #endregion

        #region Data Classes

        public class MatchSession
        {
            public string RoomCode { get; set; }
            public string Player1Username { get; set; }
            public string Player2Username { get; set; }
            public IPEndPoint Player1Endpoint { get; set; }
            public IPEndPoint Player2Endpoint { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastActivity { get; set; }
        }

        #endregion
    }
}
