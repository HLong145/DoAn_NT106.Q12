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
    }
}
