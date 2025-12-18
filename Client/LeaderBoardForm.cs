using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using DoAn_NT106.Services;
using DoAn_NT106.Server;

namespace DoAn_NT106
{
    public partial class LeaderBoardForm : Form
    {
        public LeaderBoardForm()
        {
            InitializeComponent();
            Load += LeaderBoardForm_Load;
        }

        private async void LeaderBoardForm_Load(object sender, EventArgs e)
        {
            await LoadTopPlayersAsync();
        }

        private Task<List<(string Username,int Level,int Xp)>> FetchTopPlayersAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    var db = new DatabaseService();
                    var res = db.GetTopPlayers(10);
                    var list = new List<(string Username, int Level, int Xp)>();
                    if (res != null)
                    {
                        foreach (var item in res)
                        {
                            list.Add((item.Username ?? "?", item.UserLevel, item.Xp));
                        }
                    }
                    return list;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Leaderboard fetch error: " + ex.Message);
                    return new List<(string Username, int Level, int Xp)>();
                }
            });
        }

        private async Task LoadTopPlayersAsync()
        {
            var data = await FetchTopPlayersAsync();

            // Update top 3 display
            var top3 = data.Take(3).ToList();
            if (top3.Count >= 1)
            {
                lblTop1Name.Text = top3[0].Username;
                lblTop1Level.Text = $"Lv {top3[0].Level}";
                lblTop1Score.Text = top3[0].Xp.ToString();
            }
            else
            {
                lblTop1Name.Text = "-";
                lblTop1Score.Text = "-";
            }

            if (top3.Count >= 2)
            {
                lblTop2Name.Text = top3[1].Username;
                lblTop2Level.Text = $"Lv {top3[1].Level}";
                lblTop2Score.Text = top3[1].Xp.ToString();
            }
            else
            {
                lblTop2Name.Text = "-";
                lblTop2Score.Text = "-";
            }

            if (top3.Count >= 3)
            {
                lblTop3Name.Text = top3[2].Username;
                lblTop3Level.Text = $"Lv {top3[2].Level}";
                lblTop3Score.Text = top3[2].Xp.ToString();
            }
            else
            {
                lblTop3Name.Text = "-";
                lblTop3Score.Text = "-";
            }

            // Load avatars for top3 if user saved one locally (UserAvatars\{username}.txt)
            try
            {
                // top1
                if (top3.Count >= 1)
                {
                    var img = LoadPlayerAvatar(top3[0].Username, pbTop1.ClientSize) ?? Properties.Resources.boy1;
                    pbTop1.Image = img;
                    pbTop1.SizeMode = PictureBoxSizeMode.Zoom;
                    pbTop1.Visible = true;
                    pbCrown.Visible = true;
                    // ensure crown and avatars are on top
                    pbTop1.BringToFront();
                    pbCrown.BringToFront();
                }

                // top2
                if (top3.Count >= 2)
                {
                    var img = LoadPlayerAvatar(top3[1].Username, pbTop2.ClientSize) ?? Properties.Resources.boy2;
                    pbTop2.Image = img;
                    pbTop2.SizeMode = PictureBoxSizeMode.Zoom;
                    pbTop2.Visible = true;
                    pbTop2.BringToFront();
                    pbRank2.Visible = true;
                    pbRank2.BringToFront();
                }

                // top3
                if (top3.Count >= 3)
                {
                    var img = LoadPlayerAvatar(top3[2].Username, pbTop3.ClientSize) ?? Properties.Resources.boy3;
                    pbTop3.Image = img;
                    pbTop3.SizeMode = PictureBoxSizeMode.Zoom;
                    pbTop3.Visible = true;
                    pbTop3.BringToFront();
                    pbRank3.Visible = true;
                    pbRank3.BringToFront();
                }
            }
            catch { }

            // Populate the grid starting from rank 4
            dgv.Rows.Clear();
            int rank = 4;
            foreach (var p in data.Skip(3))
            {
                dgv.Rows.Add(rank, p.Level, p.Username, p.Xp);
                rank++;
            }
            // Fill placeholders to show up to 10
            while (rank <= 10)
            {
                dgv.Rows.Add(rank, "-", "-", "-");
                rank++;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private Image LoadPlayerAvatar(string username, Size targetSize)
        {
            try
            {
                if (string.IsNullOrEmpty(username)) return null;
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserAvatars");
                string txt = Path.Combine(folder, username + ".txt");
                if (!File.Exists(txt)) return null;
                string content = File.ReadAllText(txt).Trim();
                if (!int.TryParse(content, out int idx)) return null;

                // Map index to resource (same as MainForm.gameAvatars ordering)
                Image res = idx switch
                {
                    0 => Properties.Resources.avt_knightgirl,
                    1 => Properties.Resources.avt_bringer,
                    2 => Properties.Resources.avt_warrior,
                    3 => Properties.Resources.avt_goatman,
                    _ => null
                };

                if (res == null) return null;

                // Create fitted image
                var bmp = new Bitmap(targetSize.Width > 0 ? targetSize.Width : res.Width, targetSize.Height > 0 ? targetSize.Height : res.Height);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    float scale = Math.Min((float)bmp.Width / res.Width, (float)bmp.Height / res.Height);
                    int w = (int)(res.Width * scale);
                    int h = (int)(res.Height * scale);
                    int x = (bmp.Width - w) / 2;
                    int y = (bmp.Height - h) / 2;
                    g.DrawImage(res, new Rectangle(x, y, w, h));
                }
                return bmp;
            }
            catch { return null; }
        }
    }
}
