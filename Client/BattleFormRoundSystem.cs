// ===== BEST-OF-THREE ROUND SYSTEM EXTENSION =====
// This file contains the round system implementation for BattleForm
// File: Client\BattleFormRoundSystem.cs

using System;
using System.Windows.Forms;
using System.Drawing;

namespace DoAn_NT106
{
    public partial class BattleForm
    {
        // === Round System Fields ===
        private int _roundNumber = 1;
        private int _player1Wins = 0;
        private int _player2Wins = 0;
        private System.Windows.Forms.Timer _roundTimer;
        private int _roundTimeRemainingMs = 3 * 60 * 1000; // 3 minutes per round
        private Label _lblRoundCenter; // centered between HP/Stamina/Mana bars
        private bool _roundInProgress = false;

        // Round countdown timer for showing "ROUND X" at start
        private System.Windows.Forms.Timer _roundStartTimer;
        private int _roundStartCountdownMs = 0;
        private Label _lblRoundStart; // Large "ROUND X" label

        // ✅ THÊM: Lưu mana giữa các hiệp
        private int _player1ManaCarryover = 0;
        private int _player2ManaCarryover = 0;

        /// <summary>Gets formatted round info text with round number, timer, and scores</summary>
        private string GetRoundCenterText()
        {
            var remaining = TimeSpan.FromMilliseconds(Math.Max(0, _roundTimeRemainingMs));
            
            // ✅ Đảm bảo có giá trị cho player names
            string p1Name = !string.IsNullOrEmpty(username) ? username.ToUpper() : "PLAYER 1";
            string p2Name = !string.IsNullOrEmpty(opponent) ? opponent.ToUpper() : "PLAYER 2";

            // Header line
            string header = $"[ ROUND {_roundNumber} ]";
            // Body line: PLAYER 1 (xW)  ⏱ mm:ss  (yW) PLAYER 2
            string body = $"{p1Name} ({_player1Wins}W)  ⏱️ {remaining.Minutes:00}:{remaining.Seconds:00}  ({_player2Wins}W) {p2Name}";

            // Compose with spacing lines
            return header + "\n\n" + body + "\n\n";
        }

        /// <summary>Updates the center label text with current round info</summary>
        private void UpdateRoundCenterText()
        {
            if (_lblRoundCenter != null)
            {
                _lblRoundCenter.Text = GetRoundCenterText();
                ResizeRoundCenterLabel();
                _lblRoundCenter.Refresh();
                PositionRoundCenterLabel();
            }
        }

        /// <summary>Resizes the center label to fit the formatted text snugly</summary>
        private void ResizeRoundCenterLabel()
        {
            if (_lblRoundCenter == null) return;
            var text = _lblRoundCenter.Text;
            var font = _lblRoundCenter.Font;
            // Measure text size accurately for Label
            var measured = TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.NoClipping);

            // Apply small padding
            int padX = 16;
            int padY = 10;
            int newW = measured.Width + padX;
            int newH = measured.Height + padY;
            // Ensure a minimum width so the last token doesn't wrap/cut
            if (newW < 580) newW = 580;
            _lblRoundCenter.Size = new Size(newW, newH);
        }

        /// <summary>Positions the round center label at top center, ngang với các thanh HP/Stamina/Mana</summary>
        private void PositionRoundCenterLabel()
        {
            if (_lblRoundCenter == null) return;
            int screenWidth = this.ClientSize.Width;
            
            // Positioned at top, centered horizontally (ngang với các thanh HP/Stamina/Mana)
            int centerX = (screenWidth / 2) - (_lblRoundCenter.Width / 2);
            int topY = 10; // Ngang với các thanh bars
            
            _lblRoundCenter.Location = new Point(centerX, topY);
        }

