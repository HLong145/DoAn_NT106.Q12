using System.Drawing;

namespace DoAn_NT106
{
    partial class FormXacThucOTP
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
            lbl_Subtitle = new Label();
            lbl_Description = new Label();
            lbl_SentTo = new Label();
            panelOTP = new Panel();
            tb_otp6 = new Tb_Pixel();
            tb_otp5 = new Tb_Pixel();
            tb_otp4 = new Tb_Pixel();
            tb_otp3 = new Tb_Pixel();
            tb_otp2 = new Tb_Pixel();
            tb_otp1 = new Tb_Pixel();
            lblOTP = new Label();
            lblOTPError = new Label();
            lbl_timer = new Label();
            btn_verify = new Btn_Pixel();
            btn_resend = new Btn_Pixel();
            btn_backToLogin = new Btn_Pixel();
            pnl_Main.SuspendLayout();
            pnl_Title.SuspendLayout();
            panelOTP.SuspendLayout();
            SuspendLayout();
            // 
            // pnl_Main
            // 
            pnl_Main.BackColor = Color.FromArgb(210, 105, 30);
            pnl_Main.Controls.Add(pnl_Title);
            pnl_Main.Controls.Add(lbl_Description);
            pnl_Main.Controls.Add(lbl_SentTo);
            pnl_Main.Controls.Add(panelOTP);
            pnl_Main.Controls.Add(lbl_timer);
            pnl_Main.Controls.Add(btn_verify);
            pnl_Main.Controls.Add(btn_resend);
            pnl_Main.Controls.Add(btn_backToLogin);
            pnl_Main.Location = new Point(87, 29);
            pnl_Main.Name = "pnl_Main";
            pnl_Main.Size = new Size(413, 573);
            pnl_Main.TabIndex = 0;
            // 
            // pnl_Title
            // 
            pnl_Title.BackColor = Color.FromArgb(210, 105, 30);
            pnl_Title.Controls.Add(lbl_Title);
            pnl_Title.Controls.Add(lbl_Subtitle);
            pnl_Title.Location = new Point(20, 20);
            pnl_Title.Name = "pnl_Title";
            pnl_Title.Size = new Size(360, 100);
            pnl_Title.TabIndex = 0;
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
            lbl_Title.Text = "🔐 VERIFY CODE 🔐";
            lbl_Title.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbl_Subtitle
            // 
            lbl_Subtitle.BackColor = Color.Transparent;
            lbl_Subtitle.Font = new Font("Courier New", 7F, FontStyle.Bold);
            lbl_Subtitle.ForeColor = Color.White;
            lbl_Subtitle.Location = new Point(16, 49);
            lbl_Subtitle.Name = "lbl_Subtitle";
            lbl_Subtitle.Size = new Size(325, 20);
            lbl_Subtitle.TabIndex = 1;
            lbl_Subtitle.Text = "ENTER VERIFICATION CODE";
            lbl_Subtitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbl_Description
            // 
            lbl_Description.BackColor = Color.Transparent;
            lbl_Description.Font = new Font("Courier New", 8F);
            lbl_Description.ForeColor = Color.White;
            lbl_Description.Location = new Point(20, 140);
            lbl_Description.Name = "lbl_Description";
            lbl_Description.Size = new Size(360, 40);
            lbl_Description.TabIndex = 1;
            lbl_Description.Text = "We have sent a 6-digit code to\r\nyour email/phone number.";
            lbl_Description.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbl_SentTo
            // 
            lbl_SentTo.BackColor = Color.Transparent;
            lbl_SentTo.Font = new Font("Courier New", 8F, FontStyle.Bold);
            lbl_SentTo.ForeColor = Color.Gold;
            lbl_SentTo.Location = new Point(20, 185);
            lbl_SentTo.Name = "lbl_SentTo";
            lbl_SentTo.Size = new Size(360, 20);
            lbl_SentTo.TabIndex = 2;
            lbl_SentTo.Text = "Sent to: ***@***.*** or 0*********";
            lbl_SentTo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelOTP
            // 
            panelOTP.Controls.Add(tb_otp6);
            panelOTP.Controls.Add(tb_otp5);
            panelOTP.Controls.Add(tb_otp4);
            panelOTP.Controls.Add(tb_otp3);
            panelOTP.Controls.Add(tb_otp2);
            panelOTP.Controls.Add(tb_otp1);
            panelOTP.Controls.Add(lblOTP);
            panelOTP.Controls.Add(lblOTPError);
            panelOTP.Location = new Point(20, 220);
            panelOTP.Name = "panelOTP";
            panelOTP.Size = new Size(360, 110);
            panelOTP.TabIndex = 3;
            // 
            // tb_otp6
            // 
            tb_otp6.BackColor = Color.FromArgb(42, 31, 26);
            tb_otp6.BorderStyle = BorderStyle.None;
            tb_otp6.Font = new Font("Courier New", 19.8000011F, FontStyle.Bold);
            tb_otp6.ForeColor = Color.White;
            tb_otp6.Location = new Point(296, 35);
            tb_otp6.MaxLength = 1;
            tb_otp6.Multiline = true;
            tb_otp6.Name = "tb_otp6";
            tb_otp6.Size = new Size(50, 38);
            tb_otp6.TabIndex = 6;
            tb_otp6.TextAlign = HorizontalAlignment.Center;
            // 
            // tb_otp5
            // 
            tb_otp5.BackColor = Color.FromArgb(42, 31, 26);
            tb_otp5.BorderStyle = BorderStyle.None;
            tb_otp5.Font = new Font("Courier New", 19.8000011F, FontStyle.Bold);
            tb_otp5.ForeColor = Color.White;
            tb_otp5.Location = new Point(240, 35);
            tb_otp5.MaxLength = 1;
            tb_otp5.Multiline = true;
            tb_otp5.Name = "tb_otp5";
            tb_otp5.Size = new Size(50, 38);
            tb_otp5.TabIndex = 5;
            tb_otp5.TextAlign = HorizontalAlignment.Center;
            // 
            // tb_otp4
            // 
            tb_otp4.BackColor = Color.FromArgb(42, 31, 26);
            tb_otp4.BorderStyle = BorderStyle.None;
            tb_otp4.Font = new Font("Courier New", 19.8000011F, FontStyle.Bold);
            tb_otp4.ForeColor = Color.White;
            tb_otp4.Location = new Point(184, 35);
            tb_otp4.MaxLength = 1;
            tb_otp4.Multiline = true;
            tb_otp4.Name = "tb_otp4";
            tb_otp4.Size = new Size(50, 38);
            tb_otp4.TabIndex = 4;
            tb_otp4.TextAlign = HorizontalAlignment.Center;
            // 
            // tb_otp3
            // 
            tb_otp3.BackColor = Color.FromArgb(42, 31, 26);
            tb_otp3.BorderStyle = BorderStyle.None;
            tb_otp3.Font = new Font("Courier New", 19.8000011F, FontStyle.Bold);
            tb_otp3.ForeColor = Color.White;
            tb_otp3.Location = new Point(128, 35);
            tb_otp3.MaxLength = 1;
            tb_otp3.Multiline = true;
            tb_otp3.Name = "tb_otp3";
            tb_otp3.Size = new Size(50, 38);
            tb_otp3.TabIndex = 3;
            tb_otp3.TextAlign = HorizontalAlignment.Center;
            // 
            // tb_otp2
            // 
            tb_otp2.BackColor = Color.FromArgb(42, 31, 26);
            tb_otp2.BorderStyle = BorderStyle.None;
            tb_otp2.Font = new Font("Courier New", 19.8000011F, FontStyle.Bold);
            tb_otp2.ForeColor = Color.White;
            tb_otp2.Location = new Point(72, 35);
            tb_otp2.MaxLength = 1;
            tb_otp2.Multiline = true;
            tb_otp2.Name = "tb_otp2";
            tb_otp2.Size = new Size(50, 38);
            tb_otp2.TabIndex = 2;
            tb_otp2.TextAlign = HorizontalAlignment.Center;
            // 
            // tb_otp1
            // 
            tb_otp1.BackColor = Color.FromArgb(42, 31, 26);
            tb_otp1.BorderStyle = BorderStyle.None;
            tb_otp1.Font = new Font("Courier New", 19.8000011F, FontStyle.Bold);
            tb_otp1.ForeColor = Color.White;
            tb_otp1.Location = new Point(16, 35);
            tb_otp1.MaxLength = 1;
            tb_otp1.Multiline = true;
            tb_otp1.Name = "tb_otp1";
            tb_otp1.Size = new Size(50, 38);
            tb_otp1.TabIndex = 1;
            tb_otp1.TextAlign = HorizontalAlignment.Center;
            // 
            // lblOTP
            // 
            lblOTP.BackColor = Color.Transparent;
            lblOTP.Font = new Font("Courier New", 8F, FontStyle.Bold);
            lblOTP.ForeColor = Color.White;
            lblOTP.Location = new Point(0, 0);
            lblOTP.Name = "lblOTP";
            lblOTP.Size = new Size(250, 20);
            lblOTP.TabIndex = 0;
            lblOTP.Text = "🔢 ENTER 6-DIGIT CODE:";
            // 
            // lblOTPError
            // 
            lblOTPError.BackColor = Color.FromArgb(128, 64, 0);
            lblOTPError.Dock = DockStyle.Bottom;
            lblOTPError.Font = new Font("Arial", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblOTPError.ForeColor = Color.Red;
            lblOTPError.Location = new Point(0, 88);
            lblOTPError.Name = "lblOTPError";
            lblOTPError.Size = new Size(360, 22);
            lblOTPError.TabIndex = 7;
            // 
            // lbl_timer
            // 
            lbl_timer.BackColor = Color.Transparent;
            lbl_timer.Font = new Font("Courier New", 8F, FontStyle.Bold);
            lbl_timer.ForeColor = Color.White;
            lbl_timer.Location = new Point(20, 345);
            lbl_timer.Name = "lbl_timer";
            lbl_timer.Size = new Size(360, 20);
            lbl_timer.TabIndex = 7;
            lbl_timer.Text = "Code expires in: 05:00";
            lbl_timer.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btn_verify
            // 
            btn_verify.BtnColor = Color.FromArgb(34, 139, 34);
            btn_verify.FlatStyle = FlatStyle.Flat;
            btn_verify.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btn_verify.ForeColor = Color.White;
            btn_verify.Location = new Point(20, 380);
            btn_verify.Name = "btn_verify";
            btn_verify.Size = new Size(360, 50);
            btn_verify.TabIndex = 4;
            btn_verify.Text = "✓ VERIFY & CONTINUE ✓";
            btn_verify.Click += btn_verify_Click;
            // 
            // btn_resend
            // 
            btn_resend.BtnColor = Color.FromArgb(205, 133, 63);
            btn_resend.FlatStyle = FlatStyle.Flat;
            btn_resend.Font = new Font("Courier New", 8F, FontStyle.Bold);
            btn_resend.ForeColor = Color.White;
            btn_resend.Location = new Point(20, 450);
            btn_resend.Name = "btn_resend";
            btn_resend.Size = new Size(360, 40);
            btn_resend.TabIndex = 5;
            btn_resend.Text = "↻ RESEND CODE";
            btn_resend.Click += btn_resend_Click;
            // 
            // btn_backToLogin
            // 
            btn_backToLogin.BtnColor = Color.FromArgb(139, 69, 19);
            btn_backToLogin.FlatStyle = FlatStyle.Flat;
            btn_backToLogin.Font = new Font("Courier New", 8F, FontStyle.Bold);
            btn_backToLogin.ForeColor = Color.White;
            btn_backToLogin.Location = new Point(20, 500);
            btn_backToLogin.Name = "btn_backToLogin";
            btn_backToLogin.Size = new Size(360, 40);
            btn_backToLogin.TabIndex = 6;
            btn_backToLogin.Text = "← BACK TO LOGIN";
            btn_backToLogin.Click += btn_backToLogin_Click;
            // 
            // FormXacThucOTP
            // 
            BackColor = SystemColors.ControlDark;
            BackgroundImage = Properties.Resources.background2;
            ClientSize = new Size(581, 641);
            Controls.Add(pnl_Main);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "FormXacThucOTP";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Verify Code";
            pnl_Main.ResumeLayout(false);
            pnl_Title.ResumeLayout(false);
            panelOTP.ResumeLayout(false);
            panelOTP.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Pnl_Pixel pnl_Main;
        private Pnl_Pixel pnl_Title;
        private Label lbl_Title;
        private Label lbl_Subtitle;
        private Label lbl_Description;
        private Label lbl_SentTo;
        private Panel panelOTP;
        private Tb_Pixel tb_otp1;
        private Tb_Pixel tb_otp2;
        private Tb_Pixel tb_otp3;
        private Tb_Pixel tb_otp4;
        private Tb_Pixel tb_otp5;
        private Tb_Pixel tb_otp6;
        private Label lblOTP;
        private Label lblOTPError;
        private Btn_Pixel btn_verify;
        private Btn_Pixel btn_resend;
        private Btn_Pixel btn_backToLogin;
        private Label lbl_timer;
    }
}