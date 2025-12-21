using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using DoAn_NT106.Client.BattleSystems;
using DoAn_NT106.Client.Class;

namespace DoAn_NT106.Client
{
    public partial class BattleForm : Form
    {
        // Guard to prevent duplicate match end handling (forfeit/server race)
        private bool _matchEnded = false;
        private string username;
        private string token;
        private string opponent;
        private string roomCode = "000000";

        //   Public properties for room code and player info
        public string RoomCode => roomCode;
        public int MyPlayerNumber => myPlayerNumber;

        //   UDP Game Client
        private UDPGameClient udpClient;
        //   TCP GameClient for reliable broadcast of damage events
        private GameClient tcpGameClient;
        private bool isOnlineMode = false; // Track nếu đang chơi online
        private int myPlayerNumber = 0; // 1 or 2 assigned by server
        private bool isCreator = false; //   Track if this is Player 1

        // =====  NEW SYSTEMS (ADDED) =====
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
        private int backgroundWidth = 2400; //  TĂNG từ 2000 → 2400 để cover camera khi soft-overshoot
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
        //   Theo dõi hit nào đã xử lý (cho đòn đánh nhiều lần như Warrior attack1)
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

        //   Dash effect GIF cho Bringer of Death và Goatman
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
        private bool isPaused = false;

        // Kích thước nhân vật (dynamic)
        private int PLAYER_WIDTH = 80;
        private int PLAYER_HEIGHT = 120;
        private float characterHeightRatio = 0.30f; // relative to ClientSize.Height
        private float globalCharacterScale = 1.5f;

        // Hitbox configuration - nhỏ hơn và hướng theo facing
        private int HITBOX_WIDTH_RATIO = 2; // Hitbox = PLAYER_WIDTH / 2
        private int HITBOX_HEIGHT_RATIO = 2; // Hitbox = PLAYER_HEIGHT / 2
                                             //   Class cấu hình vùng tấn công

        public class HitEffectInstance
        {
            public int X { get; set; }
            public int Y { get; set; }
            public System.Windows.Forms.Timer Timer { get; set; }
            public Image EffectImage { get; set; }
        }