        /// <summary>Initializes round UI and timer (call from SetupGame)</summary>
        private void InitializeRoundSystem()
        {
            // Create center label if not already created
            if (_lblRoundCenter == null)
            {
                string initialText = GetRoundCenterText();
                _lblRoundCenter = new Label
                {
                    Text = GetRoundCenterText(),
                    Size = new Size(1, 1),
                    ForeColor = Color.Gold,
                    Font = new Font("Courier New", 15, FontStyle.Bold),
                    BackColor = Color.FromArgb(200, 0, 0, 0),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(6),
                    AutoSize = false
                };
                this.Controls.Add(_lblRoundCenter);
                ResizeRoundCenterLabel();
                _lblRoundCenter.BringToFront();
            }
            else
            {
                UpdateRoundCenterText(); 
            }
            PositionRoundCenterLabel();

            // Create large "ROUND X" label for round start countdown
            if (_lblRoundStart == null)
            {
                _lblRoundStart = new Label
                {
                    Text = $"ROUND {_roundNumber}",
                    Size = new Size(600, 200),
                    ForeColor = Color.Red,
                    Font = new Font("Arial", 80, FontStyle.Bold),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Visible = false
                };
                this.Controls.Add(_lblRoundStart);
                _lblRoundStart.BringToFront();
            }

            // Create round timer if not already created
            if (_roundTimer == null)
            {
                _roundTimer = new System.Windows.Forms.Timer { Interval = 1000 };
                _roundTimer.Tick += RoundTimer_Tick;
            }

            // Create round start countdown timer
            if (_roundStartTimer == null)
            {
                _roundStartTimer = new System.Windows.Forms.Timer { Interval = 100 };
                _roundStartTimer.Tick += RoundStartTimer_Tick;
            }

            // Delay hiển thị ROUND 1 cho đến khi form đã load xong
            var delayTimer = new System.Windows.Forms.Timer { Interval = 200 };
            delayTimer.Tick += (s, e) =>
            {
                delayTimer.Stop();
                delayTimer.Dispose();
                
                // Kiểm tra form đã sẵn sàng chưa
                if (this.ClientSize.Width > 100 && this.ClientSize.Height > 100)
                {
                    // ✅ Cập nhật lại text và vị trí SAU KHI form đã load
                    Console.WriteLine($"[RoundSystem] Updating center text with: {username} vs {opponent}");
                    UpdateRoundCenterText();
                    PositionRoundCenterLabel();
                    _lblRoundCenter?.BringToFront();
                    
                    // Hiển thị ROUND animation
                    DisplayRoundStartAnimation();
                }
            };
            delayTimer.Start();
        }
        
