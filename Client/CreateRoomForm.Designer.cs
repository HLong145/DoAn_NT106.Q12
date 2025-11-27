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
            this.lblTitle = new Label();
            this.lblRoomName = new Label();
            this.lblPassword = new Label();
            this.lblPasswordHint = new Label();
            this.txtRoomName = new TextBox();
            this.txtPassword = new TextBox();
            this.chkHasPassword = new CheckBox();
            this.btnCreate = new Button();
            this.btnCancel = new Button();
            this.pnlMain = new Panel();
            this.pnlMain.SuspendLayout();
            this.SuspendLayout();

            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = Color.FromArgb(74, 50, 25);
            this.pnlMain.BorderStyle = BorderStyle.FixedSingle;
            this.pnlMain.Controls.Add(this.lblTitle);
            this.pnlMain.Controls.Add(this.lblRoomName);
            this.pnlMain.Controls.Add(this.txtRoomName);
            this.pnlMain.Controls.Add(this.chkHasPassword);
            this.pnlMain.Controls.Add(this.lblPassword);
            this.pnlMain.Controls.Add(this.txtPassword);
            this.pnlMain.Controls.Add(this.lblPasswordHint);
            this.pnlMain.Controls.Add(this.btnCreate);
            this.pnlMain.Controls.Add(this.btnCancel);
            this.pnlMain.Dock = DockStyle.Fill;
            this.pnlMain.Location = new Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new Padding(20);
            this.pnlMain.Size = new Size(420, 320);
            this.pnlMain.TabIndex = 0;

            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Courier New", 18F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.Gold;
            this.lblTitle.Location = new Point(100, 25);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(220, 35);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "CREATE ROOM";

            // 
            // lblRoomName
            // 
            this.lblRoomName.AutoSize = true;
            this.lblRoomName.Font = new Font("Courier New", 11F, FontStyle.Bold);
            this.lblRoomName.ForeColor = Color.Gold;
            this.lblRoomName.Location = new Point(30, 75);
            this.lblRoomName.Name = "lblRoomName";
            this.lblRoomName.Size = new Size(120, 22);
            this.lblRoomName.TabIndex = 1;
            this.lblRoomName.Text = "Room Name:";

            // 
            // txtRoomName
            // 
            this.txtRoomName.BackColor = Color.FromArgb(101, 67, 51);
            this.txtRoomName.BorderStyle = BorderStyle.FixedSingle;
            this.txtRoomName.Font = new Font("Courier New", 12F);
            this.txtRoomName.ForeColor = Color.White;
            this.txtRoomName.Location = new Point(30, 100);
            this.txtRoomName.MaxLength = 50;
            this.txtRoomName.Name = "txtRoomName";
            this.txtRoomName.Size = new Size(350, 30);
            this.txtRoomName.TabIndex = 2;

            // 
            // chkHasPassword
            // 
            this.chkHasPassword.AutoSize = true;
            this.chkHasPassword.Font = new Font("Courier New", 10F, FontStyle.Bold);
            this.chkHasPassword.ForeColor = Color.Gold;
            this.chkHasPassword.Location = new Point(30, 145);
            this.chkHasPassword.Name = "chkHasPassword";
            this.chkHasPassword.Size = new Size(280, 24);
            this.chkHasPassword.TabIndex = 3;
            this.chkHasPassword.Text = "🔒 Set Password (Optional)";
            this.chkHasPassword.UseVisualStyleBackColor = true;

            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new Font("Courier New", 11F, FontStyle.Bold);
            this.lblPassword.ForeColor = Color.Gold;
            this.lblPassword.Location = new Point(30, 180);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new Size(100, 22);
            this.lblPassword.TabIndex = 4;
            this.lblPassword.Text = "Password:";
            this.lblPassword.Visible = false;

            // 
            // txtPassword
            // 
            this.txtPassword.BackColor = Color.FromArgb(101, 67, 51);
            this.txtPassword.BorderStyle = BorderStyle.FixedSingle;
            this.txtPassword.Enabled = false;
            this.txtPassword.Font = new Font("Courier New", 12F);
            this.txtPassword.ForeColor = Color.White;
            this.txtPassword.Location = new Point(30, 205);
            this.txtPassword.MaxLength = 50;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new Size(350, 30);
            this.txtPassword.TabIndex = 5;
            this.txtPassword.UseSystemPasswordChar = true;
            this.txtPassword.Visible = false;

            // 
            // lblPasswordHint
            // 
            this.lblPasswordHint.AutoSize = true;
            this.lblPasswordHint.Font = new Font("Courier New", 8F);
            this.lblPasswordHint.ForeColor = Color.LightGray;
            this.lblPasswordHint.Location = new Point(30, 238);
            this.lblPasswordHint.Name = "lblPasswordHint";
            this.lblPasswordHint.Size = new Size(300, 18);
            this.lblPasswordHint.TabIndex = 6;
            this.lblPasswordHint.Text = "Players need this password to join";
            this.lblPasswordHint.Visible = false;

            // 
            // btnCreate
            // 
            this.btnCreate.BackColor = Color.FromArgb(0, 128, 0);
            this.btnCreate.Cursor = Cursors.Hand;
            this.btnCreate.FlatAppearance.BorderColor = Color.Gold;
            this.btnCreate.FlatStyle = FlatStyle.Flat;
            this.btnCreate.Font = new Font("Courier New", 12F, FontStyle.Bold);
            this.btnCreate.ForeColor = Color.White;
            this.btnCreate.Location = new Point(30, 265);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new Size(165, 40);
            this.btnCreate.TabIndex = 7;
            this.btnCreate.Text = "✅ CREATE";
            this.btnCreate.UseVisualStyleBackColor = false;

            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = Color.FromArgb(128, 128, 128);
            this.btnCancel.Cursor = Cursors.Hand;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderColor = Color.Gold;
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.Font = new Font("Courier New", 12F, FontStyle.Bold);
            this.btnCancel.ForeColor = Color.White;
            this.btnCancel.Location = new Point(215, 265);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(165, 40);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "❌ CANCEL";
            this.btnCancel.UseVisualStyleBackColor = false;

            // 
            // CreateRoomForm
            // 
            this.AcceptButton = this.btnCreate;
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(160, 82, 45);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new Size(420, 320);
            this.Controls.Add(this.pnlMain);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateRoomForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Create New Room";
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            this.ResumeLayout(false);
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
    }
}