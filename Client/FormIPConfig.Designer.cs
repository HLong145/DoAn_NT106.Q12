namespace DoAn_NT106.Client
{
    partial class FormIPConfig
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private Pnl_Pixel pnlMain;
        private Pnl_Pixel pnlTitle;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblIP;
        private Tb_Pixel txtServerIP;
        private Btn_Pixel btnConnect;
        private Btn_Pixel btnCancel;
        private System.Windows.Forms.Label lblStatus;

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
            pnlMain = new Pnl_Pixel();
            pnlTitle = new Pnl_Pixel();
            lblTitle = new System.Windows.Forms.Label();
            lblSubtitle = new System.Windows.Forms.Label();
            lblIP = new System.Windows.Forms.Label();
            txtServerIP = new Tb_Pixel();
            lblStatus = new System.Windows.Forms.Label();
            btnConnect = new Btn_Pixel();
            btnCancel = new Btn_Pixel();

            pnlMain.SuspendLayout();
            pnlTitle.SuspendLayout();
            SuspendLayout();

            // 
            // pnlMain
            // 
            pnlMain.BackColor = System.Drawing.Color.FromArgb(210, 105, 30);
            pnlMain.Controls.Add(pnlTitle);
            pnlMain.Controls.Add(lblIP);
            pnlMain.Controls.Add(txtServerIP);
            pnlMain.Controls.Add(lblStatus);
            pnlMain.Controls.Add(btnConnect);
            pnlMain.Controls.Add(btnCancel);
            pnlMain.Location = new System.Drawing.Point(30, 20);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new System.Drawing.Size(360, 240);
            pnlMain.TabIndex = 0;

            // 
            // pnlTitle
            // 
            pnlTitle.BackColor = System.Drawing.Color.FromArgb(210, 105, 30);
            pnlTitle.Controls.Add(lblTitle);
            pnlTitle.Controls.Add(lblSubtitle);
            pnlTitle.Location = new System.Drawing.Point(10, 10);
            pnlTitle.Name = "pnlTitle";
            pnlTitle.Size = new System.Drawing.Size(340, 60);
            pnlTitle.TabIndex = 0;

            // 
            // lblTitle
            // 
            lblTitle.BackColor = System.Drawing.Color.Transparent;
            lblTitle.Font = new System.Drawing.Font("Courier New", 14F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.Gold;
            lblTitle.Location = new System.Drawing.Point(0, 5);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(340, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "⚔️ SERVER CONFIG ⚔️";
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // lblSubtitle
            // 
            lblSubtitle.BackColor = System.Drawing.Color.Transparent;
            lblSubtitle.Font = new System.Drawing.Font("Courier New", 8F, System.Drawing.FontStyle.Bold);
            lblSubtitle.ForeColor = System.Drawing.Color.White;
            lblSubtitle.Location = new System.Drawing.Point(0, 35);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new System.Drawing.Size(340, 20);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Enter server IP to connect";
            lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // lblIP
            // 
            lblIP.BackColor = System.Drawing.Color.Transparent;
            lblIP.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Bold);
            lblIP.ForeColor = System.Drawing.Color.White;
            lblIP.Location = new System.Drawing.Point(20, 85);
            lblIP.Name = "lblIP";
            lblIP.Size = new System.Drawing.Size(120, 20);
            lblIP.TabIndex = 1;
            lblIP.Text = "🌐 SERVER IP:";

            // 
            // txtServerIP
            // 
            txtServerIP.BackColor = System.Drawing.Color.FromArgb(42, 31, 26);
            txtServerIP.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtServerIP.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold);
            txtServerIP.ForeColor = System.Drawing.Color.White;
            txtServerIP.Location = new System.Drawing.Point(20, 108);
            txtServerIP.Name = "txtServerIP";
            txtServerIP.Size = new System.Drawing.Size(320, 30);
            txtServerIP.TabIndex = 0;
            txtServerIP.Text = "192.168.0.1";
            txtServerIP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            txtServerIP.KeyDown += txtServerIP_KeyDown;

            // 
            // lblStatus
            // 
            lblStatus.BackColor = System.Drawing.Color.Transparent;
            lblStatus.Font = new System.Drawing.Font("Courier New", 8F, System.Drawing.FontStyle.Bold);
            lblStatus.ForeColor = System.Drawing.Color.Yellow;
            lblStatus.Location = new System.Drawing.Point(20, 145);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(320, 20);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "";
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // btnConnect
            // 
            btnConnect.BtnColor = System.Drawing.Color.FromArgb(34, 139, 34);
            btnConnect.Cursor = System.Windows.Forms.Cursors.Hand;
            btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnConnect.Font = new System.Drawing.Font("Courier New", 11F, System.Drawing.FontStyle.Bold);
            btnConnect.ForeColor = System.Drawing.Color.White;
            btnConnect.Location = new System.Drawing.Point(20, 175);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new System.Drawing.Size(150, 45);
            btnConnect.TabIndex = 1;
            btnConnect.Text = "✓ CONNECT";
            btnConnect.Click += btnConnect_Click;

            // 
            // btnCancel
            // 
            btnCancel.BtnColor = System.Drawing.Color.FromArgb(178, 34, 34);
            btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCancel.Font = new System.Drawing.Font("Courier New", 11F, System.Drawing.FontStyle.Bold);
            btnCancel.ForeColor = System.Drawing.Color.White;
            btnCancel.Location = new System.Drawing.Point(190, 175);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(150, 45);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "✗ EXIT";
            btnCancel.Click += btnCancel_Click;

            // 
            // FormIPConfig
            // 
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.BackgroundImage = Properties.Resources.background2;
            this.ClientSize = new System.Drawing.Size(420, 280);
            this.Controls.Add(pnlMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormIPConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Server Configuration";

            pnlMain.ResumeLayout(false);
            pnlMain.PerformLayout();
            pnlTitle.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }

}
