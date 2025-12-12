using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using DoAn_NT106.Client.BattleSystems;
using DoAn_NT106.Client;
using PixelGameLobby;
using DoAn_NT106.Services;
using System.Text.Json;
using System.Threading.Tasks;

namespace DoAn_NT106
{
    public partial class BattleForm : Form
    {
        private string username;
        private string token;
        private string opponent;
        private string roomCode = "000000";

        // ✅ THÊM: UDP Game Client
        private UDPGameClient udpClient;
        private bool isOnlineMode = false; // Track nếu đang chơi online
        private int myPlayerNumber = 0; // 1 or 2

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
        // If online mode, both players use WASD locally. If offline, Player2 uses arrow keys.
        private bool bothPlayersUseWASD = false;
        private bool isPaused = false;

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
            public float WidthPercent { get; set; }
            public float HeightPercent { get; set; }
            public float RangePercent { get; set; }
            public float OffsetYPercent { get; set; }
        }

        public class AttackAnimationConfig
        {
            public int FPS { get; set; }
            public int TotalFrames { get; set; }
            public List<int> HitFrames { get; set; }
            public int Duration => (int)((TotalFrames / (float)FPS) * 1000);
        }

        public class HitboxConfig
        {
            public float WidthPercent { get; set; }
            public float HeightPercent { get; set; }
            public float OffsetYPercent { get; set; }
            public float OffsetXPercent { get; set; } = 0f;
        }

        // Compute attack hitbox for an attacker based on config
        private Rectangle GetAttackHitbox(PlayerState attacker, string attackType)
        {
            var actual = GetActualCharacterSize(attacker.CharacterType);
            int actualWidth = actual.actualWidth;
            int actualHeight = actual.actualHeight;
            int yOffset = actual.yOffset;
            int groundAdjustment = actual.groundAdjustment;

            if (!characterAttackConfigs.ContainsKey(attacker.CharacterType) ||
                !characterAttackConfigs[attacker.CharacterType].ContainsKey(attackType))
            {
                int attackRange = (int)(actualWidth * 0.7f);
                int attackHeight = (int)(actualHeight * 0.6f);
                int centerX = attacker.X + actualWidth / 2;
                int attackX = attacker.Facing == "right" ? centerX : centerX - attackRange;
                int attackY = attacker.Y + yOffset + groundAdjustment + (int)(actualHeight * 0.3f);
                return new Rectangle(attackX, attackY, attackRange, attackHeight);
            }

            var cfg = characterAttackConfigs[attacker.CharacterType][attackType];
            int attackW = (int)(actualWidth * cfg.WidthPercent);
            int attackH = (int)(actualHeight * cfg.HeightPercent);
            int attackRangeVal = (int)(actualWidth * cfg.RangePercent);
            int offsetY = (int)(actualHeight * cfg.OffsetYPercent);

            int center = attacker.X + actualWidth / 2;
            if (attacker.CharacterType == "girlknight" && attackType == "skill")
            {
                int finalX = (center - attackRangeVal) + 10;
                int finalW = attackRangeVal * 2 - 20;
                if (finalW < 0) finalW = 0;
                return new Rectangle(finalX, attacker.Y + yOffset + groundAdjustment + offsetY, finalW, attackH);
            }

            int finalX2 = attacker.Facing == "right" ? center : center - attackRangeVal;
            int finalY2 = attacker.Y + yOffset + groundAdjustment + offsetY;
            return new Rectangle(finalX2, finalY2, attackRangeVal, attackH);
        }

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

        // Hurt handling
        private const int HURT_DISPLAY_MS = 400;
        private string _prevAnimPlayer1 = null;
        private string _prevAnimPlayer2 = null;

        // Character types
        private string player1CharacterType = "girlknight";
        private string player2CharacterType = "girlknight";

