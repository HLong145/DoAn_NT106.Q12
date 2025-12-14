using System;
using System.Collections.Generic;
using System.Drawing;

namespace DoAn_NT106.Client.BattleSystems
{
    /// <summary>
    /// Xử lý vật lý: jump, gravity, movement
    /// </summary>
    public class PhysicsSystem
    {
        private const float GRAVITY = 1.5f;
        private const float JUMP_FORCE = -10f;

        private int groundLevel;
        private int playerSpeed = 18; // increased base speed from 14 -> 18
        private int backgroundWidth;
        private int playerWidth;
        private int playerHeight;
        private Func<PlayerState, Rectangle> getPlayerHurtboxCallback;
        public PhysicsSystem(int groundLevel, int backgroundWidth, int playerWidth, int playerHeight,
                        Func<PlayerState, Rectangle> getPlayerHurtboxCallback = null)
        {
            this.groundLevel = groundLevel;
            this.backgroundWidth = backgroundWidth;
            this.playerWidth = playerWidth;
            this.playerHeight = playerHeight;
            this.getPlayerHurtboxCallback = getPlayerHurtboxCallback;
        }
        /// <summary>
        /// Get boundary based on actual hurtbox position
        /// </summary>
        private (int minX, int maxX) GetBoundaryFromHurtbox(PlayerState player)
        {
            if (getPlayerHurtboxCallback == null)
            {
                return (0, backgroundWidth - playerWidth);
            }

            Rectangle hurtbox = getPlayerHurtboxCallback(player);

            // Tính toán dựa trên hurtbox thực tế
            // hurtbox.X là vị trí thực tế của nhân vật
            // player.X là vị trí góc trái sprite

            int offsetFromSprite = hurtbox.X - player.X;

            // MinX: khi hurtbox chạm biên trái
            int minX = 0 - offsetFromSprite;

            // MaxX: khi hurtbox chạm biên phải
            int maxX = backgroundWidth - hurtbox.Width - offsetFromSprite;

            return (minX, maxX);
        }
      
        /// <summary>
        /// Update ground level (when window resizes)
        /// </summary>
        public void UpdateGroundLevel(int newGroundLevel)
        {
            groundLevel = newGroundLevel;
        }

        /// <summary>
        /// Update player size
        /// </summary>
        public void UpdatePlayerSize(int width, int height)
        {
            playerWidth = width;
            playerHeight = height;
        }

        /// <summary>
        /// Make player jump
        /// </summary>
        public void Jump(PlayerState player)
        {
            // ✅ Chặn nhảy khi skill đang active
            if (player.IsSkillActive)
            {
                return;
            }

            if (!player.IsJumping && player.Y >= groundLevel - playerHeight && player.CanMove)
            {
                player.IsJumping = true;
                player.JumpVelocity = JUMP_FORCE;
            }
        }

