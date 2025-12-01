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
    /// Qu?n lý các hi?u ?ng visual: hit effects, dash effects, impacts
    /// </summary>
    public class EffectManager
    {
        private List<HitEffectInstance> activeHitEffects = new List<HitEffectInstance>();
        private Image hitEffectImage;
        private Image dashEffectImage; // ? GIF effect for dash
        private Image gmImpactEffect;

        // ? DASH EFFECTS - TELEPORT STYLE
        public class DashEffectInstance
        {
            public int X { get; set; }
            public int Y { get; set; }
            public string Facing { get; set; }
            public Timer Timer { get; set; }
            public bool IsActive { get; set; }
        }

        private List<DashEffectInstance> activeDashEffects = new List<DashEffectInstance>();

        // Impact effects (Goatman)
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

        private const int HIT_EFFECT_DURATION_MS = 150;
        private const int DASH_EFFECT_DURATION_MS = 150; // ? 0.15s duration

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
                // ? TRY TO LOAD DASH EFFECT GIF t? Resources
                try
                {
                    // Th? load t? Resources (n?u có Dash_effect ho?c dash_effect)
                    var dashResource = Properties.Resources.ResourceManager.GetObject("Dash_effect") 
                                    ?? Properties.Resources.ResourceManager.GetObject("dash_effect");
                    
                    if (dashResource != null)
                    {
                        dashEffectImage = ResourceToImage(dashResource);
                        if (dashEffectImage != null && ImageAnimator.CanAnimate(dashEffectImage))
                        {
                            ImageAnimator.Animate(dashEffectImage, (s, e) => { }); // Auto-animate
                            Console.WriteLine("? Loaded dash effect GIF from Resources");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"?? Could not load Dash_effect from Resources: {ex.Message}");
                }

                // ? FALLBACK: Create programmatic dash effect
                if (dashEffectImage == null)
                {
                    Console.WriteLine("?? Creating fallback dash effect");
                    dashEffectImage = CreateDashEffectImage();
                }

                // Load other effects
                hitEffectImage = CreateColoredImage(50, 50, Color.FromArgb(200, Color.Red));
                gmImpactEffect = CreateColoredImage(100, 100, Color.OrangeRed);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error loading effect images: {ex.Message}");
            }
        }

        /// <summary>
        /// Create dash effect image programmatically (fallback)
        /// </summary>
        private Image CreateDashEffectImage()
        {
            // ? Create a simple animated dash effect (3 frames)
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
        /// Show hit effect at player position
        /// </summary>
        public void ShowHitEffectAtPosition(int x, int y, Action invalidateCallback)
        {
            var effect = new HitEffectInstance
            {
                X = x,
                Y = y,
                EffectImage = hitEffectImage,
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
        /// ? Show dash effect at position (for teleport dash)
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
                Console.WriteLine($"? Dash effect expired for player {playerNum}");
            };

            activeDashEffects.Add(effect);
            effect.Timer.Start();
            invalidateCallback?.Invoke();
            
            Console.WriteLine($"?? Dash effect started at X={x}, Y={y}, Facing={facing}");
        }

        /// <summary>
        /// Show impact effect (Goatman kick)
        /// </summary>
        public void ShowImpactEffect(int playerNum, int x, int y, string facing, Action invalidateCallback)
        {
            if (playerNum == 1)
            {
                Impact1Active = true;
                Impact1X = x;
                Impact1Y = y;
                Impact1Facing = facing;

                Impact1Timer.Stop();
                Impact1Timer.Interval = 150;
                Impact1Timer.Tick += (s, e) =>
                {
                    Impact1Timer.Stop();
                    Impact1Active = false;
                    invalidateCallback?.Invoke();
                };
                Impact1Timer.Start();
            }
            else
            {
                Impact2Active = true;
                Impact2X = x;
                Impact2Y = y;
                Impact2Facing = facing;

                Impact2Timer.Stop();
                Impact2Timer.Interval = 150;
                Impact2Timer.Tick += (s, e) =>
                {
                    Impact2Timer.Stop();
                    Impact2Active = false;
                    invalidateCallback?.Invoke();
                };
                Impact2Timer.Start();
            }

            invalidateCallback?.Invoke();
        }

        /// <summary>
        /// ? Draw dash effects (BEHIND characters) - WITH DIRECTION SUPPORT
        /// </summary>
        public void DrawEffects(Graphics g, int viewportX, int playerWidth, int playerHeight)
        {
            // ? DRAW DASH EFFECTS (Teleport style - at feet)
            foreach (var effect in activeDashEffects.ToArray())
            {
                if (!effect.IsActive) continue;

                int screenX = effect.X - viewportX;
                
                // Draw at feet position (below character)
                int effectY = effect.Y + playerHeight - 40; // Draw at feet
                
                if (dashEffectImage != null)
                {
                    // ? FLIP IMAGE theo h??ng nhìn + OFFSET cho t? nhiên
                    if (effect.Facing == "left")
                    {
                        // ?? FLIP HORIZONTALLY for left facing + OFFSET 20px sang ph?i
                        g.DrawImage(
                            dashEffectImage,
                            new Rectangle(screenX + 80 + 20, effectY, -80, 40), // ? +20px offset!
                            new Rectangle(0, 0, dashEffectImage.Width, dashEffectImage.Height),
                            GraphicsUnit.Pixel
                        );
                    }
                    else
                    {
                        // ?? NORMAL cho right facing (không c?n offset)
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
            foreach (var effect in activeHitEffects.ToArray())
            {
                int screenX = effect.X - viewportX;
                if (screenX >= -50 && screenX <= g.ClipBounds.Width)
                {
                    g.DrawImage(effect.EffectImage,
                        screenX + playerWidth / 2 - 25,
                        effect.Y + playerHeight / 2 - 25,
                        50, 50);
                }
            }
        }

        /// <summary>
        /// Draw impact effects
        /// </summary>
        public void DrawImpactEffects(Graphics g, int viewportX)
        {
            if (Impact1Active && gmImpactEffect != null)
            {
                int screenX = Impact1X - viewportX;
                if (Impact1Facing == "left")
                {
                    g.DrawImage(gmImpactEffect,
                        new Rectangle(screenX + 100, Impact1Y, -100, 100),
                        new Rectangle(0, 0, gmImpactEffect.Width, gmImpactEffect.Height),
                        GraphicsUnit.Pixel);
                }
                else
                {
                    g.DrawImage(gmImpactEffect, screenX, Impact1Y, 100, 100);
                }
            }

            if (Impact2Active && gmImpactEffect != null)
            {
                int screenX = Impact2X - viewportX;
                if (Impact2Facing == "left")
                {
                    g.DrawImage(gmImpactEffect,
                        new Rectangle(screenX + 100, Impact2Y, -100, 100),
                        new Rectangle(0, 0, gmImpactEffect.Width, gmImpactEffect.Height),
                        GraphicsUnit.Pixel);
                }
                else
                {
                    g.DrawImage(gmImpactEffect, screenX, Impact2Y, 100, 100);
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
