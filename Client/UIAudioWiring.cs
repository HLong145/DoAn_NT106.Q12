using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DoAn_NT106
{
    /// <summary>Globally attaches button click sound to all buttons on all open forms.</summary>
    public static class UIAudioWiring
    {
        private static readonly HashSet<int> _wiredControls = new HashSet<int>();
        private static System.Windows.Forms.Timer _scanTimer;

        public static void Start()
        {
            if (_scanTimer != null) return;
            _scanTimer = new System.Windows.Forms.Timer { Interval = 800 };
            _scanTimer.Tick += (s, e) => ScanOpenForms();
            _scanTimer.Start();
            ScanOpenForms();
        }

        public static void Stop()
        {
            try
            {
                if (_scanTimer != null)
                {
                    _scanTimer.Stop();
                    _scanTimer.Dispose();
                    _scanTimer = null;
                    Console.WriteLine("🛑 UIAudioWiring timer stopped and disposed");
                }
                _wiredControls.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error stopping UIAudioWiring: {ex.Message}");
            }
        }

        private static void ScanOpenForms()
        {
            try
            {
                foreach (Form f in Application.OpenForms)
                {
                    WireRecursive(f);
                }
            }
            catch { }
        }

        private static void WireRecursive(Control c)
        {
            if (c == null) return;
            int id = c.GetHashCode();
            if (!_wiredControls.Contains(id))
            {
                if (c is Button btn)
                {
                    btn.Click -= ButtonPlaySound_Click;
                    // Insert handler at beginning of invocation list by reordering: remove others then reattach
                    btn.Click += ButtonPlaySound_Click;
                }
                _wiredControls.Add(id);
            }
            foreach (Control child in c.Controls)
            {
                WireRecursive(child);
            }
        }

        private static void ButtonPlaySound_Click(object sender, EventArgs e)
        {
            try { SoundManager.PlaySound(Client.SoundEffect.ButtonClick); } catch { }
        }
    }
}