        /// <summary>Display the "ROUND X" animation at the start of a round</summary>
        private void DisplayRoundStartAnimation()
        {
            if (_lblRoundStart == null) return;
            
            // Đảm bảo form đã có kích thước đúng
            int screenWidth = this.ClientSize.Width;
            int screenHeight = this.ClientSize.Height;
            
            // Fallback nếu form chưa sẵn sàng
            if (screenWidth < 100 || screenHeight < 100)
            {
                screenWidth = 1920;
                screenHeight = 1080;
            }
            
            _roundStartCountdownMs = 1000; // 1 seconds
            _lblRoundStart.Text = $"ROUND {_roundNumber}";
            // ✅ Play round announcement sound (Stop only sound effects, NOT music)
            try
            {
                // ✅ IMPORTANT: Stop only sound effects, NOT background music!
                // This allows round announcement to play over the current music
                Console.WriteLine($"[RoundSystem] About to play Round {_roundNumber} sound");
                
                // Play generic sound for round announcement or use ButtonClick as fallback
                try
                {
                    if (_roundNumber == 1) DoAn_NT106.SoundManager.PlaySound(DoAn_NT106.Client.SoundEffect.Round1);
                    else if (_roundNumber == 2) DoAn_NT106.SoundManager.PlaySound(DoAn_NT106.Client.SoundEffect.Round2);
                    else if (_roundNumber == 3) DoAn_NT106.SoundManager.PlaySound(DoAn_NT106.Client.SoundEffect.Round3);
                    
                    Console.WriteLine($"[RoundSystem] ✅ Round {_roundNumber} sound initiated successfully");
                }
                catch (Exception roundSoundEx)
                {
                    // ✅ Fallback: Use ButtonClick sound if Round-specific sounds don't exist
                    Console.WriteLine($"[RoundSystem] Round sound failed, trying fallback ButtonClick: {roundSoundEx.Message}");
                    try { DoAn_NT106.SoundManager.PlaySound(DoAn_NT106.Client.SoundEffect.ButtonClick); } catch { }
                }
            }
            catch (Exception ex) { Console.WriteLine($"[RoundSystem] Error playing round sound: {ex.Message}"); }
            
            // ✅ Đo kích thước thực tế của label
            using (var g = _lblRoundStart.CreateGraphics())
            {
                var size = g.MeasureString(_lblRoundStart.Text, _lblRoundStart.Font);
                // Căn giữa dựa trên kích thước thực tế
                _lblRoundStart.Location = new Point(
                    (screenWidth - (int)size.Width) / 2 - 50,
                    (screenHeight - (int)size.Height) / 2 - 50
                );
            }
            
            _lblRoundStart.Visible = true;
            _lblRoundStart.BringToFront();
            gameTimer?.Stop(); // Lock input during countdown
            
            if (_roundStartTimer != null && !_roundStartTimer.Enabled)
                _roundStartTimer.Start();
            
            if (_roundTimer != null && !_roundTimer.Enabled)
                _roundTimer.Start();
            
            _roundInProgress = false; // Lock gameplay during countdown
            
            // ✅ Cập nhật lại center text với tên player
            UpdateRoundCenterText();
            
            Console.WriteLine($"[RoundSystem] Displaying ROUND {_roundNumber} at center");
        }

        /// <summary>Countdown timer tick - updates every 1 second</summary>
        private void RoundTimer_Tick(object sender, EventArgs e)
        {
            if (!_roundInProgress) return;

            _roundTimeRemainingMs = Math.Max(0, _roundTimeRemainingMs - 1000);
            UpdateRoundCenterText();

            if (_roundTimeRemainingMs <= 0)
            {
                HandleRoundTimeout();
            }
        }

        /// <summary>Round start countdown timer - shows "ROUND X" for 2 seconds at start</summary>
        private void RoundStartTimer_Tick(object sender, EventArgs e)
        {
            _roundStartCountdownMs -= 100;
            
            if (_roundStartCountdownMs <= 0)
            {
                _roundStartTimer?.Stop();
                _lblRoundStart.Visible = false;
                
                // Enable gameplay
                _roundInProgress = true;
                gameTimer?.Start();
            }
        }

        /// <summary>Handles round timeout (time expired - lower HP loses)</summary>
        private void HandleRoundTimeout()
        {
            _roundInProgress = false;
            _roundTimer?.Stop();

            // ✅ THÊM: Lưu mana hiện tại trước khi qua hiệp
            _player1ManaCarryover = player1State.Mana;
            _player2ManaCarryover = player2State.Mana;

            // Determine winner by HP
            if (player1State.Health < player2State.Health)
                _player2Wins++;
            else if (player2State.Health < player1State.Health)
                _player1Wins++;
            // Equal HP = tie, no win awarded

            // Check if match ends (first to 2 wins)
            if (_player1Wins >= 2 || _player2Wins >= 2)
            {
                EndMatch(_player1Wins >= 2 ? username : opponent);
                return;
            }

            // Start next round
            _roundNumber++;
            StartNextRound();
        }

        /// <summary>Handles round end by death (someone's HP reached 0)</summary>
        private void HandleRoundEndByDeath()
        {
            _roundInProgress = false;
            _roundTimer?.Stop();

            // ✅ THÊM: Lưu mana hiện tại trước khi qua hiệp
            _player1ManaCarryover = player1State.Mana;
            _player2ManaCarryover = player2State.Mana;

            // Award win to survivor
            if (player1State.IsDead && !player2State.IsDead)
                _player2Wins++;
            else if (player2State.IsDead && !player1State.IsDead)
                _player1Wins++;
            // Both dead = tie, no win awarded

            // Check if match ends
            if (_player1Wins >= 2 || _player2Wins >= 2)
            {
                EndMatch(_player1Wins >= 2 ? username : opponent);
                return;
            }

            // Start next round
            _roundNumber++;
            StartNextRound();
        }

