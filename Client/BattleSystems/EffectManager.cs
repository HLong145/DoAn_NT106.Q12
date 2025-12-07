using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace DoAn_NT106.Client.BattleSystems
{
    // Helper class for hit effects
    public class HitEffectInstance
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Timer Timer { get; set; }
        public Image EffectImage { get; set; }
    }

    /// <summary>
    /// Quản lý các hiệu ứng visual: hit effects, dash effects, impacts
    /// </summary>
    public class EffectManager
    {
        private List<HitEffectInstance> activeHitEffects = new List<HitEffectInstance>();
        private Image hitEffectImage; // base reference
        private Image dashEffectImage; // GIF effect for dash
        private Image gmImpactEffect;

        public class DashEffectInstance
        {
            public int X { get; set; }
            public int Y { get; set; }
            public string Facing { get; set; }
            public Timer Timer { get; set; }
            public bool IsActive { get; set; }
        }

        private List<DashEffectInstance> activeDashEffects = new List<DashEffectInstance>();

        public bool Impact1Active { get; set; }
        public bool Impact2Active { get; set; }
        public int Impact1X { get; set; }
        public int Impact1Y { get; set; }
        public int Impact2X { get; set; }
        public int Impact2Y { get; set; }
        public string Impact1Facing { get; set; }
        public string Impact2Facing { get; set; }
        public Timer Impact1Timer { get; set; }
        public Timer Impact2Timer { get; set; }

        private const int HIT_EFFECT_DURATION_MS = 300; // 0.3s
        private const int DASH_EFFECT_DURATION_MS = 150; // 0.15s duration
        private const int GM_IMPACT_DURATION_MS = 200;

        public EffectManager()
        {
            LoadEffectImages();
            Impact1Timer = new Timer();
            Impact2Timer = new Timer();
        }

        private void LoadEffectImages()
        {
            try
            {
                // TRY TO LOAD DASH EFFECT GIF từ Resources
                try
                {
                    var dashResource = Properties.Resources.ResourceManager.GetObject("Dash_effect")
                                    ?? Properties.Resources.ResourceManager.GetObject("dash_effect");
                    
                    if (dashResource != null)
                    {
                        dashEffectImage = ResourceToImage(dashResource);
                        if (dashEffectImage != null && ImageAnimator.CanAnimate(dashEffectImage))
                        {
                            ImageAnimator.Animate(dashEffectImage, (s, e) => { }); // Auto-animate
                            Console.WriteLine("✓ Loaded dash effect GIF from Resources");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Could not load Dash_effect from Resources: {ex.Message}");
                }

                // FALLBACK: Create programmatic dash effect
                if (dashEffectImage == null)
                {
                    Console.WriteLine("⚠️ Creating fallback dash effect");
                    dashEffectImage = CreateDashEffectImage();
                }

                // Load hit_effect GIF reference (for cloning per spawn)
                try
                {
                    hitEffectImage = ResourceToImage(Properties.Resources.hit_effect);
                    if (hitEffectImage != null && ImageAnimator.CanAnimate(hitEffectImage))
                    {
                        // Pre-animate reference (instances will be reset per spawn)
                        ImageAnimator.Animate(hitEffectImage, (s, e) => { });
                        Console.WriteLine("✓ Loaded hit_effect GIF from Resources");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Could not load hit_effect from Resources: {ex.Message}");
                    hitEffectImage = CreateColoredImage(50, 50, Color.FromArgb(220, Color.Red));
                }

                // Try to load GM_impact from resources (gif as byte[])
                try
                {
                    var gmRes = Properties.Resources.GM_impact; // byte[] expected
                    gmImpactEffect = ResourceToImage(gmRes);
                    if (gmImpactEffect != null && ImageAnimator.CanAnimate(gmImpactEffect))
                    {
                        // Pre-animate; we'll reset on each show
                        ImageAnimator.Animate(gmImpactEffect, (s, e) => { });
                        Console.WriteLine("✓ Loaded GM_impact GIF from Resources");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Could not load GM_impact from Resources: {ex.Message}");
                }

                // Fallback if resource not found
                if (gmImpactEffect == null)
                {
                    gmImpactEffect = CreateColoredImage(160, 160, Color.FromArgb(255, 255, 80, 0));
                    Console.WriteLine("⚠️ Using fallback GM_impact rectangle");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❗ Error loading effect images: {ex.Message}");
            }
        }

        private Image CreateDashEffectImage()
        {
            var bmp = new Bitmap(80, 40);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                
                // Draw multiple motion lines (speed lines effect)
                using (var brush = new SolidBrush(Color.FromArgb(180, Color.White)))
                using (var pen = new Pen(Color.FromArgb(220, Color.Cyan), 2))
                {
                    // Draw 3 horizontal speed lines
                    for (int i = 0; i < 3; i++)
                    {
                        int y = 10 + i * 10;
                        g.DrawLine(pen, 0, y, 60 - i * 15, y);
                    }
                    
                    // Add small particles
                    for (int i = 0; i < 5; i++)
                    {
                        int x = i * 15;
                        int y = 15 + i * 5;
                        g.FillEllipse(brush, x, y, 4, 4);
                    }
                }
            }
            return bmp;
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
        /// Show hit effect at player position with per-character horizontal offset
        /// </summary>
        public void ShowHitEffectAtPosition(string characterType, int x, int y, Action invalidateCallback)
        {
            // Apply horizontal offset by character type
            int offsetX = 0;
            if (string.Equals(characterType, "girlknight", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(characterType, "goatman", StringComparison.OrdinalIgnoreCase))
            {
                offsetX = -50; // shift left
            }
            else if (string.Equals(characterType, "bringerofdeath", StringComparison.OrdinalIgnoreCase))
            {
                offsetX = 150; // shift right
            }

            // Create a fresh instance of the GIF to guarantee starting at first frame
            Image instance = null;
            try
            {
                instance = ResourceToImage(Properties.Resources.hit_effect);
                if (instance != null && ImageAnimator.CanAnimate(instance))
                {
                    // Reset/animate per instance
                    ImageAnimator.Animate(instance, (s, e) => { invalidateCallback?.Invoke(); });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ hit_effect reload error: {ex.Message}");
                instance = CreateColoredImage(50, 50, Color.FromArgb(220, Color.Red));
            }

            var effect = new HitEffectInstance
            {
                X = x + offsetX,
                Y = y,
                EffectImage = instance ?? hitEffectImage,
                Timer = new Timer { Interval = HIT_EFFECT_DURATION_MS }
            };

            effect.Timer.Tick += (s, e) =>
            {
                effect.Timer.Stop();
                effect.Timer.Dispose();
                activeHitEffects.Remove(effect);
                invalidateCallback?.Invoke();
            };

            activeHitEffects.Add(effect);
            effect.Timer.Start();
            invalidateCallback?.Invoke();
        }

        /// <summary>
        /// Show dash effect at position (for teleport dash)
        /// </summary>
        public void ShowDashEffect(int playerNum, int x, int y, string facing, Action invalidateCallback)
        {
            var effect = new DashEffectInstance
            {
                X = x,
                Y = y,
                Facing = facing,
                IsActive = true,
                Timer = new Timer { Interval = DASH_EFFECT_DURATION_MS }
            };

            effect.Timer.Tick += (s, e) =>
            {
                effect.Timer.Stop();
                effect.Timer.Dispose();
                effect.IsActive = false;
                activeDashEffects.Remove(effect);
                invalidateCallback?.Invoke();
                Console.WriteLine($"✓ Dash effect expired for player {playerNum}");
            };

            activeDashEffects.Add(effect);
            effect.Timer.Start();
            invalidateCallback?.Invoke();
            
            Console.WriteLine($"✓ Dash effect started at X={x}, Y={y}, Facing={facing}");
        }

        /// <summary>
        /// Show impact effect (Goatman kick)
        /// </summary>
        public void ShowImpactEffect(int playerNum, int x, int y, string facing, Action invalidateCallback)
        {
            // Hard reset GM_impact animation to first frame by reloading from resources
            try
            {
                var gmRes = Properties.Resources.GM_impact; // byte[] expected
                var reloaded = ResourceToImage(gmRes);
                if (reloaded != null)
                {
                    gmImpactEffect = reloaded; // replace instance to reset frame index
                    if (ImageAnimator.CanAnimate(gmImpactEffect))
                    {
                        ImageAnimator.Animate(gmImpactEffect, (s, e) => { invalidateCallback?.Invoke(); });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ GM_impact reload error: {ex.Message}");
            }

            if (playerNum == 1)
            {
                Impact1Active = true;
                Impact1X = x;
                Impact1Y = y;
                Impact1Facing = facing;

                Impact1Timer.Stop();
                Impact1Timer.Interval = GM_IMPACT_DURATION_MS;
                // Reassign handler to avoid multiple subscriptions
                Impact1Timer.Tick -= Impact1Timer_Tick;
                Impact1Timer.Tick += Impact1Timer_Tick;
                Impact1Timer.Start();
            }
            else
            {
                Impact2Active = true;
                Impact2X = x;
                Impact2Y = y;
                Impact2Facing = facing;

                Impact2Timer.Stop();
                Impact2Timer.Interval = GM_IMPACT_DURATION_MS;
                // Reassign handler to avoid multiple subscriptions
                Impact2Timer.Tick -= Impact2Timer_Tick;
                Impact2Timer.Tick += Impact2Timer_Tick;
                Impact2Timer.Start();
            }

            invalidateCallback?.Invoke();
        }

        private void Impact1Timer_Tick(object sender, EventArgs e)
        {
            Impact1Timer.Stop();
            Impact1Active = false;
            Console.WriteLine("✓ GM_impact P1 ended");
        }

        private void Impact2Timer_Tick(object sender, EventArgs e)
        {
            Impact2Timer.Stop();
            Impact2Active = false;
            Console.WriteLine("✓ GM_impact P2 ended");
        }

        /// <summary>
        /// Draw dash effects (BEHIND characters) - WITH DIRECTION SUPPORT
        /// </summary>
        public void DrawEffects(Graphics g, int viewportX, int playerWidth, int playerHeight)
        {
            // DRAW DASH EFFECTS (Teleport style - at feet)
            foreach (var effect in activeDashEffects.ToArray())
            {
                if (!effect.IsActive) continue;

                int screenX = effect.X - viewportX;
                
                // Draw at feet position (below character)
                int effectY = effect.Y + playerHeight - 40; // Draw at feet
                
                if (dashEffectImage != null)
                {
                    // FLIP IMAGE theo hướng nhìn + OFFSET cho tự nhiên
                    if (effect.Facing == "left")
                    {
                        // FLIP HORIZONTALLY for left facing + OFFSET 20px sang phải
                        g.DrawImage(
                            dashEffectImage,
                            new Rectangle(screenX + 80 + 20, effectY, -80, 40), // +20px offset
                            new Rectangle(0, 0, dashEffectImage.Width, dashEffectImage.Height),
                            GraphicsUnit.Pixel
                        );
                    }
                    else
                    {
                        // NORMAL cho right facing
                        g.DrawImage(
                            dashEffectImage,
                            screenX, effectY, 80, 40
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Draw hit effects (on top of characters)
        /// </summary>
        public void DrawHitEffects(Graphics g, int viewportX, int playerWidth, int playerHeight)
        {
            // Advance hit_effect frames per instance
            foreach (var effect in activeHitEffects.ToArray())
            {
                if (effect.EffectImage != null && ImageAnimator.CanAnimate(effect.EffectImage))
                {
                    ImageAnimator.UpdateFrames(effect.EffectImage);
                }
            }

            const int DRAW_W = 150; // 3x scale from 50
            const int DRAW_H = 150; // 3x scale from 50

            foreach (var effect in activeHitEffects.ToArray())
            {
                int screenX = effect.X - viewportX;
                if (screenX >= -DRAW_W && screenX <= g.ClipBounds.Width)
                {
                    // Center the larger effect around player center
                    int drawX = screenX + playerWidth / 2 - (DRAW_W / 2);
                    int drawY = effect.Y + playerHeight / 2 - (DRAW_H / 2);

                    g.DrawImage(effect.EffectImage,
                        drawX,
                        drawY,
                        DRAW_W,
                        DRAW_H);
                }
            }
        }

        /// <summary>
        /// Draw impact effects
        /// </summary>
        public void DrawImpactEffects(Graphics g, int viewportX)
        {
            // Ensure impact GIF advances frames
            if (gmImpactEffect != null && ImageAnimator.CanAnimate(gmImpactEffect))
            {
                ImageAnimator.UpdateFrames(gmImpactEffect);
            }

            if (Impact1Active && gmImpactEffect != null)
            {
                int screenX = Impact1X - viewportX;
                int drawW = 160, drawH = 160;
                int drawY = Impact1Y - drawH / 2; // center vertically around impact point
                if (Impact1Facing == "left")
                {
                    g.DrawImage(gmImpactEffect,
                        new Rectangle(screenX + drawW, drawY, -drawW, drawH),
                        new Rectangle(0, 0, gmImpactEffect.Width, gmImpactEffect.Height),
                        GraphicsUnit.Pixel);
                }
                else
                {
                    g.DrawImage(gmImpactEffect, screenX, drawY, drawW, drawH);
                }
            }

            if (Impact2Active && gmImpactEffect != null)
            {
                int screenX = Impact2X - viewportX;
                int drawW = 160, drawH = 160;
                int drawY = Impact2Y - drawH / 2;
                if (Impact2Facing == "left")
                {
                    g.DrawImage(gmImpactEffect,
                        new Rectangle(screenX + drawW, drawY, -drawW, drawH),
                        new Rectangle(0, 0, gmImpactEffect.Width, gmImpactEffect.Height),
                        GraphicsUnit.Pixel);
                }
                else
                {
                    g.DrawImage(gmImpactEffect, screenX, drawY, drawW, drawH);
                }
            }
        }

        /// <summary>
        /// Cleanup all effects
        /// </summary>
        public void Cleanup()
        {
            foreach (var effect in activeHitEffects.ToArray())
            {
                try
                {
                    effect.Timer?.Stop();
                    effect.Timer?.Dispose();
                }
                catch { }
            }
            activeHitEffects.Clear();

            foreach (var effect in activeDashEffects.ToArray())
            {
                try
                {
                    effect.Timer?.Stop();
                    effect.Timer?.Dispose();
                }
                catch { }
            }
            activeDashEffects.Clear();

            Impact1Timer?.Stop();
            Impact1Timer?.Dispose();
            Impact2Timer?.Stop();
            Impact2Timer?.Dispose();
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
