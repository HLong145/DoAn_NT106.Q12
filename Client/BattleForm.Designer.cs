using System;
using System.Windows.Forms;
using System.Drawing;

namespace DoAn_NT106
{
    partial class BattleForm
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

        #region Windows Form Designer generated code

        // Controls declared here (single place)
        protected internal ComboBox cmbBackground;
        protected internal Button btnBack;
        protected internal System.Windows.Forms.Timer gameTimer;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cmbBackground = new System.Windows.Forms.ComboBox();
            this.btnBack = new System.Windows.Forms.Button();
            this.gameTimer = new System.Windows.Forms.Timer(this.components);

            // cmbBackground
            this.cmbBackground.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBackground.FormattingEnabled = true;
            this.cmbBackground.Location = new System.Drawing.Point(400, 550);
            this.cmbBackground.Name = "cmbBackground";
            this.cmbBackground.Size = new System.Drawing.Size(150, 24);
            this.cmbBackground.TabIndex = 0;

            // btnBack
            this.btnBack.Location = new System.Drawing.Point(570, 550);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(120, 30);
            this.btnBack.TabIndex = 1;
            this.btnBack.Text = "Back to Lobby";
            this.btnBack.UseVisualStyleBackColor = true;

            // gameTimer
            this.gameTimer.Interval = 50;

            // BattleForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 50);
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.cmbBackground);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "BattleForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Street Fighter Battle";
        }

        #endregion
    }
}