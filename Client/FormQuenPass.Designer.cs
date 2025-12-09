using System.Drawing;

namespace DoAn_NT106
{
    partial class FormQuenPass
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
            panelContact = new Panel();
            lblContactError = new Label();
            tb_contact = new Tb_Pixel();
            lblContact = new Label();
            btn_continue = new Btn_Pixel();
            btn_backToLogin = new Btn_Pixel();
            pnl_Main.SuspendLayout();
            pnl_Title.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panelContact.SuspendLayout();
            SuspendLayout();
            // 
            // pnl_Main
            // 
            pnl_Main.BackColor = Color.FromArgb(210, 105, 30);
            pnl_Main.Controls.Add(pnl_Title);
            pnl_Main.Controls.Add(lbl_Description);
            pnl_Main.Controls.Add(panelContact);
            pnl_Main.Controls.Add(btn_continue);
            pnl_Main.Controls.Add(btn_backToLogin);
            pnl_Main.Location = new Point(87, 29);
            pnl_Main.Name = "pnl_Main";
            pnl_Main.Size = new Size(413, 493);
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
            lbl_Title.Text = "🔑 FORGOT PASSWORD? 🔑";
            lbl_Title.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = Properties.Resources.thor;
            pictureBox1.Location = new Point(-20, 38);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(125, 62);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Image = Properties.Resources.images__1__removebg_preview;
            pictureBox2.Location = new Point(248, 38);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(109, 62);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 5;
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
            lbl_Subtitle.TabIndex = 1;
            lbl_Subtitle.Text = "PASSWORD RECOVERY";
            lbl_Subtitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbl_Description
            // 
            lbl_Description.BackColor = Color.Transparent;
            lbl_Description.Font = new Font("Courier New", 8F);
            lbl_Description.ForeColor = Color.White;
            lbl_Description.Location = new Point(20, 140);
            lbl_Description.Name = "lbl_Description";
            lbl_Description.Size = new Size(360, 60);
            lbl_Description.TabIndex = 1;
            lbl_Description.Text = "Enter your email or phone number.\r\nWe will send you a verification\r\ncode to reset your password.";
            lbl_Description.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelContact
            // 
            panelContact.Controls.Add(lblContactError);
            panelContact.Controls.Add(tb_contact);
            panelContact.Controls.Add(lblContact);
            panelContact.Location = new Point(20, 220);
            panelContact.Name = "panelContact";
            panelContact.Size = new Size(360, 80);
            panelContact.TabIndex = 2;
            // 
            // lblContactError
            // 
            lblContactError.BackColor = Color.FromArgb(128, 64, 0);
            lblContactError.Dock = DockStyle.Bottom;
            lblContactError.Font = new Font("Arial", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblContactError.ForeColor = Color.Red;
            lblContactError.Location = new Point(0, 58);
            lblContactError.Name = "lblContactError";
            lblContactError.Size = new Size(360, 22);
            lblContactError.TabIndex = 0;
            // 
            // tb_contact
            // 
            tb_contact.BackColor = Color.FromArgb(42, 31, 26);
            tb_contact.BorderStyle = BorderStyle.None;
            tb_contact.Font = new Font("Courier New", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tb_contact.ForeColor = Color.White;
            tb_contact.Location = new Point(0, 25);
            tb_contact.Multiline = true;
            tb_contact.Name = "tb_contact";
            tb_contact.Size = new Size(360, 31);
            tb_contact.TabIndex = 1;
            tb_contact.TextChanged += tb_contact_TextChanged;
            tb_contact.KeyDown += tb_contact_KeyDown;
            // 
            // lblContact
            // 
            lblContact.BackColor = Color.Transparent;
            lblContact.Font = new Font("Courier New", 8F, FontStyle.Bold);
            lblContact.ForeColor = Color.White;
            lblContact.Location = new Point(0, 0);
            lblContact.Name = "lblContact";
            lblContact.Size = new Size(216, 22);
            lblContact.TabIndex = 2;
            lblContact.Text = "✉/📞 EMAIL OR PHONE:";
            // 
            // btn_continue
            // 
            btn_continue.BtnColor = Color.FromArgb(34, 139, 34);
            btn_continue.FlatStyle = FlatStyle.Flat;
            btn_continue.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btn_continue.ForeColor = Color.White;
            btn_continue.Location = new Point(20, 330);
            btn_continue.Name = "btn_continue";
            btn_continue.Size = new Size(360, 50);
            btn_continue.TabIndex = 3;
            btn_continue.Text = "▶ CONTINUE ▶";
            btn_continue.Click += btn_continue_Click;
            // 
            // btn_backToLogin
            // 
            btn_backToLogin.BtnColor = Color.FromArgb(139, 69, 19);
            btn_backToLogin.FlatStyle = FlatStyle.Flat;
            btn_backToLogin.Font = new Font("Courier New", 8F, FontStyle.Bold);
            btn_backToLogin.ForeColor = Color.White;
            btn_backToLogin.Location = new Point(20, 400);
            btn_backToLogin.Name = "btn_backToLogin";
            btn_backToLogin.Size = new Size(360, 40);
            btn_backToLogin.TabIndex = 4;
            btn_backToLogin.Text = "← BACK TO LOGIN";
            btn_backToLogin.Click += btn_backToLogin_Click;
            // 
            // FormQuenPass
            // 
            BackColor = SystemColors.ControlDark;
            BackgroundImage = Properties.Resources.background2;
            ClientSize = new Size(581, 561);
            Controls.Add(pnl_Main);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "FormQuenPass";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Forgot Password";
            pnl_Main.ResumeLayout(false);
            pnl_Title.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panelContact.ResumeLayout(false);
            panelContact.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Pnl_Pixel pnl_Main;
        private Pnl_Pixel pnl_Title;
        private Label lbl_Title;
        private Label lbl_Subtitle;
        private Label lbl_Description;
        private Panel panelContact;
        private Label lblContactError;
        private Tb_Pixel tb_contact;
        private Label lblContact;
        private Btn_Pixel btn_continue;
        private Btn_Pixel btn_backToLogin;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
    }
}