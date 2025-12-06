using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace DoAn_NT106.Client.BattleSystems
{
    public class ProjectileManager
    {
        public class WarriorProjectile
        {
            public bool IsActive { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Direction { get; set; }
            public int Owner { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        private List<WarriorProjectile> activeWarriorProjectiles = new List<WarriorProjectile>();

        // Spell (Bringer of Death)
        public bool SpellActive { get; set; }
        public int SpellX { get; set; }
        public int SpellY { get; set; }
        public int SpellOwner { get; set; }
        private Image spellAnimation;
        // Scale up 1.5x (from previous 500x500)
        private const int SPELL_WIDTH = 750;
        private const int SPELL_HEIGHT = 750;
        private const int SPELL_DAMAGE_DELAY_MS = 400; // 0.4s
        private Timer spellDamageTimer;
        private int spellTargetPlayer = 0;
        private Func<int, Rectangle> getTargetHurtbox;
        // Narrower hitbox width centered on spell (for collision)
        private const int SPELL_HITBOX_WIDTH = 100;
        private const int SPELL_HITBOX_HEIGHT = 200; // reduced height per request

        private Image warriorSkillEffect;
        private const int PROJECTILE_SPEED = 23;
        private const int PROJECTILE_WIDTH = 160;
        private const int PROJECTILE_HEIGHT = 160;
        private const int PROJECTILE_LIFETIME_MS = 4000;

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
                try
                {
                    spellAnimation = ResourceToImage(Properties.Resources.BringerofDeath_Spell);
                    if (spellAnimation != null && ImageAnimator.CanAnimate(spellAnimation))
                    {
                        ImageAnimator.Animate(spellAnimation, frameChangedHandler);
                        Console.WriteLine("✓ Spell animation loaded");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Spell load error: {ex.Message}");
                    spellAnimation = CreateColoredImage(SPELL_WIDTH, SPELL_HEIGHT, Color.Purple);
                }

                try
                {
                    var warriorEffect = ResourceToImage(Properties.Resources.Warrior_skill_effect);
                    if (warriorEffect != null)
                    {
                        warriorSkillEffect = warriorEffect;
                        if (ImageAnimator.CanAnimate(warriorSkillEffect))
                        {
                            ImageAnimator.Animate(warriorSkillEffect, frameChangedHandler);
                            Console.WriteLine("✓ Warrior effect loaded");
                        }
                    }
                    else
                    {
                        warriorSkillEffect = CreateColoredImage(PROJECTILE_WIDTH, PROJECTILE_HEIGHT, Color.Gold);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Warrior effect error: {ex.Message}");
                    warriorSkillEffect = CreateColoredImage(PROJECTILE_WIDTH, PROJECTILE_HEIGHT, Color.Gold);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ LoadProjectileImages error: {ex.Message}");
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
        /// Spawn spell centered on target hurtbox (with offsets). Resets animation on summon.
        /// </summary>
        public void SpawnSpell(int targetPlayer, int owner,
            Func<int, Rectangle> getTargetHurtbox,
            Action<int, int, bool> applyHurtCallback, Action<string, Color> showHitEffectCallback)
        {
            this.getTargetHurtbox = getTargetHurtbox;
            this.spellTargetPlayer = targetPlayer;

            var hb = getTargetHurtbox?.Invoke(targetPlayer) ?? Rectangle.Empty;
            if (hb == Rectangle.Empty)
            {
                Console.WriteLine("⚠️ SpawnSpell: target hurtbox empty");
                return;
            }
            int centerX = hb.X + hb.Width / 2;
            int centerY = hb.Y + hb.Height / 2;

            // Apply adjustments per latest request: shift gif right 15px, lower 20px
            centerX += 15; // shift gif to the right
            centerY -= 200;
            SpellActive = true;
            SpellOwner = owner;
            SpellX = centerX - 10 + 20 - 50; // move GIF 50px left
            SpellY = centerY + 20; // unchanged vertical offset for GIF

            // Reset animation frames for spell
            try
            {
                if (spellAnimation != null && ImageAnimator.CanAnimate(spellAnimation))
                {
                    ImageAnimator.StopAnimate(spellAnimation, frameChangedHandler);
                    ImageAnimator.Animate(spellAnimation, frameChangedHandler);
                }
            }
            catch { }

            Console.WriteLine($"✓ Spell spawned (adjusted) at CX={centerX}, CY={centerY}");

            spellDamageTimer?.Stop();
            spellDamageTimer?.Dispose();

            spellDamageTimer = new Timer { Interval = SPELL_DAMAGE_DELAY_MS };
            spellDamageTimer.Tick += (s, e) =>
            {
                spellDamageTimer.Stop();
                spellDamageTimer.Dispose();

                try
                {
                    var currentHb = getTargetHurtbox?.Invoke(spellTargetPlayer) ?? Rectangle.Empty;
                    int offsetX = (SPELL_WIDTH - 120) / 2;
                    int offsetY = (SPELL_HEIGHT - 120) / 2;
                    Rectangle spellRect = new Rectangle(SpellX - offsetX, SpellY - offsetY, SPELL_WIDTH, SPELL_HEIGHT);

                    // Hitbox shifted right 50px and down 100px, then move left 10px overall, plus global +20px shift, and 50px left
                    int narrowX = SpellX + (50 - 10) + 20 - 50 - (SPELL_HITBOX_WIDTH / 2);
                    int narrowY = SpellY + 100 - (SPELL_HITBOX_HEIGHT / 2);
                    Rectangle spellHitRect = new Rectangle(narrowX, narrowY, SPELL_HITBOX_WIDTH, SPELL_HITBOX_HEIGHT);

                    bool hit = !currentHb.IsEmpty && spellHitRect.IntersectsWith(currentHb);
                    Console.WriteLine($"[Spell Damage] Hit={(hit ? "YES" : "NO")}, SpellHitRect=({spellHitRect.X},{spellHitRect.Y},{spellHitRect.Width}x{spellHitRect.Height}), TargetHb=({currentHb.X},{currentHb.Y},{currentHb.Width}x{currentHb.Height})");

                    if (hit)
                    {
                        applyHurtCallback?.Invoke(spellTargetPlayer, 25, false);
                        showHitEffectCallback?.Invoke("Spell Damage!", Color.Purple);
                    }
                }
                finally
                {
                    var removeTimer = new Timer { Interval = 500 };
                    removeTimer.Tick += (s2, e2) =>
                    {
                        removeTimer.Stop();
                        removeTimer.Dispose();
                        SpellActive = false;
                    };
                    removeTimer.Start();
                }
            };
            spellDamageTimer.Start();
        }

        public void SpawnWarriorProjectile(int x, int y, int direction, int owner)
        {
            activeWarriorProjectiles.Add(new WarriorProjectile
            {
                IsActive = true,
                X = x,
                Y = y,
                Direction = direction,
                Owner = owner,
                CreatedAt = DateTime.UtcNow
            });

            Console.WriteLine($"✓ Warrior projectile at X={x}, Y={y} by player {owner}, dir={(direction > 0 ? "right" : "left")}");
        }

        public void UpdateFireball(Func<int, int, int, int, Rectangle> getPlayerHurtbox,
            Func<int, bool> checkParrying, Action reflectFireball, Action<int, int> applyHurt,
            Action<string, Color> showHitEffect)
        {
            // Not implemented
        }

        public void UpdateWarriorProjectiles(Func<int, int, int, int, Rectangle> getPlayerHurtbox,
            Action<int, int> applyHurt, Action<string, Color> showHitEffect)
        {
            for (int i = activeWarriorProjectiles.Count - 1; i >= 0; i--)
            {
                var proj = activeWarriorProjectiles[i];

                var ageMs = (int)(DateTime.UtcNow - proj.CreatedAt).TotalMilliseconds;
                if (ageMs >= PROJECTILE_LIFETIME_MS)
                {
                    activeWarriorProjectiles.RemoveAt(i);
                    continue;
                }

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

            if (warriorSkillEffect != null && ImageAnimator.CanAnimate(warriorSkillEffect))
            {
                ImageAnimator.UpdateFrames(warriorSkillEffect);
            }
        }

        public void UpdateSpellAnimation()
        {
            if (SpellActive && spellAnimation != null && ImageAnimator.CanAnimate(spellAnimation))
            {
                ImageAnimator.UpdateFrames(spellAnimation);
            }
        }

        public void DrawProjectiles(Graphics g, int viewportX)
        {
            if (SpellActive && spellAnimation != null)
            {
                int spellScreenX = SpellX - viewportX;
                if (spellScreenX >= -SPELL_WIDTH && spellScreenX <= g.ClipBounds.Width)
                {
                    int offsetX = (SPELL_WIDTH - 120) / 2;
                    int offsetY = (SPELL_HEIGHT - 120) / 2;

                    g.DrawImage(spellAnimation,
                        spellScreenX - offsetX,
                        SpellY - offsetY,
                        SPELL_WIDTH,
                        SPELL_HEIGHT);

                    // Debug: draw spell drawn rect
                    using (var pen = new Pen(Color.Purple, 2))
                    {
                        g.DrawRectangle(pen, spellScreenX - offsetX, SpellY - offsetY, SPELL_WIDTH, SPELL_HEIGHT);
                    }
                    // Debug: draw narrow hitbox (shifted 40px to the left)
                    using (var pen2 = new Pen(Color.Magenta, 2))
                    {
                        int narrowX = SpellX - viewportX + (50 - 10) + 20 - 50 - (SPELL_HITBOX_WIDTH / 2);
                        int narrowY = SpellY + 100 - (SPELL_HITBOX_HEIGHT / 2);
                        g.DrawRectangle(pen2, narrowX, narrowY, SPELL_HITBOX_WIDTH, SPELL_HITBOX_HEIGHT);
                    }
                }
            }

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

                    using (var pen = new Pen(Color.Gold, 2))
                    {
                        g.DrawRectangle(pen, projScreenX, proj.Y, PROJECTILE_WIDTH, PROJECTILE_HEIGHT);
                    }
                }
            }
        }

        public void ShootFireball(int x, int y, int direction, int owner)
        {
            // Fireball projectile not implemented in this build. Log for debug.
            Console.WriteLine($"[ShootFireball] Requested by player {owner} at X={x}, Y={y}, dir={(direction>0?"right":"left")}");
        }

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
