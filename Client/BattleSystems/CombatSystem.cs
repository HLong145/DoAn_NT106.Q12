using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using NAudio.Wave;

namespace DoAn_NT106.Client.BattleSystems
{
    public class CombatSystem
    {
        private PlayerState player1;
        private PlayerState player2;
        private CharacterAnimationManager player1AnimManager;
        private CharacterAnimationManager player2AnimManager;
        private EffectManager effectManager;
        private ProjectileManager projectileManager;

        private const int HIT_STUN_DURATION_MS = 200;
        private const int DASH_DISTANCE = 400;
        private const int DASH_DURATION_MS = 200;
        private const int SMOOTH_DASH_DURATION_MS = 300; // 0.3s for smooth dash
        private const float CHARGE_ACCELERATION = 20f;
        private const float CHARGE_MAX_SPEED = 30f;
        private const int CHARGE_DURATION_MS = 3000;

        private const int PARRY_WINDOW_MS = 300;
        private const int PARRY_COOLDOWN_MS = 900;
        private const int PARRY_STAMINA_COST = 10;
        private Timer p1ParryTimer;
        private Timer p1ParryCooldownTimer;
        private Timer p2ParryTimer;
        private Timer p2ParryCooldownTimer;

        private int playerWidth;
        private int playerHeight;
        private int backgroundWidth;

        private Action invalidateCallback;
        private Action<string, Color> showHitEffectCallback;
        private Func<PlayerState, string, Rectangle> getAttackHitboxCallback;

        private readonly Dictionary<string, Dictionary<string, float>> frameTimings = new Dictionary<string, Dictionary<string, float>>
        {
            ["goatman"] = new Dictionary<string, float>
            {
                ["punch"] = 1000f / 11f,
                ["kick"] = 1000f / 8f
            },
            ["girlknight"] = new Dictionary<string, float>
            {
                ["punch"] = 1000f / 8f,
                ["kick"] = 1000f / 8f,
                ["special"] = 1000f / 10f
            },
            ["warrior"] = new Dictionary<string, float>
            {
                ["punch"] = 1000f / 12f,
                ["kick"] = 1000f / 10f,
                ["special"] = 1000f / 7f
            },
            ["bringerofdeath"] = new Dictionary<string, float>
            {
                ["punch"] = 1000f / 8f,
                ["kick"] = 1000f / 18f,
                ["special"] = 1000f
            }
        };

        private Func<PlayerState, Rectangle> getPlayerHurtboxCallback;

        public CombatSystem(
            PlayerState p1, PlayerState p2,
            CharacterAnimationManager p1AnimManager, CharacterAnimationManager p2AnimManager,
            EffectManager effectMgr, ProjectileManager projectileMgr,
            int playerWidth, int playerHeight, int backgroundWidth,
            Action invalidateCallback,
            Action<string, Color> showHitEffectCallback,
            Func<PlayerState, string, Rectangle> getAttackHitboxCallback,
            Func<PlayerState, Rectangle> getPlayerHurtboxCallback // NEW
        )
        {
            this.player1 = p1;
            this.player2 = p2;
            this.player1AnimManager = p1AnimManager;
            this.player2AnimManager = p2AnimManager;
            this.effectManager = effectMgr;
            this.projectileManager = projectileMgr;
            this.playerWidth = playerWidth;
            this.playerHeight = playerHeight;
            this.backgroundWidth = backgroundWidth;
            this.invalidateCallback = invalidateCallback;
            this.showHitEffectCallback = showHitEffectCallback;
            this.getAttackHitboxCallback = getAttackHitboxCallback;
            this.getPlayerHurtboxCallback = getPlayerHurtboxCallback;
            SetupParryTimers();
        }
        private void SetupParryTimers()
        {
            p1ParryTimer = new Timer { Interval = PARRY_WINDOW_MS };
            p1ParryTimer.Tick += (s, e) =>
            {
                p1ParryTimer.Stop();
                player1.IsParrying = false;
                player1.IsParryOnCooldown = true;
                if (!player1.IsAttacking && !player1.IsJumping)
                    player1.ResetToIdle();
                p1ParryCooldownTimer.Start();
                invalidateCallback?.Invoke();
            };

            p1ParryCooldownTimer = new Timer { Interval = PARRY_COOLDOWN_MS };
            p1ParryCooldownTimer.Tick += (s, e) =>
            {
                p1ParryCooldownTimer.Stop();
                player1.IsParryOnCooldown = false;
            };

            p2ParryTimer = new Timer { Interval = PARRY_WINDOW_MS };
            p2ParryTimer.Tick += (s, e) =>
            {
                p2ParryTimer.Stop();
                player2.IsParrying = false;
                player2.IsParryOnCooldown = true;
                if (!player2.IsAttacking && !player2.IsJumping)
                    player2.ResetToIdle();
                p2ParryCooldownTimer.Start();
                invalidateCallback?.Invoke();
            };

            p2ParryCooldownTimer = new Timer { Interval = PARRY_COOLDOWN_MS };
            p2ParryCooldownTimer.Tick += (s, e) =>
            {
                p2ParryCooldownTimer.Stop();
                player2.IsParryOnCooldown = false;
            };
        }

        private int GetFrameTiming(string characterType, string attackType, int frameNumber)
        {
            if (!frameTimings.ContainsKey(characterType) || !frameTimings[characterType].ContainsKey(attackType))
                return frameNumber * 100;

            float msPerFrame = frameTimings[characterType][attackType];
            return (int)(frameNumber * msPerFrame);
        }

        public void StartParry(int playerNum)
        {
            PlayerState player = playerNum == 1 ? player1 : player2;
            CharacterAnimationManager animMgr = playerNum == 1 ? player1AnimManager : player2AnimManager;
            Timer parryTimer = playerNum == 1 ? p1ParryTimer : p2ParryTimer;

            if (!player.CanParry || player.IsAttacking) return;
            if (!player.ConsumeStamina(PARRY_STAMINA_COST))
            {
                showHitEffectCallback?.Invoke("No Stamina!", Color.Gray);
                return;
            }

            player.IsParrying = true;
            player.CurrentAnimation = "parry";
            animMgr.ResetAnimationToFirstFrame("parry");
            // ✅ Play parry sound: warrior uses its own resource, others use shared resource
            try
            {
                if (player.CharacterType == "warrior")
                    TryPlayParryResource("parry_warrior");
                else if (player.CharacterType == "girlknight" || player.CharacterType == "bringerofdeath" || player.CharacterType == "goatman")
                    TryPlayParryResource("parry_KG_bringer_goatman");
            }
            catch { }
            parryTimer.Stop();
            parryTimer.Start();
            // ✅ SỬA: Không hồi mana khi bắt đầu parry, chỉ hồi khi parry thành công (dính attack)
            showHitEffectCallback?.Invoke("Parry!", Color.Cyan);
            invalidateCallback?.Invoke();
        }

        // Try to play embedded parry sound resource using NAudio (supports mp3)
        private void TryPlayParryResource(string resourceKey)
        {
            try
            {
                var obj = Properties.Resources.ResourceManager.GetObject(resourceKey);
                byte[] audioBytes = null;

                if (obj is byte[] bb) audioBytes = bb;
                else if (obj is System.IO.UnmanagedMemoryStream ums)
                {
                    using var ms = new System.IO.MemoryStream();
                    ums.CopyTo(ms);
                    audioBytes = ms.ToArray();
                }
                else if (obj is System.IO.Stream s0)
                {
                    using var ms2 = new System.IO.MemoryStream();
                    s0.Position = 0;
                    s0.CopyTo(ms2);
                    audioBytes = ms2.ToArray();
                }

                if (audioBytes != null && audioBytes.Length > 0)
                {
                    var ms = new System.IO.MemoryStream(audioBytes);
                    var reader = new Mp3FileReader(ms);
                    var wo = new WaveOutEvent();
                    wo.Init(reader);
                    wo.PlaybackStopped += (s, e) =>
                    {
                        try { wo.Dispose(); } catch { }
                        try { reader.Dispose(); } catch { }
                        try { ms.Dispose(); } catch { }
                    };
                    wo.Play();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CombatSystem] TryPlayParryResource failed: {ex.Message}");
            }
        }

