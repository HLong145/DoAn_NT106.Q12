using DoAn_NT106.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DoAn_NT106.Client
{
    public partial class TinhXP : Form
    {

        private readonly MatchResult _data;
        private readonly DatabaseService _db = new DatabaseService();

        // XP logic fields
        private int _xpEarned;
        private int _xpBefore;
        private int _xpAfter;
        private int _oldLevel;
        private int _newLevel;
        private int _totalXpGained;

        private const int XP_PER_LEVEL = 2000;

        public TinhXP(MatchResult data)
        {
            InitializeComponent();
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        private void TinhXP_Load(object sender, EventArgs e)
        {
            // ========== LẤY XP TRONG DATABASE ==========
            var info = _db.GetPlayerXpInfo(_data.PlayerUsername);
            if (info.HasValue)
            {
                _data.PreviousLevel = info.Value.Level;
                _data.PreviousXP = info.Value.XP;
                _data.PreviousTotalXP = info.Value.TotalXP;
            }
            else
            {
                _data.PreviousLevel = Math.Max(1, _data.PreviousLevel);
                _data.PreviousXP = Math.Max(0, _data.PreviousXP);
                _data.PreviousTotalXP = Math.Max(0, _data.PreviousTotalXP);
            }

            _oldLevel = _data.PreviousLevel;
            _xpBefore = _data.PreviousXP;

            // ========== HIỂN THỊ THÔNG TIN CƠ BẢN ==========
            lbl_PlayerValue.Text = _data.PlayerUsername;
            lbl_TimeValue.Text = _data.MatchTime.ToString(@"mm\:ss");

            if (_data.PlayerIsWinner)
            {
                lbl_ResultValue.Text = "WIN";
                lbl_ResultValue.ForeColor = Color.LimeGreen;
            }
            else
            {
                lbl_ResultValue.Text = "LOSE";
                lbl_ResultValue.ForeColor = Color.Red;
            }

            // ===========================================
            // ============ TÍNH XP THEO BẢNG BẠN GỬI =============
            // ===========================================

            _xpEarned = 0;

            // 1. XP thắng/thua
            if (_data.PlayerIsWinner)
                _xpEarned += 100;
            else
                _xpEarned += 40;

            // 2. Không thua hiệp nào
            if (_data.PlayerIsWinner && _data.OpponentWins == 0)
                _xpEarned += 50;

            // 3. Action XP
            // (DÙNG ĐÚNG BẢNG BẠN CHỤP)
            _xpEarned += _data.ParryCount * 10;
            _xpEarned += _data.ComboCount * 5;
            _xpEarned += _data.SkillCount * 20;

            // tổng XP để cộng vào TotalXP trong DB
            _totalXpGained = _xpEarned;

            // ===========================================
            // ============ TÍNH LEVEL UP ================
            // ===========================================

            int tempXP = _xpBefore + _xpEarned;
            int lvl = _oldLevel;

            while (tempXP >= XP_PER_LEVEL)
            {
                tempXP -= XP_PER_LEVEL;
                lvl++;
            }

            _newLevel = lvl;
            _xpAfter = tempXP;

            // ========== UPDATE UI ==========
            lbl_XPEarnedValue.Text = $"+{_xpEarned} XP";
            lbl_XPBefore.Text = $"XP Before Match: {_xpBefore} XP";
            lbl_XPGained.Text = $"XP Gained: +{_xpEarned} XP";
            lbl_XPAfter.Text = $"XP After Match: {_xpAfter} XP";

            lbl_XPProgress.Text = $"Level {_oldLevel} → Level {_newLevel}";
            lbl_XPProgressValue.Text = $"{_xpAfter} / {XP_PER_LEVEL} XP";

            UpdateXPBarInstant(_xpAfter, XP_PER_LEVEL);
        }

        // ============================== HIỂN THỊ CHI TIẾT XP ==============================
        private void LoadXPDetails()
        {
            pnl_XPDetails.Controls.Clear();

            AddXPDetail("Match Result", _data.PlayerIsWinner ? "+100 XP" : "+40 XP");

            if (_data.PlayerIsWinner && _data.OpponentWins == 0)
                AddXPDetail("No round lost", "+50 XP");

            if (_data.ParryCount > 0)
                AddXPDetail($"Parry ×{_data.ParryCount}", $"+{_data.ParryCount * 10} XP");

            if (_data.ComboCount > 0)
                AddXPDetail($"Combo ×{_data.ComboCount}", $"+{_data.ComboCount * 5} XP");

            if (_data.SkillCount > 0)
                AddXPDetail($"Skill ×{_data.SkillCount}", $"+{_data.SkillCount * 20} XP");

            AddXPDetail("--------------------------------------", "");
            AddXPDetail("TOTAL XP", $"+{_xpEarned} XP");
        }

        private void AddXPDetail(string title, string value)
        {
            var lbl = new Label
            {
                Text = $"{title.PadRight(30)} {value}",
                ForeColor = Color.White,
                Font = new Font("Courier New", 9, FontStyle.Bold),
                AutoSize = false,
                Height = 20,
                Width = pnl_XPDetails.Width - 10,
                Padding = new Padding(4)
            };
            pnl_XPDetails.Controls.Add(lbl);
        }

        // ====== UPDATE XP BAR ======
        private void UpdateXPBarInstant(int xp, int max)
        {
            double pct = max == 0 ? 0 : (double)xp / max;
            int w = (int)(pct * pnl_XPBarContainer.Width);

            pnl_XPBarFill.Width = w;
            lbl_XPPercent.Text = $"{Math.Round(pct * 100)}%";
        }

        private void pnl_Main_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btn_Continue_Click(object sender, EventArgs e)
        {
            // Disable button to prevent double click
            btn_Continue.Enabled = false;

            bool ok = _db.UpdatePlayerXp(_data.PlayerUsername, _newLevel, _xpAfter, _totalXpGained);
            if (!ok)
            {
                MessageBox.Show("Failed to update XP in database. Check connection.", "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // Optionally: show a success toast or sound
            }

            // close summary and return user to main/menu (or close app)
            this.Close();

        }

        private void btn_ViewStats_Click(object sender, EventArgs e)
        {
            LoadXPDetails();

            // Optional: scroll to top
            pnl_XPDetails.VerticalScroll.Value = 0;
        }
    }
}
