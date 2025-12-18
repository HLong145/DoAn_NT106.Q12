using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoAn_NT106.Client.Class
{
    /// <summary>
    /// Helper class xử lý mất kết nối server cho tất cả các form
    /// </summary>
    public static class ConnectionHelper
    {
        //  để chống hiện nhiều MessageBox cùng lúc
        private static bool _isShowingDisconnectDialog = false;
        private static readonly object _lock = new object();

        /// <summary>
        /// Xử lý khi mất kết nối - hiển thị dialog Retry/Cancel
        /// Chỉ hiện 1 dialog duy nhất dù có nhiều nơi gọi
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
                    Console.WriteLine($"[ConnectionHelper] Dialog already showing, skipping duplicate for: {form.Name}");
                    return;
                }
                _isShowingDisconnectDialog = true;
            }

            try
            {
                Console.WriteLine($"[ConnectionHelper] Showing disconnect dialog for: {form.Name}");

                var result = MessageBox.Show(
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
                    _ = RetryConnectionAsync(form, onRetrySuccess, onCancel);
                }
                else
                {
                    // Reset flag trước khi thực hiện action
                    ResetDialogFlag();

                    // Thực hiện action cancel hoặc đóng form mặc định
                    if (onCancel != null)
                    {
                        onCancel.Invoke();
                    }
                    else
                    {
                        form.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConnectionHelper] Error: {ex.Message}");
                ResetDialogFlag();
            }
        }

        /// <summary>
        /// Reset flag khi dialog đã đóng
        /// </summary>
        private static void ResetDialogFlag()
        {
            lock (_lock)
            {
                _isShowingDisconnectDialog = false;
            }
        }

        /// <summary>
        /// Thử kết nối lại server
        /// </summary>
        private static async Task RetryConnectionAsync(Form form, Action onSuccess = null, Action onCancel = null)
        {
            if (form == null || form.IsDisposed)
            {
                ResetDialogFlag();
                return;
            }

            // Hiển thị cursor chờ
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

                    // flag TRƯỚC khi hiện thông báo thành công
                    ResetDialogFlag();

                    MessageBox.Show(
                        "✅ Reconnected successfully!",
                        "Notification",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    // Thực hiện action khi retry thành công
                    onSuccess?.Invoke();
                }
                else
                {
                    Console.WriteLine("❌ Reconnection failed!");

                    // Reset flag TRƯỚC khi gọi lại HandleDisconnect
                    ResetDialogFlag();

                    // Gọi lại HandleDisconnect để hiện dialog retry/cancel
                    HandleDisconnect(form, "Unable to reconnect to the server.", onSuccess, onCancel);
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

        /// <summary>
        /// Kiểm tra kết nối khi form load
        /// </summary>
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

        /// <summary>
        /// Reset trạng thái khi cần (ví dụ khi đóng app)
        /// </summary>
        public static void Reset()
        {
            ResetDialogFlag();
        }
    }
}