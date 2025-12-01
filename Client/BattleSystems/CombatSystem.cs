using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

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

        private readonly Dictionary<string, Dictionary<string, float>> frameTimings = new Dictionary<string, Dictionary<string, float>>
        {
            ["goatman"] = new Dictionary<string, float>
            {
                ["punch"] = 1000f / 11f,
                ["kick"] = 1000f / 9f
            },
            ["girlknight"] = new Dictionary<string, float>
            {
                ["punch"] = 1000f / 6f,
                ["kick"] = 1000f / 6f,
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

        public CombatSystem(PlayerState p1, PlayerState p2, 
            CharacterAnimationManager p1AnimManager, CharacterAnimationManager p2AnimManager,
            EffectManager effectMgr, ProjectileManager projectileMgr,
            int playerWidth, int playerHeight, int backgroundWidth,
            Action invalidateCallback, Action<string, Color> showHitEffectCallback)
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
            parryTimer.Stop();
            parryTimer.Start();
            showHitEffectCallback?.Invoke("Parry!", Color.Cyan);
            invalidateCallback?.Invoke();
        }

        public void ExecuteAttack(int playerNum, string attackType)
        {
            PlayerState attacker = playerNum == 1 ? player1 : player2;
            PlayerState defender = playerNum == 1 ? player2 : player1;
            CharacterAnimationManager animMgr = playerNum == 1 ? player1AnimManager : player2AnimManager;

            if (!attacker.CanAttack || attacker.IsDashing || attacker.IsAttacking)
            {
                Console.WriteLine($"?? Player{playerNum} không th? attack!");
                return;
            }

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

            if (attackType == "punch")
            {
                attacker.Stamina = Math.Max(0, attacker.Stamina - 10);
                ExecutePunchAttack(playerNum, attacker, defender, animMgr);
            }
            else if (attackType == "kick")
            {
                attacker.Stamina = Math.Max(0, attacker.Stamina - 15);
                ExecuteKickAttack(playerNum, attacker, defender, animMgr);
            }
            else if (attackType == "special")
            {
                ExecuteSpecialAttack(playerNum, attacker, defender, animMgr);
            }

            invalidateCallback?.Invoke();
        }

        private void ExecutePunchAttack(int playerNum, PlayerState attacker, PlayerState defender, CharacterAnimationManager animMgr)
        {
            string charType = attacker.CharacterType;

            if (charType == "warrior")
            {
                int hitFrame6 = GetFrameTiming("warrior", "punch", 6);
                int hitFrame10 = GetFrameTiming("warrior", "punch", 10);

                var hitTimer1 = new Timer { Interval = hitFrame6 };
                hitTimer1.Tick += (s, e) =>
                {
                    hitTimer1.Stop();
                    hitTimer1.Dispose();
                    if (attacker.IsStunned || attacker.CurrentAnimation == "hurt")
                    {
                        Console.WriteLine($"?? Warrior punch hit 1 interrupted!");
                        return;
                    }
                    if (CheckAttackHit(attacker, defender))
                    {
                        ApplyDamage(playerNum == 1 ? 2 : 1, 10);
                        showHitEffectCallback?.Invoke("Strike!", Color.Yellow);
                    }
                };
                hitTimer1.Start();

                var hitTimer2 = new Timer { Interval = hitFrame10 };
                hitTimer2.Tick += (s, e) =>
                {
                    hitTimer2.Stop();
                    hitTimer2.Dispose();
                    if (attacker.IsStunned || attacker.CurrentAnimation == "hurt")
                    {
                        Console.WriteLine($"?? Warrior punch hit 2 interrupted!");
                        return;
                    }
                    if (CheckAttackHit(attacker, defender))
                    {
                        ApplyDamage(playerNum == 1 ? 2 : 1, 10);
                        showHitEffectCallback?.Invoke("Strike!", Color.Orange);
                    }
                };
                hitTimer2.Start();
            }
            else if (charType == "goatman")
            {
                int hitDelay = GetFrameTiming("goatman", "punch", 4);
                var hitTimer = new Timer { Interval = hitDelay };
                hitTimer.Tick += (s, e) =>
                {
                    hitTimer.Stop();
                    hitTimer.Dispose();
                    if (attacker.IsStunned || attacker.CurrentAnimation == "hurt")
                    {
                        Console.WriteLine($"?? Goatman punch interrupted!");
                        return;
                    }
                    if (CheckAttackHit(attacker, defender))
                    {
                        ApplyDamage(playerNum == 1 ? 2 : 1, 10);
                        showHitEffectCallback?.Invoke("Punch!", Color.Orange);
                    }
                };
                hitTimer.Start();
            }
            else if (charType == "girlknight")
            {
                int hitDelay = GetFrameTiming("girlknight", "punch", 3);
                var hitTimer = new Timer { Interval = hitDelay };
                hitTimer.Tick += (s, e) =>
                {
                    hitTimer.Stop();
                    hitTimer.Dispose();
                    if (attacker.IsStunned || attacker.CurrentAnimation == "hurt")
                    {
                        Console.WriteLine($"?? Girl Knight punch interrupted!");
                        return;
                    }
                    if (CheckAttackHit(attacker, defender))
                    {
                        ApplyDamage(playerNum == 1 ? 2 : 1, 10);
                        showHitEffectCallback?.Invoke("Punch!", Color.Pink);
                    }
                };
                hitTimer.Start();
            }
            else if (charType == "bringerofdeath")
            {
                int hitDelay = GetFrameTiming("bringerofdeath", "punch", 6);
                var hitTimer = new Timer { Interval = hitDelay };
                hitTimer.Tick += (s, e) =>
                {
                    hitTimer.Stop();
                    hitTimer.Dispose();
                    if (attacker.IsStunned || attacker.CurrentAnimation == "hurt")
                    {
                        Console.WriteLine($"?? Bringer punch interrupted!");
                        return;
                    }
                    if (CheckAttackHit(attacker, defender))
                    {
                        ApplyDamage(playerNum == 1 ? 2 : 1, 10);
                        showHitEffectCallback?.Invoke("Punch!", Color.Purple);
                    }
                };
                hitTimer.Start();
            }

            int duration = animMgr.GetAnimationDuration("punch");
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
                    if (attacker.IsStunned || attacker.CurrentAnimation == "hurt")
                    {
                        Console.WriteLine($"?? Bringer kick interrupted!");
                        return;
                    }
                    if (CheckAttackHit(attacker, defender))
                    {
                        ApplyDamage(playerNum == 1 ? 2 : 1, 15);
                        showHitEffectCallback?.Invoke("Kick!", Color.Purple);
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

                if (attacker.IsStunned || attacker.CurrentAnimation == "hurt")
                {
                    Console.WriteLine($"?? Goatman kick interrupted!");
                    return;
                }

                Rectangle attackHitbox = GetAttackHitbox(attacker);
                Rectangle targetHurtbox = GetPlayerHurtbox(defender);

                if (attackHitbox.IntersectsWith(targetHurtbox))
                {
                    int impactX = attacker.Facing == "right" ? attackHitbox.X + attackHitbox.Width : attackHitbox.X - 60;
                    int impactY = attackHitbox.Y;
                    effectManager.ShowImpactEffect(playerNum, impactX, impactY, attacker.Facing, invalidateCallback);
                    ApplyDamage(playerNum == 1 ? 2 : 1, 15, false);
                    int knockbackDir = attacker.Facing == "right" ? 1 : -1;
                    ApplyKnockback(defender, knockbackDir, knockbackDistance);
                    showHitEffectCallback?.Invoke("Heavy Impact!", Color.OrangeRed);
                }
            };
            hitTimer.Start();
        }

        private void ExecuteGirlKnightKick(int playerNum, PlayerState attacker, PlayerState defender, CharacterAnimationManager animMgr)
        {
            float msPerFrame = frameTimings["girlknight"]["kick"];
            int slideStartFrame = 6;
            int slideEndFrame = 9;
            int slideFrameCount = slideEndFrame - slideStartFrame;
            
            int slideStartTime = (int)(slideStartFrame * msPerFrame);
            int slideDuration = (int)(slideFrameCount * msPerFrame);
            int slideDistance = 400;
            
            Console.WriteLine($"?? Girl Knight Kick: Slide {slideFrameCount} frames @ {msPerFrame}ms/frame = {slideDuration}ms total");
            
            var slideStartTimer = new Timer { Interval = slideStartTime };
            slideStartTimer.Tick += (s, e) =>
            {
                slideStartTimer.Stop();
                slideStartTimer.Dispose();
                
                int slideDirection = attacker.Facing == "right" ? 1 : -1;
                int slideRemaining = slideDistance;
                int slideElapsed = 0;
                float slideSpeed = (float)slideDistance / slideDuration * 16;
                bool hasDamagedThisSlide = false;
                
                var slideTimer = new Timer { Interval = 16 };
                slideTimer.Tick += (s2, e2) =>
                {
                    slideElapsed += 16;
                    
                    if (attacker.IsStunned || attacker.CurrentAnimation == "hurt")
                    {
                        slideTimer.Stop();
                        slideTimer.Dispose();
                        Console.WriteLine($"?? Girl Knight slide interrupted by damage!");
                        return;
                    }
                    
                    if (slideElapsed < slideDuration && slideRemaining > 0)
                    {
                        int moveAmount = Math.Min(slideRemaining, (int)Math.Ceiling(slideSpeed));
                        attacker.X += moveAmount * slideDirection;
                        attacker.X = Math.Max(0, Math.Min(backgroundWidth - playerWidth, attacker.X));
                        slideRemaining -= moveAmount;
                        
                        if (!hasDamagedThisSlide && CheckAttackHit(attacker, defender))
                        {
                            ApplyDamage(playerNum == 1 ? 2 : 1, 15);
                            showHitEffectCallback?.Invoke("Slide Impact!", Color.Pink);
                            hasDamagedThisSlide = true;
                            Console.WriteLine($"? Girl Knight slide contact damage dealt!");
                        }
                    }
                    else
                    {
                        slideTimer.Stop();
                        slideTimer.Dispose();
                        Console.WriteLine($"? Girl Knight slide ended after {slideDuration}ms");
                    }
                };
                slideTimer.Start();
            };
            slideStartTimer.Start();
        }

        private void ExecuteWarriorKick(int playerNum, PlayerState attacker, PlayerState defender, CharacterAnimationManager animMgr)
        {
            float msPerFrame = frameTimings["warrior"]["kick"];
            int slideStartFrame = 1;
            int slideEndFrame = 3;
            int slideFrameCount = slideEndFrame - slideStartFrame;
            int hitFrame = 4;
            
            int slideDuration = (int)(slideFrameCount * msPerFrame);
            int hitTime = (int)(hitFrame * msPerFrame);
            int slideDistance = 400;
            
            Console.WriteLine($"?? Warrior Kick: Slide {slideFrameCount} frames @ {msPerFrame}ms/frame = {slideDuration}ms total");
            
            int slideDirection = attacker.Facing == "right" ? 1 : -1;
            int slideRemaining = slideDistance;
            int slideElapsed = 0;
            float slideSpeed = (float)slideDistance / slideDuration * 16;
            
            var slideTimer = new Timer { Interval = 16 };
            slideTimer.Tick += (s, e) =>
            {
                slideElapsed += 16;
                
                if (attacker.IsStunned || attacker.CurrentAnimation == "hurt")
                {
                    slideTimer.Stop();
                    slideTimer.Dispose();
                    Console.WriteLine($"?? Warrior slide interrupted by damage!");
                    return;
                }
                
                if (slideElapsed < slideDuration && slideRemaining > 0)
                {
                    int moveAmount = Math.Min(slideRemaining, (int)Math.Ceiling(slideSpeed));
                    attacker.X += moveAmount * slideDirection;
                    attacker.X = Math.Max(0, Math.Min(backgroundWidth - playerWidth, attacker.X));
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
            
            var hitTimer = new Timer { Interval = hitTime };
            hitTimer.Tick += (s, e) =>
            {
                hitTimer.Stop();
                hitTimer.Dispose();
                if (attacker.IsStunned || attacker.CurrentAnimation == "hurt")
                {
                    Console.WriteLine($"?? Warrior kick hit interrupted!");
                    return;
                }
                if (CheckAttackHit(attacker, defender))
                {
                    ApplyDamage(playerNum == 1 ? 2 : 1, 15);
                    showHitEffectCallback?.Invoke("Warrior Kick!", Color.Gold);
                }
            };
            hitTimer.Start();
        }

        private void ExecuteSpecialAttack(int playerNum, PlayerState attacker, PlayerState defender, CharacterAnimationManager animMgr)
        {
            if (!attacker.ConsumeMana(30))
            {
                attacker.IsAttacking = false;
                return;
            }

            attacker.CurrentAnimation = "fireball";
            animMgr.ResetAnimationToFirstFrame("fireball");

            string charType = attacker.CharacterType;

            if (charType == "bringerofdeath")
            {
                var castTimer = new Timer { Interval = 300 };
                castTimer.Tick += (s, e) =>
                {
                    castTimer.Stop();
                    castTimer.Dispose();
                    projectileManager.SpawnSpell(defender.X, defender.Y, playerNum, playerNum == 1 ? 2 : 1, ApplyDamage, showHitEffectCallback);
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
                    int direction = attacker.Facing == "right" ? 1 : -1;
                    int startX = attacker.Facing == "right" ? attacker.X + playerWidth : attacker.X;
                    int startY = attacker.Y + playerHeight / 2 - 80;
                    projectileManager.SpawnWarriorProjectile(startX, startY, direction, playerNum);
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

            if (player.IsAttacking || player.IsDashing || !player.CanDash)
            {
                Console.WriteLine($"?? Player{playerNum} không th? dash!");
                return;
            }

            if (!player.ConsumeStamina(20))
            {
                showHitEffectCallback?.Invoke("No Stamina!", Color.Gray);
                return;
            }

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
            int slideDirection = player.Facing == "right" ? 1 : -1;
            
            Console.WriteLine($"?? {player.CharacterType} smooth dash: {slideDistance}px in {slideDuration}ms");
            
            player.IsDashing = true;
            
            if (animMgr.HasAnimation("slide"))
            {
                player.CurrentAnimation = "slide";
                animMgr.ResetAnimationToFirstFrame("slide");
            }
            
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
                    player.X += moveAmount * slideDirection;
                    player.X = Math.Max(0, Math.Min(backgroundWidth - playerWidth, player.X));
                    slideRemaining -= moveAmount;
                }
                else
                {
                    slideTimer.Stop();
                    slideTimer.Dispose();
                    player.IsDashing = false;
                    
                    if (!player.IsAttacking && !player.IsJumping && !player.IsStunned)
                        player.ResetToIdle();
                    
                    Console.WriteLine($"? {player.CharacterType} dash ended after {slideDuration}ms");
                }
            };
            slideTimer.Start();
        }

        private void ExecuteTeleportDash(int playerNum, PlayerState player, CharacterAnimationManager animMgr)
        {
            // ? TELEPORT DASH with disappear effect
            // Step 1: Show dash effect at current position
            // Step 2: Player disappears (invisible) for 0.3s with iframe
            // Step 3: Player reappears at destination after 0.3s
            
            int startX = player.X;
            int startY = player.Y;
            string facing = player.Facing;
            
            // ? Calculate destination
            player.IsDashing = true;
            int dashDirection = player.Facing == "right" ? 1 : -1;
            int destinationX = player.X + (DASH_DISTANCE * dashDirection);
            destinationX = Math.Max(0, Math.Min(backgroundWidth - playerWidth, destinationX));
            
            Console.WriteLine($"? {player.CharacterType} teleport dash: {DASH_DISTANCE}px instant");
            
            // ? STEP 1: Show dash effect at START position (0.15s duration)
            effectManager.ShowDashEffect(playerNum, startX, startY, facing, invalidateCallback);
            
            // ? STEP 2: Make player INVISIBLE (hide for 0.3s)
            string originalAnimation = player.CurrentAnimation;
            player.CurrentAnimation = "invisible"; // Special state to hide player
            
            // ? STEP 3: Move to destination (but still invisible)
            player.X = destinationX;
            
            invalidateCallback?.Invoke();
            
            // ? STEP 4: After 0.3s, make player visible again at destination
            var reappearTimer = new Timer { Interval = SMOOTH_DASH_DURATION_MS }; // 300ms
            reappearTimer.Tick += (s, e) =>
            {
                reappearTimer.Stop();
                reappearTimer.Dispose();
                
                // ? Make player visible again
                player.IsDashing = false; // IFRAME OFF
                player.CurrentAnimation = "idle"; // Back to normal
                
                if (!player.IsAttacking && !player.IsJumping && !player.IsStunned)
                    player.ResetToIdle();
                
                invalidateCallback?.Invoke();
                Console.WriteLine($"? {player.CharacterType} reappeared at X={player.X}");
            };
            reappearTimer.Start();
        }

        public void ToggleSkill(int playerNum)
        {
            PlayerState player = playerNum == 1 ? player1 : player2;
            if (player.IsAttacking) return;

            if (player.CharacterType == "girlknight")
                ToggleKnightGirlSkill(playerNum, player);
            else if (player.CharacterType == "goatman")
                ExecuteGoatmanCharge(playerNum, player);
            else if (player.Mana >= 30)
                ExecuteAttack(playerNum, "special");
        }

        private void ToggleKnightGirlSkill(int playerNum, PlayerState player)
        {
            CharacterAnimationManager animMgr = playerNum == 1 ? player1AnimManager : player2AnimManager;
            PlayerState opponent = playerNum == 1 ? player2 : player1;

            if (!player.IsSkillActive)
            {
                if (!player.ConsumeMana(30))
                {
                    showHitEffectCallback?.Invoke("Not enough mana!", Color.Gray);
                    return;
                }

                player.IsSkillActive = true;
                player.CurrentAnimation = "fireball";
                animMgr.ResetAnimationToFirstFrame("fireball");

                var skillTimer = new Timer { Interval = 1000 };
                skillTimer.Tick += (s, e) =>
                {
                    player.Mana -= 30;

                    var halfSecondTimer = new Timer { Interval = 500 };
                    halfSecondTimer.Tick += (s2, e2) =>
                    {
                        halfSecondTimer.Stop();
                        halfSecondTimer.Dispose();
                        if (CheckSkillHit(player, opponent))
                        {
                            ApplyDamage(playerNum == 1 ? 2 : 1, 5, false);
                            showHitEffectCallback?.Invoke("Energy!", Color.Cyan);
                        }
                    };
                    halfSecondTimer.Start();

                    var oneSecondTimer = new Timer { Interval = 1000 };
                    oneSecondTimer.Tick += (s3, e3) =>
                    {
                        oneSecondTimer.Stop();
                        oneSecondTimer.Dispose();
                        if (CheckSkillHit(player, opponent))
                        {
                            ApplyDamage(playerNum == 1 ? 2 : 1, 5, false);
                            showHitEffectCallback?.Invoke("Energy!", Color.Cyan);
                        }
                    };
                    oneSecondTimer.Start();

                    if (player.Mana < 30)
                    {
                        player.IsSkillActive = false;
                        skillTimer.Stop();
                        skillTimer.Dispose();
                        if (!player.IsAttacking && !player.IsJumping)
                            player.ResetToIdle();
                        showHitEffectCallback?.Invoke("Out of Mana!", Color.Gray);
                    }
                };
                skillTimer.Start();
                showHitEffectCallback?.Invoke("Energy Shield!", Color.Cyan);
            }
            else
            {
                player.IsSkillActive = false;
                if (!player.IsAttacking && !player.IsJumping)
                    player.ResetToIdle();
            }
        }

        private void ExecuteGoatmanCharge(int playerNum, PlayerState player)
        {
            CharacterAnimationManager animMgr = playerNum == 1 ? player1AnimManager : player2AnimManager;
            PlayerState opponent = playerNum == 1 ? player2 : player1;

            if (!player.ConsumeMana(30)) return;

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
                    player.ChargeSpeed += CHARGE_ACCELERATION / (1000f / 16);
                    player.ChargeSpeed = Math.Min(CHARGE_MAX_SPEED, player.ChargeSpeed);
                }
                else
                {
                    player.ChargeSpeed = CHARGE_MAX_SPEED;
                }

                player.X += (int)(player.ChargeSpeed * chargeDirection);
                player.X = Math.Max(0, Math.Min(backgroundWidth - playerWidth, player.X));

                Rectangle chargeHitbox = GetPlayerHurtbox(player);
                Rectangle targetHurtbox = GetPlayerHurtbox(opponent);

                if (chargeHitbox.IntersectsWith(targetHurtbox))
                {
                    int impactX = player.Facing == "right" ? chargeHitbox.X + chargeHitbox.Width : chargeHitbox.X - 60;
                    int impactY = chargeHitbox.Y;
                    effectManager.ShowImpactEffect(playerNum, impactX, impactY, player.Facing, invalidateCallback);
                    ApplyDamage(playerNum == 1 ? 2 : 1, 25, false);
                    showHitEffectCallback?.Invoke("Charged!", Color.Gold);

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
            showHitEffectCallback?.Invoke($"-{damage}", Color.Red);
            effectManager.ShowHitEffectAtPosition(target.X, target.Y, invalidateCallback);
            
            target.IsStunned = true;
            CancelAttack(targetPlayer);
            
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
                Console.WriteLine($"?? Player{targetPlayer} charge INTERRUPTED by damage!");
                showHitEffectCallback?.Invoke("Interrupted!", Color.Orange);
            }

            target.CurrentAnimation = "hurt";
            CharacterAnimationManager targetAnimMgr = targetPlayer == 1 ? player1AnimManager : player2AnimManager;
            targetAnimMgr.ResetAnimationToFirstFrame("hurt");

            if (knockback)
            {
                int kb = (attacker.X > target.X) ? -20 : 20;
                target.X = Math.Max(0, Math.Min(backgroundWidth - playerWidth, target.X + kb));
            }

            invalidateCallback?.Invoke();

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

        private void CancelAttack(int playerNum)
        {
            PlayerState player = playerNum == 1 ? player1 : player2;
            player.IsAttacking = false;
            player.IsSkillActive = false;
            player.IsCharging = false;
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

        private bool CheckAttackHit(PlayerState attacker, PlayerState defender)
        {
            Rectangle attackHitbox = GetAttackHitbox(attacker);
            Rectangle targetHurtbox = GetPlayerHurtbox(defender);
            return attackHitbox.IntersectsWith(targetHurtbox);
        }

        private bool CheckSkillHit(PlayerState player, PlayerState opponent)
        {
            Rectangle skillRange = new Rectangle(player.X - 50, player.Y - 50, playerWidth + 100, playerHeight + 100);
            Rectangle targetRect = GetPlayerHurtbox(opponent);
            return skillRange.IntersectsWith(targetRect);
        }

        private Rectangle GetAttackHitbox(PlayerState player)
        {
            int hitboxWidth = playerWidth / 2;
            int hitboxHeight = playerHeight / 2;
            int hitboxY = player.Y + (playerHeight - hitboxHeight) / 2;
            int hitboxX = player.Facing == "right" ? player.X + playerWidth / 2 : player.X + playerWidth / 2 - hitboxWidth;
            return new Rectangle(hitboxX, hitboxY, hitboxWidth, hitboxHeight);
        }

        private Rectangle GetPlayerHurtbox(PlayerState player)
        {
            return new Rectangle(player.X, player.Y, playerWidth, playerHeight);
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
