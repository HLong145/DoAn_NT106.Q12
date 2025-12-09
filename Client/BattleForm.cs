using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DoAn_NT106.Client.BattleSystems; // ✅ THÊM NAMESPACE MỚI
using DoAn_NT106.Client; // ✅ THÊM CHO SOUNDMANAGER

namespace DoAn_NT106
{
    // Helper class for hit effects
    public partial class BattleForm : Form
    {
        private string username;
        private string token;
        private string opponent;

        // ===== ✅ NEW SYSTEMS (ADDED) =====
        private PlayerState player1State;
        private PlayerState player2State;
        private ResourceSystem resourceSystem;
        private PhysicsSystem physicsSystem;
        private EffectManager effectManager;
        private ProjectileManager projectileManager;
        private CombatSystem combatSystem;
        // ==================================

        // Game assets
        private Image background;
        private Image fireball;
        private List<string> backgroundImages = new List<string>();

        // Player positions (x = left, y = top) - ⚠️ WILL BE MIGRATED
        private int player1X = 300;
        private int player1Y;
        private int player2X = 600;
        private int player2Y;
        private int playerSpeed = 14;

        // Kích thước background và viewport
        private int backgroundWidth = 2000;
        private int viewportX = 0;
        private int groundLevel = 520;            // cập nhật động theo kích thước form
        private int groundOffset = 150;           // khoảng cách từ đáy cửa sổ tới "mặt đất" (tùy chỉnh theo background)
        private List<System.IO.Stream> resourceStreams = new List<System.IO.Stream>(); // giữ stream cho GIF nếu cần

        // groundOffset tùy thuộc vào background đã chọn
        private Dictionary<string, int> backgroundGroundOffsets = new Dictionary<string, int>{
            {"battleground1", 140},
            {"battleground2", 160},
            {"battleground3", 150},
            {"battleground4", 170}
        };

        // Vật lý nhảy
        private bool player1Jumping = false;
        private bool player2Jumping = false;
        private float player1JumpVelocity = 0;
        private float player2JumpVelocity = 0;
        private const float GRAVITY = 1.5f;
        private const float JUMP_FORCE = -10f;

        // Health, Stamina và Mana
        private int player1Health = 100;
        private int player2Health = 100;
        private int player1Stamina = 100;
        private int player2Stamina = 100;
        private int player1Mana = 100;
        private int player2Mana = 100;

        // Animation system
        private CharacterAnimationManager player1AnimationManager;
        private CharacterAnimationManager player2AnimationManager;
        private string player1CurrentAnimation = "stand";
        private string player2CurrentAnimation = "stand";
        private string player1Facing = "right";
        private string player2Facing = "left";
        private bool player1Walking = false;
        private bool player2Walking = false;
        private bool player1Attacking = false;
        private bool player2Attacking = false;
        private System.Windows.Forms.Timer walkAnimationTimer;
        private bool player1AttackHitProcessed = false;
        private bool player2AttackHitProcessed = false;
        private int player1AttackFrameCounter = 0;
        private int player2AttackFrameCounter = 0;
        // ✅ THÊM: Theo dõi hit nào đã xử lý (cho đòn đánh nhiều lần như Warrior attack1)
        private HashSet<int> player1ProcessedHitFrames = new HashSet<int>();
        private HashSet<int> player2ProcessedHitFrames = new HashSet<int>();
        // Progress bars (assume GameProgressBar exists in project)
        // ❌ Removed legacy bar fields (managed by ResourceSystem)
        // private GameProgressBar healthBar1, healthBar2;
        // private GameProgressBar staminaBar1, staminaBar2;
        // private GameProgressBar manaBar1, manaBar2;
        private Label lblPlayer1Name, lblPlayer2Name;
        private Label lblControlsInfo;
        // Parry (đỡ)
        private bool player1Parrying = false;
        private bool player2Parrying = false;
        private bool player1ParryOnCooldown = false;
        private bool player2ParryOnCooldown = false;
        private int parryWindowMs = 300;      // cửa sổ parry (ms)
        private int parryCooldownMs = 900;    // cooldown sau khi parry (ms)
        private int parryStaminaCost = 10;    // tốn stamina khi bật parry
        private System.Windows.Forms.Timer p1ParryTimer;
        private System.Windows.Forms.Timer p1ParryCooldownTimer;
        private System.Windows.Forms.Timer p2ParryTimer;
        private System.Windows.Forms.Timer p2ParryCooldownTimer;
        // Fireball
        private bool fireballActive = false;
        private int fireballX, fireballY;
        private int fireballDirection = 1;
        private int fireballOwner = 0; // 1 or 2, who shot the fireball
        private const int FIREBALL_WIDTH = 150;
        private const int FIREBALL_HEIGHT = 100;
        private int currentBackground = 0;
        private int fireballSpeed = 1;

        // Spell System (for Bringer of Death)
        private bool spellActive = false;
        private int spellX, spellY;
        private int spellOwner = 0;
        private Image spellAnimation = null;
        private const int SPELL_WIDTH = 120;
        private const int SPELL_HEIGHT = 120;
        private const int SPELL_DAMAGE_DELAY_MS = 200;
        private System.Windows.Forms.Timer spellDamageTimer = null;

        // Dash System
        private bool player1Dashing = false;
        private bool player2Dashing = false;
        private int player1DashDirection = 0;
        private int player2DashDirection = 0;
        private const int DASH_DISTANCE = 400; // Tăng từ 100 lên 400
        private const int DASH_DURATION_MS = 200;
        private System.Windows.Forms.Timer player1DashTimer;
        private System.Windows.Forms.Timer player2DashTimer;
        private Image dashEffectImage = null;
        private bool dashEffect1Active = false;
        private bool dashEffect2Active = false;
        private int dashEffect1X, dashEffect1Y;
        private int dashEffect2X, dashEffect2Y;
        private string dashEffect1Facing, dashEffect2Facing;

        // ✅ THÊM: Dash effect GIF cho Bringer of Death và Goatman
        private Image dashEffectGif = null;
        private bool player1DashEffectActive = false;
        private bool player2DashEffectActive = false;
        private int player1DashEffectX, player1DashEffectY;
        private int player2DashEffectX, player2DashEffectY;
        private string player1DashEffectFacing;
        private string player2DashEffectFacing;
        private System.Windows.Forms.Timer player1DashEffectTimer;
        private System.Windows.Forms.Timer player2DashEffectTimer;

        // Knight Girl Skill System
        private bool player1SkillActive = false;
        private bool player2SkillActive = false;
        private System.Windows.Forms.Timer player1SkillTimer;
        private System.Windows.Forms.Timer player2SkillTimer;
        private const int SKILL_MANA_COST_PER_SECOND = 30;
        private const int SKILL_DAMAGE_INTERVAL_MS = 500;
        private int player1SkillDamageCounter = 0;
        private int player2SkillDamageCounter = 0;
        // Xử lý skill Knight Girl (10fps, 5 frame, đánh 2 lần ở 0.5s và 1s)
        private int player1SkillHitCounter = 0;
        private int player2SkillHitCounter = 0;
        private const int KNIGHT_SKILL_HIT_INTERVAL_MS = 500; // 0.5s giữa 2 lần đánh


        // Goatman Charge System
        private bool player1Charging = false;
        private bool player2Charging = false;
        private float player1ChargeSpeed = 0;
        private float player2ChargeSpeed = 0;
        private const float CHARGE_ACCELERATION = 20f; // Tăng từ 12f lên 20f để nhanh hơn
        private const float CHARGE_MAX_SPEED = 30f;
        private const int CHARGE_DURATION_MS = 3000;
        private System.Windows.Forms.Timer player1ChargeTimer;
        private System.Windows.Forms.Timer player2ChargeTimer;

        // Warrior Projectile System
        private bool projectile1Active = false;
        private bool projectile2Active = false;
        private int projectile1X, projectile1Y, projectile1Direction;
        private int projectile2X, projectile2Y, projectile2Direction;
        private Image warriorSkillEffect = null;
        private const int PROJECTILE_SPEED = 23; // Tăng từ 15 lên 23 (x1.5 ~= 22.5)
        private const int PROJECTILE_WIDTH = 160; // Tăng gấp đôi từ 80 lên 160
        private const int PROJECTILE_HEIGHT = 160; // Tăng gấp đôi từ 80 lên 160

        // Impact Effect System
        private Image gmImpactEffect = null;
        private bool impact1Active = false;
        private bool impact2Active = false;
        private int impact1X, impact1Y;
        private int impact2X, impact2Y;
        private string impact1Facing, impact2Facing;
        private System.Windows.Forms.Timer impact1Timer;
        private System.Windows.Forms.Timer impact2Timer;

        // Hit Effect System
        private Image hitEffectImage = null;
        private List<HitEffectInstance> activeHitEffects = new List<HitEffectInstance>();

        // Stun System
        private bool player1Stunned = false;
        private bool player2Stunned = false;
        private const int HIT_STUN_DURATION_MS = 200;
        private const int HIT_EFFECT_DURATION_MS = 150;

        // Key states
        private bool aPressed, dPressed;
        private bool leftPressed, rightPressed;

        // Kích thước nhân vật (dynamic)
        private int PLAYER_WIDTH = 80;
        private int PLAYER_HEIGHT = 120;
        private float characterHeightRatio = 0.30f; // relative to ClientSize.Height
        private float globalCharacterScale = 1.5f;

        // Hitbox configuration - nhỏ hơn và hướng theo facing
        private int HITBOX_WIDTH_RATIO = 2; // Hitbox = PLAYER_WIDTH / 2
        private int HITBOX_HEIGHT_RATIO = 2; // Hitbox = PLAYER_HEIGHT / 2
                                             // ✅ THÊM: Class cấu hình vùng tấn công

        public class HitEffectInstance
        {
            public int X { get; set; }
            public int Y { get; set; }
            public System.Windows.Forms.Timer Timer { get; set; }
            public Image EffectImage { get; set; }
        }

        public class AttackHitboxConfig
        {
            public float WidthPercent { get; set; }      // Chiều rộng vùng tấn công
            public float HeightPercent { get; set; }     // Chiều cao vùng tấn công
            public float RangePercent { get; set; }      // Khoảng cách tấn công (nhân với PLAYER_WIDTH)
            public float OffsetYPercent { get; set; }    // Độ cao của vùng tấn công
        }

        public class AttackAnimationConfig
        {
            public int FPS { get; set; }
            public int TotalFrames { get; set; }
            public List<int> HitFrames { get; set; } // Các frame gây sát thương
            public int Duration => (int)((TotalFrames / (float)FPS) * 1000); // ms
        }

