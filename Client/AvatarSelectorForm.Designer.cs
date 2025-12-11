namespace DoAn_NT106
{
    partial class AvatarSelectorForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // AvatarSelectorForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1199, 671);
            // remove outer window border and close button
            FormBorderStyle = FormBorderStyle.None;
            ControlBox = false;
            // Keep title visible inside form via label in designer code if needed
            Name = "AvatarSelectorForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Chọn Avatar";
            ResumeLayout(false);
        }
    }
}