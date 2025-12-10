using System;
using System.Windows.Forms;
using DoAn_NT106.Services;

namespace DoAn_NT106
{
    public partial class FormQuenPass : Form
    {
        private readonly PersistentTcpClient tcpClient;
        private readonly ValidationService validationService;

        public FormQuenPass()
        {
            InitializeComponent();
            validationService = new ValidationService();
            lblContactError.Text = string.Empty;

            tcpClient = PersistentTcpClient.Instance;
        }

        private void btn_backToLogin_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ✅ BẤM "TIẾP TỤC" - ASYNC VERSION
        private async void btn_continue_Click(object sender, EventArgs e)
        {
            lblContactError.Text = "";
            string input = tb_contact.Text.Trim();

            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show(
                    "⚠ Please enter your Email or Phone number.",
                    "Validation error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                tb_contact.Focus();
                return;
            }
            else if (!IsValidEmail(input) && !IsValidPhone(input))
            {
                MessageBox.Show(
                    "Please enter a valid email or phone number format!",
                    "Validation error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                tb_contact.Focus();
                return;
            }

            bool isEmail = IsValidEmail(input);
            bool isPhone = IsValidPhone(input);

            try
            {
                // Disable nút trong lúc chờ phản hồi
                btn_continue.Enabled = false;

                string username = null;

                var getUserResponse = await tcpClient.GetUserByContactAsync(input, isEmail);

                if (!getUserResponse.Success)
                {
                    MessageBox.Show("No account found matching this information!",
                        "Account Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btn_continue.Enabled = true;
                    return;
                }

                username = getUserResponse.GetDataValue("username");

                var otpResponse = await tcpClient.GenerateOtpAsync(username);

                if (!otpResponse.Success)
                {
                    MessageBox.Show("Unable to generate OTP. Please try again!",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btn_continue.Enabled = true;
                    return;
                }

                // Do not read or show OTP in client. Server sends it by email only.

                FormXacThucOTP formOtp = new FormXacThucOTP(username);
                formOtp.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occurred: " + ex.Message,
                    "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btn_continue.Enabled = true;
            }
        }

        private bool IsValidEmail(string email)
        {
            return validationService.IsValidEmail(email);
        }

        private bool IsValidPhone(string phone)
        {
            return validationService.IsValidPhone(phone);
        }

        private void tb_contact_TextChanged(object sender, EventArgs e)
        {
            string input = tb_contact.Text.Trim();

            if (string.IsNullOrEmpty(input))
            {
                lblContactError.Text = "⚠ Please enter your Email or Phone number.";
                return;
            }

            else if (!IsValidEmail(input) && !IsValidPhone(input))
            {
                lblContactError.Text = "Please enter a valid email or phone number format!";
                return;
            }
            else
            {
                lblContactError.Text = string.Empty;
            }
        }

        private void tb_contact_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                btn_continue_Click(sender, e);
            }
        }
    }
}
