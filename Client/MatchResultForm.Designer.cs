using System.Drawing;

namespace DoAn_NT106.Client
{
    partial class MatchResultForm
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
            btn_ReturnLobby = new Btn_Pixel();
            pictureBox1 = new PictureBox();
            pictureBox4 = new PictureBox();
            pictureBox3 = new PictureBox();
            panelStats = new Panel();
            lblStats = new Label();
            panelResults = new Panel();
            lblWinner = new Label();
            lblWinnerName = new Label();
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
            panelStats.SuspendLayout();
            panelResults.SuspendLayout();
            SuspendLayout();
            // 
            // panelOuter
            // 
            panelOuter.BackColor = Color.FromArgb(180, 83, 9);
            panelOuter.Controls.Add(panelMain);
            panelOuter.Dock = DockStyle.Fill;
            panelOuter.Location = new Point(0, 0);
            panelOuter.Name = "panelOuter";
            panelOuter.Padding = new Padding(10);
            panelOuter.Size = new Size(600, 500);
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
            panelMain.Location = new Point(10, 10);
            panelMain.Name = "panelMain";
            panelMain.Padding = new Padding(20);
            panelMain.Size = new Size(580, 480);
            panelMain.TabIndex = 0;
            // 
            // pnl_Main
            // 
            pnl_Main.BackColor = Color.FromArgb(210, 105, 30);
            pnl_Main.BackgroundImage = Properties.Resources.background2;
            pnl_Main.Controls.Add(pnl_Title);
            pnl_Main.Controls.Add(btn_ReturnLobby);
            pnl_Main.Controls.Add(pictureBox1);
            pnl_Main.Controls.Add(pictureBox4);
            pnl_Main.Controls.Add(pictureBox3);
            pnl_Main.Controls.Add(panelStats);
            pnl_Main.Controls.Add(panelResults);
            pnl_Main.Location = new Point(15, 10);
            pnl_Main.Name = "pnl_Main";
            pnl_Main.Size = new Size(550, 460);
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
            pnl_Title.Location = new Point(35, 10);
            pnl_Title.Name = "pnl_Title";
            pnl_Title.Size = new Size(480, 80);
            pnl_Title.TabIndex = 7;
            // 
            // lbl_Title
            // 
            lbl_Title.BackColor = Color.Transparent;
            lbl_Title.Font = new Font("Courier New", 16F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbl_Title.ForeColor = Color.Gold;
            lbl_Title.Location = new Point(75, 10);
            lbl_Title.Name = "lbl_Title";
            lbl_Title.Size = new Size(330, 25);
            lbl_Title.TabIndex = 0;
            lbl_Title.Text = "** MATCH RESULT **";
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
            lbl_Subtitle.Font = new Font("Courier New", 6.5F, FontStyle.Bold);
            lbl_Subtitle.ForeColor = Color.White;
            lbl_Subtitle.Location = new Point(75, 50);
            lbl_Subtitle.Name = "lbl_Subtitle";
            lbl_Subtitle.Size = new Size(330, 15);
            lbl_Subtitle.TabIndex = 4;
            lbl_Subtitle.Text = "MATCH COMPLETED";
            lbl_Subtitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btn_ReturnLobby
            // 
            btn_ReturnLobby.BackColor = Color.FromArgb(217, 119, 6);
            btn_ReturnLobby.BtnColor = Color.FromArgb(217, 119, 6);
            btn_ReturnLobby.FlatAppearance.BorderSize = 0;
            btn_ReturnLobby.FlatStyle = FlatStyle.Flat;
            btn_ReturnLobby.Font = new Font("Courier New", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_ReturnLobby.ForeColor = Color.White;
            btn_ReturnLobby.Location = new Point(15, 330);
            btn_ReturnLobby.Name = "btn_ReturnLobby";
            btn_ReturnLobby.Size = new Size(520, 45);
            btn_ReturnLobby.TabIndex = 6;
            btn_ReturnLobby.Text = "<- RETURN TO LOBBY";
            btn_ReturnLobby.UseVisualStyleBackColor = false;
            btn_ReturnLobby.Click += btn_ReturnLobby_Click;
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
            pictureBox4.Location = new Point(-18, 380);
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
            pictureBox3.Location = new Point(356, 390);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(194, 97);
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.TabIndex = 10;
            pictureBox3.TabStop = false;
            // 
            // panelStats
            // 
            panelStats.Controls.Add(lblStats);
            panelStats.Location = new Point(15, 180);
            panelStats.Name = "panelStats";
            panelStats.Size = new Size(520, 140);
            panelStats.TabIndex = 1;
            // 
            // lblStats
            // 
            lblStats.BackColor = Color.FromArgb(42, 31, 26);
            lblStats.Font = new Font("Courier New", 10F, FontStyle.Bold);
            lblStats.ForeColor = Color.FromArgb(214, 211, 209);
            lblStats.Location = new Point(0, 0);
            lblStats.Name = "lblStats";
            lblStats.Padding = new Padding(8);
            lblStats.Size = new Size(520, 140);
            lblStats.TabIndex = 0;
            lblStats.Text = "Match Statistics";
            lblStats.TextAlign = ContentAlignment.TopCenter;
            // 
            // panelResults
            // 
            panelResults.Controls.Add(lblWinner);
            panelResults.Controls.Add(lblWinnerName);
            panelResults.Location = new Point(15, 100);
            panelResults.Name = "panelResults";
            panelResults.Size = new Size(520, 70);
            panelResults.TabIndex = 0;
            // 
            // lblWinner
            // 
            lblWinner.BackColor = Color.FromArgb(42, 31, 26);
            lblWinner.Font = new Font("Courier New", 12F, FontStyle.Bold);
            lblWinner.ForeColor = Color.Gold;
            lblWinner.Location = new Point(0, 0);
            lblWinner.Name = "lblWinner";
            lblWinner.Size = new Size(520, 25);
            lblWinner.TabIndex = 0;
            lblWinner.Text = "** VICTORY **";
            lblWinner.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblWinnerName
            // 
            lblWinnerName.BackColor = Color.FromArgb(42, 31, 26);
            lblWinnerName.Font = new Font("Courier New", 13F, FontStyle.Bold);
            lblWinnerName.ForeColor = Color.Lime;
            lblWinnerName.Location = new Point(0, 25);
            lblWinnerName.Name = "lblWinnerName";
            lblWinnerName.Size = new Size(520, 45);
            lblWinnerName.TabIndex = 1;
            lblWinnerName.Text = "PLAYER NAME";
            lblWinnerName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // MatchResultForm
            // 
            ClientSize = new Size(600, 500);
            Controls.Add(panelOuter);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "MatchResultForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "FIGHTER X FIGHTER - MATCH RESULT";
            Load += MatchResultForm_Load;
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
            panelStats.ResumeLayout(false);
            panelResults.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelOuter;
        private Panel panelMain;
        private Pnl_Pixel pnl_Main;
        private Pnl_Pixel pnl_Title;
        private Label lbl_Title;
        private Label lbl_Subtitle;
        private Btn_Pixel btn_ReturnLobby;
        private Panel panelResults;
        private Label lblWinner;
        private Label lblWinnerName;
        private Panel panelStats;
        private Label lblStats;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
        private PictureBox pictureBox4;
        private PictureBox pictureBox5;
        private PictureBox pictureBox6;
    }
}
