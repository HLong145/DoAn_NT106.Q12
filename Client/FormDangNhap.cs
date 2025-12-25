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
using DoAn_NT106.Client.Class;

namespace DoAn_NT106.Client
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

        private bool isProcessing = false;

        #endregion

        #region Constructor

        public FormDangNhap()
        {
            InitializeComponent();

            SetupFloatingAnimation();                         
            tcpClient = PersistentTcpClient.Instance;         
            tcpClient = PersistentTcpClient.Instance;
            this.Load += FormDangNhap_Load;

            ConnectionHelper.OnReconnected += OnServerReconnected;

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
            await ConnectionHelper.CheckConnectionOnLoadAsync(
            this,
            onSuccess: () => SetControlsEnabled(true),  // Enable lại khi thành công
            onFail: null
            );// Mặc định sẽ hiện dialog retry/cancel
        }

        private void OnServerReconnected()
        {
            // Chỉ xử lý nếu form này đang visible
            if (!this.Visible || this.IsDisposed) return;

            Console.WriteLine("[FormDangNhap] 🔄 Server reconnected");

            // Enable lại controls nếu form đang hiển thị
            SetControlsEnabled(true);
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

            if (isProcessing)
                return; // Tránh việc nhấn nhiều lần

            SetAllControl(false);

            try
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
                SetAllControl(true);
            }
        }

        #endregion

        #region Register / Forgot Password Navigation

        // Register button
        private void btn_Register_Click(object sender, EventArgs e)
        {
            if (isProcessing)
                return; // Tránh việc nhấn nhiều lần

            SetAllControl(false);

            try
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
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error while opening registration form:\n" + ex.Message,
                    "⚠ Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetAllControl(true);
            }
        }

        private void btn_Forgot_Click(object sender, EventArgs e)
        {
            if (isProcessing) return;
            SetAllControl(false);

            try
            {
                // Mở form quên mật khẩu, ẩn form hiện tại
                this.Hide();
                FormQuenPass formQuenPass = new FormQuenPass();
                formQuenPass.FormClosed += (s, args) => this.Show();
                formQuenPass.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error while opening forgot password form:\n" + ex.Message,
                    "⚠ Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetAllControl(true);
            }   
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

        private void SetAllControl (bool set)
        {
            isProcessing = !set;
            btn_Forgot.Enabled = set;
            btn_Login.Enabled = set;
            btn_Register.Enabled = set;
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

            ConnectionHelper.OnReconnected -= OnServerReconnected;

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
