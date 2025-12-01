using System.Drawing;

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
        public int Mana { get; set; } = 100;

        // Dash state
        public int DashDirection { get; set; }

        // Charge state (for Goatman)
        public float ChargeSpeed { get; set; }

        // Key states
        public bool LeftKeyPressed { get; set; }
        public bool RightKeyPressed { get; set; }

        public PlayerState(string playerName, string characterType, int playerNumber)
        {
            PlayerName = playerName;
            CharacterType = characterType;
            PlayerNumber = playerNumber;
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
        /// Regenerate resources
        /// </summary>
        public void RegenerateResources()
        {
            if (Stamina < 100) Stamina = Math.Min(100, Stamina + 2);
            if (Mana < 100) Mana = Math.Min(100, Mana + 1);
        }

        /// <summary>
        /// Check if player is dead
        /// </summary>
        public bool IsDead => Health <= 0;

        /// <summary>
        /// Check if player can move
        /// </summary>
        public bool CanMove => !IsStunned && !IsCharging && !IsDashing && !IsAttacking;

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
    }
}