        // Listen to PersistentTcpClient broadcasts (fallback) to catch GAME_END
        private void Persistent_OnBroadcast(string action, System.Text.Json.JsonElement data)
        {
            try
            {
                if (string.Equals(action, "GAME_ENDED", StringComparison.OrdinalIgnoreCase))
                {
                    string roomCodeProp = data.TryGetProperty("roomCode", out var src) ? src.GetString() : null;
                    if (!string.IsNullOrEmpty(roomCodeProp) && !string.Equals(roomCodeProp, roomCode, StringComparison.OrdinalIgnoreCase)) return;

                    var winner = data.TryGetProperty("winner", out var w) ? w.GetString() : null;
                    var reason = data.TryGetProperty("reason", out var r) ? r.GetString() : null;

                    // GAME_ENDED có kèm XP data không (forfeit case)
                    bool hasXpData = data.TryGetProperty("hasXpData", out var hxd) && hxd.GetBoolean();

                    Console.WriteLine($"[BattleForm] 📥 GAME_ENDED: winner={winner}, reason={reason}, hasXpData={hasXpData}");

                    this.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            // NẾU CÓ XP DATA (forfeit case) - Hiển thị TinhXP trực tiếp
                            if (hasXpData && data.TryGetProperty("xpData", out var xpDataElement))
                            {
                                Console.WriteLine($"[BattleForm] 🎯 Forfeit with XP data - showing TinhXP directly");

                                // Parse XP data
                                string xpUsername = xpDataElement.TryGetProperty("username", out var xun) ? xun.GetString() : null;
                                bool isWinner = xpDataElement.TryGetProperty("isWinner", out var xiw) && xiw.GetBoolean();
                                int gainedXp = xpDataElement.TryGetProperty("gainedXp", out var xgx) ? xgx.GetInt32() : 0;
                                int oldXp = xpDataElement.TryGetProperty("oldXp", out var xox) ? xox.GetInt32() : 0;
                                int newXp = xpDataElement.TryGetProperty("newXp", out var xnx) ? xnx.GetInt32() : 0;
                                int oldLevel = xpDataElement.TryGetProperty("oldLevel", out var xol) ? xol.GetInt32() : 1;
                                int newLevel = xpDataElement.TryGetProperty("newLevel", out var xnl) ? xnl.GetInt32() : 1;
                                int totalXp = xpDataElement.TryGetProperty("totalXp", out var xtx) ? xtx.GetInt32() : 1000;
                                int matchDuration = xpDataElement.TryGetProperty("matchDuration", out var xmd) ? xmd.GetInt32() : 0;

                                // Stop timers và set match ended
                                _matchEnded = true;
                                _roundInProgress = false;
                                try { _roundTimer?.Stop(); } catch { }
                                try { gameTimer?.Stop(); } catch { }
                                try { walkAnimationTimer?.Stop(); } catch { }

                                // Show MatchResultForm trước
                                try
                                {
                                    string loserName = string.Equals(winner, username, StringComparison.OrdinalIgnoreCase)
                                        ? opponent : username;

                                    var resultForm = new MatchResultForm();
                                    resultForm.SetMatchResult(winner, loserName, 2, 0, MatchEndReason.Forfeit,
                                        player1State?.PlayerName ?? username,
                                        player2State?.PlayerName ?? opponent);
                                    resultForm.ShowDialog();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[BattleForm] Error showing MatchResultForm: {ex.Message}");
                                }

                                // Show TinhXP với XP từ server
                                try
                                {
                                    var result = new DoAn_NT106.Client.MatchResult
                                    {
                                        PlayerUsername = username,
                                        OpponentUsername = opponent,
                                        PlayerIsWinner = isWinner,
                                        MatchTime = TimeSpan.FromSeconds(matchDuration),
                                        PlayerWins = isWinner ? 2 : 0,
                                        OpponentWins = isWinner ? 0 : 2,
                                        RoomCode = roomCode,
                                        Token = token,
                                        Xp = gainedXp
                                    };

                                    using (var xpForm = new DoAn_NT106.Client.TinhXP(
                                        result, gainedXp, oldXp, newXp, oldLevel, newLevel, totalXp))
                                    {
                                        xpForm.StartPosition = FormStartPosition.CenterScreen;
                                        xpForm.ShowDialog(this);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[BattleForm] Error showing TinhXP: {ex.Message}");
                                }

                                // Play music và close
                                try { SoundManager.PlayMusic(BackgroundMusic.ThemeMusic, loop: true); } catch { }
                                try { this.Close(); } catch { }
                            }
                            else
                            {
                                //  Gọi EndMatch bình thường (sẽ gửi MATCH_RESULT)
                                EndMatch(winner);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[BattleForm] Persistent_OnBroadcast EndMatch error: {ex.Message}");
                        }
                    }));
                    return;
                }
                // If server broadcasts PLAYER_LEFT or LOBBY_PLAYER_LEFT, treat as opponent forfeit
                if (string.Equals(action, "PLAYER_LEFT", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(action, "LOBBY_PLAYER_LEFT", StringComparison.OrdinalIgnoreCase))
                {
                    string leftUser = data.TryGetProperty("username", out var lu) ? lu.GetString() : null;
                    if (!string.IsNullOrEmpty(leftUser) && isOnlineMode && string.Equals(leftUser, opponent, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"[BattleForm] Detected {action} for opponent {leftUser} - treating as forfeit");
                        this.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                // Immediately end match and declare local player the winner
                                EndMatch(this.username);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[BattleForm] Error ending match on {action}: {ex.Message}");
                            }
                        }));
                        return;
                    }
                }

                if (string.Equals(action, "XP_RESULT", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string xpRoomCode = data.TryGetProperty("roomCode", out var rc) ? rc.GetString() : null;
                        if (!string.IsNullOrEmpty(xpRoomCode) &&
                            !string.Equals(xpRoomCode, roomCode, StringComparison.OrdinalIgnoreCase))
                        {
                            return; // Không phải room của mình
                        }

                        string xpUsername = data.TryGetProperty("username", out var un) ? un.GetString() : null;

                        // Chỉ xử lý nếu là XP của chính mình
                        if (!string.Equals(xpUsername, username, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"[BattleForm] XP_RESULT for {xpUsername}, not me ({username}) - ignoring");
                            return;
                        }

                        Console.WriteLine($"[BattleForm] 📥 Received XP_RESULT for {xpUsername}");

                        // Parse XP data
                        bool isWinner = data.TryGetProperty("isWinner", out var iw) && iw.GetBoolean();
                        int gainedXp = data.TryGetProperty("gainedXp", out var gx) ? gx.GetInt32() : 0;
                        int oldXp = data.TryGetProperty("oldXp", out var ox) ? ox.GetInt32() : 0;
                        int newXp = data.TryGetProperty("newXp", out var nx) ? nx.GetInt32() : 0;
                        int oldLevel = data.TryGetProperty("oldLevel", out var ol) ? ol.GetInt32() : 1;
                        int newLevel = data.TryGetProperty("newLevel", out var nl) ? nl.GetInt32() : 1;
                        int totalXp = data.TryGetProperty("totalXp", out var tx) ? tx.GetInt32() : 1000;
                        int matchDuration = data.TryGetProperty("matchDuration", out var mdur) ? mdur.GetInt32() : 0;

                        this.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                // Tạo MatchResult với XP từ server
                                var result = new DoAn_NT106.Client.MatchResult
                                {
                                    PlayerUsername = username,
                                    OpponentUsername = opponent,
                                    PlayerIsWinner = isWinner,
                                    MatchTime = TimeSpan.FromSeconds(matchDuration),
                                    PlayerWins = _player1Wins,
                                    OpponentWins = _player2Wins,
                                    RoomCode = roomCode,
                                    Token = token,
                                    // XP từ server
                                    Xp = gainedXp
                                };

                                // Mở form TinhXP với XP data từ server
                                using (var xpForm = new DoAn_NT106.Client.TinhXP(result, gainedXp, oldXp, newXp, oldLevel, newLevel, totalXp))
                                {
                                    xpForm.StartPosition = FormStartPosition.CenterScreen;
                                    xpForm.ShowDialog(this);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[BattleForm] Error showing TinhXP: {ex.Message}");
                            }
                        }));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BattleForm] XP_RESULT parse error: {ex.Message}");
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BattleForm] Persistent_OnBroadcast error: {ex.Message}");
            }
        }

        // Simple camera used in offline mode: center midpoint of both players
        private void UpdateCameraSimple()
        {
            if (player1State == null || player2State == null) return;
            int viewWidth = Math.Max(1, this.ClientSize.Width);
            int worldWidth = backgroundWidth;

            var p1Hb = GetPlayerHitbox(player1State);
            var p2Hb = GetPlayerHitbox(player2State);
            int leftMost = Math.Min(p1Hb.Left, p2Hb.Left);
            int rightMost = Math.Max(p1Hb.Right, p2Hb.Right);
            int centerX = (leftMost + rightMost) / 2;

            int desired = centerX - viewWidth / 2;
            int maxViewport = Math.Max(0, worldWidth - viewWidth);
            desired = Math.Max(0, Math.Min(maxViewport, desired));

            float delta = desired - viewportX;
            float smoothing = 0.15f;
            viewportX += (int)(delta * smoothing);
            viewportX = Math.Max(0, Math.Min(maxViewport, viewportX));
        }

        private void EnsurePlayersInWorld()
        {
            if (player1State == null || player2State == null) return;

            try
            {
                // Single source of truth: use PhysicsSystem boundaries when available
                if (physicsSystem != null)
                {
                    // Only enforce boundary/wrap for the local player.
                    // Remote (opponent) positions arrive via UDP and are
                    // already clamped on their side; re-clamping here can
                    // create off-by-one blocking behavior. We'll still
                    // optionally log if remote goes out of expected bounds.

                    var local = myPlayerNumber == 1 ? player1State : player2State;
                    var remote = local == player1State ? player2State : player1State;
                    int prevLocal = local == player1State ? player1X : player2X;
                    int wrapThreshold = Math.Max(100, backgroundWidth / 2);

                    var localBounds = physicsSystem.GetBoundaryFromHurtboxPublic(local);

                    // Handle wrap/teleport for local only
                    if (Math.Abs(local.X - prevLocal) > wrapThreshold)
                    {
                        if (local.X > prevLocal)
                        {
                            Console.WriteLine($"[EnsurePlayersInWorld] Local wrap detected (rightward). Snapping to max {localBounds.maxX}");
                            local.X = localBounds.maxX;
                        }
                        else
                        {
                            Console.WriteLine($"[EnsurePlayersInWorld] Local wrap detected (leftward). Snapping to min {localBounds.minX}");
                            local.X = localBounds.minX;
                        }
                        local.VelocityX = 0;
                    }
                    else
                    {
                        int clamped = Math.Max(localBounds.minX, Math.Min(localBounds.maxX, local.X));
                        if (clamped != local.X)
                        {
                            Console.WriteLine($"[EnsurePlayersInWorld] Local clamped X: {local.X} -> {clamped} using bounds [{localBounds.minX},{localBounds.maxX}]");
                            local.X = clamped;
                            local.VelocityX = 0;
                        }
                    }

                    // Light sanity-check logging for remote (do NOT modify remote.X)
                    try
                    {
                        var remoteBounds = physicsSystem.GetBoundaryFromHurtboxPublic(remote);
                        if (remote.X < remoteBounds.minX || remote.X > remoteBounds.maxX)
                        {
                            Console.WriteLine($"[EnsurePlayersInWorld] WARNING: Remote X out of bounds: remote.X={remote.X}, bounds=[{remoteBounds.minX},{remoteBounds.maxX}]");
                        }
                    }
                    catch { }
                }
                else
                {
                    // Fallback: clamp to world extents and detect extreme jumps
                    int minX = 0;
                    int maxX = Math.Max(0, backgroundWidth - PLAYER_WIDTH);
                    int prev1 = player1X;
                    int prev2 = player2X;
                    int wrapThreshold = Math.Max(100, backgroundWidth / 2);

                    if (Math.Abs(player1State.X - prev1) > wrapThreshold)
                    {
                        player1State.X = player1State.X > prev1 ? maxX : minX;
                        player1State.VelocityX = 0;
                        Console.WriteLine($"[EnsurePlayersInWorld] P1 fallback wrap snap to {player1State.X}");
                    }
                    else
                    {
                        int c1 = Math.Max(minX, Math.Min(maxX, player1State.X));
                        if (c1 != player1State.X) { Console.WriteLine($"[EnsurePlayersInWorld] P1 fallback clamp {player1State.X} -> {c1}"); player1State.X = c1; player1State.VelocityX = 0; }
                    }

                    if (Math.Abs(player2State.X - prev2) > wrapThreshold)
                    {
                        player2State.X = player2State.X > prev2 ? maxX : minX;
                        player2State.VelocityX = 0;
                        Console.WriteLine($"[EnsurePlayersInWorld] P2 fallback wrap snap to {player2State.X}");
                    }
                    else
                    {
                        int c2 = Math.Max(minX, Math.Min(maxX, player2State.X));
                        if (c2 != player2State.X) { Console.WriteLine($"[EnsurePlayersInWorld] P2 fallback clamp {player2State.X} -> {c2}"); player2State.X = c2; player2State.VelocityX = 0; }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EnsurePlayersInWorld] ERROR: {ex.Message}");
            }
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
        // Cấu hình Attack Hitbox cho từng loại tấn công của từng nhân vật
        private Dictionary<string, Dictionary<string, AttackHitboxConfig>> characterAttackConfigs = new Dictionary<string, Dictionary<string, AttackHitboxConfig>>
        {
            ["girlknight"] = new Dictionary<string, AttackHitboxConfig>
            {
                ["punch"] = new AttackHitboxConfig
                {
                    WidthPercent = 0.6f,      //  Tăng từ 0.5f lên 0.6f
                    HeightPercent = 0.5f,     //  Tăng từ 0.4f lên 0.5f
                    RangePercent = 0.47f,     //  Tăng từ 0.35f lên 0.55f (tầm xa hơn)
                    OffsetYPercent = 0.30f    //  Giảm từ 0.35f xuống 0.30f (cao hơn)
                },
                ["kick"] = new AttackHitboxConfig
                {
                    WidthPercent = 0.7f,      //  Tăng từ 0.6f lên 0. 7f
                    HeightPercent = 0.45f,    //  Tăng từ 0.35f lên 0.45f
                    RangePercent = 0.7f,      //  Tăng từ 0.5f lên 0. 7f
                    OffsetYPercent = 0.50f    //  Giảm từ 0. 55f xuống 0.50f
                }, 
                ["skill"] = new AttackHitboxConfig
                {
                    WidthPercent = 1f,      //  Tăng từ 1.2f lên 1. 5f
                    HeightPercent = 1f,     //  Tăng từ 0.8f lên 1.0f
                    RangePercent = 0.5f,      //  Tăng từ 0.8f lên 1.2f (skill vùng gần)
                    OffsetYPercent = 0.10f    //  Giảm từ 0.15f xuống 0.10f
                }
            },
            ["bringerofdeath"] = new Dictionary<string, AttackHitboxConfig>
            {
                ["punch"] = new AttackHitboxConfig
                {
                    WidthPercent = 0.6f,
                    HeightPercent = 0.4f,
                    RangePercent = 0.33f,      //  GIẢM từ 0. 6f xuống 0.4f
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
                    RangePercent = 0.5f,      //  GIẢM từ 2.6f xuống 1.8f (spell xa hơn)
                    OffsetYPercent = 0.10f
                }
            },
            ["goatman"] = new Dictionary<string, AttackHitboxConfig>
            {
                ["punch"] = new AttackHitboxConfig
                {
                    WidthPercent = 0.7f,
                    HeightPercent = 0.5f,
                    RangePercent = 0.7f,      //  TĂNG từ 0. 4f lên 0.8f
                    OffsetYPercent = 0.30f    //  GIẢM từ 0.35f xuống 0.30f (cao hơn)
                },
                ["kick"] = new AttackHitboxConfig
                {
                    WidthPercent = 0.8f,
                    HeightPercent = 0.5f,     //  TĂNG từ 0.4f lên 0.5f
                    RangePercent = 0.7f,      //  TĂNG từ 0.6f lên 0. 9f
                    OffsetYPercent = 0.40f    //  GIẢM từ 0.45f xuống 0.40f
                },
                ["skill"] = new AttackHitboxConfig
                {
                    WidthPercent = 1.2f,      //  TĂNG từ 1.0f lên 1. 2f
                    HeightPercent = 0.8f,     //  TĂNG từ 0.7f lên 0.8f
                    RangePercent = 1f,      //  TĂNG từ 1.0f lên 1.4f
                    OffsetYPercent = 0.15f    //  GIẢM từ 0. 18f xuống 0.15f
                }
            },
            ["warrior"] = new Dictionary<string, AttackHitboxConfig>
            {
                ["punch"] = new AttackHitboxConfig
                {
                    WidthPercent = 0.8f,      //  TĂNG từ 0.7f lên 0.8f
                    HeightPercent = 0.5f,     //  TĂNG từ 0.4f lên 0.5f
                    RangePercent = 0.5f,      //  TĂNG từ 0.35f lên 0.5f
                    OffsetYPercent = 0.35f
                },
                ["kick"] = new AttackHitboxConfig
                {
                    WidthPercent = 0.7f,
                    HeightPercent = 0.35f,
                    RangePercent = 0.5f,      //  GIẢM từ 0. 7f xuống 0.5f
                    OffsetYPercent = 0.50f
                },
                ["skill"] = new AttackHitboxConfig
                {
                    WidthPercent = 1.2f,      //  Giảm từ 1.8f
                    HeightPercent = 0.6f,
                    RangePercent = 2.0f,      //  GIẢM từ 2.8f xuống 2.0f (projectile nên xa hơn)
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

        private string player1Name;
        private string player2Name;

       
        // Battle Statistics
        private int player1ParryCount = 0;
        private int player2ParryCount = 0;
        private int player1SkillCount = 0;
        private int player2SkillCount = 0;
        private int player1ComboCount = 0;
        private int player2ComboCount = 0;

        // Round tracking for consecutive wins
        private int player1ConsecutiveWins = 0;
        private int player2ConsecutiveWins = 0;
        private int currentRound = 1;
        private int player1RoundsWon = 0;
        private int player2RoundsWon = 0;

        public BattleForm(string player1NameParam, string token, string player2NameParam, string player1Character, string player2Character, string selectedMap = "battleground1", string roomCode = "000000", int myPlayerNumber = 0, bool isCreator = false)
        {
            InitializeComponent();

            this.player1Name = player1NameParam;     //  SỬAR: Player 1 name từ server
            this.player2Name = player2NameParam;     //  SỬAR: Player 2 name từ server
            this.username = myPlayerNumber == 1 ? player1NameParam : player2NameParam;  //  Local player name
            this.opponent = myPlayerNumber == 1 ? player2NameParam : player1NameParam;  //  Opponent name
            this.token = token;
            this.roomCode = roomCode;
            this.player1CharacterType = player1Character;
            this.player2CharacterType = player2Character;
            this.myPlayerNumber = myPlayerNumber; //  set role from server
            this.isCreator = isCreator; //   Track if I'm the creator

            //   Kiểm tra online mode
            isOnlineMode = !string.IsNullOrEmpty(roomCode) && roomCode != "000000";

            //  If online mode, prepare UDP client (using AppConfig IP)
            if (isOnlineMode)
            {
                try
                {
                    //  USE AppConfig for IP/Port
                    udpClient = new UDPGameClient(AppConfig.SERVER_IP, AppConfig.UDP_PORT, roomCode, username);
                    udpClient.SetPlayerNumber(myPlayerNumber);
                    udpClient.OnLog += msg => Console.WriteLine(msg);
                    udpClient.OnOpponentState += OnOpponentUdpState;
                    udpClient.Connect();
                    // Also prepare TCP GameClient for reliable messages
                    try
                    {
                        tcpGameClient = new DoAn_NT106.Client.Class.GameClient();
                        var _ = tcpGameClient.ConnectAsync();
                        tcpGameClient.OnError += (err) => Console.WriteLine($"[BattleForm][TCP] {err}");

                        // Server will broadcast damage events and game end. When we receive one and
                        // the target is this client, apply damage locally. Also handle GAME_ENDED so
                        // opponent gets declared winner on forfeit.
                        tcpGameClient.OnDamageEvent += (d) =>
                        {
                            try
                            {
                                if (d.TargetPlayerNum == myPlayerNumber)
                                {
                                    Console.WriteLine($"[BattleForm] Received DAMAGE_EVENT from server target={d.TargetPlayerNum} dmg={d.Damage} parried={d.IsParried}");
                                    this.BeginInvoke(new Action(() =>
                                    {
                                        combatSystem.IsNetworked = true;
                                        combatSystem.LocalPlayerNumber = myPlayerNumber;
                                        // When applying from server, ensure CombatSystem will apply locally
                                        combatSystem.ApplyDamage(d.TargetPlayerNum, d.Damage);
                                    }));
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[BattleForm] OnDamageEvent handler error: {ex.Message}");
                            }
                        };
                        // Handle game end broadcasts from server (for forfeits and match end)
                        tcpGameClient.OnGameEnded += (endData) =>
                        {
                            try
                            {
                                if (endData == null) return;
                                if (!string.Equals(endData.RoomCode, roomCode, StringComparison.OrdinalIgnoreCase)) return;

                                Console.WriteLine($"[BattleForm][TCP] Received GAME_END: winner={endData.Winner}, reason={endData.Reason}");

                                // Ensure UI update runs on UI thread
                                this.BeginInvoke(new Action(() =>
                                {
                                    try
                                    {
                                        EndMatch(endData.Winner);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"[BattleForm] OnGameEnded handler error: {ex.Message}");
                                    }
                                }));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[BattleForm] GameEnded event processing error: {ex.Message}");
                            }
                        };
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BattleForm] TCP GameClient init error: {ex.Message}");
                    }
                    // Also subscribe PersistentTcpClient broadcast as a fallback to ensure GAME_END is handled
                    try
                    {
                        PersistentTcpClient.Instance.OnBroadcast += Persistent_OnBroadcast;
                    }
                    catch { }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BattleForm] UDP connect error: {ex.Message}");
                    // Nếu UDP lỗi thì vẫn cho chơi local, chỉ mất đồng bộ LAN
                }
            }

            //  Normalize selectedMap and set map background
            selectedMap = (selectedMap ?? "battleground1").Trim().ToLowerInvariant();
            int mapIndex = -1;
            if (!string.IsNullOrEmpty(selectedMap))
            {
                // Extract number from "battleground4" → 4
                string mapNumberStr = selectedMap.Replace("battleground", "", StringComparison.OrdinalIgnoreCase);
                if (int.TryParse(mapNumberStr, out int mapNum) && mapNum >= 1 && mapNum <= 4)
                {
                    mapIndex = mapNum - 1;  // 1 → index 0, 4 → index 3
                }
            }
            currentBackground = (mapIndex >= 0 && mapIndex < 4) ? mapIndex : 0;
            
            Console.WriteLine($"[BattleForm] selectedMap='{selectedMap}' → mapIndex={currentBackground}");

            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            player1Y = 0;
            player2Y = 0;
            this.Load += BattleForm_Load;

            //  Initialize Sound Manager
            SoundManager.Initialize();

            //  Initialize once here
            SetupGame();
            SetupEventHandlers();

            this.Text = $"⚔️ Street Fighter - {player1Name} vs {player2Name}";
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

            //  ENABLE: Play battle music when form loads
            SoundManager.PlayMusic(BackgroundMusic.BattleMusic, loop: true);
            Console.WriteLine("🎵 Battle music started");

            // ❌ Avoid re-running full setup; only force redraw
            this.Invalidate();
        }

        /// <summary>
        ///   Method for Player 2 to join existing BattleForm
        /// </summary>
        public void JoinAsPlayer2(string player2Name, string token, string player1Character, string player2Character, int playerNumber)
        {
            Console.WriteLine($"[BattleForm] JoinAsPlayer2 called: {player2Name} as P{playerNumber}");
            // player1Name already set when Player 1 created the form
            this.player2Name = player2Name;  //  Set Player 2 name
            this.username = player2Name;     //  Local player is now Player 2
            this.token = token;
            this.player1CharacterType = player1Character;
            this.player2CharacterType = player2Character;
            this.myPlayerNumber = playerNumber;

            //  Reinitialize animations and game systems if needed
            if (player1AnimationManager == null || player2AnimationManager == null)
            {
                player1AnimationManager = new CharacterAnimationManager(player1CharacterType, OnFrameChanged);
                player1AnimationManager.LoadAnimations();

                player2AnimationManager = new CharacterAnimationManager(player2CharacterType, OnFrameChanged);
                player2AnimationManager.LoadAnimations();
            }

            //  Bring form to front
            this.BringToFront();
            this.Show();

            Console.WriteLine($"[BattleForm] Player 2 joined successfully");
        }

        // Allow updating selected map at runtime (used when Player2 joins an existing BattleForm)
        public void UpdateSelectedMap(string selectedMap)
        {
            try
            {
                if (string.IsNullOrEmpty(selectedMap)) return;

                int mapIndex = -1;
                string mapNumberStr = selectedMap.Replace("battleground", "");
                if (int.TryParse(mapNumberStr, out int mapNum) && mapNum >= 1 && mapNum <= 4)
                {
                    mapIndex = mapNum - 1;
                }

                currentBackground = (mapIndex >= 0 && mapIndex < 4) ? mapIndex : 0;
                string backgroundName = $"battleground{currentBackground + 1}";

                // Ensure call on UI thread
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() => SetBackground(backgroundName)));
                }
                else
                {
                    SetBackground(backgroundName);
                }

                Console.WriteLine($"[BattleForm] UpdateSelectedMap applied: {backgroundName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BattleForm] UpdateSelectedMap error: {ex.Message}");
            }
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

                // =====  INITIALIZE NEW SYSTEMS =====
                // 1. Initialize PlayerState instances - SỬA Y position
                player1State = new PlayerState(player1Name, player1CharacterType, 1)
                {
                    X = 150, //  SỬA: từ 300 → 150
                    Y = groundLevel - PLAYER_HEIGHT,
                    Facing = "right",
                    CurrentAnimation = "stand"
                };
                
                //  SỬA: Set HP theo character type
                SetPlayerHealth(player1State, player1CharacterType);

                player2State = new PlayerState(player2Name, player2CharacterType, 2)
                {
                    X = 700, //  SỬA: từ 600 → 900
                    Y = groundLevel - PLAYER_HEIGHT,
                    Facing = "left",
                    CurrentAnimation = "stand"
                };
                
                //  SỬA: Set HP theo character type
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
                // Provide network callbacks to CombatSystem
                combatSystem.IsNetworked = isOnlineMode;
                combatSystem.LocalPlayerNumber = myPlayerNumber;
                combatSystem.SendDamageRequestCallback = (targetPlayer, damage, knockbackFlag, resultingHealth) =>
                {
                    try
                    {
                        // Send a UDP damage notification so opponent client can apply damage immediately
                        udpClient?.SendDamageNotification(targetPlayer, damage, resultingHealth);

                        // Also notify server via TCP GAME_DAMAGE for authoritative broadcast/persistence
                        var _ = tcpGameClient?.BroadcastDamageEvent(roomCode, username, targetPlayer, damage, false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BattleForm] SendDamageRequestCallback error: {ex.Message}");
                    }
                };
                // Subscribe to damage applied event to send immediate UDP damage packet
                combatSystem.DamageApplied += (targetPlayer, damage) =>
                {
                    try
                    {
                        // Apply local sync already done in CombatSystem. Just send immediate packet with current health and attack id (0)
                        var target = targetPlayer == 1 ? player1State : player2State;
                        // 1) Send immediate UDP state update so opponent sees HP change fast
                        udpClient?.SendImmediateHealthUpdate(target.Health, 0);

                        // 2) Also notify server over TCP so it can persist/broadcast authoritative damage event
                        try
                        {
                            tcpGameClient?.BroadcastDamageEvent(roomCode, username, targetPlayer, damage, false);
                        }
                        catch { }
                    }
                    catch { }
                };
                // =====================================

                //  THÊM DÒNG NÀY - SAU KHI ĐÃ CÓ physicsSystem!
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

            // =====  SETUP UI WITH NEW SYSTEMS =====
            // Add bars to form controls once
            if (resourceSystem != null)
            {
                if (!this.Controls.Contains(resourceSystem.HealthBar1)) this.Controls.Add(resourceSystem.HealthBar1);
                if (!this.Controls.Contains(resourceSystem.StaminaBar1)) this.Controls.Add(resourceSystem.StaminaBar1);
                if (!this.Controls.Contains(resourceSystem.ManaBar1)) this.Controls.Add(resourceSystem.ManaBar1);
                if (!this.Controls.Contains(resourceSystem.HealthBar2)) this.Controls.Add(resourceSystem.HealthBar2);
                if (!this.Controls.Contains(resourceSystem.StaminaBar2)) this.Controls.Add(resourceSystem.StaminaBar2);
                if (!this.Controls.Contains(resourceSystem.ManaBar2)) this.Controls.Add(resourceSystem.ManaBar2);
                
                //  Add portraits
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
                    Text = player1Name,  //  SỬAR: Sử dụng player1Name thay vì username
                    Location = new Point(20, startY + 3 * (barHeight + spacing) + 90),
                    Size = new Size(barWidth, 25),
                    ForeColor = Color.Cyan,
                    Font = new Font("Arial", 12, FontStyle.Bold),
                    BackColor = Color.Transparent
                };
                this.Controls.Add(lblPlayer1Name);
            }

            if (lblPlayer2Name == null)
            {
                lblPlayer2Name = new Label
                {
                    Text = player2Name,  //  SỬAR: Sử dụng player2Name thay vì opponent
                    Location = new Point(screenWidth - barWidth - 20, startY + 3 * (barHeight + spacing) + 90),
                    Size = new Size(barWidth, 25),
                    ForeColor = Color.Orange,
                    Font = new Font("Arial", 12, FontStyle.Bold),
                    TextAlign = ContentAlignment.TopRight,
                    BackColor = Color.Transparent
                };
                this.Controls.Add(lblPlayer2Name);
            }



            //   Load background map
            string backgroundName = $"battleground{currentBackground + 1}";
            SetBackground(backgroundName);
            Console.WriteLine($"[SetupGame] Background loaded: {backgroundName}");

            // Initialize round system
            InitializeRoundSystem();
        }

        // In BattleForm.cs - GameTimer_Tick() - VERSION CLEAN

        // ===== NETWORK INTERPOLATION =====
        /// <summary>
        /// Interpolates opponent position between UDP packets to smooth out network lag
        /// </summary>
        private void InterpolateOpponentPosition()
        {
            if (!isOnlineMode || opponentPositionQueue.Count < 2) return;
            
            PlayerState opponent = myPlayerNumber == 1 ? player2State : player1State;
            
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long targetTime = currentTime - 50; // Delay 50ms để interpolation
            
            // Tìm 2 positions gần nhất để interpolate
            (int x1, int y1, long t1) = opponentPositionQueue.Peek();
            (int x2, int y2, long t2) = (0, 0, 0);
            
            // Lấy position thứ 2
            var tempQueue = new Queue<(int x, int y, long timestamp)>(opponentPositionQueue);
            tempQueue.Dequeue(); // Bỏ position đầu
            if (tempQueue.Count > 0)
            {
                (x2, y2, t2) = tempQueue.Peek();
            }
            else
            {
                return; // Không đủ dữ liệu
            }
            
            // Interpolation linear
            if (t2 > t1 && targetTime >= t1 && targetTime <= t2)
            {
                float t = (float)(targetTime - t1) / (t2 - t1);
                t = Math.Max(0, Math.Min(1, t));
                
                int interpolatedX = (int)(x1 + (x2 - x1) * t);
                int interpolatedY = (int)(y1 + (y2 - y1) * t);
                
                opponent.X = interpolatedX;
                opponent.Y = interpolatedY;
            }
            
            // Xóa positions cũ (> 200ms)
            while (opponentPositionQueue.Count > 0 && 
                   currentTime - opponentPositionQueue.Peek().timestamp > 200)
            {
                opponentPositionQueue.Dequeue();
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // ===== MOVEMENT LOGIC =====
            player1State.IsWalking = false;
            player2State.IsWalking = false;

            //  ONLINE MODE: Only the local player can be moved by local input
            //  OFFLINE MODE: Player 1 uses A/D, Player 2 uses Left/Right
            
            if (isOnlineMode)
            {
                // ONLINE MODE: Only move the local player based on input
                if (myPlayerNumber == 1)
                {
                    if (player1State.CanMove)
                    {
                        if (aPressed) physicsSystem.MovePlayer(player1State, -1);
                        else if (dPressed) physicsSystem.MovePlayer(player1State, 1);
                        else if (!player1State.IsJumping && !player1State.IsParrying && !player1State.IsSkillActive)
                            physicsSystem.StopMovement(player1State);
                    }
                }
                else if (myPlayerNumber == 2)
                {
                    if (player2State.CanMove)
                    {
                        if (aPressed) physicsSystem.MovePlayer(player2State, -1);
                        else if (dPressed) physicsSystem.MovePlayer(player2State, 1);
                        else if (!player2State.IsJumping && !player2State.IsParrying && !player2State.IsSkillActive)
                            physicsSystem.StopMovement(player2State);
                    }
                }
            }
            else
            {
                // OFFLINE MODE: Both players can move with their own keys
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
            }

            //   Check if walk animation should be converted to stand (every 16ms)
            // Nếu đang ở animation walk nhưng vị trí không thay đổi → quay về stand
            physicsSystem.CheckWalkAnimation(player1State);
            physicsSystem.CheckWalkAnimation(player2State);

        // ===== JUMP PHYSICS =====
        physicsSystem.UpdateJump(player1State);
        physicsSystem.UpdateJump(player2State);

        // ===== NETWORK INTERPOLATION: Smooth remote player movement =====
        if (isOnlineMode)
        {
            InterpolateOpponentPosition();
            // Ensure players are clamped before camera computes viewport to avoid camera inducing wrap-like shifts
            EnsurePlayersInWorld();
        }

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

            // ===== ONLINE SYNC: gửi state local cho server qua UDP =====
            //  SỬA: Gửi position cập nhật ngay lập tức mỗi frame, không buffer
            if (isOnlineMode && udpClient != null && udpClient.IsConnected)
            {
                try
                {
                    // Chỉ gửi state của chính mình; đối thủ sẽ tự gửi state của họ
                    var me = myPlayerNumber == 1 ? player1State : player2State;

                    // IMPORTANT: Ensure we send WORLD POSITION over UDP (not screen coordinates).
                    // Heuristic: if X looks like a screen coordinate (within ClientSize.Width)
                    // and adding viewportX keeps it inside world bounds, convert it to world X.
                    // Send world coordinates directly. The local PlayerState.X is
                    // already a world position (not a screen coordinate), so avoid
                    // heuristic conversion which may double-add viewportX and cause
                    // incorrect positions to be transmitted.
                    int sendX = me.X;
                    int sendY = me.Y; // vertical camera locked to 0 in this game (world Y == screen Y)

                    // Clamp to valid world play range to avoid sending invalid positions
                    // Use PhysicsSystem boundary (hurtbox-based) when available so we
                    // send the same allowed range as local movement (may include
                    // negative sprite.X values). Fall back to 0..(backgroundWidth-PLAYER_WIDTH).
                    try
                    {
                        if (physicsSystem != null)
                        {
                            var b = physicsSystem.GetBoundaryFromHurtboxPublic(me);
                            sendX = Math.Max(b.minX, Math.Min(b.maxX, sendX));
                        }
                        else
                        {
                            sendX = Math.Max(0, Math.Min(Math.Max(0, backgroundWidth - PLAYER_WIDTH), sendX));
                        }
                    }
                    catch
                    {
                        sendX = Math.Max(0, Math.Min(Math.Max(0, backgroundWidth - PLAYER_WIDTH), sendX));
                    }
                    sendY = Math.Max(0, Math.Min(Math.Max(0, this.ClientSize.Height), sendY));

                    // DEBUG: Log sendX near edges to help diagnose boundary sync issues
                    try
                    {
                        if (physicsSystem != null)
                        {
                            var localBounds = physicsSystem.GetBoundaryFromHurtboxPublic(me);
                            if (sendX <= localBounds.minX + 8 || sendX >= localBounds.maxX - 8)
                            {
                                Console.WriteLine($"[UDP DEBUG] Sending pos near edge: sendX={sendX}, localBounds=[{localBounds.minX},{localBounds.maxX}], viewportX={viewportX}");
                            }
                        }
                    }
                    catch { }

                    udpClient.UpdateState(
                        sendX,
                        sendY,
                        me.Health,
                        me.Stamina,
                        me.Mana,
                        me.CurrentAnimation ?? "stand",
                        me.Facing ?? "right",           //  NEW: Facing
                        me.IsAttacking,                 //  NEW: IsAttacking
                        me.IsParrying,                  //  NEW: IsParrying
                        me.IsStunned,                   //   IsStunned
                        me.IsSkillActive,               //   IsSkillActive
                        me.IsCharging,                  //   IsCharging
                        me.IsDashing);                  //   IsDashing
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BattleForm] UDP UpdateState error: {ex.Message}");
                }
            }

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

            // ? NEW: Update opponent projectiles (ch? v?, không gây damage)
            projectileManager.UpdateOpponentProjectiles(
                (playerNum, x, y, _) =>
                {
                    var p = playerNum == 1 ? player1State : player2State;
                    // Use actual configured hurtbox
                    var hb = GetPlayerHitbox(p);
                    return new Rectangle(hb.X, hb.Y, hb.Width, hb.Height);
                }
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

            // ===== WORLD BOUNDARIES: Handled by PhysicsSystem.MovePlayer via hurtbox =====
            //  REMOVED: ApplyWorldBoundaries - hurtbox-based clamping in MovePlayer is sufficient

            // ===== UPDATE CAMERA =====
            if (isOnlineMode)
            {
                // Online: use network-aware camera
                UpdateCameraNetworkSync();
            }
            else
            {
                // Offline: use no-overshoot camera
                UpdateCameraNoOvershoot();
            }

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

        //   Xử lý state nhận từ UDP (đối thủ)
        //  NEW: Network interpolation fields for smooth remote player movement
        private Queue<(int x, int y, long timestamp)> opponentPositionQueue = new Queue<(int, int, long)>();
        private long lastOpponentUpdateTime = 0;
        private int lastRemoteX = 0;
        
        private void OnOpponentUdpState(byte[] data)
        {
            try
            {
                if (data == null || data.Length < 22) return;

                //  EXPANDED PACKET STRUCTURE:
                // [RoomCode(6)] [PlayerNum(1)] [X(2)] [Y(2)] [Health(1)] [Stamina(1)] [Mana(1)]
                // [Facing(1)] [IsAttacking(1)] [IsParrying(1)] [IsStunned(1)] [IsSkillActive(1)] [IsCharging(1)] [IsDashing(1)] [ActionLen(1)] [Action(var)]

                string room = System.Text.Encoding.UTF8.GetString(data, 0, 6).TrimEnd('\0');
                if (!string.Equals(room, roomCode, StringComparison.OrdinalIgnoreCase))
                    return; // khác trận

                int remotePlayerNum = data[6];
                // Đối thủ là playerNumber khác mình
                int opponentNum = myPlayerNumber == 1 ? 2 : 1;
                if (remotePlayerNum != opponentNum)
                    return;

                // Decode signed 16-bit little-endian values for X/Y (supports negative world X)
                short sx = (short)(data[7] | (data[8] << 8));
                short sy = (short)(data[9] | (data[10] << 8));
                int x = sx;
                int y = sy;
                int health = data[11];
                int stamina = data[12];
                int mana = data[13];

                //  NEW: Queue position for interpolation
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                lock (opponentPositionQueue)
                {
                    opponentPositionQueue.Enqueue((x, y, timestamp));
                    
                    // Limit queue size to prevent memory leak
                    if (opponentPositionQueue.Count > 10)
                    {
                        opponentPositionQueue.Dequeue();
                    }
                }
                lastOpponentUpdateTime = timestamp;

                //  EXPANDED: Parse all combat flags (bytes 14-20)
                string facing = data[14] == 'L' ? "left" : "right";
                bool isAttacking = data[15] != 0;
                bool isParrying = data[16] != 0;
                bool isStunned = data[17] != 0;           //  THÊM
                bool isSkillActive = data[18] != 0;       //  THÊM
                bool isCharging = data[19] != 0;          //  THÊM
                bool isDashing = data[20] != 0;           //  THÊM

                int actionLen = data[21];
                string action = "stand";
                if (actionLen > 0 && 22 + actionLen <= data.Length)
                {
                    action = System.Text.Encoding.UTF8.GetString(data, 22, actionLen);
                }

                // If action encodes a DAMAGE notification from opponent, parse and apply locally
                if (!string.IsNullOrEmpty(action) && action.StartsWith("DAMAGE:", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        // Format: DAMAGE:{targetPlayerNum}:{damage}:{resultingHealth}
                        var parts = action.Split(':');
                        if (parts.Length >= 4 && int.TryParse(parts[1], out int dmgTarget) && int.TryParse(parts[2], out int dmgAmount))
                        {
                            int dmgResulting = 0;
                            int.TryParse(parts[3], out dmgResulting);

                            // If the damage target is this client, apply damage locally via CombatSystem
                            if (dmgTarget == myPlayerNumber)
                            {
                                // If we are currently in round transition/countdown, ignore late UDP damage
                                if (!_roundInProgress)
                                {
                                    Console.WriteLine("[UDP] Ignored DAMAGE during round transition/countdown");
                                }
                                else
                                {
                                    Console.WriteLine($"[UDP] Received DAMAGE notification for me: dmg={dmgAmount} resultingHP={dmgResulting}");
                                    this.BeginInvoke(new Action(() =>
                                    {
                                        try
                                        {
                                            //  SỬA: Gọi ApplyDamage() từ CombatSystem để xử lý animation hurt, stun, hit effect
                                            // Điều này đảm bảo animation hurt được reset và hiệu ứng hit được hiển thị
                                            combatSystem.IsNetworked = true;
                                            combatSystem.LocalPlayerNumber = myPlayerNumber;
                                            

                                            var me = myPlayerNumber == 1 ? player1State : player2State;
                                            var currentHealth = me.Health;
                                            
                                            // ApplyDamage sẽ xử lý:
                                            // 1. Kiểm tra dash/parry
                                            // 2. Cập nhật HP (TakeDamage)
                                            // 3. Đặt IsStunned = true
                                            // 4. Reset animation sang "hurt"
                                            // 5. Hiển thị hit effect
                                            // 6. Kích hoạt stun timer
                                            
                                            int damage = currentHealth - dmgResulting;
                                            if (damage > 0)
                                            {
                                                // Chỉ gọi ApplyDamage nếu HP thực sự giảm
                                                combatSystem.ApplyDamage(myPlayerNumber, damage, true);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"[UDP] Error applying damage locally: {ex.Message}");
                                        }
                                    }));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[UDP] Parse DAMAGE action error: {ex.Message}");
                    }
                    // Continue processing to also update positional/state bytes below
                }

                // Cập nhật vào PlayerState đối thủ trên UI thread
                this.BeginInvoke(new Action(() =>
                {
                    var opp = opponentNum == 1 ? player1State : player2State;
                    var oppAnimMgr = opponentNum == 1 ? player1AnimationManager : player2AnimationManager;
                    var prevAnim = opponentNum == 1 ? _prevAnimPlayer1 : _prevAnimPlayer2;
                    
                    //  CRITICAL: ALWAYS UPDATE POSITION FIRST!
                    // Position update is independent from animation update
                    opp.X = x;
                    opp.Y = y;
                    
                    // NOTE: Do NOT re-apply local PhysicsSystem boundaries here.
                    // The remote client already clamps its own position before
                    // sending via UDP. Re-clamping on the receiver causes a
                    // "one-space" early block at the edges. Always trust the
                    // opponent's sent world position and only update state below.

                    // DEBUG: Log received position vs computed hurtbox/bounds for investigation
                    try
                    {
                        if (physicsSystem != null)
                        {
                            // opp.X already set to received x
                            var recvBounds = physicsSystem.GetBoundaryFromHurtboxPublic(opp);
                            var hb = GetPlayerHitbox(opp);
                            Console.WriteLine($"[UDP DEBUG] Received opponent pos X={opp.X} Y={opp.Y}; hurtboxLeft={hb.Left} hurtboxRight={hb.Right}; bounds=[{recvBounds.minX},{recvBounds.maxX}]");
                        }
                        else
                        {
                            Console.WriteLine($"[UDP DEBUG] Received opponent pos X={opp.X} Y={opp.Y} (no physics)");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[UDP DEBUG] Bound debug error: {ex.Message}");
                    }
                    
                    //  UPDATED: Only update health/stamina/mana if round is in progress
                    // During round countdown, ignore these updates to prevent premature HP reset
                    if (action == "ROUND_RESET")
                    {
                        // FORCE apply health/stamina/mana reset even during countdown
                        opp.Health = health;
                        opp.Stamina = stamina;
                        opp.Mana = mana;
                        Console.WriteLine($"[UDP] Applied ROUND_RESET from opponent P{opponentNum}: HP={health} ST={stamina} MA={mana}");
                    }
                    else if (_roundInProgress)
                    {
                        opp.Health = health;
                        opp.Stamina = stamina;
                        opp.Mana = mana;
                        
                        // DEBUG: Log health update from UDP
                        try
                        {
                            Console.WriteLine($"[UDP] Updated opponent P{opponentNum} health={health} during gameplay");
                        }
                        catch { }
                    }
                    else
                    {
                        // Round countdown: ignore health updates but still process other state
                        Console.WriteLine($"[UDP] ⏭️ Ignored health update from opponent (round countdown): health={health} (waiting for round start)");
                    }
                    
                    opp.Facing = facing;
                    opp.IsAttacking = isAttacking;
                    opp.IsParrying = isParrying;
                    opp.IsStunned = isStunned;
                    opp.IsSkillActive = isSkillActive;
                    opp.IsCharging = isCharging;
                    opp.IsDashing = isDashing;

                    //  ANIMATION UPDATE (only update when needed, separate from position)
                    // NẾU ĐÃ ATTACK: KHÔNG CẬP NHẬT - ĐỢI ANIMATION CHẠY XONG
                    if (opp.IsAttacking && (opp.CurrentAnimation == "punch" || opp.CurrentAnimation == "kick" || opp.CurrentAnimation == "fireball"))
                    {
                        Console.WriteLine($"[UDP] ⏭️ Skipped animation update during attack: {opp.CurrentAnimation} (stay in attack until done)");
                        // KHÔNG cập nhật animation - để nó chạy hết
                    }
                    else
                    {
                        bool isWalkOrJump = action == "walk" || action == "jump";
                        bool currentIsWalkOrJump = opp.CurrentAnimation == "walk" || opp.CurrentAnimation == "jump";
                        bool animationChanged = opp.CurrentAnimation != action;
                        
                        //  IGNORE: stand → stand (không cập nhật idle state từ opponent)
                        if (action == "stand" && opp.CurrentAnimation == "stand")
                        {
                            // Bỏ qua - không làm gì
                            Console.WriteLine($"[UDP] ⏭️ Ignored animation: stand → stand (but position ALWAYS updated)");
                        }
                        // Nếu animation thay đổi (và không phải stand → stand)
                        else if (animationChanged)
                        {
                            //  NEW: Phát hiện skill để spawn opponent projectile
                            if (action == "fireball")
                            {
                                Console.WriteLine($"[UDP] 🎯 Opponent {opponentNum} used skill (fireball)!");

                                try
                                {
                                    // Xác định character type của opponent
                                    string oppCharType = opponentNum == 1 ? player1CharacterType : player2CharacterType;
                                    
                                    // Chỉ Warrior và Bringer of Death có projectile/spell
                                    if (oppCharType == "warrior")
                                    {
                                        //  Warrior projectile: tính toán spawn từ hit frame (frame 3)
                                        // Frame timing: 7fps, mỗi frame = 1000/7 ≈ 143ms
                                        // Hit frame 3 = 3 * 143 = 429ms
                                        int delayBeforeSpawn = 429; // ms
                                        
                                        var spawnTimer = new System.Windows.Forms.Timer { Interval = delayBeforeSpawn };
                                        spawnTimer.Tick += (s, e) =>
                                        {
                                            spawnTimer.Stop();
                                            spawnTimer.Dispose();
                                            
                                            try
                                            {
                                                var oppHurtbox = GetPlayerHitbox(opp);
                                                
                                                // Tính hướng dựa trên facing, KHÔNG so sánh position
                                                int projDirection = opp.Facing == "right" ? 1 : -1;
                                                
                                                int projStartX, projStartY;
                                                if (projDirection > 0)
                                                {
                                                    // Bắn phải: từ bên phải hurtbox
                                                    projStartX = oppHurtbox.X + oppHurtbox.Width;
                                                }
                                                else
                                                {
                                                    // Bắn trái: từ bên trái hurtbox
                                                    projStartX = oppHurtbox.X - 160;
                                                }
                                                projStartY = oppHurtbox.Y + (oppHurtbox.Height / 2) - (160 / 2);
                                                
                                                Console.WriteLine($"[UDP] Spawning warrior projectile at X={projStartX}, Y={projStartY}, dir={projDirection}, facing={opp.Facing}");
                                                projectileManager.SpawnOpponentWarriorProjectile(projStartX, projStartY, projDirection, opponentNum);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"[UDP] Error spawning warrior projectile: {ex.Message}");
                                            }
                                        };
                                        spawnTimer.Start();
                                    }
                                    else if (oppCharType == "bringerofdeath")
                                    {
                                        //  Bringer of Death spell: spawn từ hit frame (frame 6)
                                        // Frame timing: 8fps, mỗi frame = 1000/8 = 125ms
                                        // Hit frame 6 = 6 * 125 = 750ms
                                        int delayBeforeSpawn = 750; // ms
                                        
                                        var spawnTimer = new System.Windows.Forms.Timer { Interval = delayBeforeSpawn };
                                        spawnTimer.Tick += (s, e) =>
                                        {
                                            spawnTimer.Stop();
                                            spawnTimer.Dispose();
                                            
                                            try
                                            {
                                                //  Spell xuất hiện tại vị trí của MÌNH (canh theo hitbox của mình)
                                                // Vì spell bringer là ổn định tại một vị trí, không di chuyển
                                                var meState = myPlayerNumber == 1 ? player1State : player2State;
                                                var meHurtbox = GetPlayerHitbox(meState);
                                                
                                                int centerX = meHurtbox.X + meHurtbox.Width / 2;
                                                int centerY = meHurtbox.Y + meHurtbox.Height / 2;
                                                

                                                // Apply same spell offsets as local
                                                int projStartX = centerX - 10 + 20 - 50; // same calculation as SpawnSpell
                                                int projStartY = centerY - 200 + 20;
                                                
                                                Console.WriteLine($"[UDP] Spawning bringer spell at X={projStartX}, Y={projStartY} (mình, hitbox canh)");
                                                projectileManager.SpawnOpponentSpell(projStartX, projStartY, opponentNum);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"[UDP] Error spawning bringer spell: {ex.Message}");
                                            }
                                        };
                                        spawnTimer.Start();
                                    }
                                    else
                                    {
                                        // ❌ Goatman và Knight Girl KHÔNG có projectile - bỏ qua
                                        Console.WriteLine($"[UDP] ⏭️ {oppCharType} doesn't have projectile (only Warrior and BringerOfDeath do)");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[UDP] Error handling opponent skill: {ex.Message}");
                                }
                            }
                            // Nếu thay đổi sang walk/jump từ animation khác (lần đầu)
                            if (isWalkOrJump && !currentIsWalkOrJump)
                            {
                                try
                                {
                                    oppAnimMgr.ResetAnimationToFirstFrame(action);
                                    opp.CurrentAnimation = action;
                                    Console.WriteLine($"[UDP]  Changed to walk/jump: {prevAnim} → {action}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[UDP] ⚠️ Reset animation error: {ex.Message}");
                                }
                            }
                            // Nếu thay đổi sang animation khác (punch, kick, stand, etc.)
                            else if (!isWalkOrJump)
                            {
                                try
                                {
                                    oppAnimMgr.ResetAnimationToFirstFrame(action);
                                    opp.CurrentAnimation = action;
                                    Console.WriteLine($"[UDP]  Changed animation: {prevAnim} → {action}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[UDP] ⚠️ Reset animation error: {ex.Message}");
                                }
                            }
                            // walk → jump hoặc jump → walk: cập nhật nhưng KHÔNG reset
                            else if (isWalkOrJump && currentIsWalkOrJump)
                            {
                                opp.CurrentAnimation = action;
                                Console.WriteLine($"[UDP] → Changed within walk/jump: {prevAnim} → {action} (NO RESET)");
                            }
                        }
                        
                        //  CẬP NHẬT animation trước đó cho lần tiếp theo
                        if (opponentNum == 1)
                            _prevAnimPlayer1 = action;
                        else
                            _prevAnimPlayer2 = action;
                    }
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BattleForm] OnOpponentUdpState error: {ex.Message}");
            }
        }

        private void BattleForm_KeyDown(object sender, KeyEventArgs e)
        {
            //  OFFLINE MODE: Accept inputs for both players locally
            if (!isOnlineMode)
            {
                // Player1 controls (left side) - WASD + JKLUÍ
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

                // Player2 controls (right side) - Arrow keys + D1-D5 or NumPad1-5
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
                    case Keys.Down:
                        // Reserved for future use (crouch, etc.)
                        break;

                    // Allow both top-row numbers and numpad for player2 actions
                    case Keys.D1:
                    case Keys.NumPad1:
                        if (player2State.CanAttack) ExecuteAttackWithHitbox(2, "punch", 10, 15);
                        break;
                    case Keys.D2:
                    case Keys.NumPad2:
                        if (player2State.CanAttack) ExecuteAttackWithHitbox(2, "kick", 15, 20);
                        break;
                    case Keys.D3:
                    case Keys.NumPad3:
                        if (player2State.CanDash) combatSystem.ExecuteDash(2);
                        break;
                    case Keys.D4:
                    case Keys.NumPad4:
                        if (player2State.CanAttack) combatSystem.ToggleSkill(2);
                        break;
                    case Keys.D5:
                    case Keys.NumPad5:
                        if (player2State.CanParry) combatSystem.StartParry(2);
                        break;
                }

                e.Handled = true;
                // Don't return - allow Escape handling below
            }
            //  ONLINE MODE: Each client controls ONLY their own player
            else if (myPlayerNumber == 1)
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
                e.Handled = true;
                // Don't return - allow Escape handling below
            }
            else if (myPlayerNumber == 2)
            {
                //  ONLINE MODE: Player 2 (use WASD like Player 1)
                switch (e.KeyCode)
                {
                    case Keys.A:
                        if (player2State.CanMove) { aPressed = true; player2State.LeftKeyPressed = true; }
                        break;
                    case Keys.D:
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
                e.Handled = true;
                // Don't return - allow Escape handling below
            }

            // Escape/menu handling remains shared for all modes
            if (e.KeyCode == Keys.Escape)
            {
                try
                {
                    // Show menu but do NOT pause the game. Timers and UDP continue running.
                    using (var menu = new MainMenuForm(roomCode))
                    {
                        var res = menu.ShowDialog(this);
                        if (res == DialogResult.OK)
                        {
                            try { gameTimer?.Stop(); } catch { }
                            try { walkAnimationTimer?.Stop(); } catch { }

                            // Close this BattleForm and return to lobby UI
                            this.Close();

                            bool isOffline = string.IsNullOrEmpty(roomCode) || roomCode == "000000";
                            if (isOffline)
                            {
                                if (this.Owner is DoAn_NT106.Client.JoinRoomForm ownerJoin)
                                {
                                    try { ownerJoin.Show(); ownerJoin.BringToFront(); }
                                    catch { Console.WriteLine("[BattleForm] Failed to show owner JoinRoomForm"); }
                                }
                                else
                                {
                                    var existingJoin = Application.OpenForms.OfType<DoAn_NT106.Client.JoinRoomForm>().FirstOrDefault();
                                    if (existingJoin != null) { existingJoin.Show(); existingJoin.BringToFront(); }
                                    else { Console.WriteLine("[BattleForm] No existing JoinRoomForm found for offline mode; skipping creation."); }
                                }
                            }
                            else
                            {
                                try
                                {
                                    var tcp = PersistentTcpClient.Instance;
                                    _ = System.Threading.Tasks.Task.Run(async () =>
                                    {
                                        try { var r = await tcp.LeaveRoomAsync(roomCode, username); Console.WriteLine($"[BattleForm] LeaveRoomAsync: {r.Success} - {r.Message}"); }
                                        catch (Exception ex) { Console.WriteLine($"[BattleForm] LeaveRoomAsync error: {ex.Message}"); }
                                    });

                                    DoAn_NT106.Client.GameLobbyForm lobbyForm = null;
                                    foreach (Form f in Application.OpenForms)
                                    {
                                        if (f is DoAn_NT106.Client.GameLobbyForm gf)
                                        {
                                            try
                                            {
                                                var field = f.GetType().GetField("roomCode", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                                                if (field != null)
                                                {
                                                    var val = field.GetValue(f) as string;
                                                    if (!string.IsNullOrEmpty(val) && string.Equals(val, roomCode, StringComparison.OrdinalIgnoreCase)) { lobbyForm = gf; break; }
                                                }
                                            }
                                            catch { }
                                            if (lobbyForm == null) lobbyForm = gf;
                                        }
                                    }

                                    if (lobbyForm != null) { try { if (lobbyForm.WindowState == FormWindowState.Minimized) lobbyForm.WindowState = FormWindowState.Normal; lobbyForm.Show(); lobbyForm.BringToFront(); } catch (Exception ex) { Console.WriteLine($"[BattleForm] Error showing GameLobbyForm: {ex.Message}"); } }
                                    else
                                    {
                                        var existingJoin = Application.OpenForms.OfType<DoAn_NT106.Client.JoinRoomForm>().FirstOrDefault();
                                        if (existingJoin != null) { if (existingJoin.WindowState == FormWindowState.Minimized) existingJoin.WindowState = FormWindowState.Normal; existingJoin.Show(); existingJoin.BringToFront(); }
                                        else { try { var newLobby = new DoAn_NT106.Client.GameLobbyForm(roomCode, username, token); newLobby.Show(); } catch (Exception ex) { Console.WriteLine($"[BattleForm] Error showing fallback lobby: {ex.Message}"); } }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[BattleForm] Error returning to lobby: {ex.Message}");
                                }
                            }
                        }
                        // else: user closed menu without leaving -- do nothing, game continues
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BattleForm] Escape menu error: {ex.Message}");
                }
            }

            e.Handled = true;
        }

        private BattleForm Refresh()
        {
            Console.WriteLine("Form refreshed"); return this;
        } // chát chơi màu mè quá chắc phải làm lại cả project



        private void BattleForm_KeyUp(object sender, KeyEventArgs e)
        {
            //  FIX: Handle offline mode where myPlayerNumber might be 0
            // In offline mode, handle all player controls
            // In online mode, only handle your own controls
            
            if (!isOnlineMode)
            {
                // OFFLINE MODE: Handle both players
                switch (e.KeyCode)
                {
                    // Player 1 controls
                    case Keys.A:
                        aPressed = false;
                        if (player1State != null)
                        {
                            player1State.LeftKeyPressed = false;
                            //   Gọi ngay StopMovement để dừng liền
                            if (player1State.CanMove && !player1State.IsJumping && 
                                !player1State.IsAttacking && !player1State.IsParrying && !player1State.IsSkillActive)
                            {
                                physicsSystem.StopMovement(player1State);
                            }
                        }
                        break;
                    case Keys.D:
                        dPressed = false;
                        if (player1State != null)
                        {
                            player1State.RightKeyPressed = false;
                            //   Gọi ngay StopMovement để dừng liền
                            if (player1State.CanMove && !player1State.IsJumping && 
                                !player1State.IsAttacking && !player1State.IsParrying && !player1State.IsSkillActive)
                            {
                                physicsSystem.StopMovement(player1State);
                            }
                        }
                        break;
                    
                    // Player 2 controls
                    case Keys.Left:
                        leftPressed = false;
                        if (player2State != null)
                        {
                            player2State.LeftKeyPressed = false;
                            //   Gọi ngay StopMovement để dừng liền
                            if (player2State.CanMove && !player2State.IsJumping && 
                                !player2State.IsAttacking && !player2State.IsParrying && !player2State.IsSkillActive)
                            {
                                physicsSystem.StopMovement(player2State);
                            }
                        }
                        break;
                    case Keys.Right:
                        rightPressed = false;
                        if (player2State != null)
                        {
                            player2State.RightKeyPressed = false;
                            //   Gọi ngay StopMovement để dừng liền
                            if (player2State.CanMove && !player2State.IsJumping && 
                                !player2State.IsAttacking && !player2State.IsParrying && !player2State.IsSkillActive)
                            {
                                physicsSystem.StopMovement(player2State);
                            }
                        }
                        break;
                }
            }
            else if (myPlayerNumber == 1)
            {
                // ONLINE MODE: Player 1
                switch (e.KeyCode)
                {
                    case Keys.A:
                        aPressed = false;
                        if (player1State != null)
                        {
                            player1State.LeftKeyPressed = false;
                            //   Gọi ngay StopMovement để dừng liền
                            if (player1State.CanMove && !player1State.IsJumping && 
                                !player1State.IsAttacking && !player1State.IsParrying && !player1State.IsSkillActive)
                            {
                                physicsSystem.StopMovement(player1State);
                            }
                        }
                        break;
                    case Keys.D:
                        dPressed = false;
                        if (player1State != null)
                        {
                            player1State.RightKeyPressed = false;
                            //   Gọi ngay StopMovement để dừng liền
                            if (player1State.CanMove && !player1State.IsJumping && 
                                !player1State.IsAttacking && !player1State.IsParrying && !player1State.IsSkillActive)
                            {
                                physicsSystem.StopMovement(player1State);
                            }
                        }
                        break;
                }
            }
            else if (myPlayerNumber == 2)
            {
                // ONLINE MODE: Player 2
                switch (e.KeyCode)
                {
                    case Keys.A:
                        aPressed = false;
                        if (player2State != null)
                        {
                            player2State.LeftKeyPressed = false;
                            //   Gọi ngay StopMovement để dừng liền
                            if (player2State.CanMove && !player2State.IsJumping && 
                                !player2State.IsAttacking && !player2State.IsParrying && !player2State.IsSkillActive)
                            {
                                physicsSystem.StopMovement(player2State);
                            }
                        }
                        break;
                    case Keys.D:
                        dPressed = false;
                        if (player2State != null)
                        {
                            player2State.RightKeyPressed = false;
                            //   Gọi ngay StopMovement để dừng liền
                            if (player2State.CanMove && !player2State.IsJumping && 
                                !player2State.IsAttacking && !player2State.IsParrying && !player2State.IsSkillActive)
                            {
                                physicsSystem.StopMovement(player2State);
                            }
                        }
                        break;
                }
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

            //  FIX: Fill background đầu tiên để không có lỗ đen
            e.Graphics.Clear(Color.Black);

            // Draw background
            if (background != null)
            {
                e.Graphics.DrawImage(background,
                    new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height),
                    new Rectangle(viewportX, 0, this.ClientSize.Width, this.ClientSize.Height),
                    GraphicsUnit.Pixel);
            }
            else
            {
                //  Fallback: Fill color nếu không có background
                e.Graphics.Clear(Color.DarkGreen);
            }

            // =====  MIGRATED: Draw effects (behind characters) =====
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

            // =====  MIGRATED: Draw characters using PlayerState =====
            DrawCharacter(e.Graphics, player1State.X, player1State.Y, player1State.CurrentAnimation, player1State.Facing, player1AnimationManager);
            DrawCharacter(e.Graphics, player2State.X, player2State.Y, player2State.CurrentAnimation, player2State.Facing, player2AnimationManager);

            // =====  MIGRATED: Draw hit effects (on top of characters) =====
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

            // =====  MIGRATED: Draw projectiles =====
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

            // =====  MIGRATED: Draw impact effects =====
            effectManager.DrawImpactEffects(e.Graphics, viewportX);

            // =====  MIGRATED: Draw parry indicators using PlayerState =====
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

            // =====  MIGRATED: Draw stun indicators using PlayerState =====
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
    

        /// <summary>
        ///  LOCAL-FIRST CAMERA: Focus on local player, then keep edge player visible
        /// Priority: 1) Center local player  2) Ensure edge player (left) visible  3) Both on screen
        /// </summary>
        private void UpdateCameraNetworkSync()
        {
            if (player1State == null || player2State == null) return;
            
            int viewWidth = this.ClientSize.Width;
            int worldWidth = backgroundWidth;
            int maxViewport = Math.Max(0, worldWidth - viewWidth);
            
            Rectangle p1Hb = GetPlayerHitbox(player1State);
            Rectangle p2Hb = GetPlayerHitbox(player2State);
            
            //   PRIMARY: Identify local and remote player
            PlayerState localPlayer = myPlayerNumber == 1 ? player1State : player2State;
            PlayerState remotePlayer = localPlayer == player1State ? player2State : player1State;
            Rectangle localHb = myPlayerNumber == 1 ? p1Hb : p2Hb;
            Rectangle remoteHb = localPlayer == player1State ? p2Hb : p1Hb;
            
            // SECONDARY: Identify who's nearest to left edge for safety check
            PlayerState edgePlayer = p1Hb.Left <= p2Hb.Left ? player1State : player2State;
            Rectangle edgeHb = p1Hb.Left <= p2Hb.Left ? p1Hb : p2Hb;
            
            int desired;
            int safeMarginLeft = 80;   // Keep players 80px from left edge
            int safeMarginRight = 80;  // Keep players 80px from right edge
            


            // Check if both players fit in viewport
            int leftMost = Math.Min(p1Hb.Left, p2Hb.Left);
            int rightMost = Math.Max(p1Hb.Right, p2Hb.Right);
            int widthNeeded = rightMost - leftMost;
            
            if (widthNeeded <= viewWidth)
            {
                // CASE 1: Both fit → center both
                int centerX = (leftMost + rightMost) / 2;
                desired = centerX - viewWidth / 2;
            }
            else
            {
                // CASE 2: Too far apart → LOCAL PLAYER FIRST
                // Start from local-centered viewport
                int centerLocal = localHb.Left + localHb.Width / 2;
                desired = centerLocal - viewWidth / 2;

                // Ensure REMOTE stays visible: nudge minimally if remote would go off-screen
                // Compute remote edges on screen for this desired
                int remoteLeftOnScreen = remoteHb.Left - desired;
                int remoteRightOnScreen = remoteHb.Right - desired;

                // If remote's left edge would be left of safe margin, shift viewport left (decrease desired)
                if (remoteLeftOnScreen < safeMarginLeft)
                {
                    int shift = safeMarginLeft - remoteLeftOnScreen; // positive amount to move viewport left
                    // Moving viewport left means decreasing desired
                    int maxShift = Math.Max(1, viewWidth / 3);
                    desired -= Math.Min(shift, maxShift);
                }

                // Recompute remote right on screen after possible shift
                remoteRightOnScreen = remoteHb.Right - desired;

                // If remote's right edge would be right of allowed area, shift viewport right (increase desired)
                if (remoteRightOnScreen > viewWidth - safeMarginRight)
                {
                    int shift = remoteRightOnScreen - (viewWidth - safeMarginRight);
                    int maxShift = Math.Max(1, viewWidth / 3);
                    desired += Math.Min(shift, maxShift);
                }

                // Secondary: if edge player (near left world edge) is critical, and currently would be off-screen, try to keep them visible
                if (edgeHb.Left < 50)
                {
                    int edgeLeftOnScreen = edgeHb.Left - desired;
                    if (edgeLeftOnScreen < safeMarginLeft)
                    {
                        int shift = safeMarginLeft - edgeLeftOnScreen;
                        int maxShift = Math.Max(1, viewWidth / 4);
                        // move viewport left a bit to reveal edge player but keep remote visible
                        int newDesired = desired - Math.Min(shift, maxShift);
                        // apply only if remote still visible after this
                        int newRemoteLeft = remoteHb.Left - newDesired;
                        int newRemoteRight = remoteHb.Right - newDesired;
                        if (newRemoteLeft >= -50 && newRemoteRight <= viewWidth + 50)
                        {
                            desired = newDesired;
                        }
                    }
                }
            }
            
            // Hard clamp to world bounds
            desired = Math.Max(0, Math.Min(maxViewport, desired));

            // Smooth interpolation with adaptive speed and deadzone to avoid jitter from network noise
            float delta = desired - viewportX;
            float absDelta = Math.Abs(delta);

            // Deadzone: ignore tiny adjustments to prevent camera jitter
            const float deadzone = 6f;
            if (absDelta < deadzone)
            {
                // small movement — do nothing
                return;
            }

            float smoothing;
            // If local near edge respond faster
            if (localHb.Left < 150 || localHb.Right > worldWidth - 150)
            {
                smoothing = absDelta > 150 ? 0.5f : 0.35f;
            }
            else if (absDelta > 300)
            {
                // Very large jump (teleport/respawn) — snap faster
                smoothing = 0.6f;
            }
            else if (absDelta > 120)
            {
                smoothing = 0.35f;
            }
            else
            {
                smoothing = 0.18f;
            }

            viewportX += (int)(delta * smoothing);
            viewportX = Math.Max(0, Math.Min(maxViewport, viewportX));
            
            // Debug logging
            if (Math.Abs(delta) > 50 || edgeHb.Left < 150)
            {
                int p1ScreenX = p1Hb.Left - viewportX;
                int p2ScreenX = p2Hb.Left - viewportX;
                bool p1Visible = p1ScreenX + p1Hb.Width > 0 && p1ScreenX < viewWidth;
                bool p2Visible = p2ScreenX + p2Hb.Width > 0 && p2ScreenX < viewWidth;
                
                Console.WriteLine($"[CAMERA LOCAL-FIRST] Local: {localPlayer.PlayerName}(X={localHb.Left}), Edge: {edgePlayer.PlayerName}(X={edgeHb.Left}), P1Vis={p1Visible}, P2Vis={p2Visible}, VP={viewportX}");
            }
        }


        /// <summary>
        ///  FIX: Camera with hurtbox-based boundaries like offline mode
        /// - Uses GetPlayerHitbox for accurate collision detection
        /// - Synchronizes with PhysicsSystem boundary checking
        /// - Prevents players from disappearing at edges
        /// </summary>
        private void UpdateCameraNoOvershoot()
        {
            if (player1State == null || player2State == null) return;

            int viewWidth = this.ClientSize.Width;
            int worldWidth = backgroundWidth;

            // Get player hitboxes for accurate boundary checking
            Rectangle p1Hb = GetPlayerHitbox(player1State);
            Rectangle p2Hb = GetPlayerHitbox(player2State);

            // 1. TÍNH CAMERA TARGET BÌNH THƯỜNG - hiển thị cả hai player
            int leftMost = Math.Min(p1Hb.Left, p2Hb.Left);
            int rightMost = Math.Max(p1Hb.Right, p2Hb.Right);
            int centerX = (leftMost + rightMost) / 2;

            int desired;

            // Nếu cả hai fit trong màn hình
            if ((rightMost - leftMost) <= viewWidth)
            {
                desired = centerX - viewWidth / 2;
            }
            else
            {
                // Không fit → ưu tiên local player nhưng đảm bảo thấy opponent
                PlayerState local = (isOnlineMode && myPlayerNumber > 0) 
                    ? (myPlayerNumber == 1 ? player1State : player2State) 
                    : player1State;
                Rectangle localHb = (local == player1State) ? p1Hb : p2Hb;
                Rectangle remoteHb = (local == player1State) ? p2Hb : p1Hb;

                int localCenter = localHb.Left + localHb.Width / 2;
                desired = localCenter - viewWidth / 2;

                // Đảm bảo opponent không ra khỏi màn hình
                int remoteScreenX = remoteHb.Left - desired;
                if (remoteScreenX < 80)
                {
                    desired = remoteHb.Left - 80;
                }
                else if (remoteScreenX > viewWidth - 80)
                {
                    desired = remoteHb.Right - viewWidth + 80;
                }
            }

            // 2. CLAMP TO WORLD BOUNDS - KHÔNG CHO OVERSHOOT
            int minViewport = 0;
            int maxViewport = Math.Max(0, worldWidth - viewWidth);
            desired = Math.Max(minViewport, Math.Min(maxViewport, desired));

            // 3. SMOOTH INTERPOLATION
            float delta = desired - viewportX;
            float absDelta = Math.Abs(delta);

            if (absDelta > 1) // Only update if meaningful change
            {
                float smoothing = 0.20f;
                
                // Faster response when players near edges
                if (p1Hb.Left < 150 || p2Hb.Left < 150 || 
                    p1Hb.Right > worldWidth - 150 || p2Hb.Right > worldWidth - 150)
                {
                    smoothing = 0.35f;
                }
                else if (absDelta > 200)
                {
                    smoothing = 0.40f;
                }
                
                viewportX += (int)(delta * smoothing);
            }

            // Final clamp to world
            viewportX = Math.Max(minViewport, Math.Min(maxViewport, viewportX));
        }


        private void BattleForm_Resize(object sender, EventArgs e)
        {
            if (resourceSystem != null && resourceSystem.HealthBar1 != null) //  CHECK NEW SYSTEM
            {
                int screenWidth = this.ClientSize.Width;

                // =====  MIGRATED: Resize bars using ResourceSystem =====
                resourceSystem.ResizeBars(screenWidth);

                // Update player name labels
                int barWidth = screenWidth / 4;
                int nameY = 10 + 3 * (20 + 5) + 90;  //  Cùng vị trí với lblPlayer1Name (dưới portrait)
                if (lblPlayer1Name != null)
                {
                    lblPlayer1Name.Location = new Point(20, nameY);
                    lblPlayer1Name.Size = new Size(barWidth, 25);
                }
                if (lblPlayer2Name != null)
                {
                    lblPlayer2Name.Location = new Point(screenWidth - barWidth - 20, nameY);  //  Cùng Y như Player 1
                    lblPlayer2Name.Size = new Size(barWidth, 25);
                }

                // =====  MIGRATED: Update ground level in PhysicsSystem =====
                groundLevel = Math.Max(0, this.ClientSize.Height - groundOffset);
                physicsSystem.UpdateGroundLevel(groundLevel);
                UpdateCharacterSize();

                // =====  MIGRATED: Reset player positions to ground =====
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
            int groundAdjustment = 0; //   Điều chỉnh vị trí so với mặt đất

            if (characterType == "girlknight")
            {
                sizeScale = 0.7f;
                yOffset = (int)(PLAYER_HEIGHT * (1.0f - sizeScale));
                groundAdjustment = 0; // Vị trí chuẩn
            }
            else if (characterType == "bringerofdeath")
            {
                sizeScale = 1.6f;
                yOffset = 0;    
                groundAdjustment = -95;
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
            //  SỬA: Áp dụng hệ số nhân toàn cục
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

            // =====  UPDATE: Update PhysicsSystem with new player size =====
            physicsSystem.UpdatePlayerSize(PLAYER_WIDTH, PLAYER_HEIGHT);

            groundLevel = Math.Max(0, this.ClientSize.Height - groundOffset);

            // =====  MIGRATED: Reset positions using PhysicsSystem =====
            if (!player1State.IsJumping) physicsSystem.ResetToGround(player1State);
            if (!player2State.IsJumping) physicsSystem.ResetToGround(player2State);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            //   Disconnect UDP khi đóng form
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

            // =====  MIGRATED: Cleanup new systems =====
            //   Cleanup PlayerState timers
            try { player1State?.Cleanup(); } catch { }
            try { player2State?.Cleanup(); } catch { }
            try { combatSystem?.Cleanup(); } catch { }
            try { effectManager?.Cleanup(); } catch { }
            try { projectileManager?.Cleanup(); } catch { }
            // =========================================

            //  RESUME: Theme music when returning to MainForm
            try { SoundManager.PlayMusic(BackgroundMusic.ThemeMusic, loop: true); } catch { }
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
        //  HELPER METHODS (KEPT FROM ORIGINAL)
        // ===========================
        //   Xử lý tấn công với attack hitbox mới
        //  SỬA LẦN 2: Xử lý tấn công với frame counter
        // In BattleForm. cs - THAY THẾ HÀM ExecuteAttackWithHitbox()

        private void ExecuteAttackWithHitbox(int playerNum, string attackType, int damage, int staminaCost)
        {
            Console.WriteLine($"[BattleForm] Player {playerNum} attempts {attackType}");

            bool hitSuccess = combatSystem.ExecuteAttack(playerNum, attackType);

            if (hitSuccess)
            {
                if (playerNum == 1) player1ComboCount++;
                else player2ComboCount++;
            }

            // NOTE: AttackCount should only be incremented when a hit actually lands.
            // CombatSystem.ApplyDamage already increments PlayerState.AttackCount/SkillCount
            // when damage is applied. Do NOT increment AttackCount here (attempts).
            // if (playerNum == 1) player1State.AttackCount++;
            // else player2State.AttackCount++;
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

                //  NEW: Fix right-facing warrior kick horizontal offset by 30px (shift left)
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
        }


        private void SetupEventHandlers()
        {
            gameTimer.Tick += GameTimer_Tick;
            if (!gameTimer.Enabled) gameTimer.Start();

            this.KeyDown += BattleForm_KeyDown;
            this.KeyUp += BattleForm_KeyUp;
            this.Resize += BattleForm_Resize;
        }

        private void SetBackground(string backgroundName)
        {
            try
            {
                // Normalize common variants: accept "battlefield2", "battleground2", with any case/whitespace
                if (!string.IsNullOrEmpty(backgroundName))
                {
                    var bn = backgroundName.Trim().ToLowerInvariant();
                    if (bn.StartsWith("battlefield"))
                    {
                        var num = bn.Replace("battlefield", "").Trim();
                        if (int.TryParse(num, out int n) && n >= 1 && n <= 4)
                        {
                            backgroundName = $"battleground{n}";
                        }
                    }
                    else if (bn.StartsWith("battleground"))
                    {
                        // ensure canonical form
                        var num = bn.Replace("battleground", "").Trim();
                        if (int.TryParse(num, out int n) && n >= 1 && n <= 4)
                        {
                            backgroundName = $"battleground{n}";
                        }
                    }
                }

                // KIỂM TRA 1: Form đã sẵn sàng chưa?
                if (this.ClientSize.Height <=  100 || this.IsDisposed || !this.IsHandleCreated)
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
                switch ((backgroundName ?? "").ToLower())
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
                    if (screenHeight <=  100)
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
                    Console.WriteLine("[SetBackground] Using fallback background");
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

  
        /// <summary>
        /// Get actual character width for movement boundaries
        /// </summary>

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

            //  CĂN GIỮA HOÀN HẢO
            int offsetX = (actualWidth - hitboxWidth) / 2;
            int offsetY = (int)(actualHeight * config.OffsetYPercent);

            //  CHỈ GOATMAN MỚI CÓ HARD FIX
            if (player.CharacterType == "goatman")
            {
                offsetX += 65; // Sprite padding fix
            }
            // Bringer of Death, Warrior, GirlKnight đều căn giữa tự nhiên

            // hurtbox debug removed

            return new Rectangle(
                player.X + offsetX,
                player.Y + yOffset + groundAdjustment + offsetY,
                hitboxWidth,
                hitboxHeight
            );
        }

        //   Phương thức tính vùng tấn công của nhân vật
        private Rectangle GetAttackHitbox(PlayerState attacker, string attackType)
        {
            //  LẤY actualSize NGAY ĐẦU ĐỂ TRÁNH LỖI
            var actualSize = GetActualCharacterSize(attacker.CharacterType);
            int actualWidth = actualSize.actualWidth;
            int actualHeight = actualSize.actualHeight;
            int yOffset = actualSize.yOffset;
            int groundAdjustment = actualSize.groundAdjustment;

            if (!characterAttackConfigs.ContainsKey(attacker.CharacterType) ||
                !characterAttackConfigs[attacker.CharacterType].ContainsKey(attackType))
            {
                Console.WriteLine($"⚠️ No attack config for {attacker.CharacterType}.{attackType}, using default");

                int attackWidth = (int)(actualWidth * 0.8f);
                int attackHeight = (int)(actualHeight * 0.6f);
                int attackRange = (int)(actualWidth * 0.7f);

                int defaultCenterX = attacker.X + (actualWidth / 2);
                int attackX = attacker.Facing == "right" ? defaultCenterX : defaultCenterX - attackRange;
                int attackY = attacker.Y + yOffset + groundAdjustment + (int)(actualHeight * 0.3f);

                return new Rectangle(attackX, attackY, attackRange, attackHeight);
            }

            var config = characterAttackConfigs[attacker.CharacterType][attackType];

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

            //  SPECIAL CASE: Girl Knight skill should hit forward AND backward (extend range to opposite direction)
            if (attacker.CharacterType == "girlknight" && attackType == "skill")
            {
                // Bidirectional area: cover forward + backward
                // Reduce 10px on each side → shift start by +10 and reduce total width by 20
                finalAttackX = (configCenterX - attackRangeValue) + 10;
                finalAttackY = attacker.Y + yOffset + groundAdjustment + offsetY;
                int bidirectionalWidth = (attackRangeValue * 2) - 20;
                if (bidirectionalWidth < 0) bidirectionalWidth = 0; // safety
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

            return new Rectangle(finalAttackX, finalAttackY, attackRangeValue, attackHeightValue);
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
            //  GỌI COMBATSYSTEM TRỰC TIẾP - ĐÃ XỬ LÝ ĐẦY ĐỦ
            combatSystem.ApplyDamage(player, damage, knockback);

            //  SYNC LẠI BIẾN CŨ (để UI hoạt động)
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