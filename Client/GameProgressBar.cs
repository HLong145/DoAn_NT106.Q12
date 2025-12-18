using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DoAn_NT106.Client
{ 
    public class GameProgressBar : ProgressBar
    {
        public Color CustomForeColor { get; set; } = Color.FromArgb(50, 220, 50);  // Default green
        public Color BorderColor { get; set; } = Color.Black;
        public int BorderWidth { get; set; } = 3;

        public GameProgressBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Height = 24;
            DoubleBuffered = true;
        }

        // ? Override Value ?? t? ??ng Invalidate
        private int _value;
        public new int Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = Math.Max(Minimum, Math.Min(Maximum, value));
                    base.Value = _value;
                    this.Invalidate();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.None;
            Rectangle rect = ClientRectangle;
            Graphics g = e.Graphics;

            // ? V? background gradient (gi?ng Btn_Pixel - n?n g?)
            using (var bgBrush = new LinearGradientBrush(
                rect,
                Color.FromArgb(100, 80, 60),
                Color.FromArgb(70, 50, 30),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(bgBrush, rect);
            }

            // ? V? thanh ti?n trình v?i gradient
            if (Value > 0)
            {
                int progressWidth = (int)(rect.Width * ((double)Value / Maximum));
                Rectangle progressRect = new Rectangle(
                    rect.X + 3, 
                    rect.Y + 3,
                    Math.Max(0, progressWidth - 6),
                    rect.Height - 6
                );

                if (progressRect.Width > 0)
                {
                    // Gradient màu thanh ti?n trình
                    Color topColor = ControlPaint.Light(CustomForeColor, 0.3f);
                    Color bottomColor = ControlPaint.Dark(CustomForeColor, 0.2f);
                    
                    using (var progressBrush = new LinearGradientBrush(
                        progressRect,
                        topColor,
                        bottomColor,
                        LinearGradientMode.Vertical))
                    {
                        g.FillRectangle(progressBrush, progressRect);
                    }

                    // ? Vi?n sáng bên trong thanh
                    using (var penInner = new Pen(Color.FromArgb(150, 255, 255, 255), 1))
                    {
                        g.DrawRectangle(penInner, progressRect);
                    }
                }
            }

            // ? Vi?n ?en bên ngoài (gi?ng Btn_Pixel pixel style)
            using (var penOuter = new Pen(BorderColor, BorderWidth))
            {
                g.DrawRectangle(penOuter, 
                    1, 1,
                    rect.Width - BorderWidth - 1, 
                    rect.Height - BorderWidth - 1);
            }

            // ? Vi?n sáng bên trong
            using (var penInner = new Pen(Color.FromArgb(100, 255, 255, 255), 1))
            {
                g.DrawRectangle(penInner, 
                    BorderWidth - 1, 
                    BorderWidth - 1,
                    rect.Width - (BorderWidth * 2) + 1, 
                    rect.Height - (BorderWidth * 2) + 1);
            }

            // ? V? text (giá tr? %) - tr?ng, ??m
            string text = $"{Value}/{Maximum}";
            using (var brush = new SolidBrush(Color.White))
            using (var sf = new StringFormat 
            { 
                Alignment = StringAlignment.Center, 
                LineAlignment = StringAlignment.Center 
            })
            {
                g.DrawString(text, new Font(Font.FontFamily, Font.Size, FontStyle.Bold), brush, rect, sf);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Invalidate();
        }
    }
}