        public void ExecuteAttack(int playerNum, string attackType)
        {
            PlayerState attacker = playerNum == 1 ? player1 : player2;
            PlayerState defender = playerNum == 1 ? player2 : player1;
            CharacterAnimationManager animMgr = playerNum == 1 ? player1AnimManager : player2AnimManager;

            // ✅ Chặn tấn công khi skill đang active
            if (attacker.IsSkillActive)
            {
                showHitEffectCallback?.Invoke("Skill Active!", Color.Cyan);
                return;
            }

            if (!attacker.CanAttack || attacker.IsDashing || attacker.IsAttacking)
            {
                Console.WriteLine($"⚠️ Player{playerNum} không thể attack!");
                return;
            }

            // ✅ SỬA: Special attack không tiêu tốn stamina ở đây, sẽ quản lý riêng trong ExecuteSpecialAttack
            int staminaCost = 0;
            if (attackType != "special")
            {
                staminaCost = attackType == "kick" ? 15 : (attackType == "punch" ? 15 : 0);
                
                // ✅ SỬA: Warrior punch tốn 15 stamina, kick tốn 20 stamina
                if (attacker.CharacterType == "warrior" && attackType == "punch")
                {
                    staminaCost = 15;
                }
                
                if (attacker.CharacterType == "warrior" && attackType == "kick")
                {
                    staminaCost = 20;
                }
                
                // ✅ SỬA: Bringer of Death punch tốn 20 stamina
                if (attacker.CharacterType == "bringerofdeath" && attackType == "punch")
                {
                    staminaCost = 20;
                }
                
                // ✅ SỬA: Bringer of Death kick tốn 30 stamina
                if (attacker.CharacterType == "bringerofdeath" && attackType == "kick")
                {
                    staminaCost = 30;
                }
                
                if (!attacker.ConsumeStamina(staminaCost))
                {
                    showHitEffectCallback?.Invoke("No Stamina!", Color.Gray);
                    Console.WriteLine($"❌ Player{playerNum} không đủ stamina! Need {staminaCost}, have {attacker.Stamina}");
                    return;
                }

                Console.WriteLine($"✅ Player{playerNum} consumed {staminaCost} stamina, remaining: {attacker.Stamina}");
            }

            // ✅ Play attack sound
            CombatSoundExtensions.PlayAttackSound(attacker.CharacterType, attackType);

            attacker.IsAttacking = true;
            attacker.IsWalking = false;

            string oldAnim = attacker.CurrentAnimation;
            if (!string.IsNullOrEmpty(oldAnim) && oldAnim != attackType)
            {
                try
                {
                    var oldAnimImg = animMgr.GetAnimation(oldAnim);
                    if (oldAnimImg != null && ImageAnimator.CanAnimate(oldAnimImg))
                        ImageAnimator.StopAnimate(oldAnimImg, (s, e) => invalidateCallback?.Invoke());
                }
                catch { }
            }

            attacker.CurrentAnimation = attackType;
            animMgr.ResetAnimationToFirstFrame(attackType);

            if (attackType == "punch") ExecutePunchAttack(playerNum, attacker, defender, animMgr);
            else if (attackType == "kick") ExecuteKickAttack(playerNum, attacker, defender, animMgr);
            else if (attackType == "special") ExecuteSpecialAttack(playerNum, attacker, defender, animMgr);

            invalidateCallback?.Invoke();
        }
        private void ExecutePunchAttack(int playerNum, PlayerState attacker, PlayerState defender, CharacterAnimationManager animMgr)
        {
            string charType = attacker.CharacterType;
            Console.WriteLine($"[ExecutePunch] START - Player={playerNum}, Char={charType}");

            if (charType == "warrior")
            {
                // ✅ Play warrior punch sound twice at the start (regardless of hit)
                // First sound immediately
                try { DoAn_NT106.SoundManager.PlaySound(DoAn_NT106.Client.SoundEffect.PunchWarrior); } catch { }
                // Second sound with 500ms delay (0.5 seconds) to match animation spacing
                try { DoAn_NT106.SoundManager.PlaySoundWithDelay(DoAn_NT106.Client.SoundEffect.PunchWarrior, 500); } catch { }

                int hitFrame6 = GetFrameTiming("warrior", "punch", 6);
                int hitFrame10 = GetFrameTiming("warrior", "punch", 10);

                Console.WriteLine($"[ExecutePunch] Warrior hit timings: Frame6={hitFrame6}ms, Frame10={hitFrame10}ms");

                var hitTimer1 = new Timer { Interval = hitFrame6 };
                hitTimer1.Tick += (s, e) =>
                {
                    hitTimer1.Stop();
                    hitTimer1.Dispose();

                    Console.WriteLine($"[ExecutePunch] ⏰ HIT TIMER 1 FIRED at {hitFrame6}ms");

                    // Luôn kiểm tra collision với hitbox của punch
                    Rectangle attackBox = getAttackHitboxCallback(attacker, "punch");
                    Rectangle hurtBox = getPlayerHurtboxCallback(defender);

                    Console.WriteLine($"[ExecutePunch] Attack Box: X={attackBox.X}, Y={attackBox.Y}, W={attackBox.Width}, H={attackBox.Height}");
                    Console.WriteLine($"[ExecutePunch] Hurt Box:   X={hurtBox.X}, Y={hurtBox.Y}, W={hurtBox.Width}, H={hurtBox.Height}");

                    bool hit = attackBox.IntersectsWith(hurtBox);
                    Console.WriteLine($"[ExecutePunch] Collision: {(hit ? "✅ HIT!" : "❌ MISS")}");

                    if (hit)
                    {
                        Console.WriteLine($"[ExecutePunch] 💥 APPLYING DAMAGE 7 to Player {(playerNum == 1 ? 2 : 1)}");
                        ApplyDamage(playerNum == 1 ? 2 : 1, 7); // ✅ SỬA: 10 -> 7
                        attacker.RegenerateManaOnHitLand(); // ✅ THÊM: Hồi mana khi đánh trúng
                        showHitEffectCallback?.Invoke("Strike!", Color.Yellow);
                    }
                };
                hitTimer1.Start();
                Console.WriteLine($"[ExecutePunch] Timer 1 STARTED");

                var hitTimer2 = new Timer { Interval = hitFrame10 };
                hitTimer2.Tick += (s, e) =>
                {
                    hitTimer2.Stop();
                    hitTimer2.Dispose();

                    Console.WriteLine($"[ExecutePunch] ⏰ HIT TIMER 2 FIRED at {hitFrame10}ms");

                    // Không chặn bởi IsStunned/hurt để đảm bảo frame hit vẫn check
                    Rectangle attackBox = getAttackHitboxCallback(attacker, "punch");
                    Rectangle hurtBox = getPlayerHurtboxCallback(defender);

                    bool hit = attackBox.IntersectsWith(hurtBox);
                    if (hit)
                    {
                        Console.WriteLine($"[ExecutePunch] 💥 APPLYING DAMAGE 7 to Player {(playerNum == 1 ? 2 : 1)}");
                        ApplyDamage(playerNum == 1 ? 2 : 1, 7); // ✅ SỬA: 10 -> 7
                        attacker.RegenerateManaOnHitLand(); // ✅ THÊM: Hồi mana khi đánh trúng
                        showHitEffectCallback?.Invoke("Strike!", Color.Orange);
                    }
                };
                hitTimer2.Start();
                Console.WriteLine($"[ExecutePunch] Timer 2 STARTED");
            }
            else if (charType == "goatman")
            {
                int hitDelay = GetFrameTiming("goatman", "punch", 4);
                Console.WriteLine($"[ExecutePunch] Goatman hit at {hitDelay}ms");

                var hitTimer = new Timer { Interval = hitDelay };
                hitTimer.Tick += (s, e) =>
                {
                    hitTimer.Stop();
                    hitTimer.Dispose();

                    Console.WriteLine($"[ExecutePunch] ⏰ Goatman HIT TIMER FIRED");

                    Rectangle attackBox = getAttackHitboxCallback(attacker, "punch");
                    Rectangle hurtBox = getPlayerHurtboxCallback(defender);

                    if (attackBox.IntersectsWith(hurtBox))
                    {
                        Console.WriteLine($"[ExecutePunch] 💥 Goatman DAMAGE 10");
                        ApplyDamage(playerNum == 1 ? 2 : 1, 10);
                        attacker.RegenerateManaOnHitLand(); // ✅ THÊM: Hồi mana khi đánh trúng
                        showHitEffectCallback?.Invoke("Punch!", Color.Orange);
                        // ✅ Sound is played at startup via PlayAttackSound, NOT on hit
                    }
                };
                hitTimer.Start();
            }
            else if (charType == "girlknight")
            {
                int hitDelay = GetFrameTiming("girlknight", "punch", 3);
                Console.WriteLine($"[ExecutePunch] GirlKnight hit at {hitDelay}ms");

                var hitTimer = new Timer { Interval = hitDelay };
                hitTimer.Tick += (s, e) =>
                {
                    hitTimer.Stop();
                    hitTimer.Dispose();

                    Console.WriteLine($"[ExecutePunch] ⏰ GirlKnight HIT TIMER FIRED");

                    Rectangle attackBox = getAttackHitboxCallback(attacker, "punch");
                    Rectangle hurtBox = getPlayerHurtboxCallback(defender);

                    if (attackBox.IntersectsWith(hurtBox))
                    {
                        Console.WriteLine($"[ExecutePunch] 💥 GirlKnight DAMAGE 10");
                        ApplyDamage(playerNum == 1 ? 2 : 1, 10);
                        attacker.RegenerateManaOnHitLand(); // ✅ TH ÊM: Hồi mana khi đánh trúng
                        showHitEffectCallback?.Invoke("Punch!", Color.Pink);
                        // KG punch: only play later sound (no early sound elsewhere)
                        try { DoAn_NT106.SoundManager.PlaySound(DoAn_NT106.Client.SoundEffect.PunchKG); } catch { }
                    }
                };
                hitTimer.Start();
            }
            else if (charType == "bringerofdeath")
            {
                int hitDelay = GetFrameTiming("bringerofdeath", "punch", 6);
                Console.WriteLine($"[ExecutePunch] Bringer hit at {hitDelay}ms");

                var hitTimer = new Timer { Interval = hitDelay };
                hitTimer.Tick += (s, e) =>
                {
                    hitTimer.Stop();
                    hitTimer.Dispose();

                    Console.WriteLine($"[ExecutePunch] ⏰ Bringer HIT TIMER FIRED");

                    Rectangle attackBox = getAttackHitboxCallback(attacker, "punch");
                    Rectangle hurtBox = getPlayerHurtboxCallback(defender);

                    if (attackBox.IntersectsWith(hurtBox))
                    {
                        Console.WriteLine($"[ExecutePunch] 💥 Bringer DAMAGE 20"); // ✅ SỬA: 10 -> 20
                        ApplyDamage(playerNum == 1 ? 2 : 1, 20); // ✅ SỬA: 10 -> 20
                        attacker.RegenerateManaOnHitLand();
                        showHitEffectCallback?.Invoke("Punch!", Color.Purple);
                        // ✅ Play punch sound on hit (khớp animation)
                        try { DoAn_NT106.SoundManager.PlaySound(DoAn_NT106.Client.SoundEffect.PunchBringer); } catch { }
                    }
                };
                hitTimer.Start();
            }

            int duration = animMgr.GetAnimationDuration("punch");
            Console.WriteLine($"[ExecutePunch] Animation duration: {duration}ms");
            ResetAttackAnimation(duration, playerNum);
        }
        private void ExecuteKickAttack(int playerNum, PlayerState attacker, PlayerState defender, CharacterAnimationManager animMgr)
        {
            string charType = attacker.CharacterType;

            if (charType == "goatman")
                ExecuteGoatmanKick(playerNum, attacker, defender, animMgr);
            else if (charType == "girlknight")
                ExecuteGirlKnightKick(playerNum, attacker, defender, animMgr);
            else if (charType == "warrior")
                ExecuteWarriorKick(playerNum, attacker, defender, animMgr);
            else if (charType == "bringerofdeath")
            {
                int hitDelay = GetFrameTiming("bringerofdeath", "kick", 6);
                var hitTimer = new Timer { Interval = hitDelay };
                hitTimer.Tick += (s, e) =>
                {
                    hitTimer.Stop();
                    hitTimer.Dispose();

                    Rectangle attackBox = getAttackHitboxCallback(attacker, "kick");
                    Rectangle hurtBox = getPlayerHurtboxCallback(defender);

                    if (attackBox.IntersectsWith(hurtBox))
                    {
                        ApplyDamage(playerNum == 1 ? 2 : 1, 10); // ✅ SỬA: 15 -> 10 damage
                        attacker.RegenerateManaOnHitLand(); // ✅ TH ÊM: Hồi mana khi đánh trúng
                        showHitEffectCallback?.Invoke("Kick!", Color.Orange);
                    }
                };
                hitTimer.Start();
            }

            int duration = animMgr.GetAnimationDuration("kick");
            ResetAttackAnimation(duration, playerNum);
        }

