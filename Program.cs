using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DoAn_NT106.Client;
using DoAn_NT106.Client.Class;

namespace DoAn_NT106
{
    internal static class Program
    {
        private static bool isShuttingDown = false;
        private static readonly object shutdownLock = new object();
        private static System.Windows.Forms.Timer shutdownCheckTimer;
        private static int pendingCloseCount = 0;

        [STAThread]
        static void Main(string[] args)
        {
            // GLOBAL EXCEPTION HANDLERS
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ConnectionHelper.Initialize();

            try
            {
                // Initialize Sound Manager and UI Audio Wiring at startup
                SoundManager.Initialize();
                UIAudioWiring.Start();
                Console.WriteLine("🎵 UIAudioWiring started - all buttons will play sound");

                // Start UI styling enforcer
                UIStyling.Start();

                // KHỞI TẠO TIMER KIỂM TRA SHUTDOWN (delay 500ms)
                shutdownCheckTimer = new System.Windows.Forms.Timer();
                shutdownCheckTimer.Interval = 500;
                shutdownCheckTimer.Tick += ShutdownCheckTimer_Tick;

                // HOOK VÀO TẤT CẢ FORMS ĐƯỢC TẠO
                Application.Idle += Application_Idle;

                Console.WriteLine("🌐 Opening IP Configuration...");


                // Xoá khi build với Internet
                using (var ipForm = new FormIPConfig())
                {
                    DialogResult result = ipForm.ShowDialog();

                    if (result != DialogResult.OK || !ipForm.IsConfirmed)
                    {
                        // User đã cancel → thoát app
                        Console.WriteLine("❌ User cancelled IP configuration. Exiting...");
                        return;
                    }

                    Console.WriteLine($"IP configured: {AppConfig.SERVER_IP}");
                }
                //

                Console.WriteLine("🚀 Starting Login Form...");
                Application.Run(new FormDangNhap());

                // Cmt dòng trên và bỏ cmt dòng dưới khi build với Internet
                //Application.Run(new FormDangNhao()); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fatal error: {ex.Message}");
                MessageBox.Show($"Fatal error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // THEO DÕI TẤT CẢ FORMS - HOOK EVENTS
        private static void Application_Idle(object sender, EventArgs e)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form.Tag?.ToString() != "Hooked")
                {
                    form.Tag = "Hooked";
                    form.FormClosed += AnyForm_FormClosed;
                    form.VisibleChanged += AnyForm_VisibleChanged;
                    Console.WriteLine($"📋 Hooked form: {form.Name} ({form.GetType().Name})");
                }
            }
        }

        // KHI FORM ẨN ĐI (this.Hide()) - KHÔNG SHUTDOWN NGAY
        private static void AnyForm_VisibleChanged(object sender, EventArgs e)
        {
            if (isShuttingDown) return;

            Form form = sender as Form;
            if (form != null && !form.Visible)
            {
                Console.WriteLine($"👁️ Form hidden: {form.Name} ({form.GetType().Name})");
                // Không làm gì - chỉ log
            }
        }

        // KHI FORM ĐÓNG (this.Close()) - DELAY KIỂM TRA
        private static void AnyForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (isShuttingDown) return;

            Form closedForm = sender as Form;
            Console.WriteLine($"🚪 Form closed: {closedForm?.Name} ({closedForm?.GetType().Name}), Reason: {e.CloseReason}");

            // RESET VÀ START TIMER - delay kiểm tra để form mới có thời gian show
            pendingCloseCount++;
            shutdownCheckTimer.Stop();
            shutdownCheckTimer.Start();
        }

