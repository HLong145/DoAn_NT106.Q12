using DoAn_NT106.Services;
using PixelGameLobby;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;

namespace DoAn_NT106
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

        public class Particle
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Speed { get; set; }
            public int Size { get; set; }
            public Color Color { get; set; }
            public bool IsStar { get; set; }
        }

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomUI();
            this.FormClosing += MainForm_FormClosing;
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
            };
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

            frm_DangKy = new FormDangKy
            {
                TopLevel = false,
                Dock = DockStyle.Fill,
                FormBorderStyle = FormBorderStyle.None
            };
            frm_DangKy.SwitchToLogin += OnSwitchToDangNhap;

            pnl_Overlay.Controls.Add(frm_DangNhap);
            pnl_Overlay.Controls.Add(frm_DangKy);

            frm_DangNhap.Show();
            frm_DangKy.Hide();
            frm_DangNhap.BringToFront();
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
                this.Close();
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
            this.Hide();
        }

        public string CurrentUsername
        {
            get { return username; }
            set { UpdateUsernameDisplay(value); }
        }
    }
}