using System.Drawing;

namespace DoAn_NT106.Client
{
    partial class FormResetPass
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
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
            pnl_Main = new Pnl_Pixel();
            pnl_Title = new Pnl_Pixel();
            lbl_Title = new Label();
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            lbl_Subtitle = new Label();
            lbl_Description = new Label();
            panelNewPassword = new Panel();
            lblNewPasswordError = new Label();
            tbPassword = new Tb_Pixel();
            lblNewPassword = new Label();
            panelConfirmPassword = new Panel();
            lblConfirmPasswordError = new Label();
            tbconfirmPassword = new Tb_Pixel();
            lblConfirmPassword = new Label();
            btn_complete = new Btn_Pixel();
            btn_backToLogin = new Btn_Pixel();
            pnl_Main.SuspendLayout();
            pnl_Title.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panelNewPassword.SuspendLayout();
            panelConfirmPassword.SuspendLayout();
            SuspendLayout();
            // 
            // pnl_Main
            // 
            pnl_Main.BackColor = Color.FromArgb(210, 105, 30);
            pnl_Main.Controls.Add(pnl_Title);
            pnl_Main.Controls.Add(lbl_Description);
            pnl_Main.Controls.Add(panelNewPassword);
            pnl_Main.Controls.Add(panelConfirmPassword);
            pnl_Main.Controls.Add(btn_complete);
            pnl_Main.Controls.Add(btn_backToLogin);
            pnl_Main.Location = new Point(87, 29);
            pnl_Main.Name = "pnl_Main";
            pnl_Main.Size = new Size(413, 553);
            pnl_Main.TabIndex = 0;
            // 
            // pnl_Title
            // 
            pnl_Title.BackColor = Color.FromArgb(210, 105, 30);
            pnl_Title.Controls.Add(lbl_Title);
            pnl_Title.Controls.Add(pictureBox1);
            pnl_Title.Controls.Add(pictureBox2);
            pnl_Title.Controls.Add(lbl_Subtitle);
            pnl_Title.Location = new Point(20, 20);
            pnl_Title.Name = "pnl_Title";
            pnl_Title.Size = new Size(360, 100);
            pnl_Title.TabIndex = 4; // không nằm trong chuỗi Tab chính
            // 
            // lbl_Title
            // 
            lbl_Title.BackColor = Color.Transparent;
            lbl_Title.Font = new Font("Courier New", 14F, FontStyle.Bold);
            lbl_Title.ForeColor = Color.Gold;
            lbl_Title.Location = new Point(16, 19);
            lbl_Title.Name = "lbl_Title";
            lbl_Title.Size = new Size(341, 30);
            lbl_Title.TabIndex = 0;
            lbl_Title.Text = "🔓 RESET PASSWORD 🔓";
            lbl_Title.TextAlign = ContentAlignment.MiddleCenter;
            lbl_Title.TabStop = false;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = Properties.Resources.key;
            pictureBox1.Location = new Point(262, 49);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(79, 36);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Image = Properties.Resources.núi;
            pictureBox2.Location = new Point(0, 32);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(135, 68);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 2;
            pictureBox2.TabStop = false;
            // 
            // lbl_Subtitle
            // 
            lbl_Subtitle.BackColor = Color.Transparent;
            lbl_Subtitle.Font = new Font("Courier New", 7F, FontStyle.Bold);
            lbl_Subtitle.ForeColor = Color.White;
            lbl_Subtitle.Location = new Point(16, 49);
            lbl_Subtitle.Name = "lbl_Subtitle";
            lbl_Subtitle.Size = new Size(325, 20);
            lbl_Subtitle.TabIndex = 3;
            lbl_Subtitle.Text = "CREATE NEW PASSWORD";
            lbl_Subtitle.TextAlign = ContentAlignment.MiddleCenter;
            lbl_Subtitle.TabStop = false;
            // 
            // lbl_Description
            // 
            lbl_Description.BackColor = Color.Transparent;
            lbl_Description.Font = new Font("Courier New", 8F);
            lbl_Description.ForeColor = Color.White;
            lbl_Description.Location = new Point(20, 140);
            lbl_Description.Name = "lbl_Description";
            lbl_Description.Size = new Size(360, 60);
            lbl_Description.TabIndex = 5;
            lbl_Description.Text = "Please enter your new password.\r\nMake sure it's strong and secure!\r\n(Min 8 characters)";
            lbl_Description.TextAlign = ContentAlignment.MiddleCenter;
            lbl_Description.TabStop = false;
            // 
            // panelNewPassword
            // 
            panelNewPassword.Controls.Add(lblNewPasswordError);
            panelNewPassword.Controls.Add(tbPassword);
            panelNewPassword.Controls.Add(lblNewPassword);
            panelNewPassword.Location = new Point(20, 220);
            panelNewPassword.Name = "panelNewPassword";
            panelNewPassword.Size = new Size(360, 80);
            panelNewPassword.TabIndex = 0; // 1: tbPassword
            // 
            // lblNewPasswordError
            // 
            lblNewPasswordError.BackColor = Color.FromArgb(128, 64, 0);
            lblNewPasswordError.Dock = DockStyle.Bottom;
            lblNewPasswordError.Font = new Font("Arial", 10.2F, FontStyle.Bold);
            lblNewPasswordError.ForeColor = Color.Red;
            lblNewPasswordError.Location = new Point(0, 58);
            lblNewPasswordError.Name = "lblNewPasswordError";
            lblNewPasswordError.Size = new Size(360, 22);
            lblNewPasswordError.TabIndex = 2;
            lblNewPasswordError.TabStop = false;
            // 
            // tbPassword
            // 
            tbPassword.BackColor = Color.FromArgb(42, 31, 26);
            tbPassword.BorderStyle = BorderStyle.None;
            tbPassword.Font = new Font("Courier New", 16.2F, FontStyle.Bold);
            tbPassword.ForeColor = Color.White;
            tbPassword.Location = new Point(0, 25);
            tbPassword.Multiline = true;
            tbPassword.Name = "tbPassword";
            tbPassword.PasswordChar = '●';
            tbPassword.Size = new Size(360, 31);
            tbPassword.TabIndex = 0; // đầu tiên
            tbPassword.TextChanged += tbnewPassword_TextChanged;
            tbPassword.KeyDown += tbnewPassword_KeyDown;
            // 
            // lblNewPassword
            // 
            lblNewPassword.BackColor = Color.Transparent;
            lblNewPassword.Font = new Font("Courier New", 8F, FontStyle.Bold);
            lblNewPassword.ForeColor = Color.White;
            lblNewPassword.Location = new Point(0, 0);
            lblNewPassword.Name = "lblNewPassword";
            lblNewPassword.Size = new Size(200, 20);
            lblNewPassword.TabIndex = 1;
            lblNewPassword.Text = "🔒 NEW PASSWORD:";
            lblNewPassword.TabStop = false;
            // 
            // panelConfirmPassword
            // 
            panelConfirmPassword.Controls.Add(lblConfirmPasswordError);
            panelConfirmPassword.Controls.Add(tbconfirmPassword);
            panelConfirmPassword.Controls.Add(lblConfirmPassword);
            panelConfirmPassword.Location = new Point(20, 320);
            panelConfirmPassword.Name = "panelConfirmPassword";
            panelConfirmPassword.Size = new Size(360, 80);
            panelConfirmPassword.TabIndex = 1; 
            // 
            // lblConfirmPasswordError
            // 
            lblConfirmPasswordError.BackColor = Color.FromArgb(128, 64, 0);
            lblConfirmPasswordError.Dock = DockStyle.Bottom;
            lblConfirmPasswordError.Font = new Font("Arial", 10.2F, FontStyle.Bold);
            lblConfirmPasswordError.ForeColor = Color.Red;
            lblConfirmPasswordError.Location = new Point(0, 58);
            lblConfirmPasswordError.Name = "lblConfirmPasswordError";
            lblConfirmPasswordError.Size = new Size(360, 22);
            lblConfirmPasswordError.TabIndex = 2;
            lblConfirmPasswordError.TabStop = false;
            // 
            // tbconfirmPassword
            // 
            tbconfirmPassword.BackColor = Color.FromArgb(42, 31, 26);
            tbconfirmPassword.BorderStyle = BorderStyle.None;
            tbconfirmPassword.Font = new Font("Courier New", 16.2F, FontStyle.Bold);
            tbconfirmPassword.ForeColor = Color.White;
            tbconfirmPassword.Location = new Point(0, 25);
            tbconfirmPassword.Multiline = true;
            tbconfirmPassword.Name = "tbconfirmPassword";
            tbconfirmPassword.PasswordChar = '●';
            tbconfirmPassword.Size = new Size(360, 31);
            tbconfirmPassword.TabIndex = 0; 
            tbconfirmPassword.TextChanged += tbconfirmPassword_TextChanged;
            tbconfirmPassword.KeyDown += tbconfirmPassword_KeyDown;
            // 
            // lblConfirmPassword
            // 
            lblConfirmPassword.BackColor = Color.Transparent;
            lblConfirmPassword.Font = new Font("Courier New", 8F, FontStyle.Bold);
            lblConfirmPassword.ForeColor = Color.White;
            lblConfirmPassword.Location = new Point(0, 0);
            lblConfirmPassword.Name = "lblConfirmPassword";
            lblConfirmPassword.Size = new Size(250, 20);
            lblConfirmPassword.TabIndex = 1;
            lblConfirmPassword.Text = "🔒 CONFIRM PASSWORD:";
            lblConfirmPassword.TabStop = false;
            // 
            // btn_complete
            // 
            btn_complete.BtnColor = Color.FromArgb(34, 139, 34);
            btn_complete.FlatStyle = FlatStyle.Flat;
            btn_complete.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btn_complete.ForeColor = Color.White;
            btn_complete.Location = new Point(20, 430);
            btn_complete.Name = "btn_complete";
            btn_complete.Size = new Size(360, 50);
            btn_complete.TabIndex = 2; // thứ ba trong chuỗi Tab
            btn_complete.Text = "★ COMPLETE RESET ★";
            btn_complete.Click += btn_complete_Click;
            // 
            // btn_backToLogin
            // 
            btn_backToLogin.BtnColor = Color.FromArgb(139, 69, 19);
            btn_backToLogin.FlatStyle = FlatStyle.Flat;
            btn_backToLogin.Font = new Font("Courier New", 8F, FontStyle.Bold);
            btn_backToLogin.ForeColor = Color.White;
            btn_backToLogin.Location = new Point(20, 490);
            btn_backToLogin.Name = "btn_backToLogin";
            btn_backToLogin.Size = new Size(360, 40);
            btn_backToLogin.TabIndex = 3; // cuối cùng
            btn_backToLogin.Text = "← BACK TO LOGIN";
            btn_backToLogin.Click += btn_backToLogin_Click;
            // 
            // FormResetPass
            // 
            BackColor = SystemColors.ControlDark;
            BackgroundImage = Properties.Resources.background2;
            ClientSize = new Size(581, 621);
            Controls.Add(pnl_Main);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "FormResetPass";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Reset Password";
            pnl_Main.ResumeLayout(false);
            pnl_Title.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panelNewPassword.ResumeLayout(false);
            panelNewPassword.PerformLayout();
            panelConfirmPassword.ResumeLayout(false);
            panelConfirmPassword.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Pnl_Pixel pnl_Main;
        private Pnl_Pixel pnl_Title;
        private Label lbl_Title;
        private Label lbl_Subtitle;
        private Label lbl_Description;
        private Panel panelNewPassword;
        private Label lblNewPasswordError;
        private Tb_Pixel tbPassword;
        private Label lblNewPassword;
        private Panel panelConfirmPassword;
        private Label lblConfirmPasswordError;
        private Tb_Pixel tbconfirmPassword;
        private Label lblConfirmPassword;
        private Btn_Pixel btn_complete;
        private Btn_Pixel btn_backToLogin;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
    }
}
