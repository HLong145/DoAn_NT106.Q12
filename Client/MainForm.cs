using DoAn_NT106.Services;
using DoAn_NT106.Client;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;
using DoAn_NT106.Client.Class;

namespace DoAn_NT106.Client
{
    public partial class MainForm : Form
    {
        private FormDangNhap frm_DangNhap;
        private FormDangKy frm_DangKy;
        private Panel pnl_Overlay;
        private string username;
        private string token;
        private bool isLoggedIn = false;
        private readonly PersistentTcpClient tcpClient;
        private System.Windows.Forms.Timer rainTimer;
        private List<Particle> particles = new List<Particle>();
        private Random rand = new Random();
        // Avatar hint label (declared in Designer)
        private ToolTip avatarToolTip;

        public class Particle
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Speed { get; set; }
            public int Size { get; set; }
            public Color Color { get; set; }
            public bool IsStar { get; set; }
        }

        private void BtnLeaderboard_Click(object sender, EventArgs e)
        {
            try
            {
                var lb = new LeaderBoardForm();
                lb.ShowDialog();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening LeaderBoard: " + ex.Message);
            }
        }

        private void BtnMusic_Click(object sender, EventArgs e)
        {
            try
            {
                bool newState = !SoundManager.MusicEnabled;
                ToggleMusic(newState);
                btnMusic.Text = newState ? "Music: On" : "Music: Off";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Toggle music error: " + ex.Message);
            }
        }

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomUI();
            // Make avatar larger for better visibility and center it in sidebar
            try
            {
                pbAvatar.Size = new Size(260, 260);
                pbAvatar.SizeMode = PictureBoxSizeMode.Zoom;
                pbAvatar.BorderStyle = BorderStyle.None; // Xóa đường viền
                // center horizontally inside panelSidebar
                pbAvatar.Left = Math.Max(8, (panelSidebar.ClientSize.Width - pbAvatar.Width) / 2);
                pbAvatar.Top = 20;
                // center username label under avatar
                try
                {
                    lblUserName.Size = new Size(Math.Min(314, panelSidebar.ClientSize.Width - 20), lblUserName.Height);
                    lblUserName.Left = (panelSidebar.ClientSize.Width - lblUserName.Width) / 2;
                    lblUserName.Top = pbAvatar.Bottom + 12;
                }
                catch { }
                // keep username centered if sidebar resizes
                panelSidebar.SizeChanged += (s, e) =>
                {
                    try
                    {
                        lblUserName.Left = (panelSidebar.ClientSize.Width - lblUserName.Width) / 2;
                    }
                    catch { }
                };
            }
            catch { }

            pbAvatar.Cursor = Cursors.Hand;

            // Hint label (created in Designer) - ensure initial state
            try
            {
                lblAvatarHint.Text = ""; // empty by default
                lblAvatarHint.ForeColor = Color.LightGoldenrodYellow;
                lblAvatarHint.BackColor = Color.Transparent;
                lblAvatarHint.Font = new Font("Courier New", 9, FontStyle.Italic);
                lblAvatarHint.Visible = false;
            }
            catch { }

            avatarToolTip = new ToolTip();
            avatarToolTip.SetToolTip(pbAvatar, "Click to change avatar");

            // Show hint on hover - position above avatar to not block player name
            pbAvatar.MouseEnter += (s, e) =>
            {
                try
                {
                    lblAvatarHint.Text = "Click to change avatar";
                    lblAvatarHint.Location = new Point(pbAvatar.Left + 10, pbAvatar.Top - 20);
                    lblAvatarHint.Visible = true;
                }
                catch { }
                pbAvatar.BorderStyle = BorderStyle.None;
            };
            pbAvatar.MouseLeave += (s, e) => { lblAvatarHint.Visible = false; pbAvatar.BorderStyle = BorderStyle.None; };
            this.FormClosing += MainForm_FormClosing;

            // ✅ Initialize Sound Manager khi MainForm khởi tạo
            SoundManager.Initialize();
            Console.WriteLine("✅ SoundManager initialized in MainForm");

            // ✅ Start global UI button sound wiring
            UIAudioWiring.Start();

