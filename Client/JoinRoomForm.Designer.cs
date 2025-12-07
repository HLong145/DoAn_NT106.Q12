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
            headerPanel = new Panel();
            btnBack = new Button();
            lblWelcome = new Label();
            lblTitle = new Label();
            pnlSearch = new Panel();
            lb_pass = new Label();
            lb_roomcode = new Label();
            txtRoomCode = new TextBox();
            txtPassword = new TextBox();
            btnSearchJoin = new Button();
            btnCreateRoom = new Button();
            pnlRoomList = new Panel();
            roomsPanel = new FlowLayoutPanel();
            pnlHelp = new Panel();
            btn_refresh = new Button();
            btnTestRoom = new Button();
            lblHelp = new Label();
            pnlGlobalChat = new Panel();
            pnlChatMessages = new Panel();
            pnlChatInput = new Panel();
            btnSendChat = new Button();
            txtChatInput = new TextBox();
            pnlChatHeader = new Panel();
            lblOnlineCount = new Label();
            lblChatTitle = new Label();
            headerPanel.SuspendLayout();
            pnlSearch.SuspendLayout();
            pnlRoomList.SuspendLayout();
            pnlHelp.SuspendLayout();
            pnlGlobalChat.SuspendLayout();
            pnlChatInput.SuspendLayout();
            pnlChatHeader.SuspendLayout();
            SuspendLayout();
            // 
            // headerPanel
            // 
            headerPanel.BackColor = Color.FromArgb(74, 50, 25);
            headerPanel.BorderStyle = BorderStyle.FixedSingle;
            headerPanel.Controls.Add(btnBack);
            headerPanel.Controls.Add(lblWelcome);
            headerPanel.Location = new Point(100, 20);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(1170, 60);
            headerPanel.TabIndex = 0;
            // 
            // btnBack
            // 
            btnBack.BackColor = Color.FromArgb(139, 69, 19);
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btnBack.ForeColor = Color.White;
            btnBack.Location = new Point(1000, 12);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(140, 35);
            btnBack.TabIndex = 1;
            btnBack.Text = "← BACK";
            btnBack.UseVisualStyleBackColor = false;
            // 
            // lblWelcome
            // 
            lblWelcome.AutoSize = true;
            lblWelcome.Font = new Font("Courier New", 14F, FontStyle.Bold);
            lblWelcome.ForeColor = Color.Gold;
            lblWelcome.Location = new Point(20, 18);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new Size(236, 27);
            lblWelcome.TabIndex = 0;
            lblWelcome.Text = "Welcome, Player!";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Goudy Stout", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Gold;
            lblTitle.Location = new Point(500, 85);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(344, 41);
            lblTitle.TabIndex = 5;
            lblTitle.Text = "JOIN ROOM";
            // 
            // pnlSearch
            // 
            pnlSearch.BackColor = Color.FromArgb(74, 50, 25);
            pnlSearch.BorderStyle = BorderStyle.FixedSingle;
            pnlSearch.Controls.Add(lb_pass);
            pnlSearch.Controls.Add(lb_roomcode);
            pnlSearch.Controls.Add(txtRoomCode);
            pnlSearch.Controls.Add(txtPassword);
            pnlSearch.Controls.Add(btnSearchJoin);
            pnlSearch.Controls.Add(btnCreateRoom);
            pnlSearch.Location = new Point(100, 130);
            pnlSearch.Name = "pnlSearch";
            pnlSearch.Size = new Size(960, 80);
            pnlSearch.TabIndex = 1;
            // 
            // lb_pass
            // 
            lb_pass.AutoSize = true;
            lb_pass.Font = new Font("Courier New", 11F, FontStyle.Bold);
            lb_pass.ForeColor = Color.Gold;
            lb_pass.Location = new Point(319, 10);
            lb_pass.Name = "lb_pass";
            lb_pass.Size = new Size(109, 22);
            lb_pass.TabIndex = 6;
            lb_pass.Text = "PASSWORD:";
            // 
            // lb_roomcode
            // 
            lb_roomcode.AutoSize = true;
            lb_roomcode.Font = new Font("Courier New", 11F, FontStyle.Bold);
            lb_roomcode.ForeColor = Color.Gold;
            lb_roomcode.Location = new Point(21, 10);
            lb_roomcode.Name = "lb_roomcode";
            lb_roomcode.Size = new Size(120, 22);
            lb_roomcode.TabIndex = 5;
            lb_roomcode.Text = "ROOM CODE:";
            // 
            // txtRoomCode
            // 
            txtRoomCode.BackColor = Color.FromArgb(74, 50, 25);
            txtRoomCode.Font = new Font("Courier New", 13.8F, FontStyle.Bold);
            txtRoomCode.ForeColor = Color.Gold;
            txtRoomCode.Location = new Point(21, 39);
            txtRoomCode.Name = "txtRoomCode";
            txtRoomCode.PlaceholderText = "Enter room code...";
            txtRoomCode.Size = new Size(280, 34);
            txtRoomCode.TabIndex = 0;
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.FromArgb(74, 50, 25);
            txtPassword.Font = new Font("Courier New", 13.8F, FontStyle.Bold);
            txtPassword.ForeColor = Color.Gold;
            txtPassword.Location = new Point(319, 39);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "Enter password (if any)...";
            txtPassword.Size = new Size(280, 34);
            txtPassword.TabIndex = 1;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnSearchJoin
            // 
            btnSearchJoin.BackColor = Color.FromArgb(139, 69, 19);
            btnSearchJoin.FlatStyle = FlatStyle.Flat;
            btnSearchJoin.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btnSearchJoin.ForeColor = Color.White;
            btnSearchJoin.Location = new Point(620, 39);
            btnSearchJoin.Name = "btnSearchJoin";
            btnSearchJoin.Size = new Size(160, 35);
            btnSearchJoin.TabIndex = 2;
            btnSearchJoin.Text = "FIND ROOM";
            btnSearchJoin.UseVisualStyleBackColor = false;
            // 
            // btnCreateRoom
            // 
            btnCreateRoom.BackColor = Color.FromArgb(0, 128, 0);
            btnCreateRoom.FlatStyle = FlatStyle.Flat;
            btnCreateRoom.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btnCreateRoom.ForeColor = Color.White;
            btnCreateRoom.Location = new Point(790, 39);
            btnCreateRoom.Name = "btnCreateRoom";
            btnCreateRoom.Size = new Size(160, 35);
            btnCreateRoom.TabIndex = 3;
            btnCreateRoom.Text = "CREATE ROOM";
            btnCreateRoom.UseVisualStyleBackColor = false;
            // 
            // pnlRoomList
            // 
            pnlRoomList.BackColor = Color.FromArgb(74, 50, 25);
            pnlRoomList.BorderStyle = BorderStyle.FixedSingle;
            pnlRoomList.Controls.Add(roomsPanel);
            pnlRoomList.Location = new Point(100, 220);
            pnlRoomList.Name = "pnlRoomList";
            pnlRoomList.Size = new Size(960, 390);
            pnlRoomList.TabIndex = 2;
            // 
            // roomsPanel
            // 
            roomsPanel.AutoScroll = true;
            roomsPanel.BackColor = Color.FromArgb(74, 50, 25);
            roomsPanel.FlowDirection = FlowDirection.TopDown;
            roomsPanel.Location = new Point(10, 10);
            roomsPanel.Name = "roomsPanel";
            roomsPanel.Size = new Size(938, 368);
            roomsPanel.TabIndex = 0;
            roomsPanel.WrapContents = false;
            // 
            // pnlHelp
            // 
            pnlHelp.BackColor = Color.FromArgb(74, 50, 25);
            pnlHelp.BorderStyle = BorderStyle.FixedSingle;
            pnlHelp.Controls.Add(btn_refresh);
            pnlHelp.Controls.Add(btnTestRoom);
            pnlHelp.Controls.Add(lblHelp);
            pnlHelp.Location = new Point(100, 620);
            pnlHelp.Name = "pnlHelp";
            pnlHelp.Size = new Size(960, 60);
            pnlHelp.TabIndex = 4;
            // 
            // btn_refresh
            // 
            btn_refresh.BackColor = Color.DarkOrchid;
            btn_refresh.FlatStyle = FlatStyle.Flat;
            btn_refresh.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btn_refresh.ForeColor = Color.White;
            btn_refresh.Location = new Point(800, 13);
            btn_refresh.Name = "btn_refresh";
            btn_refresh.Size = new Size(152, 35);
            btn_refresh.TabIndex = 3;
            btn_refresh.Text = "REFRESH";
            btn_refresh.UseVisualStyleBackColor = false;
            // 
            // btnTestRoom
            // 
            btnTestRoom.BackColor = Color.FromArgb(0, 102, 204);
            btnTestRoom.FlatStyle = FlatStyle.Flat;
            btnTestRoom.Font = new Font("Courier New", 11F, FontStyle.Bold);
            btnTestRoom.ForeColor = Color.White;
            btnTestRoom.Location = new Point(620, 13);
            btnTestRoom.Name = "btnTestRoom";
            btnTestRoom.Size = new Size(170, 35);
            btnTestRoom.TabIndex = 4;
            btnTestRoom.Text = "\U0001f9ea TEST ROOM";
            btnTestRoom.UseVisualStyleBackColor = false;
            // 
            // lblHelp
            // 
            lblHelp.Location = new Point(0, 0);
            lblHelp.Name = "lblHelp";
            lblHelp.Size = new Size(100, 23);
            lblHelp.TabIndex = 5;
            // 
            // pnlGlobalChat
            // 
            pnlGlobalChat.BackColor = Color.FromArgb(74, 50, 25);
            pnlGlobalChat.BorderStyle = BorderStyle.FixedSingle;
            pnlGlobalChat.Controls.Add(pnlChatMessages);
            pnlGlobalChat.Controls.Add(pnlChatInput);
            pnlGlobalChat.Controls.Add(pnlChatHeader);
            pnlGlobalChat.Location = new Point(1080, 130);
            pnlGlobalChat.Name = "pnlGlobalChat";
            pnlGlobalChat.Size = new Size(280, 508);
            pnlGlobalChat.TabIndex = 10;
            // 
            // pnlChatMessages
            // 
            pnlChatMessages.AutoScroll = true;
            pnlChatMessages.BackColor = Color.FromArgb(74, 50, 25);
            pnlChatMessages.Location = new Point(0, 50);
            pnlChatMessages.Name = "pnlChatMessages";
            pnlChatMessages.Size = new Size(278, 400);
            pnlChatMessages.TabIndex = 1;
            // 
            // pnlChatInput
            // 
            pnlChatInput.BackColor = Color.FromArgb(101, 67, 51);
            pnlChatInput.Controls.Add(btnSendChat);
            pnlChatInput.Controls.Add(txtChatInput);
            pnlChatInput.Dock = DockStyle.Bottom;
            pnlChatInput.Location = new Point(0, 450);
            pnlChatInput.Name = "pnlChatInput";
            pnlChatInput.Size = new Size(278, 56);
            pnlChatInput.TabIndex = 2;
            // 
            // btnSendChat
            // 
            btnSendChat.BackColor = Color.FromArgb(139, 69, 19);
            btnSendChat.Cursor = Cursors.Hand;
            btnSendChat.FlatStyle = FlatStyle.Flat;
            btnSendChat.Font = new Font("Courier New", 9F, FontStyle.Bold);
            btnSendChat.ForeColor = Color.Gold;
            btnSendChat.Location = new Point(208, 12);
            btnSendChat.Name = "btnSendChat";
            btnSendChat.Size = new Size(65, 30);
            btnSendChat.TabIndex = 1;
            btnSendChat.Text = "SEND";
            btnSendChat.UseVisualStyleBackColor = false;
            // 
            // txtChatInput
            // 
            txtChatInput.BackColor = Color.FromArgb(74, 50, 25);
            txtChatInput.BorderStyle = BorderStyle.FixedSingle;
            txtChatInput.Font = new Font("Courier New", 10F, FontStyle.Bold);
            txtChatInput.ForeColor = Color.Gold;
            txtChatInput.Location = new Point(5, 13);
            txtChatInput.MaxLength = 1000;
            txtChatInput.Name = "txtChatInput";
            txtChatInput.PlaceholderText = "Send message...";
            txtChatInput.Size = new Size(200, 26);
            txtChatInput.TabIndex = 0;
            // 
            // pnlChatHeader
            // 
            pnlChatHeader.BackColor = Color.FromArgb(101, 67, 51);
            pnlChatHeader.Controls.Add(lblOnlineCount);
            pnlChatHeader.Controls.Add(lblChatTitle);
            pnlChatHeader.Dock = DockStyle.Top;
            pnlChatHeader.Location = new Point(0, 0);
            pnlChatHeader.Name = "pnlChatHeader";
            pnlChatHeader.Size = new Size(278, 50);
            pnlChatHeader.TabIndex = 0;
            // 
            // lblOnlineCount
            // 
            lblOnlineCount.AutoSize = true;
            lblOnlineCount.Font = new Font("Courier New", 9F, FontStyle.Bold);
            lblOnlineCount.ForeColor = Color.LimeGreen;
            lblOnlineCount.Location = new Point(10, 28);
            lblOnlineCount.Name = "lblOnlineCount";
            lblOnlineCount.Size = new Size(106, 17);
            lblOnlineCount.TabIndex = 1;
            lblOnlineCount.Text = "\U0001f7e2 0 online";
            // 
            // lblChatTitle
            // 
            lblChatTitle.AutoSize = true;
            lblChatTitle.Font = new Font("Courier New", 11F, FontStyle.Bold);
            lblChatTitle.ForeColor = Color.Gold;
            lblChatTitle.Location = new Point(10, 8);
            lblChatTitle.Name = "lblChatTitle";
            lblChatTitle.Size = new Size(164, 22);
            lblChatTitle.TabIndex = 0;
            lblChatTitle.Text = "💬 GLOBAL CHAT";
            // 
            // JoinRoomForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.FromArgb(160, 82, 45);
            ClientSize = new Size(1378, 720);
            Controls.Add(pnlGlobalChat);
            Controls.Add(headerPanel);
            Controls.Add(lblTitle);
            Controls.Add(pnlSearch);
            Controls.Add(pnlRoomList);
            Controls.Add(pnlHelp);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "JoinRoomForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Join Room";
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            pnlSearch.ResumeLayout(false);
            pnlSearch.PerformLayout();
            pnlRoomList.ResumeLayout(false);
            pnlHelp.ResumeLayout(false);
            pnlGlobalChat.ResumeLayout(false);
            pnlChatInput.ResumeLayout(false);
            pnlChatInput.PerformLayout();
            pnlChatHeader.ResumeLayout(false);
            pnlChatHeader.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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