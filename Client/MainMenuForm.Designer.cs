namespace DoAn_NT106.Client
{
    partial class MainMenuForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            mainPanel = new Pnl_Pixel();
            pictureBox1 = new PictureBox();
            pictureBox5 = new PictureBox();
            lblTitle = new Label();
            btnContinue = new Btn_Pixel();
            btnBackToLobby = new Btn_Pixel();
            btnInstructions = new Btn_Pixel();
            mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).BeginInit();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.BackColor = Color.FromArgb(101, 67, 51);
            mainPanel.Controls.Add(pictureBox1);
            mainPanel.Controls.Add(pictureBox5);
            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(btnContinue);
            mainPanel.Controls.Add(btnBackToLobby);
            mainPanel.Controls.Add(btnInstructions);
            mainPanel.Location = new Point(10, 10);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(480, 258);
            mainPanel.TabIndex = 0;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = Properties.Resources.mây;
            pictureBox1.Location = new Point(390, 45);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(153, 80);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 9;
            pictureBox1.TabStop = false;
            // 
            // pictureBox5
            // 
            pictureBox5.BackColor = Color.Transparent;
            pictureBox5.Image = Properties.Resources.moon;
            pictureBox5.Location = new Point(-29, -1);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new Size(118, 73);
            pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox5.TabIndex = 11;
            pictureBox5.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Courier New", 24F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(255, 215, 0);
            lblTitle.Location = new Point(10, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(460, 70);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "⏸️ PAUSED ⏸️";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnContinue
            // 
            btnContinue.BtnColor = Color.FromArgb(34, 139, 34);
            btnContinue.FlatStyle = FlatStyle.Flat;
            btnContinue.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btnContinue.ForeColor = Color.Gold;
            btnContinue.Location = new Point(20, 140);
            btnContinue.Name = "btnContinue";
            btnContinue.Size = new Size(210, 45);
            btnContinue.TabIndex = 1;
            btnContinue.Text = "▶ CONTINUE GAME";
            btnContinue.UseVisualStyleBackColor = true;
            btnContinue.Click += BtnContinue_Click;
            // 
            // btnBackToLobby
            // 
            btnBackToLobby.BtnColor = Color.FromArgb(220, 20, 60);
            btnBackToLobby.FlatStyle = FlatStyle.Flat;
            btnBackToLobby.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btnBackToLobby.ForeColor = Color.Gold;
            btnBackToLobby.Location = new Point(250, 140);
            btnBackToLobby.Name = "btnBackToLobby";
            btnBackToLobby.Size = new Size(210, 45);
            btnBackToLobby.TabIndex = 2;
            btnBackToLobby.Text = "← BACK TO LOBBY";
            btnBackToLobby.UseVisualStyleBackColor = true;
            btnBackToLobby.Click += BtnBackToLobby_Click;
            // 
            // btnInstructions
            // 
            btnInstructions.BtnColor = Color.FromArgb(139, 69, 19);
            btnInstructions.FlatStyle = FlatStyle.Flat;
            btnInstructions.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btnInstructions.ForeColor = Color.Gold;
            btnInstructions.Location = new Point(20, 190);
            btnInstructions.Name = "btnInstructions";
            btnInstructions.Size = new Size(440, 45);
            btnInstructions.TabIndex = 3;
            btnInstructions.Text = "🛈 INSTRUCTIONS";
            btnInstructions.UseVisualStyleBackColor = true;
            btnInstructions.Click += BtnInstructions_Click;
            // 
            // MainMenuForm
            // 
            AutoScaleDimensions = new SizeF(12F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(160, 82, 45);
            ClientSize = new Size(500, 280);
            ControlBox = false;
            Controls.Add(mainPanel);
            Font = new Font("Courier New", 12F, FontStyle.Bold);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainMenuForm";
            StartPosition = FormStartPosition.CenterParent;
            mainPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).EndInit();
            ResumeLayout(false);
        }

        private Pnl_Pixel mainPanel;
        private System.Windows.Forms.Label lblTitle;
        private Btn_Pixel btnContinue;
        private Btn_Pixel btnBackToLobby;
        private Btn_Pixel btnInstructions;

        private void Panel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            var panel = sender as System.Windows.Forms.Panel;
            if (panel != null)
            {
                System.Windows.Forms.ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle,
                    System.Drawing.Color.FromArgb(74, 50, 25), 3, System.Windows.Forms.ButtonBorderStyle.Solid,
                    System.Drawing.Color.FromArgb(74, 50, 25), 3, System.Windows.Forms.ButtonBorderStyle.Solid,
                    System.Drawing.Color.FromArgb(74, 50, 25), 3, System.Windows.Forms.ButtonBorderStyle.Solid,
                    System.Drawing.Color.FromArgb(74, 50, 25), 3, System.Windows.Forms.ButtonBorderStyle.Solid);
            }
        }

        private void BtnContinue_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void BtnBackToLobby_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
        private PictureBox pictureBox1;
        private PictureBox pictureBox5;
    }
}
