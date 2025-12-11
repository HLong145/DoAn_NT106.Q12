using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PixelGameLobby
{
    /// <summary>
    /// Form for selecting characters for offline testing (both players)
    /// </summary>
    public class OfflineTestForm : Form
    {
        // Results
        public string Player1Character { get; private set; }
        public string Player2Character { get; private set; }
        public string Player2Name { get; private set; }

        // Colors
        private Color primaryBrown = Color.FromArgb(160, 82, 45);
        private Color darkBrown = Color.FromArgb(101, 67, 51);
        private Color darkerBrown = Color.FromArgb(74, 50, 25);
        private Color goldColor = Color.FromArgb(255, 215, 0);

        // Character options
        private List<CharacterOption> characters = new List<CharacterOption>
        {
            new CharacterOption { Name = "girlknight", DisplayName = "Scarlet Hunter" },
            new CharacterOption { Name = "bringerofdeath", DisplayName = "Bringer of Death" },
            new CharacterOption { Name = "goatman", DisplayName = "Goatman Berserker" },
            new CharacterOption { Name = "warrior", DisplayName = "Elite Warrior" }
        };

        // UI Components
        private ComboBox cmbPlayer1;
        private ComboBox cmbPlayer2;
        private ComboBox cmbMap;
        private TextBox txtPlayer2Name;
        private Button btnStart;
        private Button btnCancel;
        public string SelectedMap { get; private set; } = "battleground1";

        public OfflineTestForm(string player1Name)
        {
            InitializeUI(player1Name);
        }

        private void InitializeUI(string player1Name)
        {
            this.Text = "Offline Test Room";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = primaryBrown;

            // Main panel
            var mainPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(440, 320),
                BackColor = darkBrown,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(mainPanel);

            // Title
            var lblTitle = new Label
            {
                Text = "🧪 OFFLINE MODE",
                Font = new Font("Courier New", 16, FontStyle.Bold),
                ForeColor = goldColor,
                Location = new Point(20, 15),
                Size = new Size(400, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblTitle);

            // Player 1 section
            var lblPlayer1 = new Label
            {
                Text = $"Player 1: {player1Name}",
                Font = new Font("Courier New", 11, FontStyle.Bold),
                ForeColor = Color.Cyan,
                Location = new Point(20, 60),
                Size = new Size(400, 25)
            };
            mainPanel.Controls.Add(lblPlayer1);

            var lblP1Char = new Label
            {
                Text = "Character:",
                Font = new Font("Courier New", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(40, 90),
                Size = new Size(120, 25)
            };
            mainPanel.Controls.Add(lblP1Char);

            cmbPlayer1 = new ComboBox
            {
                Location = new Point(160, 88),
                Size = new Size(250, 30),
                Font = new Font("Courier New", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = darkerBrown,
                ForeColor = goldColor
            };
            foreach (var character in characters)
            {
                cmbPlayer1.Items.Add(character.DisplayName);
            }
            cmbPlayer1.SelectedIndex = 0; // Default: Girl Knight
            mainPanel.Controls.Add(cmbPlayer1);

            // Player 2 section
            var lblPlayer2 = new Label
            {
                Text = "Player 2:",
                Font = new Font("Courier New", 11, FontStyle.Bold),
                ForeColor = Color.Orange,
                Location = new Point(20, 130),
                Size = new Size(120, 25)
            };
            mainPanel.Controls.Add(lblPlayer2);

            var lblP2Name = new Label
            {
                Text = "Name:",
                Font = new Font("Courier New", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(40, 160),
                Size = new Size(120, 25)
            };
            mainPanel.Controls.Add(lblP2Name);

            txtPlayer2Name = new TextBox
            {
                Location = new Point(160, 158),
                Size = new Size(250, 30),
                Font = new Font("Courier New", 10),
                BackColor = darkerBrown,
                ForeColor = goldColor,
                Text = "Player 2"
            };
            mainPanel.Controls.Add(txtPlayer2Name);

            var lblP2Char = new Label
            {
                Text = "Character:",
                Font = new Font("Courier New", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(40, 195),
                Size = new Size(120, 25)
            };
            mainPanel.Controls.Add(lblP2Char);

            cmbPlayer2 = new ComboBox
            {
                Location = new Point(160, 193),
                Size = new Size(250, 30),
                Font = new Font("Courier New", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = darkerBrown,
                ForeColor = goldColor
            };
            foreach (var character in characters)
            {
                cmbPlayer2.Items.Add(character.DisplayName);
            }
            cmbPlayer2.SelectedIndex = 0; // Default: Girl Knight
            mainPanel.Controls.Add(cmbPlayer2);

            // Map selection
            var lblMap = new Label
            {
                Text = "Map:",
                Font = new Font("Courier New", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(40, 235),
                Size = new Size(120, 25)
            };
            mainPanel.Controls.Add(lblMap);

            cmbMap = new ComboBox
            {
                Location = new Point(160, 233),
                Size = new Size(250, 30),
                Font = new Font("Courier New", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = darkerBrown,
                ForeColor = goldColor
            };
            cmbMap.Items.Add("Battleground 1");
            cmbMap.Items.Add("Battleground 2");
            cmbMap.Items.Add("Battleground 3");
            cmbMap.Items.Add("Battleground 4");
            cmbMap.SelectedIndex = 0; // default
            mainPanel.Controls.Add(cmbMap);

            // Start button
            btnStart = new Button
            {
                Text = "▶ START BATTLE",
                Location = new Point(40, 275),
                Size = new Size(180, 35),
                Font = new Font("Courier New", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnStart.Click += BtnStart_Click;
            btnStart.MouseEnter += Button_MouseEnter;
            btnStart.MouseLeave += (s, e) =>
            {
                if (s is Button btn) btn.BackColor = Color.FromArgb(34, 139, 34);
            };
            mainPanel.Controls.Add(btnStart);

            // Cancel button
            btnCancel = new Button
            {
                Text = "CANCEL",
                Location = new Point(230, 275),
                Size = new Size(180, 35),
                Font = new Font("Courier New", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(178, 34, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            btnCancel.MouseEnter += Button_MouseEnter;
            btnCancel.MouseLeave += (s, e) =>
            {
                if (s is Button btn) btn.BackColor = Color.FromArgb(178, 34, 34);
            };
            mainPanel.Controls.Add(btnCancel);
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            // Validate Player 2 name
            Player2Name = txtPlayer2Name.Text.Trim();
            if (string.IsNullOrEmpty(Player2Name))
            {
                MessageBox.Show("Please enter Player 2 name!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPlayer2Name.Focus();
                return;
            }

            // Get selected characters
            Player1Character = characters[cmbPlayer1.SelectedIndex].Name;
            Player2Character = characters[cmbPlayer2.SelectedIndex].Name;

            // Selected map (battleground1..4)
            if (cmbMap != null && cmbMap.SelectedIndex >= 0)
            {
                SelectedMap = $"battleground{cmbMap.SelectedIndex + 1}";
            }
            else
            {
                SelectedMap = "battleground1";
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Button_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                button.BackColor = Color.FromArgb(
                    Math.Min(button.BackColor.R + 30, 255),
                    Math.Min(button.BackColor.G + 30, 255),
                    Math.Min(button.BackColor.B + 30, 255)
                );
            }
        }

        private class CharacterOption
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
        }
    }
}
