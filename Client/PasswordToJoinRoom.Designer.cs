using System.Drawing;
using System.Windows.Forms;

namespace DoAn_NT106
{
    partial class PasswordToJoinRoom
    {
        private DoAn_NT106.Tb_Pixel txtPassword;
        private DoAn_NT106.Btn_Pixel btnOK;
        private DoAn_NT106.Btn_Pixel btnCancel;
        private Label lblInfo;
        private Label lblPrompt;
        private DoAn_NT106.Pnl_Pixel mainPanel;

        private void InitializeComponent()
        {
            mainPanel = new Pnl_Pixel();
            lblInfo = new Label();
            lblPrompt = new Label();
            txtPassword = new Tb_Pixel();
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
            mainPanel.Controls.Add(txtPassword);
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
            lblInfo.TabIndex = 0;
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
            lblPrompt.TabIndex = 1;
            lblPrompt.Text = "Please enter password:";
            // 
            // txtPassword
            // 
            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.Font = new Font("Arial Narrow", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtPassword.Location = new Point(20, 89);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(290, 27);
            txtPassword.TabIndex = 2;
            txtPassword.UseSystemPasswordChar = true;
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
            btnOK.TabIndex = 3;
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
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.Click += BtnCancel_Click;
            // 
            // PasswordForm
            // 
            BackColor = Color.FromArgb(240, 240, 240);
            BackgroundImage = Properties.Resources.background2;
            ClientSize = new Size(391, 205);
            Controls.Add(mainPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PasswordForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Enter Password";
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            ResumeLayout(false);
        }
    }
}
