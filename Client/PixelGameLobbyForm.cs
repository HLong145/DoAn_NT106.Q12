using DoAn_NT106;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PixelGameLobby
{
    public partial class GameLobbyForm : Form
    {
        private List<ChatMessage> messages = new List<ChatMessage>();
        private List<Player> players = new List<Player>();
        private string roomCode;
        private string username;
        private string token;

        // Màu sắc
        private Color primaryBrown = Color.FromArgb(160, 82, 45);    // #a0522d
        private Color darkBrown = Color.FromArgb(101, 67, 51);       // #654321
        private Color darkerBrown = Color.FromArgb(74, 50, 25);      // #4a3219
        private Color goldColor = Color.FromArgb(255, 215, 0);       // #ffd700
        private Color darkGold = Color.FromArgb(139, 69, 19);        // #8b4513
        private Color readyColor = Color.FromArgb(100, 200, 100);    // Xanh lá - Ready
        private Color notReadyColor = Color.FromArgb(255, 0, 0);     // Đỏ - Not Ready

        public GameLobbyForm(string roomCode, string username, string token)
        {
            InitializeComponent();
            this.roomCode = roomCode ?? GenerateRoomCode();
            this.username = username;
            this.token = token;
            InitializeData();
            SetupPixelStyling();
        }

        public GameLobbyForm(string roomCode = null) : this(roomCode, "Guest", "")
        {
        }

        private string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var code = new char[6];
            for (int i = 0; i < 6; i++)
            {
                code[i] = chars[random.Next(chars.Length)];
            }
            return new string(code);
        }

        private void InitializeData()
        {
            // Sử dụng username thực tế
            players.Add(new Player { Id = 1, Name = username, Status = "Ready", IsReady = true });
            players.Add(new Player { Id = 2, Name = "Waiting for player...", Status = "Not Ready", IsReady = false });

            messages.Add(new ChatMessage
            {
                Id = 1,
                Player = "System",
                Message = $"Welcome {username} to the lobby!",
                Time = DateTime.Now.ToString("HH:mm")
            });
            messages.Add(new ChatMessage
            {
                Id = 2,
                Player = "System",
                Message = $"Room created! Code: {roomCode}",
                Time = DateTime.Now.ToString("HH:mm")
            });

            UpdatePlayersDisplay();
            UpdateChatDisplay();
            AddTestButton();
        }

        private void AddTestButton()
        {
            var btnTest = new Button
            {
                Text = "TEST: Add Opponent",
                Size = new Size(230, 35),
                Location = new Point(10, 265),
                BackColor = Color.Orange,
                ForeColor = Color.Black,
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };

            btnTest.Click += (s, e) =>
            {
                if (players.Count >= 2 && players[1].Name == "Waiting for player...")
                {
                    players[1].Name = "AI_Opponent";
                    players[1].IsReady = true;
                    players[1].Status = "Ready";

                    messages.Add(new ChatMessage
                    {
                        Id = messages.Count + 1,
                        Player = "System",
                        Message = "AI_Opponent has joined and is ready!",
                        Time = DateTime.Now.ToString("HH:mm")
                    });

                    UpdatePlayersDisplay();
                    UpdateChatDisplay();

                    MessageBox.Show("AI Opponent đã tham gia và sẵn sàng!", "Test");
                }
            };

            this.Controls.Add(btnTest); // Thêm trực tiếp vào Form thay vì playersPanel
            btnTest.BringToFront(); // Đưa lên trên cùng
        }

        private void SetupPixelStyling()
        {
            this.Font = new Font("Courier New", 10, FontStyle.Bold);
            this.BackColor = primaryBrown;
            this.Text = $"Game Lobby - Room: {roomCode}";

            // Cập nhật room code label
            roomCodeValueLabel.Text = roomCode;
            player1NameLabel.Text = username;
        }

        private void UpdatePlayersDisplay()
        {
            // Cập nhật thông tin người chơi
            if (players.Count > 0)
            {
                player1NameLabel.Text = players[0].Name;
                player1StatusLabel.Text = players[0].IsReady ? "#Ready" : "#Not Ready";
                player1StatusLabel.ForeColor = players[0].IsReady ? readyColor : notReadyColor;
            }

            if (players.Count > 1)
            {
                player2NameLabel.Text = players[1].Name;
                player2StatusLabel.Text = players[1].IsReady ? "#Ready" : "#Not Ready";
                player2StatusLabel.ForeColor = players[1].IsReady ? readyColor : notReadyColor;
            }

            // Cập nhật room code
            roomCodeValueLabel.Text = roomCode;
        }

        private void UpdateChatDisplay()
        {
            chatMessagesPanel.Controls.Clear();

            int yPos = 5;
            foreach (var message in messages)
            {
                var messageLabel = new Label
                {
                    Text = $"[B&B] {message.Player}: {message.Message}",
                    ForeColor = message.Player == "System" ? Color.Orange : goldColor,
                    Font = new Font("Courier New", 9, FontStyle.Bold),
                    Location = new Point(5, yPos),
                    Size = new Size(chatMessagesPanel.Width - 10, 20),
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent
                };

                chatMessagesPanel.Controls.Add(messageLabel);
                yPos += 25;
            }

            // Auto scroll xuống dưới
            chatMessagesPanel.VerticalScroll.Value = chatMessagesPanel.VerticalScroll.Maximum;
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(messageTextBox.Text))
            {
                var newMessage = new ChatMessage
                {
                    Id = messages.Count + 1,
                    Player = "You",
                    Message = messageTextBox.Text,
                    Time = DateTime.Now.ToString("HH:mm")
                };

                messages.Add(newMessage);
                messageTextBox.Clear();
                UpdateChatDisplay();
            }
        }

        private void notReadyButton_Click(object sender, EventArgs e)
        {
            if (notReadyButton.Text == "NOT READY")
            {
                notReadyButton.Text = "READY";
                notReadyButton.BackColor = readyColor;
                notReadyButton.ForeColor = Color.Black;

                // Cập nhật trạng thái người chơi
                if (players.Count > 0)
                {
                    players[0].IsReady = true;
                    players[0].Status = "Ready";
                }
            }
            else
            {
                notReadyButton.Text = "NOT READY";
                notReadyButton.BackColor = notReadyColor;
                notReadyButton.ForeColor = Color.White;

                // Cập nhật trạng thái người chơi
                if (players.Count > 0)
                {
                    players[0].IsReady = false;
                    players[0].Status = "Not Ready";
                }
            }

            UpdatePlayersDisplay();
        }

        private void startGameButton_Click(object sender, EventArgs e)
        {
            bool allReady = true;
            foreach (var player in players)
            {
                if (!player.IsReady)
                {
                    allReady = false;
                    break;
                }
            }

            if (allReady)
            {
                string opponentName = "Opponent";
                foreach (var player in players)
                {
                    if (player.Name != username)
                    {
                        opponentName = player.Name;
                        break;
                    }
                }

                // Mở form chọn tướng thay vì vào thẳng BattleForm
                CharacterSelectForm selectForm = new CharacterSelectForm(username, token, roomCode, opponentName, true);
                selectForm.FormClosed += (s, args) =>
                {
                    if (selectForm.DialogResult != DialogResult.OK)
                    {
                        this.Show();
                    }
                };
                selectForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Not all players are ready!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void leaveRoomButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to leave the room?", "Leave Room",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void messageTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                sendButton_Click(sender, e);
                e.Handled = true;
            }
        }

        private void CopyCodeButton_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(roomCode);
            messages.Add(new ChatMessage
            {
                Id = messages.Count + 1,
                Player = "System",
                Message = "Room code copied to clipboard!",
                Time = DateTime.Now.ToString("HH:mm")
            });
            UpdateChatDisplay();
        }

        // Custom painting
        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            Control panel = sender as Control;
            if (panel == null) return;

            using (Pen darkPen = new Pen(darkerBrown, 4))
            using (Pen lightPen = new Pen(Color.FromArgb(120, 60, 30), 2))
            {
                e.Graphics.DrawRectangle(darkPen, 0, 0, panel.Width - 1, panel.Height - 1);
                e.Graphics.DrawRectangle(lightPen, 2, 2, panel.Width - 5, panel.Height - 5);
            }
        }

        private void Button_Paint(object sender, PaintEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            Color backgroundColor = darkGold;
            if (button == notReadyButton)
            {
                backgroundColor = button.Text == "READY" ? readyColor : notReadyColor;
            }

            e.Graphics.FillRectangle(new SolidBrush(backgroundColor), 0, 0, button.Width, button.Height);

            Color textColor = button == notReadyButton && button.Text == "NOT READY" ? Color.White : goldColor;
            TextRenderer.DrawText(e.Graphics, button.Text, button.Font,
                new Rectangle(0, 0, button.Width, button.Height), textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            e.Graphics.DrawRectangle(new Pen(darkerBrown, 2), 0, 0, button.Width - 1, button.Height - 1);
        }
    }

    public class ChatMessage
    {
        public int Id { get; set; }
        public string Player { get; set; }
        public string Message { get; set; }
        public string Time { get; set; }
    }

    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public bool IsReady { get; set; }
    }
}