        private void ExecuteGoatmanKick(int playerNum, PlayerState attacker, PlayerState defender, CharacterAnimationManager animMgr)
        {
            int hitDelay = GetFrameTiming("goatman", "kick", 4);
            int knockbackDistance = animMgr.GetKnockbackDistance("kick");

            var hitTimer = new Timer { Interval = hitDelay };
            hitTimer.Tick += (s, e) =>
            {
                hitTimer.Stop();
                hitTimer.Dispose();

                // Compute attack hitbox and impact position at hit frame
                Rectangle attackHitbox = getAttackHitboxCallback(attacker, "kick");
                int impactXBase = attacker.Facing == "right" ? (attackHitbox.X + attackHitbox.Width) : (attackHitbox.X - 100);
                int impactY = attackHitbox.Y + (attackHitbox.Height / 2) - 50;

                // Visual offset per facing: left -> +90px, right -> -150px
                int impactX = impactXBase + (attacker.Facing == "left" ? 90 : -150);

                effectManager.ShowImpactEffect(playerNum, impactX, impactY, attacker.Facing, invalidateCallback);

                // Collision and damage remain unchanged
                Rectangle targetHurtbox = getPlayerHurtboxCallback(defender);
                if (attackHitbox.IntersectsWith(targetHurtbox))
                {
                    ApplyDamage(playerNum == 1 ? 2 : 1, 15, false);
                    attacker.RegenerateManaOnHitLand(); // ✅ TH ÊM: Hồi mana khi đánh trúng
                    int knockbackDir = attacker.Facing == "right" ? 1 : -1;
                    ApplyKnockback(defender, knockbackDir, knockbackDistance);
                    showHitEffectCallback?.Invoke("Heavy Impact!", Color.OrangeRed);
                    // ✅ Delay kick sound 500ms to match requested timing
                    // Play impact sound immediately and also ensure it can play multiple times
                    try { DoAn_NT106.SoundManager.PlaySound(DoAn_NT106.Client.SoundEffect.KickGM); } catch { }
                }
            };
            hitTimer.Start();
        }

