using DoAn_NT106.Services;
using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;

namespace DoAn_NT106
{
    public partial class FormDangNhap : Form
    {
        #region Fields

        private int floatingOffset = 0;                        
        private Random random = new Random();
        private System.Windows.Forms.Timer floatingItemsTimer; 

        public string ReturnedUsername { get; private set; }   
        public string Token { get; private set; }         

        private readonly PersistentTcpClient tcpClient;      
        private ValidationService validationService = new ValidationService();

        private static bool isAutoLoginPerformed = false;    

        public event EventHandler SwitchToRegister;

        #endregion

        #region Constructor

        public FormDangNhap()
        {
            InitializeComponent();

            SetupFloatingAnimation();                         
            tcpClient = PersistentTcpClient.Instance;         
            tcpClient = PersistentTcpClient.Instance;
            this.Load += FormDangNhap_Load;
            tcpClient.OnDisconnected += HandleServerDisconnected;

            if (!isAutoLoginPerformed)
            {
                // Chỉ auto load login 1 lần trong vòng đời app
                this.Shown += async (sender, e) =>
                {
                    await LoadRememberedLoginAsync();
                };
            }
        }

        #endregion

        #region Secure Storage (Encrypt / Decrypt)
        //Remember Login 
        // Encrypt plain text using DPAPI (bind to current user)
        private string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return "";
            var bytes = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(plainText),
                null,
                DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(bytes);
        }

