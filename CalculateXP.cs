using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn_NT106
{
    internal class CalculateXP
    {    
        private const int WIN_XP = 100;
        private const int LOSE_XP = 40;
        private const int NO_ROUND_LOSS_BONUS = 50;

        // Hàm chính tính XP
        public int GetXP(
            bool isWinner,
            bool noRoundLost,
            int parryCount,
            int comboHitCount,
            int blockCount,
            int skillCount
        )
        {
            int totalXP = 0;

            // 1. XP thắng/thua
            totalXP += isWinner ? WIN_XP : LOSE_XP;

            // 2. Bonus nếu không thua hiệp nào
            if (noRoundLost)
                totalXP += NO_ROUND_LOSS_BONUS;

            // 3. Tính tổng điểm chiến đấu
            int score =
                (parryCount * 10) +
                (comboHitCount * 5) +
                (blockCount * 3) +
                (skillCount * 20);

            // 4. XP từ điểm (chia 5)
            totalXP += score / 5;

            return totalXP;
        }
    }
}