        private void ExecuteGirlKnightKick(int playerNum, PlayerState attacker, PlayerState defender, CharacterAnimationManager animMgr)
        {
            // Continuous collision from attack hit frame (minus 2 frames) to animation end, single damage only (pierce)
            float msPerFrame = frameTimings["girlknight"]["kick"]; // per-frame duration
            int duration = animMgr.GetAnimationDuration("kick");

            // Slide (frames 6..9)
            int slideStartFrame = 6;
            int slideEndFrame = 9;
            int slideFrameCount = slideEndFrame - slideStartFrame;
            int slideStartTime = (int)(slideStartFrame * msPerFrame);
            int slideDuration = (int)(slideFrameCount * msPerFrame);
            int slideDistance = 400;

            // Start slide at slideStartTime
            var slideStartTimer = new Timer { Interval = slideStartTime };
            slideStartTimer.Tick += (s, e) =>
            {
                slideStartTimer.Stop();
                slideStartTimer.Dispose();

                int slideDirection = attacker.Facing == "right" ? 1 : -1;
                int slideRemaining = slideDistance;
                int slideElapsed = 0;
                float slideSpeed = (float)slideDistance / slideDuration * 16;

                var slideTimer = new Timer { Interval = 16 };
                slideTimer.Tick += (s2, e2) =>
                {
                    slideElapsed += 16;

                    if (slideElapsed < slideDuration && slideRemaining > 0)
                    {
                        int moveAmount = Math.Min(slideRemaining, (int)Math.Ceiling(slideSpeed));
                        attacker.X += moveAmount * slideDirection;
                        ClampPlayerToMap(attacker);
                        slideRemaining -= moveAmount;
                    }
                    else
                    {
                        slideTimer.Stop();
                        slideTimer.Dispose();
                    }
                };
                slideTimer.Start();
            };
            slideStartTimer.Start();

            // Start continuous hit check from the actual hit frame timing, adjusted earlier by 2 frames
            int hitStartTime = 0;
            try { hitStartTime = animMgr.GetHitFrameDelay("kick"); } catch { hitStartTime = (int)(6 * msPerFrame); }
            hitStartTime = Math.Max(0, hitStartTime - (int)(2 * msPerFrame));

            bool hasDealtDamage = false;
            bool soundPlayed = false;

            var startCheckTimer = new Timer { Interval = hitStartTime };
            startCheckTimer.Tick += (s, e) =>
            {
                startCheckTimer.Stop();
                startCheckTimer.Dispose();

                int elapsed = 0;
                var continuousCheckTimer = new Timer { Interval = 16 };
                continuousCheckTimer.Tick += (s2, e2) =>
                {
                    elapsed += 16;

                    // Stop when animation expected duration is reached or attacker state changed
                    if (elapsed >= duration || attacker.CurrentAnimation != "kick")
                    {
                        continuousCheckTimer.Stop();
                        continuousCheckTimer.Dispose();
                        return;
                    }

                    if (!hasDealtDamage)
                    {
                        // Apply damage immediately upon first collision, do not stop slide (pierce)
                        if (CheckAttackHit(attacker, defender, "kick") && !defender.IsParrying && !defender.IsDashing)
                        {
                            ApplyDamage(playerNum == 1 ? 2 : 1, 15);
                            attacker.RegenerateManaOnHitLand(); // ✅ TH ÊM: Hồi mana khi đánh trúng
                            showHitEffectCallback?.Invoke("Kick!", Color.Pink);
                            hasDealtDamage = true; // only once
                            // KG: do not play early sound; only fallback late sound will play
                        }
                    }
                };
                continuousCheckTimer.Start();
            };
            startCheckTimer.Start();

            // ✅ Fallback: if no hit, play sound near end of animation
            int fallbackDelay = Math.Max(0, duration - hitStartTime - 10);
            var fallbackTimer = new Timer { Interval = fallbackDelay };
            fallbackTimer.Tick += (s3, e3) =>
            {
                try { fallbackTimer.Stop(); fallbackTimer.Dispose(); } catch { }
                if (!soundPlayed && attacker.CurrentAnimation == "kick")
                {
                    try { DoAn_NT106.SoundManager.PlaySound(DoAn_NT106.Client.SoundEffect.KickKG); } catch { }
                    soundPlayed = true;
                }
            };
            fallbackTimer.Start();
        }
        private void ExecuteWarriorKick(int playerNum, PlayerState attacker, PlayerState defender, CharacterAnimationManager animMgr)
        {
            float msPerFrame = frameTimings["warrior"]["kick"];
            int slideStartFrame = 1;
            int slideEndFrame = 3;
            int slideFrameCount = slideEndFrame - slideStartFrame;
            // ✅ SỬA: Lấy hit frame từ animator thay vì cứng cáp
            int hitTime = animMgr.GetHitFrameDelay("kick");

            int slideDuration = (int)(slideFrameCount * msPerFrame);
            int slideDistance = 400;

            // Slide (có thể ngắt nếu muốn), nhưng KHÔNG ảnh hưởng hit timer
            int slideDirection = attacker.Facing == "right" ? 1 : -1;
            int slideRemaining = slideDistance;
            int slideElapsed = 0;
            float slideSpeed = (float)slideDistance / slideDuration * 16;

            var slideTimer = new Timer { Interval = 16 };
            slideTimer.Tick += (s, e) =>
            {
                slideElapsed += 16;

                if (slideElapsed < slideDuration && slideRemaining > 0)
                {
                    int moveAmount = Math.Min(slideRemaining, (int)Math.Ceiling(slideSpeed));
                    attacker.X += moveAmount * slideDirection;
                    ClampPlayerToMap(attacker);
                    slideRemaining -= moveAmount;
                }
                else
                {
                    slideTimer.Stop();
                    slideTimer.Dispose();
                    Console.WriteLine($"? Warrior slide ended after {slideDuration}ms");
                }
            };
            slideTimer.Start();

            // HIT TIMER: không chặn bởi IsStunned/hurt
            var hitTimer = new Timer { Interval = hitTime };
            hitTimer.Tick += (s, e) =>
            {
                hitTimer.Stop();
                hitTimer.Dispose();

                if (CheckAttackHit(attacker, defender, "kick"))
                {
                    ApplyDamage(playerNum == 1 ? 2 : 1, 10); // ✅ SỬA: 15 -> 10
                    attacker.RegenerateManaOnHitLand(); // ✅ THÊM: Hồi mana khi đánh trúng
                    showHitEffectCallback?.Invoke("Warrior Kick!", Color.Gold);
                    // ✅ Warrior kick: Chỉ phát ButtonClick ở startup (từ PlayAttackSound)
                }
            };
            hitTimer.Start();
        }
        private void ExecuteSpecialAttack(int playerNum, PlayerState attacker, PlayerState defender, CharacterAnimationManager animMgr)
        {
            string charType = attacker.CharacterType;
            
            // ✅ SỬA: Check mana và stamina TRƯỚC khi tiêu tốn gì
            int manaCost = 30;
            int staminaCost = 0;
            
            // ✅ SỬA: Bringer of Death skill tốn 35 mana, KHÔNG tốn stamina
            if (charType == "bringerofdeath")
            {
                manaCost = 35;
                staminaCost = 0;
            }
            else if (charType == "warrior")
            {
                staminaCost = 15;
            }
            
            // ✅ Kiểm tra đủ tài nguyên TRƯỚC
            if (attacker.Mana < manaCost)
            {
                showHitEffectCallback?.Invoke("Not enough mana!", Color.Gray);
                attacker.IsAttacking = false;
                return;
            }
            
            if (staminaCost > 0 && attacker.Stamina < staminaCost)
            {
                showHitEffectCallback?.Invoke("Not enough stamina!", Color.Gray);
                attacker.IsAttacking = false;
                return;
            }
            
            // ✅ CHỈ tiêu tốn khi đã kiểm tra xong
            if (!attacker.ConsumeMana(manaCost))
            {
                attacker.IsAttacking = false;
                return;
            }
            
            if (staminaCost > 0 && !attacker.ConsumeStamina(staminaCost))
            {
                showHitEffectCallback?.Invoke("No Stamina!", Color.Gray);
                attacker.IsAttacking = false;
                return;
            }

            attacker.CurrentAnimation = "fireball";
            animMgr.ResetAnimationToFirstFrame("fireball");

            if (charType == "bringerofdeath")
            {
                // ✅ Bringer skill: play skill_bringer sound at cast moment
                var castTimer = new Timer { Interval = 300 };
                castTimer.Tick += (s, e) =>
                {
                    castTimer.Stop();
                    castTimer.Dispose();
                    try { DoAn_NT106.SoundManager.PlaySound(DoAn_NT106.Client.SoundEffect.SkillBringer); } catch { }
                    int targetPlayer = playerNum == 1 ? 2 : 1;
                    projectileManager.SpawnSpell(
                        targetPlayer,
                        playerNum,
                        (pn) => getPlayerHurtboxCallback(pn == 1 ? player1 : player2),
                        ApplyDamage,
                        showHitEffectCallback
                    );
                };
                castTimer.Start();
            }
            else if (charType == "warrior")
            {
                int castDelay = GetFrameTiming("warrior", "special", 3);
                var castTimer = new Timer { Interval = castDelay };
                castTimer.Tick += (s, e) =>
                {
                    castTimer.Stop();
                    castTimer.Dispose();
                    Rectangle hurtbox = getPlayerHurtboxCallback(attacker);
                    int direction = attacker.Facing == "right" ? 1 : -1;
                    int startX = direction > 0 ? (hurtbox.X + hurtbox.Width) : (hurtbox.X - 160);
                    int startY = hurtbox.Y + (hurtbox.Height / 2) - (160 / 2);
                    projectileManager.SpawnWarriorProjectile(startX, startY, direction, playerNum);
                    // Warrior skill: play skill_warrior sound
                    try { DoAn_NT106.SoundManager.PlaySound(DoAn_NT106.Client.SoundEffect.SkillWarrior); } catch { }
                };
                castTimer.Start();
            }
            else
            {
                int direction = attacker.Facing == "right" ? 1 : -1;
                int startX = attacker.Facing == "right" ? attacker.X + playerWidth : attacker.X - 150;
                projectileManager.ShootFireball(startX, attacker.Y + 30, direction, playerNum);
            }

            int duration = animMgr.GetAnimationDuration("special");
            ResetAttackAnimation(duration, playerNum);
        }

