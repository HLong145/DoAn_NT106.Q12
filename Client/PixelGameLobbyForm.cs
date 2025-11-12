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

        // Màu sắc chính xác từ ảnh gốc
        private Color primaryBrown = Color.FromArgb(160, 82, 45);    // #a0522d
        private Color darkBrown = Color.FromArgb(101, 67, 51);       // #654321
        private Color darkerBrown = Color.FromArgb(74, 50, 25);      // #4a3219
        private Color goldColor = Color.FromArgb(255, 215, 0);       // #ffd700
        private Color darkGold = Color.FromArgb(139, 69, 19);        // #8b4513
        private Color readyColor = Color.FromArgb(100, 200, 100);    // Xanh lá - Ready
        private Color notReadyColor = Color.FromArgb(255, 0, 0);     // Đỏ - Not Ready

        public GameLobbyForm(string roomCode = null)
        {
            InitializeComponent();
            this.roomCode = roomCode ?? GenerateRoomCode();
            InitializeData();
            SetupPixelStyling();
        }

        private string GenerateRoomCode()
        {
            // Tạo mã phòng ngẫu nhiên 6 ký tự
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
            // CHỈ có 2 người chơi, không có slot trống
            players.Add(new Player { Id = 1, Name = "Player 1", Status = "Ready", IsReady = true });
            players.Add(new Player { Id = 2, Name = "Player 2", Status = "Not Ready", IsReady = false });

            messages.Add(new ChatMessage { Id = 1, Player = "Player 1", Message = "Hello everyone!", Time = "10:30" });
            messages.Add(new ChatMessage { Id = 2, Player = "Player 2", Message = "Ready to start!", Time = "10:31" });
            messages.Add(new ChatMessage
            {
                Id = 3,
                Player = "System",
                Message = $"Room created! Code: {roomCode}",
                Time = "10:32"
            });

            UpdatePlayersDisplay();
            UpdateChatDisplay();
        }

        private void SetupPixelStyling()
        {
            // Font pixelated
            this.Font = new Font("Courier New", 10, FontStyle.Bold);
            this.BackColor = primaryBrown;
            this.Text = $"Game Lobby - Room: {roomCode}"; // Hiển thị mã phòng trên title bar
        }

        private void UpdatePlayersDisplay()
        {
            playersPanel.Controls.Clear();

            // Tiêu đề PLAYERS với số lượng 2/2
            var playersTitle = new Label
            {
                Text = "PLAYERS (2/2)",
                ForeColor = goldColor,
                Font = new Font("Courier New", 11, FontStyle.Bold),
                Size = new Size(200, 25),
                Location = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleLeft
            };
            playersPanel.Controls.Add(playersTitle);

            // CHỈ hiển thị 2 người chơi
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];

                var playerPanel = new Panel
                {
                    Size = new Size(200, 40),
                    Location = new Point(10, 40 + i * 45),
                    BackColor = darkBrown,
                    BorderStyle = BorderStyle.FixedSingle
                };


                var playerName = new Label
                {
                    Text = player.Name,
                    ForeColor = goldColor,
                    Font = new Font("Courier New", 11, FontStyle.Bold),
                    Location = new Point(5, 5),
                    Size = new Size(120, 20),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Status với màu sắc tương ứng
                var playerStatus = new Label
                {
                    Text = player.IsReady ? "#Ready" : "#Not Ready",
                    ForeColor = player.IsReady ? readyColor : notReadyColor,
                    Font = new Font("Courier New", 11, FontStyle.Bold),
                    Location = new Point(130, 5),
                    Size = new Size(65, 20),
                    TextAlign = ContentAlignment.MiddleRight
                };

                playerPanel.Controls.Add(playerName);
                playerPanel.Controls.Add(playerStatus);
                playersPanel.Controls.Add(playerPanel);
            }

            // Thêm thông tin room code
            var roomCodePanel = new Panel
            {
                Size = new Size(200, 60),
                Location = new Point(10, 140),
                BackColor = darkBrown,
                BorderStyle = BorderStyle.FixedSingle
            };
            roomCodePanel.Paint += Panel_Paint;

            var roomCodeTitle = new Label
            {
                Text = "ROOM CODE",
                ForeColor = goldColor,
                Font = new Font("Courier New", 11, FontStyle.Bold),
                Location = new Point(5, 8),
                Size = new Size(190, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var roomCodeLabel = new Label
            {
                Text = roomCode,
                ForeColor = Color.White,
                Font = new Font("Courier New", 14, FontStyle.Bold),
                Location = new Point(5, 30),
                Size = new Size(190, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(80, 60, 40)
            };

            roomCodePanel.Controls.Add(roomCodeTitle);
            roomCodePanel.Controls.Add(roomCodeLabel);
            playersPanel.Controls.Add(roomCodePanel);

            // Thêm nút copy room code
            var copyCodeButton = new Button
            {
                Text = "COPY CODE",
                Size = new Size(200, 30),
                Location = new Point(11, 210),
                BackColor = darkGold,
                ForeColor = goldColor,
                Font = new Font("Courier New", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            copyCodeButton.Click += CopyCodeButton_Click;
            copyCodeButton.Paint += Button_Paint;
            playersPanel.Controls.Add(copyCodeButton);
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
            // Thay đổi trạng thái button và cập nhật màu sắc
            if (notReadyButton.Text == "NOT READY")
            {
                notReadyButton.Text = "READY";
                notReadyButton.BackColor = readyColor;
                notReadyButton.ForeColor = Color.Black;

                // Cập nhật trạng thái người chơi
                var currentPlayer = players[0]; // Giả sử đây là người chơi hiện tại
                currentPlayer.IsReady = true;
                currentPlayer.Status = "Ready";
            }
            else
            {
                notReadyButton.Text = "NOT READY";
                notReadyButton.BackColor = notReadyColor;
                notReadyButton.ForeColor = Color.White;

                // Cập nhật trạng thái người chơi
                var currentPlayer = players[0]; // Giả sử đây là người chơi hiện tại
                currentPlayer.IsReady = false;
                currentPlayer.Status = "Not Ready";
            }

            UpdatePlayersDisplay();
        }

        private void startGameButton_Click(object sender, EventArgs e)
        {
            // Kiểm tra tất cả người chơi đã ready chưa
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
                MessageBox.Show("Starting game...", "Game Start", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        // Custom painting cho các panel để tạo hiệu ứng border pixelated
        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            Control panel = sender as Control;
            if (panel == null) return;

            // Vẽ border dày giống pixel art
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

            // Xác định màu nền dựa trên trạng thái
            Color backgroundColor = darkGold;
            if (button == notReadyButton)
            {
                backgroundColor = button.Text == "READY" ? readyColor : notReadyColor;
            }

            // Tạo hiệu ứng button pixelated với shadow
            e.Graphics.FillRectangle(new SolidBrush(backgroundColor), 0, 0, button.Width, button.Height);

            // Vẽ text
            Color textColor = button == notReadyButton && button.Text == "NOT READY" ? Color.White : goldColor;
            TextRenderer.DrawText(e.Graphics, button.Text, button.Font,
                new Rectangle(0, 0, button.Width, button.Height), textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            // Border pixelated
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