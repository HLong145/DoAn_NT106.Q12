using System;
using System.Drawing;
using System.Windows.Forms;

namespace DoAn_NT106
{
    public partial class AvatarSelectorForm : Form
    {
        public int SelectedIndex { get; private set; } = -1;
        private Image[] avatars;

        public AvatarSelectorForm(Image[] gameAvatars)
        {
            InitializeComponent();
            avatars = gameAvatars;
            CreateAvatarGrid();
        }

        // ✅ Tạo lưới avatar
        private void CreateAvatarGrid()
        {
            int size = 80;
            int margin = 15;
            int cols = 3;

            for (int i = 0; i < avatars.Length; i++)
            {
                PictureBox pb = new PictureBox();
                pb.Image = avatars[i];
                pb.Width = size;
                pb.Height = size;
                pb.SizeMode = PictureBoxSizeMode.Zoom;
                pb.BorderStyle = BorderStyle.FixedSingle;
                pb.Cursor = Cursors.Hand;
                pb.Tag = i;
                pb.Left = 10 + (i % cols) * (size + margin);
                pb.Top = 10 + (i / cols) * (size + margin);

                pb.Click += Avatar_Click;
                Controls.Add(pb);
            }
        }

        // ✅ Khi click avatar
        private void Avatar_Click(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            SelectedIndex = (int)pb.Tag;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}