using DoAn_NT106.Client.Class;
using DoAn_NT106.Services;
using System;
using System.Drawing;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace DoAn_NT106.Client
{
    public partial class FormDangKy : Form
    {
        #region Fields

        private readonly PersistentTcpClient tcpClient;
        private readonly ValidationService validationService = new ValidationService();

        public event EventHandler SwitchToLogin;

        private System.Windows.Forms.Timer myTimer;


        private bool isPasswordVisible = false;
        private bool isConfirmPasswordVisible = false;

        private bool isProcessing = false;

        #endregion

        #region Constructor and Basic Setup

        public FormDangKy()
        {
            InitializeComponent();

            InitializeCustomUI();               
            this.VisibleChanged += FormDangKy_VisibleChanged;
            

            tcpClient = PersistentTcpClient.Instance;

            this.Shown += (s, e) =>
            {
                this.BringToFront();
                this.Focus();
                StartAnimations();
                Console.WriteLine("✅ FormDangKy shown!");
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.BringToFront();
            this.Focus();
            StartAnimations();
            MessageBox.Show("Phone number registration does not support password reset. \nPlease use email if you want to account recovery.",
                "⚠ Registration Notice",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);


        }

        #endregion

        #region UI / Placeholder Setup

        // GIAO DIỆN & PLACEHOLDER
        private void InitializeCustomUI()
        {
            SetPlaceholder(tb_username, "ENTER USERNAME");
            SetPlaceholder(tb_contact, "EMAIL OR PHONE");
            SetPasswordPlaceholder(tb_password, "ENTER PASSWORD");
            SetPasswordPlaceholder(tb_confirmPassword, "CONFIRM PASSWORD");


            DrawLockIcon(pictureBoxLock1, "🔒");
            DrawLockIcon(pictureBoxLock2, "🔒");

            // Thiết lập text và event cho checkbox captcha
            chkNotRobot.Text = " ☐ I'M NOT A ROBOT 🤖";
            chkNotRobot.CheckedChanged += (s, e) =>
            {
                lblRobotError.Text = "";
                chkNotRobot.Text = chkNotRobot.Checked
                    ? " ☑ I'M NOT A ROBOT 🤖"
                    : " ☐ I'M NOT A ROBOT 🤖";
            };

            this.AutoScroll = true;
        }

        private void SetPlaceholder(TextBox tb, string placeholder)
        {
            tb.Text = placeholder;
            tb.ForeColor = Color.FromArgb(87, 83, 78);

            tb.Enter += (s, e) =>
            {
                if (tb.Text == placeholder)
                {
                    tb.Text = "";
                    tb.ForeColor = Color.FromArgb(214, 211, 209);
                }
            };

            tb.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = placeholder;
                    tb.ForeColor = Color.FromArgb(87, 83, 78);
                }
            };
        }

        private void SetPasswordPlaceholder(TextBox tb, string placeholder)
        {
            tb.Text = placeholder;
            tb.ForeColor = Color.FromArgb(87, 83, 78);
            tb.PasswordChar = '\0'; 

            tb.Enter += (s, e) =>
            {
                if (tb.Text == placeholder)
                {
                    tb.Text = "";
                    tb.ForeColor = Color.FromArgb(214, 211, 209);
                    tb.PasswordChar = '●'; // Bắt đầu ẩn mật khẩu khi user nhập
                }
            };

            tb.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = placeholder;
                    tb.ForeColor = Color.FromArgb(87, 83, 78);
                    tb.PasswordChar = '\0';
                }
            };
        }

        private void DrawLockIcon(PictureBox pb, string icon)
        {
            // Vẽ emoji icon khóa lên picturebox
            pb.Paint += (s, e) =>
            {
                e.Graphics.DrawString(
                    icon,
                    new Font("Segoe UI Emoji", 16),
                    new SolidBrush(Color.FromArgb(217, 119, 6)),
                    5,
                    5);
            };
        }

        // Override Show/Hide để gắn logic animation
        public new void Show()
        {
            this.Visible = true;
            this.StartAnimations();
            this.BringToFront();
        }

        public new void Hide()
        {
            this.StopAnimations();
            this.Visible = false;
        }

        #endregion

        #region Helpers
        private bool IsValidEmail(string email)
        {
            return validationService.IsValidEmail(email);
        }

        private bool IsValidPhone(string phone)
        {
            return validationService.IsValidPhone(phone);
        }

        private bool IsValidPassword(string password)
        {
            return validationService.IsValidPassword(password);
        }

        private void SetAllControl(bool set)
        {
            isProcessing = !set;
            btn_alreadyHaveAccount.Enabled = set;
            btn_register.Enabled = set;
        }

        #endregion

        #region Register Button Logic

        private async void btn_register_Click(object sender, EventArgs e)
        {

            if (isProcessing)
            {
                Console.WriteLine("⚠ Registration already in process, ignoring duplicate click.");
                return;
            }

            SetAllControl(false);

            try
            {
                // Clear error cũ
                lblUsernameError.Text = string.Empty;
                lblContactError.Text = string.Empty;
                lblPasswordError.Text = string.Empty;
                lblConfirmPasswordError.Text = string.Empty;
                lblRobotError.Text = string.Empty;

                string username = tb_username.Text.Trim();
                string contact = tb_contact.Text.Trim();
                string password = tb_password.Text;
                string confirm = tb_confirmPassword.Text;

                tb_password.Text = string.Empty;
                tb_confirmPassword.Text = string.Empty;

                // 1. Input Validation
                if (string.IsNullOrEmpty(username) || username == "ENTER USERNAME")
                {
                    lblUsernameError.Text = "⚠ Please enter your username";
                    tb_username.Focus();
                    MessageBox.Show(
                        "⚠ Please enter your username.",
                        "Validation error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(contact) || contact == "EMAIL OR PHONE")
                {
                    lblContactError.Text = "⚠ Please enter your username.";
                    tb_contact.Focus();
                    MessageBox.Show(
                        "⚠ Please enter your username.",
                        "Validation error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (!chkNotRobot.Checked)
                {
                    lblRobotError.Text = "⚠ Please verify the captcha.";
                    MessageBox.Show(
                        "⚠ Please verify the captcha.",
                        "Validation error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    chkNotRobot.Focus();
                    return;
                }
                chkNotRobot.Checked = false;

                bool isEmail = IsValidEmail(contact);
                bool isPhone = IsValidPhone(contact);

                if (!isEmail && !isPhone)
                {
                    lblContactError.Text = "⚠ Please enter a valid Email or Phone number.";
                    tb_contact.Focus();
                    MessageBox.Show(
                        "⚠ Please enter a valid Email or Phone number.",
                        "Validation error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (!IsValidPassword(password))
                {
                    lblPasswordError.Text = "⚠ Weak password. Must contain ≥8 chars, upper/lowercase, number, symbol.";
                    tb_password.Focus();
                    MessageBox.Show(
                        "⚠ Weak password. Must contain ≥8 chars, upper/lowercase, number, symbol.",
                        "Validation error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (password != confirm)
                {
                    lblConfirmPasswordError.Text = "⚠ Password confirmation does not match.";
                    tb_password.Focus();
                    MessageBox.Show(
                        "⚠ Password confirmation does not match.",
                        "Validation error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Xử lý đăng ký
                bool success = false;
                string message = "";

                var response = await tcpClient.RegisterAsync(
                    username,
                    isEmail ? contact : null,
                    isPhone ? contact : null,
                    password
                );

                success = response.Success;
                message = response.Message;

                if (success)
                {
                    MessageBox.Show(
                        "🎉 Registration Successful!\n\nWelcome, " + username + "!",
                        "✓ Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // Stop animations before switching
                    StopAnimations();

                    var loginForm = new FormDangNhap();
                    loginForm.FormClosed += (s, args) => this.Close();
                    loginForm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show(
                        message,
                        "❌ Registration Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Registration error: " + ex.Message);
                MessageBox.Show(
                    "❌ An error occurred during registration. Please try again later.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetAllControl(true);
            }
        }

        #endregion

        #region Switch Back to Login
        private void btn_alreadyHaveAccount_Click(object sender, EventArgs e)
        {

            if (isProcessing)
            {
                Console.WriteLine("⚠ Cannot switch to login while registration is in process.");
                return;
            }

            SetAllControl(false);

            try
            {
                Console.WriteLine("🎯 Already have account button CLICKED in FormDangKy!");


                StopAnimations();

                // Xử lý chuyển về form đăng nhập
                if (SwitchToLogin != null)
                {
                    Console.WriteLine("✅ SwitchToLogin event is connected, invoking...");
                    SwitchToLogin?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    Console.WriteLine("❌ ERROR: SwitchToLogin event is NULL!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error switching to login: " + ex.Message);
            }
            finally
            {
                SetAllControl(true);
            }
        }

        public void ResetForm()
        {
            // Reset textboxes về placeholder
            tb_username.Text = "ENTER USERNAME";
            tb_contact.Text = "EMAIL OR PHONE";
            tb_password.Text = "ENTER PASSWORD";
            tb_confirmPassword.Text = "CONFIRM PASSWORD";

            chkNotRobot.Checked = false;

            // Clear error labels
            lblUsernameError.Text = "";
            lblContactError.Text = "";
            lblPasswordError.Text = "";
            lblConfirmPasswordError.Text = "";
            lblRobotError.Text = "";

            Console.WriteLine("✅ FormDangKy reset completed!");
        }

        #endregion

        #region Animation
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopAnimations();
            base.OnFormClosing(e);
        }

        private void StopAnimations()
        {
            if (myTimer != null)
            {
                myTimer.Stop();
                myTimer.Dispose();
                myTimer = null;
            }
        }

        private void FormDangKy_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                StartAnimations();

                // Reset form state mỗi lần form hiển thị lại
                tb_username.Text = "ENTER USERNAME";
                tb_contact.Text = "EMAIL OR PHONE";
                tb_password.Text = "ENTER PASSWORD";
                tb_confirmPassword.Text = "CONFIRM PASSWORD";

                chkNotRobot.Checked = false;

                // Clear errors
                lblUsernameError.Text = "";
                lblContactError.Text = "";
                lblPasswordError.Text = "";
                lblConfirmPasswordError.Text = "";
                lblRobotError.Text = "";
            }
            else
            {
                StopAnimations();
            }
        }

        public void StartAnimations()
        {
            if (myTimer == null)
            {
                myTimer = new System.Windows.Forms.Timer();
                myTimer.Interval = 50; // animation refresh rate
                myTimer.Tick += MyTimer_Tick;
            }

            myTimer.Start();
        }

        private void MyTimer_Tick(object sender, EventArgs e)
        {
            pictureBox1.Left -= 2;
            if (pictureBox1.Right < 0)
                pictureBox1.Left = this.Width;
        }

        private void FormDangKy_Load(object sender, EventArgs e)
        {
            StartAnimations();
        }

        #endregion

        #region Live Validation (TextChanged)

        private void tb_username_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tb_username.Text.Trim()) || tb_username.Text == "ENTER USERNAME")
            {
                lblUsernameError.Text = "⚠ Please enter your username.";
            }
            else
            {
                lblUsernameError.Text = string.Empty;
            }
        }

        private void tb_contact_TextChanged(object sender, EventArgs e)
        {
            string contact = tb_contact.Text.Trim();

            if (string.IsNullOrEmpty(contact) || tb_contact.Text == "EMAIL OR PHONE")
            {
                lblContactError.Text = "⚠ Please enter a valid Email or Phone number.";
            }
            else if (!IsValidEmail(contact) && !IsValidPhone(contact))
            {
                lblContactError.Text = "⚠ Please enter a valid Email or Phone number.";
            }
            else
            {
                lblContactError.Text = string.Empty;
            }
        }

        private void tb_password_TextChanged(object sender, EventArgs e)
        {
            string password = tb_password.Text;

            if (!IsValidPassword(password) || tb_password.Text == "ENTER PASSWORD")
            {
                lblPasswordError.Text = "⚠ Weak password. Must contain ≥8 chars, upper/lowercase, number, symbol.";
            }
            else
            {
                lblPasswordError.Text = string.Empty;
            }

            if (!string.IsNullOrEmpty(tb_confirmPassword.Text) && tb_confirmPassword.Text != "CONFIRM PASSWORD")
            {
                if (tb_confirmPassword.Text != password)
                {
                    lblConfirmPasswordError.Text = "⚠ Password confirmation does not match.";
                }
                else
                {
                    lblConfirmPasswordError.Text = string.Empty;
                }
            }
        }

        private void tb_confirmPassword_TextChanged(object sender, EventArgs e)
        {
            if (tb_confirmPassword.Text != tb_password.Text)
            {
                lblConfirmPasswordError.Text = "⚠ Password confirmation does not match.";
            }
            else
            {
                lblConfirmPasswordError.Text = string.Empty;
            }
        }

        private void chkNotRobot_TextChanged(object sender, EventArgs e)
        {
            if (!chkNotRobot.Checked)
            {
                lblRobotError.Text = "⚠ Please verify the captcha.";
            }
        }

        #endregion

        #region Keyboard Shortcuts

        private void EnterNext_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                this.SelectNextControl(this.ActiveControl, true, true, true, true);
            }
        }

        private void tb_confirmPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btn_register_Click(sender, e);
            }
        }

        #endregion

        #region Password Visibility Toggle

        private void pictureBoxLock1_Click(object sender, EventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            if (isPasswordVisible)
                tb_password.PasswordChar = '\0';
            else
                tb_password.PasswordChar = '●';
        }

        private void pictureBoxLock2_Click(object sender, EventArgs e)
        {
            isConfirmPasswordVisible = !isConfirmPasswordVisible;

            if (isConfirmPasswordVisible)
                tb_confirmPassword.PasswordChar = '\0';
            else
                tb_confirmPassword.PasswordChar = '●';
        }

        #endregion
    }
}
