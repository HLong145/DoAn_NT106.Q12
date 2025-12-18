using DoAn_NT106.Services;
using DoAn_NT106.Client.Class;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoAn_NT106.Client
{
    public partial class TinhXP : Form
    {
        private readonly MatchResult _result;
        private int _calculatedXp;
        private int _levelBefore;
        private int _levelAfter;
        private int _xpBefore;
        private int _xpAfter;
        private int _xpNeededForNextLevel;

        public TinhXP(MatchResult result)
        {
            InitializeComponent();
            this.AutoScroll = true;
            this.AutoScrollMinSize = Size.Empty;
            _result = result ?? throw new ArgumentNullException(nameof(result));

            // Start async load to avoid blocking UI
            _ = LoadAndDisplayXpAsync();
        }

        public TinhXP() : this(new MatchResult())
        {
        }

        private async Task LoadAndDisplayXpAsync()
        {
            // Simple XP rule: win = 100 XP, lose = 40 XP
            _calculatedXp = _result.PlayerIsWinner ? 100 : 40;

            const int xpPerLevel = 1000;

            // Get current XP from local database (run on thread pool)
            _xpBefore = 0;
            try
            {
                if (!string.IsNullOrEmpty(_result.PlayerUsername))
                {
                    _xpBefore = await Task.Run(() =>
                    {
                        try
                        {
                            //var db = new DatabaseService();
                            //return db.GetPlayerXp(_result.PlayerUsername);

                            return 0;
                        }
                        catch
                        {
                            return 0;
                        }
                    }).ConfigureAwait(false);
                }
            }
            catch
            {
                _xpBefore = 0;
            }

            _xpAfter = _xpBefore + _calculatedXp;

            _levelBefore = Math.Max(1, (_xpBefore / xpPerLevel) + 1);
            _levelAfter = Math.Max(1, (_xpAfter / xpPerLevel) + 1);

            _xpNeededForNextLevel = _levelAfter * xpPerLevel;

            // Play level up sound if player leveled up
            if (_levelAfter > _levelBefore)
            {
                try
                {
                    PlayLevelUpSound();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TinhXP] Error playing level up sound: {ex.Message}");
                }
            }

            // Persist new XP and level into database (best-effort)
            try
            {
                if (!string.IsNullOrEmpty(_result.PlayerUsername))
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            //var db = new DatabaseService();
                            //db.UpdatePlayerXp(_result.PlayerUsername, _xpAfter, _xpNeededForNextLevel);
                            //if (_levelAfter > _levelBefore)
                            //{
                            //    db.UpdatePlayerLevel(_result.PlayerUsername, _levelAfter);
                            //}
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[TinhXP] DB update failed: {ex.Message}");
                        }
                    }).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TinhXP] Update DB error: {ex.Message}");
            }

            // Update UI on UI thread
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateUi));
            }
            else
            {
                UpdateUi();
            }
        }

        private void UpdateUi()
        {
            if (lbl_XPEarnedValue != null)
            {
                lbl_XPEarnedValue.Text = "+" + _calculatedXp + " XP";
            }

            if (lbl_PlayerValue != null)
            {
                lbl_PlayerValue.Text = _result.PlayerUsername ?? "PLAYER";
            }

            if (lbl_ResultValue != null)
            {
                if (_result.PlayerIsWinner)
                {
                    lbl_ResultValue.Text = "WIN";
                    lbl_ResultValue.ForeColor = Color.LimeGreen;
                }
                else
                {
                    lbl_ResultValue.Text = "LOSE";
                    lbl_ResultValue.ForeColor = Color.Red;
                }
            }

            if (lbl_TimeValue != null)
            {
                TimeSpan time = _result.MatchTime;
                if (time <= TimeSpan.Zero)
                {
                    lbl_TimeValue.Text = "00:00";
                }
                else
                {
                    lbl_TimeValue.Text = string.Format("{0:00}:{1:00}", (int)time.TotalMinutes, time.Seconds);
                }
            }

            if (lbl_XPProgress != null)
            {
                if (_levelAfter > _levelBefore)
                {
                    lbl_XPProgress.Text = $"Level {_levelBefore} -> Level {_levelAfter}";
                }
                else
                {
                    lbl_XPProgress.Text = $"Level {_levelAfter}";
                }
            }

            if (lbl_XPProgressValue != null)
            {
                // Show total XP accumulated so far / xpPerLevel (simple compact format)
                const int xpPerLevel = 1000;
                lbl_XPProgressValue.Text = $"{_xpAfter} / {xpPerLevel}";
            }

            // Update progress bar fill based on xp from DB + gained XP
            const int xpPerLevelConst = 1000;
            int xpInCurr = _xpAfter % xpPerLevelConst;
            float percent = xpPerLevelConst > 0 ? (xpInCurr * 100f / xpPerLevelConst) : 0f;

            if (pnl_XPBarFill != null && pnl_XPBarContainer != null)
            {
                int maxWidth = pnl_XPBarContainer.Width;
                int fillWidth = (int)(maxWidth * (percent / 100f));
                if (fillWidth < 0) fillWidth = 0;
                if (fillWidth > maxWidth) fillWidth = maxWidth;
                pnl_XPBarFill.Width = fillWidth;
            }
        }

        private void btn_Continue_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>Play level up sound from resources</summary>
        private void PlayLevelUpSound()
        {
            try
            {
                // Try to play level_up resource (check if it exists in Resources)
                try
                {
                    // First, try using SoundManager if it has a LevelUp enum
                    var seType = typeof(DoAn_NT106.Client.Class.SoundEffect);
                    if (Enum.IsDefined(seType, "LevelUp"))
                    {
                        var levelUpEffect = (DoAn_NT106.Client.Class.SoundEffect)Enum.Parse(seType, "LevelUp");
                        DoAn_NT106.Client.Class.SoundManager.PlaySound(levelUpEffect);
                        Console.WriteLine("[TinhXP] ✅ Level up sound played via SoundManager");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TinhXP] SoundManager LevelUp not found: {ex.Message}");
                }

                // Fallback: Try to get level_up from resources directly
                try
                {
                    var obj = Properties.Resources.ResourceManager.GetObject("level_up");
                    if (obj != null)
                    {
                        byte[] audioBytes = null;
                        
                        if (obj is byte[] bb)
                        {
                            audioBytes = bb;
                        }
                        else if (obj is System.IO.UnmanagedMemoryStream ums)
                        {
                            using var tmp = new System.IO.MemoryStream();
                            ums.CopyTo(tmp);
                            audioBytes = tmp.ToArray();
                        }
                        else if (obj is System.IO.Stream s)
                        {
                            using var tmp = new System.IO.MemoryStream();
                            s.Position = 0;
                            s.CopyTo(tmp);
                            audioBytes = tmp.ToArray();
                        }

                        if (audioBytes != null && audioBytes.Length > 0)
                        {
                            // Try NAudio for MP3 support
                            try
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
                                Console.WriteLine("[TinhXP] ✅ Level up sound played via NAudio");
                                return;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[TinhXP] NAudio play failed: {ex.Message}");
                            }

                            // Fallback: Try SoundPlayer for WAV
                            try
                            {
                                using var player = new System.Media.SoundPlayer(new System.IO.MemoryStream(audioBytes));
                                player.PlaySync();
                                Console.WriteLine("[TinhXP] ✅ Level up sound played via SoundPlayer");
                                return;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[TinhXP] SoundPlayer play failed: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("[TinhXP] ⚠️ 'level_up' resource not found in Resources");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TinhXP] Resource lookup failed: {ex.Message}");
                }

                // Final fallback: Play system beep
                try
                {
                    System.Media.SystemSounds.Exclamation.Play();
                    Console.WriteLine("[TinhXP] ✅ Level up - system beep played (fallback)");
                }
                catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TinhXP] PlayLevelUpSound error: {ex.Message}");
            }
        }

        // Removed server fetch/update methods; DB is used as source of truth
    }

    public enum MatchReturnMode
    {
        ReturnToJoinRoom = 0,
        ReturnToGameLobby = 1
    }

    public class MatchResult
    {
        public string PlayerUsername { get; set; }
        public string OpponentUsername { get; set; }
        public bool PlayerIsWinner { get; set; }
        public TimeSpan MatchTime { get; set; }
        public int PlayerWins { get; set; }
        public int OpponentWins { get; set; }

        public string RoomCode { get; set; }
        public string Token { get; set; }

        public MatchReturnMode ReturnMode { get; set; } = MatchReturnMode.ReturnToJoinRoom;

        public int ParryCount { get; set; }
        public int AttackCount { get; set; }
        public int SkillCount { get; set; }
    }
}

