using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace DoAn_NT106.Client
{
    public partial class Dashboard : Form
    {
        //   TRACKING CHO CHILD PROCESSES
        private Process clientProcess;
        //  FLAG ĐỂ TRÁNH GỌI MESSAGEBOX NHIỀU LẦN
        private bool isClosing = false;

        public Dashboard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Khi đóng Dashboard → Thoát toàn bộ ứng dụng
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            //  KIỂM TRA FLAG ĐỂ TRÁNH GỌI NHIỀU LẦN
            if (isClosing)
            {
                e.Cancel = false;
                base.OnFormClosing(e);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to exit the application?", "⚠️ Exit Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true; // Hủy đóng form
            }
            else
            {
                //  SET FLAG ĐỂ TRÁNH MESSAGEBOX LẦN THỨ 2
                isClosing = true;
                e.Cancel = false;

                //  KILL CHILD PROCESS NẾU CÓ
                KillChildProcess();
                
                //  FORCE SHUTDOWN
                ForceShutdown();
            }

            base.OnFormClosing(e);
        }

        //  HỖ TRỢ FUNCTION: KILL CHILD PROCESS
        private void KillChildProcess()
        {
            try
            {
                if (clientProcess != null && !clientProcess.HasExited)
                {
                    Console.WriteLine($"🛑 Terminating child process (PID: {clientProcess.Id})...");
                    clientProcess.Kill();
                    clientProcess.WaitForExit(3000); // Chờ 3 giây
                    clientProcess?.Dispose();
                    clientProcess = null;
                    Console.WriteLine(" Child process terminated");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error killing child process: {ex.Message}");
            }
        }

        //  HỖ TRỢ FUNCTION: FORCE SHUTDOWN
        private void ForceShutdown()
        {
            Console.WriteLine("🛑 Force shutdown initiated...");
            
            //  Tìm và kill tất cả child processes
            try
            {
                foreach (var process in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
                {
                    if (process.Id != Process.GetCurrentProcess().Id)
                    {
                        try
                        {
                            Console.WriteLine($"🛑 Killing process: {process.ProcessName} (PID: {process.Id})");
                            process.Kill();
                        }
                        catch { }
                    }
                }
            }
            catch { }

            //  FORCE EXIT - Không dùng Application.Exit() vì nó sẽ trigger OnFormClosing lần nữa
            Console.WriteLine("🛑 Force exit now");
            Environment.Exit(0);
        }

        private void btn_Client_Click(object sender, EventArgs e)
        {
            try
            {
                DoAn_NT106.Client.FormDangNhap loginForm = new DoAn_NT106.Client.FormDangNhap();
                loginForm.Show();
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
                //  MỞ SERVER FORM

                DoAn_NT106.Server.ServerForm serverForm = new DoAn_NT106.Server.ServerForm();
                serverForm.Show();

                MessageBox.Show("Server window opened!\n\nClick 'Start' to begin listening for connections on port 8080.",
                    "🖥️ Server Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //  KHI SERVER ĐÓNG → HIỆN LẠI DASHBOARD
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

        /// <summary>
        /// Khi rời khỏi textbox IP - cập nhật AppConfig
        /// </summary>
        private void txtServerIP_Leave(object sender, EventArgs e)
        {
            string ip = txtServerIP.Text.Trim();

            if (!string.IsNullOrEmpty(ip))
            {
                AppConfig.SERVER_IP = ip;
                Console.WriteLine($"[Dashboard] ✅ Server IP set to: {ip}");
            }
        }
    }

    /// <summary>
    /// Controller quản lý Client flow (Login/Register)
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

            //  TẠO VÀ KẾT NỐI FORM NGAY LẬP TỨC
            InitializeAndConnectForms();

            // Hiển thị form đăng nhập
            ShowLoginForm();
        }

        //  PHƯƠNG THỨC: KHỞI TẠO VÀ KẾT NỐI FORM
        private void InitializeAndConnectForms()
        {
            // Tạo form đăng nhập
            loginForm = new FormDangNhap();
            loginForm.StartPosition = FormStartPosition.CenterScreen;

            // Tạo form đăng ký
            registerForm = new FormDangKy();
            registerForm.StartPosition = FormStartPosition.CenterScreen;

            Console.WriteLine("🔗 Connecting events...");

            //  KẾT NỐI SỰ KIỆN: Login → Register
            loginForm.SwitchToRegister += (s, e) =>
            {
                Console.WriteLine("🔄 Switching to Register form from Login...");
                loginForm.Hide();
                registerForm.ResetForm();
                registerForm.Show();
                registerForm.BringToFront();
            };

            //  KẾT NỐI SỰ KIỆN: Register → Login  
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

            Console.WriteLine(" Events connected successfully!");
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