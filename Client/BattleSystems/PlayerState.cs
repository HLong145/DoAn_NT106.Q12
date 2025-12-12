using System;
using System.Drawing;
using System.Windows.Forms;

namespace DoAn_NT106.Client.BattleSystems
{
    /// <summary>
    /// Quản lý trạng thái của một player trong trận đấu
    /// </summary>
    public class PlayerState
    {
        // Player info
        public string PlayerName { get; set; }
        public string CharacterType { get; set; }
        public int PlayerNumber { get; set; } // 1 or 2

        // Position and physics
        public int X { get; set; }
        public int Y { get; set; }
        public string Facing { get; set; } = "right";
        public bool IsJumping { get; set; }
        public float JumpVelocity { get; set; }

        // Animation state
        public string CurrentAnimation { get; set; } = "stand";
        public bool IsWalking { get; set; }
        public bool IsAttacking { get; set; }

        // Combat state
        public bool IsStunned { get; set; }
        public bool IsParrying { get; set; }
        public bool IsParryOnCooldown { get; set; }
        public bool IsDashing { get; set; }
        public bool IsCharging { get; set; }
        public bool IsSkillActive { get; set; }

        // Resources
        public int Health { get; set; } = 100;
        public int Stamina { get; set; } = 100;
        public int Mana { get; set; } = 0; // ✅ Khởi đầu với 0 (không phải 100)

        // Dash state
        public int DashDirection { get; set; }

        // Charge state (for Goatman)
        public float ChargeSpeed { get; set; }

        // Key states
        public bool LeftKeyPressed { get; set; }
        public bool RightKeyPressed { get; set; }

        // =====================
        // Networking / interpolation
        // =====================
        // Target position received from network (remote authoritative snapshot)
        public int TargetX { get; set; }
        public int TargetY { get; set; }

        // Timestamp of last network update
        public long LastNetworkUpdateMs { get; set; }
        // Last received network sequence number for snapshot ordering
        public uint LastReceivedSeq { get; set; } = 0;
        // Previous target to compute velocity
        private int prevTargetX;
        private int prevTargetY;
        // Estimated velocity (pixels per second)
        private float estVelX = 0f;
        private float estVelY = 0f;
        // Timestamp of last time this state took damage locally (Environment.TickCount ms)
        public long LastDamageTimeMs { get; set; } = 0;
        // Last damage amount applied locally (used to deduplicate network damage events)
        public int LastDamageAmount { get; set; } = 0;
        // Snapshot buffer for interpolation-based smoothing (time-based)
        private struct Snapshot
        {
            public int X;
            public int Y;
            public uint Seq;
            public long ServerTsMs; // server-provided timestamp (if available)
            public long RecvMs; // local receive time (Environment.TickCount)
            // optional metadata
            public string Facing;
            public string Action;
            public int Health;
        }

        private readonly List<Snapshot> snapshotBuffer = new List<Snapshot>();
        // Buffer target delay in milliseconds (render snapshot time = now - bufferMs)
        // Typical values: 50..100ms. Lower -> more responsive but more jitter; higher -> smoother but more latency.
        public int InterpolationDelayMs { get; set; } = 80; // default 80ms (tune 50..100)

        /// <summary>
        /// Update state from a received network snapshot (seq and timestamp optional).
        /// Drop if older-than-last-received-seq. Compute velocity estimate for extrapolation.
        /// </summary>
        public void UpdateFromSnapshot(int x, int y, int seq = 0, long timestampMs = 0)
        {
            try
            {
                // If seq provided and older, drop
                if (seq != 0 && seq <= LastReceivedSeq) return;

                var now = Environment.TickCount;
                var snap = new Snapshot
                {
                    X = x,
                    Y = y,
                    Seq = (uint)seq,
                    ServerTsMs = timestampMs > 0 ? timestampMs : 0,
                    RecvMs = now,
                    Facing = this.Facing,
                    Action = this.CurrentAnimation,
                    Health = this.Health
                };

                // Insert keeping order by ServerTsMs when present else by RecvMs
                long key = snap.ServerTsMs > 0 ? snap.ServerTsMs : snap.RecvMs;
                int insertAt = snapshotBuffer.Count;
                for (int i = snapshotBuffer.Count - 1; i >= 0; --i)
                {
                    var existing = snapshotBuffer[i];
                    long existingKey = existing.ServerTsMs > 0 ? existing.ServerTsMs : existing.RecvMs;
                    if (existingKey <= key)
                    {
                        insertAt = i + 1;
                        break;
                    }
                    insertAt = i;
                }
                snapshotBuffer.Insert(insertAt, snap);

                // Trim buffer to reasonable size
                if (snapshotBuffer.Count > 60) snapshotBuffer.RemoveRange(0, snapshotBuffer.Count - 60);

                // update TargetX/TargetY for compatibility
                TargetX = x;
                TargetY = y;

                // estimate velocity from last two snapshots (use server timestamps if available)
                if (snapshotBuffer.Count >= 2)
                {
                    var last = snapshotBuffer[snapshotBuffer.Count - 1];
                    var prev = snapshotBuffer[snapshotBuffer.Count - 2];
                    long tLast = last.ServerTsMs > 0 ? last.ServerTsMs : last.RecvMs;
                    long tPrev = prev.ServerTsMs > 0 ? prev.ServerTsMs : prev.RecvMs;
                    long dt = Math.Max(1, tLast - tPrev);
                    if (dt >= 5 && dt <= 2000)
                    {
                        float invSec = 1000f / dt;
                        estVelX = (last.X - prev.X) * invSec;
                        estVelY = (last.Y - prev.Y) * invSec;
                        const float maxVel = 4000f;
                        estVelX = Math.Max(-maxVel, Math.Min(maxVel, estVelX));
                        estVelY = Math.Max(-maxVel, Math.Min(maxVel, estVelY));
                    }
                }

                LastNetworkUpdateMs = now;
                if (seq != 0) LastReceivedSeq = (uint)seq;
            }
            catch { }
        }