        /// <summary>
        /// Update jump physics
        /// </summary>
        public void UpdateJump(PlayerState player)
        {
            if (player.IsJumping)
            {
                player.JumpVelocity += GRAVITY;
                player.Y += (int)player.JumpVelocity;

                player.CurrentAnimation = "jump";

                // Land
                if (player.Y >= groundLevel - playerHeight)
                {
                    player.Y = groundLevel - playerHeight;
                    player.IsJumping = false;
                    player.JumpVelocity = 0;

                    if (!player.IsAttacking && !player.IsStunned && !player.IsParrying)
                    {
                        // ✅ FIX: Nếu đang bấm phím, set walk; nếu không thì stand
                        // Kiểm tra key state để quyết định animation
                        if (player.LeftKeyPressed || player.RightKeyPressed)
                        {
                            player.CurrentAnimation = "walk";
                            player.IsWalking = true;
                        }
                        else
                        {
                            player.CurrentAnimation = "stand";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Move player horizontally - ✅ SAME BOUNDARY FOR ALL CHARACTERS
        /// </summary>
        public void MovePlayer(PlayerState player, int direction)
        {
            if (!player.CanMove) return;

            // ✅ THÊM: Character-specific movement speeds
            int moveSpeed = playerSpeed;
            if (player.CharacterType == "bringerofdeath")
            {
                moveSpeed = (int)(playerSpeed * 0.9f); // 0.9x speed
            }
            else
            if (player.CharacterType == "goatman")
            {
                moveSpeed = (int)(playerSpeed * 0.8f); // 0.8x speed
            }
            else if (player.CharacterType == "warrior")
            {
                moveSpeed = (int)(playerSpeed * 1.2f); // ✅ SỬA: Warrior 1.2x speed
            }

            // Smooth horizontal movement: interpolate toward desired position to reduce jitter
            int prevX = player.X;
            float desiredX = player.X + moveSpeed * direction;

            // smoothing factor: closer to 1 -> snappier (faster response)
            const float smoothing = 0.9f; // increased to make movement more responsive

            float newXf = player.X + (desiredX - player.X) * smoothing;
            int newX = (int)Math.Round(newXf);

            player.X = newX;

            // update velocity estimate
            player.VelocityX = player.X - prevX;

            var boundary = GetBoundaryFromHurtbox(player);
            player.X = Math.Max(boundary.minX, Math.Min(boundary.maxX, player.X));

            player.Facing = direction > 0 ? "right" : "left";
            player.IsWalking = true;

            // Only set walk animation if NOT jumping or in a skill
            if (!player.IsSkillActive && !player.IsJumping)
            {
                player.CurrentAnimation = "walk";
            }
        }

        /// <summary>
        /// Clamp player position to map bounds - ✅ SAME BOUNDARY FOR DASH/SKILL
        /// </summary>
        public void ClampToMapBounds(PlayerState player)
        {
            player.X = Math.Max(0, Math.Min(backgroundWidth - playerWidth, player.X));
        }

        // ✅ THÊM: Track position để kiểm tra walk movement
        private Dictionary<int, int> lastPlayerX = new Dictionary<int, int>();
        // ✅ THÊM: Counter để kiểm tra mỗi 2 frame (32ms)
        private Dictionary<int, int> walkCheckFrameCounter = new Dictionary<int, int>();

        /// <summary>
        /// Stop player movement
        /// </summary>
        public void StopMovement(PlayerState player)
        {
            player.IsWalking = false;
            // Apply soft deceleration to create smoother stopping
            // If there's residual horizontal velocity, apply a damped step
            if (Math.Abs(player.VelocityX) > 0.5f)
            {
                // apply velocity and damp
                player.X += (int)Math.Round(player.VelocityX);
                player.VelocityX *= 0.55f; // damping factor

                // keep walk animation while still sliding
                if (!player.IsJumping && !player.IsSkillActive)
                {
                    player.CurrentAnimation = "walk";
                    player.IsWalking = true;
                }
                return;
            }

            // No residual velocity: fully stop
            player.VelocityX = 0;
            if (!player.IsJumping && !player.IsAttacking && !player.IsParrying && !player.IsSkillActive)
            {
                if (player.CurrentAnimation != "walk" && player.CurrentAnimation != "jump")
                {
                    player.CurrentAnimation = "stand";
                }
            }
        }

        /// <summary>
        /// ✅ THÊM: Kiểm tra vị trí walk - gọi từ GameTimer_Tick mỗi 16ms
        /// Nhưng chỉ thực sự kiểm tra mỗi 2 frame (32ms) để tránh UDP chưa kịp
        /// Nếu vị trí không đổi → quay về stand
        /// </summary>
        public void CheckWalkAnimation(PlayerState player)
        {
            int playerNum = player.PlayerNumber;

            // ✅ ĐIỀU KIỆN: Chỉ kiểm tra nếu đang ở animation walk
            if (player.CurrentAnimation != "walk")
            {
                // Xóa entry cũ nếu animation thay đổi
                if (lastPlayerX.ContainsKey(playerNum))
                    lastPlayerX.Remove(playerNum);
                if (walkCheckFrameCounter.ContainsKey(playerNum))
                    walkCheckFrameCounter.Remove(playerNum);
                return;
            }

            // ✅ COUNTER: Kiểm tra mỗi 2 frame (32ms)
            if (!walkCheckFrameCounter.ContainsKey(playerNum))
                walkCheckFrameCounter[playerNum] = 0;

            walkCheckFrameCounter[playerNum]++;

            // ✅ Chỉ kiểm tra khi counter == 2
            if (walkCheckFrameCounter[playerNum] < 2)
                return; // Chưa đủ 2 frame, chờ frame tiếp theo

            // Reset counter
            walkCheckFrameCounter[playerNum] = 0;

            // ✅ Lần đầu tiên: lưu vị trí hiện tại
            if (!lastPlayerX.ContainsKey(playerNum))
            {
                lastPlayerX[playerNum] = player.X;
                return;
            }

            // ✅ So sánh vị trí hiện tại vs vị trí trước (cách nhau 32ms)
            int currentX = player.X;
            int previousX = lastPlayerX[playerNum];

            // Nếu vị trí KHÔNG đổi trong 32ms → quay về stand
            if (currentX == previousX)
            {
                Console.WriteLine($"[PhysicsSystem] Player {playerNum} walk check: Position unchanged ({currentX}) after 32ms, setting to STAND");
                player.CurrentAnimation = "stand";
                lastPlayerX.Remove(playerNum);
                walkCheckFrameCounter.Remove(playerNum);
                return;
            }

            // Nếu vị trí ĐÃ ĐỔIE → cập nhật vị trí cũ
            lastPlayerX[playerNum] = currentX;
        }

        /// <summary>
        /// Reset player position to ground
        /// </summary>
        public void ResetToGround(PlayerState player)
        {
            if (!player.IsJumping)
            {
                player.Y = groundLevel - playerHeight;
            }
        }

        /// <summary>
        /// ✅ THÊM: Áp dụng world boundaries - ngăn player đi ra ngoài bản đồ
        /// </summary>
        public void ApplyWorldBoundaries(PlayerState player, int worldWidth, int playerWidth)
        {
            // Ngăn player đi ra ngoài trái
            if (player.X < 0)
            {
                player.X = 0;
            }
            
            // Ngăn player đi ra ngoài phải
            if (player.X > worldWidth - playerWidth)
            {
                player.X = worldWidth - playerWidth;
            }
        }

        /// <summary>
        /// ✅ THÊM: Áp dụng knockback với dự đoán boundary
        /// Giảm knockback nếu dự đoán vị trí vượt biên
        /// </summary>
        public void ApplyKnockbackWithBoundary(PlayerState player, int knockbackForce, string direction, 
                                              int worldWidth, int playerWidth)
        {
            // Dự đoán vị trí sau knockback
            int predictedX = direction == "left" ? 
                player.X - (knockbackForce * 2) : 
                player.X + (knockbackForce * 2);
            
            // Nếu dự đoán ra ngoài biên, giảm knockback
            if (predictedX < 0)
            {
                // Giảm knockback để dừng ở biên
                int movement = player.X > 0 ? -player.X / 2 : 0;
                player.X += movement;
                Console.WriteLine($"[Physics] Reduced left knockback to avoid boundary");
            }
            else if (predictedX > worldWidth - playerWidth)
            {
                // Giảm knockback để dừng ở biên
                int remaining = worldWidth - playerWidth - player.X;
                int movement = remaining > 0 ? remaining / 2 : 0;
                player.X += movement;
                Console.WriteLine($"[Physics] Reduced right knockback to avoid boundary");
            }
            else
            {
                // Áp dụng knockback bình thường
                if (direction == "left")
                {
                    player.X -= knockbackForce;
                }
                else
                {
                    player.X += knockbackForce;
                }
            }
        }
    }
}