        private Dictionary<string, Dictionary<string, AttackAnimationConfig>> characterAnimationConfigs = new Dictionary<string, Dictionary<string, AttackAnimationConfig>>
        {
            ["goatman"] = new Dictionary<string, AttackAnimationConfig>
            {
                ["attack1"] = new AttackAnimationConfig
                {
                    FPS = 11,
                    TotalFrames = 6,
                    HitFrames = new List<int> { 4 }
                },
                ["attack2"] = new AttackAnimationConfig
                {
                    FPS = 9,
                    TotalFrames = 5,
                    HitFrames = new List<int> { 4 }
                },
                ["punch"] = new AttackAnimationConfig
                {
                    FPS = 10,
                    TotalFrames = 5,
                    HitFrames = new List<int> { 3 }
                },
                ["kick"] = new AttackAnimationConfig
                {
                    FPS = 8,
                    TotalFrames = 5,
                    HitFrames = new List<int> { 3 }
                }
                ,
                 ["skill"] = new AttackAnimationConfig
                 {
                     FPS = 12,
                     TotalFrames = 8,
                     HitFrames = new List<int> { 3, 4, 5, 6 } // Đâm liên tục khi charge
                 }
            },
            ["bringerofdeath"] = new Dictionary<string, AttackAnimationConfig>
            {
                ["attack1"] = new AttackAnimationConfig
                {
                    FPS = 8,
                    TotalFrames = 10,
                    HitFrames = new List<int> { 6 }
                },
                ["attack2"] = new AttackAnimationConfig
                {
                    FPS = 18,
                    TotalFrames = 10,
                    HitFrames = new List<int> { 6 }
                },
                ["punch"] = new AttackAnimationConfig
                {
                    FPS = 10,
                    TotalFrames = 8,
                    HitFrames = new List<int> { 5 }
                },
                ["kick"] = new AttackAnimationConfig
                {
                    FPS = 10,
                    TotalFrames = 8,
                    HitFrames = new List<int> { 5 }
                },
                ["skill"] = new AttackAnimationConfig
                {
                    FPS = 10,
                    TotalFrames = 13,
                    HitFrames = new List<int> { 8 } // Cast spell ở frame 8
                }
            },
            ["girlknight"] = new Dictionary<string, AttackAnimationConfig>
            {
                ["attack1"] = new AttackAnimationConfig
                {
                    FPS = 6,
                    TotalFrames = 6,
                    HitFrames = new List<int> { 3 }
                },
                ["attack2"] = new AttackAnimationConfig
                {
                    FPS = 6,
                    TotalFrames = 9,
                    HitFrames = new List<int> { 6 }
                },
                ["punch"] = new AttackAnimationConfig
                {
                    FPS = 8,
                    TotalFrames = 5,
                    HitFrames = new List<int> { 3 }
                },
                ["kick"] = new AttackAnimationConfig
                {
                    FPS = 8,
                    TotalFrames = 9,
                    HitFrames = new List<int> { 3 }
                },
                ["skill"] = new AttackAnimationConfig
                {
                    FPS = 10,
                    TotalFrames = 5,
                    HitFrames = new List<int> { } // Xử lý riêng bởi timer (hit ở 0.5s và 1s)
                }
            },
            ["warrior"] = new Dictionary<string, AttackAnimationConfig>
            {
                ["attack1"] = new AttackAnimationConfig
                {
                    FPS = 12,
                    TotalFrames = 12,
                    HitFrames = new List<int> { 6, 10 } // Đánh 2 lần
                },
                ["attack2"] = new AttackAnimationConfig
                {
                    FPS = 10,
                    TotalFrames = 10,
                    HitFrames = new List<int> { 4 } // Frame 4 sau khi lướt
                },
                ["punch"] = new AttackAnimationConfig
                {
                    FPS = 10,
                    TotalFrames = 6,
                    HitFrames = new List<int> { 4 }
                },
                ["kick"] = new AttackAnimationConfig
                {
                    FPS = 10,
                    TotalFrames = 6,
                    HitFrames = new List<int> { 4 }
                }
                 ,
                ["skill"] = new AttackAnimationConfig
                {
                    FPS = 7,
                    TotalFrames = 5,
                    HitFrames = new List<int> { 3 } // Đánh ở frame 3
                }
            }
        };

        // Cấu hình Attack Hitbox cho từng loại tấn công của từng nhân vật
        private Dictionary<string, Dictionary<string, AttackHitboxConfig>> characterAttackConfigs = new Dictionary<string, Dictionary<string, AttackHitboxConfig>>
        {
            ["girlknight"] = new Dictionary<string, AttackHitboxConfig>
            {
                ["punch"] = new AttackHitboxConfig
                {
                    WidthPercent = 0.6f,      // ✅ Tăng từ 0.5f lên 0.6f
                    HeightPercent = 0.5f,     // ✅ Tăng từ 0.4f lên 0.5f
                    RangePercent = 0.47f,     // ✅ Tăng từ 0.35f lên 0.55f (tầm xa hơn)
                    OffsetYPercent = 0.30f    // ✅ Giảm từ 0.35f xuống 0.30f (cao hơn)
                },
                ["kick"] = new AttackHitboxConfig
                {
                    WidthPercent = 0.7f,      // ✅ Tăng từ 0.6f lên 0. 7f
                    HeightPercent = 0.45f,    // ✅ Tăng từ 0.35f lên 0.45f
                    RangePercent = 0.7f,      // ✅ Tăng từ 0.5f lên 0. 7f
                    OffsetYPercent = 0.50f    // ✅ Giảm từ 0. 55f xuống 0.50f
                }, 
                ["skill"] = new AttackHitboxConfig
                {
                    WidthPercent = 1f,      // ✅ Tăng từ 1.2f lên 1. 5f
                    HeightPercent = 1f,     // ✅ Tăng từ 0.8f lên 1.0f
                    RangePercent = 0.5f,      // ✅ Tăng từ 0.8f lên 1.2f (skill vùng gần)
                    OffsetYPercent = 0.10f    // ✅ Giảm từ 0.15f xuống 0.10f
                }
            },
            ["bringerofdeath"] = new Dictionary<string, AttackHitboxConfig>
            {
                ["punch"] = new AttackHitboxConfig
                {
                    WidthPercent = 0.6f,
                    HeightPercent = 0.4f,
                    RangePercent = 0.33f,      // ✅ GIẢM từ 0. 6f xuống 0.4f
                    OffsetYPercent = 0.30f
                },
                ["kick"] = new AttackHitboxConfig
                {
                    WidthPercent = 0.7f,
                    HeightPercent = 0.35f,
                    RangePercent = 0.33f,
                    OffsetYPercent = 0.50f
                },
                ["skill"] = new AttackHitboxConfig
                {
                    WidthPercent = 1f,
                    HeightPercent = 1f,
                    RangePercent = 0.5f,      // ✅ GIẢM từ 2.6f xuống 1.8f (spell xa hơn)
                    OffsetYPercent = 0.10f
                }
            },
            ["goatman"] = new Dictionary<string, AttackHitboxConfig>
            {
                ["punch"] = new AttackHitboxConfig
                {
                    WidthPercent = 0.7f,
                    HeightPercent = 0.5f,
                    RangePercent = 0.7f,      // ✅ TĂNG từ 0. 4f lên 0.8f
                    OffsetYPercent = 0.30f    // ✅ GIẢM từ 0.35f xuống 0.30f (cao hơn)
                },
                ["kick"] = new AttackHitboxConfig
                {
                    WidthPercent = 0.8f,
                    HeightPercent = 0.5f,     // ✅ TĂNG từ 0.4f lên 0.5f
                    RangePercent = 0.7f,      // ✅ TĂNG từ 0.6f lên 0. 9f
                    OffsetYPercent = 0.40f    // ✅ GIẢM từ 0.45f xuống 0.40f
                },
                ["skill"] = new AttackHitboxConfig
                {
                    WidthPercent = 1.2f,      // ✅ TĂNG từ 1.0f lên 1. 2f
                    HeightPercent = 0.8f,     // ✅ TĂNG từ 0.7f lên 0.8f
                    RangePercent = 1f,      // ✅ TĂNG từ 1.0f lên 1.4f
                    OffsetYPercent = 0.15f    // ✅ GIẢM từ 0. 18f xuống 0.15f
                }
            },
            ["warrior"] = new Dictionary<string, AttackHitboxConfig>
            {
                ["punch"] = new AttackHitboxConfig
                {
                    WidthPercent = 0.8f,      // ✅ TĂNG từ 0.7f lên 0.8f
                    HeightPercent = 0.5f,     // ✅ TĂNG từ 0.4f lên 0.5f
                    RangePercent = 0.5f,      // ✅ TĂNG từ 0.35f lên 0.5f
                    OffsetYPercent = 0.35f
                },
                ["kick"] = new AttackHitboxConfig
                {
                    WidthPercent = 0.7f,
                    HeightPercent = 0.35f,
                    RangePercent = 0.5f,      // ✅ GIẢM từ 0. 7f xuống 0.5f
                    OffsetYPercent = 0.50f
                },
                ["skill"] = new AttackHitboxConfig
                {
                    WidthPercent = 1.2f,      // ✅ Giảm từ 1.8f
                    HeightPercent = 0.6f,
                    RangePercent = 2.0f,      // ✅ GIẢM từ 2.8f xuống 2.0f (projectile nên xa hơn)
                    OffsetYPercent = 0.25f
                }
            }
        };
        // Cấu hình HURTBOX chi tiết cho từng loại nhân vật 
        private Dictionary<string, HitboxConfig> characterHurtboxConfigs = new Dictionary<string, HitboxConfig>
        {
            ["girlknight"] = new HitboxConfig
            {
                WidthPercent = 0.3f,    
                HeightPercent = 0.68f,   
                OffsetYPercent = 0.22f     
            },
            ["bringerofdeath"] = new HitboxConfig
            {
                WidthPercent = 0.18f,      
                HeightPercent = 0.45f,   
                OffsetYPercent = 0.38f,   
                OffsetXPercent = 0f
            },
            ["goatman"] = new HitboxConfig
            {
                WidthPercent = 0.60f,
                HeightPercent = 0.78f,
                OffsetYPercent = 0.12f,
                OffsetXPercent = -0.05f
            },
            ["warrior"] = new HitboxConfig
            {
                WidthPercent = 0.27f,      
                HeightPercent = 0.5f,    
                OffsetYPercent = 0.40f    
            }
        };
        public class HitboxConfig
        {
            public float WidthPercent { get; set; }
            public float HeightPercent { get; set; }
            public float OffsetYPercent { get; set; }
            public float OffsetXPercent { get; set; } = 0f;
        }
        // Hurt handling
        private const int HURT_DISPLAY_MS = 400;
        private string _prevAnimPlayer1 = null;
        private string _prevAnimPlayer2 = null;

        // Character types
        private string player1CharacterType = "girlknight";
        private string player2CharacterType = "girlknight";

        public BattleForm(string username, string token, string opponent, string player1Character, string player2Character)
        {
            InitializeComponent();

            this.username = username;
            this.token = token;
            this.opponent = opponent;
            this.player1CharacterType = player1Character;
            this.player2CharacterType = player2Character;

            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            player1Y = 0;
            player2Y = 0;
            this.Load += BattleForm_Load;

            // ✅ Initialize Sound Manager
            SoundManager.Initialize();

            // ✅ Initialize once here
            SetupGame();
            SetupEventHandlers();

            this.Text = $"⚔️ Street Fighter - {username} vs {opponent}";
            this.DoubleBuffered = true;
            this.KeyPreview = true;
        }

