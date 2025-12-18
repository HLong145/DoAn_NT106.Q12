using System;
using System.Windows.Forms;

namespace DoAn_NT106.Client
{
    public partial class MapSelectForm : Form
    {
        public string SelectedMap { get; private set; }

        public MapSelectForm()
        {
            InitializeComponent();
            LoadMaps();
        }

        private void LoadMaps()
        {
            try
            {
                //  Initialize mapImages if null
                if (mapImages == null)
                    mapImages = new Dictionary<string, Image>();
                else
                    mapImages.Clear();

                string[] mapNames = { "Battlefield 1", "Battlefield 2", "Battlefield 3", "Battlefield 4" };
                foreach (var m in mapNames)
                {
                    cmbMaps.Items.Add(m);
                }

                // Load images safely
                try { mapImages["Battlefield 1"] = new Bitmap(Properties.Resources.battleground1); } catch (Exception ex) { Console.WriteLine($"Failed to load Battlefield 1: {ex.Message}"); }
                try { mapImages["Battlefield 2"] = new Bitmap(Properties.Resources.battleground2); } catch (Exception ex) { Console.WriteLine($"Failed to load Battlefield 2: {ex.Message}"); }
                try { mapImages["Battlefield 3"] = new Bitmap(Properties.Resources.battleground3); } catch (Exception ex) { Console.WriteLine($"Failed to load Battlefield 3: {ex.Message}"); }
                try { mapImages["Battlefield 4"] = new Bitmap(Properties.Resources.battleground4); } catch (Exception ex) { Console.WriteLine($"Failed to load Battlefield 4: {ex.Message}"); }

                Console.WriteLine($"[LoadMaps] Loaded {mapImages.Count} maps");

                if (cmbMaps.Items.Count > 0)
                    cmbMaps.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadMaps error: {ex.Message}");
            }
        }

        private void CmbMaps_SelectedIndexChanged(object sender, EventArgs e)
        {
            var key = cmbMaps.SelectedItem as string;
            if (key != null && mapImages.ContainsKey(key))
            {
                pbPreview.Image = mapImages[key];
            }
            else
            {
                pbPreview.Image = null;
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (cmbMaps.SelectedItem == null)
            {
                MessageBox.Show("Please choose a battleground.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string chosen = cmbMaps.SelectedItem as string;  // e.g., "Battlefield 2"
            
            //  "Battlefield 2" → "battleground2"
            int mapNum = -1;
            if (chosen.StartsWith("Battlefield"))
            {
                string numStr = chosen.Replace("Battlefield ", "").Trim();
                if (int.TryParse(numStr, out mapNum))
                {
                    SelectedMap = $"battleground{mapNum}";  // e.g., "battleground2"
                }
            }
            
            if (mapNum == -1)
            {
                SelectedMap = "battleground1";  // Fallback
            }

            Console.WriteLine($"[MapSelectForm.BtnOk_Click] Chosen: '{chosen}' -> mapNum={mapNum} -> SelectedMap='{SelectedMap}'");
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