        // Added myPlayerNumber last param so caller can indicate which player this client controls
        public BattleForm(string username, string token, string opponent, string player1Character, string player2Character, string selectedMap = "battleground1", string roomCode = "000000", int myPlayerNumber = 1)
        {
            InitializeComponent();

            this.username = username;
            this.token = token;
            this.opponent = opponent;
            this.roomCode = roomCode;
            this.player1CharacterType = player1Character;
            this.player2CharacterType = player2Character;

            // ✅ THÊM: Kiểm tra online mode
            isOnlineMode = !string.IsNullOrEmpty(roomCode) && roomCode != "000000";

            // If online, prefer WASD for both players locally
            bothPlayersUseWASD = isOnlineMode;

            // ✅ Set map background based on selectedMap ("battleground1" format)
            // ✅ FIX: selectedMap is already "battleground1" format, find its index
            int mapIndex = -1;
            if (!string.IsNullOrEmpty(selectedMap))
            {
                // Extract number from "battleground4" → 4
                string mapNumberStr = selectedMap.Replace("battleground", "");
                if (int.TryParse(mapNumberStr, out int mapNum) && mapNum >= 1 && mapNum <= 4)
                {
                    mapIndex = mapNum - 1;  // 1 → index 0, 4 → index 3
                }
            }
            currentBackground = (mapIndex >= 0 && mapIndex < 4) ? mapIndex : 0;
            
            Console.WriteLine($"[BattleForm] selectedMap='{selectedMap}' → mapIndex={currentBackground}");

            // ✅ RESTORE: Start maximized like original behavior, allow ALT+TAB
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable; // Allow resizing
            this.Text = $"⚔️ {(myPlayerNumber == 1 ? "Player 1" : "Player 2")} - {username} vs {opponent}";
            
            Console.WriteLine($"[BattleForm] Window created for Player {myPlayerNumber}");
            this.myPlayerNumber = myPlayerNumber;
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

                // ✅ THÊM: Initialize UDP if online mode
                if (isOnlineMode)
                {
                    try
                    {
                        udpClient = new UDPGameClient("127.0.0.1", 5000, roomCode, username);
                        udpClient.SetPlayerNumber(myPlayerNumber);
                        udpClient.OnLog += (msg) => Console.WriteLine($"[UDP] {msg}");
                        udpClient.OnOpponentState += HandleOpponentState;
                        udpClient.OnCombatEvent += HandleCombatEvent; // ✅ ADD: Subscribe to combat events
                        udpClient.Connect();
                        Console.WriteLine($"✅ UDP initialized for player {myPlayerNumber}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ UDP init error: {ex.Message}");
                    }
                }

                // Load animations...
                player1AnimationManager = new CharacterAnimationManager(player1CharacterType, OnFrameChanged);
                player1AnimationManager.LoadAnimations();

                player2AnimationManager = new CharacterAnimationManager(player2CharacterType, OnFrameChanged);
                player2AnimationManager.LoadAnimations();

                // ===== ✅ INITIALIZE NEW SYSTEMS =====
                // 1. Initialize PlayerState instances - SỬA Y position
                // ✅ FIX: Determine correct names based on myPlayerNumber
                string actualPlayer1Name = myPlayerNumber == 1 ? username : opponent;
                string actualPlayer2Name = myPlayerNumber == 1 ? opponent : username;
                
                Console.WriteLine($"[SetupGame] Player {myPlayerNumber}: Me={username}, Opponent={opponent}");
                Console.WriteLine($"[SetupGame] PlayerState: P1={actualPlayer1Name}, P2={actualPlayer2Name}");
                
                player1State = new PlayerState(actualPlayer1Name, player1CharacterType, 1)
                {
                    X = 150, // ✅ SỬA: từ 300 → 150
                    Y = groundLevel - PLAYER_HEIGHT,
                    Facing = "right",
                    CurrentAnimation = "stand"
                };
                
                // ✅ SỬA: Set HP theo character type
                SetPlayerHealth(player1State, player1CharacterType);

                player2State = new PlayerState(actualPlayer2Name, player2CharacterType, 2)
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
                    GetPlayerHitbox,
                    SendCombatEventViaUDP // ✅ ADD: Pass callback to send combat events
                );
                // =====================================

                // ✅ THÊM DÒNG NÀY - SAU KHI ĐÃ CÓ physicsSystem!
                UpdateCharacterSize();

                // Khởi tạo walk animation timer
                walkAnimationTimer = new System.Windows.Forms.Timer();
                walkAnimationTimer.Interval = 100;
                walkAnimationTimer.Tick += WalkAnimationTimer_Tick;
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

            // Subscribe to TCP broadcasts for online mode sync
            try { PersistentTcpClient.Instance.OnBroadcast += HandleTcpBroadcast; } catch { }

            // Ensure this form is tracked as an open BattleForm for lookup by other UI
            try
            {
                var list = Application.OpenForms.OfType<BattleForm>().ToList();
                // nothing to do, presence in OpenForms is enough
            }
            catch { }

            // ✅ THÊM: Load background map
            string backgroundName = $"battleground{currentBackground + 1}";
            SetBackground(backgroundName);
            Console.WriteLine($"[SetupGame] Background loaded: {backgroundName}");