        private void BattleForm_Load(object sender, EventArgs e)
        {
            // ĐẢM BẢO FORM ĐÃ HOÀN TOÀN HIỆN RA
            this.Refresh();
            Application.DoEvents();

            // BÂY GIỜ mới tính toán groundLevel chính xác
            groundLevel = Math.Max(0, this.ClientSize.Height - groundOffset);
            player1Y = groundLevel - PLAYER_HEIGHT;
            player2Y = groundLevel - PLAYER_HEIGHT;

            // CẬP NHẬT LẠI PlayerState positions
            if (player1State != null)
            {
                player1State.Y = groundLevel - PLAYER_HEIGHT;
                player1Y = player1State.Y;
            }

            if (player2State != null)
            {
                player2State.Y = groundLevel - PLAYER_HEIGHT;
                player2Y = player2State.Y;
            }

            // CẬP NHẬT PhysicsSystem
            if (physicsSystem != null)
            {
                physicsSystem.UpdateGroundLevel(groundLevel);
                physicsSystem.ResetToGround(player1State);
                physicsSystem.ResetToGround(player2State);
            }

            // ✅ ENABLE: Play battle music when form loads
            SoundManager.PlayMusic(BackgroundMusic.BattleMusic, loop: true);
            Console.WriteLine("🎵 Battle music started");

            // ❌ Avoid re-running full setup; only force redraw
            this.Invalidate();
        }

        private void SetupGame()
        {
            try
            {
                if (groundLevel == 0)
                {
                    groundLevel = Math.Max(0, this.ClientSize.Height - groundOffset);
                }

                player1Y = groundLevel - PLAYER_HEIGHT;
                player2Y = groundLevel - PLAYER_HEIGHT;

                // Load animations...
                player1AnimationManager = new CharacterAnimationManager(player1CharacterType, OnFrameChanged);
                player1AnimationManager.LoadAnimations();

                player2AnimationManager = new CharacterAnimationManager(player2CharacterType, OnFrameChanged);
                player2AnimationManager.LoadAnimations();

                // ===== ✅ INITIALIZE NEW SYSTEMS =====
                // 1. Initialize PlayerState instances - SỬA Y position
                player1State = new PlayerState(username, player1CharacterType, 1)
                {
                    X = 150, // ✅ SỬA: từ 300 → 150
                    Y = groundLevel - PLAYER_HEIGHT,
                    Facing = "right",
                    CurrentAnimation = "stand"
                };
                
                // ✅ SỬA: Set HP theo character type
                SetPlayerHealth(player1State, player1CharacterType);

                player2State = new PlayerState(opponent, player2CharacterType, 2)
                {
                    X = 700, // ✅ SỬA: từ 600 → 900
                    Y = groundLevel - PLAYER_HEIGHT,
                    Facing = "left",
                    CurrentAnimation = "stand"
                };
                
                // ✅ SỬA: Set HP theo character type
                SetPlayerHealth(player2State, player2CharacterType);

                // 2. Initialize ResourceSystem (only once)
                if (resourceSystem == null)
                {
                    resourceSystem = new ResourceSystem(player1State, player2State);
                    resourceSystem.SetupStatusBars(this.ClientSize.Width);
                }

                // 3. Initialize PhysicsSystem
                physicsSystem = new PhysicsSystem(
                    groundLevel,
                    backgroundWidth,
                    PLAYER_WIDTH,
                    PLAYER_HEIGHT,
                    GetPlayerHitbox
                );


                // 4. Initialize EffectManager
                effectManager = new EffectManager();

                // 5. Initialize ProjectileManager
                projectileManager = new ProjectileManager(backgroundWidth, OnFrameChanged);

                // 6. Initialize CombatSystem
                combatSystem = new CombatSystem(
                    player1State, player2State,
                    player1AnimationManager, player2AnimationManager,
                    effectManager, projectileManager,
                    PLAYER_WIDTH, PLAYER_HEIGHT, backgroundWidth,
                    () => this.Invalidate(),
                    ShowHitEffect,
                    GetAttackHitbox,
                    GetPlayerHitbox
                );
                // =====================================

                // ✅ THÊM DÒNG NÀY - SAU KHI ĐÃ CÓ physicsSystem!
                UpdateCharacterSize();

                // Khởi tạo walk animation timer
                walkAnimationTimer = new System.Windows.Forms.Timer();
                walkAnimationTimer.Interval = 100;
                walkAnimationTimer.Tick += WalkAnimationTimer_Tick;

                // ❌ Remove legacy parry timers in BattleForm (CombatSystem handles parry)
                // p1ParryTimer = new System.Windows.Forms.Timer();
                // p1ParryTimer.Interval = parryWindowMs;
                // p1ParryTimer.Tick += (s, e) =>
                // {
                //     p1ParryTimer.Stop();
                //     player1Parrying = false;
                //     player1ParryOnCooldown = true;
                //     // restore previous animation if still valid
                //     if (!player1Attacking && !player1Jumping)
                //         player1CurrentAnimation = (_prevAnimPlayer1 == "walk" && (aPressed || dPressed)) ? "walk" : "stand";
                //     p1ParryCooldownTimer.Start();
                //     this.Invalidate();
                // };

                // p1ParryCooldownTimer = new System.Windows.Forms.Timer();
                // p1ParryCooldownTimer.Interval = parryCooldownMs;
                // p1ParryCooldownTimer.Tick += (s, e) =>
                // {
                //     p1ParryCooldownTimer.Stop();
                //     player1ParryOnCooldown = false;
                // };

                // p2ParryTimer = new System.Windows.Forms.Timer();
                // p2ParryTimer.Interval = parryWindowMs;
                // p2ParryTimer.Tick += (s, e) =>
                // {
                //     p2ParryTimer.Stop();
                //     player2Parrying = false;
                //     player2ParryOnCooldown = true;
                //     if (!player2Attacking && !player2Jumping)
                //         player2CurrentAnimation = (_prevAnimPlayer2 == "walk" && (leftPressed || rightPressed)) ? "walk" : "stand";
                //     p2ParryCooldownTimer.Start();
                //     this.Invalidate();
                // };

                // p2ParryCooldownTimer = new System.Windows.Forms.Timer();
                // p2ParryCooldownTimer.Interval = parryCooldownMs;
                // p2ParryCooldownTimer.Tick += (s, e) =>
                // {
                //     p2ParryCooldownTimer.Stop();
                //     player2ParryOnCooldown = false;
                // };
                // Load background options
                if (cmbBackground.Items.Count == 0) // CHỈ thêm nếu ComboBox rỗng
        {
            // Xóa danh sách cũ và thêm mới
            backgroundImages.Clear();
            backgroundImages.Add("battleground1");
            backgroundImages.Add("battleground2");
            backgroundImages.Add("battleground3");
            backgroundImages.Add("battleground4");

            cmbBackground.Items.Clear(); // Xóa trước khi thêm mới
            cmbBackground.Items.AddRange(new object[] {
                "Battlefield 1", "Battlefield 2", "Battlefield 3", "Battlefield 4"
            });
            
            Console.WriteLine($"[SetupGame] Added {cmbBackground.Items.Count} background options");
        }
                // Set background (uses current ClientSize; safe-guards inside)
                SetBackground(backgroundImages[currentBackground]);

                // Load fireball (resource may be Image or byte[])
                try
                {
                    fireball = ResourceToImage(Properties.Resources.fireball);
                    if (fireball != null && ImageAnimator.CanAnimate(fireball))
                    {
                        ImageAnimator.Animate(fireball, OnFrameChanged);
                    }
                }
                catch
                {
                    fireball = CreateColoredImage(FIREBALL_WIDTH, FIREBALL_HEIGHT, Color.Orange);
                }

                // Load spell animation (for Bringer of Death)
                try
                {
                    spellAnimation = ResourceToImage(Properties.Resources.BringerofDeath_Spell);
                    if (spellAnimation != null && ImageAnimator.CanAnimate(spellAnimation))
                    {
                        ImageAnimator.Animate(spellAnimation, OnFrameChanged);
                    }
                }
                catch
                {
                    spellAnimation = CreateColoredImage(SPELL_WIDTH, SPELL_HEIGHT, Color.Purple);
                }

                // Load dash effect
                try
                {
                    dashEffectImage = CreateColoredImage(80, 80, Color.FromArgb(150, Color.White));
                }
                catch
                {
                    dashEffectImage = CreateColoredImage(80, 80, Color.White);
                }

                // Load GM impact effect ( TẠO LỚN HƠN để dễ nhìn - từ 60x60 lên 100x100)
                try
                {
                    gmImpactEffect = CreateColoredImage(100, 100, Color.OrangeRed);
                }
                catch
                {
                    gmImpactEffect = CreateColoredImage(100, 100, Color.OrangeRed);
                }

                // Load warrior skill effect (projectile)
                try
                {
                    var warriorEffect = ResourceToImage(Properties.Resources.Warrior_skill_effect);
                    if (warriorEffect != null)
                    {
                        warriorSkillEffect = warriorEffect;
                        if (ImageAnimator.CanAnimate(warriorSkillEffect))
                        {
                            ImageAnimator.Animate(warriorSkillEffect, OnFrameChanged);
                        }
                    }
                    else
                    {
                        warriorSkillEffect = CreateColoredImage(PROJECTILE_WIDTH, PROJECTILE_HEIGHT, Color.FromArgb(200, Color.Gold));
                    }
                }
                catch
                {
                    warriorSkillEffect = CreateColoredImage(PROJECTILE_WIDTH, PROJECTILE_HEIGHT, Color.FromArgb(200, Color.Gold));
                }

                // Load hit effect
                try
                {
                    hitEffectImage = CreateColoredImage(50, 50, Color.FromArgb(200, Color.Red));
                }
                catch
                {
                    hitEffectImage = CreateColoredImage(50, 50, Color.Red);
                }

                // Initialize timers
                player1DashTimer = new System.Windows.Forms.Timer();
                player2DashTimer = new System.Windows.Forms.Timer();
                player1SkillTimer = new System.Windows.Forms.Timer();
                player2SkillTimer = new System.Windows.Forms.Timer();
                player1ChargeTimer = new System.Windows.Forms.Timer();
                player2ChargeTimer = new System.Windows.Forms.Timer();
                impact1Timer = new System.Windows.Forms.Timer();
                impact2Timer = new System.Windows.Forms.Timer();
                gameTimer.Interval = 16;
            }
            catch (Exception ex)
            {
                CreateFallbackGraphics();
                Console.WriteLine($"Setup error: {ex.Message}");
            }

            // ===== ✅ SETUP UI WITH NEW SYSTEMS =====
            // Add bars to form controls once
            if (resourceSystem != null)
            {
                if (!this.Controls.Contains(resourceSystem.HealthBar1)) this.Controls.Add(resourceSystem.HealthBar1);
                if (!this.Controls.Contains(resourceSystem.StaminaBar1)) this.Controls.Add(resourceSystem.StaminaBar1);
                if (!this.Controls.Contains(resourceSystem.ManaBar1)) this.Controls.Add(resourceSystem.ManaBar1);
                if (!this.Controls.Contains(resourceSystem.HealthBar2)) this.Controls.Add(resourceSystem.HealthBar2);
                if (!this.Controls.Contains(resourceSystem.StaminaBar2)) this.Controls.Add(resourceSystem.StaminaBar2);
                if (!this.Controls.Contains(resourceSystem.ManaBar2)) this.Controls.Add(resourceSystem.ManaBar2);
                
                // ✅ Add portraits
                if (!this.Controls.Contains(resourceSystem.Portrait1)) this.Controls.Add(resourceSystem.Portrait1);
                if (!this.Controls.Contains(resourceSystem.Portrait2)) this.Controls.Add(resourceSystem.Portrait2);
            }

            // Add player name labels (keep existing style)
            int screenWidth = this.ClientSize.Width;
            int barWidth = screenWidth / 4;
            int barHeight = 20;
            int spacing = 5;
            int startY = 10;

            if (lblPlayer1Name == null)
            {
                lblPlayer1Name = new Label
                {
                    Text = username,
                    Location = new Point(20, startY + 3 * (barHeight + spacing) + 90),  // ✅ Dịch xuống dưới portrait (nhỏ)
                    Size = new Size(barWidth, 25),  // ✅ Tăng chiều cao từ 20 → 25
                    ForeColor = Color.Cyan,
                    Font = new Font("Arial", 12, FontStyle.Bold),  // ✅ Tăng size từ 10 → 12
                    BackColor = Color.Transparent
                };
                this.Controls.Add(lblPlayer1Name);
            }

            if (lblPlayer2Name == null)
            {
                lblPlayer2Name = new Label
                {
                    Text = opponent,
                    Location = new Point(screenWidth - barWidth - 20, startY + 3 * (barHeight + spacing) + 90),  // ✅ Dịch xuống dưới portrait (nhỏ)
                    Size = new Size(barWidth, 25),  // ✅ Tăng chiều cao từ 20 → 25
                    ForeColor = Color.Orange,
                    Font = new Font("Arial", 12, FontStyle.Bold),  // ✅ Tăng size từ 10 → 12
                    TextAlign = ContentAlignment.TopRight,
                    BackColor = Color.Transparent
                };
                this.Controls.Add(lblPlayer2Name);
            }

            SetupControlsInfo();

            // Initialize round system
            InitializeRoundSystem();
        }

