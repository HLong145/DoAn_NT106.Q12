using System.Collections.Generic;
using System.Drawing;

namespace DoAn_NT106
{
    partial class MapSelectForm
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
            lblTitle = new Label();
            cmbMaps = new ComboBox();
            pbPreview = new PictureBox();
            btnOk = new Btn_Pixel();
            btnCancel = new Btn_Pixel();
            pictureBox5 = new PictureBox();
            mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbPreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).BeginInit();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.BackColor = Color.FromArgb(101, 67, 51);
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            mainPanel.Controls.Add(pictureBox5);
            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(cmbMaps);
            mainPanel.Controls.Add(pbPreview);
            mainPanel.Controls.Add(btnOk);
            mainPanel.Controls.Add(btnCancel);
            mainPanel.Location = new Point(10, 10);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(580, 480);
            mainPanel.TabIndex = 0;
            mainPanel.Paint += Panel_Paint;
            // 
            // lblTitle
            // 
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Courier New", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(255, 215, 0);
            lblTitle.Location = new Point(10, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(560, 40);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "SELECT BATTLEGROUND";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // cmbMaps
            // 
            cmbMaps.BackColor = Color.FromArgb(80, 60, 40);
            cmbMaps.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMaps.Font = new Font("Courier New", 11F, FontStyle.Bold);
            cmbMaps.ForeColor = Color.Gold;
            cmbMaps.Location = new Point(20, 70);
            cmbMaps.Name = "cmbMaps";
            cmbMaps.Size = new Size(540, 29);
            cmbMaps.TabIndex = 1;
            cmbMaps.SelectedIndexChanged += CmbMaps_SelectedIndexChanged;
            // 
            // pbPreview
            // 
            pbPreview.BackColor = Color.FromArgb(60, 40, 25);
            pbPreview.BorderStyle = BorderStyle.FixedSingle;
            pbPreview.Location = new Point(56, 105);
            pbPreview.Name = "pbPreview";
            pbPreview.Size = new Size(460, 280);
            pbPreview.SizeMode = PictureBoxSizeMode.Zoom;
            pbPreview.TabIndex = 2;
            pbPreview.TabStop = false;
            // 
            // btnOk
            // 
            btnOk.BtnColor = Color.FromArgb(34, 139, 34);
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btnOk.ForeColor = Color.Gold;
            btnOk.Location = new Point(56, 415);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(220, 45);
            btnOk.TabIndex = 3;
            btnOk.Text = "✓ SELECT";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += BtnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.BtnColor = Color.FromArgb(220, 20, 60);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btnCancel.ForeColor = Color.Gold;
            btnCancel.Location = new Point(296, 415);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(220, 45);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "✕ CANCEL";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // pictureBox5
            // 
            pictureBox5.BackColor = Color.Transparent;
            pictureBox5.Image = Properties.Resources.moon;
            pictureBox5.Location = new Point(469, 3);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new Size(91, 62);
            pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox5.TabIndex = 5;
            pictureBox5.TabStop = false;
            // 
            // MapSelectForm
            // 
            AutoScaleDimensions = new SizeF(12F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(160, 82, 45);
            ClientSize = new Size(600, 500);
            Controls.Add(mainPanel);
            Font = new Font("Courier New", 12F, FontStyle.Bold);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MapSelectForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "⚔️ Choose Battleground ⚔️";
            mainPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pbPreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).EndInit();
            ResumeLayout(false);
        }

        private Pnl_Pixel mainPanel;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ComboBox cmbMaps;
        private System.Windows.Forms.PictureBox pbPreview;
        private Btn_Pixel btnOk;
        private Btn_Pixel btnCancel;
        internal Dictionary<string, Image> mapImages = new Dictionary<string, Image>();

        private void Panel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            var panel = sender as System.Windows.Forms.Panel;
            if (panel != null)
            {
                System.Windows.Forms.ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle,
                    System.Drawing.Color.FromArgb(74, 50, 25), 3, System.Windows.Forms.ButtonBorderStyle.Solid,
                    System.Drawing.Color.FromArgb(74, 50, 25), 3, System.Windows.Forms.ButtonBorderStyle.Solid,
                    System.Drawing.Color.FromArgb(74, 50, 25), 3, System.Windows.Forms.ButtonBorderStyle.Solid,
                    System.Drawing.Color.FromArgb(74, 50, 25), 3, System.Windows.Forms.ButtonBorderStyle.Solid);
            }
        }
        private PictureBox pictureBox5;
    }
}