        public void ExecuteDash(int playerNum)
        {
            PlayerState player = playerNum == 1 ? player1 : player2;
            CharacterAnimationManager animMgr = playerNum == 1 ? player1AnimManager : player2AnimManager;

            // ✅ Chặn dash khi skill đang active
            if (player.IsSkillActive)
            {
                showHitEffectCallback?.Invoke("Skill Active!", Color.Cyan);
                return;
            }

            if (player.IsAttacking || player.IsDashing || !player.CanDash)
            {
                  Console.WriteLine($"⚠️ Player{playerNum} không thể dash!");

                return;
            }

            if (!player.ConsumeStamina(20))
            {
                showHitEffectCallback?.Invoke("No Stamina!", Color.Gray);
                return;
            }

            // ✅ Play dash sound
            CombatSoundExtensions.PlayAttackSound(player.CharacterType, "dash");

            string oldAnim = player.CurrentAnimation;
            if (!string.IsNullOrEmpty(oldAnim))
            {
                try
                {
                    var oldAnimImg = animMgr.GetAnimation(oldAnim);
                    if (oldAnimImg != null && ImageAnimator.CanAnimate(oldAnimImg))
                        ImageAnimator.StopAnimate(oldAnimImg, (s, e) => invalidateCallback?.Invoke());
                }
                catch { }
            }

            if (player.CharacterType == "girlknight" || player.CharacterType == "warrior")
            {
                ExecuteSmoothDash(playerNum, player, animMgr);
            }
            else
            {
                ExecuteTeleportDash(playerNum, player, animMgr);
            }
        }

        private void ExecuteSmoothDash(int playerNum, PlayerState player, CharacterAnimationManager animMgr)
        {
            int slideDuration = SMOOTH_DASH_DURATION_MS;
            int slideDistance = DASH_DISTANCE;
            if (player.CharacterType == "warrior")
            {
                // ✅ SỬA: Warrior speed 1.2x Knight Girl
                // Knight Girl: 300ms, Warrior: 300 * (1/1.2) = 250ms
                slideDuration = 250;
                slideDistance = 400;
            }

            int slideDirection = player.Facing == "right" ? 1 : -1;

            Console.WriteLine($"?? {player.CharacterType} smooth dash: {slideDistance}px in {slideDuration}ms");

            player.IsDashing = true;

            if (animMgr.HasAnimation("slide"))
            {
                player.CurrentAnimation = "slide";
                animMgr.ResetAnimationToFirstFrame("slide");
            }

            // ✅ Tính khoảng cách có thể di chuyển
            int availableDistance = CalculateAvailableDistance(player, slideDirection, slideDistance);
            int slideRemaining = Math.Abs(availableDistance);
            int slideElapsed = 0;
            float slideSpeed = (float)slideRemaining / slideDuration * 16;

            var slideTimer = new Timer { Interval = 16 };
            slideTimer.Tick += (s, e) =>
            {
                slideElapsed += 16;

                if (slideElapsed < slideDuration && slideRemaining > 0)
                {
                    int moveAmount = Math.Min(slideRemaining, (int)Math.Ceiling(slideSpeed));
                    player.X += moveAmount * slideDirection;
                    slideRemaining -= moveAmount;

                    // ✅ Soft clamp khi cần
                    if (slideRemaining <= 0)
                    {
                        ClampPlayerToMap(player);
                      }
                }
                else
                {
                    slideTimer.Stop();
                    slideTimer.Dispose();
                    player.IsDashing = false;

                    if (!player.IsAttacking && !player.IsJumping && !player.IsStunned)
                        player.ResetToIdle();

                    Console.WriteLine($"? {player.CharacterType} dash ended, remaining: {slideRemaining}px");
                }
            };
            slideTimer.Start();
        }

        public void ToggleSkill(int playerNum)
        {
            PlayerState player = playerNum == 1 ? player1 : player2;
            if (player.IsAttacking) return;

            if (player.CharacterType == "girlknight")
                ToggleKnightGirlSkill(playerNum, player);
            else if (player.CharacterType == "goatman")
                ExecuteGoatmanCharge(playerNum, player,playerNum == 1 ? player1AnimManager : player2AnimManager);
            else if (player.CharacterType == "warrior")
            {
                // ✅ SỬA: Warrior cần 30 mana + 15 stamina
                if (player.Mana >= 30 && player.Stamina >= 15)
                    ExecuteAttack(playerNum, "special");
                else if (player.Mana < 30)
                    showHitEffectCallback?.Invoke("Need 30 Mana!", Color.Gray);
                else
                    showHitEffectCallback?.Invoke("Need 15 Stamina!", Color.Gray);
            }
            else if (player.CharacterType == "bringerofdeath")
            {
                // ✅ SỬA: Bringer of Death cần 35 mana, KHÔNG tốn stamina
                if (player.Mana >= 35)
                    ExecuteAttack(playerNum, "special");
                else
                    showHitEffectCallback?.Invoke("Need 35 Mana!", Color.Gray);
            }
        }

