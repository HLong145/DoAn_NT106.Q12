using System;
using System.Windows.Forms;
using DoAn_NT106.Client;

namespace DoAn_NT106
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Dashboard());
        }
    }

    // ✅ FORM MANAGER ĐƠN GIẢN
    public static class FormManager
    {
        public static void StartApplication()
        {
            var loginForm = new FormDangNhap();
            var registerForm = new FormDangKy();

            // Ẩn form đăng ký ban đầu
            registerForm.Hide();

            // ✅ KẾT NỐI SỰ KIỆN: Login → Register
            loginForm.SwitchToRegister += (s, e) =>
            {
                Console.WriteLine("🔄 Switching to Register form...");
                loginForm.Hide();
                registerForm.Show();
                registerForm.BringToFront();
            };

            // ✅ KẾT NỐI SỰ KIỆN: Register → Login  
            registerForm.SwitchToLogin += (s, e) =>
            {
                Console.WriteLine("🔄 Switching to Login form...");
                registerForm.Hide();
                registerForm.ResetForm();
                loginForm.Show();
                loginForm.BringToFront();
            };

            // Khi đóng form đăng nhập thì thoát app
            loginForm.FormClosed += (s, e) => Application.Exit();

            // Hiển thị form đăng nhập
            loginForm.Show();

            Application.Run();
        }
    }
}