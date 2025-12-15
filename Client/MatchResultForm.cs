using DoAn_NT106.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DoAn_NT106
{
    /// <summary>
    /// Enum for match end reasons
    /// </summary>
    public enum MatchEndReason
    {
        Normal,      // Won by accumulating 2 round wins
        Forfeit,     // Opponent quit/disconnected
        Draw         // Match ended in a draw
    }

    public partial class MatchResultForm : Form
    {
        #region Fields

        private string _winnerName;
        private string _loserName;
        private string _player1Name;  // Host name
        private string _player2Name;  // Guest name
        private int _player1Wins;
        private int _player2Wins;
        private bool _isDraw;
        private MatchEndReason _endReason;
        private System.Windows.Forms.Timer myTimer;

        public event EventHandler ReturnToLobbyRequested;

        #endregion

        #region Constructor

        public MatchResultForm()
        {
            InitializeComponent();
            InitializeCustomUI();
            
            this.Shown += (s, e) =>
            {
                this.BringToFront();
                this.Focus();
                StartAnimations();
                Console.WriteLine("✅ MatchResultForm shown!");
            };
        }

        #endregion

        #region Public Methods to Set Match Results

        /// <summary>
        /// Set match result for a win scenario
        /// </summary>
        public void SetMatchResult(string winner, string loser, int player1Wins, int player2Wins, MatchEndReason reason = MatchEndReason.Normal, string player1Name = "", string player2Name = "")
        {
            _winnerName = winner;
            _loserName = loser;
            _player1Name = player1Name;
            _player2Name = player2Name;
            _player1Wins = player1Wins;
            _player2Wins = player2Wins;
            _isDraw = false;
            _endReason = reason;

            UpdateUI();
        }

        /// <summary>
        /// Set match result for a draw scenario
        /// </summary>
        public void SetMatchResultDraw(string player1Name, string player2Name, int player1Wins, int player2Wins)
        {
            _winnerName = "DRAW";
            _loserName = "";
            _player1Name = player1Name;
            _player2Name = player2Name;
            _player1Wins = player1Wins;
            _player2Wins = player2Wins;
            _isDraw = true;
            _endReason = MatchEndReason.Draw;

            UpdateUI();
        }

        #endregion

        #region UI Initialization

        private void InitializeCustomUI()
        {
            this.AutoScroll = true;
            
            // Set button colors to match FormDangKy style
            btn_ReturnLobby.BackColor = Color.FromArgb(217, 119, 6);
        }

        #endregion

        #region UI Updates

        private void UpdateUI()
        {
            // Update title and result display
            if (_isDraw)
            {
                lbl_Title.Text = "MATCH DRAW";
                lbl_Title.ForeColor = Color.Gold;
                lblWinner.Text = "MATCH DRAW";
                lblWinner.ForeColor = Color.Gold;
                lblWinnerName.Text = "BOTH PLAYERS FOUGHT WELL!";
                lblWinnerName.ForeColor = Color.Cyan;
            }
            else
            {
                lbl_Title.Text = "MATCH RESULT";
                lbl_Title.ForeColor = Color.Gold;
                lblWinner.Text = "VICTORY";
                lblWinner.ForeColor = Color.Gold;
                lblWinnerName.Text = $"{_winnerName.ToUpper()}";
                lblWinnerName.ForeColor = Color.Lime;
            }

            // Update stats
            UpdateStats();
        }

        private void UpdateStats()
        {
            string statsText;

            if (_isDraw)
            {
                statsText = $"=====================================\n\n" +
                           $"ROUND WINS\n\n" +
                           $"{_player1Name.ToUpper()}: {_player1Wins} Wins\n" +
                           $"{_player2Name.ToUpper()}: {_player2Wins} Wins\n\n" +
                           $"=====================================";
            }
            else
            {
                string reason = _endReason switch
                {
                    MatchEndReason.Forfeit => $"(Opponent Quit)",
                    MatchEndReason.Normal => $"(Best of 3)",
                    _ => ""
                };

                // Determine winner's and loser's win counts
                int winnerWins, loserWins;
                
                if (_winnerName == _player1Name)
                {
                    winnerWins = _player1Wins;
                    loserWins = _player2Wins;
                }
                else
                {
                    winnerWins = _player2Wins;
                    loserWins = _player1Wins;
                }

                statsText = $"=====================================\n\n" +
                           $"WINNER: {_winnerName.ToUpper()} {reason}\n\n" +
                           $"MATCH SCORE\n\n" +
                           $"{_winnerName.ToUpper()}: {winnerWins} Wins\n" +
                           $"{_loserName.ToUpper()}: {loserWins} Wins\n\n" +
                           $"=====================================";
            }

            lblStats.Text = statsText;
            lblStats.ForeColor = Color.FromArgb(214, 211, 209);
        }

        #endregion

        #region Button Events

        private void btn_ReturnLobby_Click(object sender, EventArgs e)
        {
            Console.WriteLine("🏠 Return to lobby requested from MatchResultForm");
            StopAnimations();
            ReturnToLobbyRequested?.Invoke(this, EventArgs.Empty);
            this.Close();
        }

        #endregion

        #region Animation

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopAnimations();
            base.OnFormClosing(e);
        }

        private void StopAnimations()
        {
            if (myTimer != null)
            {
                myTimer.Stop();
                myTimer.Dispose();
                myTimer = null;
            }
        }

        public new void Show()
        {
            this.Visible = true;
            this.StartAnimations();
            this.BringToFront();
        }

        public new void Hide()
        {
            this.StopAnimations();
            this.Visible = false;
        }

        public void StartAnimations()
        {
            if (myTimer == null)
            {
                myTimer = new System.Windows.Forms.Timer();
                myTimer.Interval = 50; // animation refresh rate
                myTimer.Tick += MyTimer_Tick;
            }

            myTimer.Start();
        }

        private void MyTimer_Tick(object sender, EventArgs e)
        {
            // Cloud animation (if pictureBox1 exists)
            try
            {
                if (pictureBox1 != null)
                {
                    pictureBox1.Left -= 2;
                    if (pictureBox1.Right < 0)
                        pictureBox1.Left = this.Width;
                }
            }
            catch { }
        }

        private void MatchResultForm_Load(object sender, EventArgs e)
        {
            StartAnimations();
        }

        #endregion
    }
}
