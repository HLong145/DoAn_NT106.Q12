using System.Drawing;
using System.Windows.Forms;

namespace DoAn_NT106.Client
{
    partial class PasswordToJoinRoom
    {
        private DoAn_NT106.Client.Tb_Pixel tbPassword;
        private DoAn_NT106.Client.Btn_Pixel btnOK;
        private DoAn_NT106.Client.Btn_Pixel btnCancel;
        private Label lblInfo;
        private Label lblPrompt;
        private DoAn_NT106.Client.Pnl_Pixel mainPanel;

        private void InitializeComponent()
        {
            mainPanel = new Pnl_Pixel();
            lblInfo = new Label();
            lblPrompt = new Label();
            tbPassword = new Tb_Pixel();
            btnOK = new Btn_Pixel();
            btnCancel = new Btn_Pixel();
            mainPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.BackColor = Color.FromArgb(210, 105, 30);
            mainPanel.Controls.Add(lblInfo);
            mainPanel.Controls.Add(lblPrompt);
            mainPanel.Controls.Add(tbPassword);
            mainPanel.Controls.Add(btnOK);
            mainPanel.Controls.Add(btnCancel);
            mainPanel.Location = new Point(10, 10);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(354, 190);
            mainPanel.TabIndex = 0;
            // 
            // lblInfo
            // 
            lblInfo.BackColor = Color.Transparent;
            lblInfo.Font = new Font("Courier New", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblInfo.ForeColor = Color.White;
            lblInfo.Location = new Point(20, 20);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(290, 37);
            lblInfo.TabIndex = 10;
            lblInfo.Text = "Room:";
            // 
            // lblPrompt
            // 
            lblPrompt.BackColor = Color.Transparent;
            lblPrompt.Font = new Font("Courier New", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblPrompt.ForeColor = Color.White;
            lblPrompt.Location = new Point(20, 57);
            lblPrompt.Name = "lblPrompt";
            lblPrompt.Size = new Size(200, 20);
            lblPrompt.TabIndex = 10;
            lblPrompt.Text = "Please enter password:";
            // 
            // tbPassword
            // 
            tbPassword.BackColor = Color.FromArgb(42, 31, 26);
            tbPassword.BorderStyle = BorderStyle.None;
            tbPassword.Font = new Font("Courier New", 13.8F, FontStyle.Bold);
            tbPassword.ForeColor = Color.White;
            tbPassword.Location = new Point(20, 89);
            tbPassword.Multiline = true;
            tbPassword.Name = "tbPassword";
            tbPassword.Size = new Size(290, 27);
            tbPassword.TabIndex = 0;
            tbPassword.UseSystemPasswordChar = true;
            tbPassword.KeyDown += tbPassword_KeyDown;
            // 
            // btnOK
            // 
            btnOK.BtnColor = Color.FromArgb(34, 139, 34);
            btnOK.FlatStyle = FlatStyle.Flat;
            btnOK.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btnOK.ForeColor = Color.White;
            btnOK.Location = new Point(72, 138);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(83, 30);
            btnOK.TabIndex = 1;
            btnOK.Text = "OK";
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.BtnColor = Color.FromArgb(220, 20, 60);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(188, 138);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(83, 30);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.Click += BtnCancel_Click;
            // 
            // PasswordToJoinRoom
            // 
            BackColor = Color.FromArgb(240, 240, 240);
            BackgroundImage = Properties.Resources.background2;
            ClientSize = new Size(391, 205);
            Controls.Add(mainPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PasswordToJoinRoom";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Enter Password";
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            ResumeLayout(false);
        }
    }
}
