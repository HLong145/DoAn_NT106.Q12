using DoAn_NT106.Services;
using System;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DoAn_NT106
{
    public partial class FormResetPass : Form
    {
        private readonly string _username;
        private readonly PersistentTcpClient tcpClient;
        private readonly ValidationService _validationService;


        public FormResetPass(string username)
        {
            InitializeComponent();
            _username = username;
            _validationService = new ValidationService();

            // Khởi tạo tcp client
            tcpClient = PersistentTcpClient.Instance;
        }

        public FormResetPass() : this(string.Empty)
        {
        }

        // ✅ RESET PASSWORD (ASYNC)
        private async void btn_complete_Click(object sender, EventArgs e)
        {
            lblNewPasswordError.Text = string.Empty;
            lblConfirmPasswordError.Text = string.Empty;

            string newPass = tbPassword.Text.Trim();
            string confirmPass = tbconfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(newPass))
            {
                lblNewPasswordError.Text = "Please enter both password fields!";
                lblConfirmPasswordError.Text = string.Empty;

                MessageBox.Show(
                    "Please enter both password fields!",
                    "Validation error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                tbPassword.Focus();
                return;
            }

            if (string.IsNullOrEmpty(confirmPass))
            {
                lblNewPasswordError.Text = "Please enter both password fields!";
                lblConfirmPasswordError.Text = "Please enter both password fields!";

                MessageBox.Show(
                    "Please enter both password fields!",
                    "Validation error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                tbconfirmPassword.Focus();
                return;
            }

            if (!_validationService.IsValidPassword(newPass))
            {
                tbPassword.Text = string.Empty;
                tbconfirmPassword.Text = string.Empty;
                lblConfirmPasswordError.Text = string.Empty;

                lblNewPasswordError.Text =
                    "Password must be at least 8 characters long, including uppercase, lowercase, number and a special character!";

                MessageBox.Show(
                    "Password must be at least 8 characters long, including uppercase, lowercase, number and a special character!",
                    "Invalid Password",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                tbPassword.Focus();
                return;
            }

            if (newPass != confirmPass)
            {
                lblNewPasswordError.Text = string.Empty;
                lblConfirmPasswordError.Text = "Password confirmation does not match!";

                MessageBox.Show(
                    "Password confirmation does not match!",
                    "Validation error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                tbconfirmPassword.Focus();
                return;
            }

            bool success = false;
            string message = "";

            btn_complete.Enabled = false;

            try
            {
                var response = await tcpClient.ResetPasswordAsync(_username, newPass);
                success = response.Success;
                message = response.Message;
            }
            catch (Exception ex)
            {
                success = false;
                message = "An error occurred while resetting the password: " + ex.Message;
            }
            finally
            {
                btn_complete.Enabled = true;
            }

            if (success)
            {
                MessageBox.Show(
                    "Password has been reset successfully!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                this.Hide();
                FormDangNhap formLogin = new FormDangNhap();
                formLogin.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show(
                    message,
                    "System Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void btn_backToLogin_Click(object sender, EventArgs e)
        {
            this.Close();
            FormDangNhap formLogin = new FormDangNhap();
            formLogin.Show();
        }

        private void tbnewPassword_TextChanged(object sender, EventArgs e)
        {
            string password = tbPassword.Text;
            bool isValidPassword = _validationService.IsValidPassword(password);

            if (string.IsNullOrEmpty(tbPassword.Text))
            {
                lblNewPasswordError.Text = "Please fill password field";
            }
            else
            {
                if (!isValidPassword)
                {
                    lblNewPasswordError.Text = "Password must be at least 8 characters long, including uppercase, lowercase, number and a special character!";
                }
                else
                {
                    lblNewPasswordError.Text = string.Empty;
                }

                if (!string.IsNullOrEmpty(tbconfirmPassword.Text) && tbconfirmPassword.Text != tbPassword.Text)
                {
                    lblConfirmPasswordError.Text = "Password confirmation does not match!";
                }
                else
                {
                    lblConfirmPasswordError.Text = string.Empty;
                }
            }
        }

        private void tbconfirmPassword_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbconfirmPassword.Text) || string.IsNullOrEmpty(tbPassword.Text))
            {
                lblNewPassword.Text = string.Empty;
                lblConfirmPassword.Text = string.Empty;
            }
            else if (tbconfirmPassword.Text.Trim() != tbPassword.Text.Trim())
            {
                lblConfirmPasswordError.Text = "Password confirmation does not match!";
            }
            else
            {
                lblConfirmPasswordError.Text = string.Empty;
            }
        }

        private void tbnewPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                this.SelectNextControl(this.ActiveControl, true, true, true, true);
            }
        }

        private void tbconfirmPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                btn_complete_Click(sender, e);
            }
        }
    }
}
