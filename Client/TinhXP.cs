using DoAn_NT106.Services;
using DoAn_NT106.Client.Class;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoAn_NT106.Client
{
    public partial class TinhXP : Form
    {
        private readonly MatchResult _result;
        private int _gainedXp;      // XP được cộng (100 win / 40 lose)
        private int _xpBefore;      // XP trước trận (0-999)
        private int _xpAfter;       // XP sau trận (0-999)
        private int _levelBefore;   // Level trước trận
        private int _levelAfter;    // Level sau trận
        private int _totalXpBefore; // Ngưỡng XP cần để lên level tiếp theo
        private int _totalXpAfter;  // Ngưỡng XP sau khi tăng level

        // Constructor 1: Nhận MatchResult và tự request XP từ server
        public TinhXP(MatchResult result)
        {
            InitializeComponent();
            AutoScroll = true;
            AutoScrollMinSize = Size.Empty;
            _result = result ?? throw new ArgumentNullException(nameof(result));

            _ = LoadAndDisplayXpAsync();
        }

        // Constructor 2: Nhận XP data trực tiếp từ server (XP_RESULT broadcast)
        // CONSTRUCTOR NÀY KHÔNG REQUEST LẠI SERVER - CHỈ HIỂN THỊ DATA ĐÃ CÓ
        public TinhXP(MatchResult result, int gainedXp, int oldXp, int newXp, int oldLevel, int newLevel, int totalXp)
        {
            InitializeComponent();
            AutoScroll = true;
            AutoScrollMinSize = Size.Empty;
            _result = result ?? throw new ArgumentNullException(nameof(result));

            // Set XP từ server TRỰC TIẾP - DATA ĐÃ ĐƯỢC DATABASE XÁC NHẬN
            _gainedXp = gainedXp;           // XP được cộng (100 win / 40 lose)
            _xpBefore = oldXp;              // XP trước khi cộng (VD: 600)
            _xpAfter = newXp;               // XP sau khi cộng (VD: 700) - ĐÃ LƯU VÀO DATABASE
            _levelBefore = oldLevel;        // Level trước khi update
            _levelAfter = newLevel;         // Level sau khi update - ĐÃ LƯU VÀO DATABASE
            _totalXpBefore = oldLevel * 1000;
            _totalXpAfter = totalXp;        // TOTAL_XP từ database (VD: 2000 nếu Level 2)

            Console.WriteLine($"[TinhXP] 📊 ===== XP FROM DATABASE (via server broadcast) =====");
            Console.WriteLine($"  - Gained XP: +{_gainedXp}");
            Console.WriteLine($"  - XP Before: {_xpBefore} (Level {_levelBefore})");
            Console.WriteLine($"  - XP After: {_xpAfter} (Level {_levelAfter})");  // PHẢI SHOW GIÁ TRỊ ĐÚNG
            Console.WriteLine($"  - TOTAL_XP: {_totalXpAfter}");
            Console.WriteLine($"[TinhXP] =================================================");

            // Phát âm thanh nếu lên level
            if (_levelAfter > _levelBefore)
            {
                try
                {
                    PlayLevelUpSound();
                    Console.WriteLine($"[TinhXP] 🎉 LEVEL UP! {_levelBefore} → {_levelAfter}");
                }
                catch { }
            }

            // Cập nhật UI ngay lập tức với data từ server (KHÔNG CẦN ASYNC)
            UpdateUi();
        }

        public TinhXP() : this(new MatchResult())
        {
        }

        /// <summary>
        /// Gửi request GET_PLAYER_XP để lấy XP hiện tại từ server
        /// </summary>
        private async Task<(int xp, int totalXp, int level)> RequestPlayerXpAsync(string username, string token)
        {
            try
            {
                Console.WriteLine($"[TinhXP] 📤 Sending GET_PLAYER_XP for {username}");

                var response = await PersistentTcpClient.Instance.SendRequestAsync(
                    "GET_PLAYER_XP",
                    new Dictionary<string, object>
                    {
                        { "username", username },
                        { "token", token }
                    });

                Console.WriteLine($"[TinhXP] 📥 GET_PLAYER_XP Response: Success={response.Success}, Message={response.Message}");

                if (!response.Success || response.Data == null)
                {
                    Console.WriteLine($"[TinhXP] ❌ GET_PLAYER_XP failed: {response.Message}");
                    return (0, 1000, 1);
                }

                int xp = 0;
                int totalXp = 1000;
                int level = 1;

                if (response.Data.TryGetValue("xp", out var xpObj))
                {
                    xp = Convert.ToInt32(xpObj);
                    Console.WriteLine($"[TinhXP] XP from server: {xp}");
                }

                if (response.Data.TryGetValue("totalXp", out var totalObj))
                {
                    totalXp = Convert.ToInt32(totalObj);
                    Console.WriteLine($"[TinhXP] TotalXP from server: {totalXp}");
                }

                if (response.Data.TryGetValue("level", out var lvlObj))
                {
                    level = Math.Max(1, Convert.ToInt32(lvlObj));
                    Console.WriteLine($"[TinhXP] Level from server: {level}");
                }

                Console.WriteLine($"[TinhXP] ✅ Got player XP: XP={xp}, TotalXP={totalXp}, Level={level}");
                return (xp, totalXp, level);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TinhXP] ❌ Error in RequestPlayerXpAsync: {ex.Message}");
                return (0, 1000, 1);
            }
        }

        /// <summary>
        /// Gửi request UPDATE_PLAYER_XP để cập nhật XP (server sẽ xử lý logic level up)
        /// </summary>
        private async Task<(bool success, int newXp, int newTotalXp, int newLevel)> RequestUpdatePlayerXpAsync(
            string username,
            string token,
            int gainedXp)
        {
            try
            {
                Console.WriteLine($"[TinhXP] 📤 Sending UPDATE_PLAYER_XP: {username} +{gainedXp} XP");

                var response = await PersistentTcpClient.Instance.SendRequestAsync(
                    "UPDATE_PLAYER_XP",
                    new Dictionary<string, object>
                    {
                        { "username", username },
                        { "token", token },
                        { "gainedXp", gainedXp }
                    });

                Console.WriteLine($"[TinhXP] 📥 UPDATE_PLAYER_XP Response: Success={response.Success}, Message={response.Message}");

                if (!response.Success || response.Data == null)
                {
                    Console.WriteLine($"[TinhXP] ❌ UPDATE failed: {response.Message}");
                    return (false, 0, 1000, 1);
                }

                int newXp = 0;
                int newTotalXp = 1000;
                int newLevel = 1;

                if (response.Data.TryGetValue("xp", out var xpObj))
                {
                    newXp = Convert.ToInt32(xpObj);
                    Console.WriteLine($"[TinhXP] New XP: {newXp}");
                }

                if (response.Data.TryGetValue("totalXp", out var totalObj))
                {
                    newTotalXp = Convert.ToInt32(totalObj);
                    Console.WriteLine($"[TinhXP] New TotalXP: {newTotalXp}");
                }

                if (response.Data.TryGetValue("level", out var lvlObj))
                {
                    newLevel = Math.Max(1, Convert.ToInt32(lvlObj));
                    Console.WriteLine($"[TinhXP] New Level: {newLevel}");
                }

                Console.WriteLine($"[TinhXP] ✅ After update: XP={newXp}, TotalXP={newTotalXp}, Level={newLevel}");
                return (true, newXp, newTotalXp, newLevel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TinhXP] ❌ Error in RequestUpdatePlayerXpAsync: {ex.Message}");
                return (false, 0, 1000, 1);
            }
        }

        private async Task LoadAndDisplayXpAsync()
        {
            Console.WriteLine($"[TinhXP] ========== STARTING XP CALCULATION ==========");
            Console.WriteLine($"[TinhXP] Player: {_result.PlayerUsername}, Win: {_result.PlayerIsWinner}");

            // 1. Lấy XP/Level TRƯỚC trận từ database
            try
            {
                if (!string.IsNullOrEmpty(_result.PlayerUsername) && !string.IsNullOrEmpty(_result.Token))
                {
                    var before = await RequestPlayerXpAsync(_result.PlayerUsername, _result.Token);
                    _xpBefore = before.xp;
                    _totalXpBefore = before.totalXp;
                    _levelBefore = before.level;

                    Console.WriteLine($"[TinhXP] 📊 BEFORE MATCH: XP={_xpBefore}, TotalXP={_totalXpBefore}, Level={_levelBefore}");
                }
                else
                {
                    Console.WriteLine($"[TinhXP] ⚠️ Missing username or token, using defaults");
                    _xpBefore = 0;
                    _totalXpBefore = 1000;
                    _levelBefore = 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TinhXP] ❌ Error getting XP before: {ex.Message}");
                _xpBefore = 0;
                _totalXpBefore = 1000;
                _levelBefore = 1;
            }

            // 2. Tính XP được cộng (thắng: 100, thua: 40)
            _gainedXp = _result.PlayerIsWinner ? 100 : 40;
            Console.WriteLine($"[TinhXP] 💰 Gained XP: {_gainedXp} ({(_result.PlayerIsWinner ? "WIN" : "LOSE")})");

            // 3. Gửi request cập nhật XP lên server
            try
            {
                if (!string.IsNullOrEmpty(_result.PlayerUsername) && !string.IsNullOrEmpty(_result.Token))
                {
                    var update = await RequestUpdatePlayerXpAsync(
                        _result.PlayerUsername,
                        _result.Token,
                        _gainedXp);

                    if (update.success)
                    {
                        _xpAfter = update.newXp;
                        _totalXpAfter = update.newTotalXp;
                        _levelAfter = update.newLevel;

                        Console.WriteLine($"[TinhXP] 📊 AFTER UPDATE (from server): XP={_xpAfter}, TotalXP={_totalXpAfter}, Level={_levelAfter}");

                        //  CRITICAL: Chờ 200ms để database commit xong
                        await Task.Delay(200);

                        //  REQUEST LẠI để lấy dữ liệu CHÍNH XÁC từ database
                        Console.WriteLine($"[TinhXP] 🔄 Requesting FRESH data from database...");
                        var freshData = await RequestPlayerXpAsync(_result.PlayerUsername, _result.Token);

                        // Cập nhật lại với dữ liệu MỚI NHẤT từ database
                        _xpAfter = freshData.xp;
                        _totalXpAfter = freshData.totalXp;
                        _levelAfter = freshData.level;

                        Console.WriteLine($"[TinhXP] ✅ FRESH DATA FROM DB: XP={_xpAfter}, TotalXP={_totalXpAfter}, Level={_levelAfter}");
                    }
                    else
                    {
                        // Fallback: tính toán local nếu server fail
                        Console.WriteLine("[TinhXP] ⚠️ Server update failed, using local calculation");
                        CalculateXpLocally();
                    }
                }
                else
                {
                    Console.WriteLine("[TinhXP] ⚠️ Missing credentials, using local calculation");
                    CalculateXpLocally();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TinhXP] ❌ Error updating XP: {ex.Message}");
                CalculateXpLocally();
            }

            // 4. Phát âm thanh nếu lên level
            if (_levelAfter > _levelBefore)
            {
                try
                {
                    PlayLevelUpSound();
                    Console.WriteLine($"[TinhXP] 🎉 LEVEL UP! {_levelBefore} → {_levelAfter}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TinhXP] ⚠️ Error playing sound: {ex.Message}");
                }
            }

            Console.WriteLine($"[TinhXP] ========== XP CALCULATION COMPLETE ==========");

            // 5. Cập nhật UI
            if (InvokeRequired)
                Invoke(new Action(UpdateUi));
            else
                UpdateUi();
        }

        /// <summary>
        /// Tính toán XP local nếu server không phản hồi (fallback)
        /// </summary>
        private void CalculateXpLocally()
        {
            _xpAfter = _xpBefore + _gainedXp;
            _levelAfter = _levelBefore;
            _totalXpAfter = _totalXpBefore;

            const int XP_PER_LEVEL = 1000;

            // Xử lý lên level
            while (_xpAfter >= XP_PER_LEVEL)
            {
                _xpAfter -= XP_PER_LEVEL;
                _levelAfter++;
                _totalXpAfter += XP_PER_LEVEL;
            }

            Console.WriteLine($"[TinhXP] 🔧 Local calculation: XP={_xpAfter}, Level={_levelAfter}, TotalXP={_totalXpAfter}");
        }

        private void UpdateUi()
        {
            Console.WriteLine($"[TinhXP] 🎨 ===== UPDATING UI =====");
            Console.WriteLine($"  - XP Gained: +{_gainedXp}");
            Console.WriteLine($"  - XP Before: {_xpBefore}");
            Console.WriteLine($"  - XP After: {_xpAfter}");  // PHẢI SHOW 700
            Console.WriteLine($"  - Level: {_levelBefore} → {_levelAfter}");
            Console.WriteLine($"  - TotalXP: {_totalXpAfter}");

            // 1. Hiển thị XP earned
            if (lbl_XPEarnedValue != null)
            {
                lbl_XPEarnedValue.Text = $"+{_gainedXp} XP";
            }

            // 2. Hiển thị tên player
            if (lbl_PlayerValue != null)
            {
                lbl_PlayerValue.Text = _result.PlayerUsername ?? "PLAYER";
            }

            // 3. Hiển thị kết quả (WIN/LOSE)
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

            // 4. Hiển thị thời gian
            if (lbl_TimeValue != null)
            {
                TimeSpan time = _result.MatchTime;
                lbl_TimeValue.Text = time <= TimeSpan.Zero
                    ? "00:00"
                    : string.Format("{0:00}:{1:00}", (int)time.TotalMinutes, time.Seconds);
            }

            // 5. Hiển thị level progress
            if (lbl_XPProgress != null)
            {
                if (_levelAfter > _levelBefore)
                {
                    lbl_XPProgress.Text = $"Level {_levelBefore} → Level {_levelAfter}";
                    lbl_XPProgress.ForeColor = Color.Gold;
                }
                else
                {
                    lbl_XPProgress.Text = $"Level {_levelAfter}";
                }
            }

            // 6. HIỂN THỊ XP PROGRESS VALUE - DÙNG _xpAfter TỪ DATABASE
            if (lbl_XPProgressValue != null)
            {
                const int XP_PER_LEVEL = 1000;
                // _xpAfter ĐÃ LÀ GIÁ TRỊ TỪ DATABASE (VD: 700)
                lbl_XPProgressValue.Text = $"{_xpAfter}/{XP_PER_LEVEL} XP";
                Console.WriteLine($"[TinhXP] UI: XP Progress = {_xpAfter}/{XP_PER_LEVEL} XP");  // PHẢI LOG: 700/1000 XP
            }

            // 7. HIỂN THỊ XP BAR - DÙNG _xpAfter TỪ DATABASE
            if (pnl_XPBarFill != null && pnl_XPBarContainer != null)
            {
                const int XP_PER_LEVEL = 1000;

                // _xpAfter = 700 → percent = 70%
                float percent = (_xpAfter * 100f) / XP_PER_LEVEL;

                int maxWidth = pnl_XPBarContainer.Width;
                int fillWidth = (int)(maxWidth * (percent / 100f));

                // Safety clamp
                fillWidth = Math.Max(0, Math.Min(maxWidth, fillWidth));

                pnl_XPBarFill.Width = fillWidth;

                Console.WriteLine($"[TinhXP] UI: XP Bar = {_xpAfter}/{XP_PER_LEVEL} = {percent:F1}% (width: {fillWidth}px)");  // PHẢI LOG: 70%
            }

            Console.WriteLine($"[TinhXP] ✅ UI Update Complete");
        }   

        private void btn_Continue_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PlayLevelUpSound()
        {
            System.Media.SystemSounds.Exclamation.Play();
        }
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

        public int Xp { get; set; }
    }
}