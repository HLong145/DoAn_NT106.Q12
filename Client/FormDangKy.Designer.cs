using System.Drawing;

namespace DoAn_NT106
{
    partial class FormDangKy
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelOuter = new Panel();
            panelMain = new Panel();
            pnl_Main = new Pnl_Pixel();
            pnl_Title = new Pnl_Pixel();
            lbl_Title = new Label();
            pictureBox6 = new PictureBox();
            pictureBox5 = new PictureBox();
            pictureBox2 = new PictureBox();
            lbl_Subtitle = new Label();
            btn_alreadyHaveAccount = new Btn_Pixel();
            pictureBox1 = new PictureBox();
            pictureBox4 = new PictureBox();
            pictureBox3 = new PictureBox();
            btn_register = new Btn_Pixel();
            panelContact = new Panel();
            lblContactError = new Label();
            tb_contact = new Tb_Pixel();
            lblContact = new Label();
            panelRobotCheck = new Panel();
            lblRobotError = new Label();
            chkNotRobot = new CheckBox();
            panelPassword = new Panel();
            lblPasswordError = new Label();
            pictureBoxLock1 = new PictureBox();
            tb_password = new Tb_Pixel();
            lblPassword = new Label();
            panelConfirmPassword = new Panel();
            lblConfirmPasswordError = new Label();
            pictureBoxLock2 = new PictureBox();
            tb_confirmPassword = new Tb_Pixel();
            lblConfirmPassword = new Label();
            panelUsername = new Panel();
            tb_username = new Tb_Pixel();
            lblUsername = new Label();
            lblUsernameError = new Label();
            panelOuter.SuspendLayout();
            panelMain.SuspendLayout();
            pnl_Main.SuspendLayout();
            pnl_Title.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            panelContact.SuspendLayout();
            panelRobotCheck.SuspendLayout();
            panelPassword.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLock1).BeginInit();
            panelConfirmPassword.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLock2).BeginInit();
            panelUsername.SuspendLayout();
            SuspendLayout();
            // 
            // panelOuter
            // 
            panelOuter.BackColor = Color.FromArgb(180, 83, 9);
            panelOuter.Controls.Add(panelMain);
            panelOuter.Dock = DockStyle.Fill;
            panelOuter.Location = new Point(0, 0);
            panelOuter.Name = "panelOuter";
            panelOuter.Padding = new Padding(12);
            panelOuter.Size = new Size(701, 749);
            panelOuter.TabIndex = 0;
            // 
            // panelMain
            // 
            panelMain.BackColor = Color.FromArgb(129, 64, 0);
            panelMain.BackgroundImage = Properties.Resources.background2;
            panelMain.Controls.Add(pnl_Main);
            panelMain.Dock = DockStyle.Fill;
            panelMain.Font = new Font("Microsoft Sans Serif", 10.8F, FontStyle.Bold);
            panelMain.ForeColor = Color.Red;
            panelMain.Location = new Point(12, 12);
            panelMain.Name = "panelMain";
            panelMain.Padding = new Padding(32);
            panelMain.Size = new Size(677, 725);
            panelMain.TabIndex = 0;
            // 
            // pnl_Main
            // 
            pnl_Main.BackColor = Color.FromArgb(210, 105, 30);
            pnl_Main.BackgroundImage = Properties.Resources.background2;
            pnl_Main.Controls.Add(pnl_Title);
            pnl_Main.Controls.Add(btn_alreadyHaveAccount);
            pnl_Main.Controls.Add(pictureBox1);
            pnl_Main.Controls.Add(pictureBox4);
            pnl_Main.Controls.Add(pictureBox3);
            pnl_Main.Controls.Add(btn_register);
            pnl_Main.Controls.Add(panelContact);
            pnl_Main.Controls.Add(panelRobotCheck);
            pnl_Main.Controls.Add(panelPassword);
            pnl_Main.Controls.Add(panelConfirmPassword);
            pnl_Main.Controls.Add(panelUsername);
            pnl_Main.Location = new Point(21, 15);
            pnl_Main.Name = "pnl_Main";
            pnl_Main.Size = new Size(633, 755);
            pnl_Main.TabIndex = 0;
            // 
            // pnl_Title
            // 
            pnl_Title.BackColor = Color.FromArgb(210, 105, 30);
            pnl_Title.Controls.Add(lbl_Title);
            pnl_Title.Controls.Add(pictureBox6);
            pnl_Title.Controls.Add(pictureBox5);
            pnl_Title.Controls.Add(pictureBox2);
            pnl_Title.Controls.Add(lbl_Subtitle);
            pnl_Title.Location = new Point(61, 22);
            pnl_Title.Name = "pnl_Title";
            pnl_Title.Size = new Size(510, 100);
            pnl_Title.TabIndex = 7;
            // 
            // lbl_Title
            // 
            lbl_Title.BackColor = Color.Transparent;
            lbl_Title.Font = new Font("Courier New", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbl_Title.ForeColor = Color.Gold;
            lbl_Title.Location = new Point(99, 19);
            lbl_Title.Name = "lbl_Title";
            lbl_Title.Size = new Size(341, 30);
            lbl_Title.TabIndex = 0;
            lbl_Title.Text = "⚔️ NEW PLAYER ⚔️";
            lbl_Title.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pictureBox6
            // 
            pictureBox6.BackColor = Color.Transparent;
            pictureBox6.Image = Properties.Resources.mayxanh;
            pictureBox6.Location = new Point(351, 28);
            pictureBox6.Name = "pictureBox6";
            pictureBox6.Size = new Size(194, 97);
            pictureBox6.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox6.TabIndex = 1;
            pictureBox6.TabStop = false;
            // 
            // pictureBox5
            // 
            pictureBox5.BackColor = Color.Transparent;
            pictureBox5.Image = Properties.Resources.moon;
            pictureBox5.Location = new Point(-88, -19);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new Size(194, 78);
            pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox5.TabIndex = 2;
            pictureBox5.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Image = Properties.Resources.mayxanh;
            pictureBox2.Location = new Point(-79, 52);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(194, 73);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 3;
            pictureBox2.TabStop = false;
            // 
            // lbl_Subtitle
            // 
            lbl_Subtitle.BackColor = Color.Transparent;
            lbl_Subtitle.Font = new Font("Courier New", 7F, FontStyle.Bold);
            lbl_Subtitle.ForeColor = Color.White;
            lbl_Subtitle.Location = new Point(99, 62);
            lbl_Subtitle.Name = "lbl_Subtitle";
            lbl_Subtitle.Size = new Size(325, 20);
            lbl_Subtitle.TabIndex = 4;
            lbl_Subtitle.Text = "CREATE YOUR ACCOUNT";
            lbl_Subtitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btn_alreadyHaveAccount
            // 
            btn_alreadyHaveAccount.BackColor = Color.FromArgb(217, 119, 6);
            btn_alreadyHaveAccount.BtnColor = Color.FromArgb(217, 119, 6);
            btn_alreadyHaveAccount.FlatAppearance.BorderSize = 0;
            btn_alreadyHaveAccount.FlatStyle = FlatStyle.Flat;
            btn_alreadyHaveAccount.Font = new Font("Courier New", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_alreadyHaveAccount.ForeColor = Color.White;
            btn_alreadyHaveAccount.Location = new Point(17, 656);
            btn_alreadyHaveAccount.Name = "btn_alreadyHaveAccount";
            btn_alreadyHaveAccount.Size = new Size(592, 62);
            btn_alreadyHaveAccount.TabIndex = 6;
            btn_alreadyHaveAccount.Text = "← HAVE ACCOUNT? LOGIN";
            btn_alreadyHaveAccount.UseVisualStyleBackColor = false;
            btn_alreadyHaveAccount.Click += btn_alreadyHaveAccount_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = Properties.Resources.mây;
            pictureBox1.Location = new Point(507, -12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(153, 80);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 8;
            pictureBox1.TabStop = false;
            // 
            // pictureBox4
            // 
            pictureBox4.BackColor = Color.Transparent;
            pictureBox4.Image = Properties.Resources.mây;
            pictureBox4.Location = new Point(-18, 672);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new Size(194, 97);
            pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox4.TabIndex = 9;
            pictureBox4.TabStop = false;
            // 
            // pictureBox3
            // 
            pictureBox3.BackColor = Color.Transparent;
            pictureBox3.Image = Properties.Resources.mayxanh;
            pictureBox3.Location = new Point(483, 684);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(194, 97);
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.TabIndex = 10;
            pictureBox3.TabStop = false;
            // 
            // btn_register
            // 
            btn_register.BackColor = Color.FromArgb(34, 197, 94);
            btn_register.BtnColor = Color.FromArgb(34, 197, 94);
            btn_register.FlatAppearance.BorderSize = 0;
            btn_register.FlatStyle = FlatStyle.Flat;
            btn_register.Font = new Font("Courier New", 19.8000011F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_register.ForeColor = Color.FromArgb(41, 37, 36);
            btn_register.Location = new Point(17, 590);
            btn_register.Name = "btn_register";
            btn_register.Size = new Size(592, 60);
            btn_register.TabIndex = 5;
            btn_register.Text = "★ REGISTER ★";
            btn_register.UseVisualStyleBackColor = false;
            btn_register.Click += btn_register_Click;
            // 
            // panelContact
            // 
            panelContact.Controls.Add(lblContactError);
            panelContact.Controls.Add(tb_contact);
            panelContact.Controls.Add(lblContact);
            panelContact.Location = new Point(17, 228);
            panelContact.Name = "panelContact";
            panelContact.Size = new Size(592, 80);
            panelContact.TabIndex = 1;
            // 
            // lblContactError
            // 
            lblContactError.BackColor = Color.FromArgb(129, 64, 0);
            lblContactError.Dock = DockStyle.Bottom;
            lblContactError.Font = new Font("Microsoft Sans Serif", 10.8F, FontStyle.Bold);
            lblContactError.ForeColor = Color.Red;
            lblContactError.Location = new Point(0, 58);
            lblContactError.Name = "lblContactError";
            lblContactError.Size = new Size(592, 22);
            lblContactError.TabIndex = 2;
            // 
            // tb_contact
            // 
            tb_contact.BackColor = Color.FromArgb(42, 31, 26);
            tb_contact.BorderStyle = BorderStyle.None;
            tb_contact.Font = new Font("Courier New", 16.2F, FontStyle.Bold);
            tb_contact.ForeColor = Color.FromArgb(214, 211, 209);
            tb_contact.Location = new Point(0, 25);
            tb_contact.Multiline = true;
            tb_contact.Name = "tb_contact";
            tb_contact.Size = new Size(592, 31);
            tb_contact.TabIndex = 0;
            tb_contact.TextChanged += tb_contact_TextChanged;
            tb_contact.KeyDown += EnterNext_KeyDown;
            // 
            // lblContact
            // 
            lblContact.Font = new Font("Arial", 10F, FontStyle.Bold);
            lblContact.ForeColor = Color.White;
            lblContact.Location = new Point(0, 0);
            lblContact.Name = "lblContact";
            lblContact.Size = new Size(250, 20);
            lblContact.TabIndex = 1;
            lblContact.Text = "✉/📞 EMAIL/PHONE:";
            // 
            // panelRobotCheck
            // 
            panelRobotCheck.BackColor = Color.FromArgb(41, 37, 36);
            panelRobotCheck.Controls.Add(lblRobotError);
            panelRobotCheck.Controls.Add(chkNotRobot);
            panelRobotCheck.Location = new Point(17, 528);
            panelRobotCheck.Name = "panelRobotCheck";
            panelRobotCheck.Size = new Size(592, 45);
            panelRobotCheck.TabIndex = 4;
            // 
            // lblRobotError
            // 
            lblRobotError.Dock = DockStyle.Bottom;
            lblRobotError.Font = new Font("Arial", 8F, FontStyle.Bold);
            lblRobotError.ForeColor = Color.Red;
            lblRobotError.Location = new Point(0, 33);
            lblRobotError.Name = "lblRobotError";
            lblRobotError.Size = new Size(592, 12);
            lblRobotError.TabIndex = 1;
            // 
            // chkNotRobot
            // 
            chkNotRobot.Appearance = Appearance.Button;
            chkNotRobot.BackColor = Color.Transparent;
            chkNotRobot.Dock = DockStyle.Fill;
            chkNotRobot.FlatAppearance.BorderSize = 0;
            chkNotRobot.FlatAppearance.CheckedBackColor = Color.Green;
            chkNotRobot.FlatAppearance.MouseDownBackColor = Color.FromArgb(64, 64, 64);
            chkNotRobot.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 80);
            chkNotRobot.FlatStyle = FlatStyle.Flat;
            chkNotRobot.Font = new Font("Courier New", 14F, FontStyle.Bold);
            chkNotRobot.ForeColor = Color.White;
            chkNotRobot.Location = new Point(0, 0);
            chkNotRobot.Name = "chkNotRobot";
            chkNotRobot.Size = new Size(592, 45);
            chkNotRobot.TabIndex = 0;
            chkNotRobot.Text = "  ☐ I'M NOT A ROBOT  🤖";
            chkNotRobot.UseVisualStyleBackColor = false;
            chkNotRobot.TextChanged += chkNotRobot_TextChanged;
            // 
            // panelPassword
            // 
            panelPassword.Controls.Add(lblPasswordError);
            panelPassword.Controls.Add(pictureBoxLock1);
            panelPassword.Controls.Add(tb_password);
            panelPassword.Controls.Add(lblPassword);
            panelPassword.Location = new Point(17, 328);
            panelPassword.Name = "panelPassword";
            panelPassword.Size = new Size(592, 80);
            panelPassword.TabIndex = 2;
            // 
            // lblPasswordError
            // 
            lblPasswordError.BackColor = Color.FromArgb(129, 64, 0);
            lblPasswordError.Dock = DockStyle.Bottom;
            lblPasswordError.Font = new Font("Microsoft Sans Serif", 10.8F, FontStyle.Bold);
            lblPasswordError.ForeColor = Color.Red;
            lblPasswordError.Location = new Point(0, 58);
            lblPasswordError.Name = "lblPasswordError";
            lblPasswordError.Size = new Size(592, 22);
            lblPasswordError.TabIndex = 3;
            // 
            // pictureBoxLock1
            // 
            pictureBoxLock1.BackColor = Color.FromArgb(42, 31, 26);
            pictureBoxLock1.Location = new Point(554, 25);
            pictureBoxLock1.Name = "pictureBoxLock1";
            pictureBoxLock1.Size = new Size(35, 35);
            pictureBoxLock1.TabIndex = 2;
            pictureBoxLock1.TabStop = false;
            pictureBoxLock1.Click += pictureBoxLock1_Click;
            // 
            // tb_password
            // 
            tb_password.BackColor = Color.FromArgb(42, 31, 26);
            tb_password.BorderStyle = BorderStyle.None;
            tb_password.Font = new Font("Courier New", 16.2F, FontStyle.Bold);
            tb_password.ForeColor = Color.FromArgb(214, 211, 209);
            tb_password.Location = new Point(0, 25);
            tb_password.Multiline = true;
            tb_password.Name = "tb_password";
            tb_password.PasswordChar = '●';
            tb_password.Size = new Size(592, 31);
            tb_password.TabIndex = 0;
            tb_password.TextChanged += tb_password_TextChanged;
            tb_password.KeyDown += EnterNext_KeyDown;
            // 
            // lblPassword
            // 
            lblPassword.Font = new Font("Arial", 10F, FontStyle.Bold);
            lblPassword.ForeColor = Color.White;
            lblPassword.Location = new Point(0, 0);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(150, 20);
            lblPassword.TabIndex = 1;
            lblPassword.Text = "🔒 PASSWORD:";
            // 
            // panelConfirmPassword
            // 
            panelConfirmPassword.Controls.Add(lblConfirmPasswordError);
            panelConfirmPassword.Controls.Add(pictureBoxLock2);
            panelConfirmPassword.Controls.Add(tb_confirmPassword);
            panelConfirmPassword.Controls.Add(lblConfirmPassword);
            panelConfirmPassword.Location = new Point(17, 428);
            panelConfirmPassword.Name = "panelConfirmPassword";
            panelConfirmPassword.Size = new Size(592, 80);
            panelConfirmPassword.TabIndex = 3;
            // 
            // lblConfirmPasswordError
            // 
            lblConfirmPasswordError.BackColor = Color.FromArgb(129, 64, 0);
            lblConfirmPasswordError.Dock = DockStyle.Bottom;
            lblConfirmPasswordError.Font = new Font("Microsoft Sans Serif", 10.8F, FontStyle.Bold);
            lblConfirmPasswordError.ForeColor = Color.Red;
            lblConfirmPasswordError.Location = new Point(0, 58);
            lblConfirmPasswordError.Name = "lblConfirmPasswordError";
            lblConfirmPasswordError.Size = new Size(592, 22);
            lblConfirmPasswordError.TabIndex = 3;
            // 
            // pictureBoxLock2
            // 
            pictureBoxLock2.BackColor = Color.FromArgb(42, 31, 26);
            pictureBoxLock2.Location = new Point(554, 25);
            pictureBoxLock2.Name = "pictureBoxLock2";
            pictureBoxLock2.Size = new Size(35, 35);
            pictureBoxLock2.TabIndex = 2;
            pictureBoxLock2.TabStop = false;
            pictureBoxLock2.Click += pictureBoxLock2_Click;
            // 
            // tb_confirmPassword
            // 
            tb_confirmPassword.BackColor = Color.FromArgb(42, 31, 26);
            tb_confirmPassword.BorderStyle = BorderStyle.None;
            tb_confirmPassword.Font = new Font("Courier New", 16.2F, FontStyle.Bold);
            tb_confirmPassword.ForeColor = Color.FromArgb(214, 211, 209);
            tb_confirmPassword.Location = new Point(0, 25);
            tb_confirmPassword.Multiline = true;
            tb_confirmPassword.Name = "tb_confirmPassword";
            tb_confirmPassword.PasswordChar = '●';
            tb_confirmPassword.Size = new Size(592, 31);
            tb_confirmPassword.TabIndex = 0;
            tb_confirmPassword.TextChanged += tb_confirmPassword_TextChanged;
            tb_confirmPassword.KeyDown += tb_confirmPassword_KeyDown;
            // 
            // lblConfirmPassword
            // 
            lblConfirmPassword.Font = new Font("Arial", 10F, FontStyle.Bold);
            lblConfirmPassword.ForeColor = Color.White;
            lblConfirmPassword.Location = new Point(0, 0);
            lblConfirmPassword.Name = "lblConfirmPassword";
            lblConfirmPassword.Size = new Size(250, 20);
            lblConfirmPassword.TabIndex = 1;
            lblConfirmPassword.Text = "🔒 CONFIRM PASS:";
            // 
            // panelUsername
            // 
            panelUsername.Controls.Add(tb_username);
            panelUsername.Controls.Add(lblUsername);
            panelUsername.Controls.Add(lblUsernameError);
            panelUsername.Location = new Point(17, 128);
            panelUsername.Name = "panelUsername";
            panelUsername.Size = new Size(592, 80);
            panelUsername.TabIndex = 0;
            // 
            // tb_username
            // 
            tb_username.BackColor = Color.FromArgb(42, 31, 26);
            tb_username.BorderStyle = BorderStyle.None;
            tb_username.Font = new Font("Courier New", 16.2F, FontStyle.Bold);
            tb_username.ForeColor = Color.FromArgb(214, 211, 209);
            tb_username.Location = new Point(0, 25);
            tb_username.Multiline = true;
            tb_username.Name = "tb_username";
            tb_username.Size = new Size(592, 31);
            tb_username.TabIndex = 0;
            tb_username.TextChanged += tb_username_TextChanged;
            tb_username.KeyDown += EnterNext_KeyDown;
            // 
            // lblUsername
            // 
            lblUsername.Font = new Font("Arial", 10F, FontStyle.Bold);
            lblUsername.ForeColor = Color.White;
            lblUsername.Location = new Point(0, 0);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(150, 20);
            lblUsername.TabIndex = 1;
            lblUsername.Text = "👤 USERNAME:";
            // 
            // lblUsernameError
            // 
            lblUsernameError.BackColor = Color.FromArgb(129, 64, 0);
            lblUsernameError.Dock = DockStyle.Bottom;
            lblUsernameError.Font = new Font("Microsoft Sans Serif", 10.8F, FontStyle.Bold);
            lblUsernameError.ForeColor = Color.Red;
            lblUsernameError.Location = new Point(0, 58);
            lblUsernameError.Name = "lblUsernameError";
            lblUsernameError.Size = new Size(592, 22);
            lblUsernameError.TabIndex = 2;
            // 
            // FormDangKy
            // 
            ClientSize = new Size(701, 749);
            Controls.Add(panelOuter);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "FormDangKy";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "FIGHTER X FIGHTER- NEW PLAYER REGISTRATION";
            Load += FormDangKy_Load;
            panelOuter.ResumeLayout(false);
            panelMain.ResumeLayout(false);
            pnl_Main.ResumeLayout(false);
            pnl_Title.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox6).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            panelContact.ResumeLayout(false);
            panelContact.PerformLayout();
            panelRobotCheck.ResumeLayout(false);
            panelPassword.ResumeLayout(false);
            panelPassword.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLock1).EndInit();
            panelConfirmPassword.ResumeLayout(false);
            panelConfirmPassword.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLock2).EndInit();
            panelUsername.ResumeLayout(false);
            panelUsername.PerformLayout();
            ResumeLayout(false);
        }


        #endregion

        private Panel panelOuter;
        private Panel panelMain;
        private Pnl_Pixel pnl_Main;
        private Pnl_Pixel pnl_Title;
        private Label lbl_Title;
        private Label lbl_Subtitle;
        private Btn_Pixel btn_alreadyHaveAccount;
        private Btn_Pixel btn_register;
        private Panel panelContact;
        private Label lblContactError;
        private Tb_Pixel tb_contact;
        private Label lblContact;
        private Panel panelRobotCheck;
        private Label lblRobotError;
        private CheckBox chkNotRobot;
        private Panel panelPassword;
        private Label lblPasswordError;
        private PictureBox pictureBoxLock1;
        private Tb_Pixel tb_password;
        private Label lblPassword;
        private Panel panelConfirmPassword;
        private Label lblConfirmPasswordError;
        private PictureBox pictureBoxLock2;
        private Tb_Pixel tb_confirmPassword;
        private Label lblConfirmPassword;
        private Panel panelUsername;
        private Tb_Pixel tb_username;
        private Label lblUsername;
        private Label lblUsernameError;
        private PictureBox pictureBox2;
        private PictureBox pictureBox1;
        private PictureBox pictureBox4;
        private PictureBox pictureBox3;
        private PictureBox pictureBox5;
        private PictureBox pictureBox6;
    }
}
