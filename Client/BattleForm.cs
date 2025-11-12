using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace DoAn_NT106
{
    public partial class BattleForm : Form
    {
        private string username;
        private string token;
        private string opponent;

        // Game assets
        private Image player1;
        private Image player2;
        private Image background;
        private Image fireball;
        private List<string> backgroundImages = new List<string>();

        // Player positions
        private int player1X = 100;
        private int player1Y = 300;
        private int player2X = 700;
        private int player2Y = 300;

        // Game state
        private bool player1Moving, player2Moving;
        private bool player1Attacking, player2Attacking;
        private bool fireballActive;
        private int fireballX, fireballY;
        private int currentBackground = 0;

        // Health and stats
        private int player1Health = 100;
        private int player2Health = 100;

        public BattleForm(string username, string token, string opponent = "Opponent")
        {
            InitializeComponent();
            this.username = username;
            this.token = token;
            this.opponent = opponent;

            SetupGame();
            SetupEventHandlers();

            this.Text = $"⚔️ Street Fighter - {username} vs {opponent}";
            this.DoubleBuffered = true;
        }

        private void SetupGame()
        {
            try
            {
                // Load backgrounds
                backgroundImages.Add("background/battleground1.jpg");
                backgroundImages.Add("background/battleground2.jpg");
                backgroundImages.Add("background/battleround3.jpg");
                backgroundImages.Add("background/battleground4.jpg");

                background = (Image)Properties.Resources.ResourceManager.GetObject(backgroundImages[currentBackground]);

                // Load character sprites (fallback nếu không có file)
                player1 = CreateColoredImage(100, 150, Color.Red);
                player2 = CreateColoredImage(100, 150, Color.Blue);
                fireball = CreateColoredImage(50, 30, Color.Orange);

                // Thử load ảnh GIF nếu có
                try { player1 = Image.FromFile("characters/ryu_stand.gif"); } catch { }
                try { player2 = Image.FromFile("characters/ken_stand.gif"); } catch { }
                try { fireball = Image.FromFile("characters/fireball.gif"); } catch { }

                // Setup animations nếu là GIF
                if (player1 != null) ImageAnimator.Animate(player1, OnFrameChanged);
                if (player2 != null) ImageAnimator.Animate(player2, OnFrameChanged);
                if (fireball != null) ImageAnimator.Animate(fireball, OnFrameChanged);
            }
            catch (Exception ex)
            {
                CreateFallbackGraphics();
                Console.WriteLine($"Setup error: {ex.Message}");
            }

            // Setup health bars
            SetupHealthBars();
        }
        private void SetBackground(string backgroundName)
        {
            if (backgroundImages.Contains(backgroundName))
            {
                // Tìm index của backgroundName trong list
                currentBackground = backgroundImages.IndexOf(backgroundName);
                background = (Image)Properties.Resources.ResourceManager.GetObject(backgroundName);
                this.Invalidate(); // Vẽ lại form
            }
        }
        private void CreateFallbackGraphics()
        {
            // Create simple colored rectangles as fallback
            player1 = CreateColoredImage(100, 150, Color.Red);
            player2 = CreateColoredImage(100, 150, Color.Blue);
            background = CreateColoredImage(1000, 600, Color.DarkGreen);
            fireball = CreateColoredImage(50, 30, Color.Orange);
        }

        private Bitmap CreateColoredImage(int width, int height, Color color)
        {
            var bmp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bmp))
            using (var brush = new SolidBrush(color))
                g.FillRectangle(brush, 0, 0, width, height);
            return bmp;
        }

        private void SetupHealthBars()
        {
            // Player 1 Health Bar (Top Left)
            healthBar1 = new GameProgressBar
            {
                Location = new Point(20, 20),
                Size = new Size(300, 25),
                Maximum = 100,
                Value = 100,
                CustomForeColor = Color.FromArgb(0, 200, 50),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            // Player 2 Health Bar (Top Right)
            healthBar2 = new GameProgressBar
            {
                Location = new Point(680, 20),
                Size = new Size(300, 25),
                Maximum = 100,
                Value = 100,
                CustomForeColor = Color.FromArgb(200, 50, 0),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            // Player name labels
            lblPlayer1Name = new Label
            {
                Text = username,
                Location = new Point(20, 50),
                Size = new Size(150, 20),
                ForeColor = Color.Cyan,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            lblPlayer2Name = new Label
            {
                Text = opponent,
                Location = new Point(830, 50),
                Size = new Size(150, 20),
                ForeColor = Color.Orange,
                Font = new Font("Arial", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.TopRight,
                BackColor = Color.Transparent
            };

            this.Controls.AddRange(new Control[] { healthBar1, healthBar2, lblPlayer1Name, lblPlayer2Name });
        }

        private void SetupEventHandlers()
        {
            // Attack buttons
            btnPunch1.Click += (s, e) => Player1Attack("punch");
            btnKick1.Click += (s, e) => Player1Attack("kick");
            btnSpecial1.Click += (s, e) => Player1Attack("special");

            btnPunch2.Click += (s, e) => Player2Attack("punch");
            btnKick2.Click += (s, e) => Player2Attack("kick");
            btnSpecial2.Click += (s, e) => Player2Attack("special");

            // Movement buttons
            btnLeft1.Click += (s, e) => MovePlayer1(-20);
            btnRight1.Click += (s, e) => MovePlayer1(20);
            btnLeft2.Click += (s, e) => MovePlayer2(-20);
            btnRight2.Click += (s, e) => MovePlayer2(20);

            // Background selector
            cmbBackground.SelectedIndexChanged += CmbBackground_SelectedIndexChanged;

            // Back button
            btnBack.Click += BtnBack_Click;

            // Game timer
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        // ===============================
        // GAME TIMER EVENT HANDLER
        // ===============================
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            UpdateGame();
            this.Invalidate(); // Trigger paint event
        }

        private void UpdateGame()
        {
            // Update animations
            if (player1 != null) ImageAnimator.UpdateFrames(player1);
            if (player2 != null) ImageAnimator.UpdateFrames(player2);
            if (fireball != null && fireballActive) ImageAnimator.UpdateFrames(fireball);

            // Update fireball position
            if (fireballActive)
            {
                fireballX += 15;
                CheckFireballHit();

                if (fireballX > this.ClientSize.Width)
                {
                    fireballActive = false;
                }
            }

            // Update health bars
            healthBar1.Value = player1Health;
            healthBar2.Value = player2Health;

            // Check game over
            if (player1Health <= 0 || player2Health <= 0)
            {
                gameTimer.Stop();
                string winner = player1Health <= 0 ? opponent : username;
                ShowGameOver(winner);
            }
        }

        private void OnFrameChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        // ===============================
        // PLAYER ACTIONS
        // ===============================
        private void Player1Attack(string attackType)
        {
            player1Attacking = true;

            try
            {
                switch (attackType)
                {
                    case "punch":
                        player1 = CreateColoredImage(100, 150, Color.DarkRed);
                        if (CheckCollision(player1X, player1Y, player2X, player2Y))
                            player2Health -= 10;
                        break;
                    case "kick":
                        player1 = CreateColoredImage(100, 150, Color.DarkOrange);
                        if (CheckCollision(player1X, player1Y, player2X, player2Y))
                            player2Health -= 15;
                        break;
                    case "special":
                        player1 = CreateColoredImage(100, 150, Color.Yellow);
                        ShootFireball(player1X + 80, player1Y + 50);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Attack error: {ex.Message}");
                if (attackType != "special" && CheckCollision(player1X, player1Y, player2X, player2Y))
                    player2Health -= attackType == "punch" ? 10 : 15;
            }

            ResetAttackState();
        }

        private void Player2Attack(string attackType)
        {
            player2Attacking = true;

            try
            {
                switch (attackType)
                {
                    case "punch":
                        player2 = CreateColoredImage(100, 150, Color.DarkBlue);
                        if (CheckCollision(player2X, player2Y, player1X, player1Y))
                            player1Health -= 10;
                        break;
                    case "kick":
                        player2 = CreateColoredImage(100, 150, Color.Purple);
                        if (CheckCollision(player2X, player2Y, player1X, player1Y))
                            player1Health -= 15;
                        break;
                    case "special":
                        player2 = CreateColoredImage(100, 150, Color.Cyan);
                        ShootFireball(player2X - 30, player2Y + 50);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Attack error: {ex.Message}");
                if (attackType != "special" && CheckCollision(player2X, player2Y, player1X, player1Y))
                    player1Health -= attackType == "punch" ? 10 : 15;
            }

            ResetAttackState();
        }

        private void MovePlayer1(int distance)
        {
            int newX = player1X + distance;
            if (newX > 50 && newX < player2X - 100)
            {
                player1X = newX;
                player1Moving = true;
            }
        }

        private void MovePlayer2(int distance)
        {
            int newX = player2X + distance;
            if (newX > player1X + 100 && newX < this.ClientSize.Width - 150)
            {
                player2X = newX;
                player2Moving = true;
            }
        }

        private void ShootFireball(int x, int y)
        {
            fireballActive = true;
            fireballX = x;
            fireballY = y;
        }

        private void CheckFireballHit()
        {
            if (fireballActive && CheckCollision(fireballX, fireballY, player2X, player2Y))
            {
                player2Health -= 20;
                fireballActive = false;
                ShowHitEffect("Fireball Hit!");
            }
            else if (fireballActive && CheckCollision(fireballX, fireballY, player1X, player1Y))
            {
                player1Health -= 20;
                fireballActive = false;
                ShowHitEffect("Fireball Hit!");
            }
        }

        private bool CheckCollision(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) < 100 && Math.Abs(y1 - y2) < 100;
        }

        private void ShowHitEffect(string message)
        {
            var hitLabel = new Label
            {
                Text = message,
                ForeColor = Color.Red,
                BackColor = Color.FromArgb(150, 255, 255, 255),
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(this.ClientSize.Width / 2 - 50, 100)
            };

            this.Controls.Add(hitLabel);
            hitLabel.BringToFront();

            // SỬA LỖI TIMER Ở ĐÂY - Sử dụng System.Windows.Forms.Timer
            System.Windows.Forms.Timer removeTimer = new System.Windows.Forms.Timer();
            removeTimer.Interval = 1000;
            removeTimer.Tick += (s, e) =>
            {
                this.Controls.Remove(hitLabel);
                removeTimer.Stop();
            };
            removeTimer.Start();
        }

        private void ResetAttackState()
        {
            // SỬA LỖI TIMER Ở ĐÂY - Sử dụng System.Windows.Forms.Timer
            System.Windows.Forms.Timer resetTimer = new System.Windows.Forms.Timer();
            resetTimer.Interval = 500;
            resetTimer.Tick += (s, e) =>
            {
                player1 = CreateColoredImage(100, 150, Color.Red);
                player2 = CreateColoredImage(100, 150, Color.Blue);

                player1Attacking = false;
                player2Attacking = false;
                player1Moving = false;
                player2Moving = false;
                resetTimer.Stop();
            };
            resetTimer.Start();
        }

        // ===============================
        // PAINT EVENT
        // ===============================
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (background != null)
                e.Graphics.DrawImage(background, 0, 0, this.ClientSize.Width, this.ClientSize.Height);

            if (player1 != null)
                e.Graphics.DrawImage(player1, player1X, player1Y, 100, 150);
            if (player2 != null)
                e.Graphics.DrawImage(player2, player2X, player2Y, 100, 150);

            if (fireballActive && fireball != null)
            {
                e.Graphics.DrawImage(fireball, fireballX, fireballY, 50, 30);
            }

            DrawGameUI(e.Graphics);
        }

        private void DrawGameUI(Graphics g)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(100, 0, 0, 0)),
                10, 10, this.ClientSize.Width - 20, 70);

            g.DrawString($"{username}: {player1Health} HP",
                new Font("Arial", 10, FontStyle.Bold),
                Brushes.Cyan, 20, 50);

            g.DrawString($"{opponent}: {player2Health} HP",
                new Font("Arial", 10, FontStyle.Bold),
                Brushes.Orange, this.ClientSize.Width - 200, 50);
        }

        // ===============================
        // UI EVENT HANDLERS
        // ===============================
        private void CmbBackground_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbBackground.SelectedIndex >= 0 && cmbBackground.SelectedIndex < backgroundImages.Count)
            {
                string bgName = backgroundImages[cmbBackground.SelectedIndex];
                SetBackground(bgName);
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            gameTimer.Stop();

            // Quay lại MainForm
            foreach (Form form in Application.OpenForms)
            {
                if (form is MainForm mainForm)
                {
                    mainForm.Show();
                    break;
                }
            }
            this.Close();
        }

        private void ShowGameOver(string winner)
        {
            MessageBox.Show($"🎉 {winner} wins the battle!\n\nFinal Scores:\n{username}: {player1Health} HP\n{opponent}: {player2Health} HP",
                "Battle Finished", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            gameTimer.Stop();
            base.OnFormClosing(e);
        }
    }
}