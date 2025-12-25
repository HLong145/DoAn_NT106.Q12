using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoAn_NT106.Client.Class
{
    public static class ConnectionHelper
    {
        private static bool _isShowingDisconnectDialog = false;
        private static readonly object _lock = new object();
        private static bool _isSubscribed = false; 

        // Event để các form subscribe và tự reconnect services
        public static event Action OnReconnected;

        /// <summary>
        /// Khởi tạo ConnectionHelper - gọi 1 lần khi app start
        /// </summary>
        public static void Initialize()
        {
            if (_isSubscribed) return;

            PersistentTcpClient.Instance.OnDisconnected += OnTcpDisconnected;
            _isSubscribed = true;
            Console.WriteLine("[ConnectionHelper] ✅ Initialized and subscribed to OnDisconnected");
        }

        /// <summary>
        /// Xử lý khi TCP disconnect - tìm form visible và hiện dialog
        /// </summary>
        private static void OnTcpDisconnected(string message)
        {
            Console.WriteLine($"[ConnectionHelper] 🔴 TCP Disconnected: {message}");

            try
            {
                if (Application.OpenForms.Count > 0)
                {
                    Form anyForm = Application.OpenForms[0];

                    if (anyForm.InvokeRequired)
                    {
                        anyForm.BeginInvoke(new Action(() => ShowDisconnectDialog(message)));
                    }
                    else
                    {
                        ShowDisconnectDialog(message);
                    }
                }
                else
                {
                    Console.WriteLine("[ConnectionHelper] No forms available!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConnectionHelper] OnTcpDisconnected error: {ex.Message}");
            }
        }

        private static void ShowDisconnectDialog(string message)
        {
            Form activeForm = GetActiveVisibleForm();
            if (activeForm == null)
            {
                Console.WriteLine("[ConnectionHelper] No visible form found!");
                return;
            }

            HandleDisconnect(activeForm, message, null, null);
        }


        /// <summary>
        /// Tìm form đang visible và active
        /// </summary>
        private static Form GetActiveVisibleForm()
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f.Visible && !f.IsDisposed)
                {
                    return f;
                }
            }
            return null;
        }

        /// <summary>
        /// Xử lý khi mất kết nối - hiển thị dialog Retry/Cancel
        /// </summary>
        public static void HandleDisconnect(
            Form form,
            string message,
            Action onRetrySuccess = null,
            Action onCancel = null)
        {
            if (form == null || form.IsDisposed) return;

            // Đảm bảo chạy trên UI thread
            if (form.InvokeRequired)
            {
                try
                {
                    form.Invoke(new Action(() => HandleDisconnect(form, message, onRetrySuccess, onCancel)));
                }
                catch { }
                return;
            }

            // Nếu đang hiện dialog rồi thì bỏ qua
            lock (_lock)
            {
                if (_isShowingDisconnectDialog)
                {
                    Console.WriteLine($"[ConnectionHelper] Dialog already showing, skipping");
                    return;
                }
                _isShowingDisconnectDialog = true;
            }

            try
            {
                // Tìm form đang visible để hiển thị dialog
                Form dialogOwner = GetActiveVisibleForm() ?? form;

                Console.WriteLine($"[ConnectionHelper] Showing disconnect dialog on: {dialogOwner.Name}");

                var result = MessageBox.Show(
                    dialogOwner,
                    $"❌ Lost connection to the server!\n\n" +
                    $"Details: {message}\n\n" +
                    "Please check:\n" +
                    "• Is your network connection stable?\n" +
                    "• Is the server running?\n\n" +
                    "Do you want to try reconnecting?",
                    "⚠️ Connection Error",
                    MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Retry)
                {
                    _ = RetryConnectionAsync(dialogOwner, onRetrySuccess, onCancel);
                }
                else
                {
                    ResetDialogFlag();

                    if (onCancel != null)
                    {
                        onCancel.Invoke();
                    }
                    else
                    {
                        dialogOwner.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConnectionHelper] Error: {ex.Message}");
                ResetDialogFlag();
            }
        }

        private static void ResetDialogFlag()
        {
            lock (_lock)
            {
                _isShowingDisconnectDialog = false;
            }
        }

        private static async Task RetryConnectionAsync(Form form, Action onSuccess = null, Action onCancel = null)
        {
            if (form == null || form.IsDisposed)
            {
                ResetDialogFlag();
                return;
            }

            form.Cursor = Cursors.WaitCursor;

            try
            {
                Console.WriteLine("🔄 Attempting to reconnect...");

                bool connected = await PersistentTcpClient.Instance.ConnectAsync();

                if (form.IsDisposed)
                {
                    ResetDialogFlag();
                    return;
                }

                if (connected)
                {
                    Console.WriteLine("✅ Reconnection successful!");
                    ResetDialogFlag();

                    MessageBox.Show(
                        form,
                        "✅ Reconnected successfully!",
                        "Notification",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    // Gọi callback riêng của form gốc (nếu có)
                    onSuccess?.Invoke();

                    // Broadcast để tất cả forms tự reconnect services
                    Console.WriteLine("[ConnectionHelper] 📢 Broadcasting OnReconnected...");
                    OnReconnected?.Invoke();
                }
                else
                {
                    Console.WriteLine("❌ Reconnection failed!");
                    ResetDialogFlag();

                    Form activeForm = GetActiveVisibleForm() ?? form;
                    HandleDisconnect(activeForm, "Unable to reconnect to the server.", onSuccess, onCancel);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Retry error: {ex.Message}");
                ResetDialogFlag();

                if (!form.IsDisposed)
                {
                    HandleDisconnect(form, ex.Message, onSuccess, onCancel);
                }
            }
            finally
            {
                if (!form.IsDisposed)
                {
                    form.Cursor = Cursors.Default;
                }
            }
        }

        public static async Task CheckConnectionOnLoadAsync(
            Form form,
            Action onSuccess = null,
            Action onFail = null)
        {
            if (form == null || form.IsDisposed) return;

            form.Cursor = Cursors.WaitCursor;
            string originalTitle = form.Text;
            form.Text = originalTitle + " - Checking connection...";

            try
            {
                Console.WriteLine("🔍 Checking server connection...");

                bool connected = await PersistentTcpClient.Instance.ConnectAsync();

                if (form.IsDisposed) return;

                if (connected)
                {
                    Console.WriteLine("✅ Server connection successful!");
                    form.Text = originalTitle;
                    onSuccess?.Invoke();
                }
                else
                {
                    Console.WriteLine("❌ Server connection failed!");
                    form.Text = originalTitle + " - Unable to connect";

                    if (onFail != null)
                    {
                        onFail.Invoke();
                    }
                    else
                    {
                        HandleDisconnect(form, "Unable to connect to the server during startup.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection check error: {ex.Message}");
                if (!form.IsDisposed)
                {
                    form.Text = originalTitle + " - Connection error";
                    HandleDisconnect(form, ex.Message);
                }
            }
            finally
            {
                if (!form.IsDisposed)
                {
                    form.Cursor = Cursors.Default;
                }
            }
        }

        public static void Reset()
        {
            ResetDialogFlag();
        }
    }
}