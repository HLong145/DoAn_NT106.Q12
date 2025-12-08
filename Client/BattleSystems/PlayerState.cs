using System;
using System.Drawing;
using System.Windows.Forms;

namespace DoAn_NT106.Client.BattleSystems
{
    /// <summary>
    /// Quản lý trạng thái của một player trong trận đấu
    /// </summary>
    public class PlayerState
    {
        // Player info
        public string PlayerName { get; set; }
        public string CharacterType { get; set; }
        public int PlayerNumber { get; set; } // 1 or 2

        // Position and physics
        public int X { get; set; }
        public int Y { get; set; }
        public string Facing { get; set; } = "right";
        public bool IsJumping { get; set; }
        public float JumpVelocity { get; set; }

        // Animation state
        public string CurrentAnimation { get; set; } = "stand";
        public bool IsWalking { get; set; }
        public bool IsAttacking { get; set; }

        // Combat state
        public bool IsStunned { get; set; }
        public bool IsParrying { get; set; }
        public bool IsParryOnCooldown { get; set; }
        public bool IsDashing { get; set; }
        public bool IsCharging { get; set; }
        public bool IsSkillActive { get; set; }

        // Resources
        public int Health { get; set; } = 100;
        public int Stamina { get; set; } = 100;
        public int Mana { get; set; } = 0; // ✅ Khởi đầu với 0 (không phải 100)

        // Dash state
        public int DashDirection { get; set; }

        // Charge state (for Goatman)
        public float ChargeSpeed { get; set; }

        // Key states
        public bool LeftKeyPressed { get; set; }
        public bool RightKeyPressed { get; set; }

        // ✅ THÊM: Hệ thống hồi stamina/mana
        private long lastStaminaUsedTime = 0; // Thời điểm lần cuối dùng stamina (ms)
        private const long STAMINA_REGEN_DELAY_MS = 1000; // 1 giây trước khi bắt đầu hồi
        private const int STAMINA_REGEN_PER_SECOND = 20; // 20 stamina/s
        private const int MANA_REGEN_PER_SECOND = 2; // ✅ THAY ĐỔI: 1 mana/s -> 2 mana/s
        private const int MANA_REGEN_ON_HIT_MISS = 5; // Hồi 5 mana khi bị đánh trúng (không parry)
        private const int MANA_REGEN_ON_HIT_LAND = 5; // Hồi 5 mana khi đánh trúng
        private const int MANA_REGEN_ON_PARRY = 10; // Hồi 10 mana khi parry thành công
        private System.Windows.Forms.Timer manaRegenTimer; // ✅ FIX: Dùng fully qualified name
        private System.Windows.Forms.Timer staminaRegenTimer; // ✅ FIX: Dùng fully qualified name

        public PlayerState(string playerName, string characterType, int playerNumber)
        {
            PlayerName = playerName;
            CharacterType = characterType;
            PlayerNumber = playerNumber;
            InitializeRegenTimers();
        }

        /// <summary>
        /// Khởi tạo timers cho hệ thống hồi mana và stamina
        /// </summary>
        private void InitializeRegenTimers()
        {
            // Timer hồi mana mỗi 1 giây
            manaRegenTimer = new System.Windows.Forms.Timer();
            manaRegenTimer.Interval = 1000; // 1 giây
            manaRegenTimer.Tick += (s, e) =>
            {
                RegenerateMana(MANA_REGEN_PER_SECOND);
            };
            manaRegenTimer.Start();

            // Timer hồi stamina (sẽ được start sau 1 giây không dùng)
            staminaRegenTimer = new System.Windows.Forms.Timer();
            staminaRegenTimer.Interval = 1000; // 1 giây (check mỗi 1 giây hồi 20 stamina)
            staminaRegenTimer.Tick += (s, e) =>
            {
                RegenerateStamina(STAMINA_REGEN_PER_SECOND);
            };
        }

        /// <summary>
        /// Reset về trạng thái idle
        /// </summary>
        public void ResetToIdle()
        {
            if (!IsStunned && !IsParrying && !IsSkillActive && !IsJumping)
            {
                CurrentAnimation = (LeftKeyPressed || RightKeyPressed) ? "walk" : "stand";
            }
        }

