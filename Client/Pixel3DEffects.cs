using System;
using System.Drawing;
using System.Windows.Forms;

namespace PixelGameLobby.UIEffects
{
    public static class Pixel3DEffects
    {
        // Màu sắc mặc định theo theme gỗ pixel
        public static class Colors
        {
            public static Color PrimaryBrown = Color.FromArgb(160, 82, 45);
            public static Color DarkBrown = Color.FromArgb(101, 67, 51);
            public static Color DarkerBrown = Color.FromArgb(74, 50, 25);
            public static Color Gold = Color.FromArgb(255, 215, 0);
            public static Color DarkGold = Color.FromArgb(139, 69, 19);
            public static Color LightBrown = Color.FromArgb(222, 184, 135);
            public static Color HoverBrown = Color.FromArgb(120, 60, 30);
            public static Color ReadyGreen = Color.FromArgb(100, 200, 100);
            public static Color NotReadyRed = Color.FromArgb(255, 0, 0);
        }

        public static void ApplyPanel3DEffect(PaintEventArgs e, Control control, Color? baseColor = null)
        {
            Color color = baseColor ?? control.BackColor;

            // Hiệu ứng nổi 3D với border đa lớp
            using (Pen shadowPen = new Pen(Color.FromArgb(80, 0, 0, 0), 3)) // Shadow
            using (Pen darkPen = new Pen(Color.FromArgb(150, Colors.DarkerBrown), 2)) // Border tối
            using (Pen lightPen = new Pen(Color.FromArgb(100, Colors.LightBrown), 1)) // Border sáng
            {
                // Vẽ shadow (hơi lệch)
                e.Graphics.DrawRectangle(shadowPen, 2, 2, control.Width - 1, control.Height - 1);

                // Border chính
                e.Graphics.DrawRectangle(darkPen, 0, 0, control.Width - 1, control.Height - 1);

                // Highlight edges tạo hiệu ứng nổi
                e.Graphics.DrawLine(lightPen, 1, 1, control.Width - 2, 1); // Top
                e.Graphics.DrawLine(lightPen, 1, 1, 1, control.Height - 2); // Left
            }
        }

