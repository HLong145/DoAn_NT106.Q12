namespace DoAn_NT106.Client
{
    partial class InstructionForm
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

        private void InitializeComponent()
        {
            mainPanel = new Pnl_Pixel();
            btnBack = new Btn_Pixel();
            tabControl = new TabControl();
            tabOffline = new TabPage();
            txtOffline = new RichTextBox();
            tabOnline = new TabPage();
            txtOnline = new RichTextBox();
            lblHeader = new Label();
            mainPanel.SuspendLayout();
            tabControl.SuspendLayout();
            tabOffline.SuspendLayout();
            tabOnline.SuspendLayout();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.BackColor = Color.FromArgb(255, 224, 192);
            mainPanel.Controls.Add(btnBack);
            mainPanel.Controls.Add(tabControl);
            mainPanel.Controls.Add(lblHeader);
            mainPanel.Location = new Point(8, 8);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(760, 520);
            mainPanel.TabIndex = 0;
            // 
            // btnBack
            // 
            btnBack.BtnColor = Color.FromArgb(178, 34, 34);
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btnBack.ForeColor = Color.Gold;
            btnBack.Location = new Point(300, 470);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(160, 40);
            btnBack.TabIndex = 2;
            btnBack.Text = "← BACK";
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Click += BtnBack_Click;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabOffline);
            tabControl.Controls.Add(tabOnline);
            tabControl.Location = new Point(20, 80);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(720, 380);
            tabControl.TabIndex = 1;
            // 
            // tabOffline
            // 
            tabOffline.Controls.Add(txtOffline);
            tabOffline.Location = new Point(4, 32);
            tabOffline.Name = "tabOffline";
            tabOffline.Padding = new Padding(3);
            tabOffline.Size = new Size(712, 344);
            tabOffline.TabIndex = 0;
            tabOffline.Text = "Offline Controls";
            tabOffline.UseVisualStyleBackColor = true;
            // 
            // txtOffline
            // 
            txtOffline.BackColor = Color.FromArgb(255, 224, 192);
            txtOffline.BorderStyle = BorderStyle.None;
            txtOffline.Dock = DockStyle.Fill;
            txtOffline.Font = new Font("Courier New", 11F);
            txtOffline.Location = new Point(3, 3);
            txtOffline.Name = "txtOffline";
            txtOffline.ReadOnly = true;
            txtOffline.Size = new Size(706, 338);
            txtOffline.TabIndex = 0;
            txtOffline.Text = "";
            // 
            // tabOnline
            // 
            tabOnline.Controls.Add(txtOnline);
            tabOnline.Location = new Point(4, 29);
            tabOnline.Name = "tabOnline";
            tabOnline.Padding = new Padding(3);
            tabOnline.Size = new Size(712, 347);
            tabOnline.TabIndex = 1;
            tabOnline.Text = "Online Controls";
            tabOnline.UseVisualStyleBackColor = true;
            // 
            // txtOnline
            // 
            txtOnline.BackColor = Color.FromArgb(255, 224, 192);
            txtOnline.BorderStyle = BorderStyle.None;
            txtOnline.Dock = DockStyle.Fill;
            txtOnline.Font = new Font("Courier New", 11F);
            txtOnline.Location = new Point(3, 3);
            txtOnline.Name = "txtOnline";
            txtOnline.ReadOnly = true;
            txtOnline.Size = new Size(706, 341);
            txtOnline.TabIndex = 0;
            txtOnline.Text = "";
            // 
            // lblHeader
            // 
            lblHeader.BackColor = Color.FromArgb(64, 0, 0);
            lblHeader.Font = new Font("Courier New", 20F, FontStyle.Bold);
            lblHeader.ForeColor = Color.Gold;
            lblHeader.Location = new Point(20, 18);
            lblHeader.Name = "lblHeader";
            lblHeader.Size = new Size(720, 48);
            lblHeader.TabIndex = 0;
            lblHeader.Text = "GAME CONTROLS - INSTRUCTIONS";
            lblHeader.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // InstructionForm
            // 
            AutoScaleDimensions = new SizeF(12F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(776, 536);
            ControlBox = false;
            Controls.Add(mainPanel);
            Font = new Font("Courier New", 12F, FontStyle.Bold);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InstructionForm";
            StartPosition = FormStartPosition.CenterParent;
            mainPanel.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            tabOffline.ResumeLayout(false);
            tabOnline.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Pnl_Pixel mainPanel;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabOffline;
        private System.Windows.Forms.TabPage tabOnline;
        private System.Windows.Forms.RichTextBox txtOffline;
        private System.Windows.Forms.RichTextBox txtOnline;
        private Btn_Pixel btnBack;
    }
}