        // In BattleForm.cs - GameTimer_Tick() - VERSION CLEAN

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // ===== MOVEMENT LOGIC =====
            player1State.IsWalking = false;
            player2State.IsWalking = false;

            if (player1State.CanMove)
            {
                if (aPressed) physicsSystem.MovePlayer(player1State, -1);
                else if (dPressed) physicsSystem.MovePlayer(player1State, 1);
                else if (!player1State.IsJumping && !player1State.IsParrying && !player1State.IsSkillActive)
                    physicsSystem.StopMovement(player1State);
            }

            if (player2State.CanMove)
            {
                if (leftPressed) physicsSystem.MovePlayer(player2State, -1);
                else if (rightPressed) physicsSystem.MovePlayer(player2State, 1);
                else if (!player2State.IsJumping && !player2State.IsParrying && !player2State.IsSkillActive)
                    physicsSystem.StopMovement(player2State);
            }

            // ===== JUMP PHYSICS =====
            physicsSystem.UpdateJump(player1State);
            physicsSystem.UpdateJump(player2State);

            // ===== SYNC OLD VARIABLES =====
            player1X = player1State.X;
            player1Y = player1State.Y;
            player2X = player2State.X;
            player2Y = player2State.Y;
            player1Jumping = player1State.IsJumping;
            player2Jumping = player2State.IsJumping;
            player1Walking = player1State.IsWalking;
            player2Walking = player2State.IsWalking;
            player1CurrentAnimation = player1State.CurrentAnimation;
            player2CurrentAnimation = player2State.CurrentAnimation;
            player1Facing = player1State.Facing;
            player2Facing = player2State.Facing;
            player1Attacking = player1State.IsAttacking;
            player2Attacking = player2State.IsAttacking;
            player1Stunned = player1State.IsStunned;
            player2Stunned = player2State.IsStunned;
            player1Parrying = player1State.IsParrying;
            player2Parrying = player2State.IsParrying;
            player1Dashing = player1State.IsDashing;
            player2Dashing = player2State.IsDashing;
            player1Charging = player1State.IsCharging;
            player2Charging = player2State.IsCharging;
            player1SkillActive = player1State.IsSkillActive;
            player2SkillActive = player2State.IsSkillActive;
            player1Health = player1State.Health;
            player1Stamina = player1State.Stamina;
            player1Mana = player1State.Mana;
            player2Health = player2State.Health;
            player2Stamina = player2State.Stamina;
            player2Mana = player2State.Mana;

            // ===== UPDATE ANIMATIONS =====
            var p1Img = player1AnimationManager.GetAnimation(player1State.CurrentAnimation);
            if (p1Img != null && ImageAnimator.CanAnimate(p1Img))
                ImageAnimator.UpdateFrames(p1Img);

            var p2Img = player2AnimationManager.GetAnimation(player2State.CurrentAnimation);
            if (p2Img != null && ImageAnimator.CanAnimate(p2Img))
                ImageAnimator.UpdateFrames(p2Img);

            // ===== UPDATE PROJECTILES =====
            projectileManager.UpdateFireball(
                (playerNum, x, y, _) =>
                {
                    var p = playerNum == 1 ? player1State : player2State;
                    return new Rectangle(p.X, p.Y, PLAYER_WIDTH, PLAYER_HEIGHT);
                },
                (playerNum) => playerNum == 1 ? player1State.IsParrying : player2State.IsParrying,
                () => { },
                (target, damage) => ApplyHurtToPlayer(target, damage, false),
                ShowHitEffect
            );

            projectileManager.UpdateWarriorProjectiles(
                (playerNum, x, y, _) =>
                {
                    var p = playerNum == 1 ? player1State : player2State;
                    // Use actual configured hurtbox
                    var hb = GetPlayerHitbox(p);
                    return new Rectangle(hb.X, hb.Y, hb.Width, hb.Height);
                },
                (target, damage) => ApplyHurtToPlayer(target, damage, false),
                ShowHitEffect
            );

            projectileManager.UpdateSpellAnimation();

            // ===== OLD PROJECTILE CODE (compatibility) =====
            if (fireballActive)
            {
                fireballX += 12 * fireballDirection;
                CheckFireballHit();
                if (fireballX > backgroundWidth || fireballX < -FIREBALL_WIDTH)
                    fireballActive = false;
                if (fireball != null && ImageAnimator.CanAnimate(fireball))
                    ImageAnimator.UpdateFrames(fireball);
            }

            if (spellActive && spellAnimation != null && ImageAnimator.CanAnimate(spellAnimation))
                ImageAnimator.UpdateFrames(spellAnimation);

            if (projectile1Active)
            {
                projectile1X += PROJECTILE_SPEED * projectile1Direction;
                Rectangle projRect = new Rectangle(projectile1X, projectile1Y, PROJECTILE_WIDTH, PROJECTILE_HEIGHT);
                Rectangle targetRect = GetPlayerHitbox(player2State);
                if (projRect.IntersectsWith(targetRect))
                {
                    ApplyHurtToPlayer(2, 20);
                    projectile1Active = false;
                    ShowHitEffect("Energy Strike!", Color.Gold);
                }
                if (projectile1X > backgroundWidth || projectile1X < -PROJECTILE_WIDTH)
                    projectile1Active = false;
                if (warriorSkillEffect != null && ImageAnimator.CanAnimate(warriorSkillEffect))
                    ImageAnimator.UpdateFrames(warriorSkillEffect);
            }

            if (projectile2Active)
            {
                projectile2X += PROJECTILE_SPEED * projectile2Direction;
                Rectangle projRect = new Rectangle(projectile2X, projectile2Y, PROJECTILE_WIDTH, PROJECTILE_HEIGHT);
                Rectangle targetRect = GetPlayerHitbox(player1State);
                if (projRect.IntersectsWith(targetRect))
                {
                    ApplyHurtToPlayer(1, 20);
                    projectile2Active = false;
                    ShowHitEffect("Energy Strike!", Color.Gold);
                }
                if (projectile2X > backgroundWidth || projectile2X < -PROJECTILE_WIDTH)
                    projectile2Active = false;
            }

            // ===== UPDATE CAMERA =====
            UpdateCamera();

            // ===== REGENERATE RESOURCES =====
            resourceSystem.RegenerateResources();
            resourceSystem.UpdateBars();

            // ===== CHECK ROUND END (by HP depletion) =====
            if (player1State.IsDead || player2State.IsDead)
            {
                gameTimer.Stop();
                walkAnimationTimer.Stop();
                HandleRoundEndByDeath();
            }

            this.Invalidate();
        }

        private void BattleForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Player 1 controls
            switch (e.KeyCode)
            {
                case Keys.A: 
                    if (player1State.CanMove) // ✅ USE PlayerState
                    {
                        aPressed = true;
                        player1State.LeftKeyPressed = true; // ✅ SYNC STATE
                    }
                    break;
                case Keys.D: 
                    if (player1State.CanMove)
                    {
                        dPressed = true;
                        player1State.RightKeyPressed = true; // ✅ SYNC STATE
                    }
                    break;
                case Keys.W:
                    // ✅ MIGRATED TO PhysicsSystem
                    physicsSystem.Jump(player1State);
                    break;
                case Keys.J: 
                    if (player1State.CanAttack) // ✅ USE PlayerState
                    {
                        // ✅ MIGRATED TO CombatSystem
                        ExecuteAttackWithHitbox(1, "punch", 10, 15);
                    }
                    break;
                case Keys.K: 
                    if (player1State.CanAttack)
                    {
                        // ✅ MIGRATED TO CombatSystem
                        ExecuteAttackWithHitbox(1, "kick", 15, 20);
                    }
                    break;
                case Keys.L: 
                    if (player1State.CanDash) // ✅ USE PlayerState
                    {
                        // ✅ MIGRATED TO CombatSystem
                        combatSystem.ExecuteDash(1);
                    }
                    break;
                case Keys.U:
                    if (player1State.CanParry)
                    {
                        // ✅ MIGRATED TO CombatSystem
                        combatSystem.StartParry(1);
                    }
                    break;
                case Keys.I:
                    if (player1State.CanAttack)
                    {
                        // ✅ MIGRATED TO CombatSystem
                        combatSystem.ToggleSkill(1);
                    }
                    break;
                case Keys.Escape:
                    BtnBack_Click(null, EventArgs.Empty);
                    break;
            }

