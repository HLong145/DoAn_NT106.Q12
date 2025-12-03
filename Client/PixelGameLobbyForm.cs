using DoAn_NT106;
using DoAn_NT106.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PixelGameLobby
{
    public partial class GameLobbyForm : Form
    {
        // ===========================
        // FIELDS
        // ===========================
        private List<ChatMessage> messages = new List<ChatMessage>();
        private List<Player> players = new List<Player>();

        private string roomCode;
        private string username;
        private string token;
        private bool isReady = false;
        private string opponentName = null;
        private bool opponentReady = false;

        // TCP Client - dùng singleton
        private PersistentTcpClient TcpClient => PersistentTcpClient.Instance;

        // Màu sắc
        private Color primaryBrown = Color.FromArgb(160, 82, 45);
        private Color darkBrown = Color.FromArgb(101, 67, 51);
        private Color darkerBrown = Color.FromArgb(74, 50, 25);
        private Color goldColor = Color.FromArgb(255, 215, 0);
        private Color readyColor = Color.FromArgb(100, 200, 100);
        private Color notReadyColor = Color.FromArgb(255, 100, 100);

        // ===========================
        // CONSTRUCTOR
        // ===========================
        public GameLobbyForm(string roomCode, string username, string token)
        {
            InitializeComponent();

            this.roomCode = roomCode ?? "000000";
            this.username = username;
            this.token = token;

            // Khởi tạo players list
            InitializePlayers();

            // Setup UI
            SetupPixelStyling();

            // Events
            this.Load += GameLobbyForm_Load;
            this.FormClosing += GameLobbyForm_FormClosing;
        }

        public GameLobbyForm(string roomCode = null) : this(roomCode, "Guest", "")
        {
        }

        // ===========================
        // FORM EVENTS
        // ===========================
        private async void GameLobbyForm_Load(object sender, EventArgs e)
        {
            // Subscribe to broadcasts
            TcpClient.OnBroadcast += HandleBroadcast;
            TcpClient.OnDisconnected += HandleDisconnected;

            // Join lobby
            await JoinLobbyAsync();
        }

        private async void GameLobbyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Unsubscribe events
            TcpClient.OnBroadcast -= HandleBroadcast;
            TcpClient.OnDisconnected -= HandleDisconnected;

            try
            {
                // ✅ FIX: Leave cả Lobby VÀ Room
                Console.WriteLine($"[GameLobby] Leaving room {roomCode}...");

                // Gọi đồng bộ để đảm bảo cleanup xong trước khi form đóng
                var leaveTask = Task.Run(async () =>
                {
                    try
                    {
                        await TcpClient.LobbyLeaveAsync(roomCode, username);
                        await TcpClient.LeaveRoomAsync(roomCode, username);
                        Console.WriteLine($"[GameLobby] Successfully left room {roomCode}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[GameLobby] Leave error: {ex.Message}");
                    }
                });

                // Đợi tối đa 2 giây
                leaveTask.Wait(2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLobby] FormClosing error: {ex.Message}");
            }
        }

        // ===========================
        // INITIALIZE
        // ===========================
        private void InitializePlayers()
        {
            players.Clear();
            players.Add(new Player { Id = 1, Name = "Loading...", Status = "Not Ready", IsReady = false });
            players.Add(new Player { Id = 2, Name = "Waiting...", Status = "Not Ready", IsReady = false });

            messages.Clear();
            messages.Add(new ChatMessage
            {
                Id = 1,
                Player = "System",
                Message = $"Connecting to room {roomCode}...",
                Time = DateTime.Now.ToString("HH:mm")
            });
        }

        private void SetupPixelStyling()
        {
            // Room code display
            roomCodeValueLabel.Text = roomCode;

            // Ready button initial state
            notReadyButton.Text = "NOT READY";
            notReadyButton.BackColor = notReadyColor;

            // Update displays
            UpdatePlayersDisplay();
            UpdateChatDisplay();
        }

        // ===========================
        // JOIN LOBBY
        // ===========================
        private async Task JoinLobbyAsync()
        {
            try
            {
                Console.WriteLine($"[GameLobby] Joining lobby {roomCode} as {username}...");

                var response = await TcpClient.LobbyJoinAsync(roomCode, username, token);

                if (response.Success)
                {
                    Console.WriteLine($"[GameLobby] Joined successfully!");

                    // Parse response data
                    if (response.RawData.ValueKind != JsonValueKind.Undefined)
                    {
                        UpdateFromServerState(response.RawData);
                        LoadChatHistory(response.RawData);
                    }

                    AddSystemMessage($"Connected to room {roomCode}!");
                }
                else
                {
                    MessageBox.Show($"Failed to join lobby: {response.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLobby] Join error: {ex.Message}");
                MessageBox.Show($"Error joining lobby: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        // ===========================
        // HANDLE BROADCASTS
        // ===========================
        private void HandleBroadcast(string action, JsonElement data)
        {
            // Thread-safe UI update
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new Action(() => HandleBroadcast(action, data)));
                }
                catch { }
                return;
            }

            Console.WriteLine($"[GameLobby] Broadcast received: {action}");

            switch (action)
            {
                case "LOBBY_STATE_UPDATE":
                    UpdateFromServerState(data);
                    break;

                case "LOBBY_PLAYER_LEFT":
                    HandlePlayerLeft(data);
                    break;

                case "LOBBY_CHAT_MESSAGE":
                    HandleChatMessage(data);
                    break;

                case "LOBBY_START_GAME":
                    HandleStartGame(data);
                    break;
            }
        }

        private void UpdateFromServerState(JsonElement data)
        {
            try
            {
                // Parse player info
                string player1 = GetStringOrNull(data, "player1");
                string player2 = GetStringOrNull(data, "player2");
                bool player1Ready = GetBoolOrFalse(data, "player1Ready");
                bool player2Ready = GetBoolOrFalse(data, "player2Ready");

                // Update Player 1
                if (!string.IsNullOrEmpty(player1))
                {
                    players[0].Name = player1 + (player1 == username ? " (You)" : "");
                    players[0].IsReady = player1Ready;
                    players[0].Status = player1Ready ? "Ready" : "Not Ready";

                    if (player1 == username)
                    {
                        isReady = player1Ready;
                    }
                    else
                    {
                        opponentName = player1;
                        opponentReady = player1Ready;
                    }
                }
                else
                {
                    players[0].Name = "Waiting for player...";
                    players[0].IsReady = false;
                    players[0].Status = "Not Ready";
                }

                // Update Player 2
                if (!string.IsNullOrEmpty(player2))
                {
                    players[1].Name = player2 + (player2 == username ? " (You)" : "");
                    players[1].IsReady = player2Ready;
                    players[1].Status = player2Ready ? "Ready" : "Not Ready";

                    if (player2 == username)
                    {
                        isReady = player2Ready;
                    }
                    else
                    {
                        opponentName = player2;
                        opponentReady = player2Ready;
                    }
                }
                else
                {
                    players[1].Name = "Waiting for player...";
                    players[1].IsReady = false;
                    players[1].Status = "Not Ready";
                }

                // Update UI
                UpdatePlayersDisplay();
                UpdateReadyButton();

                Console.WriteLine($"[GameLobby] State: P1={player1}({player1Ready}), P2={player2}({player2Ready})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLobby] UpdateFromServerState error: {ex.Message}");
            }
        }

        private void HandlePlayerLeft(JsonElement data)
        {
            try
            {
                string leftUsername = GetStringOrNull(data, "username");
                if (!string.IsNullOrEmpty(leftUsername))
                {
                    AddSystemMessage($"{leftUsername} has left the lobby.");

                    if (leftUsername == opponentName)
                    {
                        opponentName = null;
                        opponentReady = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLobby] HandlePlayerLeft error: {ex.Message}");
            }
        }

        private void HandleChatMessage(JsonElement data)
        {
            try
            {
                string msgUsername = data.GetProperty("username").GetString();
                string message = data.GetProperty("message").GetString();
                string timestamp = data.GetProperty("timestamp").GetString();

                messages.Add(new ChatMessage
                {
                    Id = messages.Count + 1,
                    Player = msgUsername,
                    Message = message,
                    Time = timestamp
                });

                UpdateChatDisplay();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLobby] HandleChatMessage error: {ex.Message}");
            }
        }

        private void HandleStartGame(JsonElement data)
        {
            try
            {
                AddSystemMessage("Both players ready! Starting game...");

                // Đợi 1 giây để user thấy message
                var timer = new System.Windows.Forms.Timer { Interval = 1000 };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    timer.Dispose();

                    // Mở Character Select Form
                    string opponent = opponentName ?? "Opponent";
                    var selectForm = new CharacterSelectForm(username, token, roomCode, opponent, true);
                    selectForm.FormClosed += (s2, args) =>
                    {
                        if (selectForm.DialogResult != DialogResult.OK)
                        {
                            this.Show();
                        }
                        else
                        {
                            this.Close();
                        }
                    };
                    selectForm.Show();
                    this.Hide();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLobby] HandleStartGame error: {ex.Message}");
            }
        }

        private void LoadChatHistory(JsonElement data)
        {
            try
            {
                if (data.TryGetProperty("chatHistory", out var historyEl))
                {
                    foreach (var item in historyEl.EnumerateArray())
                    {
                        messages.Add(new ChatMessage
                        {
                            Id = messages.Count + 1,
                            Player = item.GetProperty("username").GetString(),
                            Message = item.GetProperty("message").GetString(),
                            Time = item.GetProperty("timestamp").GetString()
                        });
                    }
                    UpdateChatDisplay();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLobby] LoadChatHistory error: {ex.Message}");
            }
        }

        private void HandleDisconnected(string reason)
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new Action(() => HandleDisconnected(reason)));
                }
                catch { }
                return;
            }

            MessageBox.Show($"Disconnected from server: {reason}", "Disconnected",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            this.Close();
        }

        // ===========================
        // UI UPDATE METHODS
        // ===========================
        private void UpdatePlayersDisplay()
        {
            if (players.Count >= 2)
            {
                // Player 1
                player1NameLabel.Text = players[0].Name;
                player1StatusLabel.Text = players[0].IsReady ? "#Ready" : "#Not Ready";
                player1StatusLabel.ForeColor = players[0].IsReady ? Color.LimeGreen : Color.Red;

                // Player 2
                player2NameLabel.Text = players[1].Name;
                player2StatusLabel.Text = players[1].IsReady ? "#Ready" : "#Not Ready";
                player2StatusLabel.ForeColor = players[1].IsReady ? Color.LimeGreen : Color.Red;
            }
        }

        private void UpdateChatDisplay()
        {
            chatMessagesPanel.Controls.Clear();

            int yOffset = 5;
            foreach (var msg in messages)
            {
                var msgLabel = new Label
                {
                    Text = $"[{msg.Time}] {msg.Player}: {msg.Message}",
                    AutoSize = false,
                    Width = chatMessagesPanel.Width - 20,
                    Height = 20,
                    Location = new Point(5, yOffset),
                    ForeColor = msg.Player == "System" ? Color.Yellow : goldColor,
                    Font = new Font("Courier New", 9, FontStyle.Bold),
                    BackColor = Color.Transparent
                };
                chatMessagesPanel.Controls.Add(msgLabel);
                yOffset += 22;
            }

            // Scroll to bottom
            if (chatMessagesPanel.VerticalScroll.Visible)
            {
                chatMessagesPanel.VerticalScroll.Value = chatMessagesPanel.VerticalScroll.Maximum;
            }
        }

        private void UpdateReadyButton()
        {
            notReadyButton.Text = isReady ? "READY" : "NOT READY";
            notReadyButton.BackColor = isReady ? readyColor : notReadyColor;
            notReadyButton.ForeColor = isReady ? Color.Black : Color.White;
        }

        private void AddSystemMessage(string message)
        {
            messages.Add(new ChatMessage
            {
                Id = messages.Count + 1,
                Player = "System",
                Message = message,
                Time = DateTime.Now.ToString("HH:mm")
            });
            UpdateChatDisplay();
        }

        // ===========================
        // BUTTON CLICK HANDLERS
        // ===========================
        private async void notReadyButton_Click(object sender, EventArgs e)
        {
            try
            {
                notReadyButton.Enabled = false;

                bool newReadyState = !isReady;
                var response = await TcpClient.LobbySetReadyAsync(roomCode, username, newReadyState);

                if (response.Success)
                {
                    isReady = newReadyState;
                    UpdateReadyButton();

                    // Update local player state
                    foreach (var player in players)
                    {
                        if (player.Name.Contains(username))
                        {
                            player.IsReady = isReady;
                            player.Status = isReady ? "Ready" : "Not Ready";
                            break;
                        }
                    }
                    UpdatePlayersDisplay();
                }
                else
                {
                    MessageBox.Show($"Failed to update status: {response.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                notReadyButton.Enabled = true;
            }
        }

        private void startGameButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Game will start automatically when both players are ready!",
                "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void leaveRoomButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to leave?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // ✅ Async calls
                    await TcpClient.LobbyLeaveAsync(roomCode, username);
                    await TcpClient.LeaveRoomAsync(roomCode, username);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GameLobby] Leave error: {ex.Message}");
                }

                this.Close();
            }
        }
        private async void sendButton_Click(object sender, EventArgs e)
        {
            string message = messageTextBox.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            try
            {
                sendButton.Enabled = false;
                messageTextBox.Enabled = false;

                var response = await TcpClient.LobbySendChatAsync(roomCode, username, message);

                if (response.Success)
                {
                    messageTextBox.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLobby] Send chat error: {ex.Message}");
            }
            finally
            {
                sendButton.Enabled = true;
                messageTextBox.Enabled = true;
                messageTextBox.Focus();
            }
        }

        private void messageTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                sendButton_Click(sender, e);
            }
        }

        private void CopyCodeButton_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(roomCode);
            MessageBox.Show("Room code copied!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ===========================
        // HELPER METHODS
        // ===========================
        private string GetStringOrNull(JsonElement data, string propertyName)
        {
            if (data.TryGetProperty(propertyName, out var prop) && prop.ValueKind != JsonValueKind.Null)
            {
                return prop.GetString();
            }
            return null;
        }

        private bool GetBoolOrFalse(JsonElement data, string propertyName)
        {
            if (data.TryGetProperty(propertyName, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.True) return true;
                if (prop.ValueKind == JsonValueKind.False) return false;
            }
            return false;
        }

        // ===========================
        // PAINT EVENTS (từ Designer)
        // ===========================
        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            var panel = sender as Panel;
            if (panel != null)
            {
                ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle,
                    darkerBrown, 3, ButtonBorderStyle.Solid,
                    darkerBrown, 3, ButtonBorderStyle.Solid,
                    darkerBrown, 3, ButtonBorderStyle.Solid,
                    darkerBrown, 3, ButtonBorderStyle.Solid);
            }
        }

        private void Button_Paint(object sender, PaintEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                ControlPaint.DrawBorder(e.Graphics, btn.ClientRectangle,
                    Color.Black, 2, ButtonBorderStyle.Solid,
                    Color.Black, 2, ButtonBorderStyle.Solid,
                    Color.Black, 2, ButtonBorderStyle.Solid,
                    Color.Black, 2, ButtonBorderStyle.Solid);
            }
        }
    }

    // ===========================
    // DATA CLASSES
    // ===========================
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public bool IsReady { get; set; }
    }

    public class ChatMessage
    {
        public int Id { get; set; }
        public string Player { get; set; }
        public string Message { get; set; }
        public string Time { get; set; }
    }
}