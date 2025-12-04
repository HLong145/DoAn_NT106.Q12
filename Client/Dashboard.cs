using ServerApp;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace DoAn_NT106.Client
{
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();

        }

        /// <summary>
        /// Mở Client mode (Login/Register)
        /// </summary>

        /// <summary>
        /// Khi đóng Dashboard → Thoát toàn bộ ứng dụng
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit the application?", "⚠️ Exit Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true; // Hủy đóng form
            }

            base.OnFormClosing(e); // Gọi base
        }

        private void btn_Client_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("Starting NEW Client Login process...");

                Process.Start(new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,   // exe hiện tại
                    Arguments = "--login",                  // báo cho Program.cs biết là chạy login
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting client process: " + ex.Message);
                MessageBox.Show(
                    "Error starting client: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private void btn_Server_Click(object sender, EventArgs e)
        {
            try
            {
                // ✅ MỞ SERVER FORM
                ServerForm serverForm = new ServerForm();
                serverForm.Show();

                MessageBox.Show("Server window opened!\n\nClick 'Start' to begin listening for connections on port 8080.",
                    "🖥️ Server Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // ✅ KHI SERVER ĐÓNG → HIỆN LẠI DASHBOARD
                serverForm.FormClosed += (s, args) =>
                {
                    this.Show();
                    this.BringToFront();
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting server: {ex.Message}",
                    "❌ Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// Controller quản lý Client flow (Login/Register)
    /// </summary>
    /// <summary>
    /// Controller quản lý Client flow (Login/Register) - BẢN ĐÃ SỬA
    /// </summary>
    public class ClientApplicationController
    {
        private FormDangNhap loginForm;
        private FormDangKy registerForm;
        private Dashboard dashboardForm;

        public ClientApplicationController(Dashboard dashboard)
        {
            dashboardForm = dashboard;
            dashboard.Hide();

            Console.WriteLine("🎯 Initializing ClientApplicationController...");

            // ✅ TẠO VÀ KẾT NỐI FORM NGAY LẬP TỨC
            InitializeAndConnectForms();

            // Hiển thị form đăng nhập
            ShowLoginForm();
        }

        // ✅ PHƯƠNG THỨC MỚI: KHỞI TẠO VÀ KẾT NỐI FORM
        private void InitializeAndConnectForms()
        {
            // Tạo form đăng nhập
            loginForm = new FormDangNhap();
            loginForm.StartPosition = FormStartPosition.CenterScreen;

            // Tạo form đăng ký
            registerForm = new FormDangKy();
            registerForm.StartPosition = FormStartPosition.CenterScreen;

            Console.WriteLine("🔗 Connecting events...");

            // ✅ KẾT NỐI SỰ KIỆN: Login → Register
            loginForm.SwitchToRegister += (s, e) =>
            {
                Console.WriteLine("🔄 Switching to Register form from Login...");
                loginForm.Hide();
                registerForm.ResetForm();
                registerForm.Show();
                registerForm.BringToFront();
            };

            // ✅ KẾT NỐI SỰ KIỆN: Register → Login  
            registerForm.SwitchToLogin += (s, e) =>
            {
                Console.WriteLine("🔄 Switching to Login form from Register...");
                registerForm.Hide();
                loginForm.Show();
                loginForm.BringToFront();
            };

            // Kết nối sự kiện đóng form
            loginForm.FormClosed += (s, e) =>
            {
                Console.WriteLine("🚪 Login form closed");
                registerForm?.Close();
                dashboardForm?.Show();
                dashboardForm?.BringToFront();
            };

            registerForm.FormClosed += (s, e) =>
            {
                Console.WriteLine("🚪 Register form closed");
                if (!loginForm.Visible)
                {
                    loginForm.Show();
                }
            };

            Console.WriteLine("✅ Events connected successfully!");
        }

        private void ShowLoginForm()
        {
            Console.WriteLine("👤 Showing Login form...");
            loginForm.Show();
            loginForm.BringToFront();
        }

        private void ShowRegisterForm()
        {
            Console.WriteLine("📝 Showing Register form...");
            registerForm.ResetForm();
            registerForm.Show();
            registerForm.BringToFront();
        }
    }
}