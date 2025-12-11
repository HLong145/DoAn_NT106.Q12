namespace DoAn_NT106.Client
{
    partial class CreateRoomForm
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblSubtitle = new Label();
            lblRoomName = new Label();
            lblPassword = new Label();
            lblPasswordHint = new Label();
            txtRoomName = new TextBox();
            txtPassword = new TextBox();
            chkHasPassword = new CheckBox();
            btnCreate = new Btn_Pixel();
            btnCancel = new Btn_Pixel();
            pnlMain = new Pnl_Pixel();
            pictureBox2 = new PictureBox();
            pnlHeader = new Pnl_Pixel();
            pictureBox1 = new PictureBox();
            pictureBox6 = new PictureBox();
            pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Courier New", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Gold;
            lblTitle.Location = new Point(71, 18);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(207, 27);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🎮 CREATE ROOM";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.BackColor = Color.Transparent;
            lblSubtitle.Font = new Font("Courier New", 8F, FontStyle.Bold);
            lblSubtitle.ForeColor = Color.White;
            lblSubtitle.Location = new Point(89, 45);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(175, 16);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "SET UP YOUR GAME ROOM";
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblRoomName
            // 
            lblRoomName.AutoSize = true;
            lblRoomName.BackColor = Color.Transparent;
            lblRoomName.Font = new Font("Courier New", 10F, FontStyle.Bold);
            lblRoomName.ForeColor = Color.Gold;
            lblRoomName.Location = new Point(30, 105);
            lblRoomName.Name = "lblRoomName";
            lblRoomName.Size = new Size(109, 20);
            lblRoomName.TabIndex = 1;
            lblRoomName.Text = "Room Name:";
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.BackColor = Color.Transparent;
            lblPassword.Font = new Font("Courier New", 10F, FontStyle.Bold);
            lblPassword.ForeColor = Color.Gold;
            lblPassword.Location = new Point(30, 193);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(99, 20);
            lblPassword.TabIndex = 4;
            lblPassword.Text = "Password:";
            lblPassword.Visible = false;
            // 
            // lblPasswordHint
            // 
            lblPasswordHint.AutoSize = true;
            lblPasswordHint.BackColor = Color.Transparent;
            lblPasswordHint.Font = new Font("Courier New", 7F);
            lblPasswordHint.ForeColor = Color.LightGoldenrodYellow;
            lblPasswordHint.Location = new Point(41, 245);
            lblPasswordHint.Name = "lblPasswordHint";
            lblPasswordHint.Size = new Size(287, 15);
            lblPasswordHint.TabIndex = 6;
            lblPasswordHint.Text = "Other players need this password to join";
            lblPasswordHint.Visible = false;
            // 
            // txtRoomName
            // 
            txtRoomName.BackColor = Color.FromArgb(255, 192, 128);
            txtRoomName.BorderStyle = BorderStyle.FixedSingle;
            txtRoomName.Font = new Font("Courier New", 10F);
            txtRoomName.ForeColor = Color.White;
            txtRoomName.Location = new Point(30, 130);
            txtRoomName.MaxLength = 50;
            txtRoomName.Name = "txtRoomName";
            txtRoomName.Size = new Size(320, 26);
            txtRoomName.TabIndex = 2;
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.FromArgb(255, 192, 128);
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.Enabled = false;
            txtPassword.Font = new Font("Courier New", 10F);
            txtPassword.ForeColor = Color.White;
            txtPassword.Location = new Point(30, 216);
            txtPassword.MaxLength = 50;
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(320, 26);
            txtPassword.TabIndex = 5;
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Visible = false;
            // 
            // chkHasPassword
            // 
            chkHasPassword.AutoSize = true;
            chkHasPassword.BackColor = Color.Transparent;
            chkHasPassword.Font = new Font("Courier New", 9F, FontStyle.Bold);
            chkHasPassword.ForeColor = Color.Gold;
            chkHasPassword.Location = new Point(30, 169);
            chkHasPassword.Name = "chkHasPassword";
            chkHasPassword.Size = new Size(263, 21);
            chkHasPassword.TabIndex = 3;
            chkHasPassword.Text = "🔒 Set Password (Optional)";
            chkHasPassword.UseVisualStyleBackColor = true;
            // 
            // btnCreate
            // 
            btnCreate.BtnColor = Color.FromArgb(34, 139, 34);
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.Font = new Font("Courier New", 11F, FontStyle.Bold);
            btnCreate.ForeColor = Color.White;
            btnCreate.Location = new Point(30, 263);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(155, 45);
            btnCreate.TabIndex = 7;
            btnCreate.Text = "✅ CREATE";
            btnCreate.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            btnCancel.BtnColor = Color.FromArgb(220, 20, 60);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Courier New", 11F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(195, 263);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(155, 45);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "❌ CANCEL";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // pnlMain
            // 
            pnlMain.BackColor = Color.FromArgb(210, 105, 30);
            pnlMain.Controls.Add(pictureBox2);
            pnlMain.Controls.Add(pnlHeader);
            pnlMain.Controls.Add(lblRoomName);
            pnlMain.Controls.Add(txtRoomName);
            pnlMain.Controls.Add(chkHasPassword);
            pnlMain.Controls.Add(lblPassword);
            pnlMain.Controls.Add(txtPassword);
            pnlMain.Controls.Add(lblPasswordHint);
            pnlMain.Controls.Add(btnCreate);
            pnlMain.Controls.Add(btnCancel);
            pnlMain.Location = new Point(-2, -1);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(381, 323);
            pnlMain.TabIndex = 0;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Image = Properties.Resources.mây;
            pictureBox2.Location = new Point(309, 169);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(100, 32);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 10;
            pictureBox2.TabStop = false;
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(210, 105, 30);
            pnlHeader.Controls.Add(pictureBox6);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(pictureBox1);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Location = new Point(15, 10);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(350, 80);
            pnlHeader.TabIndex = 0;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = Properties.Resources.mây;
            pictureBox1.Location = new Point(-12, 26);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(60, 35);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 9;
            pictureBox1.TabStop = false;
            // 
            // pictureBox6
            // 
            pictureBox6.BackColor = Color.Transparent;
            pictureBox6.Image = Properties.Resources.mayxanh;
            pictureBox6.Location = new Point(291, 18);
            pictureBox6.Name = "pictureBox6";
            pictureBox6.Size = new Size(103, 33);
            pictureBox6.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox6.TabIndex = 11;
            pictureBox6.TabStop = false;
            // 
            // CreateRoomForm
            // 
            AcceptButton = btnCreate;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(160, 82, 45);
            CancelButton = btnCancel;
            ClientSize = new Size(376, 319);
            ControlBox = false;
            Controls.Add(pnlMain);
            FormBorderStyle = FormBorderStyle.None;
            Name = "CreateRoomForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Create New Room";
            pnlMain.ResumeLayout(false);
            pnlMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Pnl_Pixel pnlMain;
        private Pnl_Pixel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblRoomName;
        private Label lblPassword;
        private Label lblPasswordHint;
        private TextBox txtRoomName;
        private TextBox txtPassword;
        private CheckBox chkHasPassword;
        private Btn_Pixel btnCreate;
        private Btn_Pixel btnCancel;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private PictureBox pictureBox6;
    }
}
