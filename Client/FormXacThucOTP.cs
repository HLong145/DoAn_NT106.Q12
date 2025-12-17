using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DoAn_NT106.Client.Class;

namespace DoAn_NT106.Client
{
    public partial class FormXacThucOTP : Form
    {
        #region Fields

        private readonly string _username;
        private readonly PersistentTcpClient tcpClient;

        private System.Windows.Forms.Timer otpTimer;        
        private int remainingSeconds = 300;                 
        #endregion

        #region Constructor

        public FormXacThucOTP(string username)
        {
            InitializeComponent();

            _username = username;

            // tcpClient dùng để verify / resend OTP qua server
            tcpClient = PersistentTcpClient.Instance;

            InitializeTimer();        
            InitializeOTPAutoFocus();   
        }

        #endregion

        #region Timer Setup and Handling

        private void InitializeTimer()
        {
            // Khởi tạo timer
            otpTimer = new System.Windows.Forms.Timer();
            otpTimer.Interval = 1000; // Tick mỗi 1 giây
            otpTimer.Tick += OtpTimer_Tick;
            otpTimer.Start();
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

            // Cảnh báo khi còn 30 giây cuối
            if (remainingSeconds <= 30)
            {
                lbl_timer.ForeColor = Color.Red;
            }
        }

        #endregion

        #region OTP TextBoxes Behavior

        private void InitializeOTPAutoFocus()
        {
            // Auto focus giữa các ô OTP: khi đủ 1 ký tự thì nhảy sang ô tiếp theo
            tb_otp1.TextChanged += (s, e) => { if (tb_otp1.Text.Length == 1) tb_otp2.Focus(); };
            tb_otp2.TextChanged += (s, e) => { if (tb_otp2.Text.Length == 1) tb_otp3.Focus(); };
            tb_otp3.TextChanged += (s, e) => { if (tb_otp3.Text.Length == 1) tb_otp4.Focus(); };
            tb_otp4.TextChanged += (s, e) => { if (tb_otp4.Text.Length == 1) tb_otp5.Focus(); };
            tb_otp5.TextChanged += (s, e) => { if (tb_otp5.Text.Length == 1) tb_otp6.Focus(); };
            tb_otp6.TextChanged += (s, e) => { if (tb_otp6.Text.Length == 1) tb_otp1.Focus(); }; 

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

            tb_otp1.Focus();

            Console.WriteLine("✅ OTP textboxes reset!");
        }

        private void OtpBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Chặn tất cả ký tự không phải số và không phải phím Backspace
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }

        #endregion

        #region Verify OTP

        private async void btn_verify_Click(object sender, EventArgs e)
        {
            lblOTPError.Text = "";

            // Ghép 6 ô OTP thành 1 chuỗi
            string otp = string.Concat(
                tb_otp1.Text.Trim(),
                tb_otp2.Text.Trim(),
                tb_otp3.Text.Trim(),
                tb_otp4.Text.Trim(),
                tb_otp5.Text.Trim(),
                tb_otp6.Text.Trim()
            );

            // Validate basic: phải đủ 6 chữ số
            if (otp.Length != 6 || !otp.All(char.IsDigit))
            {
                lblOTPError.Text = "Please enter all 6 digits of the OTP!";
                return;
            }

            bool isValid = false;
            string message = "";


            // Gửi OTP lên server để xác thực
            var response = await tcpClient.VerifyOtpAsync(_username, otp);
            isValid = response.Success;
            message = response.Message;

            if (isValid)
            {
                // Nếu OTP đúng => thông báo + mở form đổi mật khẩu
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

        #endregion

        #region Resend OTP

        // RESEND OTP
        private async void btn_resend_Click(object sender, EventArgs e)
        {
            var response = await tcpClient.GenerateOtpAsync(_username);

            if (!response.Success)
            {
                MessageBox.Show(
                    "Unable to generate new OTP. Please try again!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            ResetOtpTextBoxes();

            MessageBox.Show(
                "A new OTP has been sent to your email.",
                "New OTP",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // Đặt lại bộ đếm thời gian
            remainingSeconds = 300;
            lbl_timer.ForeColor = Color.White;
            btn_verify.Enabled = true;
            otpTimer.Start();
        }

        #endregion

        #region Navigation

        private void btn_backToLogin_Click(object sender, EventArgs e)
        {
            Console.WriteLine("🎯 Return to Login button CLICKED!");

            // Timer dừng trước khi đóng
            otpTimer?.Stop();
            otpTimer?.Dispose();
            otpTimer = null;

            //  Mở form đăng nhập
            FormDangNhap loginForm = new FormDangNhap();
            loginForm.Show();


            this.Close();
        }

        #endregion

        #region Form Lifecycle

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Giải phóng timer khi form đóng
            otpTimer?.Stop();
            otpTimer?.Dispose();

            base.OnFormClosing(e);
        }

        #endregion
    }
}
