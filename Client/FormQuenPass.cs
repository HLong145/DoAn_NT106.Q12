using System;
using System.Windows.Forms;
using DoAn_NT106.Client.Class;
using DoAn_NT106.Services;

namespace DoAn_NT106.Client
{
    public partial class FormQuenPass : Form
    {
        #region Fields

        private readonly PersistentTcpClient tcpClient;      
        private readonly ValidationService validationService;

        private bool isProcessing = false;

        #endregion

        #region Constructor

        public FormQuenPass()
        {
            InitializeComponent();

            validationService = new ValidationService();
            lblContactError.Text = string.Empty;

            tcpClient = PersistentTcpClient.Instance;
        }

        #endregion

        #region Navigation

        private void btn_backToLogin_Click(object sender, EventArgs e)
        {
            if (isProcessing)
                return;

            SetAllControl(false);
            try
            {
                // Đóng form quên mật khẩu và quay về form login
                this.Close();
            }
            catch { }
            finally
            {
                SetAllControl(true);
            }
        }

        #endregion

        #region Button Logic

        private async void btn_continue_Click(object sender, EventArgs e)
        {
            if (isProcessing)
                return;

            SetAllControl(false);

            try
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

                // Disable nút trong lúc chờ phản hồi từ server

                string username = string.Empty;

                // Gửi request tìm user
                var getUserResponse = await tcpClient.GetUserByContactAsync(input, isEmail);
                if (!getUserResponse.Success)
                {
                    MessageBox.Show(
                        "No account found matching this information!",
                        "Account Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    btn_continue.Enabled = true;
                    return;
                }

                // Lấy username từ response
                username = getUserResponse.GetDataValue("username");

                // Gửi request tạo OTP cho tài khoản này
                var otpResponse = await tcpClient.GenerateOtpAsync(username);
                if (!otpResponse.Success)
                {
                    MessageBox.Show(
                        "Unable to generate OTP. Please try again!",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    btn_continue.Enabled = true;
                    return;
                }

                // Mở form xác thực OTP
                FormXacThucOTP formOtp = new FormXacThucOTP(username);
                formOtp.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error has occurred: " + ex.Message,
                    "System Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                SetAllControl(true);
            }
        }

        #endregion

        #region  Helpers

        private bool IsValidEmail(string email)
        {
            return validationService.IsValidEmail(email);
        }

        private bool IsValidPhone(string phone)
        {
            return validationService.IsValidPhone(phone);
        }

        private void SetAllControl(bool set)
        {
            isProcessing = !set;
            btn_backToLogin.Enabled = set;
            btn_continue.Enabled = set;
        }
        #endregion

        #region Live Validation and Keyboard

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

        #endregion
    }
}
