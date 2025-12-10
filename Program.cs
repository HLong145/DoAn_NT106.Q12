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
            // ✅ THÊM GLOBAL EXCEPTION HANDLER
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ThreadException += Application_ThreadException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
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
                    // Process "launcher" mặc định → mở Dashboard
                    Application.Run(new Dashboard());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fatal error: {ex}");
                MessageBox.Show($"Fatal error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // ✅ ENSURE COMPLETE CLEANUP
                CleanupAndExit();
            }
        }

        // ✅ GLOBAL UNHANDLED EXCEPTION HANDLER
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            Console.WriteLine($"❌ Unhandled exception: {ex?.Message}");
            CleanupAndExit();
        }

        // ✅ GLOBAL THREAD EXCEPTION HANDLER
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Console.WriteLine($"❌ Thread exception: {e.Exception?.Message}");
        }

        // ✅ CLEANUP AND FORCE EXIT
        private static void CleanupAndExit()
        {
            try
            {
                Console.WriteLine("🧹 Cleaning up resources...");
                
                // ✅ Stop sound manager
                SoundManager.Cleanup();

                // ✅ Stop UI Audio wiring
                UIAudioWiring.Stop();

                Console.WriteLine("✅ Cleanup complete");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Cleanup error: {ex.Message}");
            }
            finally
            {
                // ✅ FORCE EXIT - Không chấp nhận bất kỳ background thread nào
                Console.WriteLine("🛑 Force exit now");
                Environment.Exit(0);
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
