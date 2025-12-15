using System;
using System.Windows.Forms;

namespace DoAn_NT106
{
    public partial class InstructionForm : Form
    {
        public InstructionForm()
        {
            InitializeComponent();
            LoadDefaultTexts();
        }

        private void LoadDefaultTexts()
        {
            // Offline controls will be displayed using custom method
            DisplayOfflineControls();
            DisplayOnlineControls();
        }

        private void DisplayOfflineControls()
        {
            // Clear existing controls
            tabOffline.Controls.Clear();

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            int y = 10;

            // Title
            var lblTitle = new Label
            {
                Text = "OFFLINE MODE - CONTROLS:",
                Font = new Font("Courier New", 12F, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(10, y),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);
            y += 30;

            // Player 1 section
            var lblP1 = new Label
            {
                Text = "Player 1 Controls:",
                Font = new Font("Courier New", 11F, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(10, y),
                AutoSize = true
            };
            panel.Controls.Add(lblP1);
            y += 25;

            // Player 1 controls with images
            var controls1 = new[] {
                ("Move Left:", Properties.Resources.A),
                ("Move Right:", Properties.Resources.D),
                ("Jump:", Properties.Resources.W),
                ("Punch:", Properties.Resources.J),
                ("Kick:", Properties.Resources.K),
                ("Dash:", Properties.Resources.L),
                ("Parry:", Properties.Resources.U),
                ("Skill:", Properties.Resources.I),
            };

            foreach (var (name, img) in controls1)
            {
                var row = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    AutoSize = true,
                    Location = new Point(20, y),
                    Height = 60
                };

                var lbl = new Label
                {
                    Text = name,
                    Font = new Font("Courier New", 10F),
                    AutoSize = true,
                    Margin = new Padding(0, 6, 10, 0)
                };
                row.Controls.Add(lbl);

                var pb = new PictureBox
                {
                    Image = img,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Size = new Size(48, 48),
                    Margin = new Padding(0)
                };
                row.Controls.Add(pb);

                panel.Controls.Add(row);
                y += 55;
            }

            y += 10;

            // Player 2 section
            var lblP2 = new Label
            {
                Text = "Player 2 Controls:",
                Font = new Font("Courier New", 11F, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                Location = new Point(10, y),
                AutoSize = true
            };
            panel.Controls.Add(lblP2);
            y += 25;

            // Player 2 controls with images
            var controls2 = new[] {
                ("Move Left:", Properties.Resources.ARROWLEFT),
                ("Move Right:", Properties.Resources.ARROWRIGHT),
                ("Jump:", Properties.Resources.ARROWUP),
                ("Punch:", Properties.Resources.num1),
                ("Kick:", Properties.Resources.num2),
                ("Dash:", Properties.Resources.num3),
                ("Parry:", Properties.Resources.num5),
            };

            foreach (var (name, img) in controls2)
            {
                var row = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    AutoSize = true,
                    Location = new Point(20, y),
                    Height = 60
                };

                var lbl = new Label
                {
                    Text = name,
                    Font = new Font("Courier New", 10F),
                    AutoSize = true,
                    Margin = new Padding(0, 6, 10, 0)
                };
                row.Controls.Add(lbl);

                var pb = new PictureBox
                {
                    Image = img,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Size = new Size(48, 48),
                    Margin = new Padding(0)
                };
                row.Controls.Add(pb);

                panel.Controls.Add(row);
                y += 55;
            }

            y += 10;

            // Tips section
            var lblTips = new Label
            {
                Text = "Tips:",
                Font = new Font("Courier New", 11F, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(10, y),
                AutoSize = true
            };
            panel.Controls.Add(lblTips);
            y += 25;

            var tips = new[] {
                "- Use parry to negate incoming attacks and gain mana.",
                "- Use dash to quickly close distance.",
                "- Manage stamina and mana for skills."
            };

            foreach (var tip in tips)
            {
                var lblTip = new Label
                {
                    Text = tip,
                    Font = new Font("Courier New", 10F),
                    AutoSize = true,
                    Location = new Point(20, y)
                };
                panel.Controls.Add(lblTip);
                y += 25;
            }

            tabOffline.Controls.Add(panel);
        }

        private void DisplayOnlineControls()
        {
            // Clear existing controls
            tabOnline.Controls.Clear();

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            int y = 10;

            // Title
            var lblTitle = new Label
            {
                Text = "ONLINE MODE - CONTROLS & NOTES:",
                Font = new Font("Courier New", 12F, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(10, y),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);
            y += 30;

            // Player 1 (Host)
            var lblP1 = new Label
            {
                Text = "Player 1 (Host):",
                Font = new Font("Courier New", 11F, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(10, y),
                AutoSize = true
            };
            panel.Controls.Add(lblP1);
            y += 25;

            var hostControls = new[] {
                ("Movement:", new[] { Properties.Resources.A, Properties.Resources.D }),
                ("Jump:", new[] { Properties.Resources.W }),
                ("Punch:", new[] { Properties.Resources.J }),
                ("Kick:", new[] { Properties.Resources.K }),
                ("Dash:", new[] { Properties.Resources.L }),
                ("Parry:", new[] { Properties.Resources.U }),
                ("Skill:", new[] { Properties.Resources.I }),
            };

            foreach (var (name, imgs) in hostControls)
            {
                var row = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    AutoSize = true,
                    Location = new Point(20, y),
                    Height = 60
                };

                var lbl = new Label
                {
                    Text = name,
                    Font = new Font("Courier New", 10F),
                    AutoSize = true,
                    Margin = new Padding(0, 6, 10, 0)
                };
                row.Controls.Add(lbl);

                foreach (var img in imgs)
                {
                    var pb = new PictureBox
                    {
                        Image = img,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Size = new Size(45, 45),
                        Margin = new Padding(4, 0, 4, 0)
                    };
                    row.Controls.Add(pb);
                }

                panel.Controls.Add(row);
                y += 55;
            }

            y += 10;

            // Player 2 (Guest)
            var lblP2 = new Label
            {
                Text = "Player 2 (Guest):",
                Font = new Font("Courier New", 11F, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                Location = new Point(10, y),
                AutoSize = true
            };
            panel.Controls.Add(lblP2);
            y += 25;

            var guestControls = new[] {
                ("Movement:", new[] { Properties.Resources.ARROWLEFT, Properties.Resources.ARROWRIGHT }),
                ("Jump:", new[] { Properties.Resources.ARROWUP }),
                ("Punch:", new[] { Properties.Resources.num1 }),
                ("Kick:", new[] { Properties.Resources.num2 }),
                ("Dash:", new[] { Properties.Resources.num3 }),
                ("Parry:", new[] { Properties.Resources.num5 }),
            };

            foreach (var (name, imgs) in guestControls)
            {
                var row = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    AutoSize = true,
                    Location = new Point(20, y),
                    Height = 60
                };

                var lbl = new Label
                {
                    Text = name,
                    Font = new Font("Courier New", 10F),
                    AutoSize = true,
                    Margin = new Padding(0, 6, 10, 0)
                };
                row.Controls.Add(lbl);

                foreach (var img in imgs)
                {
                    var pb = new PictureBox
                    {
                        Image = img,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Size = new Size(45, 45),
                        Margin = new Padding(4, 0, 4, 0)
                    };
                    row.Controls.Add(pb);
                }

                panel.Controls.Add(row);
                y += 55;
            }

            y += 10;

            // Network Notes
            var lblNotes = new Label
            {
                Text = "Network Notes:",
                Font = new Font("Courier New", 11F, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(10, y),
                AutoSize = true
            };
            panel.Controls.Add(lblNotes);
            y += 25;

            var notes = new[] {
                "- Both players connect to the same game room.",
                "- Controls respond with minimal latency.",
                "- Game syncs every 50ms."
            };

            foreach (var note in notes)
            {
                var lblNote = new Label
                {
                    Text = note,
                    Font = new Font("Courier New", 10F),
                    AutoSize = true,
                    Location = new Point(20, y)
                };
                panel.Controls.Add(lblNote);
                y += 25;
            }

            tabOnline.Controls.Add(panel);
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
