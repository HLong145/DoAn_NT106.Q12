using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DoAn_NT106
{
    public enum PlayerAction
    {
        None,
        Stand,
        Walk,
        Jump,
        Punch,
        Kick,
        Slide,
        Parry,
        Hurt,
        Fireball
    }

    public class PlayerController
    {
        // Player identity
        public int PlayerId { get; private set; }
        public string PlayerName { get; set; }
        public string CharacterName { get; set; }

        // Position & Physics
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; private set; } = 80;
        public int Height { get; private set; } = 120;
        public string Facing { get; set; } = "right";

        // Movement
        private int moveSpeed = 14;
        private bool isMovingLeft = false;
        private bool isMovingRight = false;

        // Jump physics
        public bool IsJumping { get; private set; } = false;
        private float jumpVelocity = 0;
        private const float GRAVITY = 1.5f;
        private const float JUMP_FORCE = -18f;

        // Stats
        public int Health { get; set; } = 100;
        public int MaxHealth { get; } = 100;
        public int Stamina { get; set; } = 100;
        public int MaxStamina { get; } = 100;
        public int Mana { get; set; } = 100;
        public int MaxMana { get; } = 100;

        // Action states
        public PlayerAction CurrentAction { get; private set; } = PlayerAction.Stand;
        public bool IsAttacking { get; private set; } = false;
        public bool IsSliding { get; private set; } = false;
        public bool IsParrying { get; private set; } = false;
        public bool IsHurt { get; private set; } = false;

        // Parry system
        private bool parryOnCooldown = false;
        private const int PARRY_WINDOW_MS = 300;
        private const int PARRY_COOLDOWN_MS = 900;
        private const int PARRY_STAMINA_COST = 10;
        private System.Windows.Forms.Timer parryWindowTimer;
        private System.Windows.Forms.Timer parryCooldownTimer;

        // Slide system
        private int slideDirection = 0;
        private int slideFramesRemaining = 0;
        private const int SLIDE_DURATION_FRAMES = 15;
        private const int SLIDE_SPEED = 20;
        private const int SLIDE_STAMINA_COST = 15;
        private bool slideOnCooldown = false;
        private System.Windows.Forms.Timer slideCooldownTimer;
        private const int SLIDE_COOLDOWN_MS = 800;

        // Attack costs
        private const int PUNCH_STAMINA_COST = 10;
        private const int KICK_STAMINA_COST = 15;
        private const int FIREBALL_MANA_COST = 30;

        // Animation
        public Dictionary<string, Image> Animations { get; private set; }
        private System.Windows.Forms.Timer actionResetTimer;

        // Events
        public event Action<PlayerController, string, int> OnAttack; // player, attackType, damage
        public event Action<PlayerController, int, int, int> OnFireball; // player, x, y, direction
        public event Action<PlayerController> OnParrySuccess;
        public event Action<PlayerController, string> OnActionChanged;

        // Ground level reference
        private int groundLevel;

        // Key bindings
        private Keys keyLeft, keyRight, keyJump, keyPunch, keyKick, keySlide, keyParry;

        public PlayerController(int playerId, string playerName, string characterName = "girlknight")
        {
            PlayerId = playerId;
            PlayerName = playerName;
            CharacterName = characterName;
            Animations = new Dictionary<string, Image>();

            InitializeTimers();
            SetDefaultKeyBindings(playerId);
        }

        private void InitializeTimers()
        {
            parryWindowTimer = new System.Windows.Forms.Timer { Interval = PARRY_WINDOW_MS };
            parryWindowTimer.Tick += ParryWindowTimer_Tick;

            parryCooldownTimer = new System.Windows.Forms.Timer { Interval = PARRY_COOLDOWN_MS };
            parryCooldownTimer.Tick += ParryCooldownTimer_Tick;

            slideCooldownTimer = new System.Windows.Forms.Timer { Interval = SLIDE_COOLDOWN_MS };
            slideCooldownTimer.Tick += SlideCooldownTimer_Tick;

            actionResetTimer = new System.Windows.Forms.Timer();
            actionResetTimer.Tick += ActionResetTimer_Tick;
        }

        private void SetDefaultKeyBindings(int playerId)
        {
            if (playerId == 1)
            {
                // Player 1: WASD + JKL + U
                keyLeft = Keys.A;
                keyRight = Keys.D;
                keyJump = Keys.W;
                keyPunch = Keys.J;
                keyKick = Keys.K;
                keySlide = Keys.L;
                keyParry = Keys.U;
            }
            else
            {
                // Player 2: Arrow keys + Numpad
                keyLeft = Keys.Left;
                keyRight = Keys.Right;
                keyJump = Keys.Up;
                keyPunch = Keys.NumPad1;
                keyKick = Keys.NumPad2;
                keySlide = Keys.NumPad3;
                keyParry = Keys.NumPad5;
            }
        }

        public void SetKeyBindings(Keys left, Keys right, Keys jump, Keys punch, Keys kick, Keys slide, Keys parry)
        {
            keyLeft = left;
            keyRight = right;
            keyJump = jump;
            keyPunch = punch;
            keyKick = kick;
            keySlide = slide;
            keyParry = parry;
        }

        public void SetGroundLevel(int ground)
        {
            groundLevel = ground;
            if (!IsJumping)
            {
                Y = groundLevel - Height;
            }
        }

        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        #region Input Handling

        public void HandleKeyDown(Keys key)
        {
            if (key == keyLeft)
            {
                isMovingLeft = true;
                Facing = "left";
            }
            else if (key == keyRight)
            {
                isMovingRight = true;
                Facing = "right";
            }
            else if (key == keyJump)
            {
                TryJump();
            }
            else if (key == keyPunch)
            {
                TryPunch();
            }
            else if (key == keyKick)
            {
                TryKick();
            }
            else if (key == keySlide)
            {
                TrySlide();
            }
            else if (key == keyParry)
            {
                TryParry();
            }
        }

        public void HandleKeyUp(Keys key)
        {
            if (key == keyLeft)
            {
                isMovingLeft = false;
            }
            else if (key == keyRight)
            {
                isMovingRight = false;
            }
        }

        #endregion

        #region Actions

        private void TryJump()
        {
            if (!IsJumping && Y >= groundLevel - Height && !IsSliding)
            {
                IsJumping = true;
                jumpVelocity = JUMP_FORCE;
                SetAction(PlayerAction.Jump);
            }
        }

        private void TryPunch()
        {
            if (IsAttacking || IsSliding || IsParrying) return;
            if (Stamina < PUNCH_STAMINA_COST) return;

            Stamina -= PUNCH_STAMINA_COST;
            IsAttacking = true;
            SetAction(PlayerAction.Punch);

            OnAttack?.Invoke(this, "punch", 10);
            StartActionReset(300);
        }

        private void TryKick()
        {
            if (IsAttacking || IsSliding || IsParrying) return;
            if (Stamina < KICK_STAMINA_COST) return;

            Stamina -= KICK_STAMINA_COST;
            IsAttacking = true;
            SetAction(PlayerAction.Kick);

            OnAttack?.Invoke(this, "kick", 15);
            StartActionReset(400);
        }

        private void TrySlide()
        {
            if (IsSliding || slideOnCooldown || IsAttacking || IsParrying || IsJumping) return;
            if (Stamina < SLIDE_STAMINA_COST) return;

            Stamina -= SLIDE_STAMINA_COST;
            IsSliding = true;
            slideDirection = Facing == "right" ? 1 : -1;
            slideFramesRemaining = SLIDE_DURATION_FRAMES;
            SetAction(PlayerAction.Slide);
        }

        private void TryParry()
        {
            if (parryOnCooldown || IsParrying || IsAttacking || IsSliding) return;
            if (Stamina < PARRY_STAMINA_COST) return;

            Stamina -= PARRY_STAMINA_COST;
            IsParrying = true;
            SetAction(PlayerAction.Parry);

            parryWindowTimer.Stop();
            parryWindowTimer.Start();
        }

        public void TryFireball()
        {
            if (IsAttacking || IsSliding || IsParrying) return;
            if (Mana < FIREBALL_MANA_COST) return;

            Mana -= FIREBALL_MANA_COST;
            IsAttacking = true;
            SetAction(PlayerAction.Fireball);

            int direction = Facing == "right" ? 1 : -1;
            int startX = Facing == "right" ? X + Width : X - 150; // 150 = fireball width
            OnFireball?.Invoke(this, startX, Y + 30, direction);

            StartActionReset(500);
        }

        #endregion

        #region Update Logic

        public void Update(int backgroundWidth)
        {
            // Handle sliding
            if (IsSliding)
            {
                if (slideFramesRemaining > 0)
                {
                    X += SLIDE_SPEED * slideDirection;
                    slideFramesRemaining--;
                }
                else
                {
                    IsSliding = false;
                    slideOnCooldown = true;
                    slideCooldownTimer.Start();
                    UpdateIdleState();
                }
            }

            // Handle normal movement (not during slide or attack)
            if (!IsSliding && !IsAttacking && !IsParrying && !IsHurt)
            {
                if (isMovingLeft)
                {
                    X -= moveSpeed;
                    Facing = "left";
                    if (!IsJumping) SetAction(PlayerAction.Walk);
                }
                else if (isMovingRight)
                {
                    X += moveSpeed;
                    Facing = "right";
                    if (!IsJumping) SetAction(PlayerAction.Walk);
                }
                else if (!IsJumping)
                {
                    SetAction(PlayerAction.Stand);
                }
            }

            // Handle jumping
            if (IsJumping)
            {
                Y += (int)jumpVelocity;
                jumpVelocity += GRAVITY;

                if (Y >= groundLevel - Height)
                {
                    Y = groundLevel - Height;
                    IsJumping = false;
                    jumpVelocity = 0;
                    UpdateIdleState();
                }
            }

            // Clamp position
            X = Math.Max(0, Math.Min(backgroundWidth - Width, X));

            // Regenerate resources
            RegenerateResources();
        }

        private void UpdateIdleState()
        {
            if (!IsAttacking && !IsSliding && !IsParrying && !IsHurt)
            {
                if (isMovingLeft || isMovingRight)
                {
                    SetAction(PlayerAction.Walk);
                }
                else
                {
                    SetAction(PlayerAction.Stand);
                }
            }
        }

        private void RegenerateResources()
        {
            if (Stamina < MaxStamina) Stamina = Math.Min(MaxStamina, Stamina + 1);
            if (Mana < MaxMana) Mana = Math.Min(MaxMana, Mana + 1);
        }

        #endregion

        #region Damage & Parry

        public bool TakeDamage(int damage, PlayerController attacker)
        {
            // Check if parrying
            if (IsParrying)
            {
                // Successful parry - block damage
                Stamina = Math.Min(MaxStamina, Stamina + 8); // Reward stamina
                OnParrySuccess?.Invoke(this);
                return false; // Damage blocked
            }

            // Take damage
            Health = Math.Max(0, Health - damage);
            IsHurt = true;
            SetAction(PlayerAction.Hurt);

            // Knockback
            int knockbackDir = attacker.X < X ? 1 : -1;
            X += knockbackDir * 20;

            // Reset hurt state after delay
            System.Windows.Forms.Timer hurtTimer = new System.Windows.Forms.Timer { Interval = 400 };
            hurtTimer.Tick += (s, e) =>
            {
                hurtTimer.Stop();
                hurtTimer.Dispose();
                IsHurt = false;
                UpdateIdleState();
            };
            hurtTimer.Start();

            return true; // Damage applied
        }

        public bool TakeFireballDamage(int damage, int fireballOwner)
        {
            if (IsParrying)
            {
                OnParrySuccess?.Invoke(this);
                return false; // Reflected
            }

            Health = Math.Max(0, Health - damage);
            IsHurt = true;
            SetAction(PlayerAction.Hurt);

            System.Windows.Forms.Timer hurtTimer = new System.Windows.Forms.Timer { Interval = 400 };
            hurtTimer.Tick += (s, e) =>
            {
                hurtTimer.Stop();
                hurtTimer.Dispose();
                IsHurt = false;
                UpdateIdleState();
            };
            hurtTimer.Start();

            return true;
        }

        #endregion

        #region Timer Handlers

        private void ParryWindowTimer_Tick(object sender, EventArgs e)
        {
            parryWindowTimer.Stop();
            IsParrying = false;
            parryOnCooldown = true;
            parryCooldownTimer.Start();
            UpdateIdleState();
        }

        private void ParryCooldownTimer_Tick(object sender, EventArgs e)
        {
            parryCooldownTimer.Stop();
            parryOnCooldown = false;
        }

        private void SlideCooldownTimer_Tick(object sender, EventArgs e)
        {
            slideCooldownTimer.Stop();
            slideOnCooldown = false;
        }

        private void ActionResetTimer_Tick(object sender, EventArgs e)
        {
            actionResetTimer.Stop();
            IsAttacking = false;
            UpdateIdleState();
        }

        private void StartActionReset(int delay)
        {
            actionResetTimer.Stop();
            actionResetTimer.Interval = delay;
            actionResetTimer.Start();
        }

        #endregion

        #region Animation

        private void SetAction(PlayerAction action)
        {
            if (CurrentAction != action)
            {
                CurrentAction = action;
                OnActionChanged?.Invoke(this, GetAnimationName(action));
            }
        }

        public string GetAnimationName(PlayerAction action)
        {
            return action switch
            {
                PlayerAction.Stand => "stand",
                PlayerAction.Walk => "walk",
                PlayerAction.Jump => "jump",
                PlayerAction.Punch => "punch",
                PlayerAction.Kick => "kick",
                PlayerAction.Slide => "slide",
                PlayerAction.Parry => "parry",
                PlayerAction.Hurt => "hurt",
                PlayerAction.Fireball => "fireball",
                _ => "stand"
            };
        }

        public string GetCurrentAnimationName()
        {
            return GetAnimationName(CurrentAction);
        }

        public Image GetCurrentAnimationImage()
        {
            string animName = GetCurrentAnimationName();
            if (Animations.ContainsKey(animName) && Animations[animName] != null)
            {
                return Animations[animName];
            }
            // Fallback
            if (Animations.ContainsKey("stand"))
            {
                return Animations["stand"];
            }
            return null;
        }

        #endregion

        #region Collision

        public Rectangle GetHitbox()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        public bool CollidesWith(PlayerController other)
        {
            return GetHitbox().IntersectsWith(other.GetHitbox());
        }

        public bool CollidesWith(Rectangle rect)
        {
            return GetHitbox().IntersectsWith(rect);
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            parryWindowTimer?.Stop();
            parryWindowTimer?.Dispose();
            parryCooldownTimer?.Stop();
            parryCooldownTimer?.Dispose();
            slideCooldownTimer?.Stop();
            slideCooldownTimer?.Dispose();
            actionResetTimer?.Stop();
            actionResetTimer?.Dispose();
        }

        #endregion
    }
}