            // Player 2 controls
            switch (e.KeyCode)
            {
                case Keys.Left: 
                    if (player2State.CanMove)
                    {
                        leftPressed = true;
                        player2State.LeftKeyPressed = true; // ✅ SYNC STATE
                    }
                    break;
                case Keys.Right: 
                    if (player2State.CanMove)
                    {
                        rightPressed = true;
                        player2State.RightKeyPressed = true; // ✅ SYNC STATE
                    }
                    break;
                case Keys.Up:
                    // ✅ MIGRATED TO PhysicsSystem
                    physicsSystem.Jump(player2State);
                    break;
                case Keys.NumPad1: 
                    if (player2State.CanAttack)
                    {
                        // ✅ MIGRATED TO CombatSystem
                        ExecuteAttackWithHitbox(2, "punch", 10, 15);
                    }
                    break;
                case Keys.NumPad2: 
                    if (player2State.CanAttack)
                    {
                        // ✅ MIGRATED TO CombatSystem
                        ExecuteAttackWithHitbox(2, "kick", 15, 20);
                    }
                    break;
                case Keys.NumPad3: 
                    if (player2State.CanDash)
                    {
                        // ✅ MIGRATED TO CombatSystem
                        combatSystem.ExecuteDash(2);
                    }
                    break;
                case Keys.NumPad5:
                    if (player2State.CanParry)
                    {
                        // ✅ MIGRATED TO CombatSystem
                        combatSystem.StartParry(2);
                    }
                    break;
                case Keys.NumPad4:
                    if (player2State.CanAttack)
                    {
                        // ✅ MIGRATED TO CombatSystem
                        combatSystem.ToggleSkill(2);
                    }
                    break;
            }

