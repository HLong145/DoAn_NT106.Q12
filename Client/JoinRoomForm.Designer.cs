namespace PixelGameLobby
{
    partial class JoinRoomForm
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.headerPanel = new Panel();
            this.btnBack = new Button();
            this.lblWelcome = new Label();
            this.lblTitle = new Label();
            this.pnlSearch = new Panel();
            this.lb_pass = new Label();
            this.lb_roomcode = new Label();
            this.txtRoomCode = new TextBox();
            this.txtPassword = new TextBox();
            this.btnSearchJoin = new Button();
            this.btnCreateRoom = new Button();
            this.pnlRoomList = new Panel();
            this.roomsPanel = new FlowLayoutPanel();
            this.pnlHelp = new Panel();
            this.btn_refresh = new Button();
            this.btnTestRoom = new Button(); // ✅ NEW: Test Room button
            this.lblHelp = new Label();

            // Global Chat Controls
            this.pnlGlobalChat = new Panel();
            this.pnlChatHeader = new Panel();
            this.pnlChatMessages = new Panel();
            this.pnlChatInput = new Panel();
            this.lblChatTitle = new Label();
            this.lblOnlineCount = new Label();
            this.txtChatInput = new TextBox();
            this.btnSendChat = new Button();

            this.headerPanel.SuspendLayout();
            this.pnlSearch.SuspendLayout();
            this.pnlRoomList.SuspendLayout();
            this.pnlHelp.SuspendLayout();
            this.pnlGlobalChat.SuspendLayout();
            this.pnlChatHeader.SuspendLayout();
            this.pnlChatInput.SuspendLayout();
            this.SuspendLayout();

            // 
            // headerPanel
            // 
            this.headerPanel.BackColor = Color.FromArgb(74, 50, 25);
            this.headerPanel.BorderStyle = BorderStyle.FixedSingle;
            this.headerPanel.Controls.Add(this.btnBack);
            this.headerPanel.Controls.Add(this.lblWelcome);
            this.headerPanel.Location = new Point(100, 20);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new Size(1170, 60);
            this.headerPanel.TabIndex = 0;

            // 
            // btnBack
            // 
            this.btnBack.BackColor = Color.FromArgb(139, 69, 19);
            this.btnBack.FlatStyle = FlatStyle.Flat;
            this.btnBack.Font = new Font("Courier New", 12F, FontStyle.Bold);
            this.btnBack.ForeColor = Color.White;
            this.btnBack.Location = new Point(1000, 12);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new Size(140, 35);
            this.btnBack.TabIndex = 1;
            this.btnBack.Text = "← BACK";
            this.btnBack.UseVisualStyleBackColor = false;
            //this.btnBack.Click += new EventHandler(this.btnBack_Click);

            // 
            // lblWelcome
            // 
            this.lblWelcome.AutoSize = true;
            this.lblWelcome.Font = new Font("Courier New", 14F, FontStyle.Bold);
            this.lblWelcome.ForeColor = Color.Gold;
            this.lblWelcome.Location = new Point(20, 18);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new Size(200, 27);
            this.lblWelcome.TabIndex = 0;
            this.lblWelcome.Text = "Welcome, Player!";

            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = Color.Transparent;
            this.lblTitle.Font = new Font("Goudy Stout", 18F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.Gold;
            this.lblTitle.Location = new Point(500, 85);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(300, 40);
            this.lblTitle.TabIndex = 5;
            this.lblTitle.Text = "JOIN ROOM";

            // 
            // pnlSearch
            // 
            this.pnlSearch.BackColor = Color.FromArgb(74, 50, 25);
            this.pnlSearch.BorderStyle = BorderStyle.FixedSingle;
            this.pnlSearch.Controls.Add(this.lb_pass);
            this.pnlSearch.Controls.Add(this.lb_roomcode);
            this.pnlSearch.Controls.Add(this.txtRoomCode);
            this.pnlSearch.Controls.Add(this.txtPassword);
            this.pnlSearch.Controls.Add(this.btnSearchJoin);
            this.pnlSearch.Controls.Add(this.btnCreateRoom);
            this.pnlSearch.Location = new Point(100, 130);
            this.pnlSearch.Name = "pnlSearch";
            this.pnlSearch.Size = new Size(960, 80);
            this.pnlSearch.TabIndex = 1;

            // 
            // lb_roomcode
            // 
            this.lb_roomcode.AutoSize = true;
            this.lb_roomcode.Font = new Font("Courier New", 11F, FontStyle.Bold);
            this.lb_roomcode.ForeColor = Color.Gold;
            this.lb_roomcode.Location = new Point(21, 10);
            this.lb_roomcode.Name = "lb_roomcode";
            this.lb_roomcode.Size = new Size(130, 23);
            this.lb_roomcode.TabIndex = 5;
            this.lb_roomcode.Text = "ROOM CODE:";

            // 
            // lb_pass
            // 
            this.lb_pass.AutoSize = true;
            this.lb_pass.Font = new Font("Courier New", 11F, FontStyle.Bold);
            this.lb_pass.ForeColor = Color.Gold;
            this.lb_pass.Location = new Point(319, 10);
            this.lb_pass.Name = "lb_pass";
            this.lb_pass.Size = new Size(120, 23);
            this.lb_pass.TabIndex = 6;
            this.lb_pass.Text = "PASSWORD:";

            // 
            // txtRoomCode
            // 
            this.txtRoomCode.BackColor = Color.FromArgb(74, 50, 25);
            this.txtRoomCode.Font = new Font("Courier New", 13.8F, FontStyle.Bold);
            this.txtRoomCode.ForeColor = Color.Gold;
            this.txtRoomCode.Location = new Point(21, 39);
            this.txtRoomCode.Name = "txtRoomCode";
            this.txtRoomCode.PlaceholderText = "Enter room code...";
            this.txtRoomCode.Size = new Size(280, 34);
            this.txtRoomCode.TabIndex = 0;

            // 
            // txtPassword
            // 
            this.txtPassword.BackColor = Color.FromArgb(74, 50, 25);
            this.txtPassword.Font = new Font("Courier New", 13.8F, FontStyle.Bold);
            this.txtPassword.ForeColor = Color.Gold;
            this.txtPassword.Location = new Point(319, 39);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PlaceholderText = "Enter password (if any)...";
            this.txtPassword.Size = new Size(280, 34);
            this.txtPassword.TabIndex = 1;
            this.txtPassword.UseSystemPasswordChar = true;

            // 
            // btnSearchJoin
            // 
            this.btnSearchJoin.BackColor = Color.FromArgb(139, 69, 19);
            this.btnSearchJoin.FlatStyle = FlatStyle.Flat;
            this.btnSearchJoin.Font = new Font("Courier New", 12F, FontStyle.Bold);
            this.btnSearchJoin.ForeColor = Color.White;
            this.btnSearchJoin.Location = new Point(620, 39);
            this.btnSearchJoin.Name = "btnSearchJoin";
            this.btnSearchJoin.Size = new Size(160, 35);
            this.btnSearchJoin.TabIndex = 2;
            this.btnSearchJoin.Text = "FIND ROOM";
            this.btnSearchJoin.UseVisualStyleBackColor = false;

            // 
            // btnCreateRoom
            // 
            this.btnCreateRoom.BackColor = Color.FromArgb(0, 128, 0);
            this.btnCreateRoom.FlatStyle = FlatStyle.Flat;
            this.btnCreateRoom.Font = new Font("Courier New", 12F, FontStyle.Bold);
            this.btnCreateRoom.ForeColor = Color.White;
            this.btnCreateRoom.Location = new Point(790, 39);
            this.btnCreateRoom.Name = "btnCreateRoom";
            this.btnCreateRoom.Size = new Size(160, 35);
            this.btnCreateRoom.TabIndex = 3;
            this.btnCreateRoom.Text = "CREATE ROOM";
            this.btnCreateRoom.UseVisualStyleBackColor = false;

            // 
            // pnlRoomList
            // 
            this.pnlRoomList.BackColor = Color.FromArgb(74, 50, 25);
            this.pnlRoomList.BorderStyle = BorderStyle.FixedSingle;
            this.pnlRoomList.Controls.Add(this.roomsPanel);
            this.pnlRoomList.Location = new Point(100, 220);
            this.pnlRoomList.Name = "pnlRoomList";
            this.pnlRoomList.Size = new Size(960, 390);
            this.pnlRoomList.TabIndex = 2;

            // 
            // roomsPanel
            // 
            this.roomsPanel.AutoScroll = true;
            this.roomsPanel.BackColor = Color.FromArgb(74, 50, 25);
            this.roomsPanel.FlowDirection = FlowDirection.TopDown;
            this.roomsPanel.Location = new Point(10, 10);
            this.roomsPanel.Name = "roomsPanel";
            this.roomsPanel.Size = new Size(938, 368);
            this.roomsPanel.TabIndex = 0;
            this.roomsPanel.WrapContents = false;

            // 
            // pnlHelp
            // 
            this.pnlHelp.BackColor = Color.FromArgb(74, 50, 25);
            this.pnlHelp.BorderStyle = BorderStyle.FixedSingle;
            this.pnlHelp.Controls.Add(this.btn_refresh);
            this.pnlHelp.Controls.Add(this.btnTestRoom); // ✅ NEW: Add to controls
            this.pnlHelp.Controls.Add(this.lblHelp);
            this.pnlHelp.Location = new Point(100, 620);
            this.pnlHelp.Name = "pnlHelp";
            this.pnlHelp.Size = new Size(960, 60);
            this.pnlHelp.TabIndex = 4;

            // 
            // btn_refresh
            // 
            this.btn_refresh.BackColor = Color.DarkOrchid;
            this.btn_refresh.FlatStyle = FlatStyle.Flat;
            this.btn_refresh.Font = new Font("Courier New", 12F, FontStyle.Bold);
            this.btn_refresh.ForeColor = Color.White;
            this.btn_refresh.Location = new Point(800, 13);
            this.btn_refresh.Name = "btn_refresh";
            this.btn_refresh.Size = new Size(152, 35);
            this.btn_refresh.TabIndex = 3;
            this.btn_refresh.Text = "REFRESH";
            this.btn_refresh.UseVisualStyleBackColor = false;
            //this.btn_refresh.Click += new EventHandler(this.btn_refresh_Click);

            // ✅ NEW: Test Room button configuration
            // 
            // btnTestRoom
            // 
            this.btnTestRoom.BackColor = Color.FromArgb(0, 102, 204); // Blue color
            this.btnTestRoom.FlatStyle = FlatStyle.Flat;
            this.btnTestRoom.Font = new Font("Courier New", 11F, FontStyle.Bold);
            this.btnTestRoom.ForeColor = Color.White;
            this.btnTestRoom.Location = new Point(620, 13);
            this.btnTestRoom.Name = "btnTestRoom";
            this.btnTestRoom.Size = new Size(170, 35);
            this.btnTestRoom.TabIndex = 4;
            this.btnTestRoom.Text = "🧪 TEST ROOM";
            this.btnTestRoom.UseVisualStyleBackColor = false;

            // =====================================================
            // GLOBAL CHAT CONTROLS
            // =====================================================

            // 
            // pnlChatHeader
            // 
            this.pnlChatHeader.BackColor = Color.FromArgb(101, 67, 51);
            this.pnlChatHeader.Controls.Add(this.lblOnlineCount);
            this.pnlChatHeader.Controls.Add(this.lblChatTitle);
            this.pnlChatHeader.Dock = DockStyle.Top;
            this.pnlChatHeader.Location = new Point(0, 0);
            this.pnlChatHeader.Name = "pnlChatHeader";
            this.pnlChatHeader.Size = new Size(278, 50);
            this.pnlChatHeader.TabIndex = 0;

            // 
            // lblChatTitle
            // 
            this.lblChatTitle.AutoSize = true;
            this.lblChatTitle.Font = new Font("Courier New", 11F, FontStyle.Bold);
            this.lblChatTitle.ForeColor = Color.Gold;
            this.lblChatTitle.Location = new Point(10, 8);
            this.lblChatTitle.Name = "lblChatTitle";
            this.lblChatTitle.Size = new Size(160, 21);
            this.lblChatTitle.TabIndex = 0;
            this.lblChatTitle.Text = "💬 GLOBAL CHAT";

            // 
            // lblOnlineCount
            // 
            this.lblOnlineCount.AutoSize = true;
            this.lblOnlineCount.Font = new Font("Courier New", 9F, FontStyle.Bold);
            this.lblOnlineCount.ForeColor = Color.LimeGreen;
            this.lblOnlineCount.Location = new Point(10, 28);
            this.lblOnlineCount.Name = "lblOnlineCount";
            this.lblOnlineCount.Size = new Size(100, 18);
            this.lblOnlineCount.TabIndex = 1;
            this.lblOnlineCount.Text = "🟢 0 online";

            // 
            // pnlChatMessages
            // 
            this.pnlChatMessages.AutoScroll = true;
            this.pnlChatMessages.BackColor = Color.FromArgb(74, 50, 25);
            this.pnlChatMessages.Location = new Point(0, 50);
            this.pnlChatMessages.Name = "pnlChatMessages";
            this.pnlChatMessages.Size = new Size(278, 400);
            this.pnlChatMessages.TabIndex = 1;

            // 
            // txtChatInput
            // 
            this.txtChatInput.BackColor = Color.FromArgb(74, 50, 25);
            this.txtChatInput.BorderStyle = BorderStyle.FixedSingle;
            this.txtChatInput.Font = new Font("Courier New", 10F, FontStyle.Bold);
            this.txtChatInput.ForeColor = Color.Gold;
            this.txtChatInput.Location = new Point(5, 13);
            this.txtChatInput.MaxLength = 1000;
            this.txtChatInput.Name = "txtChatInput";
            this.txtChatInput.PlaceholderText = "Nhập tin nhắn...";
            this.txtChatInput.Size = new Size(200, 27);
            this.txtChatInput.TabIndex = 0;

            // 
            // btnSendChat
            // 
            this.btnSendChat.BackColor = Color.FromArgb(139, 69, 19);
            this.btnSendChat.Cursor = Cursors.Hand;
            this.btnSendChat.FlatStyle = FlatStyle.Flat;
            this.btnSendChat.Font = new Font("Courier New", 9F, FontStyle.Bold);
            this.btnSendChat.ForeColor = Color.Gold;
            this.btnSendChat.Location = new Point(208, 12);
            this.btnSendChat.Name = "btnSendChat";
            this.btnSendChat.Size = new Size(65, 30);
            this.btnSendChat.TabIndex = 1;
            this.btnSendChat.Text = "GỬI";
            this.btnSendChat.UseVisualStyleBackColor = false;

            // 
            // pnlChatInput
            // 
            this.pnlChatInput.BackColor = Color.FromArgb(101, 67, 51);
            this.pnlChatInput.Controls.Add(this.btnSendChat);
            this.pnlChatInput.Controls.Add(this.txtChatInput);
            this.pnlChatInput.Dock = DockStyle.Bottom;
            this.pnlChatInput.Location = new Point(0, 450);
            this.pnlChatInput.Name = "pnlChatInput";
            this.pnlChatInput.Size = new Size(278, 56);
            this.pnlChatInput.TabIndex = 2;

            // 
            // pnlGlobalChat
            // 
            this.pnlGlobalChat.BackColor = Color.FromArgb(74, 50, 25);
            this.pnlGlobalChat.BorderStyle = BorderStyle.FixedSingle;
            this.pnlGlobalChat.Controls.Add(this.pnlChatMessages);
            this.pnlGlobalChat.Controls.Add(this.pnlChatInput);
            this.pnlGlobalChat.Controls.Add(this.pnlChatHeader);
            this.pnlGlobalChat.Location = new Point(1080, 130);
            this.pnlGlobalChat.Name = "pnlGlobalChat";
            this.pnlGlobalChat.Size = new Size(280, 508);
            this.pnlGlobalChat.TabIndex = 10;

            // 
            // JoinRoomForm
            // 
            this.AutoScaleMode = AutoScaleMode.None;
            this.BackColor = Color.FromArgb(160, 82, 45);
            this.ClientSize = new Size(1378, 720);
            this.Controls.Add(this.pnlGlobalChat);
            this.Controls.Add(this.headerPanel);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.pnlSearch);
            this.Controls.Add(this.pnlRoomList);
            this.Controls.Add(this.pnlHelp);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "JoinRoomForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Join Room";

            this.pnlChatInput.ResumeLayout(false);
            this.pnlChatInput.PerformLayout();
            this.pnlChatHeader.ResumeLayout(false);
            this.pnlChatHeader.PerformLayout();
            this.pnlGlobalChat.ResumeLayout(false);
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            this.pnlSearch.ResumeLayout(false);
            this.pnlSearch.PerformLayout();
            this.pnlRoomList.ResumeLayout(false);
            this.pnlHelp.ResumeLayout(false);
            this.pnlHelp.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        // Original Controls
        private Panel headerPanel;
        private Button btnBack;
        private Label lblWelcome;
        private Label lblTitle;
        private Panel pnlSearch;
        private Label lb_pass;
        private Label lb_roomcode;
        private TextBox txtRoomCode;
        private TextBox txtPassword;
        private Button btnSearchJoin;
        private Button btnCreateRoom;
        private Panel pnlRoomList;
        private FlowLayoutPanel roomsPanel;
        private Panel pnlHelp;
        private Button btn_refresh;
        private Button btnTestRoom; // ✅ NEW: Test Room button declaration
        private Label lblHelp;

        // Global Chat Controls
        private Panel pnlGlobalChat;
        private Panel pnlChatHeader;
        private Panel pnlChatMessages;
        private Panel pnlChatInput;
        private Label lblChatTitle;
        private Label lblOnlineCount;
        private TextBox txtChatInput;
        private Button btnSendChat;
    }
}