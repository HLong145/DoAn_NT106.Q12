using System;
using System.Windows.Forms;
using DoAn_NT106.Client;

namespace DoAn_NT106
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ✅ Initialize Sound Manager and UI Audio Wiring at startup
            SoundManager.Initialize();
            UIAudioWiring.Start();
            Console.WriteLine("🎵 UIAudioWiring started - all buttons will play sound");

            if (args.Length > 0 && args[0].Equals("--login", StringComparison.OrdinalIgnoreCase))
            {
                // Process này chỉ dùng cho client login/register
                // Có thể dùng FormManager để quản lý Login/Register
                FormManager.StartApplication();
            }
            else
            {
                // Process “launcher” mặc định → mở Dashboard
                Application.Run(new Dashboard());
            }
        }
    }

    public static class FormManager
    {
        public static void StartApplication()
        {
            var loginForm = new FormDangNhap();
            var registerForm = new FormDangKy();

            registerForm.Hide();

            loginForm.SwitchToRegister += (s, e) =>
            {
                Console.WriteLine("🔄 Switching to Register form...");
                loginForm.Hide();
                registerForm.Show();
                registerForm.BringToFront();
            };

            registerForm.SwitchToLogin += (s, e) =>
            {
                Console.WriteLine("🔄 Switching to Login form...");
                registerForm.Hide();
                registerForm.ResetForm();
                loginForm.Show();
                loginForm.BringToFront();
            };

            loginForm.FormClosed += (s, e) => Application.Exit();

            // Chạy message loop với loginForm là main form
            Application.Run(loginForm);
        }
    }
}