            // Initialize round system
            InitializeRoundSystem();
        }

        // Helper: set player health based on character type
        private void SetPlayerHealth(PlayerState playerState, string characterType)
        {
            int baseHealth = 100;
            switch (characterType)
            {
                case "goatman": baseHealth = 130; break;
                case "bringerofdeath": baseHealth = 90; break;
                case "warrior": baseHealth = 80; break;
                case "girlknight": baseHealth = 100; break;
            }
            playerState.Health = baseHealth;
        }

        // Compute player hurtbox based on configuration
        private Rectangle GetPlayerHitbox(PlayerState player)
        {
            var actual = GetActualCharacterSize(player.CharacterType);
            int actualWidth = actual.actualWidth;
            int actualHeight = actual.actualHeight;
            int yOffset = actual.yOffset;
            int groundAdjustment = actual.groundAdjustment;

            if (!characterHurtboxConfigs.ContainsKey(player.CharacterType))
            {
                return new Rectangle(player.X, player.Y + yOffset + groundAdjustment, actualWidth, actualHeight);
            }

            var cfg = characterHurtboxConfigs[player.CharacterType];
            int hitW = (int)(actualWidth * cfg.WidthPercent);
            int hitH = (int)(actualHeight * cfg.HeightPercent);
            int offsetX = (actualWidth - hitW) / 2 + (int)(actualWidth * cfg.OffsetXPercent);
            int offsetY = (int)(actualHeight * cfg.OffsetYPercent);

            return new Rectangle(player.X + offsetX, player.Y + yOffset + groundAdjustment + offsetY, hitW, hitH);
        }

        // Apply damage via combatSystem and sync local state
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

        // Send game action to server (fire-and-forget)
        private async void SendGameAction(string actionType, Dictionary<string, object> data = null)
        {
            try
            {
                var tcp = PersistentTcpClient.Instance;
                if (tcp == null) return;
                if (!tcp.IsConnected) return;

                var payload = data != null ? new Dictionary<string, object>(data) : new Dictionary<string, object>();
                payload["roomCode"] = roomCode ?? string.Empty;
                payload["username"] = username ?? string.Empty;
                payload["type"] = actionType;
                payload["playerNumber"] = myPlayerNumber;

                // Fire-and-forget
                _ = tcp.SendRequestAsync("GAME_ACTION", payload);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BattleForm] SendGameAction error: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ NEW: Send combat events via UDP (called by CombatSystem)
        /// </summary>
        private void SendCombatEventViaUDP(string eventType, Dictionary<string, object> eventData)
        {
            if (!isOnlineMode || udpClient == null || !udpClient.IsConnected)
                return;

            try
            {
                udpClient.SendCombatEvent(eventType, eventData);
                Console.WriteLine($"[UDP] Sent {eventType} event: {System.Text.Json.JsonSerializer.Serialize(eventData)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SendCombatEventViaUDP error: {ex.Message}");
            }
        }

        // Send current controlled player state immediately over UDP
        private void SendLocalUdpState()
        {
            if (!isOnlineMode || udpClient == null || !udpClient.IsConnected) return;

            try
            {
                if (myPlayerNumber == 1)
                {
                    udpClient.SendImmediateState(
                        player1State.X, player1State.Y,
                        player1State.Health, player1State.Stamina, player1State.Mana,
                        player1State.CurrentAnimation, player1State.Facing
                    );
                }
                else if (myPlayerNumber == 2)
                {
                    udpClient.SendImmediateState(
                        player2State.X, player2State.Y,
                        player2State.Health, player2State.Stamina, player2State.Mana,
                        player2State.CurrentAnimation, player2State.Facing
                    );
                }
            }
            catch { }
        }

        // Show a quick hit effect label
        private void ShowHitEffect(string message, Color color)
        {
            try
            {
                var hitLabel = new Label
                {
                    Text = message,
                    ForeColor = color,
                    BackColor = Color.FromArgb(180, 255, 255, 255),
                    Font = new Font("Arial", 12, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(this.ClientSize.Width / 2 - 40, 150)
                };
                this.Controls.Add(hitLabel);
                hitLabel.BringToFront();

                var removeTimer = new System.Windows.Forms.Timer { Interval = 800 };
                removeTimer.Tick += (s, e) => { this.Controls.Remove(hitLabel); removeTimer.Stop(); removeTimer.Dispose(); };
                removeTimer.Start();
            }
            catch { }
        }

        // Check current fireball collision
        private void CheckFireballHit()
        {
            if (!fireballActive) return;

            var fireRect = new Rectangle(fireballX, fireballY, FIREBALL_WIDTH, FIREBALL_HEIGHT);

            if (fireballOwner == 1)
            {
                var p2Rect = GetPlayerHitbox(player2State);
                if (fireRect.IntersectsWith(p2Rect))
                {
                    if (player2State.IsParrying)
                    {
                        fireballDirection *= -1; fireballOwner = 2;
                        fireballX = player2State.X + (player2State.Facing == "right" ? PLAYER_WIDTH + 5 : -FIREBALL_WIDTH - 5);
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
                var p1Rect = GetPlayerHitbox(player1State);
                if (fireRect.IntersectsWith(p1Rect))
                {
                    if (player1State.IsParrying)
                    {
                        fireballDirection *= -1; fireballOwner = 1;
                        fireballX = player1State.X + (player1State.Facing == "right" ? PLAYER_WIDTH + 5 : -FIREBALL_WIDTH - 5);
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

        // In BattleForm.cs - GameTimer_Tick() - VERSION CLEAN

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // ===== MOVEMENT LOGIC =====
            player1State.IsWalking = false;
            player2State.IsWalking = false;
            
            // If offline mode, apply both players' local inputs
            if (!isOnlineMode)
            {
                if (player1State.CanMove)
                {
                    // ✅ FIX: Only move if key is pressed, always stop if neither key is pressed
                    if (aPressed && !dPressed)
                    {
                        physicsSystem.MovePlayer(player1State, -1);
                    }
                    else if (dPressed && !aPressed)
                    {
                        physicsSystem.MovePlayer(player1State, 1);
                    }
                    else
                    {
                        // ✅ Stop movement when no keys are pressed or both are pressed
                        physicsSystem.StopMovement(player1State);
                    }
                }
                else
                {
                    // ✅ Force stop if player can't move
                    physicsSystem.StopMovement(player1State);
                }

                if (player2State.CanMove)
                {
                    // ✅ FIX: Only move if key is pressed, always stop if neither key is pressed
                    if (leftPressed && !rightPressed)
                    {
                        physicsSystem.MovePlayer(player2State, -1);
                    }
                    else if (rightPressed && !leftPressed)
                    {
                        physicsSystem.MovePlayer(player2State, 1);
                    }
                    else
                    {
                        // ✅ Stop movement when no keys are pressed or both are pressed
                        physicsSystem.StopMovement(player2State);
                    }
                }
                else
                {
                    // ✅ Force stop if player can't move
                    physicsSystem.StopMovement(player2State);
                }
            }
            else
            {
                // Online mode: only apply local player's input
                if (myPlayerNumber == 1)
                {
                    if (player1State.CanMove)
                    {
                        // ✅ FIX: Only move if key is pressed, always stop if neither key is pressed
                        if (aPressed && !dPressed)
                        {
                            physicsSystem.MovePlayer(player1State, -1);
                        }
                        else if (dPressed && !aPressed)
                        {
                            physicsSystem.MovePlayer(player1State, 1);
                        }
                        else
                        {
                            // ✅ Stop movement when no keys are pressed or both are pressed
                            physicsSystem.StopMovement(player1State);
                        }
                    }
                    else
                    {
                        // ✅ Force stop if player can't move
                        physicsSystem.StopMovement(player1State);
                    }
                }

                if (myPlayerNumber == 2)
                {
                    if (player2State.CanMove)
                    {
                        // ✅ FIX: Use WASD if bothPlayersUseWASD is true, otherwise arrow keys
                        bool moveLeft = bothPlayersUseWASD ? (aPressed && !dPressed) : (leftPressed && !rightPressed);
                        bool moveRight = bothPlayersUseWASD ? (dPressed && !aPressed) : (rightPressed && !leftPressed);
                        
                        if (moveLeft)
                        {
                            physicsSystem.MovePlayer(player2State, -1);
                        }
                        else if (moveRight)
                        {
                            physicsSystem.MovePlayer(player2State, 1);
                        }
                        else
                        {
                            // ✅ Stop movement when no keys are pressed or both are pressed
                            physicsSystem.StopMovement(player2State);
                        }
                    }
                    else
                    {
                        // ✅ Force stop if player can't move
                        physicsSystem.StopMovement(player2State);
                    }
                }
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

            // ===== UDP: Send local state (only for controlled player) =====
            if (isOnlineMode && udpClient != null && udpClient.IsConnected)
            {
                try
                {
                    if (myPlayerNumber == 1)
                    {
                        udpClient.UpdateState(
                            player1State.X,
                            player1State.Y,
                            player1State.Health,
                            player1State.Stamina,
                            player1State.Mana,
                            player1State.CurrentAnimation,
                            player1State.Facing // ✅ ADD: Send facing direction
                        );
                        // send immediate one-shot for low-latency after inputs
                        SendLocalUdpState();
                    }
                    else if (myPlayerNumber == 2)
                    {
                        udpClient.UpdateState(
                            player2State.X,
                            player2State.Y,
                            player2State.Health,
                            player2State.Stamina,
                            player2State.Mana,
                            player2State.CurrentAnimation,
                            player2State.Facing // ✅ ADD: Send facing direction
                        );
                        SendLocalUdpState();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ UDP update error: {ex.Message}");
                }
            }

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
            // ✅ FIX: Remove Form.ActiveForm check to allow both forms to receive input
            // This allows Player 1 and Player 2 to control their characters in separate windows

            // ✅ FIX: Allow ESC for all players
            if (e.KeyCode == Keys.Escape)
            {
                try { OpenMainMenu(); } catch { ResumeGame(); }
                e.Handled = true;
                return;
            }

            // Handle Player1 input (if this client controls player1 or in local dual-control mode)
            if (myPlayerNumber == 1 || myPlayerNumber == 0)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        if (player1State.CanMove) { aPressed = true; player1State.LeftKeyPressed = true; }
                        break;
                    case Keys.D:
                        if (player1State.CanMove) { dPressed = true; player1State.RightKeyPressed = true; }
                        break;
                    case Keys.W:
                        physicsSystem.Jump(player1State);
                        break;
                    case Keys.J:
                        if (player1State.CanAttack) ExecuteAttackWithHitbox(1, "punch", 10, 15);
                        break;
                    case Keys.K:
                        if (player1State.CanAttack) ExecuteAttackWithHitbox(1, "kick", 15, 20);
                        break;
                    case Keys.L:
                        if (player1State.CanDash) combatSystem.ExecuteDash(1);
                        break;
                    case Keys.U:
                        if (player1State.CanParry) combatSystem.StartParry(1);
                        break;
                    case Keys.I:
                        if (player1State.CanAttack) combatSystem.ToggleSkill(1);
                        break;
                }
            }

            // Handle Player2 input (if this client controls player2, in local dual-control mode, or in offline mode)
            if (!isOnlineMode || myPlayerNumber == 2 || myPlayerNumber == 0)
            {
                // if WASD mode for both players, map A/D/W to player2 as well
                if (bothPlayersUseWASD && isOnlineMode)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.A:
                            // ✅ FIX: Player 2 uses aPressed (not leftPressed) in online mode
                            if (player2State.CanMove) { aPressed = true; player2State.LeftKeyPressed = true; }
                            break;
                        case Keys.D:
                            // ✅ FIX: Player 2 uses dPressed (not rightPressed) in online mode
                            if (player2State.CanMove) { dPressed = true; player2State.RightKeyPressed = true; }
                            break;
                        case Keys.W:
                            physicsSystem.Jump(player2State);
                            break;
                        case Keys.J:
                            if (player2State.CanAttack) ExecuteAttackWithHitbox(2, "punch", 10, 15);
                            break;
                        case Keys.K:
                            if (player2State.CanAttack) ExecuteAttackWithHitbox(2, "kick", 15, 20);
                            break;
                        case Keys.L:
                            if (player2State.CanDash) combatSystem.ExecuteDash(2);
                            break;
                        case Keys.U:
                            if (player2State.CanParry) combatSystem.StartParry(2);
                            break;
                        case Keys.I:
                            if (player2State.CanAttack) combatSystem.ToggleSkill(2);
                            break;
                    }
                }
                else
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Left:
                            if (player2State.CanMove) { leftPressed = true; player2State.LeftKeyPressed = true; }
                            break;
                        case Keys.Right:
                            if (player2State.CanMove) { rightPressed = true; player2State.RightKeyPressed = true; }
                            break;
                        case Keys.Up:
                            physicsSystem.Jump(player2State);
                            break;
                        // Allow number keys (top row) for offline player2 controls as alternative to numpad
                        case Keys.D1:
                            if (player2State.CanAttack) ExecuteAttackWithHitbox(2, "punch", 10, 15);
                            break;
                        case Keys.D2:
                            if (player2State.CanAttack) ExecuteAttackWithHitbox(2, "kick", 15, 20);
                            break;
                        case Keys.D3:
                            if (player2State.CanDash) combatSystem.ExecuteDash(2);
                            break;
                        case Keys.D5:
                            if (player2State.CanParry) combatSystem.StartParry(2);
                            break;
                        case Keys.D4:
                            if (player2State.CanAttack) combatSystem.ToggleSkill(2);
                            break;
                        // Keep numpad mapping too
                        case Keys.NumPad1:
                            if (player2State.CanAttack) ExecuteAttackWithHitbox(2, "punch", 10, 15);
                            break;
                        case Keys.NumPad2:
                            if (player2State.CanAttack) ExecuteAttackWithHitbox(2, "kick", 15, 20);
                            break;
                        case Keys.NumPad3:
                            if (player2State.CanDash) combatSystem.ExecuteDash(2);
                            break;
                        case Keys.NumPad5:
                            if (player2State.CanParry) combatSystem.StartParry(2);
                            break;
                        case Keys.NumPad4:
                            if (player2State.CanAttack) combatSystem.ToggleSkill(2);
                            break;
                    }
                }
            }

            e.Handled = true;
        }
      

        private void BattleForm_KeyUp(object sender, KeyEventArgs e)
        {
            // ✅ FIX: Remove Form.ActiveForm check to allow both forms to receive input
            switch (e.KeyCode)
            {
                case Keys.A: 
                    aPressed = false;
                    // ✅ FIX: Clear both player states (player1 or player2 depending on mode)
                    if (myPlayerNumber == 1 || myPlayerNumber == 0) player1State.LeftKeyPressed = false;
                    if (myPlayerNumber == 2 && bothPlayersUseWASD) player2State.LeftKeyPressed = false;
                    if (isOnlineMode && myPlayerNumber == 1) SendGameAction("move_stop", new Dictionary<string, object>{{"dir", -1}});
                    if (isOnlineMode && myPlayerNumber == 2 && bothPlayersUseWASD) SendGameAction("move_stop", new Dictionary<string, object>{{"dir", -1}});
                    break;
                case Keys.D: 
                    dPressed = false;
                    // ✅ FIX: Clear both player states (player1 or player2 depending on mode)
                    if (myPlayerNumber == 1 || myPlayerNumber == 0) player1State.RightKeyPressed = false;
                    if (myPlayerNumber == 2 && bothPlayersUseWASD) player2State.RightKeyPressed = false;
                    if (isOnlineMode && myPlayerNumber == 1) SendGameAction("move_stop", new Dictionary<string, object>{{"dir", 1}});
                    if (isOnlineMode && myPlayerNumber == 2 && bothPlayersUseWASD) SendGameAction("move_stop", new Dictionary<string, object>{{"dir", 1}});
                    break;
                case Keys.Left:
                    leftPressed = false;
                    player2State.LeftKeyPressed = false; // ✅ SYNC STATE
                    if (isOnlineMode && myPlayerNumber == 2) SendGameAction("move_stop", new Dictionary<string, object>{{"dir", -1}});
                    break;
                case Keys.Right:
                    rightPressed = false;
                    player2State.RightKeyPressed = false; // ✅ SYNC STATE
                    if (isOnlineMode && myPlayerNumber == 2) SendGameAction("move_stop", new Dictionary<string, object>{{"dir", 1}});
                    break;
                case Keys.Up:
                    // Up released for Player2 (offline). No persistent flag to clear.
                    break;
                case Keys.W:
                    // If using WASD for both players, releasing W should clear jump flags if any
                    if (bothPlayersUseWASD && myPlayerNumber == 2)
                    {
                        // No persistent flag for jump; physics handles jump state
                    }
                    break;
            }
            e.Handled = true;
        }

        private void PauseGame()
        {
            try
            {
                isPaused = true;
                try { gameTimer?.Stop(); } catch { }
                try { walkAnimationTimer?.Stop(); } catch { }
                try { player1DashTimer?.Stop(); } catch { }
                try { player2DashTimer?.Stop(); } catch { }
                try { player1SkillTimer?.Stop(); } catch { }
                try { player2SkillTimer?.Stop(); } catch { }
                // Optionally pause other timers/effects
            }
            catch { }
        }

        private void ResumeGame()
        {
            try
            {
                isPaused = false;
                try { if (gameTimer != null) gameTimer.Start(); } catch { }
                try { if (walkAnimationTimer != null) walkAnimationTimer.Start(); } catch { }
                try { if (player1DashTimer != null) player1DashTimer.Start(); } catch { }
                try { if (player2DashTimer != null) player2DashTimer.Start(); } catch { }
                try { if (player1SkillTimer != null) player1SkillTimer.Start(); } catch { }
                try { if (player2SkillTimer != null) player2SkillTimer.Start(); } catch { }
            }
            catch { }
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
            // Debug hitbox/hurtbox/skill drawing removed
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
            // ✅ THÊM: Disconnect UDP khi đóng form
            try
            {
                if (udpClient != null)
                {
                    udpClient.Disconnect();
                    udpClient.Dispose();
                    udpClient = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ UDP cleanup error: {ex.Message}");
            }

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

            // Unsubscribe from TCP broadcasts
            try { PersistentTcpClient.Instance.OnBroadcast -= HandleTcpBroadcast; } catch { }

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
            
            // ✅ ADD: Send attack event via UDP
            if (isOnlineMode && udpClient != null && udpClient.IsConnected)
            {
                try
                {
                    udpClient.SendCombatEvent("attack", new Dictionary<string, object>
                    {
                        {"attacker", playerNum},
                        {"attackType", attackType}
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ UDP send attack error: {ex.Message}");
                }
            }
            
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


        private void SetupEventHandlers()
        {
            gameTimer.Tick += GameTimer_Tick;
            if (!gameTimer.Enabled) gameTimer.Start();

            // Enable Key events for any local control mode
            // myPlayerNumber == 0 -> local single-window offline controlling both players
            // myPlayerNumber == 1 or 2 -> this window controls that single player (online or offline)
            this.KeyDown += BattleForm_KeyDown;
            this.KeyUp += BattleForm_KeyUp;
            this.Resize += BattleForm_Resize;
        }

        // Open main menu and handle returning to lobby (extracted from KeyDown)
        private void OpenMainMenu()
        {
            if (isPaused) return;

            PauseGame();
            using (var menu = new MainMenuForm(roomCode))
            {
                var res = menu.ShowDialog(this);
                if (res == DialogResult.OK)
                {
                    try { gameTimer?.Stop(); } catch { }
                    try { walkAnimationTimer?.Stop(); } catch { }

                    this.Close();

                    bool isOffline = string.IsNullOrEmpty(roomCode) || roomCode == "000000";
                    if (isOffline)
                    {
                        if (this.Owner is PixelGameLobby.JoinRoomForm ownerJoin)
                        {
                            try { ownerJoin.Show(); ownerJoin.BringToFront(); }
                            catch
                            {
                                var existing = Application.OpenForms.OfType<PixelGameLobby.JoinRoomForm>().FirstOrDefault();
                                if (existing != null) { existing.Show(); existing.BringToFront(); }
                                else Console.WriteLine("[BattleForm] No JoinRoomForm found to return to.");
                            }
                        }
                        else
                        {
                            var existing = Application.OpenForms.OfType<PixelGameLobby.JoinRoomForm>().FirstOrDefault();
                            if (existing != null) { existing.Show(); existing.BringToFront(); }
                            else Console.WriteLine("[BattleForm] No JoinRoomForm found to return to.");
                        }
                    }
                    else
                    {
                        var existingLobby = Application.OpenForms.OfType<PixelGameLobby.GameLobbyForm>()
                            .FirstOrDefault(f => string.Equals(((dynamic)f).RoomCode, roomCode, StringComparison.OrdinalIgnoreCase)
                                                 && string.Equals(((dynamic)f).Username, username, StringComparison.OrdinalIgnoreCase));
                        if (existingLobby != null)
                        {
                            try { existingLobby.Show(); existingLobby.BringToFront(); }
                            catch { Console.WriteLine("[BattleForm] Failed to show existing GameLobbyForm"); }
                        }
                        else
                        {
                            Console.WriteLine("[BattleForm] No existing GameLobbyForm found; skipping creation.");
                        }
                    }
                }
                else
                {
                    ResumeGame();
                }
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

        // Handle TCP broadcasts for online sync of game state
        private void HandleTcpBroadcast(string action, System.Text.Json.JsonElement data)
        {
            try
            {
                if (string.Equals(action, "GAME_STATE_UPDATE", StringComparison.OrdinalIgnoreCase))
                {
                    if (data.ValueKind == JsonValueKind.Object)
                    {
                        var gs = data;
                        // Update local PlayerState from broadcast
                        if (gs.TryGetProperty("Player1X", out var p1x)) player1State.X = p1x.GetInt32();
                        if (gs.TryGetProperty("Player1Y", out var p1y)) player1State.Y = p1y.GetInt32();
                        if (gs.TryGetProperty("Player2X", out var p2x)) player2State.X = p2x.GetInt32();
                        if (gs.TryGetProperty("Player2Y", out var p2y)) player2State.Y = p2y.GetInt32();
                        if (gs.TryGetProperty("Player1Action", out var p1a)) player1State.CurrentAnimation = p1a.GetString();
                        if (gs.TryGetProperty("Player2Action", out var p2a)) player2State.CurrentAnimation = p2a.GetString();

                        // Sync HP/Stamina/Mana if present
                        if (gs.TryGetProperty("Player1Health", out var ph1)) player1State.Health = ph1.GetInt32();
                        if (gs.TryGetProperty("Player2Health", out var ph2)) player2State.Health = ph2.GetInt32();
                        if (gs.TryGetProperty("Player1Stamina", out var ps1)) player1State.Stamina = ps1.GetInt32();
                        if (gs.TryGetProperty("Player2Stamina", out var ps2)) player2State.Stamina = ps2.GetInt32();

                        // Force redraw
                        this.InvokeIfRequired(() => this.Invalidate());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BattleForm] HandleTcpBroadcast error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle opponent state received via UDP
        /// ✅ ENHANCED: Now syncs Facing direction for proper animation
        /// </summary>
        private void HandleOpponentState(byte[] data)
        {
            try
            {
                if (data == null || data.Length < 17)
                    return;

                // Parse binary packet: [PacketType(1)] [RoomCode(6)] [PlayerNum(1)] [X(2)] [Y(2)] [Health(1)] [Stamina(1)] [Mana(1)] [FacingLen(1)] [Facing(var)] [ActionLen(1)] [Action(var)]

                int opponentPlayerNum = data[7];

                // Only process if it's opponent's data
                if (opponentPlayerNum == myPlayerNumber)
                    return;

                int x = data[8] | (data[9] << 8);
                int y = data[10] | (data[11] << 8);
                int health = data[12];
                int stamina = data[13];
                int mana = data[14];
                int facingLen = data[15];

                string facing = "right";
                if (data.Length >= 16 + facingLen && facingLen > 0)
                {
                    facing = System.Text.Encoding.UTF8.GetString(data, 16, facingLen);
                }

                int actionLen = data[16 + facingLen];
                string action = "stand";
                if (data.Length >= 17 + facingLen + actionLen && actionLen > 0)
                {
                    action = System.Text.Encoding.UTF8.GetString(data, 17 + facingLen, actionLen);
                }

                // Update opponent's state
                this.InvokeIfRequired(() =>
                {
                    PlayerState opponentState = opponentPlayerNum == 1 ? player1State : player2State;

                    opponentState.X = x;
                    opponentState.Y = y;
                    opponentState.Health = health;
                    opponentState.Stamina = stamina;
                    opponentState.Mana = mana;
                    opponentState.CurrentAnimation = action;
                    opponentState.Facing = facing; // ✅ ADD: Sync facing direction

                    // Force redraw
                    this.Invalidate();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HandleOpponentState error: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ NEW: Handle combat events (attack, damage, parry)
        /// </summary>
        private void HandleCombatEvent(string eventType, Dictionary<string, object> eventData)
        {
            try
            {
                this.InvokeIfRequired(() =>
                {
                    switch (eventType)
                    {
                        case "attack":
                            HandleAttackEvent(eventData);
                            break;

                        case "damage":
                            HandleDamageEvent(eventData);
                            break;

                        case "parry":
                            HandleParryEvent(eventData);
                            break;

                        default:
                            Console.WriteLine($"⚠️ Unknown combat event: {eventType}");
                            break;
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HandleCombatEvent error: {ex.Message}");
            }
        }

        private void HandleAttackEvent(Dictionary<string, object> data)
        {
            try
            {
                int attacker = Convert.ToInt32(data["attacker"]);
                string attackType = data["attackType"].ToString();

                Console.WriteLine($"[UDP] Player {attacker} attacked with {attackType}");

                // Trigger attack animation on opponent's screen
                PlayerState attackerState = attacker == 1 ? player1State : player2State;
                attackerState.CurrentAnimation = attackType;
                attackerState.IsAttacking = true;

                // Reset after animation duration
                var timer = new System.Windows.Forms.Timer { Interval = 300 };
                timer.Tick += (s, e) => {
                    timer.Stop();
                    attackerState.IsAttacking = false;
                    attackerState.CurrentAnimation = "stand";
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HandleAttackEvent error: {ex.Message}");
            }
        }

        private void HandleDamageEvent(Dictionary<string, object> data)
        {
            try
            {
                int target = Convert.ToInt32(data["target"]);
                int damage = Convert.ToInt32(data["damage"]);
                bool knockback = data.ContainsKey("knockback") && Convert.ToBoolean(data["knockback"]);

                Console.WriteLine($"[UDP] Player {target} took {damage} damage");

                // If attacker snapshot present, update attacker state so remote sees the action
                try
                {
                    if (data.ContainsKey("attacker"))
                    {
                        int attacker = Convert.ToInt32(data["attacker"]);
                        var attackerState = attacker == 1 ? player1State : player2State;

                        if (data.ContainsKey("attackerX")) attackerState.X = Convert.ToInt32(data["attackerX"]);
                        if (data.ContainsKey("attackerY")) attackerState.Y = Convert.ToInt32(data["attackerY"]);
                        if (data.ContainsKey("attackerFacing")) attackerState.Facing = data["attackerFacing"]?.ToString() ?? attackerState.Facing;
                        if (data.ContainsKey("attackerAction"))
                        {
                            var act = data["attackerAction"]?.ToString() ?? "stand";
                            attackerState.CurrentAnimation = act;
                            attackerState.IsAttacking = true;
                            // Reset attacking flag shortly after to simulate animation end
                            var t = new System.Windows.Forms.Timer { Interval = 300 };
                            t.Tick += (s, e) => { try { t.Stop(); t.Dispose(); } catch { } attackerState.IsAttacking = false; attackerState.CurrentAnimation = "stand"; };
                            t.Start();
                        }
                    }
                }
                catch { }

                // Apply damage to target without re-broadcasting (this event came from network)
                try
                {
                    combatSystem.ApplyDamage(target, damage, knockback, false);

                    // Sync UI vars
                    player1Health = player1State.Health;
                    player2Health = player2State.Health;
                    player1Stunned = player1State.IsStunned;
                    player2Stunned = player2State.IsStunned;
                    player1CurrentAnimation = player1State.CurrentAnimation;
                    player2CurrentAnimation = player2State.CurrentAnimation;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Apply remote damage error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HandleDamageEvent error: {ex.Message}");
            }
        }

        private void HandleParryEvent(Dictionary<string, object> data)
        {
            try
            {
                int player = Convert.ToInt32(data["player"]);

                Console.WriteLine($"[UDP] Player {player} parried!");

                ShowHitEffect("PARRY!", Color.Cyan);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HandleParryEvent error: {ex.Message}");
            }
        }

        private void InvokeIfRequired(Action action)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                if (this.InvokeRequired) this.Invoke(action);
                else action();
            }
        }
    }
}