        private void ToggleKnightGirlSkill(int playerNum, PlayerState player)
        {
            CharacterAnimationManager animMgr = playerNum == 1 ? player1AnimManager : player2AnimManager;
            PlayerState opponent = playerNum == 1 ? player2 : player1;

            if (!player.IsSkillActive)
            {
                if (!player.ConsumeMana(25)) // ✅ SỬA: 30 -> 25 mana để bắt đầu skill
                {
                    showHitEffectCallback?.Invoke("Not enough mana!", Color.Gray);
                    return;
                }

                player.IsSkillActive = true;
                player.CurrentAnimation = "fireball";
                animMgr.ResetAnimationToFirstFrame("fireball");

                int elapsedMs = 0;
                int lastDamageTime = 0;
                int manaCheckTime = 0; // ✅ TH ÊM: Track mana check separately
                int damageCounter = 0;

                Console.WriteLine($"[SKILL START] Player {playerNum} ({player.CharacterType}) activated skill!");

                var continuousCheckTimer = new Timer { Interval = 500 }; // ✅ SỬA: 1000 -> 500ms (check damage mỗi 0.5s)
                continuousCheckTimer.Tick += (s, e) =>
                {
                    elapsedMs += 500;
                    lastDamageTime += 500;
                    manaCheckTime += 500;

                    if (!player.IsSkillActive)
                    {
                        continuousCheckTimer.Stop();
                        continuousCheckTimer.Dispose();
                        Console.WriteLine($"[SKILL END] Player {playerNum} skill stopped");
                        return;
                    }

                    // ✅ Lấy hitbox
                    Rectangle attackBox = getAttackHitboxCallback(player, "skill");
                    Rectangle hurtBox = getPlayerHurtboxCallback(opponent);

                    bool isColliding = attackBox.IntersectsWith(hurtBox);

                    Console.WriteLine($"[SKILL {elapsedMs}ms] Player {playerNum}:");
                    Console.WriteLine($"  Attack: X={attackBox.X}, Y={attackBox.Y}, W={attackBox.Width}, H={attackBox.Height}");
                    Console.WriteLine($"  Hurt:   X={hurtBox.X}, Y={hurtBox.Y}, W={hurtBox.Width}, H={hurtBox.Height}");
                    Console.WriteLine($"  Collision: {(isColliding ? "✅ YES" : "❌ NO")} | Last damage: {lastDamageTime}ms ago");

                    // ✅ GÂY DAMAGE MỖI 500MS (2 lần/giây)
                    if (isColliding && lastDamageTime >= 500)
                    {
                        int targetPlayer = playerNum == 1 ? 2 : 1;

                        Console.WriteLine($"[SKILL] 🎯 Player {playerNum} dealing damage to Player {targetPlayer}!");

                        ApplyDamage(targetPlayer, 10, false);
                        player.RegenerateManaOnHitLand();
                        showHitEffectCallback?.Invoke("Energy!", Color.Cyan);
                        damageCounter++;
                        lastDamageTime = 0;
                        // ✅ Play punch sound on skill hit (KG uses punch sound)
                        try { DoAn_NT106.SoundManager.PlaySound(DoAn_NT106.Client.SoundEffect.PunchKG); } catch { }

                        Console.WriteLine($"[SKILL] ✅ Damage #{damageCounter} dealt!");
                    }

                    // ✅ Consume mana + stamina MỎI 1000MS (mỗi 1 giây)
                    if (manaCheckTime >= 1000)
                    {
                        player.Mana -= 25; // ✅ SỬA: -30 -> -25 mana/s
                        Console.WriteLine($"[SKILL] Mana consumed at {elapsedMs}ms, remaining: {player.Mana}");

                        // ✅ Consume stamina mỗi 1000ms
                        player.Stamina = Math.Max(0, player.Stamina - 15);
                        Console.WriteLine($"[SKILL] Stamina consumed at {elapsedMs}ms, remaining: {player.Stamina}");

                        manaCheckTime = 0; // Reset counter

                        // ✅ Hết mana
                        if (player.Mana <= 0) // ✅ SỬA: < 30 -> <= 0 (vừa hết mana thì dừng)
                        {
                            continuousCheckTimer.Stop();
                            continuousCheckTimer.Dispose();
                            player.IsSkillActive = false;

                            if (!player.IsAttacking && !player.IsJumping)
                                player.ResetToIdle();

                            showHitEffectCallback?.Invoke("Out of Mana!", Color.Gray);
                            Console.WriteLine($"[SKILL END] Player {playerNum} out of mana. Total damage hits: {damageCounter}");
                        }
                    }
                };
                continuousCheckTimer.Start();

                showHitEffectCallback?.Invoke("Energy Shield!", Color.Cyan);
            }
            else
            {
                player.IsSkillActive = false;
                if (!player.IsAttacking && !player.IsJumping)
                    player.ResetToIdle();
                Console.WriteLine($"[SKILL END] Player {playerNum} manually deactivated skill");
            }
        }
        private void ExecuteGoatmanCharge(int playerNum, PlayerState player, CharacterAnimationManager animMgr)
        {
            PlayerState opponent = playerNum == 1 ? player2 : player1;

            // ✅ SỬA: Goatman skill tốn 35 mana + 35 stamina
            // ✅ KIỂM TRA điều kiện TRƯỚC khi tiêu tốn
            if (player.Mana < 35)
            {
                showHitEffectCallback?.Invoke("Need 35 Mana!", Color.Gray);
                return;
            }
            
            if (player.Stamina < 35)
            {
                showHitEffectCallback?.Invoke("Need 35 Stamina!", Color.Gray);
                return;
            }
            
            // ✅ CHỈ tiêu tốn khi đã kiểm tra xong
            if (!player.ConsumeMana(35)) return;
            if (!player.ConsumeStamina(35)) return;

            player.IsCharging = true;
            player.CurrentAnimation = "fireball";
            animMgr.ResetAnimationToFirstFrame("fireball");

            int chargeDirection = player.Facing == "right" ? 1 : -1;
            var chargeTimer = new Timer { Interval = 16 };
            int elapsedMs = 0;

            chargeTimer.Tick += (s, e) =>
            {
                elapsedMs += 16;

                if (elapsedMs < 1500)
                {
                    // ✅ SỬA: Tốc độ tăng gấp đôi
                    player.ChargeSpeed += (CHARGE_ACCELERATION * 2) / (1000f / 16);
                    player.ChargeSpeed = Math.Min(CHARGE_MAX_SPEED * 2, player.ChargeSpeed);
                }
                else
                {
                    // ✅ SỬA: Max speed tăng gấp đôi
                    player.ChargeSpeed = CHARGE_MAX_SPEED * 2;
                }

                // ✅ THAY THẾ SafeMovePlayer:
                int desiredMove = (int)(player.ChargeSpeed * chargeDirection);
                player.X += desiredMove;
                
                // ✅ SỬA: Kiểm tra và dừng skill nếu đụi rìa map
                ClampPlayerToMap(player);
                var boundary = GetBoundaryFromHurtbox(player);
                if ((chargeDirection > 0 && player.X >= boundary.maxX) ||
                    (chargeDirection < 0 && player.X <= boundary.minX))
                {
                    // ✅ Đụi rìa - dừng skill ngay
                    player.IsCharging = false;
                    player.ChargeSpeed = 0;
                    chargeTimer.Stop();
                    chargeTimer.Dispose();
                    if (!player.IsAttacking && !player.IsJumping)
                        player.ResetToIdle();
                    showHitEffectCallback?.Invoke("Hit Wall!", Color.Red);
                    return;
                }

                // ✅ Hitbox = Goatman's HURTBOX + extend forward toward facing
                Rectangle baseHurtbox = GetPlayerHurtbox(player);
                Rectangle chargeHitbox;
                
                if (player.Facing == "right")
                {
                    // Lao sang phải: mở rộng 40px từ cạnh phải của hurtbox sang phải
                    chargeHitbox = new Rectangle(
                        baseHurtbox.X, 
                        baseHurtbox.Y, 
                        baseHurtbox.Width + 40, 
                        baseHurtbox.Height
                    );
                }
                else
                {
                    // Lao sang trái: mở rộng 200px từ cạnh trái của hurtbox sang trái
                    chargeHitbox = new Rectangle(
                        baseHurtbox.X - 200, 
                        baseHurtbox.Y, 
                        baseHurtbox.Width + 200, 
                        baseHurtbox.Height
                    );
                }
                
                Rectangle targetHurtbox = GetPlayerHurtbox(opponent);

                if (chargeHitbox.IntersectsWith(targetHurtbox))
                {
                    // ❌ No GM_impact effect on charge collision (impact only for kick)
                    // ✅ SỬA: Damage tăng từ 25 → 30
                    ApplyDamage(playerNum == 1 ? 2 : 1, 30, false);
                    showHitEffectCallback?.Invoke("Charged!", Color.Gold);
                    // ✅ Goatman charge uses kick sound on hit
                    try { DoAn_NT106.SoundManager.PlaySound(DoAn_NT106.Client.SoundEffect.KickGM); } catch { }

                    player.IsCharging = false;
                    player.ChargeSpeed = 0;
                    chargeTimer.Stop();
                    chargeTimer.Dispose();
                    if (!player.IsAttacking && !player.IsJumping)
                        player.ResetToIdle();
                }

                if (elapsedMs >= CHARGE_DURATION_MS)
                {
                    player.IsCharging = false;
                    player.ChargeSpeed = 0;
                    chargeTimer.Stop();
                    chargeTimer.Dispose();
                    if (!player.IsAttacking && !player.IsJumping)
                        player.ResetToIdle();
                    showHitEffectCallback?.Invoke("Charge Ended", Color.Gray);
                }
            };
            chargeTimer.Start();
            showHitEffectCallback?.Invoke("CHARGE!", Color.Gold);
        }

