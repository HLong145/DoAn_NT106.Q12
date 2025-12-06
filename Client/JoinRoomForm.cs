using DoAn_NT106;
using DoAn_NT106.Services;
using DoAn_NT106.Client;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using DoAn_NT106.Services;

namespace PixelGameLobby
{
    public partial class JoinRoomForm : Form
    {
        private string username;
        private string token;
        private readonly List<Room> rooms = new List<Room>();
        private PersistentTcpClient TcpClient => PersistentTcpClient.Instance;

        // Color palette
        private readonly Color primaryBrown = Color.FromArgb(160, 82, 45);
        private readonly Color darkBrown = Color.FromArgb(101, 67, 51);
        private readonly Color darkerBrown = Color.FromArgb(74, 50, 25);
        private readonly Color goldColor = Color.FromArgb(255, 215, 0);
        private readonly Color darkGold = Color.FromArgb(139, 69, 19);
        private readonly Color hoverBrown = Color.FromArgb(120, 60, 30);

        // Global Chat
        private GlobalChatClient globalChatClient;
        private RoomListClient roomListClient;

        private bool isLoadingRooms = false;

        public JoinRoomForm(string username, string token)
        {
            InitializeComponent();
            this.username = username;
            this.token = token;

            SetupPixelStyling();
            SetupEventHandlers();
            SetupGlobalChatEvents();
        }

        public JoinRoomForm() : this("Guest", "")
        {
        }

        #region Initialization

        private void SetupPixelStyling()
        {
            Font = new Font("Courier New", 12, FontStyle.Bold);
            BackColor = primaryBrown;
            lblWelcome.Text = $"Welcome, {username}!";
        }

        private void SetupEventHandlers()
        {
            // Search/Join button
            btnSearchJoin.Click += BtnSearchJoin_Click;
            btnSearchJoin.MouseEnter += Button_MouseEnter;
            btnSearchJoin.MouseLeave += Button_MouseLeave;
            btnSearchJoin.Paint += Button_Paint;

            // Refresh button
            btn_refresh.Click += BtnRefresh_Click;
            btn_refresh.Text = "🔄";
            btn_refresh.MouseEnter += Button_MouseEnter;
            btn_refresh.MouseLeave += (s, e) =>
            {
                if (s is Button btn) btn.BackColor = Color.DarkOrchid;
            };

            // ✅ NEW: Test Room button
            btnTestRoom.Click += BtnTestRoom_Click;
            btnTestRoom.MouseEnter += Button_MouseEnter;
            btnTestRoom.MouseLeave += (s, e) =>
            {
                if (s is Button btn) btn.BackColor = Color.FromArgb(0, 102, 204);
            };
            btnTestRoom.Paint += Button_Paint;

            // Create room button
            btnCreateRoom.Click += BtnCreateRoom_Click;
            btnCreateRoom.MouseEnter += Button_MouseEnter;
            btnCreateRoom.MouseLeave += (s, e) =>
            {
                if (s is Button btn) btn.BackColor = Color.FromArgb(0, 128, 0);
            };

            // Back button
            btnBack.Click += BtnBack_Click;

            // Panels
            foreach (var panel in new[] { pnlRoomList, pnlSearch, pnlHelp, headerPanel, roomsPanel })
            {
                if (panel != null)
                    panel.Paint += Panel_Paint;
            }

            if (lblTitle != null)
                lblTitle.Paint += Label_Paint;

            // Textboxes
            foreach (var tb in new[] { txtRoomCode, txtPassword })
            {
                if (tb != null)
                {
                    tb.Enter += TextBox_Enter;
                    tb.Leave += TextBox_Leave;
                    tb.KeyPress += TextBox_KeyPress;
                }
            }
        }

        #endregion
        #region Initialization


