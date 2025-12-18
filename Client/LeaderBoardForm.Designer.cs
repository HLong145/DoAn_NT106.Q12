using DoAn_NT106.Client;

namespace DoAn_NT106
{
    partial class LeaderBoardForm
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

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            pbLabel = new PictureBox();
            pbCrown = new PictureBox();
            pbTop1 = new PictureBox();
            pbTop2 = new PictureBox();
            pbTop3 = new PictureBox();
            pbRank2 = new PictureBox();
            pbRank3 = new PictureBox();
            lblTop1Name = new Label();
            lblTop1Level = new Label();
            lblTop1Score = new Label();
            lblTop2Name = new Label();
            lblTop2Level = new Label();
            lblTop2Score = new Label();
            lblTop3Name = new Label();
            lblTop3Level = new Label();
            lblTop3Score = new Label();
            dgv = new DataGridView();
            colRank = new DataGridViewTextBoxColumn();
            colLevel = new DataGridViewTextBoxColumn();
            colPlayer = new DataGridViewTextBoxColumn();
            colScore = new DataGridViewTextBoxColumn();
            btnClose = new Btn_Pixel();
            ((System.ComponentModel.ISupportInitialize)pbLabel).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbCrown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbTop1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbTop2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbTop3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbRank2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbRank3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgv).BeginInit();
            SuspendLayout();
            // 
            // pbLabel
            // 
            pbLabel.Image = Properties.Resources.label_leaderboard;
            pbLabel.Location = new Point(20, 6);
            pbLabel.Name = "pbLabel";
            pbLabel.Size = new Size(480, 120);
            pbLabel.SizeMode = PictureBoxSizeMode.StretchImage;
            pbLabel.TabIndex = 0;
            pbLabel.TabStop = false;
            // 
            // pbCrown
            // 
            pbCrown.BackColor = Color.Transparent;
            pbCrown.Image = Properties.Resources.crown;
            pbCrown.Location = new Point(232, 110);
            pbCrown.Name = "pbCrown";
            pbCrown.Size = new Size(56, 36);
            pbCrown.SizeMode = PictureBoxSizeMode.StretchImage;
            pbCrown.TabIndex = 2;
            pbCrown.TabStop = false;
            pbCrown.Visible = false;
            // 
            // pbTop1
            // 
            pbTop1.BackColor = Color.Transparent;
            pbTop1.Image = Properties.Resources.boy1;
            pbTop1.Location = new Point(180, 140);
            pbTop1.Name = "pbTop1";
            pbTop1.Size = new Size(160, 160);
            pbTop1.SizeMode = PictureBoxSizeMode.Zoom;
            pbTop1.TabIndex = 1;
            pbTop1.TabStop = false;
            // 
            // pbTop2
            // 
            pbTop2.BackColor = Color.Transparent;
            pbTop2.Image = Properties.Resources.boy2;
            pbTop2.Location = new Point(60, 160);
            pbTop2.Name = "pbTop2";
            pbTop2.Size = new Size(120, 120);
            pbTop2.SizeMode = PictureBoxSizeMode.Zoom;
            pbTop2.TabIndex = 3;
            pbTop2.TabStop = false;
            // 
            // pbTop3
            // 
            pbTop3.BackColor = Color.Transparent;
            pbTop3.Image = Properties.Resources.boy3;
            pbTop3.Location = new Point(360, 160);
            pbTop3.Name = "pbTop3";
            pbTop3.Size = new Size(120, 120);
            pbTop3.SizeMode = PictureBoxSizeMode.Zoom;
            pbTop3.TabIndex = 4;
            pbTop3.TabStop = false;
            // 
            // pbRank2
            // 
            pbRank2.BackColor = Color.Transparent;
            pbRank2.Image = Properties.Resources.rank2;
            pbRank2.Location = new Point(70, 140);
            pbRank2.Name = "pbRank2";
            pbRank2.Size = new Size(31, 40);
            pbRank2.SizeMode = PictureBoxSizeMode.Zoom;
            pbRank2.TabIndex = 13;
            pbRank2.TabStop = false;
            pbRank2.Visible = false;
            // 
            // pbRank3
            // 
            pbRank3.BackColor = Color.Transparent;
            pbRank3.Image = Properties.Resources.rank3;
            pbRank3.Location = new Point(360, 140);
            pbRank3.Name = "pbRank3";
            pbRank3.Size = new Size(31, 40);
            pbRank3.SizeMode = PictureBoxSizeMode.Zoom;
            pbRank3.TabIndex = 14;
            pbRank3.TabStop = false;
            pbRank3.Visible = false;
            // 
            // lblTop1Name
            // 
            lblTop1Name.Font = new Font("Courier New", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTop1Name.ForeColor = Color.White;
            lblTop1Name.Location = new Point(180, 300);
            lblTop1Name.Name = "lblTop1Name";
            lblTop1Name.Size = new Size(160, 28);
            lblTop1Name.TabIndex = 5;
            lblTop1Name.Text = "Player";
            lblTop1Name.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTop1Level
            // 
            lblTop1Level.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTop1Level.ForeColor = Color.LightGray;
            lblTop1Level.Location = new Point(180, 330);
            lblTop1Level.Name = "lblTop1Level";
            lblTop1Level.Size = new Size(160, 22);
            lblTop1Level.TabIndex = 5;
            lblTop1Level.Text = "Lv 1";
            lblTop1Level.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTop1Score
            // 
            lblTop1Score.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTop1Score.ForeColor = Color.Gold;
            lblTop1Score.Location = new Point(180, 355);
            lblTop1Score.Name = "lblTop1Score";
            lblTop1Score.Size = new Size(160, 36);
            lblTop1Score.TabIndex = 6;
            lblTop1Score.Text = "55000";
            lblTop1Score.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTop2Name
            // 
            lblTop2Name.Font = new Font("Courier New", 13.8F, FontStyle.Bold);
            lblTop2Name.ForeColor = Color.White;
            lblTop2Name.Location = new Point(60, 285);
            lblTop2Name.Name = "lblTop2Name";
            lblTop2Name.Size = new Size(120, 24);
            lblTop2Name.TabIndex = 7;
            lblTop2Name.Text = "Player";
            lblTop2Name.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTop2Level
            // 
            lblTop2Level.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTop2Level.ForeColor = Color.LightGray;
            lblTop2Level.Location = new Point(60, 310);
            lblTop2Level.Name = "lblTop2Level";
            lblTop2Level.Size = new Size(120, 22);
            lblTop2Level.TabIndex = 7;
            lblTop2Level.Text = "Lv 1";
            lblTop2Level.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTop2Score
            // 
            lblTop2Score.Font = new Font("Segoe UI", 12F);
            lblTop2Score.ForeColor = Color.White;
            lblTop2Score.Location = new Point(60, 330);
            lblTop2Score.Name = "lblTop2Score";
            lblTop2Score.Size = new Size(120, 34);
            lblTop2Score.TabIndex = 8;
            lblTop2Score.Text = "45600";
            lblTop2Score.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTop3Name
            // 
            lblTop3Name.Font = new Font("Courier New", 13.8F, FontStyle.Bold);
            lblTop3Name.ForeColor = Color.White;
            lblTop3Name.Location = new Point(340, 285);
            lblTop3Name.Name = "lblTop3Name";
            lblTop3Name.Size = new Size(120, 24);
            lblTop3Name.TabIndex = 9;
            lblTop3Name.Text = "Player";
            lblTop3Name.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTop3Level
            // 
            lblTop3Level.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTop3Level.ForeColor = Color.LightGray;
            lblTop3Level.Location = new Point(340, 310);
            lblTop3Level.Name = "lblTop3Level";
            lblTop3Level.Size = new Size(120, 22);
            lblTop3Level.TabIndex = 9;
            lblTop3Level.Text = "Lv 1";
            lblTop3Level.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTop3Score
            // 
            lblTop3Score.Font = new Font("Segoe UI", 12F);
            lblTop3Score.ForeColor = Color.White;
            lblTop3Score.Location = new Point(340, 336);
            lblTop3Score.Name = "lblTop3Score";
            lblTop3Score.Size = new Size(120, 28);
            lblTop3Score.TabIndex = 10;
            lblTop3Score.Text = "35800";
            lblTop3Score.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // dgv
            // 
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.BackgroundColor = Color.FromArgb(255, 224, 192);
            dgv.BorderStyle = BorderStyle.None;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(64, 0, 0);
            dataGridViewCellStyle1.Font = new Font("Courier New", 12F, FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = Color.Gold;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgv.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgv.ColumnHeadersHeight = 29;
            dgv.Columns.AddRange(new DataGridViewColumn[] { colRank, colLevel, colPlayer, colScore });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(255, 224, 192);
            dataGridViewCellStyle2.Font = new Font("Courier New", 11F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dgv.DefaultCellStyle = dataGridViewCellStyle2;
            dgv.EnableHeadersVisualStyles = false;
            dgv.Location = new Point(20, 400);
            dgv.Name = "dgv";
            dgv.ReadOnly = true;
            dgv.RowHeadersVisible = false;
            dgv.RowHeadersWidth = 51;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.Size = new Size(480, 197);
            dgv.TabIndex = 11;
            // 
            // colRank
            // 
            colRank.HeaderText = "RANK";
            colRank.MinimumWidth = 6;
            colRank.Name = "colRank";
            colRank.ReadOnly = true;
            colRank.Width = 60;
            // 
            // colLevel
            // 
            colLevel.HeaderText = "LEVEL";
            colLevel.MinimumWidth = 6;
            colLevel.Name = "colLevel";
            colLevel.ReadOnly = true;
            colLevel.Width = 70;
            // 
            // colPlayer
            // 
            colPlayer.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colPlayer.HeaderText = "NAME";
            colPlayer.MinimumWidth = 6;
            colPlayer.Name = "colPlayer";
            colPlayer.ReadOnly = true;
            // 
            // colScore
            // 
            colScore.HeaderText = "XP";
            colScore.MinimumWidth = 6;
            colScore.Name = "colScore";
            colScore.ReadOnly = true;
            colScore.Width = 120;
            // 
            // btnClose
            // 
            btnClose.BtnColor = Color.FromArgb(178, 34, 34);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Courier New", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnClose.ForeColor = Color.Gold;
            btnClose.Location = new Point(167, 613);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(160, 40);
            btnClose.TabIndex = 12;
            btnClose.Text = "CLOSE";
            btnClose.Click += btnClose_Click;
            // 
            // LeaderBoardForm
            // 
            BackColor = Color.FromArgb(101, 67, 51);
            ClientSize = new Size(520, 674);
            Controls.Add(pbCrown);
            Controls.Add(pbLabel);
            Controls.Add(pbTop1);
            Controls.Add(pbTop2);
            Controls.Add(pbTop3);
            Controls.Add(pbRank2);
            Controls.Add(pbRank3);
            Controls.Add(lblTop1Name);
            Controls.Add(lblTop1Level);
            Controls.Add(lblTop1Score);
            Controls.Add(lblTop2Name);
            Controls.Add(lblTop2Level);
            Controls.Add(lblTop2Score);
            Controls.Add(lblTop3Name);
            Controls.Add(lblTop3Level);
            Controls.Add(lblTop3Score);
            Controls.Add(dgv);
            Controls.Add(btnClose);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "LeaderBoardForm";
            StartPosition = FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)pbLabel).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbCrown).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbTop1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbTop2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbTop3).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbRank2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbRank3).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgv).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox pbLabel;
        private System.Windows.Forms.PictureBox pbCrown;
        private System.Windows.Forms.PictureBox pbTop1;
        private System.Windows.Forms.PictureBox pbTop2;
        private System.Windows.Forms.PictureBox pbRank2;
        private System.Windows.Forms.PictureBox pbRank3;
        private System.Windows.Forms.PictureBox pbTop3;
        private System.Windows.Forms.Label lblTop1Name;
        private System.Windows.Forms.Label lblTop1Level;
        private System.Windows.Forms.Label lblTop1Score;
        private System.Windows.Forms.Label lblTop2Name;
        private System.Windows.Forms.Label lblTop2Level;
        private System.Windows.Forms.Label lblTop2Score;
        private System.Windows.Forms.Label lblTop3Name;
        private System.Windows.Forms.Label lblTop3Level;
        private System.Windows.Forms.Label lblTop3Score;
        private System.Windows.Forms.DataGridView dgv;
        private Btn_Pixel btnClose;
        private DataGridViewTextBoxColumn colRank;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLevel;
        private DataGridViewTextBoxColumn colPlayer;
        private DataGridViewTextBoxColumn colScore;
    }
}
