using System;
using System.Drawing;
using System.Windows.Forms;

namespace DoAn_NT106
{ 
    public class GameProgressBar : ProgressBar
    {
        public Color BorderColor { get; set; } = Color.Gray;
        public int BorderWidth { get; set; } = 2;
        public Color CustomForeColor { get; set; }

        public GameProgressBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            Height = 20;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = ClientRectangle;
            Graphics g = e.Graphics;

            // Vẽ background
            using (SolidBrush bgBrush = new SolidBrush(BackColor))
                g.FillRectangle(bgBrush, rect);

            // Vẽ thanh tiến trình
            if (Value > 0)
            {
                Rectangle progressRect = new Rectangle(
                    rect.X, rect.Y,
                    (int)(rect.Width * ((double)Value / Maximum)),
                    rect.Height
                );

                using (SolidBrush progressBrush = new SolidBrush(CustomForeColor))
                    g.FillRectangle(progressBrush, progressRect);
            }

            // Vẽ border
            using (Pen borderPen = new Pen(BorderColor, BorderWidth))
                g.DrawRectangle(borderPen,
                    rect.X, rect.Y,
                    rect.Width - BorderWidth,
                    rect.Height - BorderWidth);

            // Vẽ text (giá trị %)
            string text = $"{Value}/{Maximum}";
            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            TextRenderer.DrawText(g, text, Font, rect, ForeColor, flags);
        }
    }
}