        /// <summary>
        /// Get interpolated (or limited extrapolated) position for render time = now - InterpolationDelayMs
        /// Returns true if interpolation/extrapolation applied and sets outX/outY; otherwise returns false.
        /// </summary>
        /// <summary>
        /// Get interpolated (or limited extrapolated) position for render time (ms).
        /// Uses server timestamp when available; otherwise falls back to local receive time.
        /// This function does not perform any smoothing by lerping to a target — it returns the
        /// time-correct position computed from snapshots. Caller should assign X/Y directly.
        /// </summary>
        public bool GetInterpolatedPosition(long renderTimeMs, out int outX, out int outY)
        {
            outX = X; outY = Y;
            try
            {
                if (snapshotBuffer.Count == 0) return false;

                // Choose timeline: prefer server timestamps if majority of snapshots have them
                bool useServerTs = false;
                int withServer = 0;
                foreach (var s in snapshotBuffer) if (s.ServerTsMs > 0) withServer++;
                useServerTs = withServer * 2 >= snapshotBuffer.Count; // majority

                // Build times for each snapshot
                int n = snapshotBuffer.Count;
                long[] times = new long[n];
                for (int i = 0; i < n; ++i)
                {
                    times[i] = useServerTs && snapshotBuffer[i].ServerTsMs > 0 ? snapshotBuffer[i].ServerTsMs : snapshotBuffer[i].RecvMs;
                }

                // If renderTime is before first snapshot, snap to first
                if (renderTimeMs <= times[0])
                {
                    outX = snapshotBuffer[0].X; outY = snapshotBuffer[0].Y; return true;
                }

                // If renderTime is after last snapshot, extrapolate limited
                if (renderTimeMs >= times[n - 1])
                {
                    var last = snapshotBuffer[n - 1];
                    // compute velocity from last two (if available)
                    if (n >= 2)
                    {
                        var prev = snapshotBuffer[n - 2];
                        long tLast = times[n - 1];
                        long tPrev = times[n - 2];
                        double dtMs = Math.Max(1, tLast - tPrev);
                        double vx = (last.X - prev.X) / (dtMs / 1000.0);
                        double vy = (last.Y - prev.Y) / (dtMs / 1000.0);

                        // limit extrapolation
                        double dtEx = (renderTimeMs - tLast) / 1000.0;
                        const double MAX_EXTRAPOLATE = 0.120; // 120ms
                        double useDt = Math.Min(dtEx, MAX_EXTRAPOLATE);

                        outX = (int)Math.Round(last.X + vx * useDt);
                        outY = (int)Math.Round(last.Y + vy * useDt);
                        return true;
                    }

                    // no velocity info -> hold last
                    outX = last.X; outY = last.Y; return true;
                }

                // Otherwise find surrounding snapshots
                int idx1 = 0;
                for (int i = 0; i < n - 1; ++i)
                {
                    if (times[i] <= renderTimeMs && renderTimeMs <= times[i + 1])
                    {
                        idx1 = i; break;
                    }
                }
                var s0 = snapshotBuffer[idx1];
                var s1 = snapshotBuffer[idx1 + 1];
                long t0 = times[idx1];
                long t1 = times[idx1 + 1];

                double alpha = (double)(renderTimeMs - t0) / Math.Max(1, (t1 - t0));
                alpha = Math.Max(0.0, Math.Min(1.0, alpha));

                outX = (int)Math.Round(s0.X + (s1.X - s0.X) * alpha);
                outY = (int)Math.Round(s0.Y + (s1.Y - s0.Y) * alpha);
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Interpolate towards target and optionally extrapolate if snapshots delayed.
        /// smoothing: 0..1 where higher smooths more (but adds perceived latency)
        /// </summary>
        /// <summary>
        /// Compatibility wrapper used by game loop: compute desired render position for (now - InterpolationDelayMs)
        /// and assign X/Y directly. This avoids chasing target by smoothing and instead uses time-based interpolation.
        /// smoothing parameter is ignored (kept for compatibility).
        /// </summary>
        public void InterpolateAndExtrapolate(float smoothing, int maxExtrapolateMs = 150)
        {
            try
            {
                long renderTime = Environment.TickCount - InterpolationDelayMs;
                if (GetInterpolatedPosition(renderTime, out int ix, out int iy))
                {
                    X = ix; Y = iy;
                }
            }
            catch { }
        }

        /// <summary>
        /// Smoothly interpolate current position toward target (called every frame by UI)
        /// </summary>
        public void InterpolateTowardsTarget(float smoothing)
        {
            try
            {
                if (smoothing <= 0) return;

                // Snap if target is very far to avoid long interpolation after teleport
                int dx = Math.Abs(TargetX - X);
                int dy = Math.Abs(TargetY - Y);
                const int snapThreshold = 120; // pixels
                if (dx > snapThreshold || dy > snapThreshold)
                {
                    X = TargetX;
                    Y = TargetY;
                    return;
                }

                // Only interpolate when target differs
                if (dx > 0 || dy > 0)
                {
                    float inv = 1f - smoothing;
                    X = (int)(X * inv + TargetX * smoothing);
                    Y = (int)(Y * inv + TargetY * smoothing);
                }
            }
            catch { }
        }

        // ✅ THÊM: Hệ thống hồi stamina/mana
        private long lastStaminaUsedTime = 0; // Thời điểm lần cuối dùng stamina (ms)
        private const long STAMINA_REGEN_DELAY_MS = 1000; // 1 giây trước khi bắt đầu hồi
        private const int STAMINA_REGEN_PER_SECOND = 20; // 20 stamina/s
        private const int MANA_REGEN_PER_SECOND = 2; // ✅ THAY ĐỔI: 1 mana/s -> 2 mana/s
        private const int MANA_REGEN_ON_HIT_MISS = 5; // Hồi 5 mana khi bị đánh trúng (không parry)
        private const int MANA_REGEN_ON_HIT_LAND = 5; // Hồi 5 mana khi đánh trúng
        private const int MANA_REGEN_ON_PARRY = 10; // Hồi 10 mana khi parry thành công
        private System.Windows.Forms.Timer manaRegenTimer; // ✅ FIX: Dùng fully qualified name
        private System.Windows.Forms.Timer staminaRegenTimer; // ✅ FIX: Dùng fully qualified name

        public PlayerState(string playerName, string characterType, int playerNumber)
        {
            PlayerName = playerName;
            CharacterType = characterType;
            PlayerNumber = playerNumber;
            InitializeRegenTimers();
        }

        /// <summary>
        /// Khởi tạo timers cho hệ thống hồi mana và stamina
        /// </summary>
        private void InitializeRegenTimers()
        {
            // Timer hồi mana mỗi 1 giây
            manaRegenTimer = new System.Windows.Forms.Timer();
            manaRegenTimer.Interval = 1000; // 1 giây
            manaRegenTimer.Tick += (s, e) =>
            {
                RegenerateMana(MANA_REGEN_PER_SECOND);
            };
            manaRegenTimer.Start();

            // Timer hồi stamina - khởi động ngay từ đầu (không dùng stamina nên sẽ hồi)
            staminaRegenTimer = new System.Windows.Forms.Timer();
            staminaRegenTimer.Interval = 1000; // 1 giây (check mỗi 1 giây hồi 20 stamina)
            staminaRegenTimer.Tick += (s, e) =>
            {
                // Only regenerate if no stamina used for STAMINA_REGEN_DELAY_MS
                long now = Environment.TickCount;
                if (now - lastStaminaUsedTime >= STAMINA_REGEN_DELAY_MS)
                {
                    RegenerateStamina(STAMINA_REGEN_PER_SECOND);
                }
            };
            // Start timer but mark last use in the past so regen begins immediately
            staminaRegenTimer.Start();
            lastStaminaUsedTime = Environment.TickCount - STAMINA_REGEN_DELAY_MS - 50; // allow immediate regen
        }

        /// <summary>
        /// Reset về trạng thái idle
        /// </summary>
        public void ResetToIdle()
        {
            if (!IsStunned && !IsParrying && !IsSkillActive && !IsJumping)
            {
                CurrentAnimation = (LeftKeyPressed || RightKeyPressed) ? "walk" : "stand";
            }
        }

        /// <summary>
        /// ✅ THÊM: Reset health to max before round starts (防止初始化bug)
        /// </summary>
        public void ResetHealthToMax()
        {
            // Max health for each character type
            int maxHP = CharacterType?.ToLower() switch
            {
                "goatman" => 130,
                "bringerofdeath" => 90,
                "warrior" => 80,
                "girlknight" => 100,
                "knightgirl" => 100,
                _ => 100
            };
            
            Health = maxHP;
            Console.WriteLine($"[PlayerState] {PlayerName} health reset to {Health}");
        }

        /// <summary>
        /// Apply damage to player
        /// </summary>
        public void TakeDamage(int damage)
        {
            Health = Math.Max(0, Health - damage);
            try { LastDamageTimeMs = Environment.TickCount; LastDamageAmount = damage; } catch { }
        }

        /// <summary>
        /// Consume stamina
        /// </summary>
        public bool ConsumeStamina(int amount)
        {
            if (Stamina < amount) return false;
            Stamina -= amount;
            
            // ✅ THÊM: Reset timer hồi stamina khi dùng
            lastStaminaUsedTime = Environment.TickCount;
            
            return true;
        }

        /// <summary>
        /// Consume mana
        /// </summary>
        public bool ConsumeMana(int amount)
        {
            if (Mana < amount) return false;
            Mana -= amount;
            return true;
        }

        /// <summary>
        /// ✅ THÊM: Hồi mana
        /// </summary>
        public void RegenerateMana(int amount)
        {
            if (Mana < 100)
            {
                Mana = Math.Min(100, Mana + amount);
            }
        }

        /// <summary>
        /// ✅ THÊM: Hồi stamina
        /// </summary>
        public void RegenerateStamina(int amount)
        {
            if (Stamina < 100)
            {
                Stamina = Math.Min(100, Stamina + amount);
            }
        }

        /// <summary>
        /// ✅ THÊM: Hồi mana khi bị đánh (không parry kịp)
        /// </summary>
        public void RegenerateManaOnHitMiss()
        {
            RegenerateMana(MANA_REGEN_ON_HIT_MISS);
        }

        /// <summary>
        /// ✅ THÊM: Hồi mana khi đánh trúng
        /// </summary>
        public void RegenerateManaOnHitLand()
        {
            RegenerateMana(MANA_REGEN_ON_HIT_LAND);
        }

        /// <summary>
        /// ✅ THÊM: Hồi mana khi parry thành công
        /// </summary>
        public void RegenerateManaOnParrySuccess()
        {
            RegenerateMana(MANA_REGEN_ON_PARRY);
        }

        /// <summary>
        /// ✅ THÊM: Kiểm tra và khởi động hồi stamina nếu đã đủ 1 giây không dùng
        /// </summary>
        public void UpdateStaminaRegenDelay()
        {
            long currentTime = Environment.TickCount;
            long timeSinceLastUse = currentTime - lastStaminaUsedTime;

            if (timeSinceLastUse >= STAMINA_REGEN_DELAY_MS && !staminaRegenTimer.Enabled)
            {
                // Đã đủ 1 giây không dùng, bắt đầu hồi
                staminaRegenTimer.Start();
            }
            else if (timeSinceLastUse < STAMINA_REGEN_DELAY_MS && staminaRegenTimer.Enabled)
            {
                // Nếu lại dùng stamina trong khoảng, dừng timer hồi
                staminaRegenTimer.Stop();
            }
        }

        /// <summary>
        /// Regenerate resources (LEGACY - kept for compatibility)
        /// </summary>
        public void RegenerateResources()
        {
            // Kiểm tra xem có nên bắt đầu hồi stamina không
            UpdateStaminaRegenDelay();
        }

        /// <summary>
        /// Check if player is dead
        /// </summary>
        public bool IsDead => Health <= 0;

        /// <summary>
        /// Check if player can move
        /// </summary>
        public bool CanMove => !IsStunned && !IsCharging && !IsDashing && !IsAttacking && !IsParrying; // ✅ THÊM: !IsParrying

        /// <summary>
        /// Check if player can attack
        /// </summary>
        public bool CanAttack => !IsStunned && !IsCharging && !IsDashing;

        /// <summary>
        /// Check if player can dash
        /// </summary>
        public bool CanDash => !IsStunned && !IsCharging && !IsDashing && !IsAttacking;

        /// <summary>
        /// Check if player can parry
        /// </summary>
        public bool CanParry => !IsStunned && !IsCharging && !IsDashing && !IsParryOnCooldown;

        /// <summary>
        /// ✅ THÊM: Cleanup timers khi form đóng
        /// </summary>
        public void Cleanup()
        {
            try
            {
                manaRegenTimer?.Stop();
                manaRegenTimer?.Dispose();
                staminaRegenTimer?.Stop();
                staminaRegenTimer?.Dispose();
            }
            catch { }
        }
    }
}
