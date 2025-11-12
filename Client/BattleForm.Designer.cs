namespace DoAn_NT106
{
    partial class BattleForm
    {
        private System.ComponentModel.IContainer components = null;

        // Health bars
        protected internal GameProgressBar healthBar1;
        protected internal GameProgressBar healthBar2;
        protected internal Label lblPlayer1Name;
        protected internal Label lblPlayer2Name;

        // Control buttons - Player 1
        protected internal Button btnPunch1;
        protected internal Button btnKick1;
        protected internal Button btnSpecial1;
        protected internal Button btnLeft1;
        protected internal Button btnRight1;

        // Control buttons - Player 2
        protected internal Button btnPunch2;
        protected internal Button btnKick2;
        protected internal Button btnSpecial2;
        protected internal Button btnLeft2;
        protected internal Button btnRight2;

        // UI controls
        protected internal ComboBox cmbBackground;
        protected internal Button btnBack;
        protected internal System.Windows.Forms.Timer gameTimer;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.gameTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();

            // 
            // gameTimer
            // 
            this.gameTimer.Interval = 50;
            this.gameTimer.Tick += new System.EventHandler(this.GameTimer_Tick);

            // 
            // BattleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 50);
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "BattleForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Street Fighter Battle";
            this.ResumeLayout(false);

            // Initialize custom UI
            InitializeCustomUI();
        }

        private void InitializeCustomUI()
        {
            // Player 1 Controls (Left side)
            InitializePlayer1Controls();

            // Player 2 Controls (Right side)
            InitializePlayer2Controls();

            // Background selector
            cmbBackground = new ComboBox
            {
                Location = new Point(400, 550),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.Black,
                ForeColor = Color.Gold,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            cmbBackground.Items.AddRange(new object[] { "🌲 Forest", "🏜️ Desert", "❄️ Snow", "🌋 Volcano" });
            cmbBackground.SelectedIndex = 0;

            // Back button
            btnBack = new Button
            {
                Text = "🏠 Back to Lobby",
                Location = new Point(570, 550),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(139, 69, 19),
                ForeColor = Color.Gold,
                Font = new Font("Arial", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            this.Controls.AddRange(new Control[] { cmbBackground, btnBack });
        }

        private void InitializePlayer1Controls()
        {
            int baseX = 50;
            int baseY = 500;

            btnPunch1 = CreateControlButton("👊 Punch", baseX, baseY, Color.FromArgb(200, 50, 50));
            btnKick1 = CreateControlButton("🦶 Kick", baseX + 80, baseY, Color.FromArgb(200, 100, 50));
            btnSpecial1 = CreateControlButton("🔥 Special", baseX + 160, baseY, Color.FromArgb(200, 150, 50));

            btnLeft1 = CreateControlButton("◀️", baseX, baseY + 35, Color.FromArgb(100, 100, 200));
            btnRight1 = CreateControlButton("▶️", baseX + 160, baseY + 35, Color.FromArgb(100, 100, 200));

            this.Controls.AddRange(new Control[] { btnPunch1, btnKick1, btnSpecial1, btnLeft1, btnRight1 });
        }

        private void InitializePlayer2Controls()
        {
            int baseX = 700;
            int baseY = 500;

            btnPunch2 = CreateControlButton("👊 Punch", baseX, baseY, Color.FromArgb(200, 50, 50));
            btnKick2 = CreateControlButton("🦶 Kick", baseX + 80, baseY, Color.FromArgb(200, 100, 50));
            btnSpecial2 = CreateControlButton("🔥 Special", baseX + 160, baseY, Color.FromArgb(200, 150, 50));

            btnLeft2 = CreateControlButton("◀️", baseX, baseY + 35, Color.FromArgb(100, 100, 200));
            btnRight2 = CreateControlButton("▶️", baseX + 160, baseY + 35, Color.FromArgb(100, 100, 200));

            this.Controls.AddRange(new Control[] { btnPunch2, btnKick2, btnSpecial2, btnLeft2, btnRight2 });
        }

        private Button CreateControlButton(string text, int x, int y, Color color)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(70, 30),
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Arial", 8, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        #endregion
    }
}