using System;
using System.Collections.Concurrent;
using System.Linq;
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
                Log("⚠️ UDP Server already running");
                return;
            }

            try
            {
                udpSocket = new UdpClient(udpPort);
                isRunning = true;
                cts = new CancellationTokenSource();

                receiveTask = Task.Run(() => ReceiveLoop(cts.Token));
                
                // ✅ THÊM: Cleanup task cho timeout matches
                _ = Task.Run(() => CleanupInactiveMatches(cts.Token));

                Log($"✅ UDP Game Server started on port {udpPort}");
            }
            catch (Exception ex)
            {
                Log($"❌ UDP Server start error: {ex.Message}");
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

                Log("🛑 UDP Game Server stopped");
            }
            catch (Exception ex)
            {
                Log($"❌ UDP Server stop error: {ex.Message}");
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

                // If a match for this room already exists, treat CreateMatch as idempotent
                // - update usernames and timestamps
                // - clear stored endpoints so new clients can re-register
                if (activeMatches.TryGetValue(roomCode, out var existing))
                {
                    existing.Player1Username = player1;
                    existing.Player2Username = player2;
                    existing.CreatedAt = DateTime.Now;
                    existing.LastActivity = DateTime.Now;

                    // Remove any stored endpoints associated with this room so reconnect works cleanly
                    var toRemove = playerEndpoints
                        .Where(kvp => kvp.Value.RoomCode == roomCode)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var key in toRemove)
                        playerEndpoints.TryRemove(key, out _);

                    existing.Player1Endpoint = null;
                    existing.Player2Endpoint = null;

                    Log($"?? Match updated/recreated: {roomCode} ({player1} vs {player2})");
                    return (true, $"UDP ready on port {udpPort}");
                }

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
                    try
                    {
                        var result = await udpSocket.ReceiveAsync();
                        _ = Task.Run(() => ProcessPacket(result.Buffer, result.RemoteEndPoint), token);
                    }
                    catch (ObjectDisposedException)
                    {
                        // socket closed while stopping
                        break;
                    }
                    catch (System.Net.Sockets.SocketException sex)
                    {
                        // Transient socket errors (ICMP unreachable, etc.) - log and continue
                        if (!token.IsCancellationRequested && isRunning)
                        {
                            Log($"? UDP receive socket error (recovering): {sex.Message}");
                            try { await Task.Delay(100, token); } catch { }
                            continue;
                        }
                        break;
                    }
                    catch (Exception exInner)
                    {
                        // Unexpected - log and short delay then continue
                        if (!token.IsCancellationRequested && isRunning)
                        {
                            Log($"? UDP receive transient error (recovering): {exInner.Message}");
                            try { await Task.Delay(100, token); } catch { }
                            continue;
                        }
                        break;
                    }
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
                    Log($"⚠️ Invalid packet size: {data?.Length ?? 0} from {senderEndpoint}");
                    return;
                }

                // Parse binary packet: [RoomCode(6)] [PlayerNum(1)] [X(2)] [Y(2)] [Health(1)] [Stamina(1)] [Mana(1)] [ActionLen(1)] [Action(var)]
                // New: Action may encode immediate damage notifications: "DAMAGE:target:damage:resultingHealth"
                string roomCode = System.Text.Encoding.UTF8.GetString(data, 0, 6).TrimEnd('\0');
                int playerNum = data[6];

                // Validate match exists
                if (!activeMatches.TryGetValue(roomCode, out var match))
                {
                    // ⚠️ Match not found - this can happen if game just started and UDP server doesn't know yet
                    // Log sparingly to avoid spam
                    if (DateTime.Now.Millisecond % 5000 < 100) // Log roughly every 5 seconds
                    {
                        Log($"⚠️ Packet for unknown match: {roomCode} from P{playerNum} @ {senderEndpoint}");
                    }
                    return;
                }

                // Register player endpoint if not registered
                string endpointKey = senderEndpoint.ToString();
                if (!playerEndpoints.ContainsKey(endpointKey))
                {
                    playerEndpoints.TryAdd(endpointKey, (roomCode, playerNum));
                    Log($"✅ Registered P{playerNum} endpoint: {senderEndpoint} for room {roomCode}");
                }

                // Update last activity
                match.LastActivity = DateTime.Now;

                // Store sender endpoint for this player
                if (playerNum == 1)
                {
                    if (match.Player1Endpoint == null)
                    {
                        match.Player1Endpoint = senderEndpoint;
                        Log($"✅ P1 endpoint set: {senderEndpoint}");
                    }
                    else if (!match.Player1Endpoint.Equals(senderEndpoint))
                    {
                        // Player 1 changed endpoint (probably reconnected)
                        Log($"🔄 P1 endpoint updated from {match.Player1Endpoint} to {senderEndpoint}");
                        match.Player1Endpoint = senderEndpoint;
                    }
                }
                else if (playerNum == 2)
                {
                    if (match.Player2Endpoint == null)
                    {
                        match.Player2Endpoint = senderEndpoint;
                        Log($"✅ P2 endpoint set: {senderEndpoint}");
                    }
                    else if (!match.Player2Endpoint.Equals(senderEndpoint))
                    {
                        // Player 2 changed endpoint (probably reconnected)
                        Log($"🔄 P2 endpoint updated from {match.Player2Endpoint} to {senderEndpoint}");
                        match.Player2Endpoint = senderEndpoint;
                    }
                }

                // Relay to opponent
                IPEndPoint opponentEndpoint = playerNum == 1 ? match.Player2Endpoint : match.Player1Endpoint;

                if (opponentEndpoint != null)
                {
                    try
                    {
                        // Inspect action field if present and handle damage notifications specially
                        // NOTE: actionLen is stored at byte index 21 and action starts at index 22
                        int actionLenIndex = 21; // matches UDPGameClient.BuildPacket (actionLen @21)
                        if (data.Length > actionLenIndex)
                        {
                            int actionLen = data[actionLenIndex];
                            if (actionLen > 0 && actionLen + 22 <= data.Length)
                            {
                                string action = System.Text.Encoding.UTF8.GetString(data, 22, actionLen);
                                if (action.StartsWith("DAMAGE:", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Forward full packet to opponent so they can apply damage locally immediately
                                    udpSocket.Send(data, data.Length, opponentEndpoint);
                                    Log($"📤 Relayed DAMAGE notification from P{playerNum} to opponent at {opponentEndpoint}");
                                }
                                else
                                {
                                    udpSocket.Send(data, data.Length, opponentEndpoint);
                                }
                            }
                            else
                            {
                                udpSocket.Send(data, data.Length, opponentEndpoint);
                            }
                        }
                        else
                        {
                            udpSocket.Send(data, data.Length, opponentEndpoint);
                        }
                        
                        // ✅ SPARSE LOG - mỗi 50 packets (roughly 2 seconds at 25 FPS)
                        if (DateTime.Now.Millisecond % 2000 < 100)
                        {
                            Log($"📤 Relayed P{playerNum}→P{(playerNum == 1 ? 2 : 1)} to {opponentEndpoint}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"❌ Relay error to {opponentEndpoint}: {ex.Message}");
                    }
                }
                else
                {
                    // ⚠️ Opponent not connected yet - just log sparingly
                    if (DateTime.Now.Millisecond % 3000 < 100)
                    {
                        Log($"⏳ P{playerNum} sent packet but opponent not registered yet");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"❌ ProcessPacket error: {ex.Message}");
            }
        }

        #endregion

        #region Cleanup Task

        // ✅ THÊM: Cleanup matches that haven't received packets in 30 seconds
        private async Task CleanupInactiveMatches(CancellationToken token)
        {
            Log("🧹 Cleanup task started for inactive matches");

            try
            {
                while (!token.IsCancellationRequested && isRunning)
                {
                    await Task.Delay(10000, token); // Check every 10 seconds

                    var now = DateTime.Now;
                    var toRemove = new System.Collections.Generic.List<string>();

                    foreach (var kvp in activeMatches)
                    {
                        var match = kvp.Value;
                        var inactiveSeconds = (now - match.LastActivity).TotalSeconds;

                        if (inactiveSeconds > 30) // 30 seconds timeout
                        {
                            toRemove.Add(kvp.Key);
                            Log($"🧹 Timeout: Match {kvp.Key} inactive for {inactiveSeconds:F1}s - removing");
                        }
                    }

                    foreach (var roomCode in toRemove)
                    {
                        if (activeMatches.TryRemove(roomCode, out _))
                        {
                            // Remove associated endpoints
                            var endpointsToRemove = playerEndpoints
                                .Where(kvp => kvp.Value.RoomCode == roomCode)
                                .Select(kvp => kvp.Key)
                                .ToList();

                            foreach (var endpoint in endpointsToRemove)
                            {
                                playerEndpoints.TryRemove(endpoint, out _);
                            }

                            Log($"✅ Cleaned up match {roomCode} and {endpointsToRemove.Count} endpoints");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            catch (Exception ex)
            {
                if (isRunning)
                    Log($"❌ Cleanup task error: {ex.Message}");
            }

            Log("🧹 Cleanup task stopped");
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
