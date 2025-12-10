using System;
using System.Windows.Forms;

namespace DoAn_NT106
{
    public partial class PasswordToJoinRoom : Form
    {
        public string Password { get; private set; }

        public PasswordToJoinRoom(string roomName)
        {
            InitializeComponent();
            lblInfo.Text = $"Room: {roomName}";
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            Password = tbPassword.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void tbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                BtnOK_Click(sender, e);
            }
        }
    }
}
