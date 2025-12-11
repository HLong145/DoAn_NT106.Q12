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
        protected internal System.Windows.Forms.Timer gameTimer;

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            gameTimer = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // gameTimer
            // 
            gameTimer.Interval = 50;
            // 
            // BattleForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 50);
            ClientSize = new Size(1000, 750);
            DoubleBuffered = true;
            // remove outer window border and close button
            FormBorderStyle = FormBorderStyle.None;
            ControlBox = false;
            Margin = new Padding(3, 4, 3, 4);
            Name = "BattleForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Street Fighter Battle";
            ResumeLayout(false);
        }

        #endregion
    }
}