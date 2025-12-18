using System;
using System.Linq;
using System.Windows.Forms;

namespace DoAn_NT106.Client.Class
{
    // Runtime helper: enforce no window border and no control box for all opened forms.
    // This is a non-invasive approach that scans Application.OpenForms periodically
    // and applies the properties so designer-created forms are also updated at runtime.
    public static class UIStyling
    {
        private static System.Windows.Forms.Timer _timer;

        public static void Start()
        {
            if (_timer != null) return;
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 250; // 4 times per second
            _timer.Tick += (s, e) =>
            {
                try
                {
                    foreach (Form f in Application.OpenForms.Cast<Form>().ToArray())
                    {
                        if (f == null || f.IsDisposed) continue;
                        // Skip system dialogs and specific main forms we want to keep
                        var typeName = f.GetType().Name;
                        if (string.Equals(typeName, "Dashboard", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(typeName, "ServerForm", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(typeName, "FormDangNhap", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(typeName, "FormDangKy", StringComparison.OrdinalIgnoreCase))
                        {
                            // preserve original border and control box for these forms
                            continue;
                        }

                        if (f.FormBorderStyle != FormBorderStyle.None)
                            f.FormBorderStyle = FormBorderStyle.None;
                        if (f.ControlBox)
                            f.ControlBox = false;
                    }
                }
                catch { /* swallow to avoid crashing UI */ }
            };
            _timer.Start();
        }

        public static void Stop()
        {
            try
            {
                _timer?.Stop();
                _timer?.Dispose();
            }
            catch { }
            finally { _timer = null; }
        }
    }
}
