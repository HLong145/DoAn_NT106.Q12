using System;
using System.Windows.Forms;

namespace DoAn_NT106.Client
{
    public partial class PasswordToJoinRoom : Form
    {
        #region Properties

        public string Password { get; private set; }

        #endregion


        #region Constructors

        public PasswordToJoinRoom(string roomName)
        {
            InitializeComponent();
            lblInfo.Text = $"Room: {roomName}";
        }

        #endregion

        #region Button Events

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

        #endregion

        #region TextBox events

        private void tbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                BtnOK_Click(sender, e);
            }
        }

        #endregion
    }
}