        // Decrypt text stored by Encrypt()
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
                // Nếu giải mã lỗi thì xem như không có dữ liệu
                return "";
            }
        }

        #endregion

        #region Form Lifecycle Overrides

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.BringToFront();
            this.Focus();
        }

        private async void FormDangNhap_Load(object sender, EventArgs e)
        {
            // Kiểm tra kết nối server ngay khi form load
            await CheckServerConnectionAsync();

            await ConnectionHelper.CheckConnectionOnLoadAsync(
            this,
            onSuccess: () => SetControlsEnabled(true),  // Enable lại khi thành công
            onFail: null
            );// Mặc định sẽ hiện dialog retry/cancel
        }

        private void HandleServerDisconnected(string message)
        {
            ConnectionHelper.HandleDisconnect(
                this,
                message,
                onRetrySuccess: () => SetControlsEnabled(true),
                onCancel: () => this.Close()
            );
        }

        private async Task CheckServerConnectionAsync()
        {
            // Disable các control trong khi kiểm tra
            SetControlsEnabled(false);
            this.Text = "Login - Checking Connection...";
            this.Cursor = Cursors.WaitCursor;

            try
            {
                Console.WriteLine("🔍 Checking server connection...");

                // Thử kết nối đến server, timeout 5 giây trong PersistentTcpClient
                bool isConnected = await tcpClient.ConnectAsync();

                if (isConnected)
                {
                    Console.WriteLine("✅ Server connection successful!");
                    this.Text = "Đăng Nhập";
                    SetControlsEnabled(true);
                }
                else
                {
                    Console.WriteLine("❌ Server connection failed!");
                    ShowConnectionError();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection check error: {ex.Message}");
                ShowConnectionError();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetControlsEnabled(enabled)));
                return;
            }

            btn_Login.Enabled = enabled;
            btn_Register.Enabled = enabled;
            btn_Forgot.Enabled = enabled;
            tb_Username.Enabled = enabled;
            tb_Password.Enabled = enabled;
            chk_Remember.Enabled = enabled;
            chk_ShowPassword.Enabled = enabled;
            chk_Captcha.Enabled = enabled;
        }

        private void ShowConnectionError()
        {
            this.Text = "Login - Unable to connect";

            var result = MessageBox.Show(
                "❌ Unable to connect to the server!\n\n" +
                "Please check:\n" +
                "• Is the server running?\n" +
                "• Is your network connection stable?\n" +
                "• Is the firewall blocking the connection?\n\n" +
                "Do you want to try again?",
                "⚠️ Connection Error",
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Retry)
            {
                // Thử kết nối lại
                _ = CheckServerConnectionAsync();
            }
            else
            {
                // Đóng form đăng nhập
                this.Close();
            }
        }


        #endregion

        #region Remember Me Auto Login

        private async Task LoadRememberedLoginAsync()
        {
            // Chỉ chạy nếu chưa auto-login trong session này
            if (isAutoLoginPerformed) return;

            if (Properties.Settings.Default.RememberMe)
            {
                string savedUsername = Properties.Settings.Default.SavedUsername;
                string savedPassword = Decrypt(Properties.Settings.Default.SavedPassword);
                string savedToken = Decrypt(Properties.Settings.Default.SavedToken);

                if (string.IsNullOrEmpty(savedUsername)) return;

                // Đổ lại vào textbox cho user xem
                tb_Username.Text = savedUsername;
                tb_Password.Text = savedPassword;

                if (string.IsNullOrEmpty(savedToken)) return;

                try
                {
                    // Verify token với server để auto-login
                    var verifyResponse = await tcpClient.VerifyTokenAsync(savedToken);
                    if (verifyResponse.Success)
                    {
                        string usernameFromToken = verifyResponse.GetDataValue("username");

                        MessageBox.Show(
                            $"🎉 Auto login successful!\n\nWelcome back {usernameFromToken}!",
                            "✅ Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        isAutoLoginPerformed = true;

                        this.Hide();
                        MainForm mainForm = new MainForm(usernameFromToken, savedToken);
                        mainForm.FormClosed += (s, args) =>
                        {
                            // Cho phép auto-login lại nếu mở lại form login
                            isAutoLoginPerformed = false;
                            this.Close();
                        };
                        mainForm.Show();
                        return;
                    }
                    else
                    {
                        // Token không còn hợp lệ xoá token đã lưu
                        Properties.Settings.Default.SavedToken = "";
                        Properties.Settings.Default.Save();

                        MessageBox.Show(
                            "Your session has expired. Please click Login button.",
                            "⚠ Session Expired",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Token verification failed: {ex.Message}");
                }
            }
        }

        #endregion

        #region Login Button Logic

        //Button Login
        private async void btn_Login_Click(object sender, EventArgs e)
        {
            string contact = tb_Username.Text.Trim();
            string password = tb_Password.Text;

            // Clear password field
            tb_Password.Text = string.Empty;

            // Kiểm tra thông tin login
            if (string.IsNullOrEmpty(contact) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show(
                    "Please fill in all required login information!",
                    "⚠ Missing Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra captcha
            if (!chk_Captcha.Checked)
            {
                MessageBox.Show(
                    "Please confirm that you are not a robot!",
                    "⚠ Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string username = contact;
            bool isEmail = IsValidEmail(contact);
            bool isPhone = IsValidPhone(contact);

            try
            {
                btn_Login.Enabled = false;

                // Gửi request login lên server
                var response = await tcpClient.LoginAsync(username, password);
                if (response.Success)
                {
                    string token = response.GetDataValue("token");
                    string returnedUsername = response.GetDataValue("username");

                    if (string.IsNullOrEmpty(token))
                    {
                        MessageBox.Show(
                            "Server did not return authentication token.",
                            "❌ Login Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
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

                    MessageBox.Show(
                        $"🎉 Login successful!\n\nWelcome {returnedUsername}!",
                        "✅ Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // Mở MainForm và đóng FormDangNhap
                    MainForm mainForm = new MainForm(returnedUsername, token);
                    mainForm.FormClosed += (s, args) => this.Close();
                    mainForm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show(
                        response.Message,
                        "❌ Login Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error while connecting to server:\n" + ex.Message,
                    "⚠ Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btn_Login.Enabled = true;
            }
        }

        #endregion

        #region Register / Forgot Password Navigation

        // Register button
        private void btn_Register_Click(object sender, EventArgs e)
        {
            Console.WriteLine("🎯 Register button CLICKED in FormDangNhap!");

            // tra sự kiện
            if (SwitchToRegister == null)
            {
                Console.WriteLine("❌ ERROR: SwitchToRegister event is NULL! Using fallback...");

                this.Hide();
                var registerForm = new FormDangKy();

                // Khi FormDangKy đưa SwitchToLogin thì đóng form đăng ký, show lại form login
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
            // Mở form quên mật khẩu, ẩn form hiện tại
            this.Hide();
            FormQuenPass formQuenPass = new FormQuenPass();
            formQuenPass.FormClosed += (s, args) => this.Show();
            formQuenPass.Show();
        }

        #endregion

        #region Helpers (Validation, UI)
    
        private bool IsValidEmail(string email)
        {
            return validationService.IsValidEmail(email);
        }

        private bool IsValidPhone(string phone)
        {
            return validationService.IsValidPhone(phone);
        }

        private void ShowPasswordCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // Toggle hiển thị/ẩn mật khẩu
            tb_Password.UseSystemPasswordChar = !chk_ShowPassword.Checked;
        }

        #endregion

        #region Floating Background Animation

        // Floating Animation
        private void SetupFloatingAnimation()
        {
            floatingItemsTimer = new System.Windows.Forms.Timer();
            floatingItemsTimer.Interval = 50;          // 20 FPS
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

        #endregion

        #region Form Closing

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = false;
            }

            if (tcpClient != null)
            {
                tcpClient.OnDisconnected -= HandleServerDisconnected;
            }

            base.OnFormClosing(e);
        }

        #endregion

        #region Keyboard Shortcuts

        private void chk_Remember_CheckedChanged(object sender, EventArgs e) { }

        private void tb_Username_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                this.SelectNextControl(this.ActiveControl, true, true, true, true);
            }
        }

        private void tb_Password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btn_Login_Click(sender, e);
            }
        }

        private void btn_Login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btn_Login_Click(sender, e);
            }
        }

        private void btn_Register_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btn_Register_Click(sender, e);
            }
        }

        private void btn_Forgot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btn_Forgot_Click(sender, e);
            }
        }

        #endregion
    }
}