        public void ApplyDamage(int targetPlayer, int damage, bool knockback = true)
        {
            PlayerState target = targetPlayer == 1 ? player1 : player2;
            PlayerState attacker = targetPlayer == 1 ? player2 : player1;

            if (target.IsDashing)
            {
                showHitEffectCallback?.Invoke("Miss!", Color.Gray);
                return;
            }

            if (target.IsParrying)
            {
                target.Stamina = Math.Min(100, target.Stamina + 8);
                target.RegenerateManaOnParrySuccess(); // ✅ THÊM: Hồi mana khi parry thành công
                showHitEffectCallback?.Invoke("Blocked!", Color.Cyan);
                CancelAttack(targetPlayer == 1 ? 2 : 1);
                invalidateCallback?.Invoke();
                return;
            }

            if (target.CurrentAnimation == "hurt") return;

            bool wasAttacking = target.IsAttacking;
            bool wasSkillActive = target.IsSkillActive;
            bool wasCharging = target.IsCharging;
            
            target.TakeDamage(damage);
            target.RegenerateManaOnHitMiss(); // ✅ TH ÊM: Hồi mana khi bị đánh (không parry kịp)
            showHitEffectCallback?.Invoke($"-{damage}", Color.Red);
            effectManager.ShowHitEffectAtPosition(target.CharacterType, target.X, target.Y, invalidateCallback);
            
            // ✅ SỬA: Nếu đang charging thì KHÔNG interrupt, chỉ hiển thị stun nhưng skill vẫn chạy
            if (!target.IsCharging)
            {
                target.IsStunned = true;
                CancelAttack(targetPlayer);
            }
            
            if (wasAttacking)
            {
                Console.WriteLine($"?? Player{targetPlayer} attack INTERRUPTED by damage!");
                showHitEffectCallback?.Invoke("Interrupted!", Color.Orange);
            }
            else if (wasSkillActive)
            {
                Console.WriteLine($"?? Player{targetPlayer} skill INTERRUPTED by damage!");
                showHitEffectCallback?.Invoke("Interrupted!", Color.Orange);
            }
            else if (wasCharging)
            {
                Console.WriteLine($"?? Player{targetPlayer} charge CONTINUES despite damage!");
                showHitEffectCallback?.Invoke("Charging!", Color.Gold);
            }

            // ✅ SỬA: Chỉ chuyển sang animation hurt nếu không đang charge
            if (!target.IsCharging)
            {
                target.CurrentAnimation = "hurt";
                CharacterAnimationManager targetAnimMgr = targetPlayer == 1 ? player1AnimManager : player2AnimManager;
                targetAnimMgr.ResetAnimationToFirstFrame("hurt");
            }

            if (knockback)
            {
                int kb = (attacker.X > target.X) ? -20 : 20;
                target.X += kb;
                ClampPlayerToMap(target);

                Console.WriteLine($"? Knockback applied to Player{targetPlayer}, X={target.X}");
            }
            invalidateCallback?.Invoke();

            // ✅ SỬA: Chỉ set stun timer nếu không charging
            if (!target.IsCharging)
            {
                var restoreTimer = new Timer { Interval = HIT_STUN_DURATION_MS };
                restoreTimer.Tick += (s, e) =>
                {
                    restoreTimer.Stop();
                    restoreTimer.Dispose();
                    target.IsStunned = false;
                    if (!target.IsAttacking && !target.IsJumping && target.CurrentAnimation == "hurt")
                        target.ResetToIdle();
                    invalidateCallback?.Invoke();
                };
                restoreTimer.Start();
            }
        }

        private void ApplyKnockback(PlayerState target, int direction, int distance)
        {
            int knockbackRemaining = distance;
            var knockbackTimer = new Timer { Interval = 16 };
            float knockbackSpeed = (float)distance / 167 * 16;

            knockbackTimer.Tick += (s, e) =>
            {
                if (knockbackRemaining > 0)
                {
                    int moveAmount = Math.Min(knockbackRemaining, (int)Math.Ceiling(knockbackSpeed));
                    target.X += moveAmount * direction;
                    target.X = Math.Max(0, Math.Min(backgroundWidth - playerWidth, target.X));
                    ClampPlayerToMap(target);
                    knockbackRemaining -= moveAmount;
                }

                if (knockbackRemaining <= 0)
                {
                    knockbackTimer.Stop();
                    knockbackTimer.Dispose();
                }
            };
            knockbackTimer.Start();
        }
        /// <summary>
        /// Clamp player position to map boundaries
        /// </summary>
        private void ClampPlayerToMap(PlayerState player)
        {
            var boundary = GetBoundaryFromHurtbox(player);

            // Soft clamp với tolerance 5px
            const int TOLERANCE = 5;

            if (player.X < boundary.minX - TOLERANCE)
            {
                player.X = boundary.minX;
                Console.WriteLine($"⚠️ Player{player.PlayerNumber} reached LEFT boundary at X={player.X}");
            }
            else if (player.X > boundary.maxX + TOLERANCE)
            {
                player.X = boundary.maxX;
                Console.WriteLine($"⚠️ Player{player.PlayerNumber} reached RIGHT boundary at X={player.X}");
            }
            // Nếu trong khoảng tolerance, giữ nguyên để knockback mượt
        }

        /// <summary>
        /// Get boundary from hurtbox (giống PhysicsSystem)
        /// </summary>
        private (int minX, int maxX) GetBoundaryFromHurtbox(PlayerState player)
        {
            if (getPlayerHurtboxCallback == null)
            {
                return (0, backgroundWidth - playerWidth);
            }

            Rectangle hurtbox = getPlayerHurtboxCallback(player);

            // Tính toán dựa trên hurtbox thực tế
            int offsetFromSprite = hurtbox.X - player.X;

            // MinX: khi hurtbox chạm biên trái
            int minX = 0 - offsetFromSprite;

            // MaxX: khi hurtbox chạm biên phải
            int maxX = backgroundWidth - hurtbox.Width - offsetFromSprite;

            return (minX, maxX);
        }

