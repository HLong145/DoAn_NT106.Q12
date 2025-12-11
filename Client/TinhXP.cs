using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoAn_NT106.Client
{
    public partial class TinhXP : Form
    {
        private readonly MatchResult _result;
        private int _calculatedXp;
        private bool _detailsInitialized;

        // Giả sử các control này đã có trong Designer:
        // Label lbl_XPEarnedValue;
        // Panel pnl_XPDetails;
        // Button btn_Continue, btn_ViewStats;

        public TinhXP(MatchResult result)
        {
            InitializeComponent();
            _result = result ?? throw new ArgumentNullException(nameof(result));

            CalculateAndDisplayXp();
        }

        // Nếu form cũ vẫn cần constructor rỗng (ví dụ Designer),
        // nhưng thực tế BattleForm luôn gọi constructor có MatchResult.
        public TinhXP() : this(new MatchResult())
        {
        }

        private void CalculateAndDisplayXp()
        {
            // 1. XP thắng / thua
            int baseXp = _result.PlayerIsWinner ? 100 : 40;

            // 2. Không thua hiệp nào: +50 XP
            if (_result.PlayerIsWinner && _result.OpponentWins == 0)
            {
                baseXp += 50;
            }

            // 3. Điểm phong cách (score trong trận)
            // parry: +10, attack trúng: +5, skill: +20
            int styleScore = (_result.ParryCount * 10)
                           + (_result.AttackCount * 5)
                           + (_result.SkillCount * 20);

            // 4. Tổng XP
            _calculatedXp = baseXp + styleScore / 5;

            // Hiển thị tổng XP trên label chính
            if (lbl_XPEarnedValue != null)
            {
                lbl_XPEarnedValue.Text = _calculatedXp.ToString();
            }
        }
        private void btn_Continue_Click(object sender, EventArgs e)
        {

        }

        private void btn_ViewStats_Click(object sender, EventArgs e)
        {
            if (pnl_XPDetails == null) return;

            if (!_detailsInitialized)
            {
                BuildDetailsPanel();
                _detailsInitialized = true;
            }

            pnl_XPDetails.Visible = !pnl_XPDetails.Visible;
        }

        private void BuildDetailsPanel()
        {
            pnl_XPDetails.Controls.Clear();
            pnl_XPDetails.AutoScroll = true;

            int y = 10;
            int lineHeight = 22;

            void AddLine(string label, string value, Color? color = null)
            {
                var lbl = new Label
                {
                    AutoSize = false,
                    Width = pnl_XPDetails.Width - 20,
                    Height = lineHeight,
                    Location = new Point(10, y),
                    Text = $"{label}: {value}",
                    ForeColor = color ?? Color.White,
                    BackColor = Color.Transparent
                };
                pnl_XPDetails.Controls.Add(lbl);
                y += lineHeight + 2;
            }

            // 1. Kết quả trận
            string resultText = _result.PlayerIsWinner ? "Thắng (2 hiệp)" : "Thua";
            AddLine("Kết quả", resultText, _result.PlayerIsWinner ? Color.Lime : Color.OrangeRed);

            // 2. XP thắng/thua + bonus không thua hiệp
            int winLoseXp = _result.PlayerIsWinner ? 100 : 40;
            int noRoundLostXp = (_result.PlayerIsWinner && _result.OpponentWins == 0) ? 50 : 0;

            AddLine("XP thắng/thua", $"{winLoseXp} XP");
            AddLine("XP không thua hiệp nào", $"{noRoundLostXp} XP");

            // 3. Thống kê trong trận
            AddLine("Parry", $"{_result.ParryCount} × 10 điểm");
            AddLine("Attack trúng", $"{_result.AttackCount} × 5 điểm");
            AddLine("Skill", $"{_result.SkillCount} × 20 điểm");

            int styleScore = (_result.ParryCount * 10)
                           + (_result.AttackCount * 5)
                           + (_result.SkillCount * 20);

            AddLine("Tổng điểm phong cách", $"{styleScore} điểm");
            AddLine("XP từ phong cách (Điểm / 5)", $"{styleScore / 5} XP", Color.Cyan);

            // 4. Tổng XP
            AddLine("TỔNG XP NHẬN ĐƯỢC", $"{_calculatedXp} XP", Color.Yellow);
        }
    }

    // Giả định: MatchResult đã được định nghĩa trong project (BattleForm dùng để truyền sang đây)
    public class MatchResult
    {
        public string PlayerUsername { get; set; }
        public string OpponentUsername { get; set; }
        public bool PlayerIsWinner { get; set; }
        public TimeSpan MatchTime { get; set; }
        public int PlayerWins { get; set; }
        public int OpponentWins { get; set; }

        // Những field này phải được BattleForm gán đúng trước khi mở TinhXP
        public int ParryCount { get; set; }
        public int AttackCount { get; set; }
        public int SkillCount { get; set; }
    }
}