        // TIMER TICK - KIỂM TRA SAU KHI DELAY
        private static void ShutdownCheckTimer_Tick(object sender, EventArgs e)
        {
            shutdownCheckTimer.Stop();

            if (isShuttingDown) return;

            Console.WriteLine($"⏰ Checking forms after delay... (pending closes: {pendingCloseCount})");
            pendingCloseCount = 0;

            // KIỂM TRA CÒN FORM VISIBLE KHÔNG
            bool hasVisibleForm = false;
            int totalForms = Application.OpenForms.Count;

            foreach (Form form in Application.OpenForms)
            {
                if (!form.IsDisposed)
                {
                    Console.WriteLine($"   📋 Form: {form.Name} ({form.GetType().Name}) - Visible: {form.Visible}");
                    if (form.Visible)
                    {
                        hasVisibleForm = true;
                    }
                }
            }

            Console.WriteLine($"   📊 Total forms: {totalForms}, Has visible: {hasVisibleForm}");

            // NẾU KHÔNG CÒN FORM VISIBLE → SHUTDOWN
            if (!hasVisibleForm)
            {
                Console.WriteLine("⚠️ No visible forms remaining - initiating shutdown...");
                ForceShutdown();
            }
        }

        // GLOBAL UNHANDLED EXCEPTION HANDLER
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            Console.WriteLine($"❌ Unhandled exception: {ex?.Message}");
            ForceShutdown();
        }

        // GLOBAL THREAD EXCEPTION HANDLER
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Console.WriteLine($"❌ Thread exception: {e.Exception?.Message}");
        }

        // PROCESS EXIT EVENT
        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("📤 Process exit event");
            CleanupResources();
        }

        // CLEANUP TẤT CẢ RESOURCES
        private static void CleanupResources()
        {
            try
            {
                Console.WriteLine("🧹 Cleaning up resources...");

                // Stop timer
                try
                {
                    shutdownCheckTimer?.Stop();
                    shutdownCheckTimer?.Dispose();
                }
                catch { }

                // Disconnect PersistentTcpClient (quan trọng nhất)
                try
                {
                    PersistentTcpClient.Instance.Disconnect();
                    Console.WriteLine("✅ PersistentTcpClient disconnected");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ PersistentTcpClient cleanup error: {ex.Message}");
                }

                // Stop sound manager
                try
                {
                    SoundManager.StopMusic();
                    SoundManager.Cleanup();
                    Console.WriteLine("✅ SoundManager cleaned up");
                }
                catch { }

                // Stop UI Audio wiring
                try
                {
                    UIAudioWiring.Stop();
                    Console.WriteLine("✅ UIAudioWiring stopped");
                }
                catch { }

                // Stop UI styling helper
                try
                {
                    UIStyling.Stop();
                    Console.WriteLine("✅ UIStyling stopped");
                }
                catch { }

                Console.WriteLine("✅ Cleanup complete");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Cleanup error: {ex.Message}");
            }
        }

        // ĐÓNG TẤT CẢ FORMS
        private static void CloseAllForms()
        {
            try
            {
                var formsToClose = Application.OpenForms.Cast<Form>().ToList();

                foreach (Form form in formsToClose)
                {
                    try
                    {
                        if (!form.IsDisposed)
                        {
                            Console.WriteLine($"🔒 Force closing form: {form.Name} ({form.GetType().Name})");
                            form.Close();
                            form.Dispose();
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error closing forms: {ex.Message}");
            }
        }

        // FORCE SHUTDOWN
        public static void ForceShutdown()
        {
            lock (shutdownLock)
            {
                if (isShuttingDown) return;
                isShuttingDown = true;
            }

            Console.WriteLine("🛑 Force shutdown initiated...");

            // ĐÓNG TẤT CẢ FORMS
            CloseAllForms();

            // CLEANUP RESOURCES
            CleanupResources();

            // KILL CHILD PROCESSES
            try
            {
                string currentProcessName = Process.GetCurrentProcess().ProcessName;
                int currentProcessId = Process.GetCurrentProcess().Id;

                foreach (var process in Process.GetProcessesByName(currentProcessName))
                {
                    if (process.Id != currentProcessId)
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
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error killing child processes: {ex.Message}");
            }

            // FORCE EXIT
            Console.WriteLine("🛑 Force exit now");
            Environment.Exit(0);
        }
    }
}