using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DoAn_NT106.Client.Class
{
    /// <summary>
    /// Class quản lý animations cho nhân vật trong game
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
                ["punch"] = 1000,    //  6 frames ÷ 6 fps = 1000ms
                ["kick"] = 1500,     //  9 frames ÷ 6 fps = 1500ms
                ["special"] = 500,   //  5 frames ÷ 10 fps = 500ms (loop)
                ["slide"] = 200      // dash: 0.2s
            },
            ["bringerofdeath"] = new Dictionary<string, int>
            {
                ["punch"] = 1250,   //  10 frames ÷ 8 fps = 1250ms
                ["kick"] = 556,     //  10 frames ÷ 18 fps = 556ms
                ["special"] = 1125, //  9 frames ÷ 8 fps = 1125ms (Cast animation)
                ["slide"] = 200
            },
            ["goatman"] = new Dictionary<string, int>
            {
                ["punch"] = 545,    //  6 frames ÷ 11 fps = 545ms
                ["kick"] = 667,     //  6 frames ÷ 9 fps = 667ms
                ["special"] = 3000, // charge skill: 3s duration
                ["slide"] = 200     // dash: 0.2s
            },
            ["warrior"] = new Dictionary<string, int>
            {
                ["punch"] = 1000,   //  12 frames ÷ 12 fps = 1000ms
                ["kick"] = 800,    //  10 frames ÷ 10 fps = 1000ms
                ["special"] = 714,  //  5 frames ÷ 7 fps = 714ms
                ["slide"] = 200     // dash: 0.2s
            }
        };

        // Hit timing configuration - frame number khi gây damage (tính từ 0)
        //  HIT TIMING CONFIGURATION - FRAME NUMBER (1-based), NOT INDEX!
        // VD: Frame 4 = frame thứ 4 trong animation (không phải index 3)
        private Dictionary<string, Dictionary<string, int>> hitFrames = new Dictionary<string, Dictionary<string, int>>
        {
            ["girlknight"] = new Dictionary<string, int>
            {
                ["punch"] = 3,      //  Frame 3: 6fps, 6 frames → 3/6 * 1000ms = 500ms
                ["kick"] = 6,       //  Frame 6: 6fps, 9 frames → 6/9 * 1500ms = 1000ms
                ["special"] = 0     // Continuous damage at 0.5s and 1.0s intervals
            },
            ["bringerofdeath"] = new Dictionary<string, int>
            {
                ["punch"] = 6,      //  Frame 6: 8fps, 10 frames → 6/10 * 1250ms = 750ms
                ["kick"] = 6,       //  Frame 6: 18fps, 10 frames → 6/10 * 556ms = 333ms
                ["special"] = 6     // Frame 6 - khi spell được spawn
            },
            ["goatman"] = new Dictionary<string, int>
            {
                ["punch"] = 4,     //  Frame 4: 11fps, 6 frames → 4/6 * 545ms = 363ms
                ["kick"] = 4,      //  Frame 4: 9fps, 6 frames → 4/6 * 667ms = 445ms
                ["special"] = 0    // Collision-based damage
            },
            ["warrior"] = new Dictionary<string, int>
            {
                ["punch"] = 6,     //  Frame 6: 12fps, 12 frames → 6/12 * 1000ms = 500ms (first hit)
                ["kick"] = 2,      //  SỬA: Frame 4 → Frame 2 (sớm hơn 2 frame): 2/10 * 1000ms = 200ms
                ["special"] = 3    //  Frame 3: 7fps, 5 frames → 3/5 * 714ms = 428ms
            }
        };

        // Frame count configuration
        private Dictionary<string, Dictionary<string, int>> frameCounts = new Dictionary<string, Dictionary<string, int>>
        {
            ["girlknight"] = new Dictionary<string, int>
            {
                ["punch"] = 6,
                ["kick"] = 9,
                ["special"] = 5
            },
            ["bringerofdeath"] = new Dictionary<string, int>
            {
                ["punch"] = 10,
                ["kick"] = 10,
                ["special"] = 9
            },
            ["goatman"] = new Dictionary<string, int>
            {
                ["punch"] = 6,
                ["kick"] = 6,
                ["special"] = 0  // Charge is time-based
            },
            ["warrior"] = new Dictionary<string, int>
            {
                ["punch"] = 12,
                ["kick"] = 10,
                ["special"] = 5
            }
        };

        // Multi-hit configuration for skills
        private Dictionary<string, List<int>> multiHitTimings = new Dictionary<string, List<int>>
        {
            ["girlknight_special"] = new List<int> { 500, 1000 }, // Hit at 0.5s and 1.0s
            ["warrior_punch"] = new List<int> { 500, 833 }  // Hit at frame 6 and frame 10
        };

        // Slide distance for attack2
        private Dictionary<string, int> slideDistances = new Dictionary<string, int>
        {
            ["girlknight_kick"] = 200,  // Tăng từ 60 lên 200 (1/2 dash)
            ["warrior_kick"] = 200      // Tăng từ 40 lên 200 (1/2 dash)
        };

        // Knockback configuration
        private Dictionary<string, int> knockbackDistances = new Dictionary<string, int>
        {
            ["goatman_kick"] = 80  // Strong knockback for attack2
        };
        
        // Event handler for frame changes
        private EventHandler frameChangedHandler;
        
        // Resource streams (for GIF animations)
        private List<Stream> resourceStreams = new List<Stream>();

        public CharacterAnimationManager(string characterType, EventHandler onFrameChanged)
        {
            this.characterType = characterType;
            frameChangedHandler = onFrameChanged;
            animations = new Dictionary<string, Image>();
        }

        /// <summary>
        /// Load tất cả animations cho character
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
                else if (characterType == "goatman")
                {
                    LoadGoatmanAnimations();
                }
                else if (characterType == "warrior")
                {
                    LoadWarriorAnimations();
                }

                // Start animation for any animatable images
                foreach (var anim in animations.Values)
                {
                    if (anim != null && ImageAnimator.CanAnimate(anim))
                    {
                        ImageAnimator.Animate(anim, frameChangedHandler);
                    }
                }

                Console.WriteLine($" Đã load {animations.Count} animations cho {characterType}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading {characterType} animations: {ex.Message}");
                CreateFallbackAnimations(characterType == "girlknight" ? Color.Pink : 
                                       characterType == "goatman" ? Color.Brown :
                                       characterType == "warrior" ? Color.DarkRed : Color.Purple);
            }
        }

        private void LoadGirlKnightAnimations()
        {
            animations["stand"] = ResourceToImage(Properties.Resources.Knightgirl_Idle);
            animations["walk"] = ResourceToImage(Properties.Resources.Knightgirl_Walking);
            animations["punch"] = ResourceToImage(Properties.Resources.Knightgirl_Attack1);
            animations["kick"] = ResourceToImage(Properties.Resources.Knightgirl_Attack2);
            animations["jump"] = ResourceToImage(Properties.Resources.Knightgirl_Jump);
            animations["hurt"] = ResourceToImage(Properties.Resources.Knightgirl_Hurt);
            animations["parry"] = ResourceToImage(Properties.Resources.Knightgirl_parry);
            animations["fireball"] = ResourceToImage(Properties.Resources.Knightgirl_Skill);
            animations["slide"] = ResourceToImage(Properties.Resources.Knightgirl_Dash);
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
            animations["slide"] = ResourceToImage(Properties.Resources.BringerofDeath_Walk); // No dash animation
            animations["spell"] = ResourceToImage(Properties.Resources.BringerofDeath_Spell);
        }

        private void LoadGoatmanAnimations()
        {
            animations["stand"] = ResourceToImage(Properties.Resources.GM_Idle);
            animations["walk"] = ResourceToImage(Properties.Resources.GM_run);
            animations["punch"] = ResourceToImage(Properties.Resources.GM_Attack1);
            animations["kick"] = ResourceToImage(Properties.Resources.GM_Attack2);
            animations["jump"] = ResourceToImage(Properties.Resources.GM_run);
            animations["hurt"] = ResourceToImage(Properties.Resources.GM_Hurt);
            var gmParryObj = Properties.Resources.ResourceManager.GetObject("GM_parry");
            animations["parry"] = ResourceToImage(gmParryObj);
            animations["fireball"] = ResourceToImage(Properties.Resources.GM_skill);
            animations["slide"] = ResourceToImage(Properties.Resources.GM_run);
        }

        private void LoadWarriorAnimations()
        {
            animations["stand"] = ResourceToImage(Properties.Resources.Warrior_Idle);
            animations["walk"] = ResourceToImage(Properties.Resources.Warrior_Walk);
            animations["punch"] = ResourceToImage(Properties.Resources.Warrior_Attack1);
            animations["kick"] = ResourceToImage(Properties.Resources.Warrior_Attack2);
            animations["jump"] = ResourceToImage(Properties.Resources.Warrior_Jump);
            animations["hurt"] = ResourceToImage(Properties.Resources.Warrior_Hurt);
            animations["parry"] = ResourceToImage(Properties.Resources.Warrior_Parry);
            animations["fireball"] = ResourceToImage(Properties.Resources.Warrior_Skill);
            animations["slide"] = ResourceToImage(Properties.Resources.Warrior_Dash);
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
        /// Reset animation về frame đầu tiên
        /// </summary>
        public void ResetAnimationToFirstFrame(string animationName)
        {
            try
            {
                var animation = GetAnimation(animationName);
                if (animation == null)
                {
                    Console.WriteLine($"⚠️ Animation '{animationName}' not found for {characterType}");
                    return;
                }

                if (!ImageAnimator.CanAnimate(animation))
                {
                    Console.WriteLine($"⚠️ Animation '{animationName}' is not animatable for {characterType}");
                    return;
                }

                //  CRITICAL: Stop -> Reset -> Restart với NHIỀU LẦN ĐỂ ĐẢM BẢO
                
                //  BƯỚC 1: STOP animation NHIỀU LẦN
                try
                {
                    ImageAnimator.StopAnimate(animation, frameChangedHandler);
                    Thread.Sleep(5);
                    ImageAnimator.StopAnimate(animation, frameChangedHandler); // Lần 2
                    Console.WriteLine($" Stopped animation: {animationName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Stop error: {ex.Message}");
                }

                //  BƯỚC 2: Wait để đảm bảo stop hoàn tất
                Thread.Sleep(15);

                //  BƯỚC 3: RESET về frame 0 - CRITICAL STEP
                try
                {
                    animation.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Time, 0);
                    Console.WriteLine($" Reset to frame 0: {animationName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Reset frame error: {ex.Message}");
                }

                //  BƯỚC 4: Wait để đảm bảo reset hoàn tất
                Thread.Sleep(15);

                //  BƯỚC 5: RESTART animation từ frame 0
                try
                {
                    ImageAnimator.Animate(animation, frameChangedHandler);
                    Console.WriteLine($" Restarted animation: {animationName} for {characterType}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Restart error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ResetAnimationToFirstFrame error for '{animationName}': {ex.Message}");
            }
        }

        /// <summary>
        /// Get animation duration
        /// </summary>
        public int GetAnimationDuration(string attackType)
        {
            //  SỬ DỤNG TIMING TỪ frameTimings DICTIONARY ĐỂ TÍNH CHÍNH XÁC
            if (animationDurations.ContainsKey(characterType) && 
                animationDurations[characterType].ContainsKey(attackType))
            {
                return animationDurations[characterType][attackType];
            }
            
            //  FALLBACK: Tính từ frameTimings nếu có
            if (frameCounts.ContainsKey(characterType) && frameCounts[characterType].ContainsKey(attackType))
            {
                int frameCount = frameCounts[characterType][attackType];
                
                // Lấy FPS từ config
                float fps = 10f; // Default
                switch (characterType)
                {
                    case "goatman":
                        fps = attackType == "punch" ? 11f : 9f;
                        break;
                    case "bringerofdeath":
                        fps = attackType == "punch" ? 8f : 18f;
                        break;
                    case "girlknight":
                        fps = attackType == "special" ? 10f : 6f;
                        break;
                    case "warrior":
                        fps = attackType == "punch" ? 12f : attackType == "kick" ? 10f : 7f;
                        break;
                }
                
                //  TÍNH CHÍNH XÁC: (frames / fps) * 1000
                return (int)(frameCount / fps * 1000);
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
        /// Get hit frame delay (thời gian đến khi gây damage) -  FRAME NUMBER, NOT INDEX
        /// </summary>
        public int GetHitFrameDelay(string attackType)
        {
            //  LẤY FRAME NUMBER (1-based) từ config
            int hitFrame = 5; // Default
            if (hitFrames.ContainsKey(characterType) && hitFrames[characterType].ContainsKey(attackType))
            {
                hitFrame = hitFrames[characterType][attackType];
            }

            //  LẤY TỔNG SỐ FRAMES
            int totalFrames = 10; // Default
            if (frameCounts.ContainsKey(characterType) && frameCounts[characterType].ContainsKey(attackType))
            {
                totalFrames = frameCounts[characterType][attackType];
            }

            //  LẤY TỔNG THỜI GIAN ANIMATION
            int totalDuration = GetAnimationDuration(attackType);
            
            //  CÔNG THỨC ĐÚNG: (hitFrame / totalFrames) * totalDuration
            // VD: Goatman punch: frame 4, total 6 frames, duration 545ms
            // → (4/6) * 545 = 363ms ← ĐÚNG!
            int delay = (int)((float)hitFrame / totalFrames * totalDuration);
            
            Console.WriteLine($"🎯 {characterType} {attackType}: Frame {hitFrame}/{totalFrames} = {delay}ms (total: {totalDuration}ms)");
            
            return delay;
        }

        /// <summary>
        /// Get slide distance for attack
        /// </summary>
        public int GetSlideDistance(string attackType)
        {
            string key = $"{characterType}_{attackType}";
            if (slideDistances.ContainsKey(key))
            {
                return slideDistances[key];
            }
            return 0;
        }

        /// <summary>
        /// Get multi-hit timings for skills
        /// </summary>
        public List<int> GetMultiHitTimings(string attackType)
        {
            string key = $"{characterType}_{attackType}";
            if (multiHitTimings.ContainsKey(key))
            {
                return new List<int>(multiHitTimings[key]);
            }
            return new List<int>();
        }

        /// <summary>
        /// Get knockback distance for attack
        /// </summary>
        public int GetKnockbackDistance(string attackType)
        {
            string key = $"{characterType}_{attackType}";
            if (knockbackDistances.ContainsKey(key))
            {
                return knockbackDistances[key];
            }
            return 20; // Default knockback
        }

        /// <summary>
        /// Check if animation exists
        /// </summary>
        public bool HasAnimation(string animationName)
        {
            return animations.ContainsKey(animationName);
        }

        /// <summary>
        /// Check if character has dash animation
        /// </summary>
        public bool HasDashAnimation()
        {
            // Girl Knight và Warrior có dash animation riêng
            return characterType == "girlknight" || characterType == "warrior";
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
                    var ms = new MemoryStream(b);
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
            animations["slide"] = CreateColoredImage(80, 120, Lighten(baseColor, 0.2f));
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
