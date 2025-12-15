using System;
using System.Windows.Forms;

namespace DoAn_NT106
{
    public partial class InstructionForm : Form
    {
        public InstructionForm()
        {
            InitializeComponent();
            LoadDefaultTexts();
        }

        private void LoadDefaultTexts()
        {
            // Offline controls
            txtOffline.Text =
                "OFFLINE MODE - CONTROLS:\n\n" +
                "Player 1 \n" +
                "  Move Left: A\n" +
                "  Move Right: D\n" +
                "  Jump: W\n" +
                "  Punch: J\n" +
                "  Kick: K\n" +
                "  Dash: L\n" +
                "  Parry: U\n\n" +
                "Player 2:\n" +
                "  Move Left: Left Arrow\n" +
                "  Move Right: Right Arrow\n" +
                "  Jump: Up Arrow\n" +
                "  Punch: Numpad1\n" +
                "  Kick: Numpad2\n" +
                "  Dash: Numpad3\n" +
                "  Parry: Numpad5\n\n" +
                "Tips:\n" +
                "  - Use parry to negate incoming attacks and gain mana.\n" +
                "  - Use slide to quickly close distance.\n" +
                "  - Manage stamina and mana for skills.";

            // Online controls - include network notes and the in-game control summary
            txtOnline.Text =
                "ONLINE MODE - CONTROLS & NOTES:\n\n" +
                "CONTROL SUMMARY:\n" +
                "  Player 1 (Host):: A/D (Move) | W (Jump) | J (Attack1) | K (Attack2) | L (Dash) | U (Parry) | I (Skill)\n" +
                "  Player 2 (Guest): A/D (Move) | W (Jump) | J (Attack1) | K (Attack2) | L (Dash) | U (Parry) | I (Skill)";
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
