using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace DoAn_NT106.Client.BattleSystems
{
    /// <summary>
    /// Qu?n lý các projectile: fireball, spell, warrior projectiles
    /// </summary>
    public class ProjectileManager
    {
        // Warrior Projectile
        public class WarriorProjectile
        {
            public bool IsActive { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Direction { get; set; }
            public int Owner { get; set; }
        }

        private List<WarriorProjectile> activeWarriorProjectiles = new List<WarriorProjectile>();

        // Spell (Bringer of Death)
        public bool SpellActive { get; set; }
        public int SpellX { get; set; }
        public int SpellY { get; set; }
        public int SpellOwner { get; set; }
        private Image spellAnimation;
        private const int SPELL_WIDTH = 500; // ? 500x500 (to rõ)
        private const int SPELL_HEIGHT = 500;
        private const int SPELL_DAMAGE_DELAY_MS = 200;
        private Timer spellDamageTimer;

        private Image warriorSkillEffect;
        private const int PROJECTILE_SPEED = 23;
        private const int PROJECTILE_WIDTH = 160;
        private const int PROJECTILE_HEIGHT = 160;

        private int backgroundWidth;
        private EventHandler frameChangedHandler;

        public ProjectileManager(int backgroundWidth, EventHandler onFrameChanged)
        {
            this.backgroundWidth = backgroundWidth;
            this.frameChangedHandler = onFrameChanged;
            LoadProjectileImages();
        }

        private void LoadProjectileImages()
        {
            try
            {
                // Load spell animation (Bringer of Death)
                try
                {
                    spellAnimation = ResourceToImage(Properties.Resources.BringerofDeath_Spell);
                    if (spellAnimation != null && ImageAnimator.CanAnimate(spellAnimation))
                    {
                        ImageAnimator.Animate(spellAnimation, frameChangedHandler);
                        Console.WriteLine("? Spell animation loaded (500x500)");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"?? Spell load error: {ex.Message}");
                    spellAnimation = CreateColoredImage(SPELL_WIDTH, SPELL_HEIGHT, Color.Purple);
                }

                // Load warrior projectile
                try
                {
                    var warriorEffect = ResourceToImage(Properties.Resources.Warrior_skill_effect);
                    if (warriorEffect != null)
                    {
                        warriorSkillEffect = warriorEffect;
                        if (ImageAnimator.CanAnimate(warriorSkillEffect))
                        {
                            ImageAnimator.Animate(warriorSkillEffect, frameChangedHandler);
                            Console.WriteLine("? Warrior effect loaded");
                        }
                    }
                    else
                    {
                        warriorSkillEffect = CreateColoredImage(PROJECTILE_WIDTH, PROJECTILE_HEIGHT, Color.Gold);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"?? Warrior effect error: {ex.Message}");
                    warriorSkillEffect = CreateColoredImage(PROJECTILE_WIDTH, PROJECTILE_HEIGHT, Color.Gold);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? LoadProjectileImages error: {ex.Message}");
            }
        }

        private Image ResourceToImage(object res)
        {
            if (res == null) return null;

            if (res is Image img)
            {
                try
                {
                    return ImageAnimator.CanAnimate(img) ? img : new Bitmap(img);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ResourceToImage(Image) error: {ex}");
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
                    Console.WriteLine($"ResourceToImage(byte[]) error: {ex}");
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Spawn spell at target position
        /// </summary>
        public void SpawnSpell(int targetX, int targetY, int owner, int targetPlayer, 
            Action<int, int, bool> applyHurtCallback, Action<string, Color> showHitEffectCallback)
        {
            SpellActive = true;
            SpellX = targetX;
            SpellY = targetY;
            SpellOwner = owner;

            Console.WriteLine($"? Spell spawned at X={targetX}, Y={targetY} (500x500)");

            spellDamageTimer?.Stop();
            spellDamageTimer?.Dispose();

            spellDamageTimer = new Timer { Interval = SPELL_DAMAGE_DELAY_MS };
            spellDamageTimer.Tick += (s, e) =>
            {
                spellDamageTimer.Stop();
                spellDamageTimer.Dispose();

                applyHurtCallback?.Invoke(targetPlayer, 25, false);
                showHitEffectCallback?.Invoke("Spell Damage!", Color.Purple);

                var removeTimer = new Timer { Interval = 500 };
                removeTimer.Tick += (s2, e2) =>
                {
                    removeTimer.Stop();
                    removeTimer.Dispose();
                    SpellActive = false;
                };
                removeTimer.Start();
            };
            spellDamageTimer.Start();
        }

        /// <summary>
        /// Spawn warrior projectile
        /// </summary>
        public void SpawnWarriorProjectile(int x, int y, int direction, int owner)
        {
            activeWarriorProjectiles.Add(new WarriorProjectile
            {
                IsActive = true,
                X = x,
                Y = y + 80, // ? THÊM 20PX N?A: Y+60 ? Y+80
                Direction = direction,
                Owner = owner
            });

            Console.WriteLine($"? Warrior projectile at X={x}, Y={y + 80} by player {owner}");
        }

        public void ShootFireball(int x, int y, int direction, int owner)
        {
            // Not implemented
        }

        public void UpdateFireball(Func<int, int, int, int, Rectangle> getPlayerHurtbox,
            Func<int, bool> checkParrying, Action reflectFireball, Action<int, int> applyHurt, 
            Action<string, Color> showHitEffect)
        {
            // Not implemented
        }

        /// <summary>
        /// Update warrior projectiles
        /// </summary>
        public void UpdateWarriorProjectiles(Func<int, int, int, int, Rectangle> getPlayerHurtbox,
            Action<int, int> applyHurt, Action<string, Color> showHitEffect)
        {
            for (int i = activeWarriorProjectiles.Count - 1; i >= 0; i--)
            {
                var proj = activeWarriorProjectiles[i];

                proj.X += PROJECTILE_SPEED * proj.Direction;

                int targetPlayer = proj.Owner == 1 ? 2 : 1;
                Rectangle projRect = new Rectangle(proj.X, proj.Y, PROJECTILE_WIDTH, PROJECTILE_HEIGHT);
                Rectangle targetRect = getPlayerHurtbox(targetPlayer, 0, 0, 0);

                if (projRect.IntersectsWith(targetRect))
                {
                    applyHurt?.Invoke(targetPlayer, 20);
                    showHitEffect?.Invoke("Energy Strike!", Color.Gold);
                    activeWarriorProjectiles.RemoveAt(i);
                    continue;
                }

                if (proj.X > backgroundWidth || proj.X < -PROJECTILE_WIDTH)
                {
                    activeWarriorProjectiles.RemoveAt(i);
                }
            }

            // Update animation frames
            if (warriorSkillEffect != null && ImageAnimator.CanAnimate(warriorSkillEffect))
            {
                ImageAnimator.UpdateFrames(warriorSkillEffect);
            }
        }

        /// <summary>
        /// Update spell animation
        /// </summary>
        public void UpdateSpellAnimation()
        {
            if (SpellActive && spellAnimation != null && ImageAnimator.CanAnimate(spellAnimation))
            {
                ImageAnimator.UpdateFrames(spellAnimation);
            }
        }

        /// <summary>
        /// Draw projectiles
        /// </summary>
        public void DrawProjectiles(Graphics g, int viewportX)
        {
            // Draw spell (Bringer of Death) - 500x500
            if (SpellActive && spellAnimation != null)
            {
                int spellScreenX = SpellX - viewportX;
                if (spellScreenX >= -SPELL_WIDTH && spellScreenX <= g.ClipBounds.Width)
                {
                    // Center spell (offset for size increase from 120 to 500)
                    int offsetX = (SPELL_WIDTH - 120) / 2; // (500-120)/2 = 190
                    int offsetY = (SPELL_HEIGHT - 120) / 2;
                    
                    g.DrawImage(spellAnimation, 
                        spellScreenX - offsetX, 
                        SpellY - offsetY, 
                        SPELL_WIDTH, 
                        SPELL_HEIGHT);
                }
            }

            // Draw warrior projectiles
            foreach (var proj in activeWarriorProjectiles)
            {
                if (!proj.IsActive) continue;

                int projScreenX = proj.X - viewportX;
                if (projScreenX >= -PROJECTILE_WIDTH && projScreenX <= g.ClipBounds.Width)
                {
                    if (warriorSkillEffect != null)
                    {
                        if (proj.Direction == -1)
                        {
                            g.DrawImage(warriorSkillEffect,
                                new Rectangle(projScreenX + PROJECTILE_WIDTH, proj.Y, -PROJECTILE_WIDTH, PROJECTILE_HEIGHT),
                                new Rectangle(0, 0, warriorSkillEffect.Width, warriorSkillEffect.Height),
                                GraphicsUnit.Pixel);
                        }
                        else
                        {
                            g.DrawImage(warriorSkillEffect, projScreenX, proj.Y, PROJECTILE_WIDTH, PROJECTILE_HEIGHT);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cleanup all projectiles
        /// </summary>
        public void Cleanup()
        {
            activeWarriorProjectiles.Clear();
            spellDamageTimer?.Stop();
            spellDamageTimer?.Dispose();
            SpellActive = false;
        }

        private Bitmap CreateColoredImage(int width, int height, Color color)
        {
            var bmp = new Bitmap(Math.Max(1, width), Math.Max(1, height));
            using (var g = Graphics.FromImage(bmp))
            using (var brush = new SolidBrush(color))
                g.FillRectangle(brush, 0, 0, bmp.Width, bmp.Height);
            return bmp;
        }
    }
}
