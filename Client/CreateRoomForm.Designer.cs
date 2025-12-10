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
            lblRoomName = new Label();
            lblPassword = new Label();
            lblPasswordHint = new Label();
            txtRoomName = new TextBox();
            txtPassword = new TextBox();
            chkHasPassword = new CheckBox();
            btnCreate = new Button();
            btnCancel = new Button();
            pnlMain = new Panel();
            pictureBox1 = new PictureBox();
            pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Courier New", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Gold;
            lblTitle.Location = new Point(100, 25);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(213, 34);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "CREATE ROOM";
            // 
            // lblRoomName
            // 
            lblRoomName.AutoSize = true;
            lblRoomName.Font = new Font("Courier New", 11F, FontStyle.Bold);
            lblRoomName.ForeColor = Color.Gold;
            lblRoomName.Location = new Point(30, 75);
            lblRoomName.Name = "lblRoomName";
            lblRoomName.Size = new Size(120, 22);
            lblRoomName.TabIndex = 1;
            lblRoomName.Text = "Room Name:";
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Font = new Font("Courier New", 11F, FontStyle.Bold);
            lblPassword.ForeColor = Color.Gold;
            lblPassword.Location = new Point(30, 180);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(109, 22);
            lblPassword.TabIndex = 4;
            lblPassword.Text = "Password:";
            lblPassword.Visible = false;
            // 
            // lblPasswordHint
            // 
            lblPasswordHint.AutoSize = true;
            lblPasswordHint.Font = new Font("Courier New", 8F);
            lblPasswordHint.ForeColor = Color.LightGray;
            lblPasswordHint.Location = new Point(30, 238);
            lblPasswordHint.Name = "lblPasswordHint";
            lblPasswordHint.Size = new Size(280, 17);
            lblPasswordHint.TabIndex = 6;
            lblPasswordHint.Text = "Players need this password to join";
            lblPasswordHint.Visible = false;
            // 
            // txtRoomName
            // 
            txtRoomName.BackColor = Color.FromArgb(101, 67, 51);
            txtRoomName.BorderStyle = BorderStyle.FixedSingle;
            txtRoomName.Font = new Font("Courier New", 12F);
            txtRoomName.ForeColor = Color.White;
            txtRoomName.Location = new Point(30, 100);
            txtRoomName.MaxLength = 50;
            txtRoomName.Name = "txtRoomName";
            txtRoomName.Size = new Size(350, 30);
            txtRoomName.TabIndex = 2;
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.FromArgb(101, 67, 51);
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.Enabled = false;
            txtPassword.Font = new Font("Courier New", 12F);
            txtPassword.ForeColor = Color.White;
            txtPassword.Location = new Point(30, 205);
            txtPassword.MaxLength = 50;
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(350, 30);
            txtPassword.TabIndex = 5;
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Visible = false;
            // 
            // chkHasPassword
            // 
            chkHasPassword.AutoSize = true;
            chkHasPassword.Font = new Font("Courier New", 10F, FontStyle.Bold);
            chkHasPassword.ForeColor = Color.Gold;
            chkHasPassword.Location = new Point(30, 145);
            chkHasPassword.Name = "chkHasPassword";
            chkHasPassword.Size = new Size(293, 24);
            chkHasPassword.TabIndex = 3;
            chkHasPassword.Text = "🔒 Set Password (Optional)";
            chkHasPassword.UseVisualStyleBackColor = true;
            // 
            // btnCreate
            // 
            btnCreate.BackColor = Color.FromArgb(0, 128, 0);
            btnCreate.Cursor = Cursors.Hand;
            btnCreate.FlatAppearance.BorderColor = Color.Gold;
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btnCreate.ForeColor = Color.White;
            btnCreate.Location = new Point(30, 265);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(165, 40);
            btnCreate.TabIndex = 7;
            btnCreate.Text = "✅ CREATE";
            btnCreate.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(128, 128, 128);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.FlatAppearance.BorderColor = Color.Gold;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(215, 265);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(165, 40);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "❌ CANCEL";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // pnlMain
            // 
            pnlMain.BackColor = Color.FromArgb(74, 50, 25);
            pnlMain.BorderStyle = BorderStyle.FixedSingle;
            pnlMain.Controls.Add(pictureBox1);
            pnlMain.Controls.Add(lblTitle);
            pnlMain.Controls.Add(lblRoomName);
            pnlMain.Controls.Add(txtRoomName);
            pnlMain.Controls.Add(chkHasPassword);
            pnlMain.Controls.Add(lblPassword);
            pnlMain.Controls.Add(txtPassword);
            pnlMain.Controls.Add(lblPasswordHint);
            pnlMain.Controls.Add(btnCreate);
            pnlMain.Controls.Add(btnCancel);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 0);
            pnlMain.Name = "pnlMain";
            pnlMain.Padding = new Padding(20);
            pnlMain.Size = new Size(420, 320);
            pnlMain.TabIndex = 0;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = Properties.Resources.mây;
            pictureBox1.Location = new Point(-59, -8);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(153, 80);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 9;
            pictureBox1.TabStop = false;
            // 
            // CreateRoomForm
            // 
            AcceptButton = btnCreate;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(160, 82, 45);
            CancelButton = btnCancel;
            ClientSize = new Size(420, 320);
            Controls.Add(pnlMain);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CreateRoomForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Create New Room";
            pnlMain.ResumeLayout(false);
            pnlMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlMain;
        private Label lblTitle;
        private Label lblRoomName;
        private Label lblPassword;
        private Label lblPasswordHint;
        private TextBox txtRoomName;
        private TextBox txtPassword;
        private CheckBox chkHasPassword;
        private Button btnCreate;
        private Button btnCancel;
        private PictureBox pictureBox1;
    }
}