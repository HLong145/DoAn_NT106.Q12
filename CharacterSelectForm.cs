using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

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
        private string roomCode;
        private string opponentName;
        private bool isHost;
        private string selectedMap = "battleground1";

        private List<CharacterInfo> characters = new List<CharacterInfo>();
        private int selectedIndex = 0;

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

        private Color primaryBrown = Color.FromArgb(160, 82, 45);
        private Color darkBrown = Color.FromArgb(101, 67, 51);
        private Color goldColor = Color.FromArgb(255, 215, 0);

        public string SelectedCharacter { get; private set; }

        private int myPlayerNumber = 1;
        private string opponent1Character = string.Empty;
        private string opponent2Character = string.Empty;

        public CharacterSelectForm(string username, string token, string roomCode, string opponentName, bool isHost = true, string selectedMap = "battleground1", int myPlayerNumber = 1)
        {
            this.myPlayerNumber = myPlayerNumber;
            this.username = username;
            this.token = token;
            this.roomCode = roomCode;
            this.opponentName = opponentName;
            this.isHost = isHost;
            this.selectedMap = selectedMap;

            InitializeCharacters();
            InitializeUI();
            UpdatePreview();

            SubscribeToBroadcasts();
        }

        private void SubscribeToBroadcasts()
        {
            try
            {
                var tcpClient = Services.PersistentTcpClient.Instance;
                try { tcpClient.OnBroadcast -= HandleServerBroadcast; } catch { }
                tcpClient.OnBroadcast += HandleServerBroadcast;

                this.FormClosed += (s, e) =>
                {
                    try { tcpClient.OnBroadcast -= HandleServerBroadcast; } catch { }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CharacterSelectForm] Error subscribing: {ex.Message}");
            }
        }

        private void HandleServerBroadcast(string action, System.Text.Json.JsonElement data)
        {
            try
            {
                if (data.TryGetProperty("roomCode", out var rcEl))
                {
                    string broadcastRoomCode = rcEl.GetString();
                    if (broadcastRoomCode != roomCode) return;
                }

                if (action == "BOTH_CHARACTERS_READY")
                {
                    if (data.TryGetProperty("player1Character", out var p1)) opponent1Character = p1.GetString() ?? "girlknight";
                    if (data.TryGetProperty("player2Character", out var p2)) opponent2Character = p2.GetString() ?? "girlknight";

                    string opponentCharacter = myPlayerNumber == 1 ? opponent2Character : opponent1Character;
                    if (string.IsNullOrEmpty(opponentCharacter)) opponentCharacter = "girlknight";

                    this.Invoke(() => OpenBattleForm(opponentCharacter));
                }
                else if (action == "CHARACTER_SELECTED")
                {
                    if (data.TryGetProperty("username", out var u) && data.TryGetProperty("character", out var c))
                    {
                        string selUser = u.GetString();
                        string selChar = c.GetString();
                        if (selUser != username)
                        {
                            this.Invoke(() => lblTitle.Text = $"⚔️ {selUser} SELECTED {selChar.ToUpper()} ⚔️");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CharacterSelectForm] HandleServerBroadcast error: {ex.Message}");
            }
        }

        private void InitializeCharacters()
        {
            characters.Clear();
            characters.Add(new CharacterInfo { Name = "girlknight", DisplayName = "Girl Knight", HP = 100, Damage = 100, Speed = 100, Stamina = 100, Mana = 100, SkillDescription = "⚡Sword Spinning", PreviewImage = SafeLoadImage(Properties.Resources.Knightgirl_Idle) });
            characters.Add(new CharacterInfo { Name = "bringerofdeath", DisplayName = "Bringer of Death", HP = 90, Damage = 120, Speed = 90, Stamina = 100, Mana = 100, SkillDescription = "💀 Dark Spell", PreviewImage = SafeLoadImage(Properties.Resources.BringerofDeath_Idle) });
            characters.Add(new CharacterInfo { Name = "goatman", DisplayName = "Goatman Berserker", HP = 130, Damage = 100, Speed = 80, Stamina = 100, Mana = 100, SkillDescription = "🐐 Wild Charge", PreviewImage = SafeLoadImage(Properties.Resources.GM_Idle) });
            characters.Add(new CharacterInfo { Name = "warrior", DisplayName = "Elite Warrior", HP = 80, Damage = 105, Speed = 120, Stamina = 100, Mana = 100, SkillDescription = "⚔️ Energy Wave", PreviewImage = SafeLoadImage(Properties.Resources.Warrior_Idle) });
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

                // If resource is Image
                if (resource is Image img)
                {
                    // return a copy to avoid GDI+ shared handle issues
                    return new Bitmap(img);
                }

                // If resource is byte[] (some resx store images as bytes)
                if (resource is byte[] bytes && bytes.Length > 0)
                {
                    using (var ms = new System.IO.MemoryStream(bytes))
                    {
                        var image = Image.FromStream(ms);
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

        private Image CloneImage(Image img)
        {
            try
            {
                if (img == null) return CreatePlaceholderImage();
                return new Bitmap(img);
            }
            catch
            {
                return CreatePlaceholderImage();
            }
        }

        private Image CreatePlaceholderImage()
        {
            var bmp = new Bitmap(100, 150);
            using (var g = Graphics.FromImage(bmp)) { g.Clear(Color.Gray); }
            return bmp;
        }

        private void InitializeUI()
        {
            this.Text = $"Select Your Character - {username}";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            mainPanel = new Pnl_Pixel { Location = new Point(20, 20), Size = new Size(850, 630), BackColor = darkBrown };
            this.Controls.Add(mainPanel);

            lblTitle = new Label { Text = "⚔️ SELECT YOUR FIGHTER ⚔️", Font = new Font("Courier New", 16, FontStyle.Bold), ForeColor = goldColor, Size = new Size(810, 40), Location = new Point(20, 15), TextAlign = ContentAlignment.MiddleCenter };
            mainPanel.Controls.Add(lblTitle);

            characterListPanel = new FlowLayoutPanel { Location = new Point(20, 70), Size = new Size(350, 490), BackColor = Color.FromArgb(80, 50, 30), AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            mainPanel.Controls.Add(characterListPanel);

            for (int i = 0; i < characters.Count; i++) characterListPanel.Controls.Add(CreateCharacterButton(characters[i], i));
            // highlight default selection
            HighlightSelectedCharacter();

            previewPanel = new Panel { Location = new Point(390, 70), Size = new Size(440, 490), BackColor = Color.FromArgb(90, 60, 40), BorderStyle = BorderStyle.FixedSingle };
            mainPanel.Controls.Add(previewPanel);

            pbPreview = new PictureBox { Location = new Point(120, 20), Size = new Size(200, 200), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(60, 40, 25) };
            previewPanel.Controls.Add(pbPreview);

            lblCharName = new Label { Location = new Point(20, 230), Size = new Size(400, 35), Font = new Font("Courier New", 16, FontStyle.Bold), ForeColor = goldColor, TextAlign = ContentAlignment.MiddleCenter };
            previewPanel.Controls.Add(lblCharName);

            statsPanel = new Panel { Location = new Point(20, 270), Size = new Size(400, 120), BackColor = Color.FromArgb(70, 45, 25) };
            lblStats = new Label { Location = new Point(10, 10), Size = new Size(380, 100), Font = new Font("Courier New", 10, FontStyle.Bold), ForeColor = Color.White };
            statsPanel.Controls.Add(lblStats); previewPanel.Controls.Add(statsPanel);

            lblSkillDesc = new Label { Location = new Point(20, 400), Size = new Size(400, 80), Font = new Font("Courier New", 8), ForeColor = Color.LightGoldenrodYellow, TextAlign = ContentAlignment.TopCenter };
            previewPanel.Controls.Add(lblSkillDesc);

            btnConfirm = new Btn_Pixel { Text = "✓ CONFIRM", Location = new Point(500, 575), Size = new Size(160, 45) }; btnConfirm.Click += BtnConfirm_Click; mainPanel.Controls.Add(btnConfirm);
            btnBack = new Btn_Pixel { Text = "← BACK", Location = new Point(680, 575), Size = new Size(140, 45) }; btnBack.Click += BtnBack_Click; mainPanel.Controls.Add(btnBack);

            var lblRoomInfo = new Label { Text = $"Room: {roomCode} | VS: {opponentName}", Location = new Point(20, 580), Size = new Size(400, 30), Font = new Font("Courier New", 10, FontStyle.Bold), ForeColor = Color.White };
            mainPanel.Controls.Add(lblRoomInfo);

            UpdatePreview();
        }

        private Panel CreateCharacterButton(CharacterInfo character, int index)
        {
            var panel = new Panel { Size = new Size(320, 90), Margin = new Padding(5), BackColor = Color.FromArgb(100, 65, 40), Cursor = Cursors.Hand, Tag = index };
            var picBox = new PictureBox { Location = new Point(10, 10), Size = new Size(70, 70), SizeMode = PictureBoxSizeMode.Zoom, Image = CloneImage(character.PreviewImage), BackColor = Color.Transparent };
            panel.Controls.Add(picBox);
            var lblName = new Label { Text = character.DisplayName, Location = new Point(90, 15), Size = new Size(220, 25), Font = new Font("Courier New", 11, FontStyle.Bold), ForeColor = goldColor };
            panel.Controls.Add(lblName);
            var lblType = new Label { Text = GetCharacterType(character), Location = new Point(90, 40), Size = new Size(220, 20), Font = new Font("Courier New", 8), ForeColor = Color.LightGray };
            panel.Controls.Add(lblType);
            var lblStatsLocal = new Label { Text = $"HP:{character.HP} | DMG:{character.Damage} | SPD:{character.Speed}", Location = new Point(90, 60), Size = new Size(220, 20), Font = new Font("Courier New", 7), ForeColor = Color.White };
            panel.Controls.Add(lblStatsLocal);

            panel.Click += (s, e) => SelectCharacter(index);
            panel.MouseEnter += (s, e) => panel.BackColor = Color.FromArgb(130, 85, 50);
            panel.MouseLeave += (s, e) => panel.BackColor = index == selectedIndex ? Color.FromArgb(150, 100, 60) : Color.FromArgb(100, 65, 40);

            foreach (Control ctrl in panel.Controls)
            {
                ctrl.Click += (s, e) => SelectCharacter(index);
                ctrl.MouseEnter += (s, e) => panel.BackColor = Color.FromArgb(130, 85, 50);
                ctrl.MouseLeave += (s, e) => panel.BackColor = index == selectedIndex ? Color.FromArgb(150, 100, 60) : Color.FromArgb(100, 65, 40);
            }

            return panel;
        }

        private void HighlightSelectedCharacter()
        {
            for (int i = 0; i < characterListPanel.Controls.Count; i++)
            {
                if (characterListPanel.Controls[i] is Panel p)
                {
                    p.BackColor = i == selectedIndex ? Color.FromArgb(150, 100, 60) : Color.FromArgb(100, 65, 40);
                }
            }
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

        private void UpdatePreview()
        {
            if (selectedIndex < 0 || selectedIndex >= characters.Count) return;
            var character = characters[selectedIndex];
            // Replace preview image with a clone to avoid shared/disposed image issues
            var newImg = CloneImage(character.PreviewImage);
            var old = pbPreview.Image;
            pbPreview.Image = newImg;
            try { old?.Dispose(); } catch { }
            lblCharName.Text = character.DisplayName;
            lblSkillDesc.Text = character.SkillDescription;
            lblStats.Text = $"❤️ HP: {character.HP}  ⚔️ DMG: {character.Damage}  ⚡ SPD: {character.Speed}";
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            // Lấy character đã chọn và gửi lên server. Không mở trận tại đây.
            SelectedCharacter = characters[selectedIndex].Name;

            Console.WriteLine($"[CharacterSelectForm] Sending CHARACTER_SELECTED: {SelectedCharacter}");
            SendCharacterSelectedToServer(SelectedCharacter);

            // Disable confirm để tránh gửi nhiều lần
            btnConfirm.Enabled = false;
            btnConfirm.Text = "⏳ WAITING FOR OPPONENT...";
        }

        private void SendCharacterSelectedToServer(string character)
        {
            try
            {
                var tcpClient = Services.PersistentTcpClient.Instance;
                if (!tcpClient.IsConnected)
                {
                    MessageBox.Show("❌ Not connected to server!");
                    return;
                }

                var data = new Dictionary<string, object>
                {
                    { "roomCode", roomCode },
                    { "username", username },
                    { "character", character }
                };

                // Fire-and-forget - server sẽ broadcast BOTH_CHARACTERS_READY khi cả hai đã chọn
                _ = tcpClient.SendRequestAsync("CHARACTER_SELECTED", data).ContinueWith(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        var response = task.Result;
                        Console.WriteLine($"[CharacterSelectForm] CHARACTER_SELECTED response: {response.Success}");
                    }
                    else if (task.IsFaulted)
                    {
                        Console.WriteLine($"[CharacterSelectForm] CHARACTER_SELECTED task failed: {task.Exception?.GetBaseException().Message}");
                    }
                });

                Console.WriteLine($"[CharacterSelectForm] CHARACTER_SELECTED sent: {character}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error sending CHARACTER_SELECTED: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to go back to the lobby?", "Go Back", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            if (this.Owner is Form ownerForm)
            {
                try { ownerForm.Show(); ownerForm.BringToFront(); } catch { }
            }
            else
            {
                // Find existing lobby for this room and user and show it. Do NOT create a new instance to avoid duplicates.
                var existing = Application.OpenForms.OfType<PixelGameLobby.GameLobbyForm>()
                    .FirstOrDefault(f => string.Equals(((dynamic)f).RoomCode, roomCode, StringComparison.OrdinalIgnoreCase) && (string.Equals(((dynamic)f).Username, username, StringComparison.OrdinalIgnoreCase)));

                if (existing != null)
                {
                    existing.Show();
                    existing.BringToFront();
                }
                else
                {
                    Console.WriteLine("[CharacterSelectForm] No matching existing lobby form found; not creating a new one.");
                }
            }

            this.Close();
        }

        private void OpenBattleForm(string opponentCharacter)
        {
            try
            {
                Console.WriteLine($"[CharacterSelectForm] Opening BattleForm for Player {myPlayerNumber}");

                string player1Char, player2Char;
                if (myPlayerNumber == 1) { player1Char = SelectedCharacter; player2Char = opponentCharacter; }
                else { player1Char = opponentCharacter; player2Char = SelectedCharacter; }

                Console.WriteLine($"[CharacterSelectForm] Creating BattleForm: map={selectedMap}, myPlayerNumber={myPlayerNumber}");
                var battleForm = new BattleForm(username, token, opponentName, player1Char, player2Char, selectedMap, roomCode, myPlayerNumber);

                battleForm.FormClosed += (s, args) =>
                {
                    try
                    {
                        var existing = Application.OpenForms.OfType<PixelGameLobby.GameLobbyForm>().FirstOrDefault();
                        if (existing != null) { existing.Show(); existing.BringToFront(); }
                        else { var lobbyForm = new PixelGameLobby.GameLobbyForm(roomCode, username, token); lobbyForm.Show(); }
                    }
                    catch { try { var lobbyForm = new PixelGameLobby.GameLobbyForm(roomCode, username, token); lobbyForm.Show(); } catch { } }
                };

                Console.WriteLine($"[CharacterSelectForm] Showing BattleForm now...");
                battleForm.Show();
                battleForm.BringToFront();
                battleForm.Activate();
                
                // ✅ FIX: Hide CharacterSelectForm immediately and close it
                this.Hide();
                
                // Close CharacterSelectForm after a short delay to ensure BattleForm is ready
                var closeTimer = new System.Windows.Forms.Timer { Interval = 100 };
                closeTimer.Tick += (s, e) => { closeTimer.Stop(); this.Close(); };
                closeTimer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error opening BattleForm: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}");
                btnConfirm.Enabled = true; btnConfirm.Text = "✓ CONFIRM";
            }
        }
    }
}