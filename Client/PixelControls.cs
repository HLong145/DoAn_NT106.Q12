using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DoAn_NT106
{
    // 🌳 Panel hiệu ứng pixel (nền gỗ)
    public class Pnl_Pixel : Panel
    {
        public Pnl_Pixel()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(210, 105, 30); // màu fallback
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            // Hiệu ứng gỗ - gradient dọc
            using (var brush = new LinearGradientBrush(
                ClientRectangle,
                Color.FromArgb(160, 82, 45),
                Color.FromArgb(139, 69, 19),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // Viền đen bên ngoài
            using (var penOuter = new Pen(Color.Black, 4))
            {
                g.DrawRectangle(penOuter, 0, 0, Width - 4, Height - 4);
            }

            // Viền sáng bên trong
            using (var penInner = new Pen(Color.FromArgb(100, 255, 255, 255), 2))
            {
                g.DrawRectangle(penInner, 4, 4, Width - 12, Height - 12);
            }
        }
    }

    // ✏️ TextBox kiểu pixel
    public class Tb_Pixel : TextBox
    {
        public Tb_Pixel()
        {
            BorderStyle = BorderStyle.None;
            Multiline = true;
            TextAlign = HorizontalAlignment.Left;
            Padding = new Padding(10);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // WM_PAINT = 0xF
            if (m.Msg == 0xF)
            {
                using (Graphics g = Graphics.FromHwnd(Handle))
                {
                    g.SmoothingMode = SmoothingMode.None;
                    using (var pen = new Pen(Color.Black, 3))
                    {
                        g.DrawRectangle(pen, 0, 0, Width - 3, Height - 3);
                    }
                }
            }
        }
    }

    // 🟩 Nút kiểu pixel
    public class Btn_Pixel : Button
    {
        public Color BtnColor { get; set; } = Color.FromArgb(34, 139, 34);

        public Btn_Pixel()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            ForeColor = Color.White;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            bool isHover = ClientRectangle.Contains(PointToClient(Cursor.Position));
            Color topColor = isHover ? ControlPaint.Light(BtnColor) : ControlPaint.Light(BtnColor, 0.2f);
            Color bottomColor = isHover ? ControlPaint.Dark(BtnColor) : ControlPaint.Dark(BtnColor);

            // Gradient màu nút
            using (var brush = new LinearGradientBrush(ClientRectangle, topColor, bottomColor, LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // Viền đen pixel
            using (var pen = new Pen(Color.Black, 4))
            {
                g.DrawRectangle(pen, 2, 2, Width - 8, Height - 8);
            }

            // Chữ nút
            using (var brush = new SolidBrush(ForeColor))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(Text, Font, brush, ClientRectangle, sf);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Invalidate(); // redraw hover effect
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Invalidate(); // redraw normal
        }
    }
}
    