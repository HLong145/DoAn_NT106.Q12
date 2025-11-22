using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DoAn_NT106
{
    public partial class BattleForm : Form
    {
        private string username;
        private string token;
        private string opponent;

        // Game assets
        private Image background;
        private Image fireball;
        private List<string> backgroundImages = new List<string>();

        // Player positions (x = left, y = top)
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
        private Dictionary<string, Image> player1Animations = new Dictionary<string, Image>();
        private Dictionary<string, Image> player2Animations = new Dictionary<string, Image>();
        private string player1CurrentAnimation = "stand";
        private string player2CurrentAnimation = "stand";
        private string player1Facing = "right";
        private string player2Facing = "left";
        private bool player1Walking = false;
        private bool player2Walking = false;
        private bool player1Attacking = false;
        private bool player2Attacking = false;
        private System.Windows.Forms.Timer walkAnimationTimer;

        // Progress bars (assume GameProgressBar exists in project)
        private GameProgressBar healthBar1, healthBar2;
        private GameProgressBar staminaBar1, staminaBar2;
        private GameProgressBar manaBar1, manaBar2;
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

        // Key states
        private bool aPressed, dPressed;
        private bool leftPressed, rightPressed;

        // Kích thước nhân vật (dynamic)
        private int PLAYER_WIDTH = 80;
        private int PLAYER_HEIGHT = 120;
        private float characterHeightRatio = 0.30f; // relative to ClientSize.Height

        // Hurt handling
        private const int HURT_DISPLAY_MS = 400;
        private string _prevAnimPlayer1 = null;
        private string _prevAnimPlayer2 = null;

        public BattleForm(string username, string token, string opponent = "Opponent")
        {
            InitializeComponent();

            this.username = username;
            this.token = token;
            this.opponent = opponent;

            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;

            // set initial ground based on current ClientSize
            groundLevel = Math.Max(0, this.ClientSize.Height - groundOffset);

            // set initial Y so characters stand "on the ground"
            player1Y = groundLevel - PLAYER_HEIGHT;
            player2Y = groundLevel - PLAYER_HEIGHT;

            SetupGame();
            SetupEventHandlers();

            this.Text = $"⚔️ Street Fighter - {username} vs {opponent}";
            this.DoubleBuffered = true;
            this.KeyPreview = true;
        }

        private void SetupGame()
        {
            try
            {
                // Load animations
                LoadCharacterAnimations("girlknight", player1Animations);
                LoadCharacterAnimations("girlknight", player2Animations);

                // Update character size after animations loaded
                UpdateCharacterSize();

                // Khởi tạo walk animation timer
                walkAnimationTimer = new System.Windows.Forms.Timer();
                walkAnimationTimer.Interval = 100;
                walkAnimationTimer.Tick += WalkAnimationTimer_Tick;
                // Parry timers init
                p1ParryTimer = new System.Windows.Forms.Timer();
                p1ParryTimer.Interval = parryWindowMs;
                p1ParryTimer.Tick += (s, e) =>
                {
                    p1ParryTimer.Stop();
                    player1Parrying = false;
                    player1ParryOnCooldown = true;
                    // restore previous animation if still valid
                    if (!player1Attacking && !player1Jumping)
                        player1CurrentAnimation = (_prevAnimPlayer1 == "walk" && (aPressed || dPressed)) ? "walk" : "stand";
                    p1ParryCooldownTimer.Start();
                    this.Invalidate();
                };

                p1ParryCooldownTimer = new System.Windows.Forms.Timer();
                p1ParryCooldownTimer.Interval = parryCooldownMs;
                p1ParryCooldownTimer.Tick += (s, e) =>
                {
                    p1ParryCooldownTimer.Stop();
                    player1ParryOnCooldown = false;
                };

                p2ParryTimer = new System.Windows.Forms.Timer();
                p2ParryTimer.Interval = parryWindowMs;
                p2ParryTimer.Tick += (s, e) =>
                {
                    p2ParryTimer.Stop();
                    player2Parrying = false;
                    player2ParryOnCooldown = true;
                    if (!player2Attacking && !player2Jumping)
                        player2CurrentAnimation = (_prevAnimPlayer2 == "walk" && (leftPressed || rightPressed)) ? "walk" : "stand";
                    p2ParryCooldownTimer.Start();
                    this.Invalidate();
                };

                p2ParryCooldownTimer = new System.Windows.Forms.Timer();
                p2ParryCooldownTimer.Interval = parryCooldownMs;
                p2ParryCooldownTimer.Tick += (s, e) =>
                {
                    p2ParryCooldownTimer.Stop();
                    player2ParryOnCooldown = false;
                };
                // Load background options
                backgroundImages.Add("battleground1");
                backgroundImages.Add("battleground2");
                backgroundImages.Add("battleground3");
                backgroundImages.Add("battleground4");

                cmbBackground.Items.AddRange(new object[] {
                    "Battlefield 1", "Battlefield 2", "Battlefield 3", "Battlefield 4"
                });
                if (cmbBackground.Items.Count > 0) cmbBackground.SelectedIndex = 0;

                // Set background (uses current ClientSize; safe-guards inside)
                SetBackground(backgroundImages[currentBackground]);

                // Load fireball (resource may be Image or byte[])
                try
                {
                    fireball = ResourceToImage(Properties.Resources.fireball);
                    if (fireball != null && ImageAnimator.CanAnimate(fireball))
                    {
                        ImageAnimator.Animate(fireball, OnFrameChanged);
                    }
                }
                catch
                {
                    fireball = CreateColoredImage(FIREBALL_WIDTH, FIREBALL_HEIGHT, Color.Orange);
                }
            }
            catch (Exception ex)
            {
                CreateFallbackGraphics();
                Console.WriteLine($"Setup error: {ex.Message}");
            }

            SetupStatusBars();
            SetupControlsInfo();
        }

        private void StartParry(int player)
        {
            if (player == 1)
            {
                if (player1ParryOnCooldown) return;
                if (player1Stamina < parryStaminaCost) { ShowHitEffect("No Stamina!", Color.Gray); return; }

                player1Stamina -= parryStaminaCost;
                player1Parrying = true;

                // Lưu animation hiện tại và chuyển sang parry gif
                _prevAnimPlayer1 = player1CurrentAnimation;
                player1CurrentAnimation = "parry";

                // Nếu parry image là animatable, đảm bảo ImageAnimator chạy
                if (player1Animations.ContainsKey("parry") && player1Animations["parry"] != null
                    && ImageAnimator.CanAnimate(player1Animations["parry"]))
                {
                    try { ImageAnimator.Animate(player1Animations["parry"], OnFrameChanged); } catch { }
                }

                p1ParryTimer.Stop();
                p1ParryTimer.Interval = parryWindowMs;
                p1ParryTimer.Start();

                ShowHitEffect("Parry!", Color.Cyan);
                this.Invalidate();
            }
            else if (player == 2)
            {
                if (player2ParryOnCooldown) return;
                if (player2Stamina < parryStaminaCost) { ShowHitEffect("No Stamina!", Color.Gray); return; }

                player2Stamina -= parryStaminaCost;
                player2Parrying = true;

                _prevAnimPlayer2 = player2CurrentAnimation;
                player2CurrentAnimation = "parry";

                if (player2Animations.ContainsKey("parry") && player2Animations["parry"] != null
                    && ImageAnimator.CanAnimate(player2Animations["parry"]))
                {
                    try { ImageAnimator.Animate(player2Animations["parry"], OnFrameChanged); } catch { }
                }

                p2ParryTimer.Stop();
                p2ParryTimer.Interval = parryWindowMs;
                p2ParryTimer.Start();

                ShowHitEffect("Parry!", Color.Cyan);
                this.Invalidate();
            }
        }
        private void CreateFallbackGraphics()
        {
            // Tạo fallback cho player animations
            CreateFallbackAnimations(player1Animations, Color.Pink);
            CreateFallbackAnimations(player2Animations, Color.Purple);
            background = CreateColoredImage(backgroundWidth, this.ClientSize.Height, Color.DarkGreen);
            fireball = CreateColoredImage(40, 25, Color.Orange);
        }
        private void LoadCharacterAnimations(string characterName, Dictionary<string, Image> animations)
        {
            try
            {
                // Load resources; ResourceToImage handles whether resource is Image or byte[]
                animations["stand"] = ResourceToImage(Properties.Resources.girlknight_stand);
                animations["walk"] = ResourceToImage(Properties.Resources.girlknight_walk);
                animations["punch"] = ResourceToImage(Properties.Resources.girlknight_attack);
                animations["kick"] = ResourceToImage(Properties.Resources.girlknight_kick);
                animations["jump"] = ResourceToImage(Properties.Resources.girlknight_jump);
                animations["hurt"] = ResourceToImage(Properties.Resources.girlknight_hurt);
                // NEW: parry animation resource (animated GIF ideally)
                animations["parry"] = ResourceToImage(Properties.Resources.girlknight_parry);
                // optional fireball sprite for character
                animations["fireball"] = ResourceToImage(Properties.Resources.girlknight_fireball);

                // Start animation for any animatable images
                foreach (var anim in animations.Values)
                {
                    if (anim != null && ImageAnimator.CanAnimate(anim))
                    {
                        ImageAnimator.Animate(anim, OnFrameChanged);
                    }
                }

                Console.WriteLine($"✅ Đã load {animations.Count} animations cho {characterName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading {characterName} animations: {ex.Message}");
                CreateFallbackAnimations(animations, characterName == "girlknight" ? Color.Pink : Color.Purple);
            }
        }

        // Apply hurt properly so it's not immediately overwritten by the attack reset logic
        private void ApplyHurtToPlayer(int player, int damage)
        {
            // Helper: show damage text (reuse ShowHitEffect) with red color for damage
            void ShowDamage(int dmg)
            {
                ShowHitEffect($"-{dmg}", Color.Red);
            }

            // If target is parrying -> block the hit (no HP lost), reward small stamina and brief stun to attacker
            if (player == 1)
            {
                if (player1Parrying)
                {
                    // Successful parry: refund small stamina, show feedback, cancel attacker's attacking flag briefly
                    player1Stamina = Math.Min(100, player1Stamina + 8);
                    ShowHitEffect("Blocked!", Color.Cyan);

                    // penalize attacker a bit (cancel attacking state)
                    player2Attacking = false;

                    // tiny visual/stun feedback (no persistent state)
                    var stunTimer = new System.Windows.Forms.Timer { Interval = 200 };
                    stunTimer.Tick += (s, e) =>
                    {
                        stunTimer.Stop();
                        stunTimer.Dispose();
                    };
                    stunTimer.Start();

                    this.Invalidate();
                    return; // no damage applied
                }

                // If already in hurt animation, ignore to avoid double-hits stacking too fast
                if (player1CurrentAnimation == "hurt")
                    return;

                // Apply damage
                player1Health = Math.Max(0, player1Health - damage);
                ShowDamage(damage);

                // Set hurt animation and animate if possible
                player1CurrentAnimation = "hurt";
                if (player1Animations.ContainsKey("hurt") && player1Animations["hurt"] != null && ImageAnimator.CanAnimate(player1Animations["hurt"]))
                {
                    try { ImageAnimator.Animate(player1Animations["hurt"], OnFrameChanged); } catch { }
                }

                // small knockback away from attacker (if attacker on right, knock left, and vice versa)
                int kb = (player2X > player1X) ? -20 : 20;
                player1X = Math.Max(0, Math.Min(backgroundWidth - PLAYER_WIDTH, player1X + kb));

                // ensure UI update
                this.Invalidate();

                // restore animation after HURT_DISPLAY_MS if not attacking/jumping
                var restoreTimer = new System.Windows.Forms.Timer { Interval = HURT_DISPLAY_MS };
                restoreTimer.Tick += (s, e) =>
                {
                    restoreTimer.Stop();
                    restoreTimer.Dispose();
                    if (!player1Attacking && !player1Jumping && player1CurrentAnimation == "hurt")
                    {
                        player1CurrentAnimation = (aPressed || dPressed) ? "walk" : "stand";
                    }
                    this.Invalidate();
                };
                restoreTimer.Start();

                // Check death
                if (player1Health <= 0)
                {
                    // If both die it's handled in UpdateGame; but we can call ShowGameOver here as well
                    // Stop timers and show result
                    // (Safer to let UpdateGame detect it on next tick)
                }

                return;
            }

            // player == 2
            if (player == 2)
            {
                if (player2Parrying)
                {
                    player2Stamina = Math.Min(100, player2Stamina + 8);
                    ShowHitEffect("Blocked!", Color.Cyan);

                    player1Attacking = false;
                    var stunTimer = new System.Windows.Forms.Timer { Interval = 200 };
                    stunTimer.Tick += (s, e) =>
                    {
                        stunTimer.Stop();
                        stunTimer.Dispose();
                    };
                    stunTimer.Start();

                    this.Invalidate();
                    return;
                }

                if (player2CurrentAnimation == "hurt")
                    return;

                player2Health = Math.Max(0, player2Health - damage);
                ShowDamage(damage);

                player2CurrentAnimation = "hurt";
                if (player2Animations.ContainsKey("hurt") && player2Animations["hurt"] != null && ImageAnimator.CanAnimate(player2Animations["hurt"]))
                {
                    try { ImageAnimator.Animate(player2Animations["hurt"], OnFrameChanged); } catch { }
                }

                // small knockback
                int kb2 = (player1X > player2X) ? -20 : 20;
                player2X = Math.Max(0, Math.Min(backgroundWidth - PLAYER_WIDTH, player2X + kb2));

                this.Invalidate();

                var restoreTimer2 = new System.Windows.Forms.Timer { Interval = HURT_DISPLAY_MS };
                restoreTimer2.Tick += (s, e) =>
                {
                    restoreTimer2.Stop();
                    restoreTimer2.Dispose();
                    if (!player2Attacking && !player2Jumping && player2CurrentAnimation == "hurt")
                    {
                        player2CurrentAnimation = (leftPressed || rightPressed) ? "walk" : "stand";
                    }
                    this.Invalidate();
                };
                restoreTimer2.Start();

                // death handled in UpdateGame on next tick

                return;
            }
        }
        // Safe resource loader: preserves animated GIF streams to keep animation working
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
                    Console.WriteLine($"ResourceToImage(Image) clone error: {ex}");
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
                        resourceStreams.Add(ms); // keep stream until form closes
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
                    Console.WriteLine($"ResourceToImage(byte[]) -> Image error: {ex}");
                    return null;
                }
            }

            return null;
        }

        private void CreateFallbackAnimations(Dictionary<string, Image> animations, Color baseColor)
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
                // Frame 1
                g.FillRectangle(new SolidBrush(baseColor), 0, 0, 80, 120);
                g.FillRectangle(new SolidBrush(Color.Black), 10, 100, 60, 20);

                // Frame 2
                g.FillRectangle(new SolidBrush(Lighten(baseColor, 0.2f)), 80, 0, 80, 120);
                g.FillRectangle(new SolidBrush(Color.Black), 90, 110, 60, 10);
            }
            return walkAnimation;
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

        private void DrawCharacter(Graphics g, int x, int y, string animation, string facing, Dictionary<string, Image> animations)
        {
            int screenX = x - viewportX;

            // nếu off-screen, bỏ qua
            if (screenX + PLAYER_WIDTH < 0 || screenX > this.ClientSize.Width)
                return;

            // Nếu có ảnh animation hiện tại
            if (animations.ContainsKey(animation) && animations[animation] != null)
            {
                Image characterImage = animations[animation];

                // Tính kích thước hiển thị dựa trên PLAYER_HEIGHT để giữ tỷ lệ ảnh gốc
                int drawHeight = PLAYER_HEIGHT; // chiều cao đích = PLAYER_HEIGHT (để chân đứng đúng)
                                                // tránh chia cho 0
                int imgW = Math.Max(1, characterImage.Width);
                int imgH = Math.Max(1, characterImage.Height);
                int drawWidth = Math.Max(1, (int)(drawHeight * (float)imgW / imgH));

                // Vị trí vẽ: sao cho "chân" sprite align với hitbox (y đã là top của sprite)
                int destX = screenX; // mặc định căn trái
                int destY = y; // top coordinate (đã bằng ground - PLAYER_HEIGHT)

                // Nếu muốn căn tâm/center thay vì căn trái, điều chỉnh destX:
                // destX = screenX + (PLAYER_WIDTH - drawWidth) / 2;

                // Lưu trạng thái đồ họa
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
                        // Flip horizontally: vẽ với negative width, nhưng phải đặt destX thành destX + drawWidth
                        g.DrawImage(
                            characterImage,
                            new Rectangle(destX + drawWidth, destY, -drawWidth, drawHeight),
                            new Rectangle(0, 0, imgW, imgH),
                            GraphicsUnit.Pixel);
                    }
                    else
                    {
                        g.DrawImage(
                            characterImage,
                            new Rectangle(destX, destY, drawWidth, drawHeight),
                            new Rectangle(0, 0, imgW, imgH),
                            GraphicsUnit.Pixel);
                    }
                }
                finally
                {
                    // Restore
                    g.InterpolationMode = prevInterpolation;
                    g.SmoothingMode = prevSmoothing;
                    g.PixelOffsetMode = prevPixelOffset;
                    g.CompositingQuality = prevCompositing;
                }
            }
            else
            {
                // fallback: vẽ hộp màu nếu không có ảnh
                using (var brush = new SolidBrush(Color.Magenta))
                {
                    g.FillRectangle(brush, screenX, y, PLAYER_WIDTH, PLAYER_HEIGHT);
                }
            }
        }

        private void UpdateCharacterSize()
        {
            int newHeight = Math.Max(24, (int)(this.ClientSize.Height * characterHeightRatio));
            int spriteOrigW = 64;
            int spriteOrigH = 64;
            if (player1Animations.ContainsKey("stand") && player1Animations["stand"] != null)
            {
                spriteOrigW = player1Animations["stand"].Width;
                spriteOrigH = player1Animations["stand"].Height;
            }
            PLAYER_HEIGHT = newHeight;
            PLAYER_WIDTH = Math.Max(16, (int)(PLAYER_HEIGHT * (float)spriteOrigW / spriteOrigH));
            groundLevel = Math.Max(0, this.ClientSize.Height - groundOffset);
            if (!player1Jumping) player1Y = groundLevel - PLAYER_HEIGHT;
            if (!player2Jumping) player2Y = groundLevel - PLAYER_HEIGHT;
        }

        private void SetupStatusBars()
        {
            int screenWidth = this.ClientSize.Width;
            int barWidth = screenWidth / 4;
            int barHeight = 20;
            int spacing = 5;
            int startY = 10;

            healthBar1 = new GameProgressBar
            {
                Location = new Point(20, startY),
                Size = new Size(barWidth, barHeight),
                Maximum = 100,
                Value = player1Health,
                CustomForeColor = Color.FromArgb(220, 50, 50),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            staminaBar1 = new GameProgressBar
            {
                Location = new Point(20, startY + barHeight + spacing),
                Size = new Size(barWidth, barHeight),
                Maximum = 100,
                Value = player1Stamina,
                CustomForeColor = Color.FromArgb(50, 220, 50),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            manaBar1 = new GameProgressBar
            {
                Location = new Point(20, startY + 2 * (barHeight + spacing)),
                Size = new Size(barWidth, barHeight),
                Maximum = 100,
                Value = player1Mana,
                CustomForeColor = Color.FromArgb(50, 100, 220),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            lblPlayer1Name = new Label
            {
                Text = username,
                Location = new Point(20, startY + 3 * (barHeight + spacing)),
                Size = new Size(barWidth, 20),
                ForeColor = Color.Cyan,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            healthBar2 = new GameProgressBar
            {
                Location = new Point(screenWidth - barWidth - 20, startY),
                Size = new Size(barWidth, barHeight),
                Maximum = 100,
                Value = player2Health,
                CustomForeColor = Color.FromArgb(220, 50, 50),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            staminaBar2 = new GameProgressBar
            {
                Location = new Point(screenWidth - barWidth - 20, startY + barHeight + spacing),
                Size = new Size(barWidth, barHeight),
                Maximum = 100,
                Value = player2Stamina,
                CustomForeColor = Color.FromArgb(50, 220, 50),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            manaBar2 = new GameProgressBar
            {
                Location = new Point(screenWidth - barWidth - 20, startY + 2 * (barHeight + spacing)),
                Size = new Size(barWidth, barHeight),
                Maximum = 100,
                Value = player2Mana,
                CustomForeColor = Color.FromArgb(50, 100, 220),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            lblPlayer2Name = new Label
            {
                Text = opponent,
                Location = new Point(screenWidth - barWidth - 20, startY + 3 * (barHeight + spacing)),
                Size = new Size(barWidth, 20),
                ForeColor = Color.Orange,
                Font = new Font("Arial", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.TopRight,
                BackColor = Color.Transparent
            };

            this.Controls.AddRange(new Control[] {
                healthBar1, staminaBar1, manaBar1, lblPlayer1Name,
                healthBar2, staminaBar2, manaBar2, lblPlayer2Name
            });
        }

        private void SetupControlsInfo()
        {
            lblControlsInfo = new Label
            {
                Text = "Player 1: A/D (Move) | W (Jump) | J (Punch) | K (Kick) | L (Special) | U (Parry)\n" +
                       "Player 2: ←/→ (Move) | ↑ (Jump) | Num1 (Punch) | Num2 (Kick) | Num3 (Special) | Num5 (Parry)",
                Location = new Point(this.ClientSize.Width / 2 - 300, this.ClientSize.Height - 60),
                Size = new Size(600, 40),
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(150, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.Add(lblControlsInfo);
        }

        private void SetupEventHandlers()
        {
            cmbBackground.SelectedIndexChanged += CmbBackground_SelectedIndexChanged;
            btnBack.Click += BtnBack_Click;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            this.KeyDown += BattleForm_KeyDown;
            this.KeyUp += BattleForm_KeyUp;
            this.Resize += BattleForm_Resize;
        }

        private void BattleForm_Resize(object sender, EventArgs e)
        {
            if (healthBar1 != null)
            {
                int screenWidth = this.ClientSize.Width;
                int barWidth = screenWidth / 4;

                healthBar1.Size = new Size(barWidth, 20);
                staminaBar1.Size = new Size(barWidth, 20);
                manaBar1.Size = new Size(barWidth, 20);
                lblPlayer1Name.Size = new Size(barWidth, 20);

                healthBar2.Location = new Point(screenWidth - barWidth - 20, 10);
                healthBar2.Size = new Size(barWidth, 20);
                staminaBar2.Location = new Point(screenWidth - barWidth - 20, 35);
                staminaBar2.Size = new Size(barWidth, 20);
                manaBar2.Location = new Point(screenWidth - barWidth - 20, 60);
                manaBar2.Size = new Size(barWidth, 20);
                lblPlayer2Name.Location = new Point(screenWidth - barWidth - 20, 85);
                lblPlayer2Name.Size = new Size(barWidth, 20);

                groundLevel = Math.Max(0, this.ClientSize.Height - groundOffset);
                UpdateCharacterSize();

                lblControlsInfo.Location = new Point(screenWidth / 2 - 300, this.ClientSize.Height - 60);
            }
        }

        private void WalkAnimationTimer_Tick(object sender, EventArgs e)
        {
            if (player1Walking && player1Animations.ContainsKey("walk"))
            {
                var img = player1Animations["walk"];
                if (img != null && ImageAnimator.CanAnimate(img))
                    ImageAnimator.UpdateFrames(img);
            }
            if (player2Walking && player2Animations.ContainsKey("walk"))
            {
                var img = player2Animations["walk"];
                if (img != null && ImageAnimator.CanAnimate(img))
                    ImageAnimator.UpdateFrames(img);
            }
        }

        private void BattleForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {// Player1 parry: key U
                case Keys.U:
                    StartParry(1);
                    break;

                // Player2 parry: key NumPad5
                case Keys.NumPad5:
                    StartParry(2);
                    break;
                case Keys.A: aPressed = true; break;
                case Keys.D: dPressed = true; break;
                case Keys.W:
                    if (!player1Jumping && player1Y >= groundLevel - PLAYER_HEIGHT)
                    {
                        player1Jumping = true;
                        player1JumpVelocity = JUMP_FORCE;
                    }
                    break;
                case Keys.J: Player1Attack("punch"); break;
                case Keys.K: Player1Attack("kick"); break;
                case Keys.L: Player1Attack("special"); break;
                case Keys.Escape:
                    BtnBack_Click(null, EventArgs.Empty);
                    break;
            }

            switch (e.KeyCode)
            {
                case Keys.Left: leftPressed = true; break;
                case Keys.Right: rightPressed = true; break;
                case Keys.Up:
                    if (!player2Jumping && player2Y >= groundLevel - PLAYER_HEIGHT)
                    {
                        player2Jumping = true;
                        player2JumpVelocity = JUMP_FORCE;
                    }
                    break;
                case Keys.NumPad1: Player2Attack("punch"); break;
                case Keys.NumPad2: Player2Attack("kick"); break;
                case Keys.NumPad3: Player2Attack("special"); break;
            }

            e.Handled = true;
        }

        private void BattleForm_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A: aPressed = false; break;
                case Keys.D: dPressed = false; break;
                case Keys.Left: leftPressed = false; break;
                case Keys.Right: rightPressed = false; break;
            }
            e.Handled = true;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            player1Walking = false;
            player2Walking = false;
            if (fireballActive)
            {
                fireballX += fireballSpeed * fireballDirection;
                CheckFireballHit();

                if (fireballX > backgroundWidth || fireballX < -FIREBALL_WIDTH)
                {
                    fireballActive = false;
                }
            }
            if (aPressed)
            {
                player1X -= playerSpeed;
                player1Facing = "left";
                player1Walking = true;
                if (!player1Parrying) player1CurrentAnimation = "walk";
            }
            if (dPressed)
            {
                player1X += playerSpeed;
                player1Facing = "right";
                player1Walking = true;
                if (!player1Parrying) player1CurrentAnimation = "walk";
            }

            if (leftPressed)
            {
                player2X -= playerSpeed;
                player2Facing = "left";
                player2Walking = true;
                if (!player2Parrying) player2CurrentAnimation = "walk";
            }
            if (rightPressed)
            {
                player2X += playerSpeed;
                player2Facing = "right";
                player2Walking = true;
                if (!player2Parrying) player2CurrentAnimation = "walk";
            }

            if (!player1Walking && !player1Attacking && !player1Jumping && !player1Parrying)
            {
                if (player1CurrentAnimation != "hurt" && player1CurrentAnimation != "parry")
                    player1CurrentAnimation = "stand";
            }
            if (!player2Walking && !player2Attacking && !player2Jumping && !player2Parrying)
            {
                if (player2CurrentAnimation != "hurt" && player2CurrentAnimation != "parry")
                    player2CurrentAnimation = "stand";
            }

            if ((player1Walking || player2Walking) && !walkAnimationTimer.Enabled)
            {
                walkAnimationTimer.Start();
            }
            else if (!player1Walking && !player2Walking && walkAnimationTimer.Enabled)
            {
                walkAnimationTimer.Stop();
            }

            if (player1Jumping)
            {
                player1Y += (int)player1JumpVelocity;
                player1JumpVelocity += GRAVITY;
                player1CurrentAnimation = "jump";

                if (player1Y >= groundLevel - PLAYER_HEIGHT)
                {
                    player1Y = groundLevel - PLAYER_HEIGHT;
                    player1Jumping = false;
                    player1JumpVelocity = 0;
                    if (aPressed || dPressed)
                    {
                        player1CurrentAnimation = "walk";
                        player1Walking = true;
                    }
                    else
                    {
                        player1CurrentAnimation = "stand";
                    }
                }
            }

            if (player2Jumping)
            {
                player2Y += (int)player2JumpVelocity;
                player2JumpVelocity += GRAVITY;
                player2CurrentAnimation = "jump";

                if (player2Y >= groundLevel - PLAYER_HEIGHT)
                {
                    player2Y = groundLevel - PLAYER_HEIGHT;
                    player2Jumping = false;
                    player2JumpVelocity = 0;
                    if (leftPressed || rightPressed)
                    {
                        player2CurrentAnimation = "walk";
                        player2Walking = true;
                    }
                    else
                    {
                        player2CurrentAnimation = "stand";
                    }
                }
            }

            player1X = Math.Max(0, Math.Min(backgroundWidth - PLAYER_WIDTH, player1X));
            player2X = Math.Max(0, Math.Min(backgroundWidth - PLAYER_WIDTH, player2X));

            UpdateCamera();
            UpdateGame();
            this.Invalidate();
        }

        private void UpdateCamera()
        {
            int centerX = (player1X + player2X) / 2;
            viewportX = centerX - this.ClientSize.Width / 2;
            viewportX = Math.Max(0, Math.Min(backgroundWidth - this.ClientSize.Width, viewportX));
        }

        private void UpdateGame()
        {
            if (player1Animations.ContainsKey(player1CurrentAnimation))
            {
                var img = player1Animations[player1CurrentAnimation];
                if (img != null && ImageAnimator.CanAnimate(img))
                    ImageAnimator.UpdateFrames(img);
            }
            if (player2Animations.ContainsKey(player2CurrentAnimation))
            {
                var img = player2Animations[player2CurrentAnimation];
                if (img != null && ImageAnimator.CanAnimate(img))
                    ImageAnimator.UpdateFrames(img);
            }
            if (fireball != null && fireballActive && ImageAnimator.CanAnimate(fireball))
            {
                ImageAnimator.UpdateFrames(fireball);
            }

            if (fireballActive)
            {
                fireballX += 12 * fireballDirection;
                CheckFireballHit();

                if (fireballX > backgroundWidth || fireballX < -FIREBALL_WIDTH)
                {
                    fireballActive = false;
                }
            }

            RegenerateResources();

            healthBar1.Value = Math.Max(0, Math.Min(100, player1Health));
            staminaBar1.Value = Math.Max(0, Math.Min(100, player1Stamina));
            manaBar1.Value = Math.Max(0, Math.Min(100, player1Mana));
            healthBar2.Value = Math.Max(0, Math.Min(100, player2Health));
            staminaBar2.Value = Math.Max(0, Math.Min(100, player2Stamina));
            manaBar2.Value = Math.Max(0, Math.Min(100, player2Mana));

            if (player1Health <= 0 || player2Health <= 0)
            {
                gameTimer.Stop();
                walkAnimationTimer.Stop();

                string winner;
                if (player1Health <= 0 && player2Health <= 0)
                    winner = "Draw";
                else
                    winner = player1Health <= 0 ? opponent : username;

                ShowGameOver(winner);
            }
        }

        private void RegenerateResources()
        {
            if (player1Stamina < 100) player1Stamina += 2;
            if (player2Stamina < 100) player2Stamina += 2;
            if (player1Mana < 100) player1Mana = Math.Min(100, player1Mana + 1);
            if (player2Mana < 100) player2Mana = Math.Min(100, player2Mana + 1);
        }

        private void Player1Attack(string attackType)
        {
            if (player1Attacking) return;

            player1Attacking = true;
            player1Walking = false;

            switch (attackType)
            {
                case "punch":
                    if (player1Stamina >= 10)
                    {
                        player1Stamina -= 10;
                        player1CurrentAnimation = "punch";
                        if (CheckCollisionRect(player1X, player1Y, PLAYER_WIDTH, PLAYER_HEIGHT, player2X, player2Y, PLAYER_WIDTH, PLAYER_HEIGHT))
                        {
                            ApplyHurtToPlayer(2, 10);
                            ShowHitEffect("Punch!", Color.Orange);
                        }
                        ResetAttackAnimation(300, 1);
                    }
                    break;
                case "kick":
                    if (player1Stamina >= 15)
                    {
                        player1Stamina -= 15;
                        player1CurrentAnimation = "kick";
                        if (CheckCollisionRect(player1X, player1Y, PLAYER_WIDTH, PLAYER_HEIGHT, player2X, player2Y, PLAYER_WIDTH, PLAYER_HEIGHT))
                        {
                            ApplyHurtToPlayer(2, 15);
                            ShowHitEffect("Kick!", Color.Red);
                        }
                        ResetAttackAnimation(400, 1);
                    }
                    break;
                case "special":
                    if (player1Mana >= 30)
                    {
                        player1Mana -= 30;
                        player1CurrentAnimation = "fireball";
                        int direction = player1Facing == "right" ? 1 : -1;
                        int startX = player1Facing == "right" ? player1X + PLAYER_WIDTH : player1X - FIREBALL_WIDTH;
                        ShootFireball(startX, player1Y + 30, direction, 1);
                        ResetAttackAnimation(500, 1);
                    }
                    break;
            }
        }

        private void Player2Attack(string attackType)
        {
            if (player2Attacking) return;

            player2Attacking = true;
            player2Walking = false;

            switch (attackType)
            {
                case "punch":
                    if (player2Stamina >= 10)
                    {
                        player2Stamina -= 10;
                        player2CurrentAnimation = "punch";
                        if (CheckCollisionRect(player2X, player2Y, PLAYER_WIDTH, PLAYER_HEIGHT, player1X, player1Y, PLAYER_WIDTH, PLAYER_HEIGHT))
                        {
                            ApplyHurtToPlayer(1, 10);
                            ShowHitEffect("Punch!", Color.Orange);
                        }
                        ResetAttackAnimation(300, 2);
                    }
                    break;
                case "kick":
                    if (player2Stamina >= 15)
                    {
                        player2Stamina -= 15;
                        player2CurrentAnimation = "kick";
                        if (CheckCollisionRect(player2X, player2Y, PLAYER_WIDTH, PLAYER_HEIGHT, player1X, player1Y, PLAYER_WIDTH, PLAYER_HEIGHT))
                        {
                            ApplyHurtToPlayer(1, 15);
                            ShowHitEffect("Kick!", Color.Red);
                        }
                        ResetAttackAnimation(400, 2);
                    }
                    break;
                case "special":
                    if (player2Mana >= 30)
                    {
                        player2Mana -= 30;
                        player2CurrentAnimation = "fireball";
                        int direction = player2Facing == "right" ? 1 : -1;
                        int startX = player2Facing == "right" ? player2X + PLAYER_WIDTH : player2X - FIREBALL_WIDTH;
                        ShootFireball(startX, player2Y + 30, direction, 2);
                        ResetAttackAnimation(500, 2);
                    }
                    break;
            }
        }

        // delay: ms, player: 0 = both, 1 = player1, 2 = player2
        private void ResetAttackAnimation(int delay, int player)
        {
            System.Windows.Forms.Timer resetTimer = new System.Windows.Forms.Timer();
            resetTimer.Interval = delay;
            resetTimer.Tick += (s, e) =>
            {
                if (player == 0 || player == 1) player1Attacking = false;
                if (player == 0 || player == 2) player2Attacking = false;

                // Only overwrite animations if target is not currently showing "hurt"
                if (!player1Attacking)
                {
                    if (player1CurrentAnimation != "hurt" && player1CurrentAnimation != "parry")
                    {
                        if (aPressed || dPressed)
                        {
                            player1CurrentAnimation = "walk";
                            player1Walking = true;
                        }
                        else
                        {
                            player1CurrentAnimation = "stand";
                        }
                    }
                }

                if (!player2Attacking)
                {
                    if (player2CurrentAnimation != "hurt" && player2CurrentAnimation != "parry")
                    {
                        if (leftPressed || rightPressed)
                        {
                            player2CurrentAnimation = "walk";
                            player2Walking = true;
                        }
                        else
                        {
                            player2CurrentAnimation = "stand";
                        }
                    }
                }

                resetTimer.Stop();
                resetTimer.Dispose();
            };
            resetTimer.Start();
        }

        private void ShootFireball(int x, int y, int direction, int owner)
        {
            fireballActive = true;
            fireballX = x;
            fireballY = y;
            fireballDirection = direction;
            fireballOwner = owner;
            if (fireball != null && ImageAnimator.CanAnimate(fireball))
                ImageAnimator.Animate(fireball, OnFrameChanged);
        }

        private void CheckFireballHit()
        {
            if (!fireballActive) return;

            Rectangle fireRect = new Rectangle(fireballX, fireballY, FIREBALL_WIDTH, FIREBALL_HEIGHT);

            if (fireballOwner == 1)
            {
                Rectangle p2Rect = new Rectangle(player2X, player2Y, PLAYER_WIDTH, PLAYER_HEIGHT);
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
                Rectangle p1Rect = new Rectangle(player1X, player1Y, PLAYER_WIDTH, PLAYER_HEIGHT);
                if (fireRect.IntersectsWith(p1Rect))
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
        }

        private bool CheckCollisionRect(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2)
        {
            Rectangle r1 = new Rectangle(x1, y1, w1, h1);
            Rectangle r2 = new Rectangle(x2, y2, w2, h2);
            return r1.IntersectsWith(r2);
        }

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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (background != null)
            {
                e.Graphics.DrawImage(background,
                    new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height),
                    new Rectangle(viewportX, 0, this.ClientSize.Width, this.ClientSize.Height),
                    GraphicsUnit.Pixel);
            }

            DrawCharacter(e.Graphics, player1X, player1Y, player1CurrentAnimation, player1Facing, player1Animations);
            DrawCharacter(e.Graphics, player2X, player2Y, player2CurrentAnimation, player2Facing, player2Animations);

            if (fireballActive && fireball != null)
            {
                int fireballScreenX = fireballX - viewportX;
                if (fireballScreenX >= -FIREBALL_WIDTH && fireballScreenX <= this.ClientSize.Width)
                {
                    e.Graphics.DrawImage(fireball, fireballScreenX, fireballY, FIREBALL_WIDTH, FIREBALL_HEIGHT);
                }
            }
            // visual indicator for parry
            if (player1Parrying)
            {
                int sx = player1X - viewportX + PLAYER_WIDTH / 2 - 12;
                e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(180, Color.Cyan)), sx, player1Y - 28, 24, 24);
            }
            if (player2Parrying)
            {
                int sx = player2X - viewportX + PLAYER_WIDTH / 2 - 12;
                e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(180, Color.Cyan)), sx, player2Y - 28, 24, 24);
            }
            DrawGameUI(e.Graphics);
        }

        private void DrawGameUI(Graphics g)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(180, 0, 0, 0)),
                0, 0, this.ClientSize.Width, 110);
        }

        private Bitmap CreateColoredImage(int width, int height, Color color)
        {
            var bmp = new Bitmap(Math.Max(1, width), Math.Max(1, height));
            using (var g = Graphics.FromImage(bmp))
            using (var brush = new SolidBrush(color))
                g.FillRectangle(brush, 0, 0, bmp.Width, bmp.Height);
            return bmp;
        }

        private void OnFrameChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void CmbBackground_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbBackground.SelectedIndex >= 0 && cmbBackground.SelectedIndex < backgroundImages.Count)
            {
                string bgName = backgroundImages[cmbBackground.SelectedIndex];
                currentBackground = cmbBackground.SelectedIndex;
                SetBackground(bgName);
            }
        }
        private void SetBackground(string backgroundName)
        {
            try
            {
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
                    background = new Bitmap(backgroundWidth, screenHeight);
                    using (var g = Graphics.FromImage(background))
                    {
                        for (int x = 0; x < backgroundWidth; x += originalBg.Width)
                        {
                            g.DrawImage(originalBg, x, 0, originalBg.Width, screenHeight);
                        }
                    }
                }
                else
                {
                    background = CreateColoredImage(backgroundWidth, this.ClientSize.Height, Color.DarkGreen);
                }

                this.Invalidate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading background: {ex.Message}");
                background = CreateColoredImage(backgroundWidth, this.ClientSize.Height, Color.DarkGreen);
            }
        }
        private void BtnBack_Click(object sender, EventArgs e)
        {
            try { gameTimer?.Stop(); } catch { }
            try { walkAnimationTimer?.Stop(); } catch { }

            this.Close();

            foreach (Form form in Application.OpenForms)
            {
                if (form is MainForm mainForm)
                {
                    mainForm.Show();
                    break;
                }
            }
        }

        private void ShowGameOver(string winner)
        {
            string result;
            if (winner == "Draw")
            {
                result = $"🤝 DRAW!\n\n{username}: {player1Health} HP\n{opponent}: {player2Health} HP";
            }
            else
            {
                result = $"🎉 {winner} WINS!\n\n" +
                       $"{username}: {player1Health} HP\n" +
                       $"{opponent}: {player2Health} HP";
            }

            MessageBox.Show(result, "BATTLE FINISHED",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            BtnBack_Click(null, EventArgs.Empty);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try { gameTimer?.Stop(); } catch { }
            try { walkAnimationTimer?.Stop(); } catch { }

            foreach (var s in resourceStreams)
            {
                try { s.Dispose(); } catch { }
            }
            resourceStreams.Clear();

            base.OnFormClosing(e);
        }
    }

}