namespace DoAn_NT106.Client
{
    partial class TinhXP
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Main panels
        private Pnl_Pixel pnl_Main;
        private Pnl_Pixel pnl_Title;
        private System.Windows.Forms.Panel pnl_XPBarContainer;
        private System.Windows.Forms.Panel pnl_XPBarFill;
        private System.Windows.Forms.Panel pnl_XPDetails;
        private System.Windows.Forms.Panel pnl_PlayerInfo;

        // Labels - Title
        private System.Windows.Forms.Label lbl_Title;
        private System.Windows.Forms.Label lbl_Subtitle;

        // Labels - Player Info
        private System.Windows.Forms.Label lbl_PlayerTitle;
        private System.Windows.Forms.Label lbl_PlayerValue;
        private System.Windows.Forms.Label lbl_ResultTitle;
        private System.Windows.Forms.Label lbl_ResultValue;
        private System.Windows.Forms.Label lbl_TimeTitle;
        private System.Windows.Forms.Label lbl_TimeValue;

        // Labels - XP Section
        private System.Windows.Forms.Label lbl_XPEarned;
        private System.Windows.Forms.Label lbl_XPEarnedValue;
        private System.Windows.Forms.Label lbl_XPProgress;
        private System.Windows.Forms.Label lbl_XPProgressValue;
        private System.Windows.Forms.Label lbl_XPPercent;
        private System.Windows.Forms.Label lbl_XPBefore;
        private System.Windows.Forms.Label lbl_XPGained;
        private System.Windows.Forms.Label lbl_XPAfter;
        private System.Windows.Forms.Label lbl_XPDetailsTitle;

        // Buttons
        private Btn_Pixel btn_Continue;
        private System.Windows.Forms.PictureBox pic_Cloud4;
        private System.Windows.Forms.PictureBox pic_TitleCloud1;

        // Timers
        private System.Windows.Forms.Timer timer_XPAnimation;
        private System.Windows.Forms.Timer timer_FloatingClouds;

