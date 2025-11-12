using DoAn_NT106;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PixelGameLobby
{
    public partial class JoinRoomForm : Form
    {
        private string username;
        private string token;
        private string actionType;
        private readonly List<Room> rooms = new List<Room>();

        // Color palette
        private readonly Color primaryBrown = Color.FromArgb(160, 82, 45);
        private readonly Color darkBrown = Color.FromArgb(101, 67, 51);
        private readonly Color darkerBrown = Color.FromArgb(74, 50, 25);
        private readonly Color goldColor = Color.FromArgb(255, 215, 0);
        private readonly Color darkGold = Color.FromArgb(139, 69, 19);
        private readonly Color hoverBrown = Color.FromArgb(120, 60, 30);

        // Constructor mới nhận tham số
        public JoinRoomForm(string username, string token)
        {
            InitializeComponent();
            this.username = username;
            this.token = token;

            SetupPixelStyling();
            SetupEventHandlers();
            InitializeSampleRooms();

            // Cập nhật title với username
            this.Text = $"Pixel Game Lobby - Welcome {username}";
        }

        // Constructor cũ để tương thích
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
                Dock = DockStyle.None,
                Width = roomsPanel.ClientSize.Width - 40,
                Height = 60,
                BackColor = darkBrown,
                Margin = new Padding(10, 8, 10, 8),
                BorderStyle = BorderStyle.FixedSingle,
            };

            // Tên phòng
            var lblName = new Label
            {
                Text = room.Name,
                Font = new Font("Courier New", 12, FontStyle.Bold),
                ForeColor = goldColor,
                Location = new Point(20, 12),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Mã phòng
            var lblCode = new Label
            {
                Text = $"Code: {room.Code}",
                Font = new Font("Courier New", 9),
                ForeColor = Color.LightGoldenrodYellow,
                Location = new Point(20, 30),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Số người
            var lblPlayers = new Label
            {
                Text = room.Players,
                Font = new Font("Courier New", 14, FontStyle.Bold),
                ForeColor = goldColor,
                Location = new Point(300, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Biểu tượng khóa
            var lblLock = new Label
            {
                Text = room.IsLocked ? "🔒" : "🔓",
                Font = new Font("Arial", 18),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Nút thao tác
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

            // Canh phải tự động (thay vì toạ độ cố định)
            roomPanel.Resize += (s, e) =>
            {
                lblLock.Location = new Point(roomPanel.Width - 180, 15);
                btnAction.Location = new Point(roomPanel.Width - 130, 10);
            };

            // Sự kiện
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
                        return; // Người dùng bấm Cancel

                    var password = passForm.Password;
                    if (string.IsNullOrEmpty(password))
                    {
                        ShowMessage("Please enter password!");
                        return;
                    }

                    // Giả sử mật khẩu đúng là 123456 (bạn có thể thay thành dữ liệu thật)
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
            var lobbyForm = new GameLobbyForm(room.Code);
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
            {
                tb.BackColor = Color.FromArgb(120, 80, 60);
                tb.ForeColor = Color.White;
            }
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.BackColor = darkBrown;
                tb.ForeColor = goldColor;
            }
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                BtnSearchJoin_Click(sender, e);
                e.Handled = true;
            }
        }

        // ===============================
        // Custom Painting
        // ===============================
        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            if (sender is not Control panel) return;

            using Pen darkPen = new(darkerBrown, 4);
            using Pen lightPen = new(Color.FromArgb(180, 140, 100), 2);

            e.Graphics.DrawRectangle(darkPen, 0, 0, panel.Width - 1, panel.Height - 1);
            e.Graphics.DrawRectangle(lightPen, 2, 2, panel.Width - 5, panel.Height - 5);
        }

        private void Button_Paint(object sender, PaintEventArgs e)
        {
            if (sender is not Button button) return;

            e.Graphics.FillRectangle(new SolidBrush(button.BackColor), 0, 0, button.Width, button.Height);
            TextRenderer.DrawText(e.Graphics, button.Text, button.Font,
                new Rectangle(0, 0, button.Width, button.Height), button.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            e.Graphics.DrawRectangle(new Pen(darkerBrown, 2), 0, 0, button.Width - 1, button.Height - 1);
        }

        private void Label_Paint(object sender, PaintEventArgs e)
        {
            if (sender is Label lbl)
            {
                using Pen goldPen = new(goldColor, 2);
                e.Graphics.DrawRectangle(goldPen, 0, 0, lbl.Width - 1, lbl.Height - 1);
            }
        }

        // ===============================
        // Utility
        // ===============================
        private void ShowMessage(string message)
        {
            MessageBox.Show(message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string ShowPasswordDialog()
        {
            using var prompt = new Form
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Enter Password",
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = primaryBrown
            };

            var lbl = new Label
            {
                Left = 20,
                Top = 20,
                Text = "Room password:",
                ForeColor = goldColor,
                Font = new Font("Courier New", 9, FontStyle.Bold)
            };

            var tb = new TextBox
            {
                Left = 20,
                Top = 50,
                Width = 240,
                UseSystemPasswordChar = true,
                BackColor = darkBrown,
                ForeColor = goldColor,
                Font = new Font("Courier New", 9, FontStyle.Bold)
            };

            var btn = new Button
            {
                Text = "OK",
                Left = 120,
                Width = 80,
                Top = 80,
                DialogResult = DialogResult.OK,
                BackColor = darkGold,
                ForeColor = goldColor,
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };

            btn.Click += (_, _) => prompt.Close();

            prompt.Controls.AddRange(new Control[] { lbl, tb, btn });
            prompt.AcceptButton = btn;

            return prompt.ShowDialog() == DialogResult.OK ? tb.Text : string.Empty;
        }

        private void btn_refresh_Click(object sender, EventArgs e)
        {
            //Chưa xây dựng logic hàm này
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            MainForm mainForm = new MainForm(username, token);
            mainForm.Show();
            this.Close();
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
