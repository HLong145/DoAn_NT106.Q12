// ===== BEST-OF-THREE ROUND SYSTEM EXTENSION =====
// This file contains the round system implementation for BattleForm
// File: Client\BattleFormRoundSystem.cs

using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Reflection;
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
        // ✅ THÊM: Cached audio bytes for round announcements to ensure repeatable playback
        private byte[] _round1AudioBytes;
        private byte[] _round2AudioBytes;
        private byte[] _round3AudioBytes;

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

        /// <summary>
        /// Initializes round UI and timer (call from SetupGame)
        /// </summary>
        private void InitializeRoundSystem()
        {
            // ✅ RESET ROUND SYSTEM MỖI LẦN INITIALIZE
            _roundNumber = 1;
            _player1Wins = 0;
            _player2Wins = 0;
            _roundTimeRemainingMs = 3 * 60 * 1000; // 3 minutes per round
            _roundInProgress = false;
            _player1ManaCarryover = 0;
            _player2ManaCarryover = 0;

            // Create center label if not already created
            if (_lblRoundCenter == null)
            {
                string initialText = GetRoundCenterText();
                _lblRoundCenter = new Label
                {
                    Text = initialText,
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
                    Console.WriteLine($"[RoundSystem] Initializing with: {username} vs {opponent}");
                    UpdateRoundCenterText();
                    PositionRoundCenterLabel();
                    _lblRoundCenter?.BringToFront();
                    
                    // Hiển thị ROUND animation
                    // Preload round audio to ensure it can be played immediately and repeatedly
                    try { PreloadRoundAudioResources(); } catch { }
                    DisplayRoundStartAnimation();
                }
            };
            delayTimer.Start();
        }

        // Load embedded round_x resources into memory once so they can be played multiple times
        private void PreloadRoundAudioResources()
        {
            try
            {
                if (_round1AudioBytes == null) _round1AudioBytes = GetAudioBytesFromResourceKey("round_1");
                if (_round2AudioBytes == null) _round2AudioBytes = GetAudioBytesFromResourceKey("round_2");
                if (_round3AudioBytes == null) _round3AudioBytes = GetAudioBytesFromResourceKey("round_3");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoundSystem] PreloadRoundAudioResources error: {ex.Message}");
            }
        }

        private byte[] GetAudioBytesFromResourceKey(string key)
        {
            try
            {
                // Try typed property first
                try
                {
                    var prop = typeof(Properties.Resources).GetProperty(key, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.IgnoreCase);
                    if (prop != null)
                    {
                        var val = prop.GetValue(null);
                        if (val is byte[] bb) return bb;
                        if (val is System.IO.UnmanagedMemoryStream ums)
                        {
                            using var ms = new System.IO.MemoryStream();
                            ums.CopyTo(ms);
                            return ms.ToArray();
                        }
                        if (val is System.IO.Stream s)
                        {
                            using var ms = new System.IO.MemoryStream();
                            s.CopyTo(ms);
                            return ms.ToArray();
                        }
                    }
                }
                catch { }

                // Try resource manager stream
                try
                {
                    using var rs = Properties.Resources.ResourceManager.GetStream(key, System.Globalization.CultureInfo.CurrentUICulture);
                    if (rs != null)
                    {
                        using var ms = new System.IO.MemoryStream();
                        rs.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
                catch { }

                // Try GetObject fallback
                try
                {
                    var obj = Properties.Resources.ResourceManager.GetObject(key);
                    if (obj is byte[] b) return b;
                    if (obj is System.IO.UnmanagedMemoryStream ums2)
                    {
                        using var ms2 = new System.IO.MemoryStream();
                        ums2.CopyTo(ms2);
                        return ms2.ToArray();
                    }
                    if (obj is System.IO.Stream s2)
                    {
                        using var ms2 = new System.IO.MemoryStream();
                        s2.CopyTo(ms2);
                        return ms2.ToArray();
                    }
                }
                catch { }

                return null;
            }
            catch
            {
                return null;
            }
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
                    // Some builds may not include Round1/2/3 enums/resources; guard with TryPlay
                    TryPlayRoundSound(_roundNumber);
                }
                catch (Exception roundSoundEx)
                {
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

        // Safely attempt to play round announcement sound if enum/value exists
        private void TryPlayRoundSound(int round)
        {
            try
            {
                string enumName = round == 1 ? "Round1" : round == 2 ? "Round2" : "Round3";

                // Ensure SoundManager initialized
                try { DoAn_NT106.SoundManager.Initialize(); } catch { }

                // Try to play embedded resource named round_1 / round_2 / round_3 first
                try
                {
                    string resKey = $"round_{round}"; // matches your resources
                    var obj = Properties.Resources.ResourceManager.GetObject(resKey);
                    if (obj != null)
                    {
                        Console.WriteLine($"[RoundSystem] Found resource '{resKey}' - playing via NAudio");
                        try
                        {
                            // Play mp3 resource using NAudio to support MP3 (SoundPlayer only supports WAV)
                            byte[] audioBytes = null;
                            if (obj is byte[] bb) audioBytes = bb;
                            else if (obj is System.IO.UnmanagedMemoryStream ums)
                            {
                                using var tmp = new System.IO.MemoryStream();
                                ums.CopyTo(tmp);
                                audioBytes = tmp.ToArray();
                            }
                            else if (obj is System.IO.Stream s0)
                            {
                                using var tmp = new System.IO.MemoryStream();
                                s0.Position = 0;
                                s0.CopyTo(tmp);
                                audioBytes = tmp.ToArray();
                            }

                            if (audioBytes != null && audioBytes.Length > 0)
                            {
                                var ms = new System.IO.MemoryStream(audioBytes);
                                var reader = new NAudio.Wave.Mp3FileReader(ms);
                                var wo = new NAudio.Wave.WaveOutEvent();
                                wo.Init(reader);
                                wo.PlaybackStopped += (s, e) =>
                                {
                                    try { wo.Dispose(); } catch { }
                                    try { reader.Dispose(); } catch { }
                                    try { ms.Dispose(); } catch { }
                                };
                                wo.Play();
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[RoundSystem] NAudio play failed: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[RoundSystem] Resource '{resKey}' not found in Resources");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RoundSystem] Resource play attempt failed: {ex.Message}");
                }

                // Try play from preloaded bytes first (ensures repeatable playback)
                try
                {
                    byte[] bytesToPlay = round == 1 ? _round1AudioBytes : round == 2 ? _round2AudioBytes : _round3AudioBytes;
                    if (bytesToPlay != null && bytesToPlay.Length > 0)
                    {
                        try
                        {
                            var ms = new System.IO.MemoryStream(bytesToPlay);
                            var reader = new NAudio.Wave.Mp3FileReader(ms);
                            var wo = new NAudio.Wave.WaveOutEvent();
                            wo.Init(reader);
                            wo.PlaybackStopped += (s, e) => { try { wo.Dispose(); } catch { } try { reader.Dispose(); } catch { } try { ms.Dispose(); } catch { } };
                            wo.Play();
                            Console.WriteLine($"[RoundSystem] Played round_{round} from preloaded resource (NAudio)");
                            return;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[RoundSystem] Failed to play preloaded round_{round}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine($"[RoundSystem] Preloaded play error: {ex.Message}"); }

                // Fallback: try SoundManager mapping
                try
                {
                    var seType = typeof(DoAn_NT106.Client.SoundEffect);
                    if (Enum.IsDefined(seType, enumName))
                    {
                        var val = (DoAn_NT106.Client.SoundEffect)Enum.Parse(seType, enumName);
                        Console.WriteLine($"[RoundSystem] Playing {enumName} via SoundManager");
                        DoAn_NT106.SoundManager.PlaySound(val);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RoundSystem] SoundManager play error: {ex.Message}");
                }

                // Final fallback system sound
                try { System.Media.SystemSounds.Exclamation.Play(); } catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoundSystem] TryPlayRoundSound error: {ex.Message}");
            }
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

            // If we've already played 3 rounds and nobody reached 2 wins -> draw
            if (_roundNumber >= 3)
            {
                EndMatchDraw();
                return;
            }

            // Start next round (only if less than 3 rounds played)
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

            // If we've already played 3 rounds and nobody reached 2 wins -> draw
            if (_roundNumber >= 3)
            {
                EndMatchDraw();
                return;
            }

            // Start next round (only if less than 3 rounds played)
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

            // ✅ CLOSE battle form and return to lobby
            this.Close();
            
            // ✅ Resume theme music when returning to MainForm
            try { DoAn_NT106.SoundManager.PlayMusic(DoAn_NT106.Client.BackgroundMusic.ThemeMusic, loop: true); } catch { }
        }

        /// <summary>Ends the match as a draw and shows dialog</summary>
        private void EndMatchDraw()
        {
            _roundInProgress = false;
            try { _roundTimer?.Stop(); } catch { }
            try { gameTimer?.Stop(); } catch { }
            try { walkAnimationTimer?.Stop(); } catch { }

            string result = $"🤝 MATCH DRAW!\n\n" +
                            $"{username}: {_player1Wins} wins\n" +
                            $"{opponent}: {_player2Wins} wins";

            MessageBox.Show(
                result,
                "MATCH DRAW",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            // Close battle form and return to lobby
            this.Close();

            // Resume theme music when returning to MainForm
            try { DoAn_NT106.SoundManager.PlayMusic(DoAn_NT106.Client.BackgroundMusic.ThemeMusic, loop: true); } catch { }
        }
    }
}