        // ✅ THÊM MỚI: Setup RoomListClient
        private async void SetupRoomListClient()
        {
            try
            {
                roomListClient = new RoomListClient();

                // Subscribe to events
                roomListClient.OnRoomListUpdated += HandleRoomListUpdate;
                roomListClient.OnError += (error) =>
                {
                    Console.WriteLine($"❌ RoomListClient error: {error}");
                };

                // Connect and subscribe
                bool connected = await roomListClient.ConnectAndSubscribeAsync(username, token);

                if (!connected)
                {
                    Console.WriteLine("❌ Failed to connect to room list broadcaster");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SetupRoomListClient error: {ex.Message}");
            }
        }

        // ✅ THÊM MỚI: Handle room list updates
        // ✅ SỬA: Đổi từ RoomInfo sang RoomListInfo
        private void HandleRoomListUpdate(List<RoomListInfo> newRooms)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => HandleRoomListUpdate(newRooms)));
                return;
            }

            try
            {
                rooms.Clear();

                foreach (var room in newRooms)
                {
                    rooms.Add(new Room
                    {
                        Code = room.RoomCode,
                        Name = room.RoomName,
                        Players = $"{room.PlayerCount}/2",
                        IsLocked = room.HasPassword
                    });
                }

                UpdateRoomsDisplay();
                Console.WriteLine($"✅ Room list updated: {rooms.Count} rooms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HandleRoomListUpdate error: {ex.Message}");
            }
        }
        #endregion
        #region Room Loading from Server


        private void UpdateRefreshButtonState(string text, bool enabled)
        {
            if (btn_refresh == null) return;

            if (btn_refresh.InvokeRequired)
            {
                btn_refresh.Invoke(new Action(() =>
                {
                    btn_refresh.Text = text;
                    btn_refresh.Enabled = enabled;
                }));
            }
            else
            {
                btn_refresh.Text = text;
                btn_refresh.Enabled = enabled;
            }
        }

        private List<Room> ParseRoomsFromResponse(Dictionary<string, object> data)
        {
            var result = new List<Room>();

            try
            {
                if (data.TryGetValue("rooms", out var roomsObj) && roomsObj != null)
                {
                    // Nếu là JsonElement (từ System.Text.Json)
                    if (roomsObj is System.Text.Json.JsonElement jsonElement)
                    {
                        foreach (var item in jsonElement.EnumerateArray())
                        {
                            var room = new Room
                            {
                                Code = item.GetProperty("RoomCode").GetString(),
                                Name = item.GetProperty("RoomName").GetString() ?? "Unnamed Room",
                                IsLocked = item.GetProperty("HasPassword").GetBoolean(),
                                Players = $"{item.GetProperty("PlayerCount").GetInt32()}/2"
                            };
                            result.Add(room);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ParseRoomsFromResponse error: {ex.Message}");
            }

            return result;
        }

        private void UpdateRoomsList(List<Room> newRooms)
        {
            rooms.Clear();
            rooms.AddRange(newRooms);
            UpdateRoomsDisplay();
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            ShowMessage("Refreshing...");
        }

        #endregion

        #region UI Rendering

        private void UpdateRoomsDisplay()
        {
            roomsPanel.Controls.Clear();

            if (rooms.Count == 0)
            {
                // Hiển thị message khi không có room
                var lblEmpty = new Label
                {
                    Text = "No rooms available. Create one!",
                    Font = new Font("Courier New", 12, FontStyle.Italic),
                    ForeColor = Color.LightGray,
                    AutoSize = true,
                    Location = new Point(20, 20)
                };
                roomsPanel.Controls.Add(lblEmpty);
                return;
            }

            foreach (var room in rooms)
                AddRoomItem(room);
        }

        private void AddRoomItem(Room room)
        {
            bool isFull = room.Players == "2/2";

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
                Location = new Point(20, 32),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var lblPlayers = new Label
            {
                Text = room.Players,
                Font = new Font("Courier New", 14, FontStyle.Bold),
                ForeColor = isFull ? Color.Red : goldColor,
                Location = new Point(300, 18),
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
                Text = isFull ? "FULL" : (room.IsLocked ? "JOIN 🔒" : "JOIN"),
                Font = new Font("Courier New", 10, FontStyle.Bold),
                Size = new Size(100, 40),
                BackColor = isFull ? Color.Gray : (room.IsLocked ? Color.Orange : Color.Green),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = isFull ? Cursors.No : Cursors.Hand,
                Tag = room.Code,
                Enabled = !isFull
            };

            // Cập nhật vị trí khi resize
            roomPanel.Resize += (s, e) =>
            {
                lblLock.Location = new Point(roomPanel.Width - 160, 18);
                btnAction.Location = new Point(roomPanel.Width - 110, 10);
            };

            // Trigger resize để set vị trí ban đầu
            lblLock.Location = new Point(roomPanel.Width - 160, 18);
            btnAction.Location = new Point(roomPanel.Width - 110, 10);

            // Events
            if (!isFull)
            {
                btnAction.Click += RoomActionButton_Click;
                btnAction.MouseEnter += Button_MouseEnter;
                btnAction.MouseLeave += (s, e) =>
                {
                    if (s is Button btn)
                        btn.BackColor = room.IsLocked ? Color.Orange : Color.Green;
                };
            }
            btnAction.Paint += Button_Paint;

            roomPanel.Paint += Panel_Paint;
            roomPanel.MouseEnter += (s, e) => roomPanel.BackColor = hoverBrown;
            roomPanel.MouseLeave += (s, e) => roomPanel.BackColor = darkBrown;

            roomPanel.Controls.AddRange(new Control[] { lblName, lblCode, lblPlayers, lblLock, btnAction });
            roomsPanel.Controls.Add(roomPanel);
        }

        #endregion

        #region Room Join Logic

        private async void BtnSearchJoin_Click(object sender, EventArgs e)
        {
            var code = txtRoomCode.Text.Trim();
            var password = txtPassword.Text;

            if (string.IsNullOrEmpty(code))
            {
                ShowMessage("Please enter room code!");
                return;
            }

            // Tìm trong local list trước
            var room = rooms.Find(r => r.Code == code);

            if (room != null)
            {
                // Nếu room có password và user đã nhập sẵn password
                if (room.IsLocked && !string.IsNullOrEmpty(password))
                {
                    await JoinRoomAsync(room.Code, room.Name, password);
                }
                else
                {
                    JoinRoom(room);
                }
            }
            else
            {
                // Room không có trong list, thử join trực tiếp với server
                await JoinRoomAsync(code, code, password);
            }
        }

        private void RoomActionButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is string code)
            {
                var room = rooms.Find(r => r.Code == code);
                if (room != null) JoinRoom(room);
            }
        }

        private async void JoinRoom(Room room)
        {
            try
            {
                string password = null;

                // Nếu room có password, hiển thị dialog nhập password
                if (room.IsLocked)
                {
                    using (var passForm = new PasswordForm(room.Name))
                    {
                        if (passForm.ShowDialog() != DialogResult.OK)
                            return;

                        password = passForm.Password;

                        if (string.IsNullOrEmpty(password))
                        {
                            ShowMessage("Please enter password!");
                            return;
                        }
                    }
                }

                // Kiểm tra room đã đầy chưa
                if (room.Players == "2/2")
                {
                    ShowMessage("Room is full!");
                    return;
                }

                await JoinRoomAsync(room.Code, room.Name, password);
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Error: {ex.Message}");
            }
        }

        private async Task JoinRoomAsync(string roomCode, string roomName, string password)
        {
            try
            {
                SetJoinButtonsEnabled(false);

                var response = await TcpClient.JoinRoomAsync(roomCode, password, username);

                if (response.Success)
                {
                    globalChatClient?.Dispose();
                    globalChatClient = null;

                    // Mở GameLobbyForm (giữ nguyên tên cũ)
                    var lobbyForm = new GameLobbyForm(roomCode, username, token);
                    lobbyForm.FormClosed += async (s, e) =>
                    {
                        this.Show();
                        await ConnectGlobalChatAsync();
                    };
                    lobbyForm.Show();
                    this.Hide();
                }
                else
                {
                    ShowMessage($"❌ {response.Message}");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Error: {ex.Message}");
            }
            finally
            {
                SetJoinButtonsEnabled(true);
            }
        }


        private void SetJoinButtonsEnabled(bool enabled)
        {
            btnSearchJoin.Enabled = enabled;
            btnCreateRoom.Enabled = enabled;
            btn_refresh.Enabled = enabled;

            // Disable/Enable tất cả JOIN buttons trong room list
            foreach (Control ctrl in roomsPanel.Controls)
            {
                if (ctrl is Panel panel)
                {
                    foreach (Control child in panel.Controls)
                    {
                        if (child is Button btn && btn.Text != "FULL")
                        {
                            btn.Enabled = enabled;
                        }
                    }
                }
            }
        }

        #endregion

        #region Create Room

        // ===========================
        // ✅ SỬA: BtnCreateRoom_Click - KHÔNG tự động mở lobby
        // Thay thế method cũ trong JoinRoomForm.cs
        // ===========================

        private async void BtnCreateRoom_Click(object sender, EventArgs e)
        {
            using (var createForm = new CreateRoomForm())
            {
                if (createForm.ShowDialog() != DialogResult.OK)
                    return;

                string roomName = createForm.RoomName;
                string password = createForm.RoomPassword;

                btnCreateRoom.Enabled = false;
                btnCreateRoom.Text = "CREATING...";

                try
                {
                    var response = await TcpClient.CreateRoomAsync(roomName, password, username);

                    if (response.Success)
                    {
                        string roomCode = response.GetDataValue("roomCode");

                        if (!string.IsNullOrEmpty(roomCode))
                        {
                            // ✅ SỬA: Chỉ thông báo tạo thành công, KHÔNG mở lobby
                            MessageBox.Show(
                                $"✅ Room created successfully!\n\n" +
                                $"Room Code: {roomCode}\n" +
                                $"Room Name: {roomName}\n\n" +
                                $"Click JOIN to enter the room.",
                                "Room Created",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            // ✅ SỬA: Tự động điền room code vào ô search
                            txtRoomCode.Text = roomCode;
                            txtPassword.Text = password ?? "";

                            // ✅ Room list sẽ tự động update qua RoomListClient broadcast
                            // Không cần gọi thủ công
                        }
                    }
                    else
                    {
                        ShowMessage($"❌ {response.Message}");
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"❌ Error: {ex.Message}");
                }
                finally
                {
                    btnCreateRoom.Enabled = true;
                    btnCreateRoom.Text = "CREATE ROOM";
                }
            }
        }

        #endregion

        #region Test Room (Offline Mode)

        private void BtnTestRoom_Click(object sender, EventArgs e)
        {
            // Open offline test character selection dialog
            using (var testForm = new OfflineTestForm(username))
            {
                if (testForm.ShowDialog() == DialogResult.OK)
                {
                    string player1Character = testForm.Player1Character;
                    string player2Character = testForm.Player2Character;
                    string player2Name = testForm.Player2Name;

                    // Stop timers and chat
                    globalChatClient?.Dispose();
                    globalChatClient = null;

                    // Open BattleForm in offline mode
                    var battleForm = new BattleForm(
                        username,           // Player 1 name
                        token,              // Token (not used in offline)
                        player2Name,        // Player 2 name
                        player1Character,   // Player 1 character
                        player2Character    // Player 2 character
                    );

                    battleForm.FormClosed += async (s, args) =>
                    {
                        this.Show();
                        await ConnectGlobalChatAsync();
                    };

                    battleForm.Show();
                    this.Hide();
                }
            }
        }

        #endregion

        #region UI Effects

        private void Button_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Button button && button.Enabled)
            {
                button.BackColor = Color.FromArgb(
                    Math.Min(button.BackColor.R + 30, 255),
                    Math.Min(button.BackColor.G + 30, 255),
                    Math.Min(button.BackColor.B + 30, 255)
                );
            }
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                if (button == btnSearchJoin)
                    button.BackColor = darkGold;
                else if (button == btnCreateRoom)
                    button.BackColor = Color.FromArgb(0, 128, 0);
                else if (button == btn_refresh)
                    button.BackColor = Color.DarkOrchid;
                else if (button == btnTestRoom) // ✅ NEW: Handle test room button
                    button.BackColor = Color.FromArgb(0, 102, 204);
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

        private void BtnBack_Click(object sender, EventArgs e)
        {
            globalChatClient?.Dispose();
            globalChatClient = null;

            MainForm mainForm = new MainForm(username, token);
            mainForm.Show();
            this.Close();
        }

        #endregion

        #region Global Chat

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
            this.Load += async (s, e) =>
            {
                await ConnectGlobalChatAsync();
                SetupRoomListClient();
            };
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
                Console.WriteLine($"[JoinRoomForm] Connecting to GlobalChat as {username}...");

                globalChatClient = new GlobalChatClient();

                globalChatClient.OnChatMessage += GlobalChat_OnChatMessage;
                globalChatClient.OnOnlineCountUpdate += GlobalChat_OnOnlineCountUpdate;
                globalChatClient.OnError += GlobalChat_OnError;
                globalChatClient.OnDisconnected += GlobalChat_OnDisconnected;

                var result = await globalChatClient.ConnectAndJoinAsync(username, token);

                Console.WriteLine($"[JoinRoomForm] GlobalChat result: Success={result.Success}, OnlineCount={result.OnlineCount}, History={result.History?.Count ?? 0}");

                if (result.Success)
                {
                    Console.WriteLine($"[JoinRoomForm] Calling UpdateOnlineCount({result.OnlineCount})");
                    UpdateOnlineCount(result.OnlineCount);

                    if (result.History != null)
                    {
                        foreach (var msg in result.History)
                        {
                            AddChatMessageToUI(msg);
                        }
                    }

                    Console.WriteLine($"[JoinRoomForm] GlobalChat connected successfully");
                }
                else
                {
                    Console.WriteLine($"[JoinRoomForm] Failed to connect to GlobalChat");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JoinRoomForm] GlobalChat Error: {ex.Message}");
            }
        }

        private void GlobalChat_OnOnlineCountUpdate(int count)
        {
            Console.WriteLine($"[JoinRoomForm] OnOnlineCountUpdate received: {count}");

            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int>(GlobalChat_OnOnlineCountUpdate), count);
                return;
            }
            UpdateOnlineCount(count);
        }

        private void UpdateOnlineCount(int count)
        {
            Console.WriteLine($"[JoinRoomForm] UpdateOnlineCount: {count}");

            if (lblOnlineCount != null)
            {
                lblOnlineCount.Text = $"🟢 {count} online";
                Console.WriteLine($"[JoinRoomForm] Label updated to: {lblOnlineCount.Text}");
            }
            else
            {
                Console.WriteLine($"[JoinRoomForm] ⚠️ lblOnlineCount is NULL!");
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

        
        private void GlobalChat_OnError(string error)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(GlobalChat_OnError), error);
                return;
            }
            Console.WriteLine($"Global Chat Error: {error}");
        }

        private void GlobalChat_OnDisconnected()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(GlobalChat_OnDisconnected));
                return;
            }
            UpdateOnlineCount(0);
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
            if (txtChatInput == null) return;

            string message = txtChatInput.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            txtChatInput.Clear();
            txtChatInput.Focus();

            if (globalChatClient != null && globalChatClient.IsConnected)
            {
                await globalChatClient.SendMessageAsync(message);
            }
        }

        #endregion

        #region Form Lifecycle

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            try
            {
                // Disconnect room list client
                roomListClient?.Disconnect();

                // Disconnect global chat
                globalChatClient?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Form closing error: {ex.Message}");
            }
        }

        #endregion
    }

    #region Data Models

    /// <summary>
    /// Room model cho client-side
    /// </summary>
    public class Room
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Players { get; set; }
        public bool IsLocked { get; set; }
    }

    #endregion
}