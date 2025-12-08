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

        // ? Portrait PictureBoxes
        public PictureBox Portrait1 { get; private set; }
        public PictureBox Portrait2 { get; private set; }

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
            
            // ✅ SỬA: Lấy max HP theo character type
            int maxHP1 = GetMaxHealth(player1.CharacterType);
            int maxHP2 = GetMaxHealth(player2.CharacterType);
            
            // ✅ PORTRAIT SETUP
            int portraitSize = 80;
            // ✅ Portrait1 (Player 1 - bên trái)
            Portrait1 = new PictureBox
            {
                Location = new Point(20, startY + 3 * (barHeight + spacing) + 5),
                Size = new Size(portraitSize, portraitSize),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(50, 50, 50),
                BorderStyle = BorderStyle.Fixed3D,
                Image = FlipPortraitHorizontally(GetPortraitImage(player1.CharacterType))
            };

            // ✅ Portrait2 (Player 2 - bên phải)
            Portrait2 = new PictureBox
            {
                Location = new Point(screenWidth - portraitSize - 20, startY + 3 * (barHeight + spacing) + 5),
                Size = new Size(portraitSize, portraitSize),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(50, 50, 50),
                BorderStyle = BorderStyle.Fixed3D,
                Image = GetPortraitImage(player2.CharacterType)
            };

            // Player 1 bars
            HealthBar1 = new GameProgressBar
            {
                Location = new Point(20, startY),
                Size = new Size(barWidth, barHeight),
                Maximum = maxHP1,  // ✅ SỬA: Sử dụng max HP từ character type
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
                Maximum = maxHP2,  // ✅ SỬA: Sử dụng max HP từ character type
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

        // ✅ THÊM: Hàm lấy max HP theo character type
        private int GetMaxHealth(string characterType)
        {
            return characterType?.ToLower() switch
            {
                "goatman" => 130,
                "bringerofdeath" => 90,
                "warrior" => 80,
                "girlknight" => 100,
                "knightgirl" => 100,
                _ => 100  // Default
            };
        }

        /// <summary>
        /// Update all progress bars
        /// </summary>
        public void UpdateBars()
        {
            // ✅ SỬA: KHÔNG clamp value ở 100 nữa - để nó là giá trị thực tế
            // Vì Maximum đã được set theo max HP của character
            HealthBar1.Value = Math.Max(0, player1.Health);  // ✅ Không Math.Min(100, ...)
            StaminaBar1.Value = Math.Max(0, Math.Min(100, player1.Stamina));  // Stamina vẫn max 100
            ManaBar1.Value = Math.Max(0, Math.Min(100, player1.Mana));  // Mana vẫn max 100
            
            HealthBar2.Value = Math.Max(0, player2.Health);  // ✅ Không Math.Min(100, ...)
            StaminaBar2.Value = Math.Max(0, Math.Min(100, player2.Stamina));  // Stamina vẫn max 100
            ManaBar2.Value = Math.Max(0, Math.Min(100, player2.Mana));  // Mana vẫn max 100
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
            
            // ? Resize portraits
            int portraitSize = 80;
            Portrait1.Location = new Point(20, 10 + 3 * (20 + 5) + 5);
            Portrait1.Size = new Size(portraitSize, portraitSize);
            
            Portrait2.Location = new Point(screenWidth - portraitSize - 20, 10 + 3 * (20 + 5) + 5);
            Portrait2.Size = new Size(portraitSize, portraitSize);
        }

        /// <summary>
        /// Get portrait image based on character type
        /// </summary>
        private Image GetPortraitImage(string characterType)
        {
            try
            {
                return characterType?.ToLower() switch
                {
                    "warrior" => Properties.Resources.portrait_warrior,
                    "knightgirl" => TryLoadPortrait(Properties.Resources.portrait_knightgirl),
                    "girlknight" => TryLoadPortrait(Properties.Resources.portrait_knightgirl),
                    "bringerofdeath" => Properties.Resources.portrait_Bringer,
                    "goatman" => Properties.Resources.portrait_goatman,
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Safely load portrait resource
        /// </summary>
        private Image TryLoadPortrait(object resource)
        {
            try
            {
                if (resource is Image img)
                {
                    return new Bitmap(img);
                }
                if (resource is byte[] bytes && bytes.Length > 0)
                {
                    using (var ms = new System.IO.MemoryStream(bytes))
                    {
                        var image = Image.FromStream(ms);
                        return new Bitmap(image);
                    }
                }
                return resource as Image;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"? Error loading portrait: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Flip portrait image horizontally for player 1
        /// </summary>
        private Image FlipPortraitHorizontally(Image original)
        {
            if (original == null) return null;
            
            try
            {
                Bitmap flipped = new Bitmap(original);
                flipped.RotateFlip(RotateFlipType.RotateNoneFlipX);
                return flipped;
            }
            catch
            {
                return original;
            }
        }
    }
}
