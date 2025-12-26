using System;
using System.Drawing;
using System.Windows.Forms;
using DoAn_NT106.Client.Class;

namespace DoAn_NT106.Client
{
    /// <summary>
    /// Form nhỏ để nhập Server IP trước khi đăng nhập
    /// </summary>
    public partial class FormIPConfig : Form
    {
        #region Properties

        public string ServerIP { get; private set; }
        public bool IsConfirmed { get; private set; } = false;

        #endregion

        #region Constructor

        public FormIPConfig()
        {
            InitializeComponent();
            this.Load += FormIPConfig_Load;
        }

        #endregion

        #region Form Events

        private void FormIPConfig_Load(object sender, EventArgs e)
        {
            // Focus vào textbox và select all text
            txtServerIP.Focus();
            txtServerIP.SelectAll();
        }

        #endregion

        #region Keyboard Events

        private void txtServerIP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                btnConnect_Click(sender, e);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                btnCancel_Click(sender, e);
            }
        }

        #endregion

        #region Button Events

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string ip = txtServerIP.Text.Trim();

            // Validate IP
            if (string.IsNullOrWhiteSpace(ip))
            {
                ShowStatus("⚠️ Please enter a valid IP!", Color.Red);
                txtServerIP.Focus();
                return;
            }

            // Validate IP format
            if (!IsValidIPOrHostname(ip))
            {
                ShowStatus("⚠️ Invalid IP format!", Color.Red);
                txtServerIP.Focus();
                return;
            }

            // Hiển thị trạng thái
            ShowStatus("🔄 Configuring...", Color.Lime);
            Application.DoEvents();

            // Set IP
            ServerIP = ip;
            IsConfirmed = true;

            // Cập nhật AppConfig
            AppConfig.SERVER_IP = ip;
            Console.WriteLine($"[FormIPConfig] ✅ Server IP set to: {ip}");

            // Reset PersistentTcpClient để sử dụng IP mới
            PersistentTcpClient.ResetInstance();
            Console.WriteLine($"[FormIPConfig] 🔄 TCP Client reset for new IP");

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            IsConfirmed = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion

        #region Helper Methods

        private void ShowStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }

        private bool IsValidIPOrHostname(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Cho phép localhost
            if (input.ToLower() == "localhost")
                return true;

            // Kiểm tra IPv4 format
            if (System.Net.IPAddress.TryParse(input, out _))
                return true;

            // Cho phép hostname (không chứa khoảng trắng)
            if (!input.Contains(" ") && input.Length > 0)
                return true;

            return false;
        }

        #endregion
    }
}