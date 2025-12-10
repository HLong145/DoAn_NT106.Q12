using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace DoAn_NT106
{
    public partial class AvatarSelectorForm : Form
    {
        public int SelectedIndex { get; private set; } = -1;
        private Image[] avatars;
        private int currentSelection = -1;
        private string[] avatarNames = { "Scarlet Hunter", "Bringer of Death", "Elite Warrior", "Beserk Goatman" };

        // UI Components
        private Panel mainPanel;
        private Label lblTitle;
        private FlowLayoutPanel avatarListPanel;
        private Panel previewPanel;
        private PictureBox pbPreview;
        private Label lblAvatarName;
        private Label lblPreviewInfo;
        private Btn_Pixel btnConfirm;
        private Btn_Pixel btnCancel;

        // Colors
        private Color primaryBrown = Color.FromArgb(160, 82, 45);
        private Color darkBrown = Color.FromArgb(101, 67, 51);
        private Color accentBrown = Color.FromArgb(139, 69, 19);
        private Color goldColor = Color.FromArgb(255, 215, 0);
        private Color lightGold = Color.FromArgb(255, 228, 125);

        public AvatarSelectorForm(Image[] gameAvatars)
        {
            // Always use the four specific avatar resources
            avatars = new Image[]
            {
                Properties.Resources.avt_knightgirl,
                Properties.Resources.avt_bringer,
                Properties.Resources.avt_warrior,
                Properties.Resources.avt_goatman
            };

            InitializeComponent();
            InitializeUI();
            PopulateAvatars();

            // Select first avatar by default
            if (avatars.Length > 0)
            {
                SelectAvatar(0);
            }
        }

        private void InitializeUI()
        {
            this.Text = $"Choose Your Avatar";
            this.Size = new Size(1100, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = primaryBrown;
            this.DoubleBuffered = true;

            // Main Panel with gradient-like appearance
            mainPanel = new Panel
            {
                Location = new Point(15, 15),
                Size = new Size(1070, 770),
                BackColor = darkBrown
            };
            mainPanel.Paint += (s, e) =>
            {
                // Draw subtle gradient border
                using (var pen = new Pen(goldColor, 2))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, mainPanel.Width - 1, mainPanel.Height - 1);
                }
            };
            this.Controls.Add(mainPanel);

            // Title
            lblTitle = new Label
            {
                Text = "🎭 CHOOSE YOUR AVATAR 🎭",
                Font = new Font("Courier New", 22, FontStyle.Bold),
                ForeColor = goldColor,
                BackColor = Color.Transparent,
                Size = new Size(1030, 50),
                Location = new Point(20, 15),
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblTitle);

            // Divider line
            var divider = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(1030, 2),
                BackColor = goldColor
            };
            mainPanel.Controls.Add(divider);

            // Avatar List Panel (left side) - larger cards
            avatarListPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 85),
                Size = new Size(530, 600),
                BackColor = Color.FromArgb(70, 45, 25),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10)
            };
            mainPanel.Controls.Add(avatarListPanel);

            // Preview Panel (right side) - more spacious
            previewPanel = new Panel
            {
                Location = new Point(570, 85),
                Size = new Size(480, 600),
                BackColor = Color.FromArgb(85, 55, 35),
                BorderStyle = BorderStyle.None
            };
            previewPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(goldColor, 2))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, previewPanel.Width - 1, previewPanel.Height - 1);
                }
            };
            mainPanel.Controls.Add(previewPanel);

            // Preview Title
            var lblPreviewTitle = new Label
            {
                Text = "PREVIEW",
                Font = new Font("Courier New", 14, FontStyle.Bold),
                ForeColor = goldColor,
                BackColor = Color.Transparent,
                Size = new Size(460, 30),
                Location = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };
            previewPanel.Controls.Add(lblPreviewTitle);

            // Preview Image - larger
            pbPreview = new PictureBox
            {
                Location = new Point(65, 50),
                Size = new Size(350, 280),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(60, 40, 25),
                BorderStyle = BorderStyle.FixedSingle
            };
            previewPanel.Controls.Add(pbPreview);

            // Avatar Name
            lblAvatarName = new Label
            {
                Location = new Point(10, 345),
                Size = new Size(460, 35),
                Font = new Font("Courier New", 14, FontStyle.Bold),
                ForeColor = lightGold,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            previewPanel.Controls.Add(lblAvatarName);

            // Preview Info - details below name
            lblPreviewInfo = new Label
            {
                Location = new Point(10, 390),
                Size = new Size(460, 190),
                Font = new Font("Courier New", 9),
                ForeColor = Color.LightGoldenrodYellow,
                BackColor = Color.Transparent,
                Text = "Select an avatar to preview",
                TextAlign = ContentAlignment.TopCenter
            };
            previewPanel.Controls.Add(lblPreviewInfo);

            // Confirm Button
            btnConfirm = new Btn_Pixel
            {
                Text = "✓ CONFIRM",
                Location = new Point(570, 710),
                Size = new Size(200, 50),
                BtnColor = Color.FromArgb(34, 139, 34),
                Font = new Font("Courier New", 13, FontStyle.Bold),
                ForeColor = Color.White
            };
            btnConfirm.Click += BtnConfirm_Click;
            mainPanel.Controls.Add(btnConfirm);

            // Cancel Button
            btnCancel = new Btn_Pixel
            {
                Text = "← CANCEL",
                Location = new Point(850, 710),
                Size = new Size(200, 50),
                BtnColor = Color.FromArgb(178, 34, 34),
                Font = new Font("Courier New", 13, FontStyle.Bold),
                ForeColor = Color.White
            };
            btnCancel.Click += BtnCancel_Click;
            mainPanel.Controls.Add(btnCancel);
        }

        private void PopulateAvatars()
        {
            avatarListPanel.Controls.Clear();

            // Ensure all avatar thumbnails use the same display size
            Size thumbSize = new Size(120, 120);

            for (int i = 0; i < avatars.Length; i++)
            {
                var thumbImg = CreateThumbnail(avatars[i], thumbSize);
                var card = CreateAvatarCard(thumbImg, i);
                avatarListPanel.Controls.Add(card);
            }
        }

        // Create a centered thumbnail with exact target size
        private Image CreateThumbnail(Image src, Size target)
        {
            if (src == null) return null;
            try
            {
                var bmp = new Bitmap(target.Width, target.Height);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    float scale = Math.Min((float)target.Width / src.Width, (float)target.Height / src.Height);
                    int w = (int)(src.Width * scale);
                    int h = (int)(src.Height * scale);
                    int x = (target.Width - w) / 2;
                    int y = (target.Height - h) / 2;
                    g.DrawImage(src, new Rectangle(x, y, w, h));
                }
                return bmp;
            }
            catch { return src; }
        }

        private Panel CreateAvatarCard(Image img, int index)
        {
            var panel = new Panel
            {
                Size = new Size(490, 120),
                Margin = new Padding(5, 8, 5, 8),
                BackColor = Color.FromArgb(95, 60, 40),
                Cursor = Cursors.Hand,
                Tag = index,
                BorderStyle = BorderStyle.None // Không vẽ border
            };

            var pic = new PictureBox
            {
                Image = img,
                Size = new Size(100, 100),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(70, 45, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            panel.Controls.Add(pic);

            // Avatar name label
            var lblName = new Label
            {
                Text = avatarNames[index],
                Location = new Point(120, 15),
                Size = new Size(360, 25),
                Font = new Font("Courier New", 12, FontStyle.Bold),
                ForeColor = goldColor,
                BackColor = Color.Transparent
            };
            panel.Controls.Add(lblName);

            // Avatar description
            var lblDesc = new Label
            {
                Text = GetAvatarDescription(index),
                Location = new Point(120, 45),
                Size = new Size(360, 65),
                Font = new Font("Courier New", 8),
                ForeColor = Color.LightGoldenrodYellow,
                BackColor = Color.Transparent,
                AutoSize = false
            };
            panel.Controls.Add(lblDesc);

            // Click handlers
            panel.Click += (s, e) => SelectAvatar(index);
            pic.Click += (s, e) => SelectAvatar(index);
            lblName.Click += (s, e) => SelectAvatar(index);

            // Double-click to confirm
            panel.DoubleClick += (s, e) => ConfirmSelection(index);
            pic.DoubleClick += (s, e) => ConfirmSelection(index);

            // Smooth hover effects - chỉ thay đổi màu nền, KHÔNG vẽ border
            panel.MouseEnter += (s, e) =>
            {
                if (index != currentSelection)
                {
                    panel.BackColor = Color.FromArgb(120, 75, 50); // Hover: sáng hơn
                }
            };
            panel.MouseLeave += (s, e) =>
            {
                if (index != currentSelection)
                {
                    panel.BackColor = Color.FromArgb(95, 60, 40); // Back to normal
                }
            };

            return panel;
        }

        private string GetAvatarDescription(int index)
        {
            return index switch
            {
                0 => "Balanced fighter\nGood HP & Damage\nPerfect for beginners",
                1 => "High damage output\nLower HP\nFor aggressive players",
                2 => "High speed\nExcellent mobility\nFast attack rate",
                3 => "Tank character\nVery high HP\nSlow but powerful",
                _ => "Unknown"
            };
        }

        private void SelectAvatar(int index)
        {
            currentSelection = index;
            HighlightSelected();
            UpdatePreview();
        }

        private void ConfirmSelection(int index)
        {
            SelectAvatar(index);
            SelectedIndex = index;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void HighlightSelected()
        {
            for (int i = 0; i < avatarListPanel.Controls.Count; i++)
            {
                var p = avatarListPanel.Controls[i] as Panel;
                if (p != null)
                {
                    if (i == currentSelection)
                    {
                        // Selected: màu sáng hơn
                        p.BackColor = Color.FromArgb(130, 85, 60);
                    }
                    else
                    {
                        // Not selected: màu bình thường
                        p.BackColor = Color.FromArgb(95, 60, 40);
                    }
                }
            }
        }

        private void UpdatePreview()
        {
            if (currentSelection >= 0 && currentSelection < avatars.Length)
            {
                pbPreview.Image = avatars[currentSelection];
                lblAvatarName.Text = avatarNames[currentSelection].ToUpper();
                lblPreviewInfo.Text = $"{GetAvatarDescription(currentSelection)}\n\n💡 Double-click to select immediately";
            }
            else
            {
                pbPreview.Image = null;
                lblAvatarName.Text = "";
                lblPreviewInfo.Text = "Select an avatar to preview";
            }
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (currentSelection < 0)
            {
                MessageBox.Show("Please select an avatar.", "Select Avatar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SelectedIndex = currentSelection;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}