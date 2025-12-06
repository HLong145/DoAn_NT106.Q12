using DoAn_NT106.Services;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;

namespace DoAn_NT106
{
    public partial class FormDangNhap : Form
    {
        private int floatingOffset = 0;
        private Random random = new Random();
        private System.Windows.Forms.Timer floatingItemsTimer;
        public string ReturnedUsername { get; private set; }
        public string Token { get; private set; }

        private readonly PersistentTcpClient tcpClient;
        private readonly DatabaseService dbService = new DatabaseService();
        private static bool isAutoLoginPerformed = false;

        // ✅ QUAN TRỌNG: Sự kiện này PHẢI được khai báo
        public event EventHandler SwitchToRegister;

        public FormDangNhap()
        {
            InitializeComponent();
            SetupFloatingAnimation();

            tcpClient = PersistentTcpClient.Instance;
            if (!isAutoLoginPerformed)
            {
                this.Shown += async (sender, e) =>
                {
                    await LoadRememberedLoginAsync();
                };
            }
        }

        // =========================
        // ✅ Remember Login (ASYNC)
        // =========================
        private string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return "";
            var bytes = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(plainText),
                null,
                DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(bytes);
        }

        private string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return "";
            try
            {
                var bytes = ProtectedData.Unprotect(
                    Convert.FromBase64String(cipherText),
                    null,
                    DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return "";
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.BringToFront();
            this.Focus();
        }

        private async Task LoadRememberedLoginAsync()
        {
            if (isAutoLoginPerformed) return;

            if (Properties.Settings.Default.RememberMe)
            {
                string savedUsername = Properties.Settings.Default.SavedUsername;
                string savedPassword = Decrypt(Properties.Settings.Default.SavedPassword);
                string savedToken = Decrypt(Properties.Settings.Default.SavedToken);

                if (string.IsNullOrEmpty(savedUsername)) return;

                tb_Username.Text = savedUsername;
                tb_Password.Text = savedPassword;

                if (string.IsNullOrEmpty(savedToken)) return;

                try
                {
                    var verifyResponse = await tcpClient.VerifyTokenAsync(savedToken);
                    if (verifyResponse.Success)
                    {
                        string usernameFromToken = verifyResponse.GetDataValue("username");
                        MessageBox.Show($"🎉 Auto login successful!\n\nWelcome back {usernameFromToken}!",
                            "✅ Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        isAutoLoginPerformed = true;
                        this.Hide();
                        MainForm mainForm = new MainForm(usernameFromToken, savedToken);
                        mainForm.FormClosed += (s, args) =>
                        {
                            isAutoLoginPerformed = false;
                            this.Close();
                        };
                        mainForm.Show();
                        return;
                    }
                    else
                    {
                        Properties.Settings.Default.SavedToken = "";
                        Properties.Settings.Default.Save();
                        MessageBox.Show("Your session has expired. Please click Login button.",
                            "⚠ Session Expired", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Token verification failed: {ex.Message}");
                }
            }
        }

        // =========================
        // ✅ Button Login (ASYNC)
        // =========================
        private async void btn_Login_Click(object sender, EventArgs e)
        {
            string contact = tb_Username.Text.Trim();
            string password = tb_Password.Text;

            // Kiểm tra captcha
            if (!chk_Captcha.Checked)
            {
                MessageBox.Show("Please confirm that you are not a robot!",
                    "⚠ Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra thông tin login
            if (string.IsNullOrEmpty(contact) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill in all required login information!",
                    "⚠ Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string username = contact;
            bool isEmail = IsValidEmail(contact);
            bool isPhone = IsValidPhone(contact);

            // Nếu nhập email hoặc phone, tìm username
            if (isEmail || isPhone)
            {
                username = dbService.GetUsernameByContact(contact, isEmail);
                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("No account found for this information.",
                        "❌ Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            try
            {
                btn_Login.Enabled = false;
                var response = await tcpClient.LoginAsync(username, password);

                if (response.Success)
                {
                    string token = response.GetDataValue("token");
                    string returnedUsername = response.GetDataValue("username");

                    if (string.IsNullOrEmpty(token))
                    {
                        MessageBox.Show("Server did not return authentication token.",
                            "❌ Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Lưu RememberMe
                    if (chk_Remember.Checked)
                    {
                        Properties.Settings.Default.RememberMe = true;
                        Properties.Settings.Default.SavedUsername = returnedUsername;
                        Properties.Settings.Default.SavedPassword = Encrypt(password);
                        Properties.Settings.Default.SavedToken = Encrypt(token);
                    }
                    else
                    {
                        Properties.Settings.Default.RememberMe = false;
                        Properties.Settings.Default.SavedUsername = "";
                        Properties.Settings.Default.SavedPassword = "";
                        Properties.Settings.Default.SavedToken = "";
                    }
                    Properties.Settings.Default.Save();

                    MessageBox.Show($"🎉 Login successful!\n\nWelcome {returnedUsername}!",
                        "✅ Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Mở MainForm và đóng FormDangNhap
                    MainForm mainForm = new MainForm(returnedUsername, token);
                    mainForm.FormClosed += (s, args) => this.Close();
                    mainForm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show(response.Message,
                        "❌ Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while connecting to server:\n" + ex.Message,
                    "⚠ Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btn_Login.Enabled = true;
            }
        }

        // =========================
        // ✅ QUAN TRỌNG: Sửa sự kiện Register button
        // =========================
        private void btn_Register_Click(object sender, EventArgs e)
        {
            Console.WriteLine("🎯 Register button CLICKED in FormDangNhap!");

            // ✅ DEBUG: Kiểm tra sự kiện
            if (SwitchToRegister == null)
            {
                Console.WriteLine("❌ ERROR: SwitchToRegister event is NULL! Using fallback...");

                // FALLBACK: Chuyển form thủ công
                this.Hide();
                var registerForm = new FormDangKy();
                registerForm.SwitchToLogin += (s2, e2) =>
                {
                    registerForm.Close();
                    this.Show();
                };
                registerForm.Show();
            }
            else
            {
                Console.WriteLine("✅ SwitchToRegister event is connected, invoking...");
                SwitchToRegister?.Invoke(this, EventArgs.Empty);
            }
        }

        private void btn_Forgot_Click(object sender, EventArgs e)
        {
            this.Hide();
            FormQuenPass formQuenPass = new FormQuenPass();
            formQuenPass.FormClosed += (s, args) => this.Show();
            formQuenPass.Show();
        }

        // =========================
        // Helpers
        // =========================
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
            return Regex.IsMatch(phone, @"^0\d{9}$");
        }

        private void ShowPasswordCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            tb_Password.UseSystemPasswordChar = !chk_ShowPassword.Checked;
        }

        // =========================
        // Floating Animation
        // =========================
        private void SetupFloatingAnimation()
        {
            floatingItemsTimer = new System.Windows.Forms.Timer();
            floatingItemsTimer.Interval = 50;
            floatingItemsTimer.Tick += FloatingItemsTimer_Tick;
            floatingItemsTimer.Start();
        }

        private void FloatingItemsTimer_Tick(object sender, EventArgs e)
        {
            floatingOffset += 2;
            if (floatingOffset > this.Height + 50)
                floatingOffset = -50;
            this.Invalidate();
        }

        private void FormDangNhap_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            for (int i = 0; i < 8; i++)
            {
                int x = (i * 15 + 10) * this.Width / 100;
                int y = (floatingOffset + (i * 80)) % (this.Height + 100) - 50;

                using (SolidBrush brush = new SolidBrush(Color.Gold))
                    g.FillRectangle(brush, x, y, 12, 12);

                using (Pen pen = new Pen(Color.Black, 2))
                    g.DrawRectangle(pen, x, y, 12, 12);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = false;
            }
            base.OnFormClosing(e);
        }

        private void chk_Remember_CheckedChanged(object sender, EventArgs e) { }
    }
}