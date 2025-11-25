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
        private string player1Character = "girlknight"; // Mặc định
        private string player2Character = "girlknight"; // Mặc định

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
        private PlayerController player1Controller;
        private PlayerController player2Controller;
        // Kích thước nhân vật (dynamic)
        private int PLAYER_WIDTH = 80;
        private int PLAYER_HEIGHT = 120;
        private float characterHeightRatio = 0.30f; // relative to ClientSize.Height
        // Hurt handling
        private const int HURT_DISPLAY_MS = 400;
        private string _prevAnimPlayer1 = null;
        private string _prevAnimPlayer2 = null;

        public BattleForm(string username, string token, string opponent, string player1Character, string player2Character)
        {
            InitializeComponent();
            this.username = username;
            this.token = token;
            this.opponent = opponent;

            // ✅ NHẬN CHARACTER TỪ THAM SỐ
            this.player1Character = player1Character;
            this.player2Character = player2Character;

            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;

            // ✅ KHỞI TẠO VỚI CHARACTER ĐÚNG
            player1Controller = new PlayerController(1, username, this.player1Character);
            player2Controller = new PlayerController(2, opponent, this.player2Character);

            groundLevel = Math.Max(0, this.ClientSize.Height - groundOffset);

            player1Controller.SetGroundLevel(groundLevel);
            player2Controller.SetGroundLevel(groundLevel);

            player1Controller.SetPosition(300, groundLevel - PLAYER_HEIGHT);
            player2Controller.SetPosition(600, groundLevel - PLAYER_HEIGHT);
            player1Controller.Facing = "right";
            player2Controller.Facing = "left";

            player1Controller.OnAttack += HandlePlayerAttack;
            player2Controller.OnAttack += HandlePlayerAttack;
            player1Controller.OnFireball += HandleFireball;
            player2Controller.OnFireball += HandleFireball;
            player1Controller.OnParrySuccess += HandleParrySuccess;
            player2Controller.OnParrySuccess += HandleParrySuccess;

            SetupGame();
            SetupEventHandlers();
            this.Text = $"⚔️ {username} ({this.player1Character}) vs {opponent} ({this.player2Character})";
            this.DoubleBuffered = true;
            this.KeyPreview = true;
        }

        private void HandlePlayerAttack(PlayerController attacker, string attackType, int damage)
        {
            PlayerController target = attacker.PlayerId == 1 ? player2Controller : player1Controller;
            if (attacker.CollidesWith(target))
            {
                bool hit = target.TakeDamage(damage, attacker);
                if (hit)
                {
                    ShowHitEffect($"{attackType}!", attackType == "kick" ? Color.Red : Color.Orange);
                }
                else
                {
                    ShowHitEffect("Blocked!", Color.Cyan);
                }
            }
        }

        private void HandleFireball(PlayerController shooter, int x, int y, int direction)
        {
            fireballActive = true;
            fireballX = x;
            fireballY = y;
            fireballDirection = direction;
            fireballOwner = shooter.PlayerId;
            if (fireball != null && ImageAnimator.CanAnimate(fireball))
                ImageAnimator.Animate(fireball, OnFrameChanged);
        }

        private void HandleParrySuccess(PlayerController player)
        {
            ShowHitEffect("Parry!", Color.Cyan);
        }

        private void SetupGame()
        {
            try
            {
                // ✅ DISPOSE TIMERS CŨ TRƯỚC KHI TẠO MỚI
                walkAnimationTimer?.Stop();
                walkAnimationTimer?.Dispose();

                p1ParryTimer?.Stop();
                p1ParryTimer?.Dispose();
                p1ParryCooldownTimer?.Stop();
                p1ParryCooldownTimer?.Dispose();

                p2ParryTimer?.Stop();
                p2ParryTimer?.Dispose();
                p2ParryCooldownTimer?.Stop();
                p2ParryCooldownTimer?.Dispose();
                // Load animations - sử dụng character đã chọn
                LoadCharacterAnimations(this.player1Character, player1Controller.Animations);
                LoadCharacterAnimations(this.player2Character, player2Controller.Animations);
                player1Animations = player1Controller.Animations;
                player2Animations = player2Controller.Animations;
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
                    if (!player1Controller.IsAttacking && !player1Controller.IsJumping)
                    {
                        // Use PlayerController's current animation state after parry
                        player1CurrentAnimation = player1Controller.GetCurrentAnimationName();
                    }
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
                    if (!player2Controller.IsAttacking && !player2Controller.IsJumping)
                    {
                        player2CurrentAnimation = player2Controller.GetCurrentAnimationName();
                    }
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
                Console.WriteLine($"🎨 Loading animations for: {characterName}");
                foreach (var kvp in animations.ToList())
                {
                    if (kvp.Value != null)
                    {
                        kvp.Value.Dispose();
                    }
                }
                animations.Clear();
                switch (characterName.ToLower())
                {
                    case "girlknight":
                        animations["stand"] = SafeLoadAnimationImage(Properties.Resources.girlknight_stand, "girlknight_stand");
                        animations["walk"] = SafeLoadAnimationImage(Properties.Resources.girlknight_walk, "girlknight_walk");
                        animations["punch"] = SafeLoadAnimationImage(Properties.Resources.girlknight_attack, "girlknight_attack");
                        animations["kick"] = SafeLoadAnimationImage(Properties.Resources.girlknight_kick, "girlknight_kick");
                        animations["jump"] = SafeLoadAnimationImage(Properties.Resources.girlknight_jump, "girlknight_jump");
                        animations["hurt"] = SafeLoadAnimationImage(Properties.Resources.girlknight_hurt, "girlknight_hurt");
                        animations["parry"] = SafeLoadAnimationImage(Properties.Resources.girlknight_parry, "girlknight_parry");
                        animations["slide"] = SafeLoadAnimationImage(Properties.Resources.girlknight_walk, "girlknight_slide"); // Dùng walk nếu không có slide
                        animations["fireball"] = SafeLoadAnimationImage(Properties.Resources.girlknight_fireball, "girlknight_fireball");
                        break;

                    case "bringerofdeath":
                        animations["stand"] = SafeLoadAnimationImage(Properties.Resources.bringerofdeath_Idle, "bringerofdeath_idle");
                        animations["walk"] = SafeLoadAnimationImage(Properties.Resources.bringerofdeath_Walk, "bringerofdeath_walk");
                        animations["punch"] = SafeLoadAnimationImage(Properties.Resources.bringerofdeath_Attack1, "bringerofdeath_attack");
                        animations["kick"] = SafeLoadAnimationImage(Properties.Resources.bringerofdeath_Attack2, "bringerofdeath_attack2");
                        animations["jump"] = SafeLoadAnimationImage(Properties.Resources.bringerofdeath_Walk, "bringerofdeath_jump");
                        animations["hurt"] = SafeLoadAnimationImage(Properties.Resources.bringerofdeathHurt, "bringerofdeath_hurt");
                        animations["parry"] = SafeLoadAnimationImage(Properties.Resources.bringerofdeath_Parry, "bringerofdeath_parry"); // Dùng idle nếu không có
                        animations["slide"] = SafeLoadAnimationImage(Properties.Resources.bringerofdeath_Walk, "bringerofdeath_slide");
                        animations["fireball"] = SafeLoadAnimationImage(Properties.Resources.bringerofdeath_Cast, "bringerofdeath_spell");
                        break;

                    default:
                        Console.WriteLine($"❌ Unknown character: {characterName}, using fallback");
                        CreateFallbackAnimations(animations, Color.Pink);
                        return;
                }

                // Start animation cho các ảnh GIF
                foreach (var kvp in animations)
                {
                    if (kvp.Value != null && ImageAnimator.CanAnimate(kvp.Value))
                    {
                        ImageAnimator.Animate(kvp.Value, OnFrameChanged);
                        Console.WriteLine($"✅ Animated: {kvp.Key}");
                    }
                }

                Console.WriteLine($"✅ Loaded {animations.Count} animations for {characterName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading {characterName} animations: {ex.Message}");
                CreateFallbackAnimations(animations, characterName == "girlknight" ? Color.Pink : Color.Purple);
            }
        }
        private Image SafeLoadAnimationImage(object resource, string resourceName)
        {
            try
            {
                if (resource == null)
                {
                    Console.WriteLine($"⚠️ Resource is null: {resourceName}");
                    return CreateColoredImage(80, 120, Color.Gray);
                }

                // Nếu là Image object
                if (resource is Image img)
                {
                    // Không clone nếu là GIF animation, giữ nguyên để animation hoạt động
                    if (ImageAnimator.CanAnimate(img))
                    {
                        Console.WriteLine($"🎬 Loaded animated GIF: {resourceName}");
                        return img; // Giữ nguyên, không clone
                    }
                    else
                    {
                        return new Bitmap(img); // Clone nếu là ảnh tĩnh
                    }
                }

                // Nếu là byte array
                if (resource is byte[] bytes && bytes.Length > 0)
                {
                    var ms = new System.IO.MemoryStream(bytes);
                    var image = Image.FromStream(ms);

                    // Kiểm tra xem có phải là GIF animation không
                    if (ImageAnimator.CanAnimate(image))
                    {
                        // ❗ QUAN TRỌNG: GIỮ LUỒNG MỞ VÀ THÊM VÀO LIST ĐỂ DISPOSE SAU
                        resourceStreams.Add(ms); // Thêm vào list để dispose sau
                        Console.WriteLine($"🎬 Loaded animated GIF from bytes: {resourceName}");

                        // Bắt đầu animation
                        ImageAnimator.Animate(image, OnFrameChanged);

                        return image;
                    }
                    else
                    {
                        // Nếu là ảnh tĩnh, clone và dispose luồng
                        var bmp = new Bitmap(image);
                        image.Dispose();
                        ms.Dispose();
                        return bmp;
                    }
                }

                Console.WriteLine($"⚠️ Unknown resource type for {resourceName}: {resource.GetType()}");
                return CreateColoredImage(80, 120, Color.Gray);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading {resourceName}: {ex.Message}");
                return CreateColoredImage(80, 120, Color.Gray);
            }
        }
        // Apply hurt properly so it's not immediately overwritten by the attack reset logic
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

        private void DrawCharacter(Graphics g, int x, int y, string animation, string facing, Dictionary<string, Image> animations, string characterNameForScaling)
        {
            int screenX = x - viewportX;
            if (screenX + PLAYER_WIDTH < 0 || screenX > this.ClientSize.Width)
                return;
            if (animations.ContainsKey(animation) && animations[animation] != null)
            {
                Image characterImage = animations[animation];
                // ✅ SCALE FACTOR DỰA TRÊN characterNameForScaling
                float scaleFactor = 1.0f;
                if (characterNameForScaling.ToLower().Contains("bringerofdeath"))
                {
                    scaleFactor = 2.5f; // Phóng to 2.5 lần
                }
                // Cố gắng lấy kích thước gốc của ảnh (tránh lỗi nếu Width/Height = 0)
                int imgW = Math.Max(1, characterImage.Width);
                int imgH = Math.Max(1, characterImage.Height);

                // Tính toán kích thước vẽ dựa trên PLAYER_HEIGHT và tỷ lệ khung hình của ảnh
                int drawHeight = PLAYER_HEIGHT;
                int drawWidth = Math.Max(1, (int)(drawHeight * (float)imgW / imgH * scaleFactor));

                int destX = screenX;
                int destY = y;

                // Lưu trữ các chế độ hiện tại của Graphics
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
                    // Luôn khôi phục chế độ cũ sau khi vẽ
                    g.InterpolationMode = prevInterpolation;
                    g.SmoothingMode = prevSmoothing;
                    g.PixelOffsetMode = prevPixelOffset;
                    g.CompositingQuality = prevCompositing;
                }
            }
            else
            {
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
                Text = "Player 1: A/D (Move) | W (Jump) | J (Punch) | K (Kick) | L (Slide) | U (Parry)\n" +
                       "Player 2: ←/→ (Move) | ↑ (Jump) | Num1 (Punch) | Num2 (Kick) | Num3 (Slide) | Num5 (Parry)",
                Location = new Point(this.ClientSize.Width / 2 - 350, this.ClientSize.Height - 60),
                Size = new Size(700, 40),
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
                player1Controller?.SetGroundLevel(groundLevel);
                player2Controller?.SetGroundLevel(groundLevel);
                player1Controller?.SetSize(PLAYER_WIDTH, PLAYER_HEIGHT);
                player2Controller?.SetSize(PLAYER_WIDTH, PLAYER_HEIGHT);
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
            // Player 1 controls
            player1Controller.HandleKeyDown(e.KeyCode);
            // Player 2 controls
            player2Controller.HandleKeyDown(e.KeyCode);
            // Special keys
            if (e.KeyCode == Keys.Escape)
            {
                BtnBack_Click(null, EventArgs.Empty);
            }
            e.Handled = true;
        }

        private void BattleForm_KeyUp(object sender, KeyEventArgs e)
        {
            player1Controller.HandleKeyUp(e.KeyCode);
            player2Controller.HandleKeyUp(e.KeyCode);
            e.Handled = true;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // Update controllers
            player1Controller.Update(backgroundWidth);
            player2Controller.Update(backgroundWidth);
            // Sync positions từ controller
            player1X = player1Controller.X;
            player1Y = player1Controller.Y;
            player2X = player2Controller.X;
            player2Y = player2Controller.Y;
            // Sync animations
            player1CurrentAnimation = player1Controller.GetCurrentAnimationName();
            player2CurrentAnimation = player2Controller.GetCurrentAnimationName();
            player1Facing = player1Controller.Facing;
            player2Facing = player2Controller.Facing;
            // Sync stats
            player1Health = player1Controller.Health;
            player2Health = player2Controller.Health;
            player1Stamina = player1Controller.Stamina;
            player2Stamina = player2Controller.Stamina;
            player1Mana = player1Controller.Mana;
            player2Mana = player2Controller.Mana;
            // Update parry state
            player1Parrying = player1Controller.IsParrying;
            player2Parrying = player2Controller.IsParrying;
            // Fireball update
            if (fireballActive)
            {
                fireballX += fireballSpeed * fireballDirection;
                CheckFireballHit();
                if (fireballX > backgroundWidth || fireballX < -FIREBALL_WIDTH)
                {
                    fireballActive = false;
                }
            }
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

        // Hàm này đã bị xóa vì PlayerController tự quản lý animation state
        // private void ResetAttackAnimation(int delay, int player) { ... }

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
                if (player2Controller.CollidesWith(fireRect))
                {
                    bool hit = player2Controller.TakeFireballDamage(20, fireballOwner);
                    if (hit)
                    {
                        fireballActive = false;
                        ShowHitEffect("Fireball Hit!", Color.Yellow);
                    }
                    else
                    {
                        // Reflected
                        fireballDirection *= -1;
                        fireballOwner = 2;
                        ShowHitEffect("Reflected!", Color.Orange);
                    }
                }
            }
            else if (fireballOwner == 2)
            {
                if (player1Controller.CollidesWith(fireRect))
                {
                    bool hit = player1Controller.TakeFireballDamage(20, fireballOwner);
                    if (hit)
                    {
                        fireballActive = false;
                        ShowHitEffect("Fireball Hit!", Color.Yellow);
                    }
                    else
                    {
                        fireballDirection *= -1;
                        fireballOwner = 1;
                        ShowHitEffect("Reflected!", Color.Orange);
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
            // Gọi DrawCharacter với tên character tương ứng
            DrawCharacter(e.Graphics, player1X, player1Y, player1CurrentAnimation, player1Facing, player1Animations, this.player1Character);
            DrawCharacter(e.Graphics, player2X, player2Y, player2CurrentAnimation, player2Facing, player2Animations, this.player2Character);

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
                result = $"🤝 DRAW!\n{username}: {player1Health} HP\n{opponent}: {player2Health} HP";
            }
            else
            {
                result = $"🎉 {winner} WINS!\n" +
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

            // ✅ DISPOSE CONTROLLERS
            try
            {
                player1Controller?.Dispose();
                player2Controller?.Dispose();
            }
            catch { }

            // ✅ DISPOSE ANIMATIONS
            foreach (var kvp in player1Animations.ToList())
            {
                try { kvp.Value?.Dispose(); } catch { }
            }
            player1Animations.Clear();

            foreach (var kvp in player2Animations.ToList())
            {
                try { kvp.Value?.Dispose(); } catch { }
            }
            player2Animations.Clear();

            // ✅ DISPOSE ALL RESOURCE STREAMS
            foreach (var s in resourceStreams)
            {
                try { s.Dispose(); } catch { }
            }
            resourceStreams.Clear();

            base.OnFormClosing(e);
        }
    }
}