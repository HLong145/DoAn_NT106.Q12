using DoAn_NT106.Services;
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

