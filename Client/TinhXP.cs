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

namespace DoAn_NT106.Client
{
    public partial class TinhXP : Form
    {
        private readonly MatchResult _result;
        private int _calculatedXp;
        private bool _detailsInitialized;
        private int _levelBefore;
        private int _levelAfter;
        private int _xpBefore;
        private int _xpAfter;
        private int _xpNeededForNextLevel;

        // Giả sử các control này đã có trong Designer:
        // Label lbl_XPEarnedValue;
        // Panel pnl_XPDetails;
        // Button btn_Continue, btn_ViewStats;

        public TinhXP(MatchResult result)
        {
            InitializeComponent();
            this.AutoScroll = true;
            this.AutoScrollMinSize = Size.Empty;
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

            // 4. Tổng XP cộng thêm
            _calculatedXp = baseXp + styleScore / 5;

            // ====== TÍNH LEVEL / XP TRƯỚC & SAU, CẬP NHẬT DATABASE ======
            const int xpPerLevel = 1000;

            // Lấy XP hiện tại từ DB (nếu có), mặc định level 1, XP 0
            _xpBefore = 0;
            try
            {
                if (!string.IsNullOrEmpty(_result.PlayerUsername))
                {
                    var db = new DatabaseService();
                    _xpBefore = db.GetPlayerXp(_result.PlayerUsername); 
                }
            }
            catch
            {
                _xpBefore = 0;
            }

            _xpAfter = _xpBefore + _calculatedXp;

            _levelBefore = Math.Max(1, (_xpBefore / xpPerLevel) + 1);
            _levelAfter = Math.Max(1, (_xpAfter / xpPerLevel) + 1);

            _xpNeededForNextLevel = _levelAfter * xpPerLevel;

            // Lưu XP mới và TOTAL_XP (tổng XP cần cho level hiện tại) vào DB
            try
            {
                if (!string.IsNullOrEmpty(_result.PlayerUsername))
                {
                    var db = new DatabaseService();
                    db.UpdatePlayerXp(_result.PlayerUsername, _xpAfter, _xpNeededForNextLevel);

                    // Nếu đã lên cấp, ghi đè cột USER_LEVEL trong bảng PLAYERS
                    if (_levelAfter > _levelBefore)
                    {
                        db.UpdatePlayerLevel(_result.PlayerUsername, _levelAfter);
                    }
                }
            }
            catch
            {
            }

            // Hiển thị tổng XP nhận được trên label chính
            if (lbl_XPEarnedValue != null)
            {
                lbl_XPEarnedValue.Text = "+" + _calculatedXp + " XP";
            }

            if (lbl_XPBefore != null)
            {
                // Ví dụ: "XP Before Match: 100 XP"
                lbl_XPBefore.Text = $"XP Before Match: {_xpBefore} XP";
            }

            if (lbl_XPGained != null)
            {
                // Ví dụ: "XP Gained: 100 XP"
                lbl_XPGained.Text = $"XP Gained: {_calculatedXp} XP";
            }

            if (lbl_XPAfter != null)
            {
                // Ví dụ: "XP After Match: 200 XP"
                lbl_XPAfter.Text = $"XP After Match: {_xpAfter} XP";
            }

            // Cập nhật thông tin người chơi thắng/thua
            if (lbl_PlayerValue != null)
            {
                lbl_PlayerValue.Text = _result.PlayerUsername ?? "PLAYER";
            }

            if (lbl_ResultValue != null)
            {
                if (_result.PlayerIsWinner)
                {
                    lbl_ResultValue.Text = "WIN";
                    lbl_ResultValue.ForeColor = Color.LimeGreen;
                }
                else
                {
                    lbl_ResultValue.Text = "LOSE";
                    lbl_ResultValue.ForeColor = Color.Red;
                }
            }

            // Thời gian tổng trận đấu: lấy từ MatchResult.MatchTime nếu có
            if (lbl_TimeValue != null)
            {
                TimeSpan time = _result.MatchTime;
                if (time <= TimeSpan.Zero)
                {
                    // Nếu chưa gán từ BattleForm, để 00:00
                    lbl_TimeValue.Text = "00:00";
                }
                else
                {
                    lbl_TimeValue.Text = string.Format("{0:00}:{1:00}", (int)time.TotalMinutes, time.Seconds);
                }
            }

            // ====== CẬP NHẬT PROGRESS LEVEL / THANH XP ======
            // lbl_XPProgress: Level X → Level Y hoặc Level X nếu không lên cấp
            if (lbl_XPProgress != null)
            {
                if (_levelAfter > _levelBefore)
                {
                    lbl_XPProgress.Text = $"Level {_levelBefore} -> Level {_levelAfter}";
                }
                else
                {
                    lbl_XPProgress.Text = $"Level {_levelAfter}";
                }
            }

            // lbl_XPProgressValue: currentXP / neededXP XP
            if (lbl_XPProgressValue != null)
            {
                lbl_XPProgressValue.Text = $"{_xpAfter} / {_xpNeededForNextLevel} XP";
            }

            // lbl_XPPercent + thanh pnl_XPBarFill: phần trăm dựa trên XP trong level hiện tại
            int xpInCurrentLevel = _xpAfter % xpPerLevel;
            float percent = xpPerLevel > 0 ? (xpInCurrentLevel * 100f / xpPerLevel) : 0f;

            if (lbl_XPPercent != null)
            {
                lbl_XPPercent.Text = $"{percent:0}%";
            }

            if (pnl_XPBarFill != null && pnl_XPBarContainer != null)
            {
                int maxWidth = pnl_XPBarContainer.Width;
                int fillWidth = (int)(maxWidth * (percent / 100f));
                if (fillWidth < 0) fillWidth = 0;
                if (fillWidth > maxWidth) fillWidth = maxWidth;
                pnl_XPBarFill.Width = fillWidth;
            }
        }

        private void btn_Continue_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_ViewStats_Click(object sender, EventArgs e)
        {
            if (pnl_XPDetails == null) return;

            if (!_detailsInitialized)
            {
                BuildDetailsPanel();
                _detailsInitialized = true;

                // Lần nhấn đầu tiên: sau khi build, hiển thị luôn
                pnl_XPDetails.Visible = true;
            }
            else
            {
                // Các lần sau: toggle bình thường
                pnl_XPDetails.Visible = !pnl_XPDetails.Visible;
            }
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
    public enum MatchReturnMode
    {
        ReturnToJoinRoom = 0,
        ReturnToGameLobby = 1
    }

    public class MatchResult
    {
        public string PlayerUsername { get; set; }
        public string OpponentUsername { get; set; }
        public bool PlayerIsWinner { get; set; }
        public TimeSpan MatchTime { get; set; }
        public int PlayerWins { get; set; }
        public int OpponentWins { get; set; }

        // Thông tin room để quay lại đúng phòng
        public string RoomCode { get; set; }
        public string Token { get; set; }

        // Xác định sau khi tính XP sẽ quay về đâu
        public MatchReturnMode ReturnMode { get; set; } = MatchReturnMode.ReturnToJoinRoom;

        // Những field này phải được BattleForm gán đúng trước khi mở TinhXP
        public int ParryCount { get; set; }
        public int AttackCount { get; set; }
        public int SkillCount { get; set; }
    }
}

