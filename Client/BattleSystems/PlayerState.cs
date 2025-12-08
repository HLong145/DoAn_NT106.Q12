using System;
using System.Drawing;
using System.Windows.Forms;

namespace DoAn_NT106.Client.BattleSystems
{
    /// <summary>
    /// Qu?n lý tr?ng thái c?a m?t player trong tr?n ??u
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
        public int Mana { get; set; } = 0; // ? Kh?i ??u v?i 0 (không ph?i 100)

        // Dash state
        public int DashDirection { get; set; }

        // Charge state (for Goatman)
        public float ChargeSpeed { get; set; }

        // Key states
        public bool LeftKeyPressed { get; set; }
        public bool RightKeyPressed { get; set; }

        // ? THÊM: H? th?ng h?i stamina/mana
        private long lastStaminaUsedTime = 0; // Th?i ?i?m l?n cu?i dùng stamina (ms)
        private const long STAMINA_REGEN_DELAY_MS = 1000; // 1 giây tr??c khi b?t ??u h?i
        private const int STAMINA_REGEN_PER_SECOND = 20; // 20 stamina/s
        private const int MANA_REGEN_PER_SECOND = 2; // ? THAY ??I: 1 mana/s -> 2 mana/s
        private const int MANA_REGEN_ON_HIT_MISS = 5; // H?i 5 mana khi b? ?ánh trúng (không parry)
        private const int MANA_REGEN_ON_HIT_LAND = 5; // H?i 5 mana khi ?ánh trúng
        private const int MANA_REGEN_ON_PARRY = 10; // H?i 10 mana khi parry thành công
        private System.Windows.Forms.Timer manaRegenTimer; // ? FIX: Dùng fully qualified name
        private System.Windows.Forms.Timer staminaRegenTimer; // ? FIX: Dùng fully qualified name

        public PlayerState(string playerName, string characterType, int playerNumber)
        {
            PlayerName = playerName;
            CharacterType = characterType;
            PlayerNumber = playerNumber;
            InitializeRegenTimers();
        }

        /// <summary>
        /// Kh?i t?o timers cho h? th?ng h?i mana và stamina
        /// </summary>
        private void InitializeRegenTimers()
        {
            // Timer h?i mana m?i 1 giây
            manaRegenTimer = new System.Windows.Forms.Timer();
            manaRegenTimer.Interval = 1000; // 1 giây
            manaRegenTimer.Tick += (s, e) =>
            {
                RegenerateMana(MANA_REGEN_PER_SECOND);
            };
            manaRegenTimer.Start();

            // Timer h?i stamina (s? ???c start sau 1 giây không dùng)
            staminaRegenTimer = new System.Windows.Forms.Timer();
            staminaRegenTimer.Interval = 1000; // 1 giây (check m?i 1 giây h?i 20 stamina)
            staminaRegenTimer.Tick += (s, e) =>
            {
                RegenerateStamina(STAMINA_REGEN_PER_SECOND);
            };
        }

        /// <summary>
        /// Reset v? tr?ng thái idle
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
            
            // ? THÊM: Reset timer h?i stamina khi dùng
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
        /// ? THÊM: H?i mana
        /// </summary>
        public void RegenerateMana(int amount)
        {
            if (Mana < 100)
            {
                Mana = Math.Min(100, Mana + amount);
            }
        }

        /// <summary>
        /// ? THÊM: H?i stamina
        /// </summary>
        public void RegenerateStamina(int amount)
        {
            if (Stamina < 100)
            {
                Stamina = Math.Min(100, Stamina + amount);
            }
        }

        /// <summary>
        /// ? THÊM: H?i mana khi b? ?ánh (không parry k?p)
        /// </summary>
        public void RegenerateManaOnHitMiss()
        {
            RegenerateMana(MANA_REGEN_ON_HIT_MISS);
        }

        /// <summary>
        /// ? THÊM: H?i mana khi ?ánh trúng
        /// </summary>
        public void RegenerateManaOnHitLand()
        {
            RegenerateMana(MANA_REGEN_ON_HIT_LAND);
        }

        /// <summary>
        /// ? THÊM: H?i mana khi parry thành công
        /// </summary>
        public void RegenerateManaOnParrySuccess()
        {
            RegenerateMana(MANA_REGEN_ON_PARRY);
        }

        /// <summary>
        /// ? THÊM: Ki?m tra và kh?i ??ng h?i stamina n?u ?ã ?? 1 giây không dùng
        /// </summary>
        public void UpdateStaminaRegenDelay()
        {
            long currentTime = Environment.TickCount;
            long timeSinceLastUse = currentTime - lastStaminaUsedTime;

            if (timeSinceLastUse >= STAMINA_REGEN_DELAY_MS && !staminaRegenTimer.Enabled)
            {
                // ?ã ?? 1 giây không dùng, b?t ??u h?i
                staminaRegenTimer.Start();
            }
            else if (timeSinceLastUse < STAMINA_REGEN_DELAY_MS && staminaRegenTimer.Enabled)
            {
                // N?u l?i dùng stamina trong kho?ng, d?ng timer h?i
                staminaRegenTimer.Stop();
            }
        }

        /// <summary>
        /// Regenerate resources (LEGACY - kept for compatibility)
        /// </summary>
        public void RegenerateResources()
        {
            // Ki?m tra xem có nên b?t ??u h?i stamina không
            UpdateStaminaRegenDelay();
        }

        /// <summary>
        /// Check if player is dead
        /// </summary>
        public bool IsDead => Health <= 0;

        /// <summary>
        /// Check if player can move
        /// </summary>
        public bool CanMove => !IsStunned && !IsCharging && !IsDashing && !IsAttacking && !IsParrying; // ? THÊM: !IsParrying

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
        /// ? THÊM: Cleanup timers khi form ?óng
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