            // Audio system initialized
        }

        // Toggle music on/off (controls theme + battleground music). Other sound effects unaffected.
        private void ToggleMusic(bool enabled)
        {
            SoundManager.MusicEnabled = enabled;
            if (!enabled)
                SoundManager.StopMusic();
            else
                SoundManager.PlayMusic(BackgroundMusic.ThemeMusic, loop: true);
        }

        public MainForm(string username, string token) : this()
        {
            this.username = username;
            this.token = token;
            this.isLoggedIn = true;

            tcpClient = PersistentTcpClient.Instance;
            UpdateUsernameDisplay(username);

            this.Load += (s, e) =>
            {
                InitializeRainEffect();
                
                // ✅ Play theme music khi MainForm load (logged in)
                if (SoundManager.MusicEnabled)
                {
                    SoundManager.PlayMusic(BackgroundMusic.ThemeMusic, loop: true);
                    Console.WriteLine("🎵 Theme music started");
                }
            };

            LoadUserAvatar();
        }

        // ✅ KHỞI TẠO HIỆU ỨNG HẠT RƠI - ĐÃ SỬA
        private void InitializeRainEffect()
        {
            // Dừng timer cũ nếu có
            if (rainTimer != null)
            {
                rainTimer.Stop();
                rainTimer.Dispose();
            }

            rainTimer = new System.Windows.Forms.Timer();
            rainTimer.Interval = 30;
            rainTimer.Tick += RainTimer_Tick;

            // ✅ THÊM SỰ KIỆN PAINT CHO PANEL MAIN CONTENT
            panelMainContent.Paint += PanelMainContent_Paint;

            // Xóa particles cũ
            particles.Clear();

            // Tạo hạt mới
            for (int i = 0; i < 50; i++) // Tăng số lượng hạt
            {
                CreateNewParticle();
            }

            rainTimer.Start();
        }

        // ✅ SỰ KIỆN VẼ CHO PANEL - QUAN TRỌNG
        private void PanelMainContent_Paint(object sender, PaintEventArgs e)
        {
            DrawParticles(e.Graphics);
        }

        // ✅ TẠO HẠT MỚI - ĐÃ SỬA
        private void CreateNewParticle()
        {
            particles.Add(new Particle
            {
                X = rand.Next(-50, panelMainContent.Width + 50), // Mở rộng phạm vi
                Y = -rand.Next(0, 200), // Bắt đầu từ trên màn hình
                Speed = rand.Next(2, 6), // Tốc độ đa dạng
                Size = rand.Next(2, 5), // Kích thước lớn hơn
                Color = GetRandomParticleColor(),
                IsStar = rand.Next(0, 100) < 20 // 20% là ngôi sao
            });
        }

        // ✅ MÀU NGẪU NHIÊN CHO HẠT
        private Color GetRandomParticleColor()
        {
            Color[] colors = {
                Color.FromArgb(200, 255, 255, 255), // Trắng - tăng độ trong
                Color.FromArgb(180, 255, 255, 150), // Vàng nhạt
                Color.FromArgb(180, 150, 255, 255), // Xanh nhạt
                Color.FromArgb(180, 255, 150, 255), // Hồng nhạt
            };
            return colors[rand.Next(colors.Length)];
        }

        // ✅ DI CHUYỂN HẠT - ĐÃ SỬA
        private void MoveParticles()
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var p = particles[i];
                p.Y += p.Speed;

                // Thêm chuyển động ngang nhẹ cho tự nhiên
                p.X += rand.Next(-1, 2);

                // Xóa hạt đã rơi ra khỏi màn hình
                if (p.Y > panelMainContent.Height + 50)
                {
                    particles.RemoveAt(i);
                    CreateNewParticle(); // Tạo hạt mới thay thế
                }
            // Ensure music button initial state
            try
            {
                btnMusic.Text = SoundManager.MusicEnabled ? "Music: On" : "Music: Off";
            }
            catch { }
            }
        }

        // ✅ VẼ HẠT RƠI
        private void RainTimer_Tick(object sender, EventArgs e)
        {
            MoveParticles();
            panelMainContent.Invalidate(); // QUAN TRỌNG: Kích hoạt vẽ lại
        }

        // ✅ VẼ HẠT LÊN PANEL - ĐÃ SỬA
        private void DrawParticles(Graphics g)
        {
            foreach (var p in particles)
            {
                using (var brush = new SolidBrush(p.Color))
                {
                    if (p.IsStar && p.Size > 2)
                    {
                        // Chỉ vẽ sao cho hạt đủ lớn
                        try
                        {
                            var starPoints = CreateStarPoints(p.X, p.Y, p.Size);
                            g.FillPolygon(brush, starPoints);
                        }
                        catch
                        {
                            // Nếu lỗi vẽ sao, vẽ hình tròn thay thế
                            g.FillEllipse(brush, p.X, p.Y, p.Size, p.Size);
                        }
                    }
                    else
                    {
                        // Vẽ hình tròn
                        g.FillEllipse(brush, p.X, p.Y, p.Size, p.Size);

                        // Thêm viền sáng cho nổi bật
                        using (var pen = new Pen(Color.FromArgb(100, 255, 255, 255), 1))
                        {
                            g.DrawEllipse(pen, p.X, p.Y, p.Size, p.Size);
                        }
                    }
                }
            }
        }

        // ✅ TẠO HÌNH NGÔI SAO NHỎ - ĐÃ SỬA
        private Point[] CreateStarPoints(int x, int y, int size)
        {
            var points = new Point[10];
            double[] angles = { 0, 36, 72, 108, 144, 180, 216, 252, 288, 324 }; // 10 điểm

            for (int i = 0; i < 10; i++)
            {
                double angle = angles[i] * Math.PI / 180;
                double radius = (i % 2 == 0) ? size : size / 2; // Điểm lồi lõm
                points[i] = new Point(
                    x + (int)(radius * Math.Cos(angle)),
                    y + (int)(radius * Math.Sin(angle))
                );
            }
            return points;
        }

        // ✅ XỬ LÝ KHI THAY ĐỔI KÍCH THƯỚC - THÊM MỚI
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // Điều chỉnh lại particles khi form resize
            if (rainTimer != null && rainTimer.Enabled)
            {
                panelMainContent.Invalidate();
            }
        }

        // ✅ DỪNG ANIMATION KHI ĐÓNG FORM - THÊM MỚI
        private void StopRainEffect()
        {
            if (rainTimer != null)
            {
                rainTimer.Stop();
                rainTimer.Dispose();
                rainTimer = null;
            }
            particles.Clear();

            // Gỡ sự kiện paint
            panelMainContent.Paint -= PanelMainContent_Paint;
        }

        // ✅ SỬA LẠI FORM CLOSING
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopRainEffect(); // Dừng animation
            frm_DangNhap?.Close();
            frm_DangKy?.Close();
            
            // ✅ STOP MUSIC when closing app
            SoundManager.StopMusic();
            SoundManager.Cleanup();
            Console.WriteLine("🛑 Sound system cleaned up");
        }

        // ✅ PHƯƠNG THỨC CẬP NHẬT USERNAME
        public void UpdateUsernameDisplay(string newUsername)
        {
            username = newUsername;

            if (!string.IsNullOrEmpty(username))
            {
                if (lblUserName != null)
                {
                    lblUserName.Text = username.ToUpper();
                }
                this.Text = $"Adventure App - Welcome {username}";

                if (pnl_Overlay != null)
                {
                    pnl_Overlay.Visible = false;
                }
            }
        }

        private void InitializeCustomUI()
        {
            this.Text = "Adventure Login / Register";
            this.ClientSize = new Size(1312, 742);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            try
            {
                this.BackgroundImage = new Bitmap("wood_texture.jpg");
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }
            catch
            {
                this.BackColor = Color.FromArgb(34, 25, 18);
            }

            if (!isLoggedIn)
            {
                pnl_Overlay = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(100, 0, 0, 0)
                };
                this.Controls.Add(pnl_Overlay);
                pnl_Overlay.BringToFront();
                InitializeLoginForms();
            }
        }

        private void InitializeLoginForms()
        {
            frm_DangNhap = new FormDangNhap
            {
                TopLevel = false,
                Dock = DockStyle.Fill,
                FormBorderStyle = FormBorderStyle.None
            };
            frm_DangNhap.SwitchToRegister += OnSwitchToDangKy;
            // ✅ Button sound for login form buttons
            frm_DangNhap.Load += (s, e) =>
            {
                WireButtonClickSounds(frm_DangNhap);
            };

            frm_DangKy = new FormDangKy
            {
                TopLevel = false,
                Dock = DockStyle.Fill,
                FormBorderStyle = FormBorderStyle.None
            };
            frm_DangKy.SwitchToLogin += OnSwitchToDangNhap;
            // ✅ Button sound for register form buttons
            frm_DangKy.Load += (s, e) =>
            {
                WireButtonClickSounds(frm_DangKy);
            };

            pnl_Overlay.Controls.Add(frm_DangNhap);
            pnl_Overlay.Controls.Add(frm_DangKy);

            frm_DangNhap.Show();
            frm_DangKy.Hide();
            frm_DangNhap.BringToFront();
        }

        private void WireButtonClickSounds(Form form)
        {
            try
            {
                foreach (Control c in form.Controls)
                {
                    WireButtonClickRecursive(c);
                }
            }
            catch { }
        }

        private void WireButtonClickRecursive(Control c)
        {
            if (c is Button btn)
            {
                btn.Click -= ButtonPlaySound_Click;
                btn.Click += ButtonPlaySound_Click;
            }

            foreach (Control child in c.Controls)
            {
                WireButtonClickRecursive(child);
            }
        }

        private void ButtonPlaySound_Click(object sender, EventArgs e)
        {
            try { SoundManager.PlaySound(SoundEffect.ButtonClick); } catch { }
        }

        private void OnSwitchToDangNhap(object sender, EventArgs e)
        {
            frm_DangNhap.Show();
            frm_DangKy.Hide();
            frm_DangNhap.BringToFront();
        }

        private void OnSwitchToDangKy(object sender, EventArgs e)
        {
            frm_DangKy.Show();
            frm_DangNhap.Hide();
            frm_DangKy.BringToFront();
        }

        private async void btnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine($"🚪 Logging out user: {username}");

                if (Properties.Settings.Default.RememberMe)
                {
                    try
                    {
                        await tcpClient.LogoutAsync(token, "normal");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Normal logout error: {ex.Message}");
                    }
                    Properties.Settings.Default.SavedToken = "";
                    Properties.Settings.Default.Save();
                }
                else
                {
                    try
                    {
                        await tcpClient.LogoutAsync(token, "complete");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Complete logout error: {ex.Message}");
                    }
                    Properties.Settings.Default.RememberMe = false;
                    Properties.Settings.Default.SavedUsername = "";
                    Properties.Settings.Default.SavedPassword = "";
                    Properties.Settings.Default.SavedToken = "";
                    Properties.Settings.Default.Save();
                }

                FormDangNhap loginForm = new FormDangNhap();
                loginForm.StartPosition = FormStartPosition.CenterScreen;
                loginForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Logout error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_play_Click(object sender, EventArgs e)
        {
            JoinRoomForm joinForm = new JoinRoomForm(username, token);
            joinForm.Show();
            this.Hide();  // ĐÓNG hoàn toàn MainForm
        }

        //  Avatar available in game (use avt_* resources)
        private readonly Image[] gameAvatars =
        {
            Properties.Resources.avt_knightgirl,
            Properties.Resources.avt_bringer,
            Properties.Resources.avt_warrior,
            Properties.Resources.avt_goatman
        };

        private int currentAvatarIndex = 0;

        // ✅ CLICK: chỉ đổi avatar trong game
        private void PbAvatar_Click(object sender, EventArgs e)
        {
            try
            {
                if (gameAvatars == null || gameAvatars.Length == 0)
                    return;

                using (var selector = new AvatarSelectorForm(gameAvatars))
                {
                    // Show selector first, then apply selected index
                    if (selector.ShowDialog() == DialogResult.OK)
                    {
                        currentAvatarIndex = selector.SelectedIndex;

                        if (currentAvatarIndex >= 0 && currentAvatarIndex < gameAvatars.Length)
                        {
                            pbAvatar.Image = gameAvatars[currentAvatarIndex];
                            pbAvatar.SizeMode = PictureBoxSizeMode.StretchImage;

                            SaveAvatarForUser(username, currentAvatarIndex);
                            SendAvatarToServer(username, currentAvatarIndex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ PbAvatar_Click error: " + ex.Message);
            }
        }

        private void SaveAvatarForUser(string username, int avatarIndex)
        {
            try
            {
                string folder = "UserAvatars";
                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                string saveFile = System.IO.Path.Combine(folder, $"{username}.txt");
                System.IO.File.WriteAllText(saveFile, avatarIndex.ToString());

                Console.WriteLine($"✅ Saved avatar index for {username}: {avatarIndex}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Save avatar error: " + ex.Message);
            }
        }

        private async void SendAvatarToServer(string username, int avatarIndex)
        {
            try
            {
                if (tcpClient == null) return;

                using (var ms = new System.IO.MemoryStream())
                {
                    // ✅ LẤY ĐÚNG AVATAR
                    gameAvatars[avatarIndex].Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] imgBytes = ms.ToArray();
                    string base64Image = Convert.ToBase64String(imgBytes);

                    string packet = $"AVATAR|{username}|{avatarIndex}|{base64Image}\n";
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(packet);

                    var fields = typeof(PersistentTcpClient)
                        .GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    System.Net.Sockets.NetworkStream stream = null;

                    foreach (var f in fields)
                    {
                        if (f.FieldType == typeof(System.Net.Sockets.NetworkStream))
                        {
                            stream = (System.Net.Sockets.NetworkStream)f.GetValue(tcpClient);
                            break;
                        }
                        if (f.FieldType == typeof(System.Net.Sockets.TcpClient))
                        {
                            var tcp = (System.Net.Sockets.TcpClient)f.GetValue(tcpClient);
                            if (tcp != null && tcp.Connected)
                            {
                                stream = tcp.GetStream();
                                break;
                            }
                        }
                    }

                    if (stream == null)
                    {
                        Console.WriteLine("❌ Cannot find NetworkStream inside PersistentTcpClient");
                        return;
                    }

                    await stream.WriteAsync(data, 0, data.Length);
                    await stream.FlushAsync();

                    Console.WriteLine("✅ Avatar sent to server");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Send avatar error: " + ex.Message);
            }
        }


        private void LoadUserAvatar()
        {
            try
            {
                if (string.IsNullOrEmpty(username)) return;

                string file = System.IO.Path.Combine("UserAvatars", $"{username}.txt");

                if (System.IO.File.Exists(file))
                {
                    string content = System.IO.File.ReadAllText(file);
                    if (int.TryParse(content, out int idx))
                    {
                        if (idx >= 0 && idx < gameAvatars.Length)
                        {
                            currentAvatarIndex = idx;
                            // Create a fitted avatar image sized to the picture box for perfect alignment
                            var fitted = CreateFittedAvatar(gameAvatars[currentAvatarIndex], pbAvatar.ClientSize);
                            if (fitted != null)
                            {
                                pbAvatar.Image = fitted;
                                pbAvatar.SizeMode = PictureBoxSizeMode.Normal;
                            }
                            else
                            {
                                pbAvatar.Image = gameAvatars[currentAvatarIndex];
                                pbAvatar.SizeMode = PictureBoxSizeMode.Zoom;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadUserAvatar error: " + ex.Message);
            }
        }

        // Tạo ảnh đã scale & center để chính xác khớp khung ảnh ở MainForm
        private Image CreateFittedAvatar(Image src, Size targetSize)
        {
            try
            {
                if (src == null || targetSize.Width <= 0 || targetSize.Height <= 0)
                    return null;

                var bmp = new Bitmap(targetSize.Width, targetSize.Height);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    float scale = Math.Min((float)targetSize.Width / src.Width, (float)targetSize.Height / src.Height);
                    int drawW = (int)(src.Width * scale);
                    int drawH = (int)(src.Height * scale);
                    int offsetX = (targetSize.Width - drawW) / 2;
                    int offsetY = (targetSize.Height - drawH) / 2;

                    g.DrawImage(src, new Rectangle(offsetX, offsetY, drawW, drawH));
                }
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        public string CurrentUsername
        {
            get { return username; }
            set { UpdateUsernameDisplay(value); }
        }
    }
}