        /// <summary>Resets game state and starts next round</summary>
        private void StartNextRound()
        {
            // Reset round timer
            _roundTimeRemainingMs = 3 * 60 * 1000;
            UpdateRoundCenterText();

            // ✅ SỬA: Reset HP theo character type (không phải mặc định 100)
            int maxHP1 = GetMaxHealthForCharacter(player1State.CharacterType);
            int maxHP2 = GetMaxHealthForCharacter(player2State.CharacterType);

            player1State.Health = maxHP1;
            player1State.Stamina = 100;
            // ✅ SỬA: Sử dụng mana từ hiệp trước (carryover)
            player1State.Mana = _player1ManaCarryover;

            player2State.Health = maxHP2;
            player2State.Stamina = 100;
            // ✅ SỬA: Sử dụng mana từ hiệp trước (carryover)
            player2State.Mana = _player2ManaCarryover;

            // ✅ THÊM: Update HealthBar Maximum values
            resourceSystem.HealthBar1.Maximum = maxHP1;
            resourceSystem.HealthBar2.Maximum = maxHP2;

            resourceSystem?.UpdateBars();

            // Reset all combat statuses
            player1State.IsStunned = false;
            player2State.IsStunned = false;
            player1State.IsAttacking = false;
            player2State.IsAttacking = false;
            player1State.IsParrying = false;
            player2State.IsParrying = false;
            player1State.IsSkillActive = false;
            player2State.IsSkillActive = false;
            player1State.IsCharging = false;
            player2State.IsCharging = false;
            player1State.IsDashing = false;
            player2State.IsDashing = false;

            player1State.ResetToIdle();
            player2State.ResetToIdle();

            // Reset positions - ✅ SỬA: X = 150 và 900, force reset Y position
            player1State.X = 150;
            player1State.Y = groundLevel - PLAYER_HEIGHT;
            
            player2State.X = 900;
            player2State.Y = groundLevel - PLAYER_HEIGHT;
            
            physicsSystem?.ResetToGround(player1State);
            physicsSystem?.ResetToGround(player2State);

            // Cleanup effects
            try { effectManager?.Cleanup(); } catch { }
            try { projectileManager?.Cleanup(); } catch { }

            // Start round countdown animation
            DisplayRoundStartAnimation();
            this.Invalidate();
        }

        // ✅ THÊM: Helper function to get max health for character
        private int GetMaxHealthForCharacter(string characterType)
        {
            return characterType?.ToLower() switch
            {
                "goatman" => 130,
                "bringerofdeath" => 90,
                "warrior" => 80,
                "girlknight" => 100,
                "knightgirl" => 100,
                _ => 100
            };
        }

        /// <summary>Ends the match and shows winner dialog</summary>
        private void EndMatch(string winner)
        {
            _roundInProgress = false;
            try { _roundTimer?.Stop(); } catch { }
            try { gameTimer?.Stop(); } catch { }
            try { walkAnimationTimer?.Stop(); } catch { }

            string result = $"🎉 {winner} WINS THE MATCH!\n\n" +
                            $"{username}: {_player1Wins} wins\n" +
                            $"{opponent}: {_player2Wins} wins";

            MessageBox.Show(
                result,
                "MATCH FINISHED",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation
            );

            BtnBack_Click(null, EventArgs.Empty);
            // ✅ Resume theme music when returning to MainForm
            try { DoAn_NT106.SoundManager.PlayMusic(DoAn_NT106.Client.BackgroundMusic.ThemeMusic, loop: true); } catch { }
        }
    }
}