        public static void ApplyButton3DEffect(PaintEventArgs e, Button button, bool isPressed = false)
        {
            Color baseColor = button.BackColor;
            int offset = isPressed ? 1 : 0;

            // Hiệu ứng button 3D với bevel
            using (SolidBrush backgroundBrush = new SolidBrush(baseColor))
            using (Pen darkEdge = new Pen(Color.FromArgb(150, Colors.DarkerBrown), 2))
            using (Pen lightEdge = new Pen(Color.FromArgb(150, Colors.LightBrown), 2))
            {
                // Fill background
                e.Graphics.FillRectangle(backgroundBrush,
                    offset, offset, button.Width - 1, button.Height - 1);

                // Vẽ edges cho hiệu ứng 3D
                if (!isPressed)
                {
                    // Top và left edges sáng (nổi lên)
                    e.Graphics.DrawLine(lightEdge, 0, 0, button.Width - 1, 0);
                    e.Graphics.DrawLine(lightEdge, 0, 0, 0, button.Height - 1);

                    // Bottom và right edges tối (chìm xuống)
                    e.Graphics.DrawLine(darkEdge, 0, button.Height - 1, button.Width - 1, button.Height - 1);
                    e.Graphics.DrawLine(darkEdge, button.Width - 1, 0, button.Width - 1, button.Height - 1);
                }
                else
                {
                    // Khi pressed, đảo ngược hiệu ứng
                    e.Graphics.DrawLine(darkEdge, 0, 0, button.Width - 1, 0);
                    e.Graphics.DrawLine(darkEdge, 0, 0, 0, button.Height - 1);

                    e.Graphics.DrawLine(lightEdge, 0, button.Height - 1, button.Width - 1, button.Height - 1);
                    e.Graphics.DrawLine(lightEdge, button.Width - 1, 0, button.Width - 1, button.Height - 1);
                }

                // Vẽ text
                TextRenderer.DrawText(e.Graphics, button.Text, button.Font,
                    new Rectangle(offset, offset, button.Width, button.Height),
                    button.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        public static void ApplyTextBox3DEffect(PaintEventArgs e, TextBox textBox)
        {
            // Hiệu ứng textbox 3D
            using (Pen insetDark = new Pen(Color.FromArgb(150, Colors.DarkerBrown), 2))
            using (Pen insetLight = new Pen(Color.FromArgb(100, Colors.LightBrown), 1))
            {
                // Border chìm
                e.Graphics.DrawRectangle(insetDark, 0, 0, textBox.Width - 1, textBox.Height - 1);

                // Inner highlight
                e.Graphics.DrawRectangle(insetLight, 1, 1, textBox.Width - 3, textBox.Height - 3);
            }
        }

        public static void ApplyLabel3DEffect(PaintEventArgs e, Label label, bool isTitle = false)
        {
            if (isTitle)
            {
                // Hiệu ứng title nổi
                using (Pen goldBorder = new Pen(Colors.Gold, 2))
                using (Pen innerShadow = new Pen(Color.FromArgb(100, Colors.DarkerBrown), 1))
                {
                    // Outer gold border
                    e.Graphics.DrawRectangle(goldBorder, 0, 0, label.Width - 1, label.Height - 1);

                    // Inner shadow
                    e.Graphics.DrawRectangle(innerShadow, 2, 2, label.Width - 5, label.Height - 5);
                }
            }
            else
            {
                // Hiệu ứng label đơn giản
                using (Pen subtleBorder = new Pen(Color.FromArgb(80, Colors.DarkerBrown), 1))
                {
                    e.Graphics.DrawRectangle(subtleBorder, 0, 0, label.Width - 1, label.Height - 1);
                }
            }
        }

        public static void ApplyRaisedEffect(Control control, bool raise = true)
        {
            if (raise)
            {
                // Hiệu ứng nổi lên
                control.Location = new Point(control.Location.X - 1, control.Location.Y - 1);
            }
            else
            {
                // Trở về vị trí ban đầu
                control.Location = new Point(control.Location.X + 1, control.Location.Y + 1);
            }
        }

        public static Color GetHoverColor(Color baseColor)
        {
            // Làm sáng màu khi hover
            return Color.FromArgb(
                Math.Min(baseColor.R + 30, 255),
                Math.Min(baseColor.G + 30, 255),
                Math.Min(baseColor.B + 30, 255)
            );
        }

        public static Color GetPressedColor(Color baseColor)
        {
            // Làm tối màu khi pressed
            return Color.FromArgb(
                Math.Max(baseColor.R - 30, 0),
                Math.Max(baseColor.G - 30, 0),
                Math.Max(baseColor.B - 30, 0)
            );
        }
    }

    // Custom Controls với hiệu ứng 3D tích hợp sẵn
    public class Pixel3DPanel : Panel
    {
        public bool Enable3DEffect { get; set; } = true;

        public Pixel3DPanel()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Enable3DEffect)
            {
                Pixel3DEffects.ApplyPanel3DEffect(e, this);
            }
        }
    }

    public class Pixel3DButton : Button
    {
        private bool isMouseDown = false;
        private bool isMouseOver = false;

        public Pixel3DButton()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Pixel3DEffects.ApplyButton3DEffect(e, this, isMouseDown);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            isMouseOver = true;
            this.BackColor = Pixel3DEffects.GetHoverColor(this.BackColor);
            base.OnMouseEnter(e);
            this.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isMouseOver = false;
            // Reset màu (cần lưu màu gốc trong thực tế)
            base.OnMouseLeave(e);
            this.Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            isMouseDown = true;
            this.BackColor = Pixel3DEffects.GetPressedColor(this.BackColor);
            base.OnMouseDown(e);
            this.Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            isMouseDown = false;
            this.BackColor = isMouseOver ?
                Pixel3DEffects.GetHoverColor(this.BackColor) :
                this.BackColor; // Màu gốc
            base.OnMouseUp(e);
            this.Invalidate();
        }
    }

    public class Pixel3DTextBox : TextBox
    {
        public Pixel3DTextBox()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            this.BorderStyle = BorderStyle.None;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Pixel3DEffects.ApplyTextBox3DEffect(e, this);
        }
    }

    public class Pixel3DLabel : Label
    {
        public bool IsTitle { get; set; } = false;

        public Pixel3DLabel()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Pixel3DEffects.ApplyLabel3DEffect(e, this, IsTitle);
        }
    }
}