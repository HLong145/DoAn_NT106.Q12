using System.Drawing;

namespace DoAn_NT106.Client.BattleSystems
{
    /// <summary>
    /// Qu?n lý resources (health, stamina, mana) c?a players
    /// </summary>
    public class ResourceSystem
    {
        private PlayerState player1;
        private PlayerState player2;

        // UI Components
        public GameProgressBar HealthBar1 { get; private set; }
        public GameProgressBar StaminaBar1 { get; private set; }
        public GameProgressBar ManaBar1 { get; private set; }
        public GameProgressBar HealthBar2 { get; private set; }
        public GameProgressBar StaminaBar2 { get; private set; }
        public GameProgressBar ManaBar2 { get; private set; }

        public ResourceSystem(PlayerState p1, PlayerState p2)
        {
            player1 = p1;
            player2 = p2;
        }

        /// <summary>
        /// Setup status bars UI
        /// </summary>
        public void SetupStatusBars(int screenWidth)
        {
            int barWidth = screenWidth / 4;
            int barHeight = 20;
            int spacing = 5;
            int startY = 10;

            // Player 1 bars
            HealthBar1 = new GameProgressBar
            {
                Location = new Point(20, startY),
                Size = new Size(barWidth, barHeight),
                Maximum = 100,
                Value = player1.Health,
                CustomForeColor = Color.FromArgb(220, 50, 50),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            StaminaBar1 = new GameProgressBar
            {
                Location = new Point(20, startY + barHeight + spacing),
                Size = new Size(barWidth, barHeight),
                Maximum = 100,
                Value = player1.Stamina,
                CustomForeColor = Color.FromArgb(50, 220, 50),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            ManaBar1 = new GameProgressBar
            {
                Location = new Point(20, startY + 2 * (barHeight + spacing)),
                Size = new Size(barWidth, barHeight),
                Maximum = 100,
                Value = player1.Mana,
                CustomForeColor = Color.FromArgb(50, 100, 220),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            // Player 2 bars
            HealthBar2 = new GameProgressBar
            {
                Location = new Point(screenWidth - barWidth - 20, startY),
                Size = new Size(barWidth, barHeight),
                Maximum = 100,
                Value = player2.Health,
                CustomForeColor = Color.FromArgb(220, 50, 50),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            StaminaBar2 = new GameProgressBar
            {
                Location = new Point(screenWidth - barWidth - 20, startY + barHeight + spacing),
                Size = new Size(barWidth, barHeight),
                Maximum = 100,
                Value = player2.Stamina,
                CustomForeColor = Color.FromArgb(50, 220, 50),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            ManaBar2 = new GameProgressBar
            {
                Location = new Point(screenWidth - barWidth - 20, startY + 2 * (barHeight + spacing)),
                Size = new Size(barWidth, barHeight),
                Maximum = 100,
                Value = player2.Mana,
                CustomForeColor = Color.FromArgb(50, 100, 220),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
        }

        /// <summary>
        /// Update all progress bars - ? ?Ã S?A: Thêm debug log
        /// </summary>
        public void UpdateBars()
        {
            // ? C?P NH?T VÀ INVALIDATE
            HealthBar1.Value = Math.Max(0, Math.Min(100, player1.Health));
            StaminaBar1.Value = Math.Max(0, Math.Min(100, player1.Stamina));
            ManaBar1.Value = Math.Max(0, Math.Min(100, player1.Mana));
            
            HealthBar2.Value = Math.Max(0, Math.Min(100, player2.Health));
            StaminaBar2.Value = Math.Max(0, Math.Min(100, player2.Stamina));
            ManaBar2.Value = Math.Max(0, Math.Min(100, player2.Mana));

            // ? DEBUG LOG - xóa sau khi test xong
            // System.Console.WriteLine($"[UpdateBars] P1: HP={player1.Health}, Sta={player1.Stamina}, Mana={player1.Mana}");
            // System.Console.WriteLine($"[UpdateBars] P2: HP={player2.Health}, Sta={player2.Stamina}, Mana={player2.Mana}");
        }

        /// <summary>
        /// Regenerate resources for both players
        /// </summary>
        public void RegenerateResources()
        {
            player1.RegenerateResources();
            player2.RegenerateResources();
        }

        /// <summary>
        /// Resize bars when window resizes
        /// </summary>
        public void ResizeBars(int screenWidth)
        {
            int barWidth = screenWidth / 4;

            HealthBar1.Size = new Size(barWidth, 20);
            StaminaBar1.Size = new Size(barWidth, 20);
            ManaBar1.Size = new Size(barWidth, 20);

            HealthBar2.Location = new Point(screenWidth - barWidth - 20, 10);
            HealthBar2.Size = new Size(barWidth, 20);
            StaminaBar2.Location = new Point(screenWidth - barWidth - 20, 35);
            StaminaBar2.Size = new Size(barWidth, 20);
            ManaBar2.Location = new Point(screenWidth - barWidth - 20, 60);
            ManaBar2.Size = new Size(barWidth, 20);
        }
    }
}
