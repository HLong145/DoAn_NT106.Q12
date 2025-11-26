using DoAn_NT106;
using DoAn_NT106.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PixelGameLobby
{
    public partial class JoinRoomForm : Form
    {
        private string username;
        private string token;
        private readonly List<Room> rooms = new List<Room>();

        // Color palette
        private readonly Color primaryBrown = Color.FromArgb(160, 82, 45);
        private readonly Color darkBrown = Color.FromArgb(101, 67, 51);
        private readonly Color darkerBrown = Color.FromArgb(74, 50, 25);
        private readonly Color goldColor = Color.FromArgb(255, 215, 0);
        private readonly Color darkGold = Color.FromArgb(139, 69, 19);
        private readonly Color hoverBrown = Color.FromArgb(120, 60, 30);

        // Global Chat
        private GlobalChatClient globalChatClient;

        public JoinRoomForm(string username, string token)
        {
            InitializeComponent();
            this.username = username;
            this.token = token;

            SetupPixelStyling();
            SetupEventHandlers();
            InitializeSampleRooms();
            SetupGlobalChatEvents();

            this.Text = $"Pixel Game Lobby - Welcome {username}";
        }

        public JoinRoomForm() : this("Guest", "")
        {
        }

        // ===============================
        // Initialization
        // ===============================
        private void SetupPixelStyling()
        {
            Font = new Font("Courier New", 12, FontStyle.Bold);
            BackColor = primaryBrown;
            lblWelcome.Text = $"Welcome, {username}!";
        }

        private void SetupEventHandlers()
        {
            // Main button
            btnSearchJoin.Click += BtnSearchJoin_Click;
            btnSearchJoin.MouseEnter += Button_MouseEnter;
            btnSearchJoin.MouseLeave += Button_MouseLeave;
            btnSearchJoin.Paint += Button_Paint;

            // Panels
            foreach (var panel in new[] { pnlRoomList, pnlSearch, pnlHelp, headerPanel, roomsPanel })
            {
                panel.Paint += Panel_Paint;
            }

            lblTitle.Paint += Label_Paint;

            // Textboxes
            foreach (var tb in new[] { txtRoomCode, txtPassword })
            {
                tb.Enter += TextBox_Enter;
                tb.Leave += TextBox_Leave;
                tb.KeyPress += TextBox_KeyPress;
            }
        }

        private void InitializeSampleRooms()
        {
            rooms.Clear();
            rooms.AddRange(new[]
            {
                new Room { Name = "Room of Teo", Code = "123456", Players = "1/2", IsLocked = false },
                new Room { Name = "VIP Room", Code = "654321", Players = "2/2", IsLocked = true },
                new Room { Name = "Unnamed Room", Code = "987654", Players = "1/2", IsLocked = false },
                new Room { Name = "Pro Only Room", Code = "111222", Players = "1/2", IsLocked = false }
            });

            UpdateRoomsDisplay();
        }

        // ===============================
        // UI Rendering
        // ===============================
        private void UpdateRoomsDisplay()
        {
            roomsPanel.Controls.Clear();
            foreach (var room in rooms)
                AddRoomItem(room);
        }

        private void AddRoomItem(Room room)
        {
            var roomPanel = new Panel
            {
                Width = roomsPanel.ClientSize.Width - 40,
                Height = 60,
                BackColor = darkBrown,
                Margin = new Padding(10, 8, 10, 8),
                BorderStyle = BorderStyle.FixedSingle,
            };

            var lblName = new Label
            {
                Text = room.Name,
                Font = new Font("Courier New", 12, FontStyle.Bold),
                ForeColor = goldColor,
                Location = new Point(20, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var lblCode = new Label
            {
                Text = $"Code: {room.Code}",
                Font = new Font("Courier New", 9),
                ForeColor = Color.LightGoldenrodYellow,
                Location = new Point(20, 30),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var lblPlayers = new Label
            {
                Text = room.Players,
                Font = new Font("Courier New", 14, FontStyle.Bold),
                ForeColor = goldColor,
                Location = new Point(300, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var lblLock = new Label
            {
                Text = room.IsLocked ? "🔒" : "🔓",
                Font = new Font("Arial", 18),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var btnAction = new Button
            {
                Text = room.IsLocked ? "WATCH" : "JOIN",
                Font = new Font("Courier New", 12, FontStyle.Bold),
                Size = new Size(120, 40),
                BackColor = room.IsLocked ? Color.Orange : Color.Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Tag = room.Code
            };

            roomPanel.Resize += (s, e) =>
            {
                lblLock.Location = new Point(roomPanel.Width - 180, 15);
                btnAction.Location = new Point(roomPanel.Width - 130, 10);
            };

            btnAction.Click += RoomActionButton_Click;
            btnAction.Paint += Button_Paint;
            btnAction.MouseEnter += Button_MouseEnter;
            btnAction.MouseLeave += Button_MouseLeave;

            roomPanel.Paint += Panel_Paint;
            roomPanel.MouseEnter += (s, e) => roomPanel.BackColor = hoverBrown;
            roomPanel.MouseLeave += (s, e) => roomPanel.BackColor = darkBrown;
            roomPanel.Click += RoomPanel_Click;

            roomPanel.Controls.AddRange(new Control[] { lblName, lblCode, lblPlayers, lblLock, btnAction });
            roomsPanel.Controls.Add(roomPanel);
        }

        // ===============================
        // Logic
        // ===============================
        private void BtnSearchJoin_Click(object sender, EventArgs e)
        {
            var code = txtRoomCode.Text.Trim();
            var password = txtPassword.Text;

            if (string.IsNullOrEmpty(code))
            {
                ShowMessage("Please enter room code!");
                return;
            }

            var room = rooms.Find(r => r.Code == code);
            if (room == null)
            {
                ShowMessage($"Room with code {code} not found!");
                return;
            }

            if (room.IsLocked && string.IsNullOrEmpty(password))
            {
                ShowMessage("Room is locked! Please enter password.");
                return;
            }

            JoinRoom(room);
        }

        private void RoomActionButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is string code)
            {
                var room = rooms.Find(r => r.Code == code);
                if (room != null) JoinRoom(room);
            }
        }

        private void RoomPanel_Click(object sender, EventArgs e)
        {
            if (sender is Panel panel && panel.Tag is RoomInfo info)
            {
                var room = rooms.Find(r => r.Code == info.Code);
                if (room != null) JoinRoom(room);
            }
        }

        private void JoinRoom(Room room)
        {
            if (room.IsLocked)
            {
                using (var passForm = new PasswordForm(room.Name))
                {
                    if (passForm.ShowDialog() != DialogResult.OK)
                        return;

                    var password = passForm.Password;
                    if (string.IsNullOrEmpty(password))
                    {
                        ShowMessage("Please enter password!");
                        return;
                    }

                    if (password != "123456")
                    {
                        ShowMessage("Incorrect password!");
                        return;
                    }
                }
            }

            if (room.Players == "2/2")
            {
                ShowMessage("Room is full!");
                return;
            }

            ShowMessage($"Joining room: {room.Name}");
            var lobbyForm = new GameLobbyForm(room.Code, username, token);
            lobbyForm.Show();
            Hide();
        }

        // ===============================
        // UI Effects
        // ===============================
        private void Button_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Button button)
                button.BackColor = Color.FromArgb(
                    Math.Min(button.BackColor.R + 30, 255),
                    Math.Min(button.BackColor.G + 30, 255),
                    Math.Min(button.BackColor.B + 30, 255)
                );
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                button.BackColor = button == btnSearchJoin
                    ? darkGold
                    : button.Text == "WATCH" ? Color.Orange : Color.Green;
            }
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            if (sender is TextBox tb)
                tb.BackColor = Color.FromArgb(90, 60, 30);
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            if (sender is TextBox tb)
                tb.BackColor = darkerBrown;
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                BtnSearchJoin_Click(sender, e);
            }
        }

        private void Button_Paint(object sender, PaintEventArgs e)
        {
            if (sender is Button btn)
            {
                using (var pen = new Pen(darkerBrown, 3))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, btn.Width - 1, btn.Height - 1);
                }
            }
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            if (sender is Panel panel)
            {
                using (var pen = new Pen(darkerBrown, 3))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
                }
            }
        }

        private void Label_Paint(object sender, PaintEventArgs e)
        {
            // Custom label painting if needed
        }

        private void ShowMessage(string message)
        {
            MessageBox.Show(message, "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btn_refresh_Click(object sender, EventArgs e)
        {
            InitializeSampleRooms();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            MainForm mainForm = new MainForm(username, token);
            mainForm.Show();
            this.Close();
        }

        // ===============================
        // GLOBAL CHAT
        // ===============================

        #region Global Chat Logic

        private void SetupGlobalChatEvents()
        {
            if (txtChatInput != null)
            {
                txtChatInput.KeyPress += TxtChatInput_KeyPress;
            }

            if (btnSendChat != null)
            {
                btnSendChat.Click += BtnSendChat_Click;
                btnSendChat.Paint += Button_Paint;
                btnSendChat.MouseEnter += Button_MouseEnter;
                btnSendChat.MouseLeave += (s, e) =>
                {
                    if (s is Button btn) btn.BackColor = darkGold;
                };
            }

            // Kết nối Global Chat khi form load
            this.Load += async (s, e) => await ConnectGlobalChatAsync();
        }

        private async Task ConnectGlobalChatAsync()
        {
            if (pnlChatMessages == null)
            {
                await Task.Delay(100);
                if (pnlChatMessages == null) return;
            }

            try
            {
                globalChatClient = new GlobalChatClient("127.0.0.1", 8080);

                globalChatClient.OnChatMessage += GlobalChat_OnChatMessage;
                globalChatClient.OnOnlineCountUpdate += GlobalChat_OnOnlineCountUpdate;
                globalChatClient.OnError += GlobalChat_OnError;
                globalChatClient.OnDisconnected += GlobalChat_OnDisconnected;

                var result = await globalChatClient.ConnectAndJoinAsync(username, token);

                if (result.Success)
                {
                    UpdateOnlineCount(result.OnlineCount);

                    if (result.History != null)
                    {
                        foreach (var msg in result.History)
                        {
                            AddChatMessageToUI(msg);
                        }
                    }
                    // ✅ BỎ DÒNG AddSystemMessage ở đây
                }
                // ✅ BỎ LUÔN else AddSystemMessage lỗi
            }
            catch (Exception ex)
            {
                // Chỉ log lỗi, không hiển thị
                Console.WriteLine($"Global Chat Error: {ex.Message}");
            }
        }

        private void GlobalChat_OnChatMessage(ChatMessageData message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<ChatMessageData>(GlobalChat_OnChatMessage), message);
                return;
            }
            AddChatMessageToUI(message);
        }

        private void GlobalChat_OnOnlineCountUpdate(int count)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int>(GlobalChat_OnOnlineCountUpdate), count);
                return;
            }
            UpdateOnlineCount(count);
        }

        private void GlobalChat_OnError(string error)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(GlobalChat_OnError), error);
                return;
            }
            AddSystemMessage($"⚠️ {error}");
        }

        private void GlobalChat_OnDisconnected()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(GlobalChat_OnDisconnected));
                return;
            }
            AddSystemMessage("🔌 Đã ngắt kết nối Global Chat");
            UpdateOnlineCount(0);
        }

        private void UpdateOnlineCount(int count)
        {
            if (lblOnlineCount != null)
            {
                lblOnlineCount.Text = $"🟢 {count} online";
            }
        }

        private void AddChatMessageToUI(ChatMessageData message)
        {
            if (pnlChatMessages == null || message == null)
                return;

            var msgPanel = new Panel
            {
                Width = pnlChatMessages.Width - 25,
                AutoSize = true,
                MinimumSize = new Size(pnlChatMessages.Width - 25, 20),
                BackColor = Color.Transparent,
                Padding = new Padding(3, 2, 3, 2)
            };

            var lblHeader = new Label
            {
                Text = message.Type == "system"
                    ? $"[{message.Timestamp}] 🔔"
                    : $"[{message.Timestamp}] {message.Username}:",
                Font = new Font("Courier New", 8, FontStyle.Bold),
                ForeColor = message.Type == "system" ? Color.Orange
                          : message.Username == username ? Color.LimeGreen
                          : goldColor,
                AutoSize = true,
                Location = new Point(3, 2)
            };

            var lblContent = new Label
            {
                Text = message.Message,
                Font = new Font("Courier New", 9),
                ForeColor = message.Type == "system" ? Color.Orange : Color.White,
                AutoSize = true,
                Location = new Point(3, lblHeader.Height + 3)
            };

            msgPanel.Controls.Add(lblHeader);
            msgPanel.Controls.Add(lblContent);
            msgPanel.Height = lblHeader.Height + lblContent.Height + 8;

            int yPos = 5;
            foreach (Control ctrl in pnlChatMessages.Controls)
            {
                yPos = Math.Max(yPos, ctrl.Bottom + 3);
            }
            msgPanel.Location = new Point(3, yPos);

            pnlChatMessages.Controls.Add(msgPanel);
            pnlChatMessages.ScrollControlIntoView(msgPanel);
        }

        private void AddSystemMessage(string message)
        {
            if (pnlChatMessages == null)
                return;

            AddChatMessageToUI(new ChatMessageData
            {
                Id = Guid.NewGuid().ToString(),
                Username = "System",
                Message = message,
                Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                Type = "system"
            });
        }

        private async void BtnSendChat_Click(object sender, EventArgs e)
        {
            await SendChatMessageAsync();
        }

        private async void TxtChatInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                await SendChatMessageAsync();
            }
        }

        private async Task SendChatMessageAsync()
        {
            if (txtChatInput == null)
                return;

            string message = txtChatInput.Text.Trim();

            if (string.IsNullOrEmpty(message))
                return;

            // ✅ Xóa tin nhắn trong khung nhập TRƯỚC
            txtChatInput.Clear();
            txtChatInput.Focus();

            // Gửi lên server (nếu có kết nối)
            if (globalChatClient != null && globalChatClient.IsConnected)
            {
                await globalChatClient.SendMessageAsync(message);
            }
        }

        #endregion

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            globalChatClient?.Dispose();
            base.OnFormClosing(e);
        }
    }

    // ===============================
    // Data Models
    // ===============================
    public class Room
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Players { get; set; }
        public bool IsLocked { get; set; }
    }

    public class RoomInfo
    {
        public string Code { get; set; }
        public bool IsLocked { get; set; }
    }
}