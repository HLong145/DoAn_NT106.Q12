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
            this.AutoScroll = true;       
            this.AutoScrollMinSize = new Size(0, 1200); 
            _data = data ?? throw new ArgumentNullException(nameof(data));
            
            // ✅ THÊM: Subscribe nút Continue
            btn_Continue.Click += btn_Continue_Click;
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
                lbl_ResultValue.Text = "🏆 WIN 🏆";
                lbl_ResultValue.ForeColor = Color.Gold;
            }
            else
            {
                lbl_ResultValue.Text = "❌ LOSE";
                lbl_ResultValue.ForeColor = Color.FromArgb(255, 100, 100);
            }

            Console.WriteLine($"[TinhXP] Player: {_data.PlayerUsername} vs {_data.OpponentUsername}");
            Console.WriteLine($"[TinhXP] Match Result: {(lbl_ResultValue.Text)} ({_data.PlayerWins}:{_data.OpponentWins})");
            Console.WriteLine($"[TinhXP] Stats - Parry:{_data.ParryCount}, Combo:{_data.ComboCount}, Skill:{_data.SkillCount}");
            Console.WriteLine($"[TinhXP] XP Before: {_xpBefore}, Level: {_oldLevel}");

            // ===========================================
            // ============ TÍNH XP THEO BẢNG BẠN GỬI =============
            // ===========================================

            _xpEarned = 0;

            // 1. XP thắng/thua
            if (_data.PlayerIsWinner)
                _xpEarned += 100;
            else
                _xpEarned += 40;

            // 2. Không thua hiệp nào (perfect round)
            if (_data.PlayerWins >= 2 && _data.OpponentWins == 0)
                _xpEarned += 50;

            // 3. Action XP (tính theo chi tiết)
            int parryXP = _data.ParryCount * 10;
            int comboXP = _data.ComboCount * 5;
            int skillXP = _data.SkillCount * 20;

            _xpEarned += parryXP;
            _xpEarned += comboXP;
            _xpEarned += skillXP;

            // tổng XP để cộng vào TotalXP trong DB
            _totalXpGained = _xpEarned;

            Console.WriteLine($"[TinhXP] XP Calculation:");
            Console.WriteLine($"  - Win/Loss XP: {(_data.PlayerIsWinner ? 100 : 40)}");
            Console.WriteLine($"  - Perfect Victory XP: {(_data.PlayerWins >= 2 && _data.OpponentWins == 0 ? 50 : 0)}");
            Console.WriteLine($"  - Parry XP: {parryXP} ({_data.ParryCount} × 10)");
            Console.WriteLine($"  - Combo XP: {comboXP} ({_data.ComboCount} × 5)");
            Console.WriteLine($"  - Skill XP: {skillXP} ({_data.SkillCount} × 20)");
            Console.WriteLine($"  - TOTAL XP: {_xpEarned}");

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
            lbl_XPGained.Text = $"✅ XP Gained: +{_xpEarned} XP";
            lbl_XPAfter.Text = $"XP After Match: {_xpAfter} XP";

            if (_oldLevel < _newLevel)
            {
                lbl_XPProgress.Text = $"Level {_oldLevel} → Level {_newLevel} ⭐ LEVEL UP!";
                lbl_XPProgress.ForeColor = Color.Gold;
            }
            else
            {
                lbl_XPProgress.Text = $"Level {_oldLevel} (no level up)";
            }

            lbl_XPProgressValue.Text = $"{_xpAfter} / {XP_PER_LEVEL} XP";

            // ========== UPDATE XP BAR ==========
            int xpPercent = (_xpAfter * 100) / XP_PER_LEVEL;
            pnl_XPBarFill.Width = (xpPercent * pnl_XPBarContainer.Width) / 100;
            lbl_XPPercent.Text = $"{xpPercent}%";

            Console.WriteLine($"[TinhXP] Level Progress: {_oldLevel} → {_newLevel}, XP: {_xpAfter} / {XP_PER_LEVEL} ({xpPercent}%)");

            // ✅ THÊM: Hiển thị chi tiết breakdown
            LoadXPDetailBreakdown();
        }

        // ✅ THÊM: Hiển thị chi tiết breakdown XP
        private void LoadXPDetailBreakdown()
        {
            pnl_XPDetails.Controls.Clear();

            int yPos = 5;
            
            AddDetailLabel("=== XP BREAKDOWN ===", Color.Gold, 11, yPos);
            yPos += 25;

            // Result XP
            if (_data.PlayerIsWinner)
            {
                AddDetailLabel("🏆 Match Win", Color.LimeGreen, 10, yPos);
                AddDetailLabel("+100 XP", Color.White, 10, yPos);
                yPos += 20;
            }
            else
            {
                AddDetailLabel("❌ Match Loss", Color.FromArgb(255, 100, 100), 10, yPos);
                AddDetailLabel("+40 XP", Color.White, 10, yPos);
                yPos += 20;
            }

            // Perfect round bonus
            if (_data.PlayerWins >= 2 && _data.OpponentWins == 0)
            {
                AddDetailLabel("💎 Perfect Victory (No rounds lost)", Color.Cyan, 10, yPos);
                AddDetailLabel("+50 XP", Color.White, 10, yPos);
                yPos += 20;
            }

            // Actions
            yPos += 5;
            AddDetailLabel("--- ACTION BONUSES ---", Color.Orange, 10, yPos);
            yPos += 20;

            if (_data.ParryCount > 0)
            {
                AddDetailLabel($"🛡️ Parry × {_data.ParryCount}", Color.LightBlue, 10, yPos);
                AddDetailLabel($"+{_data.ParryCount * 10} XP", Color.White, 10, yPos);
                yPos += 20;
            }

            if (_data.ComboCount > 0)
            {
                AddDetailLabel($"⚔️ Combo Hit × {_data.ComboCount}", Color.FromArgb(255, 165, 0), 10, yPos);
                AddDetailLabel($"+{_data.ComboCount * 5} XP", Color.White, 10, yPos);
                yPos += 20;
            }

            if (_data.SkillCount > 0)
            {
                AddDetailLabel($"✨ Skill × {_data.SkillCount}", Color.Magenta, 10, yPos);
                AddDetailLabel($"+{_data.SkillCount * 20} XP", Color.White, 10, yPos);
                yPos += 20;
            }

            yPos += 10;
            AddDetailLabel("================", Color.Gold, 11, yPos);
            yPos += 20;
            AddDetailLabel($"TOTAL: +{_xpEarned} XP", Color.Gold, 12, yPos);
        }

        // ✅ THÊM: Helper để thêm label
        private void AddDetailLabel(string text, Color color, int fontSize, int yPos)
        {
            var lbl = new Label
            {
                Text = text,
                ForeColor = color,
                Font = new Font("Courier New", fontSize, FontStyle.Bold),
                AutoSize = false,
                Height = 20,
                Width = pnl_XPDetails.Width - 10,
                Location = new Point(5, yPos),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnl_XPDetails.Controls.Add(lbl);
        }

        private void LoadXPDetails()
        {
            pnl_XPDetails.Controls.Clear();

            AddXPDetail("Match Result", _data.PlayerIsWinner ? "+100 XP" : "+40 XP");

            if (_data.PlayerWins >= 2 && _data.OpponentWins == 0)
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

        private void btn_Continue_Click(object sender, EventArgs e)
        {
            // Disable button to prevent double click
            btn_Continue.Enabled = false;

            // ✅ Update XP in database
            bool ok = _db.UpdatePlayerXp(_data.PlayerUsername, _newLevel, _xpAfter, _totalXpGained);
            if (!ok)
            {
                MessageBox.Show("Failed to update XP in database. Check connection.", "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btn_Continue.Enabled = true; // Re-enable button so user can retry
                return;
            }

            Console.WriteLine($"[TinhXP] ✅ Updated XP in database: {_data.PlayerUsername} -> Level {_newLevel}, XP {_xpAfter}");

            // ✅ Close this form to return to room/menu
            this.Close();
        }
    }
}
