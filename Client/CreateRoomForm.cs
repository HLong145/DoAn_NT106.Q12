using System;
using System.Drawing;
using System.Windows.Forms;

namespace DoAn_NT106.Client
{
    public partial class CreateRoomForm : Form
    {
        #region Properties

        public string RoomName { get; private set; }      
        public string RoomPassword { get; private set; }

        #endregion

        #region Constructor and Init

        public CreateRoomForm()
        {
            InitializeComponent();
            SetupEventHandlers();
        }

        //Gắn các event handlers
        private void SetupEventHandlers()
        {
            chkHasPassword.CheckedChanged += ChkHasPassword_CheckedChanged;

            btnCreate.Click += BtnCreate_Click;
            btnCancel.Click += BtnCancel_Click;

            btnCreate.MouseEnter += Button_MouseEnter;
            btnCreate.MouseLeave += Button_MouseLeave;
            btnCancel.MouseEnter += Button_MouseEnter;
            btnCancel.MouseLeave += Button_MouseLeave;

            txtRoomName.Enter += TextBox_Enter;
            txtRoomName.Leave += TextBox_Leave;
            txtPassword.Enter += TextBox_Enter;
            txtPassword.Leave += TextBox_Leave;
        }

        #endregion

        #region Event Handlers

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
                // Clear password when user disables password option
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
            // User hủy tạo phòng
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion

        #region UI Effects
        // UI EFFECTS
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
                    btn.BackColor = Color.FromArgb(0, 128, 0);       
                else if (btn == btnCancel)
                    btn.BackColor = Color.FromArgb(128, 128, 128);   
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

        #endregion

        #region Helpers

        // HELPERS
        private void ShowError(string message)
        {
            MessageBox.Show(
                message,
                "⚠️ Validation Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        #endregion
    }
}