        /// <summary>
        /// Apply damage to player
        /// </summary>
        public void TakeDamage(int damage)
        {
            Health = Math.Max(0, Health - damage);
        }

        /// <summary>
        /// Consume stamina
        /// </summary>
        public bool ConsumeStamina(int amount)
        {
            if (Stamina < amount) return false;
            Stamina -= amount;
            
            // ✅ THÊM: Reset timer hồi stamina khi dùng
            lastStaminaUsedTime = Environment.TickCount;
            if (staminaRegenTimer.Enabled)
            {
                staminaRegenTimer.Stop();
            }
            
            return true;
        }

        /// <summary>
        /// Consume mana
        /// </summary>
        public bool ConsumeMana(int amount)
        {
            if (Mana < amount) return false;
            Mana -= amount;
            return true;
        }

        /// <summary>
        /// ✅ THÊM: Hồi mana
        /// </summary>
        public void RegenerateMana(int amount)
        {
            if (Mana < 100)
            {
                Mana = Math.Min(100, Mana + amount);
            }
        }

        /// <summary>
        /// ✅ THÊM: Hồi stamina
        /// </summary>
        public void RegenerateStamina(int amount)
        {
            if (Stamina < 100)
            {
                Stamina = Math.Min(100, Stamina + amount);
            }
        }

        /// <summary>
        /// ✅ THÊM: Hồi mana khi bị đánh (không parry kịp)
        /// </summary>
        public void RegenerateManaOnHitMiss()
        {
            RegenerateMana(MANA_REGEN_ON_HIT_MISS);
        }

        /// <summary>
        /// ✅ THÊM: Hồi mana khi đánh trúng
        /// </summary>
        public void RegenerateManaOnHitLand()
        {
            RegenerateMana(MANA_REGEN_ON_HIT_LAND);
        }

        /// <summary>
        /// ✅ THÊM: Hồi mana khi parry thành công
        /// </summary>
        public void RegenerateManaOnParrySuccess()
        {
            RegenerateMana(MANA_REGEN_ON_PARRY);
        }

        /// <summary>
        /// ✅ THÊM: Kiểm tra và khởi động hồi stamina nếu đã đủ 1 giây không dùng
        /// </summary>
        public void UpdateStaminaRegenDelay()
        {
            long currentTime = Environment.TickCount;
            long timeSinceLastUse = currentTime - lastStaminaUsedTime;

            if (timeSinceLastUse >= STAMINA_REGEN_DELAY_MS && !staminaRegenTimer.Enabled)
            {
                // Đã đủ 1 giây không dùng, bắt đầu hồi
                staminaRegenTimer.Start();
            }
            else if (timeSinceLastUse < STAMINA_REGEN_DELAY_MS && staminaRegenTimer.Enabled)
            {
                // Nếu lại dùng stamina trong khoảng, dừng timer hồi
                staminaRegenTimer.Stop();
            }
        }

        /// <summary>
        /// Regenerate resources (LEGACY - kept for compatibility)
        /// </summary>
        public void RegenerateResources()
        {
            // Kiểm tra xem có nên bắt đầu hồi stamina không
            UpdateStaminaRegenDelay();
        }

        /// <summary>
        /// Check if player is dead
        /// </summary>
        public bool IsDead => Health <= 0;

        /// <summary>
        /// Check if player can move
        /// </summary>
        public bool CanMove => !IsStunned && !IsCharging && !IsDashing && !IsAttacking && !IsParrying; // ✅ THÊM: !IsParrying

        /// <summary>
        /// Check if player can attack
        /// </summary>
        public bool CanAttack => !IsStunned && !IsCharging && !IsDashing;

        /// <summary>
        /// Check if player can dash
        /// </summary>
        public bool CanDash => !IsStunned && !IsCharging && !IsDashing && !IsAttacking;

        /// <summary>
        /// Check if player can parry
        /// </summary>
        public bool CanParry => !IsStunned && !IsCharging && !IsDashing && !IsParryOnCooldown;

        /// <summary>
        /// ✅ THÊM: Cleanup timers khi form đóng
        /// </summary>
        public void Cleanup()
        {
            try
            {
                manaRegenTimer?.Stop();
                manaRegenTimer?.Dispose();
                staminaRegenTimer?.Stop();
                staminaRegenTimer?.Dispose();
            }
            catch { }
        }
    }
}
