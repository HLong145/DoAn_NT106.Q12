using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn_NT106.Client
{
    public class MatchResult
    {
        // --- BASIC INFO ---
        public string PlayerUsername { get; set; }        // Username của người chơi local
        public string OpponentUsername { get; set; }      // Username đối thủ
        public bool PlayerIsWinner { get; set; }          // true nếu người chơi thắng

        // --- MATCH DETAILS ---
        public TimeSpan MatchTime { get; set; }           // Tổng thời gian trận đấu
        public int PlayerWins { get; set; }               // Số round thắng của người chơi
        public int OpponentWins { get; set; }             // Số round thắng của đối thủ

        // --- ACTION STATS (tùy bạn có dùng hay không) ---
        public int ParryCount { get; set; } = 0;          // Số lần parry
        public int ComboCount { get; set; } = 0;          // Số combo hits
        public int SkillCount { get; set; } = 0;          // Số skill hits

        // --- DB INFO (được load khi vào form TinhXP) ---
        public int PreviousLevel { get; set; } = 1;       // Level trước trận
        public int PreviousXP { get; set; } = 0;          // XP trước trận
        public int PreviousTotalXP { get; set; } = 0;     // TOTAL_XP trước trận
    }
}
