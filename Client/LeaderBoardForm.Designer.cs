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
            this.components = new System.ComponentModel.Container();
            this.ClientSize = new System.Drawing.Size(520, 640);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.BackColor = System.Drawing.Color.FromArgb(101, 67, 51);

            this.lblTitle = new System.Windows.Forms.Label();
            this.dgv = new System.Windows.Forms.DataGridView();
            this.btnClose = new Btn_Pixel();

            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            // 
            // lblTitle
            // 
            this.lblTitle.Location = new System.Drawing.Point(20, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(480, 40);
            this.lblTitle.Text = "LEADERBOARD - TOP 20";
            this.lblTitle.Font = new System.Drawing.Font("Courier New", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.Gold;
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Controls.Add(this.lblTitle);

            // 
            // dgv
            // 
            this.dgv.Location = new System.Drawing.Point(20, 60);
            this.dgv.Name = "dgv";
            this.dgv.Size = new System.Drawing.Size(480, 520);
            this.dgv.ReadOnly = true;
            this.dgv.AllowUserToAddRows = false;
            this.dgv.AllowUserToDeleteRows = false;
            this.dgv.RowHeadersVisible = false;
            this.dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv.BackgroundColor = System.Drawing.Color.FromArgb(255, 224, 192);
            this.dgv.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgv.EnableHeadersVisualStyles = false;
            this.dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(64, 0, 0);
            this.dgv.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.Gold;
            this.dgv.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Courier New", 11F, System.Drawing.FontStyle.Bold);

            // columns
            var colRank = new System.Windows.Forms.DataGridViewTextBoxColumn();
            var colPlayer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            var colLevel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            var colXP = new System.Windows.Forms.DataGridViewTextBoxColumn();

            colRank.Name = "Rank";
            colRank.HeaderText = "#";
            colRank.Width = 40;

            colPlayer.Name = "Player";
            colPlayer.HeaderText = "PLAYER";
            colPlayer.Width = 240;

            colLevel.Name = "Level";
            colLevel.HeaderText = "LEVEL";
            colLevel.Width = 80;

            colXP.Name = "XP";
            colXP.HeaderText = "XP";
            colXP.Width = 100;

            this.dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colRank, colPlayer, colLevel, colXP });

            this.Controls.Add(this.dgv);

            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(180, 590);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(160, 40);
            this.btnClose.Text = "CLOSE";
            this.btnClose.BtnColor = System.Drawing.Color.FromArgb(178, 34, 34);
            this.btnClose.ForeColor = System.Drawing.Color.Gold;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.Controls.Add(this.btnClose);

            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.DataGridView dgv;
        private Btn_Pixel btnClose;
    }
}