        private void CancelAttack(int playerNum)
        {
            PlayerState player = playerNum == 1 ? player1 : player2;
            player.IsAttacking = false;
            player.IsSkillActive = false;
            // ✅ SỬA: KHÔNG hủy IsCharging - để Goatman tiếp tục ủi khi nhận sát thương
            // player.IsCharging = false;
            
            // ✅ THÊM: Hủy animation hiện tại nếu là attack
            CharacterAnimationManager animMgr = playerNum == 1 ? player1AnimManager : player2AnimManager;
            if (animMgr != null)
            {
                try
                {
                    var currentAnim = animMgr.GetAnimation(player.CurrentAnimation);
                    if (currentAnim != null && ImageAnimator.CanAnimate(currentAnim))
                    {
                        ImageAnimator.StopAnimate(currentAnim, (s, e) => { });
                    }
                }
                catch { }
            }
            
            // ✅ THÊM: Chuyển về animation idle
            if (!player.IsJumping && !player.IsParrying)
                player.ResetToIdle();
        }

        private void ResetAttackAnimation(int delay, int playerNum)
        {
            PlayerState player = playerNum == 1 ? player1 : player2;
            CharacterAnimationManager animMgr = playerNum == 1 ? player1AnimManager : player2AnimManager;
            
            var animationEndTimer = new Timer { Interval = delay };
            animationEndTimer.Tick += (s, e) =>
            {
                animationEndTimer.Stop();
                animationEndTimer.Dispose();
                
                try
                {
                    var attackAnim = animMgr.GetAnimation(player.CurrentAnimation);
                    if (attackAnim != null && ImageAnimator.CanAnimate(attackAnim))
                    {
                        for (int i = 0; i < 5; i++)
                            ImageAnimator.StopAnimate(attackAnim, (s2, e2) => { });
                    }
                }
                catch { }
                
                player.IsAttacking = false;
                if (player.CurrentAnimation != "hurt" && player.CurrentAnimation != "parry")
                    player.ResetToIdle();
                
                Console.WriteLine($"? UNLOCKED Player{playerNum} at {delay}ms");
            };
            animationEndTimer.Start();
        }

        private bool CheckAttackHit(PlayerState attacker, PlayerState defender, string attackType)
        {
            Rectangle attackHitbox = getAttackHitboxCallback(attacker, attackType);
            Rectangle targetHurtbox = getPlayerHurtboxCallback(defender); // dùng cùng hệ quy chiếu
            return attackHitbox.IntersectsWith(targetHurtbox);
        }
        private bool CheckSkillHit(PlayerState player, PlayerState opponent)
        {
            Rectangle skillRange = new Rectangle(player.X - 50, player.Y - 50, playerWidth + 100, playerHeight + 100);
            Rectangle targetRect = GetPlayerHurtbox(opponent);
            return skillRange.IntersectsWith(targetRect);
        }

        private Rectangle GetPlayerHurtbox(PlayerState player)
        {
            // ✅ Tính actualSize giống như BattleForm
            float sizeScale = 1.0f;
            int yOffset = 0;
            int groundAdjustment = 0;

            if (player.CharacterType == "girlknight")
            {
                sizeScale = 0.7f;
                yOffset = (int)(playerHeight * (1.0f - sizeScale));
            }
            else if (player.CharacterType == "bringerofdeath")
            {
                sizeScale = 1.6f;
                groundAdjustment = -95;
            }
            else if (player.CharacterType == "goatman")
            {
                sizeScale = 0.7f;
                yOffset = (int)(playerHeight * (1.0f - sizeScale));
            }

            int actualHeight = (int)(playerHeight * sizeScale);
            int actualWidth = (int)(playerWidth * sizeScale);

            // ✅ Hitbox config
            float widthPercent = 0.5f;
            float heightPercent = 0.8f;
            float offsetYPercent = 0.2f;
            int offsetXAdjust = 0;

            if (player.CharacterType == "girlknight")
            {
                widthPercent = 0.5f;
                heightPercent = 0.80f;
                offsetYPercent = 0.20f;
            }
            else if (player.CharacterType == "bringerofdeath")
            {
                widthPercent = 0.30f;
                heightPercent = 0.50f;
                offsetYPercent = 0.35f;
            }
            else if (player.CharacterType == "goatman")
            {
                widthPercent = 0.60f;
                heightPercent = 0.78f;
                offsetYPercent = 0.12f;
                offsetXAdjust = 65; // ✅ Sprite padding fix
            }
            else if (player.CharacterType == "warrior")
            {
                widthPercent = 0.48f;
                heightPercent = 0.75f;
                offsetYPercent = 0.18f;
            }

            int hurtboxWidth = (int)(actualWidth * widthPercent);
            int hurtboxHeight = (int)(actualHeight * heightPercent);
            int offsetX = (actualWidth - hurtboxWidth) / 2 + offsetXAdjust;
            int offsetY = (int)(actualHeight * offsetYPercent);

            return new Rectangle(
                player.X + offsetX,
                player.Y + yOffset + groundAdjustment + offsetY,
                hurtboxWidth,
                hurtboxHeight
            );
        }

        private void ExecuteTeleportDash(int playerNum, PlayerState player, CharacterAnimationManager animMgr)
        {
            int startX = player.X;
            int startY = player.Y;
            string facing = player.Facing;

            player.IsDashing = true;
            int dashDirection = player.Facing == "right" ? 1 : -1;

            // ✅ Sử dụng CalculateAvailableDistance để tính khoảng cách thực tế
            int actualDistance = CalculateAvailableDistance(player, dashDirection, DASH_DISTANCE);
            int destinationX = player.X + actualDistance;

            Console.WriteLine($"? {player.CharacterType} teleport dash: {DASH_DISTANCE}px -> {actualDistance}px to X={destinationX}");

            // ? STEP 1: Show dash effect at FIRST position
            effectManager.ShowDashEffect(playerNum, startX, startY, facing, invalidateCallback);

            // ? STEP 2: Make player INVISIBLE
            string originalAnimation = player.CurrentAnimation;
            player.CurrentAnimation = "invisible";

            // ? STEP 3: Move to destination (đã được tính toán an toàn)
            player.X = destinationX;
            // Không cần ClampPlayerToMap vì CalculateAvailableDistance đã tính toán

            invalidateCallback?.Invoke();

            // ? STEP 4: After 0.3s, make player visible again
            var reappearTimer = new Timer { Interval = SMOOTH_DASH_DURATION_MS };
            reappearTimer.Tick += (s, e) =>
            {
                reappearTimer.Stop();
                reappearTimer.Dispose();

                player.IsDashing = false;
                player.CurrentAnimation = "idle";

                if (!player.IsAttacking && !player.IsJumping && !player.IsStunned)
                    player.ResetToIdle();

                invalidateCallback?.Invoke();
            };
            reappearTimer.Start();
        }

        /// <summary>
        /// ✅ NEW: Tính khoảng cách có thể di chuyển
        /// </summary>
        private int CalculateAvailableDistance(PlayerState player, int direction, int desiredDistance)
        {
            var boundary = GetBoundaryFromHurtbox(player);

            int availableDistance;
            if (direction > 0) // Moving right
            {
                availableDistance = Math.Max(0, boundary.maxX - player.X);
            }
            else // Moving left
            {
                availableDistance = Math.Max(0, player.X - boundary.minX);
            }

            // Giới hạn khoảng cách
            int actualDistance = Math.Min(desiredDistance, availableDistance);

            // Giữ nguyên hướng (dấu)
            return actualDistance * Math.Sign(direction);
        }
        public void Cleanup()
        {
            p1ParryTimer?.Stop();
            p1ParryTimer?.Dispose();
            p1ParryCooldownTimer?.Stop();
            p1ParryCooldownTimer?.Dispose();
            p2ParryTimer?.Stop();
            p2ParryTimer?.Dispose();
            p2ParryCooldownTimer?.Stop();
            p2ParryCooldownTimer?.Dispose();
        }
    }
}
