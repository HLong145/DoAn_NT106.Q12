using System;
using System.Drawing;

namespace DoAn_NT106.Client.BattleSystems
{
    /// <summary>
    /// X? lý v?t lý: jump, gravity, movement
    /// </summary>
    public class PhysicsSystem
    {
        private const float GRAVITY = 1.5f;
        private const float JUMP_FORCE = -10f;

        private int groundLevel;
        private int playerSpeed = 14;
        private int backgroundWidth;
        private int playerWidth;
        private int playerHeight;

        public PhysicsSystem(int groundLevel, int backgroundWidth, int playerWidth, int playerHeight)
        {
            this.groundLevel = groundLevel;
            this.backgroundWidth = backgroundWidth;
            this.playerWidth = playerWidth;
            this.playerHeight = playerHeight;
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
                        player.ResetToIdle();
                    }
                }
            }
        }

        /// <summary>
        /// Move player horizontally
        /// </summary>
        public void MovePlayer(PlayerState player, int direction)
        {
            if (!player.CanMove) return;

            player.X += playerSpeed * direction;
            player.X = Math.Max(0, Math.Min(backgroundWidth - playerWidth, player.X));

            player.Facing = direction > 0 ? "right" : "left";
            player.IsWalking = true;

            if (!player.IsSkillActive && !player.IsJumping)
            {
                player.CurrentAnimation = "walk";
            }
        }

        /// <summary>
        /// Stop player movement
        /// </summary>
        public void StopMovement(PlayerState player)
        {
            player.IsWalking = false;

            if (!player.IsJumping && !player.IsAttacking && !player.IsParrying && !player.IsSkillActive)
            {
                player.CurrentAnimation = "stand";
            }
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
    }
}
