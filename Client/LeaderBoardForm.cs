using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DoAn_NT106.Services;

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
                    // Query top 20 by level desc, xp desc. DatabaseService doesn't expose this directly,
                    // so use reflection on DatabaseService if method missing - fallback to reading all players not possible here.
                    // We'll attempt to call a method named GetTopPlayers if it exists.
                    var method = db.GetType().GetMethod("GetTopPlayers");
                    if (method != null)
                    {
                        var res = method.Invoke(db, new object[] { 20 }) as System.Collections.IEnumerable;
                        var list = new List<(string Username, int Level, int Xp)>();
                        if (res != null)
                        {
                            foreach (var item in res)
                            {
                                var uname = item.GetType().GetProperty("USERNAME")?.GetValue(item)?.ToString() ?? item.GetType().GetProperty("Username")?.GetValue(item)?.ToString();
                                int lvl = 1;
                                int xp = 0;
                                var p1 = item.GetType().GetProperty("USER_LEVEL") ?? item.GetType().GetProperty("UserLevel") ?? item.GetType().GetProperty("Level");
                                var p2 = item.GetType().GetProperty("XP") ?? item.GetType().GetProperty("Xp") ?? item.GetType().GetProperty("TotalXp");
                                if (p1 != null) int.TryParse(p1.GetValue(item)?.ToString(), out lvl);
                                if (p2 != null) int.TryParse(p2.GetValue(item)?.ToString(), out xp);
                                list.Add((uname ?? "?", lvl, xp));
                            }
                        }
                        return list;
                    }

                    // Fallback: try to call GetAllPlayers then order in-memory
                    var methodAll = db.GetType().GetMethod("GetAllPlayers");
                    if (methodAll != null)
                    {
                        var res = methodAll.Invoke(db, null) as System.Collections.IEnumerable;
                        var list = new List<(string Username, int Level, int Xp)>();
                        if (res != null)
                        {
                            foreach (var item in res)
                            {
                                var uname = item.GetType().GetProperty("USERNAME")?.GetValue(item)?.ToString() ?? item.GetType().GetProperty("Username")?.GetValue(item)?.ToString();
                                int lvl = 1;
                                int xp = 0;
                                var p1 = item.GetType().GetProperty("USER_LEVEL") ?? item.GetType().GetProperty("UserLevel") ?? item.GetType().GetProperty("Level");
                                var p2 = item.GetType().GetProperty("XP") ?? item.GetType().GetProperty("Xp") ?? item.GetType().GetProperty("TotalXp");
                                if (p1 != null) int.TryParse(p1.GetValue(item)?.ToString(), out lvl);
                                if (p2 != null) int.TryParse(p2.GetValue(item)?.ToString(), out xp);
                                list.Add((uname ?? "?", lvl, xp));
                            }
                        }
                        return list.OrderByDescending(x => x.Level).ThenByDescending(x => x.Xp).Take(20).ToList();
                    }

                    // Last fallback: return empty list
                    return new List<(string Username, int Level, int Xp)>();
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
            dgv.Rows.Clear();
            int rank = 1;
            foreach (var p in data)
            {
                dgv.Rows.Add(rank, p.Username, p.Level, p.Xp);
                rank++;
            }

            // If not enough data, show placeholders
            while (rank <= 20)
            {
                dgv.Rows.Add(rank, "-", "-", "-");
                rank++;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