        // Divider panels
        private System.Windows.Forms.Panel pnl_Divider1;
        private System.Windows.Forms.Panel pnl_Divider2;

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
            if (disposing)
            {
                timer_XPAnimation?.Stop();
                timer_XPAnimation?.Dispose();
                timer_FloatingClouds?.Stop();
                timer_FloatingClouds?.Dispose();
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
            components = new System.ComponentModel.Container();
            timer_XPAnimation = new System.Windows.Forms.Timer(components);
            timer_FloatingClouds = new System.Windows.Forms.Timer(components);
            pnl_Main = new Pnl_Pixel();
            btn_ViewStats = new Btn_Pixel();
            pictureBox3 = new PictureBox();
            pictureBox2 = new PictureBox();
            pnl_Title = new Pnl_Pixel();
            pictureBox1 = new PictureBox();
            pic_TitleCloud1 = new PictureBox();
            lbl_Title = new Label();
            lbl_Subtitle = new Label();
            pnl_PlayerInfo = new Panel();
            lbl_PlayerTitle = new Label();
            lbl_PlayerValue = new Label();
            lbl_ResultTitle = new Label();
            lbl_ResultValue = new Label();
            lbl_TimeTitle = new Label();
            lbl_TimeValue = new Label();
            pnl_Divider1 = new Panel();
            lbl_XPEarned = new Label();
            lbl_XPEarnedValue = new Label();
            lbl_XPProgress = new Label();
            lbl_XPProgressValue = new Label();
            pnl_XPBarContainer = new Panel();
            pnl_XPBarFill = new Panel();
            lbl_XPPercent = new Label();
            lbl_XPBefore = new Label();
            lbl_XPGained = new Label();
            lbl_XPAfter = new Label();
            pnl_Divider2 = new Panel();
            lbl_XPDetailsTitle = new Label();
            pnl_XPDetails = new Panel();
            btn_Continue = new Btn_Pixel();
            pic_Cloud4 = new PictureBox();
            pnl_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            pnl_Title.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pic_TitleCloud1).BeginInit();
            pnl_PlayerInfo.SuspendLayout();
            pnl_XPBarContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pic_Cloud4).BeginInit();
            SuspendLayout();
            // 
            // timer_XPAnimation
            // 
            timer_XPAnimation.Interval = 20;
            timer_XPAnimation.Tick += Timer_XPAnimation_Tick;
            // 
            // timer_FloatingClouds
            // 
            timer_FloatingClouds.Interval = 50;
            timer_FloatingClouds.Tick += Timer_FloatingClouds_Tick;
            // 
            // pnl_Main
            // 
            pnl_Main.BackColor = Color.FromArgb(210, 105, 30);
            pnl_Main.Controls.Add(btn_ViewStats);
            pnl_Main.Controls.Add(pictureBox3);
            pnl_Main.Controls.Add(pictureBox2);
            pnl_Main.Controls.Add(pnl_Title);
            pnl_Main.Controls.Add(pnl_PlayerInfo);
            pnl_Main.Controls.Add(pnl_Divider1);
            pnl_Main.Controls.Add(lbl_XPEarned);
            pnl_Main.Controls.Add(lbl_XPEarnedValue);
            pnl_Main.Controls.Add(lbl_XPProgress);
            pnl_Main.Controls.Add(lbl_XPProgressValue);
            pnl_Main.Controls.Add(pnl_XPBarContainer);
            pnl_Main.Controls.Add(lbl_XPBefore);
            pnl_Main.Controls.Add(lbl_XPGained);
            pnl_Main.Controls.Add(lbl_XPAfter);
            pnl_Main.Controls.Add(pnl_Divider2);
            pnl_Main.Controls.Add(lbl_XPDetailsTitle);
            pnl_Main.Controls.Add(pnl_XPDetails);
            pnl_Main.Controls.Add(btn_Continue);
            pnl_Main.Location = new Point(30, 20);
            pnl_Main.Name = "pnl_Main";
            pnl_Main.Size = new Size(490, 779);
            pnl_Main.TabIndex = 0;
            pnl_Main.Paint += pnl_Main_Paint;
            // 
            // btn_ViewStats
            // 
            btn_ViewStats.BtnColor = Color.FromArgb(139, 69, 19);
            btn_ViewStats.FlatStyle = FlatStyle.Flat;
            btn_ViewStats.Font = new Font("Courier New", 8F, FontStyle.Bold);
            btn_ViewStats.ForeColor = Color.White;
            btn_ViewStats.Location = new Point(163, 709);
            btn_ViewStats.Name = "btn_ViewStats";
            btn_ViewStats.Size = new Size(170, 40);
            btn_ViewStats.TabIndex = 17;
            btn_ViewStats.Text = "VIEW STATS";
            btn_ViewStats.Click += btn_ViewStats_Click;
            // 
            // pictureBox3
            // 
            pictureBox3.BackColor = Color.Transparent;
            pictureBox3.Image = Properties.Resources.mây;
            pictureBox3.Location = new Point(350, 705);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(137, 65);
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.TabIndex = 16;
            pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Image = Properties.Resources.mây;
            pictureBox2.Location = new Point(-2, 709);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(137, 65);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 15;
            pictureBox2.TabStop = false;
            // 
            // pnl_Title
            // 
            pnl_Title.BackColor = Color.FromArgb(210, 105, 30);
            pnl_Title.Controls.Add(pictureBox1);
            pnl_Title.Controls.Add(pic_TitleCloud1);
            pnl_Title.Controls.Add(lbl_Title);
            pnl_Title.Controls.Add(lbl_Subtitle);
            pnl_Title.Location = new Point(15, 15);
            pnl_Title.Name = "pnl_Title";
            pnl_Title.Size = new Size(460, 90);
            pnl_Title.TabIndex = 0;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = Properties.Resources.mây;
            pictureBox1.Location = new Point(353, 46);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(110, 43);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            // 
            // pic_TitleCloud1
            // 
            pic_TitleCloud1.BackColor = Color.Transparent;
            pic_TitleCloud1.Image = Properties.Resources.mây;
            pic_TitleCloud1.Location = new Point(-17, 46);
            pic_TitleCloud1.Name = "pic_TitleCloud1";
            pic_TitleCloud1.Size = new Size(110, 43);
            pic_TitleCloud1.SizeMode = PictureBoxSizeMode.Zoom;
            pic_TitleCloud1.TabIndex = 2;
            pic_TitleCloud1.TabStop = false;
            // 
            // lbl_Title
            // 
            lbl_Title.BackColor = Color.Transparent;
            lbl_Title.Font = new Font("Courier New", 14F, FontStyle.Bold);
            lbl_Title.ForeColor = Color.Gold;
            lbl_Title.Location = new Point(-3, 11);
            lbl_Title.Name = "lbl_Title";
            lbl_Title.Size = new Size(460, 28);
            lbl_Title.TabIndex = 0;
            lbl_Title.Text = "⚔️ PLAYER STATUS ⚔️";
            lbl_Title.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbl_Subtitle
            // 
            lbl_Subtitle.BackColor = Color.Transparent;
            lbl_Subtitle.Font = new Font("Courier New", 7F, FontStyle.Bold);
            lbl_Subtitle.ForeColor = Color.White;
            lbl_Subtitle.Location = new Point(0, 46);
            lbl_Subtitle.Name = "lbl_Subtitle";
            lbl_Subtitle.Size = new Size(460, 18);
            lbl_Subtitle.TabIndex = 1;
            lbl_Subtitle.Text = "POST-MATCH SUMMARY";
            lbl_Subtitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnl_PlayerInfo
            // 
            pnl_PlayerInfo.BackColor = Color.Transparent;
            pnl_PlayerInfo.Controls.Add(lbl_PlayerTitle);
            pnl_PlayerInfo.Controls.Add(lbl_PlayerValue);
            pnl_PlayerInfo.Controls.Add(lbl_ResultTitle);
            pnl_PlayerInfo.Controls.Add(lbl_ResultValue);
            pnl_PlayerInfo.Controls.Add(lbl_TimeTitle);
            pnl_PlayerInfo.Controls.Add(lbl_TimeValue);
            pnl_PlayerInfo.Location = new Point(25, 120);
            pnl_PlayerInfo.Name = "pnl_PlayerInfo";
            pnl_PlayerInfo.Size = new Size(440, 85);
            pnl_PlayerInfo.TabIndex = 1;
            // 
            // lbl_PlayerTitle
            // 
            lbl_PlayerTitle.BackColor = Color.Transparent;
            lbl_PlayerTitle.Font = new Font("Courier New", 9F, FontStyle.Bold);
            lbl_PlayerTitle.ForeColor = Color.Gold;
            lbl_PlayerTitle.Location = new Point(0, 0);
            lbl_PlayerTitle.Name = "lbl_PlayerTitle";
            lbl_PlayerTitle.Size = new Size(180, 23);
            lbl_PlayerTitle.TabIndex = 0;
            lbl_PlayerTitle.Text = "PLAYER:";
            // 
            // lbl_PlayerValue
            // 
            lbl_PlayerValue.BackColor = Color.Transparent;
            lbl_PlayerValue.Font = new Font("Courier New", 9F, FontStyle.Bold);
            lbl_PlayerValue.ForeColor = Color.White;
            lbl_PlayerValue.Location = new Point(180, 0);
            lbl_PlayerValue.Name = "lbl_PlayerValue";
            lbl_PlayerValue.Size = new Size(260, 23);
            lbl_PlayerValue.TabIndex = 1;
            lbl_PlayerValue.Text = "linhq";
            lbl_PlayerValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lbl_ResultTitle
            // 
            lbl_ResultTitle.BackColor = Color.Transparent;
            lbl_ResultTitle.Font = new Font("Courier New", 9F, FontStyle.Bold);
            lbl_ResultTitle.ForeColor = Color.Gold;
            lbl_ResultTitle.Location = new Point(0, 28);
            lbl_ResultTitle.Name = "lbl_ResultTitle";
            lbl_ResultTitle.Size = new Size(180, 23);
            lbl_ResultTitle.TabIndex = 2;
            lbl_ResultTitle.Text = "RESULT:";
            // 
            // lbl_ResultValue
            // 
            lbl_ResultValue.BackColor = Color.Transparent;
            lbl_ResultValue.Font = new Font("Courier New", 9F, FontStyle.Bold);
            lbl_ResultValue.ForeColor = Color.LimeGreen;
            lbl_ResultValue.Location = new Point(180, 28);
            lbl_ResultValue.Name = "lbl_ResultValue";
            lbl_ResultValue.Size = new Size(260, 23);
            lbl_ResultValue.TabIndex = 3;
            lbl_ResultValue.Text = "WIN";
            lbl_ResultValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lbl_TimeTitle
            // 
            lbl_TimeTitle.BackColor = Color.Transparent;
            lbl_TimeTitle.Font = new Font("Courier New", 9F, FontStyle.Bold);
            lbl_TimeTitle.ForeColor = Color.Gold;
            lbl_TimeTitle.Location = new Point(0, 56);
            lbl_TimeTitle.Name = "lbl_TimeTitle";
            lbl_TimeTitle.Size = new Size(180, 23);
            lbl_TimeTitle.TabIndex = 4;
            lbl_TimeTitle.Text = "MATCH TIME:";
            // 
            // lbl_TimeValue
            // 
            lbl_TimeValue.BackColor = Color.Transparent;
            lbl_TimeValue.Font = new Font("Courier New", 9F, FontStyle.Bold);
            lbl_TimeValue.ForeColor = Color.White;
            lbl_TimeValue.Location = new Point(180, 56);
            lbl_TimeValue.Name = "lbl_TimeValue";
            lbl_TimeValue.Size = new Size(260, 23);
            lbl_TimeValue.TabIndex = 5;
            lbl_TimeValue.Text = "01:28";
            lbl_TimeValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // pnl_Divider1
            // 
            pnl_Divider1.BackColor = Color.FromArgb(139, 69, 19);
            pnl_Divider1.Location = new Point(25, 215);
            pnl_Divider1.Name = "pnl_Divider1";
            pnl_Divider1.Size = new Size(440, 3);
            pnl_Divider1.TabIndex = 2;
            // 
            // lbl_XPEarned
            // 
            lbl_XPEarned.BackColor = Color.Transparent;
            lbl_XPEarned.Font = new Font("Courier New", 10F, FontStyle.Bold);
            lbl_XPEarned.ForeColor = Color.Gold;
            lbl_XPEarned.Location = new Point(25, 228);
            lbl_XPEarned.Name = "lbl_XPEarned";
            lbl_XPEarned.Size = new Size(440, 23);
            lbl_XPEarned.TabIndex = 3;
            lbl_XPEarned.Text = "XP EARNED THIS MATCH:";
            // 
            // lbl_XPEarnedValue
            // 
            lbl_XPEarnedValue.BackColor = Color.Transparent;
            lbl_XPEarnedValue.Font = new Font("Courier New", 16F, FontStyle.Bold);
            lbl_XPEarnedValue.ForeColor = Color.LimeGreen;
            lbl_XPEarnedValue.Location = new Point(25, 254);
            lbl_XPEarnedValue.Name = "lbl_XPEarnedValue";
            lbl_XPEarnedValue.Size = new Size(440, 32);
            lbl_XPEarnedValue.TabIndex = 4;
            lbl_XPEarnedValue.Text = "+284 XP";
            lbl_XPEarnedValue.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbl_XPProgress
            // 
            lbl_XPProgress.BackColor = Color.Transparent;
            lbl_XPProgress.Font = new Font("Courier New", 8F, FontStyle.Bold);
            lbl_XPProgress.ForeColor = Color.White;
            lbl_XPProgress.Location = new Point(25, 294);
            lbl_XPProgress.Name = "lbl_XPProgress";
            lbl_XPProgress.Size = new Size(225, 18);
            lbl_XPProgress.TabIndex = 5;
            lbl_XPProgress.Text = "Level 12 → Level 13";
            // 
            // lbl_XPProgressValue
            // 
            lbl_XPProgressValue.BackColor = Color.Transparent;
            lbl_XPProgressValue.Font = new Font("Courier New", 8F, FontStyle.Bold);
            lbl_XPProgressValue.ForeColor = Color.White;
            lbl_XPProgressValue.Location = new Point(252, 294);
            lbl_XPProgressValue.Name = "lbl_XPProgressValue";
            lbl_XPProgressValue.Size = new Size(213, 18);
            lbl_XPProgressValue.TabIndex = 6;
            lbl_XPProgressValue.Text = "1276 / 2000 XP";
            lbl_XPProgressValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // pnl_XPBarContainer
            // 
            pnl_XPBarContainer.BackColor = Color.FromArgb(42, 31, 26);
            pnl_XPBarContainer.BorderStyle = BorderStyle.FixedSingle;
            pnl_XPBarContainer.Controls.Add(pnl_XPBarFill);
            pnl_XPBarContainer.Controls.Add(lbl_XPPercent);
            pnl_XPBarContainer.Location = new Point(25, 317);
            pnl_XPBarContainer.Name = "pnl_XPBarContainer";
            pnl_XPBarContainer.Size = new Size(440, 36);
            pnl_XPBarContainer.TabIndex = 7;
            // 
            // pnl_XPBarFill
            // 
            pnl_XPBarFill.BackColor = Color.FromArgb(34, 139, 34);
            pnl_XPBarFill.Location = new Point(0, 0);
            pnl_XPBarFill.Name = "pnl_XPBarFill";
            pnl_XPBarFill.Size = new Size(0, 36);
            pnl_XPBarFill.TabIndex = 0;
            // 
            // lbl_XPPercent
            // 
            lbl_XPPercent.BackColor = Color.Transparent;
            lbl_XPPercent.Font = new Font("Courier New", 9F, FontStyle.Bold);
            lbl_XPPercent.ForeColor = Color.White;
            lbl_XPPercent.Location = new Point(0, 0);
            lbl_XPPercent.Name = "lbl_XPPercent";
            lbl_XPPercent.Size = new Size(439, 34);
            lbl_XPPercent.TabIndex = 1;
            lbl_XPPercent.Text = "0%";
            lbl_XPPercent.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbl_XPBefore
            // 
            lbl_XPBefore.BackColor = Color.FromArgb(42, 31, 26);
            lbl_XPBefore.Font = new Font("Courier New", 7.5F);
            lbl_XPBefore.ForeColor = Color.White;
            lbl_XPBefore.Location = new Point(25, 356);
            lbl_XPBefore.Name = "lbl_XPBefore";
            lbl_XPBefore.Padding = new Padding(4, 2, 4, 2);
            lbl_XPBefore.Size = new Size(440, 27);
            lbl_XPBefore.TabIndex = 8;
            lbl_XPBefore.Text = "XP Before Match:                              1276 XP";
            // 
            // lbl_XPGained
            // 
            lbl_XPGained.BackColor = Color.FromArgb(42, 31, 26);
            lbl_XPGained.Font = new Font("Courier New", 7.5F, FontStyle.Bold);
            lbl_XPGained.ForeColor = Color.LimeGreen;
            lbl_XPGained.Location = new Point(26, 383);
            lbl_XPGained.Name = "lbl_XPGained";
            lbl_XPGained.Padding = new Padding(4, 2, 4, 2);
            lbl_XPGained.Size = new Size(439, 22);
            lbl_XPGained.TabIndex = 9;
            lbl_XPGained.Text = "XP Gained:                                    +284 XP";
            // 
            // lbl_XPAfter
            // 
            lbl_XPAfter.BackColor = Color.FromArgb(42, 31, 26);
            lbl_XPAfter.Font = new Font("Courier New", 7.5F);
            lbl_XPAfter.ForeColor = Color.White;
            lbl_XPAfter.Location = new Point(26, 405);
            lbl_XPAfter.Name = "lbl_XPAfter";
            lbl_XPAfter.Padding = new Padding(4, 2, 4, 2);
            lbl_XPAfter.Size = new Size(439, 22);
            lbl_XPAfter.TabIndex = 10;
            lbl_XPAfter.Text = "XP After Match:                               1560 XP";
            // 
            // pnl_Divider2
            // 
            pnl_Divider2.BackColor = Color.FromArgb(139, 69, 19);
            pnl_Divider2.Location = new Point(25, 439);
            pnl_Divider2.Name = "pnl_Divider2";
            pnl_Divider2.Size = new Size(440, 3);
            pnl_Divider2.TabIndex = 11;
            // 
            // lbl_XPDetailsTitle
            // 
            lbl_XPDetailsTitle.BackColor = Color.Transparent;
            lbl_XPDetailsTitle.Font = new Font("Courier New", 10F, FontStyle.Bold);
            lbl_XPDetailsTitle.ForeColor = Color.Gold;
            lbl_XPDetailsTitle.Location = new Point(25, 453);
            lbl_XPDetailsTitle.Name = "lbl_XPDetailsTitle";
            lbl_XPDetailsTitle.Size = new Size(440, 23);
            lbl_XPDetailsTitle.TabIndex = 12;
            lbl_XPDetailsTitle.Text = "XP DETAILS:";
            // 
            // pnl_XPDetails
            // 
            pnl_XPDetails.AutoScroll = true;
            pnl_XPDetails.BackColor = Color.FromArgb(42, 31, 26);
            pnl_XPDetails.BorderStyle = BorderStyle.FixedSingle;
            pnl_XPDetails.Location = new Point(25, 481);
            pnl_XPDetails.Name = "pnl_XPDetails";
            pnl_XPDetails.Size = new Size(440, 160);
            pnl_XPDetails.TabIndex = 13;
            // 
            // btn_Continue
            // 
            btn_Continue.BtnColor = Color.FromArgb(34, 139, 34);
            btn_Continue.FlatStyle = FlatStyle.Flat;
            btn_Continue.Font = new Font("Courier New", 11F, FontStyle.Bold);
            btn_Continue.ForeColor = Color.White;
            btn_Continue.Location = new Point(25, 650);
            btn_Continue.Name = "btn_Continue";
            btn_Continue.Size = new Size(440, 45);
            btn_Continue.TabIndex = 14;
            btn_Continue.Text = "► CONTINUE ◄";
            // 
            // pic_Cloud4
            // 
            pic_Cloud4.BackColor = Color.Transparent;
            pic_Cloud4.Image = Properties.Resources.mayxanh;
            pic_Cloud4.Location = new Point(-58, 375);
            pic_Cloud4.Name = "pic_Cloud4";
            pic_Cloud4.Size = new Size(117, 40);
            pic_Cloud4.SizeMode = PictureBoxSizeMode.Zoom;
            pic_Cloud4.TabIndex = 20;
            pic_Cloud4.TabStop = false;
            // 
            // TinhXP
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(26, 20, 16);
            BackgroundImage = Properties.Resources.background2;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(551, 859);
            Controls.Add(pnl_Main);
            Controls.Add(pic_Cloud4);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "TinhXP";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Match Summary";
            pnl_Main.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            pnl_Title.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pic_TitleCloud1).EndInit();
            pnl_PlayerInfo.ResumeLayout(false);
            pnl_XPBarContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pic_Cloud4).EndInit();
            ResumeLayout(false);
        }

        #endregion

        // Add this method to the partial class TinhXP in your Designer file or (preferably) in the main code file (TinhXP.cs)
        private void Timer_XPAnimation_Tick(object sender, EventArgs e)
        {
            // TODO: Add your XP animation logic here.
        }

        // Add this method to the partial class TinhXP in your Designer file or (preferably) in the main code file (TinhXP.cs)
        private void Timer_FloatingClouds_Tick(object sender, EventArgs e)
        {
            // TODO: Add your floating clouds animation logic here.
        }

        // Add this event handler method to your partial class TinhXP
        // Add this method to your partial class TinhXP

        private PictureBox pictureBox2;
        private PictureBox pictureBox1;
        private PictureBox pictureBox3;
        private Btn_Pixel btn_ViewStats;
    }
}
