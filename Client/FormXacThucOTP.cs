using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DoAn_NT106.Services;

namespace DoAn_NT106
{
    public partial class FormXacThucOTP : Form
    {
        private readonly string _username;
        private readonly TcpClientService tcpClient; // ✅ TCP CLIENT
        private readonly DatabaseService dbService; // ✅ DATABASE SERVICE
        private System.Windows.Forms.Timer otpTimer;
        private int remainingSeconds = 300;

        // ✅ CẤU HÌNH: true = dùng Server, false = dùng Database trực tiếp
        private bool useServer = true;

        public FormXacThucOTP(string username)
        {
            InitializeComponent();
            _username = username;

            // ✅ KHỞI TẠO CẢ HAI SERVICE
            tcpClient = new TcpClientService("127.0.0.1", 8080);
            dbService = new DatabaseService();

            InitializeTimer();
            InitializeOTPAutoFocus();
        }

        private void InitializeTimer()
        {
            otpTimer = new System.Windows.Forms.Timer();
            otpTimer.Interval = 1000;
            otpTimer.Tick += OtpTimer_Tick;
            otpTimer.Start();
        }

        private void InitializeOTPAutoFocus()
        {
            // Auto focus giữa các ô OTP
            tb_otp1.TextChanged += (s, e) => { if (tb_otp1.Text.Length == 1) tb_otp2.Focus(); };
            tb_otp2.TextChanged += (s, e) => { if (tb_otp2.Text.Length == 1) tb_otp3.Focus(); };
            tb_otp3.TextChanged += (s, e) => { if (tb_otp3.Text.Length == 1) tb_otp4.Focus(); };
            tb_otp4.TextChanged += (s, e) => { if (tb_otp4.Text.Length == 1) tb_otp5.Focus(); };
            tb_otp5.TextChanged += (s, e) => { if (tb_otp5.Text.Length == 1) tb_otp6.Focus(); };

            // Chỉ cho phép nhập số
            tb_otp1.KeyPress += OtpBox_KeyPress;
            tb_otp2.KeyPress += OtpBox_KeyPress;
            tb_otp3.KeyPress += OtpBox_KeyPress;
            tb_otp4.KeyPress += OtpBox_KeyPress;
            tb_otp5.KeyPress += OtpBox_KeyPress;
            tb_otp6.KeyPress += OtpBox_KeyPress;
        }
        private void ResetOtpTextBoxes()
        {
            tb_otp1.Text = "";
            tb_otp2.Text = "";
            tb_otp3.Text = "";
            tb_otp4.Text = "";
            tb_otp5.Text = "";
            tb_otp6.Text = "";

            // Focus về ô đầu tiên
            tb_otp1.Focus();

            Console.WriteLine("✅ OTP textboxes reset!");
        }
        private void OtpTimer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;

            if (remainingSeconds <= 0)
            {
                otpTimer.Stop();
                lbl_timer.Text = "OTP has expired!";
                lbl_timer.ForeColor = Color.Red;
                btn_verify.Enabled = false;
                return;
            }

            int minutes = remainingSeconds / 60;
            int seconds = remainingSeconds % 60;
            lbl_timer.Text = $"Code expires in: {minutes:D2}:{seconds:D2}";

            // Cảnh báo khi còn 30 giây
            if (remainingSeconds <= 30)
            {
                lbl_timer.ForeColor = Color.Red;
            }
        }

        // ✅ VERIFY OTP - HỖ TRỢ CẢ SERVER & DATABASE (ASYNC)
        private async void btn_verify_Click(object sender, EventArgs e)
        {
            lblOTPError.Text = "";
            string otp = string.Concat(
                tb_otp1.Text.Trim(),
                tb_otp2.Text.Trim(),
                tb_otp3.Text.Trim(),
                tb_otp4.Text.Trim(),
                tb_otp5.Text.Trim(),
                tb_otp6.Text.Trim()
            );

            if (otp.Length != 6 || !otp.All(char.IsDigit))
            {
                lblOTPError.Text = "Please enter all 6 digits of the OTP!";
                return;
            }

            bool isValid = false;
            string message = "";

            if (useServer)
            {
                // ✅ DÙNG SERVER (ASYNC)
                var response = await tcpClient.VerifyOtpAsync(_username, otp);
                isValid = response.Success;
                message = response.Message;
            }
            else
            {
                // ✅ DÙNG DATABASE TRỰC TIẾP
                var result = dbService.VerifyOtp(_username, otp);
                isValid = result.IsValid;
                message = result.Message;
            }

            if (isValid)
            {
                MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                FormResetPass formReset = new FormResetPass(_username);
                formReset.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ✅ RESEND OTP - ASYNC
        private async void btn_resend_Click(object sender, EventArgs e)
        {
            string newOtp = null;

            if (useServer)
            {
                // ✅ DÙNG SERVER (ASYNC)
                var response = await tcpClient.GenerateOtpAsync(_username);

                if (response.Success)
                {
                    newOtp = response.GetDataValue("otp");
                }
                else
                {
                    MessageBox.Show("Unable to generate new OTP. Please try again!",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                // ✅ DÙNG DATABASE TRỰC TIẾP
                newOtp = dbService.GenerateOtp(_username);

                if (string.IsNullOrEmpty(newOtp))
                {
                    MessageBox.Show("Unable to generate new OTP. Please try again!",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            ResetOtpTextBoxes();
            MessageBox.Show($"Your new OTP is: {newOtp}\n(This is shown for testing only)",
                "New OTP", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Reset timer
            remainingSeconds = 300;
            lbl_timer.ForeColor = Color.White;
            btn_verify.Enabled = true;
            otpTimer.Start();
        }

        private void btn_backToLogin_Click(object sender, EventArgs e)
        {
            Console.WriteLine("🎯 Return to Login button CLICKED!");

            // ✅ ĐẢM BẢO DỪNG TIMER TRƯỚC KHI ĐÓNG
            otpTimer?.Stop();
            otpTimer?.Dispose();
            otpTimer = null;

            // ✅ MỞ FORM ĐĂNG NHẬP
            FormDangNhap loginForm = new FormDangNhap();
            loginForm.Show();

            // ✅ ĐÓNG FORM HIỆN TẠI
            this.Close();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            otpTimer?.Stop();
            otpTimer?.Dispose();
            base.OnFormClosing(e);
        }

        private void OtpBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }
    }
}
