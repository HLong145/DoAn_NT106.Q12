using System;
using System.Windows.Forms;
using DoAn_NT106.Services;

namespace DoAn_NT106
{
    public partial class FormQuenPass : Form
    {
        private readonly PersistentTcpClient tcpClient;  

        public FormQuenPass()
        {
            InitializeComponent();
            lblContactError.Text = "";

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
                lblContactError.Text = "⚠ Please enter your Email or Phone number.";
                return;
            }

            bool isEmail = IsValidEmail(input);
            bool isPhone = IsValidPhone(input);

            if (!isEmail && !isPhone)
            {
                lblContactError.Text = "Please enter a valid email or phone number format!";
                return;
            }

            try
            {
                // Disable nút trong lúc chờ phản hồi
                btn_continue.Enabled = false;

                string username = null;
                string otp = null;

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

                otp = otpResponse.GetDataValue("otp");

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
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^0\d{9}$");
        }
    }
}
