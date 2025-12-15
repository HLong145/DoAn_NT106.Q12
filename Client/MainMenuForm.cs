using System;
using System.Windows.Forms;

namespace DoAn_NT106
{
    public partial class MainMenuForm : Form
    {
        public string CurrentRoomCode { get; set; } = "000000";

        public MainMenuForm()
        {
            InitializeComponent();
        }

        public MainMenuForm(string roomCode) : this()
        {
            CurrentRoomCode = roomCode ?? "000000";
        }

        private void BtnInstructions_Click(object sender, EventArgs e)
        {
            try
            {
                var f = new InstructionForm();
                // show as dialog so user can return
                f.StartPosition = FormStartPosition.CenterParent;
                f.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open Instructions: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
