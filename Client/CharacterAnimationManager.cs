using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DoAn_NT106
{
    /// <summary>
    /// Class qu?n lý animations cho nhân v?t trong game
    /// </summary>
    public class CharacterAnimationManager
    {
        // Animation collections
        private Dictionary<string, Image> animations;
        
        // Animation state
        private string currentAnimation = "stand";
        private string characterType;
        
        // Animation timing configuration (in milliseconds)
        private Dictionary<string, Dictionary<string, int>> animationDurations = new Dictionary<string, Dictionary<string, int>>
        {
            ["girlknight"] = new Dictionary<string, int>
            {
                ["punch"] = 400,
                ["kick"] = 500,
                ["special"] = 600
            },
            ["bringerofdeath"] = new Dictionary<string, int>
            {
                ["punch"] = 1250,   // 10 frames @ 8 fps
                ["kick"] = 556,     // 10 frames @ 18 fps
                ["special"] = 800
            }
        };

        // Hit timing configuration - frame number khi gây damage (tính t? 0)
        private Dictionary<string, Dictionary<string, int>> hitFrames = new Dictionary<string, Dictionary<string, int>>
        {
            ["girlknight"] = new Dictionary<string, int>
            {
                ["punch"] = 5,
                ["kick"] = 5,
                ["special"] = 5
            },
            ["bringerofdeath"] = new Dictionary<string, int>
            {
                ["punch"] = 5,
                ["kick"] = 5,
                ["special"] = 5
            }
        };

        // Event handler for frame changes
        private EventHandler frameChangedHandler;
        
        // Resource streams (for GIF animations)
        private List<System.IO.Stream> resourceStreams = new List<System.IO.Stream>();

        public CharacterAnimationManager(string characterType, EventHandler onFrameChanged)
        {
            this.characterType = characterType;
            this.frameChangedHandler = onFrameChanged;
            this.animations = new Dictionary<string, Image>();
        }

        /// <summary>
        /// Load t?t c? animations cho character
        /// </summary>
        public void LoadAnimations()
        {
            try
            {
                if (characterType == "girlknight")
                {
                    LoadGirlKnightAnimations();
                }
                else if (characterType == "bringerofdeath")
                {
                    LoadBringerOfDeathAnimations();
                }

                // Start animation for any animatable images
                foreach (var anim in animations.Values)
                {
                    if (anim != null && ImageAnimator.CanAnimate(anim))
                    {
                        ImageAnimator.Animate(anim, frameChangedHandler);
                    }
                }

                Console.WriteLine($"? ?ã load {animations.Count} animations cho {characterType}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error loading {characterType} animations: {ex.Message}");
                CreateFallbackAnimations(characterType == "girlknight" ? Color.Pink : Color.Purple);
            }
        }

        private void LoadGirlKnightAnimations()
        {
            animations["stand"] = ResourceToImage(Properties.Resources.girlknight_stand);
            animations["walk"] = ResourceToImage(Properties.Resources.girlknight_walk);
            animations["punch"] = ResourceToImage(Properties.Resources.girlknight_attack);
            animations["kick"] = ResourceToImage(Properties.Resources.girlknight_kick);
            animations["jump"] = ResourceToImage(Properties.Resources.girlknight_jump);
            animations["hurt"] = ResourceToImage(Properties.Resources.girlknight_hurt);
            animations["parry"] = ResourceToImage(Properties.Resources.girlknight_parry);
            animations["fireball"] = ResourceToImage(Properties.Resources.girlknight_fireball);
            animations["slide"] = ResourceToImage(Properties.Resources.girlknight_walk);
        }

        private void LoadBringerOfDeathAnimations()
        {
            animations["stand"] = ResourceToImage(Properties.Resources.BringerofDeath_Idle);
            animations["walk"] = ResourceToImage(Properties.Resources.BringerofDeath_Walk);
            animations["punch"] = ResourceToImage(Properties.Resources.BringerofDeath_Attack2);
            animations["kick"] = ResourceToImage(Properties.Resources.BringerofDeath_Attack1);
            animations["jump"] = ResourceToImage(Properties.Resources.BringerofDeath_Walk);
            animations["hurt"] = ResourceToImage(Properties.Resources.BringerofDeath_Hurt);
            animations["parry"] = ResourceToImage(Properties.Resources.BringerofDeath_Parry);
            animations["fireball"] = ResourceToImage(Properties.Resources.BringerofDeath_Cast);
            animations["slide"] = ResourceToImage(Properties.Resources.BringerofDeath_Walk);
            animations["spell"] = ResourceToImage(Properties.Resources.BringerofDeath_Spell);
        }

        /// <summary>
        /// Get animation image
        /// </summary>
        public Image GetAnimation(string animationName)
        {
            return animations.ContainsKey(animationName) ? animations[animationName] : null;
        }

        /// <summary>
        /// Get current animation image
        /// </summary>
        public Image GetCurrentAnimation()
        {
            return GetAnimation(currentAnimation);
        }

        /// <summary>
        /// Set current animation
        /// </summary>
        public void SetCurrentAnimation(string animationName)
        {
            if (animations.ContainsKey(animationName))
            {
                currentAnimation = animationName;
            }
        }

        /// <summary>
        /// Update animation frames
        /// </summary>
        public void UpdateFrames()
        {
            var img = GetCurrentAnimation();
            if (img != null && ImageAnimator.CanAnimate(img))
            {
                ImageAnimator.UpdateFrames(img);
            }
        }

        /// <summary>
        /// Reset animation v? frame ??u tiên
        /// </summary>
        public void ResetAnimationToFirstFrame(string animationName)
        {
            var animation = GetAnimation(animationName);
            if (animation != null && ImageAnimator.CanAnimate(animation))
            {
                try
                {
                    ImageAnimator.StopAnimate(animation, frameChangedHandler);
                    animation.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Time, 0);
                    ImageAnimator.Animate(animation, frameChangedHandler);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error resetting animation: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get animation duration
        /// </summary>
        public int GetAnimationDuration(string attackType)
        {
            if (animationDurations.ContainsKey(characterType) && 
                animationDurations[characterType].ContainsKey(attackType))
            {
                return animationDurations[characterType][attackType];
            }
            
            return attackType switch
            {
                "punch" => 400,
                "kick" => 500,
                "special" => 600,
                _ => 400
            };
        }

        /// <summary>
        /// Get hit frame delay (th?i gian ??n khi gây damage)
        /// </summary>
        public int GetHitFrameDelay(string attackType)
        {
            int totalFrames = 10;
            int hitFrameIndex = 5;

            if (hitFrames.ContainsKey(characterType) && hitFrames[characterType].ContainsKey(attackType))
            {
                hitFrameIndex = hitFrames[characterType][attackType];
            }

            int totalDuration = GetAnimationDuration(attackType);
            int delay = (int)((float)hitFrameIndex / totalFrames * totalDuration);
            
            return delay;
        }

        /// <summary>
        /// Check if animation exists
        /// </summary>
        public bool HasAnimation(string animationName)
        {
            return animations.ContainsKey(animationName);
        }

        /// <summary>
        /// Safe resource loader
        /// </summary>
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
                        resourceStreams.Add(ms);
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
        /// Create fallback animations
        /// </summary>
        private void CreateFallbackAnimations(Color baseColor)
        {
            animations["stand"] = CreateColoredImage(80, 120, baseColor);
            animations["walk"] = CreateWalkingAnimation(baseColor);
            animations["punch"] = CreateColoredImage(90, 120, Darken(baseColor, 0.3f));
            animations["kick"] = CreateColoredImage(100, 120, Darken(baseColor, 0.4f));
            animations["jump"] = CreateColoredImage(80, 100, Lighten(baseColor, 0.1f));
            animations["fireball"] = CreateColoredImage(80, 120, Color.Yellow);
            animations["hurt"] = CreateColoredImage(80, 120, Color.White);
            animations["parry"] = CreateColoredImage(80, 120, Color.LightSkyBlue);
        }

        private Image CreateWalkingAnimation(Color baseColor)
        {
            var walkAnimation = new Bitmap(160, 120);
            using (var g = Graphics.FromImage(walkAnimation))
            {
                g.FillRectangle(new SolidBrush(baseColor), 0, 0, 80, 120);
                g.FillRectangle(new SolidBrush(Color.Black), 10, 100, 60, 20);
                g.FillRectangle(new SolidBrush(Lighten(baseColor, 0.2f)), 80, 0, 80, 120);
                g.FillRectangle(new SolidBrush(Color.Black), 90, 110, 60, 10);
            }
            return walkAnimation;
        }

        private Bitmap CreateColoredImage(int width, int height, Color color)
        {
            var bmp = new Bitmap(Math.Max(1, width), Math.Max(1, height));
            using (var g = Graphics.FromImage(bmp))
            using (var brush = new SolidBrush(color))
                g.FillRectangle(brush, 0, 0, bmp.Width, bmp.Height);
            return bmp;
        }

        private Color Lighten(Color color, float factor)
        {
            return Color.FromArgb(
                Math.Min(255, (int)(color.R + (255 - color.R) * factor)),
                Math.Min(255, (int)(color.G + (255 - color.G) * factor)),
                Math.Min(255, (int)(color.B + (255 - color.B) * factor))
            );
        }

        private Color Darken(Color color, float factor)
        {
            return Color.FromArgb(
                (int)(color.R * (1 - factor)),
                (int)(color.G * (1 - factor)),
                (int)(color.B * (1 - factor))
            );
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            foreach (var stream in resourceStreams)
            {
                try { stream.Dispose(); } catch { }
            }
            resourceStreams.Clear();

            foreach (var anim in animations.Values)
            {
                if (anim != null && ImageAnimator.CanAnimate(anim))
                {
                    try { ImageAnimator.StopAnimate(anim, frameChangedHandler); } catch { }
                }
            }
        }
    }
}
