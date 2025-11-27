using System;
using System.Drawing;
using System.Windows.Forms;

namespace DoAn_NT106.Client
{
    public partial class CreateRoomForm : Form
    {
        // Properties để trả về kết quả
        public string RoomName { get; private set; }
        public string RoomPassword { get; private set; }

        public CreateRoomForm()
        {
            InitializeComponent();
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            // Checkbox password
            chkHasPassword.CheckedChanged += ChkHasPassword_CheckedChanged;

            // Buttons
            btnCreate.Click += BtnCreate_Click;
            btnCancel.Click += BtnCancel_Click;

            // Hover effects
            btnCreate.MouseEnter += Button_MouseEnter;
            btnCreate.MouseLeave += Button_MouseLeave;
            btnCancel.MouseEnter += Button_MouseEnter;
            btnCancel.MouseLeave += Button_MouseLeave;

            // TextBox placeholder effect
            txtRoomName.Enter += TextBox_Enter;
            txtRoomName.Leave += TextBox_Leave;
            txtPassword.Enter += TextBox_Enter;
            txtPassword.Leave += TextBox_Leave;
        }

        // ===========================
        // EVENT HANDLERS
        // ===========================

        private void ChkHasPassword_CheckedChanged(object sender, EventArgs e)
        {
            bool showPassword = chkHasPassword.Checked;

            lblPassword.Visible = showPassword;
            txtPassword.Visible = showPassword;
            txtPassword.Enabled = showPassword;
            lblPasswordHint.Visible = showPassword;

            if (showPassword)
            {
                txtPassword.Focus();
            }
            else
            {
                txtPassword.Text = "";
            }
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            // Validate room name
            string roomName = txtRoomName.Text.Trim();

            if (string.IsNullOrEmpty(roomName))
            {
                ShowError("Please enter a room name!");
                txtRoomName.Focus();
                return;
            }

            if (roomName.Length < 3)
            {
                ShowError("Room name must be at least 3 characters!");
                txtRoomName.Focus();
                return;
            }

            if (roomName.Length > 50)
            {
                ShowError("Room name must be less than 50 characters!");
                txtRoomName.Focus();
                return;
            }

            // Validate password if checked
            string password = null;
            if (chkHasPassword.Checked)
            {
                password = txtPassword.Text;
                if (string.IsNullOrEmpty(password))
                {
                    ShowError("Please enter a password or uncheck the password option!");
                    txtPassword.Focus();
                    return;
                }

                if (password.Length < 4)
                {
                    ShowError("Password must be at least 4 characters!");
                    txtPassword.Focus();
                    return;
                }
            }

            // Set properties và close dialog
            RoomName = roomName;
            RoomPassword = password;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // ===========================
        // UI EFFECTS
        // ===========================

        private void Button_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                btn.BackColor = Color.FromArgb(
                    Math.Min(btn.BackColor.R + 30, 255),
                    Math.Min(btn.BackColor.G + 30, 255),
                    Math.Min(btn.BackColor.B + 30, 255)
                );
            }
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn == btnCreate)
                    btn.BackColor = Color.FromArgb(0, 128, 0); // Green
                else if (btn == btnCancel)
                    btn.BackColor = Color.FromArgb(128, 128, 128); // Gray
            }
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            if (sender is TextBox txt)
            {
                txt.BackColor = Color.FromArgb(120, 80, 60);
            }
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            if (sender is TextBox txt)
            {
                txt.BackColor = Color.FromArgb(101, 67, 51);
            }
        }

        // ===========================
        // HELPERS
        // ===========================

        private void ShowError(string message)
        {
            MessageBox.Show(
                message,
                "⚠️ Validation Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
    }
}