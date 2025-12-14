using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DoAn_NT106.Services;
using System.Text.Json;

namespace DoAn_NT106
{
    public class CharacterInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int HP { get; set; }
        public int Damage { get; set; }
        public int Speed { get; set; }
        public int Stamina { get; set; }
        public int Mana { get; set; }
        public string SkillDescription { get; set; }
        public Image PreviewImage { get; set; }
    }

    public partial class CharacterSelectForm : Form
    {

        private string username;
        private string token;
        private string roomCode;  // ✅ THÊM: roomCode
        private string opponentName;
        private bool isHost;
        private string selectedMap = "battleground1";  // ✅ MAP SELECTION

        // Available characters
        private List<CharacterInfo> characters = new List<CharacterInfo>();
        private int selectedIndex = 0;

        // UI Components
        private Panel mainPanel;
        private Label lblTitle;
        private FlowLayoutPanel characterListPanel;
        private Panel previewPanel;
        private PictureBox pbPreview;
        private Label lblCharName;
        private Label lblStats;
        private Label lblSkillDesc;
        private Btn_Pixel btnConfirm;
        private Btn_Pixel btnBack;
        private Panel statsPanel;

        // Colors
        private Color primaryBrown = Color.FromArgb(160, 82, 45);
        private Color darkBrown = Color.FromArgb(101, 67, 51);
        private Color goldColor = Color.FromArgb(255, 215, 0);
        private string player1Character = "girlknight"; // Mặc định
        private string player2Character = "girlknight"; // Mặc định
        public string SelectedCharacter { get; private set; }

        private int myPlayerNumber = 0; // 1 or 2 assigned by server

        public CharacterSelectForm(string username, string token, string roomCode, string opponentName, bool isHost = true, string selectedMap = "battleground1")
        {
            this.username = username;
            this.token = token;
            this.roomCode = roomCode;
            this.opponentName = opponentName;
            this.isHost = isHost;
            this.selectedMap = selectedMap;  // ✅ SAVE MAP

            // ✅ GÁN GIÁ TRỊ MẶC ĐỊNH CHO CHARACTER
            this.player1Character = "girlknight"; // Người chơi hiện tại chọn
            this.player2Character = "girlknight"; // Đối thủ (tạm thời giả định)

            InitializeCharacters();
            InitializeUI();
            UpdatePreview();
            
            // ✅ REGISTER TCP BROADCAST HANDLER BEFORE SHOWING
            PersistentTcpClient.Instance.OnBroadcast += TcpClient_OnBroadcast;
            Console.WriteLine("[CharacterSelectForm] Registered for TCP broadcasts");
        }

        // ✅ LISTEN TO SERVER BROADCASTS (INCLUDING START_GAME)
        private void TcpClient_OnBroadcast(string action, JsonElement data)
        {
            if (action != "START_GAME") return;

            Console.WriteLine("[CharacterSelectForm] Received START_GAME broadcast");

            try
            {
                // Parse START_GAME data từ server
                string msgRoomCode = GetStringOrNull(data, "roomCode");
                if (!string.Equals(msgRoomCode, roomCode, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[CharacterSelectForm] START_GAME for different room: {msgRoomCode}");
                    return;
                }

                string p1Name = GetStringOrNull(data, "player1");
                string p2Name = GetStringOrNull(data, "player2");
                string p1Char = GetStringOrNull(data, "player1Character") ?? "girlknight";
                string p2Char = GetStringOrNull(data, "player2Character") ?? "girlknight";
                // Map provided by server (preferred)
                string mapFromServer = GetStringOrNull(data, "selectedMap");
                string mapToUse = !string.IsNullOrEmpty(mapFromServer) ? mapFromServer : selectedMap;

                // ✅ DETERMINE MY PLAYER NUMBER BASED ON USERNAME MATCH
                if (string.Equals(p1Name, username, StringComparison.OrdinalIgnoreCase))
                {
                    myPlayerNumber = 1;
                    Console.WriteLine($"[CharacterSelectForm] I am Player 1 - will CREATE BattleForm");
                }
                else if (string.Equals(p2Name, username, StringComparison.OrdinalIgnoreCase))
                {
                    myPlayerNumber = 2;
                    Console.WriteLine($"[CharacterSelectForm] I am Player 2 - will JOIN BattleForm");
                }
                else
                {
                    Console.WriteLine($"[CharacterSelectForm] ⚠️ USERNAME MISMATCH! p1={p1Name}, p2={p2Name}, me={username}");
                    return;
                }

                Console.WriteLine($"[CharacterSelectForm] Game starting: P1={p1Name}({p1Char}) vs P2={p2Name}({p2Char})");

                // ✅ UNREGISTER BROADCAST BEFORE CLOSING
                PersistentTcpClient.Instance.OnBroadcast -= TcpClient_OnBroadcast;

                // ✅ MOVE TO BATTLE ON UI THREAD
                this.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (myPlayerNumber == 1)
                        {
                            // ✅ PLAYER 1: CREATE NEW BATTLEFORM
                            Console.WriteLine($"[CharacterSelectForm] Player 1 creating new BattleForm for room {roomCode}");
                            // Prefer server-provided map if present in START_GAME payload
                            var mapFromServer = GetStringOrNull(data, "selectedMap");
                            var mapToUse = !string.IsNullOrEmpty(mapFromServer) ? mapFromServer : selectedMap;

                            var battleForm = new BattleForm(
                                username,
                                token,
                                opponentName,
                                p1Char,  // Player 1's selected character
                                p2Char,  // Player 2's selected character
                                mapToUse,
                                roomCode,
                                myPlayerNumber,
                                isCreator: true  // ✅ PASS FLAG: I'm the creator
                            );

                            battleForm.FormClosed += (s, args) =>
                            {
                                this.Close();
                            };

                            battleForm.Show();
                            this.Hide();
                        }
                        else
                        {
                            // ✅ PLAYER 2: FIND AND JOIN EXISTING BATTLEFORM
                            Console.WriteLine($"[CharacterSelectForm] Player 2 searching for BattleForm in room {roomCode}");
                            
                            // Tìm BattleForm đã được tạo bởi Player 1
                            BattleForm existingBattleForm = null;
                            foreach (Form form in Application.OpenForms)
                            {
                                if (form is BattleForm bf && bf.RoomCode == roomCode)
                                {
                                    existingBattleForm = bf;
                                    break;
                                }
                            }

                            if (existingBattleForm != null && !existingBattleForm.IsDisposed)
                            {
                                // ✅ JOIN EXISTING BATTLEFORM
                                Console.WriteLine($"[CharacterSelectForm] Found existing BattleForm, joining...");
                                // Ensure existing form uses server map
                                try { existingBattleForm.UpdateSelectedMap(mapToUse); } catch { }

                                existingBattleForm.JoinAsPlayer2(
                                    username,
                                    token,
                                    p1Char,
                                    p2Char,
                                    myPlayerNumber
                                );
                                existingBattleForm.BringToFront();
                                this.Close();
                            }
                            else
                            {
                                // ✅ PLAYER 1 HASN'T CREATED YET - WAIT AND RETRY
                                Console.WriteLine($"[CharacterSelectForm] BattleForm not found yet, waiting...");
                                System.Threading.Thread.Sleep(500);
                                
                                // Retry once more
                                existingBattleForm = null;
                                foreach (Form form in Application.OpenForms)
                                {
                                    if (form is BattleForm bf && bf.RoomCode == roomCode)
                                    {
                                        existingBattleForm = bf;
                                        break;
                                    }
                                }

                                if (existingBattleForm != null && !existingBattleForm.IsDisposed)
                                {
                                    Console.WriteLine($"[CharacterSelectForm] Found BattleForm after retry, joining...");
                                    existingBattleForm.JoinAsPlayer2(
                                        username,
                                        token,
                                        p1Char,
                                        p2Char,
                                        myPlayerNumber
                                    );
                                    existingBattleForm.BringToFront();
                                    this.Close();
                                }
                                else
                                {
                                    // ✅ FALLBACK: CREATE OWN BATTLEFORM IF PLAYER 1 FAILED
                                    Console.WriteLine($"[CharacterSelectForm] BattleForm still not found, creating fallback...");
                                // For fallback or when creating as player2 with no existing BattleForm,
                                // prefer server map
                                var mapFromServer2 = GetStringOrNull(data, "selectedMap");
                                var mapToUse2 = !string.IsNullOrEmpty(mapFromServer2) ? mapFromServer2 : selectedMap;

                                // Create BattleForm with correct player ordering: pass Player1 name first
                                var battleForm = new BattleForm(
                                        p1Name,      // Player 1 (host) - must be first param
                                        token,
                                        p2Name,      // Player 2 (opponent)
                                        p1Char,
                                        p2Char,
                                        mapToUse2,
                                        roomCode,
                                        myPlayerNumber,
                                        isCreator: false
                                    );

                                    battleForm.FormClosed += (s, args) =>
                                    {
                                        this.Close();
                                    };

                                    battleForm.Show();
                                    this.Hide();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CharacterSelectForm] Failed to enter BattleForm: {ex.Message}");
                        MessageBox.Show($"Error starting battle: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CharacterSelectForm] START_GAME parse error: {ex.Message}");
            }
        }

        // ✅ HELPER: Get string from JSON safely
        private string GetStringOrNull(JsonElement data, string propertyName)
        {
            try
            {
                if (data.TryGetProperty(propertyName, out var prop) && prop.ValueKind != JsonValueKind.Null)
                {
                    return prop.GetString();
                }
            }
            catch { }
            return null;
        }

        private void InitializeCharacters()
        {
            // Character 1: Girl Knight
            characters.Add(new CharacterInfo
            {
                Name = "girlknight",
                DisplayName = "Girl Knight",
                HP = 100,
                Damage = 100,
                Speed = 100,
                Stamina = 100,
                Mana = 100,
                SkillDescription = "⚡Sword Spinning - Continuous AoE damage\n" +
                                   "Cost: 25 mana/s | Damage: 10 per hit (2x per second)\n" +
                                   "Stamina cost: 15/s",
                PreviewImage = SafeLoadImage(Properties.Resources.Knightgirl_Idle)
            });

            // Character 2: Bringer of Death
            characters.Add(new CharacterInfo
            {
                Name = "bringerofdeath",
                DisplayName = "Bringer of Death",
                HP = 90,
                Damage = 120,
                Speed = 90,
                Stamina = 100,
                Mana = 100,
                SkillDescription = "💀 Dark Spell - Summons spell at enemy position\n" +
                                   "Cost: 35 mana | Damage: 25\n" +
                                   "Punch: 20 dmg (20 sta) | Kick: 10 dmg (30 sta, stun 0.5s)",
                PreviewImage = SafeLoadImage(Properties.Resources.BringerofDeath_Idle)
            });

            // Character 3: Goatman
            characters.Add(new CharacterInfo
            {
                Name = "goatman",
                DisplayName = "Goatman Berserker",
                HP = 130,
                Damage = 100,
                Speed = 80,
                Stamina = 100,
                Mana = 100,
                SkillDescription = "🐐 Wild Charge - Rush forward for 3s\n" +
                                   "Cost: 35 mana, 25 stamina | Damage: 25\n" +
                                   "Punch: 10 dmg (15 sta) | Kick: 15 dmg (15 sta)",
                PreviewImage = SafeLoadImage(Properties.Resources.GM_Idle)
            });

            // Character 4: Warrior
            characters.Add(new CharacterInfo
            {
                Name = "warrior",
                DisplayName = "Elite Warrior",
                HP = 80,
                Damage = 105,
                Speed = 120,
                Stamina = 100,
                Mana = 100,
                SkillDescription = "⚔️ Energy Wave - Launches projectile\n" +
                                   "Cost: 30 mana, 15 stamina | Damage: 18\n" +
                                   "Attack1: Double hit (14 dmg total, 15 sta)\n" +
                                   "Attack2: Kick 10 dmg (20 sta)",
                PreviewImage = SafeLoadImage(Properties.Resources.Warrior_Idle)
            });
        }

        private Image SafeLoadImage(object resource)
        {
            try
            {
                if (resource == null)
                {
                    Console.WriteLine("⚠️ Resource is null");
                    return CreatePlaceholderImage();
                }

                // Nếu resource là Image
                if (resource is Image img)
                {
                    // Tạo bản sao để tránh lỗi GDI+
                    return new Bitmap(img);
                }

                // Nếu resource là byte array (từ Resources.resx)
                if (resource is byte[] bytes && bytes.Length > 0)
                {
                    using (var ms = new System.IO.MemoryStream(bytes))
                    {
                        var image = Image.FromStream(ms);
                        // Tạo bản sao để stream có thể đóng an toàn
                        return new Bitmap(image);
                    }
                }

                Console.WriteLine($"⚠️ Unknown resource type: {resource.GetType()}");
                return CreatePlaceholderImage();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading image: {ex.Message}");
                return CreatePlaceholderImage();
            }
        }

        private Image CreatePlaceholderImage()
        {
            try
            {
                var bmp = new Bitmap(100, 150);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.FromArgb(60, 40, 25));

                    // Draw border
                    using (var pen = new Pen(Color.Gold, 2))
                    {
                        g.DrawRectangle(pen, 1, 1, 98, 148);
                    }

                    // Draw text
                    using (var font = new Font("Arial", 40, FontStyle.Bold))
                    using (var brush = new SolidBrush(Color.White))
                    {
                        var format = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString("?", font, brush, new RectangleF(0, 0, 100, 150), format);
                    }
                }
                return bmp;
            }
            catch
            {
                // Fallback nếu tạo placeholder cũng lỗi
                return new Bitmap(100, 150);
            }
        }

        private void InitializeUI()
        {
            this.Text = $"Select Your Character - {username}";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = primaryBrown;

            // Main Panel
            mainPanel = new Pnl_Pixel
            {
                Location = new Point(20, 20),
                Size = new Size(850, 630),
                BackColor = darkBrown
            };
            this.Controls.Add(mainPanel);

            // Title
            lblTitle = new Label
            {
                Text = "⚔️ SELECT YOUR FIGHTER ⚔️",
                Font = new Font("Courier New", 20, FontStyle.Bold),
                ForeColor = goldColor,
                BackColor = Color.Transparent,
                Size = new Size(810, 40),
                Location = new Point(20, 15),
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblTitle);

            // Character List Panel (left side)
            characterListPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 70),
                Size = new Size(350, 490),
                BackColor = Color.FromArgb(80, 50, 30),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            mainPanel.Controls.Add(characterListPanel);

            // Create character buttons
            for (int i = 0; i < characters.Count; i++)
            {
                var charPanel = CreateCharacterButton(characters[i], i);
                characterListPanel.Controls.Add(charPanel);
            }

            // Preview Panel (right side)
            previewPanel = new Panel
            {
                Location = new Point(390, 70),
                Size = new Size(440, 490),
                BackColor = Color.FromArgb(90, 60, 40),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(previewPanel);

            // Preview Image
            pbPreview = new PictureBox
            {
                Location = new Point(120, 20),
                Size = new Size(200, 200),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(60, 40, 25)
            };
            previewPanel.Controls.Add(pbPreview);

            // Character Name
            lblCharName = new Label
            {
                Location = new Point(20, 230),
                Size = new Size(400, 35),
                Font = new Font("Courier New", 16, FontStyle.Bold),
                ForeColor = goldColor,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            previewPanel.Controls.Add(lblCharName);

            // Stats Panel
            statsPanel = new Panel
            {
                Location = new Point(20, 270),
                Size = new Size(400, 120),
                BackColor = Color.FromArgb(70, 45, 25)
            };
            previewPanel.Controls.Add(statsPanel);

            lblStats = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(380, 100),
                Font = new Font("Courier New", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            statsPanel.Controls.Add(lblStats);

            // Skill Description
            lblSkillDesc = new Label
            {
                Location = new Point(20, 400),
                Size = new Size(400, 80),
                Font = new Font("Courier New", 8, FontStyle.Regular),
                ForeColor = Color.LightGoldenrodYellow,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopCenter
            };
            previewPanel.Controls.Add(lblSkillDesc);

            // Confirm Button
            btnConfirm = new Btn_Pixel
            {
                Text = "✓ CONFIRM",
                Location = new Point(500, 575),
                Size = new Size(160, 45),
                BtnColor = Color.FromArgb(34, 139, 34),
                Font = new Font("Courier New", 12, FontStyle.Bold),
                ForeColor = Color.White
            };
            btnConfirm.Click += BtnConfirm_Click;
            mainPanel.Controls.Add(btnConfirm);

            // Back Button
            btnBack = new Btn_Pixel
            {
                Text = "← BACK",
                Location = new Point(680, 575),
                Size = new Size(140, 45),
                BtnColor = Color.FromArgb(178, 34, 34),
                Font = new Font("Courier New", 12, FontStyle.Bold),
                ForeColor = Color.White
            };
            btnBack.Click += BtnBack_Click;
            mainPanel.Controls.Add(btnBack);

            // Room info label
            var lblRoomInfo = new Label
            {
                Text = $"Room: {roomCode} | VS: {opponentName}",
                Location = new Point(20, 580),
                Size = new Size(400, 30),
                Font = new Font("Courier New", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(lblRoomInfo);
        }

        private Panel CreateCharacterButton(CharacterInfo character, int index)
        {
            var panel = new Panel
            {
                Size = new Size(320, 90),
                Margin = new Padding(5),
                BackColor = Color.FromArgb(100, 65, 40),
                Cursor = Cursors.Hand,
                Tag = index
            };

            var picBox = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(70, 70),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = character.PreviewImage,
                BackColor = Color.Transparent
            };
            panel.Controls.Add(picBox);

            var lblName = new Label
            {
                Text = character.DisplayName,
                Location = new Point(90, 15),
                Size = new Size(220, 25),
                Font = new Font("Courier New", 11, FontStyle.Bold),
                ForeColor = goldColor,
                BackColor = Color.Transparent
            };
            panel.Controls.Add(lblName);

            var lblType = new Label
            {
                Text = GetCharacterType(character),
                Location = new Point(90, 40),
                Size = new Size(220, 20),
                Font = new Font("Courier New", 8),
                ForeColor = Color.LightGray,
                BackColor = Color.Transparent
            };
            panel.Controls.Add(lblType);

            var lblStats = new Label
            {
                Text = $"HP:{character.HP} | DMG:{character.Damage} | SPD:{character.Speed}",
                Location = new Point(90, 60),
                Size = new Size(220, 20),
                Font = new Font("Courier New", 7),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            panel.Controls.Add(lblStats);

            // Event handlers
            panel.Click += (s, e) => SelectCharacter(index);
            panel.MouseEnter += (s, e) => panel.BackColor = Color.FromArgb(130, 85, 50);
            panel.MouseLeave += (s, e) =>
            {
                panel.BackColor = index == selectedIndex
                    ? Color.FromArgb(150, 100, 60)
                    : Color.FromArgb(100, 65, 40);
            };

            // Make child controls pass click to parent
            foreach (Control ctrl in panel.Controls)
            {
                ctrl.Click += (s, e) => SelectCharacter(index);
                ctrl.MouseEnter += (s, e) => panel.BackColor = Color.FromArgb(130, 85, 50);
                ctrl.MouseLeave += (s, e) =>
                {
                    panel.BackColor = index == selectedIndex
                        ? Color.FromArgb(150, 100, 60)
                        : Color.FromArgb(100, 65, 40);
                };
            }

            return panel;
        }

        private string GetCharacterType(CharacterInfo character)
        {
            if (character.Damage >= 115) return "⚔️ Damage Dealer";
            if (character.HP >= 120) return "🛡️ Tank";
            if (character.Speed >= 100) return "⚡ Speedster";
            return "⚖️ Balanced";
        }

        private void SelectCharacter(int index)
        {
            selectedIndex = index;
            UpdatePreview();
            HighlightSelectedCharacter();
        }

        private void HighlightSelectedCharacter()
        {
            for (int i = 0; i < characterListPanel.Controls.Count; i++)
            {
                var panel = characterListPanel.Controls[i] as Panel;
                if (panel != null)
                {
                    panel.BackColor = i == selectedIndex
                        ? Color.FromArgb(150, 100, 60)
                        : Color.FromArgb(100, 65, 40);
                }
            }
        }

        private void UpdatePreview()
        {
            if (selectedIndex < 0 || selectedIndex >= characters.Count) return;

            var character = characters[selectedIndex];
            pbPreview.Image = character.PreviewImage;
            lblCharName.Text = character.DisplayName;
            lblSkillDesc.Text = character.SkillDescription;

            lblStats.Text = $"❤️ HP:      {CreateStatBar(character.HP, 130)}\n" +
                           $"⚔️ Damage:  {CreateStatBar(character.Damage, 130)}\n" +
                           $"⚡ Speed:   {CreateStatBar(character.Speed, 130)}\n" +
                           $"💪 Stamina: {CreateStatBar(character.Stamina, 130)}\n" +
                           $"🔮 Mana:    {CreateStatBar(character.Mana, 130)}";
        }

        private string CreateStatBar(int value, int maxValue)
        {
            int bars = (int)((value / (float)maxValue) * 10);
            bars = Math.Max(1, Math.Min(10, bars));
            return new string('█', bars) + new string('░', 10 - bars) + $" {value}";
        }

        private async void BtnConfirm_Click(object sender, EventArgs e)
        {
            SelectedCharacter = characters[selectedIndex].Name;

            // ✅ Disable button and show feedback
            btnConfirm.Enabled = false;
            btnConfirm.Text = "⏳ SENDING...";

            try
            {
                // ✅ USE THE CORRECT OVERLOAD (action, data)
                var response = await PersistentTcpClient.Instance.SendRequestAsync(
                    "SELECT_CHARACTER",
                    new Dictionary<string, object>
                    {
                        { "roomCode", roomCode },
                        { "username", username },
                        { "character", SelectedCharacter }
                    }
                );

                if (response.Success)
                {
                    // ✅ Show success feedback
                    btnConfirm.Text = "✓ CONFIRMED";
                    btnConfirm.BackColor = Color.FromArgb(100, 200, 100);
                    Console.WriteLine($"[CharacterSelectForm] SELECT_CHARACTER sent successfully: {SelectedCharacter}");
                    
                    // ✅ Wait for server START_GAME (handled in TcpClient_OnBroadcast)
                    btnConfirm.Enabled = false;
                }
                else
                {
                    Console.WriteLine($"[CharacterSelectForm] SELECT_CHARACTER failed: {response.Message}");
                    MessageBox.Show($"Failed to send character select: {response.Message}", "Error");
                    
                    // Re-enable button on error
                    btnConfirm.Enabled = true;
                    btnConfirm.Text = "✓ CONFIRM";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CharacterSelectForm] Error sending SELECT_CHARACTER: {ex.Message}");
                MessageBox.Show("Network error when sending character select.", "Error");
                
                // Re-enable button on error
                btnConfirm.Enabled = true;
                btnConfirm.Text = "✓ CONFIRM";
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            // Unregister before leaving
            PersistentTcpClient.Instance.OnBroadcast -= TcpClient_OnBroadcast;
            
            // Quay lại GameLobbyForm
            var result = MessageBox.Show(
                "Are you sure you want to go back to the lobby?",
                "Go Back",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                // Tạo lại GameLobbyForm với thông tin phòng
                var lobbyForm = new PixelGameLobby.GameLobbyForm(roomCode, username, token);
                lobbyForm.FormClosed += (s, args) =>
                {
                    // Nếu đóng lobby thì thoát hoàn toàn
                    Application.Exit();
                };

                lobbyForm.Show();
                this.Close();
            }
        }
        
        // ✅ CLEANUP
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                PersistentTcpClient.Instance.OnBroadcast -= TcpClient_OnBroadcast;
                Console.WriteLine("[CharacterSelectForm] Unregistered from TCP broadcasts");
            }
            catch { }
            
            base.OnFormClosing(e);
        }
    }

}