            e.Handled = true;
        }

        private void BattleForm_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A: 
                    aPressed = false;
                    player1State.LeftKeyPressed = false; // ✅ SYNC STATE
                    break;
                case Keys.D: 
                    dPressed = false;
                    player1State.RightKeyPressed = false; // ✅ SYNC STATE
                    break;
                case Keys.Left: 
                    leftPressed = false;
                    player2State.LeftKeyPressed = false; // ✅ SYNC STATE
                    break;
                case Keys.Right: 
                    rightPressed = false;
                    player2State.RightKeyPressed = false; // ✅ SYNC STATE
                    break;
            }
            e.Handled = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw background
            if (background != null)
            {
                e.Graphics.DrawImage(background,
                    new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height),
                    new Rectangle(viewportX, 0, this.ClientSize.Width, this.ClientSize.Height),
                    GraphicsUnit.Pixel);
            }

            // ===== ✅ MIGRATED: Draw effects (behind characters) =====
            effectManager.DrawEffects(e.Graphics, viewportX, PLAYER_WIDTH, PLAYER_HEIGHT);

            // Draw dash effects - OLD CODE (kept for compatibility)
            if (dashEffect1Active && dashEffectImage != null)
            {
                int effectScreenX = dashEffect1X - viewportX;
                if (effectScreenX >= -80 && effectScreenX <= this.ClientSize.Width)
                {
                    e.Graphics.DrawImage(dashEffectImage, effectScreenX, dashEffect1Y + PLAYER_HEIGHT - 40, 80, 40);
                }
            }

            if (dashEffect2Active && dashEffectImage != null)
            {
                int effectScreenX = dashEffect2X - viewportX;
                if (effectScreenX >= -80 && effectScreenX <= this.ClientSize.Width)
                {
                    e.Graphics.DrawImage(dashEffectImage, effectScreenX, dashEffect2Y + PLAYER_HEIGHT - 40, 80, 40);
                }
            }

            // ===== ✅ MIGRATED: Draw characters using PlayerState =====
            DrawCharacter(e.Graphics, player1State.X, player1State.Y, player1State.CurrentAnimation, player1State.Facing, player1AnimationManager);
            DrawCharacter(e.Graphics, player2State.X, player2State.Y, player2State.CurrentAnimation, player2State.Facing, player2AnimationManager);

            // ===== ✅ MIGRATED: Draw hit effects (on top of characters) =====
            effectManager.DrawHitEffects(e.Graphics, viewportX, PLAYER_WIDTH, PLAYER_HEIGHT);

            // Draw hit effects - OLD CODE (kept for compatibility)
            foreach (var hitEffect in activeHitEffects.ToList())
            {
                int effectScreenX = hitEffect.X - viewportX;
                if (effectScreenX >= -50 && effectScreenX <= this.ClientSize.Width)
                {
                    e.Graphics.DrawImage(hitEffect.EffectImage,
                        effectScreenX + PLAYER_WIDTH / 2 - 25,
                        hitEffect.Y + PLAYER_HEIGHT / 2 - 25,
                        50, 50);
                }
            }

            // ===== ✅ MIGRATED: Draw projectiles =====
            projectileManager.DrawProjectiles(e.Graphics, viewportX);

            // Draw spell effect - OLD CODE (kept for compatibility)
            if (spellActive && spellAnimation != null)
            {
                int spellScreenX = spellX - viewportX;
                if (spellScreenX >= -SPELL_WIDTH && spellScreenX <= this.ClientSize.Width)
                {
                    e.Graphics.DrawImage(spellAnimation, spellScreenX, spellY, SPELL_WIDTH, SPELL_HEIGHT);
                }
            }

            // Draw fireball - OLD CODE (kept for compatibility)
            if (fireballActive && fireball != null)
            {
                int fireballScreenX = fireballX - viewportX;
                if (fireballScreenX >= -FIREBALL_WIDTH && fireballScreenX <= this.ClientSize.Width)
                {
                    e.Graphics.DrawImage(fireball, fireballScreenX, fireballY, FIREBALL_WIDTH, FIREBALL_HEIGHT);
                }
            }

            // Draw projectiles (warrior skill) - OLD CODE (kept for compatibility)
            if (projectile1Active && warriorSkillEffect != null)
            {
                int projScreenX = projectile1X - viewportX;
                if (projScreenX >= -PROJECTILE_WIDTH && projScreenX <= this.ClientSize.Width)
                {
                    if (projectile1Direction == -1)
                    {
                        e.Graphics.DrawImage(warriorSkillEffect,
                            new Rectangle(projScreenX + PROJECTILE_WIDTH, projectile1Y, -PROJECTILE_WIDTH, PROJECTILE_HEIGHT),
                            new Rectangle(0, 0, warriorSkillEffect.Width, warriorSkillEffect.Height),
                            GraphicsUnit.Pixel);
                    }
                    else
                    {
                        e.Graphics.DrawImage(warriorSkillEffect, projScreenX, projectile1Y, PROJECTILE_WIDTH, PROJECTILE_HEIGHT);
                    }
                }
            }

            if (projectile2Active && warriorSkillEffect != null)
            {
                int projScreenX = projectile2X - viewportX;
                if (projScreenX >= -PROJECTILE_WIDTH && projScreenX <= this.ClientSize.Width)
                {
                    if (projectile2Direction == -1)
                    {
                        e.Graphics.DrawImage(warriorSkillEffect,
                            new Rectangle(projScreenX + PROJECTILE_WIDTH, projectile2Y, -PROJECTILE_WIDTH, PROJECTILE_HEIGHT),
                            new Rectangle(0, 0, warriorSkillEffect.Width, warriorSkillEffect.Height),
                            GraphicsUnit.Pixel);
                    }
                    else
                    {
                        e.Graphics.DrawImage(warriorSkillEffect, projScreenX, projectile2Y, PROJECTILE_WIDTH, PROJECTILE_HEIGHT);
                    }
                }
            }

            // ===== ✅ MIGRATED: Draw impact effects =====
            effectManager.DrawImpactEffects(e.Graphics, viewportX);

            // ===== ✅ MIGRATED: Draw parry indicators using PlayerState =====
            if (player1State.IsParrying)
            {
                int sx = player1State.X - viewportX + PLAYER_WIDTH / 2 - 12;
                e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(180, Color.Cyan)), sx, player1State.Y - 28, 24, 24);
            }
            if (player2State.IsParrying)
            {
                int sx = player2State.X - viewportX + PLAYER_WIDTH / 2 - 12;
                e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(180, Color.Cyan)), sx, player2State.Y - 28, 24, 24);
            }

            // ===== ✅ MIGRATED: Draw stun indicators using PlayerState =====
            if (player1State.IsStunned)
            {
                int sx = player1State.X - viewportX + PLAYER_WIDTH / 2 - 15;
                using (var font = new Font("Arial", 20, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.Yellow))
                {
                    e.Graphics.DrawString("★", font, brush, sx, player1State.Y - 35);
                }
            }
            if (player2State.IsStunned)
            {
                int sx = player2State.X - viewportX + PLAYER_WIDTH / 2 - 15;
                using (var font = new Font("Arial", 20, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.Yellow))
                {
                    e.Graphics.DrawString("★", font, brush, sx, player2State.Y - 35);
                }
            }
            // ===== ✅ DEBUG HITBOXES =====
            // 1. Draw attack hitbox (green) when attacking
            if (player1State.IsAttacking)
            {
                var attackBox = GetAttackHitbox(player1State, player1State.CurrentAnimation);
                using (var pen = new Pen(Color.Lime, 3))
                {
                    e.Graphics.DrawRectangle(pen,
                        attackBox.X - viewportX,
                        attackBox.Y,
                        attackBox.Width,
                        attackBox.Height);
                }
            }

            if (player2State.IsAttacking)
            {
                var attackBox = GetAttackHitbox(player2State, player2State.CurrentAnimation);
                using (var pen = new Pen(Color.Lime, 3))
                {
                    e.Graphics.DrawRectangle(pen,
                        attackBox.X - viewportX,
                        attackBox.Y,
                        attackBox.Width,
                        attackBox.Height);
                }
            }

            // 2.  Draw skill hitbox (green) when skill is active
            if (player1State.IsSkillActive)
            {
                if (characterAttackConfigs.ContainsKey(player1CharacterType) &&
                    characterAttackConfigs[player1CharacterType].ContainsKey("skill"))
                {
                    var skillBox = GetAttackHitbox(player1State, "skill");
                    using (var pen = new Pen(Color.Lime, 3))
                    {
                        e.Graphics.DrawRectangle(pen,
                            skillBox.X - viewportX,
                            skillBox.Y,
                            skillBox.Width,
                            skillBox.Height);
                    }
                }
            }

            if (player2State.IsSkillActive)
            {
                if (characterAttackConfigs.ContainsKey(player2CharacterType) &&
                    characterAttackConfigs[player2CharacterType].ContainsKey("skill"))
                {
                    var skillBox = GetAttackHitbox(player2State, "skill");
                    using (var pen = new Pen(Color.Lime, 3))
                    {
                        e.Graphics.DrawRectangle(pen,
                            skillBox.X - viewportX,
                            skillBox.Y,
                            skillBox.Width,
                            skillBox.Height);
                    }
                }
            }

            // 3. Draw hurtbox (red) always
            var p1Hurtbox = GetPlayerHitbox(player1State);
            var p2Hurtbox = GetPlayerHitbox(player2State);

            using (var pen = new Pen(Color.Red, 2))
            {
                e.Graphics.DrawRectangle(pen,
                    p1Hurtbox.X - viewportX,
                    p1Hurtbox.Y,
                    p1Hurtbox.Width,
                    p1Hurtbox.Height);

                e.Graphics.DrawRectangle(pen,
                    p2Hurtbox.X - viewportX,
                    p2Hurtbox.Y,
                    p2Hurtbox.Width,
                    p2Hurtbox.Height);
            }
        }
        private void UpdateCamera()
        {
            // ✅ Calculate center point between two players
            int centerX = (player1State.X + player2State.X) / 2;
            
            // ✅ Center viewport on players
            viewportX = centerX - this.ClientSize.Width / 2;
            
            // ✅ CLAMP VIEWPORT: Ensure map boundaries are respected
            // - Minimum: viewport starts at 0 (left edge of map)
            // - Maximum: viewport ends at backgroundWidth (right edge of map)
            viewportX = Math.Max(0, Math.Min(backgroundWidth - this.ClientSize.Width, viewportX));
            
            // ✅ ADDITIONAL CHECK: If a player is too close to map edge, adjust viewport
            // This prevents players from appearing to go "outside" the visible screen
            int minPlayerX = Math.Min(player1State.X, player2State.X);
            int maxPlayerX = Math.Max(player1State.X + PLAYER_WIDTH, player2State.X + PLAYER_WIDTH);
            
            // If left player is too close to left edge
            if (minPlayerX < viewportX + 100)
            {
                viewportX = Math.Max(0, minPlayerX - 100);
            }
            
            // If right player is too close to right edge
            if (maxPlayerX > viewportX + this.ClientSize.Width - 100)
            {
                viewportX = Math.Min(backgroundWidth - this.ClientSize.Width, maxPlayerX - this.ClientSize.Width + 100);
            }
        }

        private void BattleForm_Resize(object sender, EventArgs e)
        {
            if (resourceSystem != null && resourceSystem.HealthBar1 != null) // ✅ CHECK NEW SYSTEM
            {
                int screenWidth = this.ClientSize.Width;

                // ===== ✅ MIGRATED: Resize bars using ResourceSystem =====
                resourceSystem.ResizeBars(screenWidth);

                // Update player name labels
                int barWidth = screenWidth / 4;
                int nameY = 10 + 3 * (20 + 5) + 90;  // ✅ Cùng vị trí với lblPlayer1Name (dưới portrait)
                if (lblPlayer1Name != null)
                {
                    lblPlayer1Name.Location = new Point(20, nameY);
                    lblPlayer1Name.Size = new Size(barWidth, 25);
                }
                if (lblPlayer2Name != null)
                {
                    lblPlayer2Name.Location = new Point(screenWidth - barWidth - 20, nameY);  // ✅ Cùng Y như Player 1
                    lblPlayer2Name.Size = new Size(barWidth, 25);
                }

                // ===== ✅ MIGRATED: Update ground level in PhysicsSystem =====
                groundLevel = Math.Max(0, this.ClientSize.Height - groundOffset);
                physicsSystem.UpdateGroundLevel(groundLevel);
                UpdateCharacterSize();

                // ===== ✅ MIGRATED: Reset player positions to ground =====
                physicsSystem.ResetToGround(player1State);
                physicsSystem.ResetToGround(player2State);

                // Update controls info position
                if (lblControlsInfo != null)
                {
                    lblControlsInfo.Location = new Point(screenWidth / 2 - 350, this.ClientSize.Height - 60);
                }

                // Update round center label position
                PositionRoundCenterLabel();
            }
        }
        private (int actualWidth, int actualHeight, int yOffset, int groundAdjustment) GetActualCharacterSize(string characterType)
        {
            float sizeScale = 1.0f;
            int yOffset = 0;
            int groundAdjustment = 0; // ✅ THÊM: Điều chỉnh vị trí so với mặt đất

            if (characterType == "girlknight")
            {
                sizeScale = 0.7f;
                yOffset = (int)(PLAYER_HEIGHT * (1.0f - sizeScale));
                groundAdjustment = 0; // Vị trí chuẩn
            }
            else if (characterType == "bringerofdeath")
            {
                sizeScale = 1.6f;
                // ✅ SỬA: yOffset = 0, groundAdjustment âm để nâng lên
                yOffset = 0;
                groundAdjustment = -95; // Nâng lên 40px so với mặt đất
            }
            else if (characterType == "goatman")
            {
                sizeScale = 0.7f;
                yOffset = (int)(PLAYER_HEIGHT * (1.0f - sizeScale));
                groundAdjustment = 0;
            }
            else if (characterType == "warrior")
            {
                sizeScale = 1.0f;
                yOffset = 0;
                groundAdjustment = 0;
            }

            int actualHeight = (int)(PLAYER_HEIGHT * sizeScale);
            int actualWidth = actualHeight;

            CharacterAnimationManager animManager = characterType == player1CharacterType ?
                player1AnimationManager : player2AnimationManager;

            var standImg = animManager?.GetAnimation("stand");
            if (standImg != null)
            {
                int imgW = Math.Max(1, standImg.Width);
                int imgH = Math.Max(1, standImg.Height);
                actualWidth = Math.Max(16, (int)(actualHeight * (float)imgW / imgH));
            }

            return (actualWidth, actualHeight, yOffset, groundAdjustment);
        }
        private void UpdateCharacterSize()
        {
            // ✅ SỬA: Áp dụng hệ số nhân toàn cục
            int baseHeight = Math.Max(24, (int)(this.ClientSize.Height * characterHeightRatio));
            int newHeight = (int)(baseHeight * globalCharacterScale); // Nhân với hệ số toàn cục
            int spriteOrigW = 64;
            int spriteOrigH = 64;

            var standImg = player1AnimationManager?.GetAnimation("stand");
            if (standImg != null)
            {
                spriteOrigW = standImg.Width;
                spriteOrigH = standImg.Height;
            }

            PLAYER_HEIGHT = newHeight;
            PLAYER_WIDTH = Math.Max(16, (int)(PLAYER_HEIGHT * (float)spriteOrigW / spriteOrigH));

            // ===== ✅ UPDATE: Update PhysicsSystem with new player size =====
            physicsSystem.UpdatePlayerSize(PLAYER_WIDTH, PLAYER_HEIGHT);

            groundLevel = Math.Max(0, this.ClientSize.Height - groundOffset);

            // ===== ✅ MIGRATED: Reset positions using PhysicsSystem =====
            if (!player1State.IsJumping) physicsSystem.ResetToGround(player1State);
            if (!player2State.IsJumping) physicsSystem.ResetToGround(player2State);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try { gameTimer?.Stop(); } catch { }
            try { walkAnimationTimer?.Stop(); } catch { }
            try { spellDamageTimer?.Stop(); spellDamageTimer?.Dispose(); } catch { }
            try { player1DashTimer?.Stop(); player1DashTimer?.Dispose(); } catch { }
            try { player2DashTimer?.Stop(); player2DashTimer?.Dispose(); } catch { }
            try { player1SkillTimer?.Stop(); player1SkillTimer?.Dispose(); } catch { }
            try { player2SkillTimer?.Stop(); player2SkillTimer?.Dispose(); } catch { }
            try { player1ChargeTimer?.Stop(); player1ChargeTimer?.Dispose(); } catch { }
            try { player2ChargeTimer?.Stop(); player2ChargeTimer?.Dispose(); } catch { }
            try { impact1Timer?.Stop(); impact1Timer?.Dispose(); } catch { }
            try { impact2Timer?.Stop(); impact2Timer?.Dispose(); } catch { }

            // Cleanup hit effects - OLD CODE
            foreach (var hitEffect in activeHitEffects.ToList())
            {
                try 
                { 
                    hitEffect.Timer?.Stop(); 
                    hitEffect.Timer?.Dispose(); 
                } 
                catch { }
            }
            activeHitEffects.Clear();

            // ===== ✅ MIGRATED: Cleanup new systems =====
            // ✅ THÊM: Cleanup PlayerState timers
            try { player1State?.Cleanup(); } catch { }
            try { player2State?.Cleanup(); } catch { }
            try { combatSystem?.Cleanup(); } catch { }
            try { effectManager?.Cleanup(); } catch { }
            try { projectileManager?.Cleanup(); } catch { }
            // =========================================

            // ✅ RESUME: Theme music when returning to MainForm
            try { SoundManager.PlayMusic(DoAn_NT106.Client.BackgroundMusic.ThemeMusic, loop: true); } catch { }
            Console.WriteLine("🎵 Theme music resumed");

            // Dispose animation managers
            player1AnimationManager?.Dispose();
            player2AnimationManager?.Dispose();

            foreach (var s in resourceStreams)
            {
                try { s.Dispose(); } catch { }
            }
            resourceStreams.Clear();

            base.OnFormClosing(e);
        }

        // ===========================
        // ✅ HELPER METHODS (KEPT FROM ORIGINAL)
        // ===========================
        // ✅ THÊM: Xử lý tấn công với attack hitbox mới
        // ✅ SỬA LẦN 2: Xử lý tấn công với frame counter
        // In BattleForm. cs - THAY THẾ HÀM ExecuteAttackWithHitbox()

        private void ExecuteAttackWithHitbox(int playerNum, string attackType, int damage, int staminaCost)
        {
            Console.WriteLine($"[BattleForm] Player {playerNum} attempts {attackType}");
            combatSystem.ExecuteAttack(playerNum, attackType);
        }
        private void OnFrameChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void WalkAnimationTimer_Tick(object sender, EventArgs e)
        {
            if (player1Walking)
            {
                player1AnimationManager.UpdateFrames();
            }
            if (player2Walking)
            {
                player2AnimationManager.UpdateFrames();
            }
        }

        private Bitmap CreateColoredImage(int width, int height, Color color)
        {
            var bmp = new Bitmap(Math.Max(1, width), Math.Max(1, height));
            using (var g = Graphics.FromImage(bmp))
            using (var brush = new SolidBrush(color))
                g.FillRectangle(brush, 0, 0, bmp.Width, bmp.Height);
            return bmp;
        }

        private void CreateFallbackGraphics()
        {
            // Animation managers will handle fallback
            background = CreateColoredImage(backgroundWidth, this.ClientSize.Height, Color.DarkGreen);
            fireball = CreateColoredImage(40, 25, Color.Orange);
        }

        private Image ResourceToImage(object res)
        {
            if (res == null) return null;

            if (res is Image img)
            {
                try
                {
                    if (ImageAnimator.CanAnimate(img))
                        return img;
                    return new Bitmap(img);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ResourceToImage(Image) clone error: {ex}");
                    return null;
                }
            }

            if (res is byte[] b && b.Length > 0)
            {
                try
                {
                    var ms = new System.IO.MemoryStream(b);
                    var tmp = Image.FromStream(ms);
                    if (ImageAnimator.CanAnimate(tmp))
                    {
                        resourceStreams.Add(ms); // keep stream until form closes
                        return tmp;
                    }
                    else
                    {
                        var bmp = new Bitmap(tmp);
                        tmp.Dispose();
                        ms.Dispose();
                        return bmp;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ResourceToImage(byte[]) -> Image error: {ex}");
                    return null;
                }
            }

            return null;
        }

        private void DrawCharacter(Graphics g, int x, int y, string animation, string facing, CharacterAnimationManager animationManager)
        {
            if (animation == "invisible") return;
            int screenX = x - viewportX;
            if (screenX + PLAYER_WIDTH < 0 || screenX > this.ClientSize.Width) return;

            var characterImage = animationManager.GetAnimation(animation);
            if (characterImage != null)
            {
                string charType = "";
                if (animationManager == player1AnimationManager) charType = player1CharacterType;
                else if (animationManager == player2AnimationManager) charType = player2CharacterType;

                var actualSize = GetActualCharacterSize(charType);
                int drawHeight = PLAYER_HEIGHT;
                float sizeScale = 1.0f;
                int yOffset = 0;
                int groundAdjustment = actualSize.groundAdjustment;

                if (charType == "girlknight")
                {
                    sizeScale = 0.7f;
                    yOffset = (int)(PLAYER_HEIGHT * (1.0f - sizeScale));
                }
                else if (charType == "bringerofdeath")
                {
                    sizeScale = 1.6f;
                    yOffset += (int)(10 * globalCharacterScale);
                }

                drawHeight = (int)(PLAYER_HEIGHT * sizeScale);

                int drawWidth;
                if (charType == "warrior")
                {
                    // Base unified size using stand reference
                    drawWidth = Math.Max(1, actualSize.actualWidth);
                    drawHeight = Math.Max(1, actualSize.actualHeight);

                    // Upscale warrior's Kick (K) and Skill (I) animations to match normal size
                    if (string.Equals(animation, "kick", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(animation, "skill", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(animation, "fireball", StringComparison.OrdinalIgnoreCase))
                    {
                        const float upscale = 1.3f; // fine-tuned upscale
                        drawWidth = (int)(drawWidth * upscale);
                        drawHeight = (int)(drawHeight * upscale);
                    }
                }
                else
                {
                    int imgW = Math.Max(1, characterImage.Width);
                    int imgH = Math.Max(1, characterImage.Height);
                    drawWidth = Math.Max(1, (int)(drawHeight * (float)imgW / imgH));
                }

                int destX = screenX;
                int destY = y + yOffset + groundAdjustment;

                // Lift warrior's kick/skill by 35px without changing hitbox positions
                if (charType == "warrior" &&
                    (string.Equals(animation, "kick", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(animation, "skill", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(animation, "fireball", StringComparison.OrdinalIgnoreCase)))
                {
                    destY -= 110;
                }

                // ✅ NEW: Fix right-facing warrior kick horizontal offset by 30px (shift left)
                if (charType == "warrior" &&
                    string.Equals(animation, "kick", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(facing, "right", StringComparison.OrdinalIgnoreCase))
                {
                    destX -= 180; // move sprite 30px left to align
                }
                if (charType == "warrior" &&
                    string.Equals(animation, "fireball", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(facing, "right", StringComparison.OrdinalIgnoreCase))
                {
                    destX -= 180; // move sprite 30px left to align
                }
                var prevInterpolation = g.InterpolationMode;
                var prevSmoothing = g.SmoothingMode;
                var prevPixelOffset = g.PixelOffsetMode;
                var prevCompositing = g.CompositingQuality;

                try
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

                    if (facing == "left")
                    {
                        g.DrawImage(
                            characterImage,
                            new Rectangle(destX + drawWidth, destY, -drawWidth, drawHeight),
                            new Rectangle(0, 0, Math.Max(1, characterImage.Width), Math.Max(1, characterImage.Height)),
                            GraphicsUnit.Pixel);
                    }
                    else
                    {
                        g.DrawImage(
                            characterImage,
                            new Rectangle(destX, destY, drawWidth, drawHeight),
                            new Rectangle(0, 0, Math.Max(1, characterImage.Width), Math.Max(1, characterImage.Height)),
                            GraphicsUnit.Pixel);
                    }
                }
                finally
                {
                    g.InterpolationMode = prevInterpolation;
                    g.SmoothingMode = prevSmoothing;
                    g.PixelOffsetMode = prevPixelOffset;
                    g.CompositingQuality = prevCompositing;
                }
            }
            else
            {
                using (var brush = new SolidBrush(Color.Magenta))
                {
                    g.FillRectangle(brush, screenX, y, PLAYER_WIDTH, PLAYER_HEIGHT);
                }
            }
        }

        private void DrawGameUI(Graphics g)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(180, 0, 0, 0)),
                0, 0, this.ClientSize.Width, 110);
        }

        private void SetupEventHandlers()
        {
            cmbBackground.SelectedIndexChanged += CmbBackground_SelectedIndexChanged;
            btnBack.Click += BtnBack_Click;
            gameTimer.Tick += GameTimer_Tick;
            if (!gameTimer.Enabled) gameTimer.Start();

            this.KeyDown += BattleForm_KeyDown;
            this.KeyUp += BattleForm_KeyUp;
            this.Resize += BattleForm_Resize;
        }

        private void CmbBackground_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbBackground.SelectedIndex >= 0 && cmbBackground.SelectedIndex < backgroundImages.Count)
            {
                string bgName = backgroundImages[cmbBackground.SelectedIndex];
                currentBackground = cmbBackground.SelectedIndex;
                SetBackground(bgName);
            }
        }

        private void SetBackground(string backgroundName)
        {
            try
            {
                // KIỂM TRA 1: Form đã sẵn sàng chưa?
                if (this.ClientSize.Height <= 100 || this.IsDisposed || !this.IsHandleCreated)
                {
                    // Nếu form chưa sẵn sàng, đợi một chút và thử lại
                    Console.WriteLine($"[SetBackground] Form chưa sẵn sàng, ClientSize.Height={this.ClientSize.Height}");

                    var timer = new System.Windows.Forms.Timer();
                    timer.Interval = 50; // Giảm xuống 50ms cho nhanh
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();
                        timer.Dispose();
                        if (!this.IsDisposed)
                        {
                            SetBackground(backgroundName); // Gọi lại sau 50ms
                        }
                    };
                    timer.Start();
 return;
                }

                Image originalBg = null;
                switch (backgroundName.ToLower())
                {
                    case "battleground1": originalBg = Properties.Resources.battleground1; break;
                    case "battleground2": originalBg = Properties.Resources.battleground2; break;
                    case "battleground3": originalBg = Properties.Resources.battleground3; break;
                    case "battleground4": originalBg = Properties.Resources.battleground4; break;
                }

                if (originalBg != null)
                {
                    int screenHeight = this.ClientSize.Height;

                    // KIỂM TRA 2: Đảm bảo screenHeight hợp lệ
                    if (screenHeight <= 100)
                    {
                        screenHeight = 600; // Giá trị mặc định an toàn
                        Console.WriteLine($"[SetBackground] ClientSize.Height={this.ClientSize.Height}, using safe height={screenHeight}");
                    }

                    // KIỂM TRA 3: Đảm bảo originalBg có kích thước hợp lệ
                    if (originalBg.Width <= 0 || originalBg.Height <= 0)
                    {
                        Console.WriteLine($"[SetBackground] Original background has invalid size: {originalBg.Width}x{originalBg.Height}");
                        originalBg = CreateColoredImage(800, 600, Color.DarkGreen);
                    }

                    Console.WriteLine($"[SetBackground] Creating background: {backgroundWidth}x{screenHeight} from {originalBg.Width}x{originalBg.Height}");

                    // TẠO BACKGROUND với kích thước chính xác
                    background = new Bitmap(backgroundWidth, screenHeight);
                    using (var g = Graphics.FromImage(background))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                        // Vẽ lặp background ngang
                        for (int x = 0; x < backgroundWidth; x += originalBg.Width)
                        {
                            g.DrawImage(originalBg,
                                new Rectangle(x, 0, originalBg.Width, screenHeight),
                                new Rectangle(0, 0, originalBg.Width, originalBg.Height),
                                GraphicsUnit.Pixel);
                        }
                    }

                    // CẬP NHẬT groundOffset THEO BACKGROUND ĐÃ CHỌN
                    if (backgroundGroundOffsets.ContainsKey(backgroundName.ToLower()))
                    {
                        int newGroundOffset = backgroundGroundOffsets[backgroundName.ToLower()];

                        // Chỉ cập nhật nếu khác giá trị cũ
                        if (groundOffset != newGroundOffset)
                        {
                            groundOffset = newGroundOffset;
                            groundLevel = Math.Max(0, this.ClientSize.Height - groundOffset);

                            Console.WriteLine($"[SetBackground] Updated groundOffset={groundOffset}, groundLevel={groundLevel}");

                            // CẬP NHẬT VỊ TRÍ PLAYER NGAY LẬP TỨC
                            if (player1State != null)
                            {
                                player1State.Y = groundLevel - PLAYER_HEIGHT;
                                player1Y = player1State.Y;
                            }
                            if (player2State != null)
                            {
                                player2State.Y = groundLevel - PLAYER_HEIGHT;
                                player2Y = player2State.Y;
                            }

                            // CẬP NHẬT PHYSICSSYSTEM
                            if (physicsSystem != null)
                            {
                                physicsSystem.UpdateGroundLevel(groundLevel);
                                if (player1State != null) physicsSystem.ResetToGround(player1State);
                                if (player2State != null) physicsSystem.ResetToGround(player2State);
                            }
                        }
                    }
                }
                else
                {
                    // Fallback nếu không load được background
                    int screenHeight = Math.Max(100, this.ClientSize.Height);
                    background = CreateColoredImage(backgroundWidth, screenHeight, Color.DarkGreen);
                    Console.WriteLine($"[SetBackground] Using fallback background");
                }

                // FORCE REDRAW
                this.Invalidate();

                Console.WriteLine($"[SetBackground] Background '{backgroundName}' loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SetBackground ERROR] {ex.Message}");
                Console.WriteLine($"[SetBackground ERROR] StackTrace: {ex.StackTrace}");

                // Fallback cứng
                try
                {
                    background = CreateColoredImage(backgroundWidth, 600, Color.DarkGreen);
                    this.Invalidate();
                }
                catch { }
            }
        }

        private void SetupControlsInfo()
        {
            lblControlsInfo = new Label
            {
                Text = "Player 1: A/D (Move) | W (Jump) | J (Attack1) | K (Attack2) | L (Dash) | U (Parry) | I (Skill)\n" +
                       "Player 2: ←/→ (Move) | ↑ (Jump) | Num1 (Attack1) | Num2 (Attack2) | Num3 (Dash) | Num5 (Parry) | Num4 (Skill)",
                Location = new Point(this.ClientSize.Width / 2 - 350, this.ClientSize.Height - 60),
                Size = new Size(700, 40),
                ForeColor = Color.White,
                Font = new Font("Arial", 8, FontStyle.Bold),
                BackColor = Color.FromArgb(150, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.Add(lblControlsInfo);
        }
        /// <summary>
        /// Get actual character width for movement boundaries
        /// </summary>
        private int GetCharacterBoundaryWidth(string characterType)
        {
            var actualSize = GetActualCharacterSize(characterType);
            return actualSize.actualWidth; // Trả về kích thước vẽ thực tế
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            try { gameTimer?.Stop(); } catch { }
            try { walkAnimationTimer?.Stop(); } catch { }

            this.Close();

            foreach (Form form in Application.OpenForms)
            {
                if (form is MainForm mainForm)
                {
                    mainForm.Show();
                    break;
                }
            }
        }

        private void ShowHitEffect(string message, Color color)
        {
            var hitLabel = new Label
            {
                Text = message,
                ForeColor = color,
                BackColor = Color.FromArgb(150, 255, 255, 255),
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(this.ClientSize.Width / 2 - 40, 150)
            };

            this.Controls.Add(hitLabel);
            hitLabel.BringToFront();

            System.Windows.Forms.Timer removeTimer = new System.Windows.Forms.Timer();
            removeTimer.Interval = 800;
            removeTimer.Tick += (s, e) =>
            {
                this.Controls.Remove(hitLabel);
                removeTimer.Stop();
                removeTimer.Dispose();
            };
            removeTimer.Start();
        }
        // Phương thức tính hitbox theo cấu hình nhân vật
        private Rectangle GetPlayerHitbox(int playerX, int playerY, string characterType, string facing)
        {
            if (!characterHurtboxConfigs.ContainsKey(characterType))
            {
                return new Rectangle(playerX, playerY, PLAYER_WIDTH, PLAYER_HEIGHT);
            }

            var config = characterHurtboxConfigs[characterType];

            int hitboxWidth = (int)(PLAYER_WIDTH * config.WidthPercent);
            int hitboxHeight = (int)(PLAYER_HEIGHT * config.HeightPercent);

            // Căn giữa hitbox, KHÔNG phụ thuộc vào facing
            int offsetX = (PLAYER_WIDTH - hitboxWidth) / 2;  // ⭐ THAY ĐỔI DUY NHẤT
            int offsetY = (int)(PLAYER_HEIGHT * config.OffsetYPercent);

            return new Rectangle(
                playerX + offsetX,
                playerY + offsetY,
                hitboxWidth,
                hitboxHeight
            );
        }

        // Overload để lấy hitbox từ PlayerState
        private Rectangle GetPlayerHitbox(PlayerState player)
        {
            var actualSize = GetActualCharacterSize(player.CharacterType);
            int actualWidth = actualSize.actualWidth;
            int actualHeight = actualSize.actualHeight;
            int yOffset = actualSize.yOffset;
            int groundAdjustment = actualSize.groundAdjustment;

            if (!characterHurtboxConfigs.ContainsKey(player.CharacterType))
            {
                return new Rectangle(player.X, player.Y + yOffset + groundAdjustment, actualWidth, actualHeight);
            }

            var config = characterHurtboxConfigs[player.CharacterType];

            int hitboxWidth = (int)(actualWidth * config.WidthPercent);
            int hitboxHeight = (int)(actualHeight * config.HeightPercent);

            // ✅ CĂN GIỮA HOÀN HẢO
            int offsetX = (actualWidth - hitboxWidth) / 2;
            int offsetY = (int)(actualHeight * config.OffsetYPercent);

            // ✅ CHỈ GOATMAN MỚI CÓ HARD FIX
            if (player.CharacterType == "goatman")
            {
                offsetX += 65; // Sprite padding fix
            }
            // Bringer of Death, Warrior, GirlKnight đều căn giữa tự nhiên

            Console.WriteLine($"[{player.CharacterType}] Hurtbox: PlayerX={player.X}, ActualW={actualWidth}, HitboxW={hitboxWidth}, OffsetX={offsetX}, FinalX={player.X + offsetX}");

            return new Rectangle(
                player.X + offsetX,
                player.Y + yOffset + groundAdjustment + offsetY,
                hitboxWidth,
                hitboxHeight
            );
        }

        // ✅ THÊM: Phương thức tính vùng tấn công của nhân vật
        private Rectangle GetAttackHitbox(PlayerState attacker, string attackType)
        {
            if (!characterAttackConfigs.ContainsKey(attacker.CharacterType) ||
                !characterAttackConfigs[attacker.CharacterType].ContainsKey(attackType))
            {
                Console.WriteLine($"⚠️ No attack config for {attacker.CharacterType}.{attackType}, using default");

                var actualSize = GetActualCharacterSize(attacker.CharacterType);
                int attackWidth = (int)(actualSize.actualWidth * 0.8f);
                int attackHeight = (int)(actualSize.actualHeight * 0.6f);
                int attackRange = (int)(actualSize.actualWidth * 0.7f);

                int defaultCenterX = attacker.X + (actualSize.actualWidth / 2);
                int attackX = attacker.Facing == "right" ? defaultCenterX : defaultCenterX - attackRange;
                int attackY = attacker.Y + actualSize.yOffset + actualSize.groundAdjustment + (int)(actualSize.actualHeight * 0.3f);

                return new Rectangle(attackX, attackY, attackRange, attackHeight);
            }

            var config = characterAttackConfigs[attacker.CharacterType][attackType];
            var actualSizeWithConfig = GetActualCharacterSize(attacker.CharacterType);

            int actualWidth = actualSizeWithConfig.actualWidth;
            int actualHeight = actualSizeWithConfig.actualHeight;
            int yOffset = actualSizeWithConfig.yOffset;
            int groundAdjustment = actualSizeWithConfig.groundAdjustment;

            int attackWidthValue = (int)(actualWidth * config.WidthPercent);
            int attackHeightValue = (int)(actualHeight * config.HeightPercent);
            int attackRangeValue = (int)(actualWidth * config.RangePercent);
            int offsetY = (int)(actualHeight * config.OffsetYPercent);

            int configCenterX = attacker.X + (actualWidth / 2);
            if (attacker.CharacterType == "goatman")
            {
                configCenterX += 60;
            }

            int finalAttackX, finalAttackY;

            // ✅ SPECIAL CASE: Girl Knight skill should hit forward AND backward (extend range to opposite direction)
            if (attacker.CharacterType == "girlknight" && attackType == "skill")
            {
                // Bidirectional area: cover forward + backward
                // Reduce 10px on each side → shift start by +10 and reduce total width by 20
                finalAttackX = (configCenterX - attackRangeValue) + 10;
                finalAttackY = attacker.Y + yOffset + groundAdjustment + offsetY;
                int bidirectionalWidth = (attackRangeValue * 2) - 20;
                if (bidirectionalWidth < 0) bidirectionalWidth = 0; // safety
                Console.WriteLine($"[GetAttackHitbox] GIRLKNIGHT.skill (FORWARD+BACK -10px each side): X={finalAttackX}, Y={finalAttackY}, W={bidirectionalWidth}, H={attackHeightValue}");
                return new Rectangle(finalAttackX, finalAttackY, bidirectionalWidth, attackHeightValue);
            }

            // Default directional logic (forward-only)
            if (attacker.Facing == "right")
            {
                finalAttackX = configCenterX;
                finalAttackY = attacker.Y + yOffset + groundAdjustment + offsetY;
            }
            else
            {
                finalAttackX = configCenterX - attackRangeValue;
                finalAttackY = attacker.Y + yOffset + groundAdjustment + offsetY;
            }

            Console.WriteLine($"[GetAttackHitbox] {attacker.CharacterType}.{attackType}: X={finalAttackX}, Y={finalAttackY}, W={attackRangeValue}, H={attackHeightValue}");
            return new Rectangle(finalAttackX, finalAttackY, attackRangeValue, attackHeightValue);
        }
        // ✅ THÊM: Kiểm tra va chạm giữa attack hitbox và hurtbox
        private bool CheckAttackHit(PlayerState attacker, PlayerState defender, string attackType)
        {
            // Lấy vùng tấn công của attacker
            Rectangle attackBox = GetAttackHitbox(attacker, attackType);

            // Lấy vùng nhận sát thương của defender
            Rectangle hurtBox = GetPlayerHitbox(defender);

            // ✅ THÊM DEBUG
            Console.WriteLine($"[CHECK HIT] Attack Box: ({attackBox.X}, {attackBox.Y}, {attackBox.Width}x{attackBox.Height})");
            Console.WriteLine($"[CHECK HIT] Hurt Box: ({hurtBox.X}, {hurtBox.Y}, {hurtBox.Width}x{hurtBox.Height})");

            // Kiểm tra va chạm
            bool hit = attackBox.IntersectsWith(hurtBox);

            // ✅ THÊM DEBUG
            Console.WriteLine($"[CHECK HIT] Result: {(hit ? "✅ HIT" : "❌ MISS")}");

            return hit;
        }
        private void CheckFireballHit()
        {
            if (!fireballActive) return;

            Rectangle fireRect = new Rectangle(fireballX, fireballY, FIREBALL_WIDTH, FIREBALL_HEIGHT);

            if (fireballOwner == 1)
            {
                // Sử dụng hitbox động
                Rectangle p2Rect = GetPlayerHitbox(player2State);
                if (fireRect.IntersectsWith(p2Rect))
                {
                    if (player2Parrying)
                    {
                        // reflect: send fireball back
                        fireballDirection *= -1;
                        fireballOwner = 2;
                        // reposition just in front of parrier
                        fireballX = player2X + (player2Facing == "right" ? PLAYER_WIDTH + 5 : -FIREBALL_WIDTH - 5);
                        ShowHitEffect("Reflected!", Color.Orange);
                    }
                    else
                    {
                        ApplyHurtToPlayer(2, 20);
                        fireballActive = false;
                        ShowHitEffect("Fireball Hit!", Color.Yellow);
                    }
                }
            }
            else if (fireballOwner == 2)
            {
                //  Sử dụng hitbox động
                Rectangle p1Rect = GetPlayerHitbox(player1State);
                if (fireRect.IntersectsWith(p1Rect))
                {
                    if (player1Parrying)
                    {
                        // reflect: send fireball back
                        fireballDirection *= -1;
                        fireballOwner = 1;
                        // reposition just in front of parrier
                        fireballX = player1X + (player1Facing == "right" ? PLAYER_WIDTH + 5 : -FIREBALL_WIDTH - 5);
                        ShowHitEffect("Reflected!", Color.Orange);
                    }
                    else
                    {
                        ApplyHurtToPlayer(1, 20);
                        fireballActive = false;
                        ShowHitEffect("Fireball Hit!", Color.Yellow);
                    }
                }
            }
        }

        // Apply hurt properly - KEEP OLD IMPLEMENTATION FOR NOW (COMPATIBILITY)
        private void ApplyHurtToPlayer(int player, int damage, bool knockback = true)
        {
            // ✅ GỌI COMBATSYSTEM TRỰC TIẾP - ĐÃ XỬ LÝ ĐẦY ĐỦ
            combatSystem.ApplyDamage(player, damage, knockback);

            // ✅ SYNC LẠI BIẾN CŨ (để UI hoạt động)
            player1Health = player1State.Health;
            player2Health = player2State.Health;
            player1Stunned = player1State.IsStunned;
            player2Stunned = player2State.IsStunned;
            player1CurrentAnimation = player1State.CurrentAnimation;
            player2CurrentAnimation = player2State.CurrentAnimation;
        }

        private void SetPlayerHealth(PlayerState playerState, string characterType)
{
    int baseHealth = 100; // Giá trị mặc định

    // Đặt HP tối đa theo từng nhân vật
    switch (characterType)
    {
        case "goatman":
            baseHealth = 130;
            break;
        case "bringerofdeath":
            baseHealth = 90;
            break;
        case "warrior":
            baseHealth = 80;
            break;
        case "girlknight":
            baseHealth = 100;
            break;
    }

    playerState.Health = baseHealth;
}
    }
}