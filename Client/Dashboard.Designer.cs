namespace DoAn_NT106.Client
{
    partial class Dashboard
    {
        private System.ComponentModel.IContainer components = null;
        private Pnl_Pixel pnl_Main;
        private Pnl_Pixel pnl_Title;
        private System.Windows.Forms.Label lbl_Title;
        private System.Windows.Forms.Label lbl_Subtitle;
        private Btn_Pixel btn_Client;
        private Btn_Pixel btn_Server;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnl_Main = new Pnl_Pixel();
            pnl_Title = new Pnl_Pixel();
            lbl_Title = new Label();
            lbl_Subtitle = new Label();
            btn_Client = new Btn_Pixel();
            btn_Server = new Btn_Pixel();
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            pictureBox3 = new PictureBox();
            pnl_Main.SuspendLayout();
            pnl_Title.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            SuspendLayout();
            // 
            // pnl_Main
            // 
            pnl_Main.BackColor = Color.FromArgb(210, 105, 30);
            pnl_Main.Controls.Add(pnl_Title);
            pnl_Main.Controls.Add(btn_Client);
            pnl_Main.Controls.Add(btn_Server);
            pnl_Main.Controls.Add(pictureBox1);
            pnl_Main.Controls.Add(pictureBox2);
            pnl_Main.Controls.Add(pictureBox3);
            pnl_Main.Location = new Point(60, 40);
            pnl_Main.Name = "pnl_Main";
            pnl_Main.Size = new Size(380, 320);
            pnl_Main.TabIndex = 0;
            // 
            // pnl_Title
            // 
            pnl_Title.BackColor = Color.FromArgb(210, 105, 30);
            pnl_Title.Controls.Add(lbl_Title);
            pnl_Title.Controls.Add(lbl_Subtitle);
            pnl_Title.Location = new Point(20, 10);
            pnl_Title.Name = "pnl_Title";
            pnl_Title.Size = new Size(340, 80);
            pnl_Title.TabIndex = 0;
            // 
            // lbl_Title
            // 
            lbl_Title.BackColor = Color.Transparent;
            lbl_Title.Font = new Font("Courier New", 14F, FontStyle.Bold);
            lbl_Title.ForeColor = Color.Gold;
            lbl_Title.Location = new Point(-15, 10);
            lbl_Title.Name = "lbl_Title";
            lbl_Title.Size = new Size(370, 35);
            lbl_Title.TabIndex = 0;
            lbl_Title.Text = "⚙️ DASHBOARD ⚙️";
            lbl_Title.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbl_Subtitle
            // 
            lbl_Subtitle.BackColor = Color.Transparent;
            lbl_Subtitle.Font = new Font("Courier New", 8F, FontStyle.Bold);
            lbl_Subtitle.ForeColor = Color.White;
            lbl_Subtitle.Location = new Point(0, 45);
            lbl_Subtitle.Name = "lbl_Subtitle";
            lbl_Subtitle.Size = new Size(340, 25);
            lbl_Subtitle.TabIndex = 1;
            lbl_Subtitle.Text = "CHOOSE YOUR MODE";
            lbl_Subtitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btn_Client
            // 
            btn_Client.BtnColor = Color.FromArgb(34, 139, 34);
            btn_Client.FlatStyle = FlatStyle.Flat;
            btn_Client.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btn_Client.ForeColor = Color.White;
            btn_Client.Location = new Point(60, 120);
            btn_Client.Name = "btn_Client";
            btn_Client.Size = new Size(260, 60);
            btn_Client.TabIndex = 1;
            btn_Client.Text = "\U0001f9d1‍💻 CLIENT";
            btn_Client.Click += btn_Client_Click;
            // 
            // btn_Server
            // 
            btn_Server.BtnColor = Color.FromArgb(139, 69, 19);
            btn_Server.FlatStyle = FlatStyle.Flat;
            btn_Server.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btn_Server.ForeColor = Color.White;
            btn_Server.Location = new Point(60, 200);
            btn_Server.Name = "btn_Server";
            btn_Server.Size = new Size(260, 60);
            btn_Server.TabIndex = 2;
            btn_Server.Text = "🖥️ SERVER";
            btn_Server.Click += btn_Server_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = Properties.Resources.núi;
            pictureBox1.Location = new Point(0, 254);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(120, 80);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Image = Properties.Resources.mây;
            pictureBox2.Location = new Point(326, 155);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(140, 80);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 4;
            pictureBox2.TabStop = false;
            // 
            // pictureBox3
            // 
            pictureBox3.BackColor = Color.Transparent;
            pictureBox3.Image = Properties.Resources.mây;
            pictureBox3.Location = new Point(-76, 120);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(130, 80);
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.TabIndex = 5;
            pictureBox3.TabStop = false;
            // 
            // Dashboard
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.background2;
            ClientSize = new Size(500, 400);
            Controls.Add(pnl_Main);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "Dashboard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Dashboard";
            pnl_Main.ResumeLayout(false);
            pnl_Title.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ResumeLayout(false);
        }
    }
}
