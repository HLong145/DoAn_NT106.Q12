using System;
using System.Drawing;
using System.Windows.Forms;

namespace PixelGameLobby
{
    partial class JoinRoomForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel headerPanel;
        private Label lblRoomName;
        private Label lblPlayers;
        private Label lblLock;
        private Label lblAction;
        private Label lblTitle;
        private Panel pnlSearch;
        private TextBox txtRoomCode;
        private TextBox txtPassword;
        private Button btnSearchJoin;
        private Button btnCreateRoom;
        private Button btnBack;
        private Panel pnlRoomList;
        private FlowLayoutPanel roomsPanel;
        private Panel roomPanel1;
        private Panel roomPanel2;
        private Panel roomPanel3;
        private Panel pnlHelp;
        private Label lblHelp;
        private Panel pnlGlobalChat;
        private Panel pnlChatHeader;
        private Panel pnlChatMessages;
        private Panel pnlChatInput;
        private Label lblChatTitle;
        private Label lblOnlineCount;
        private TextBox txtChatInput;
        private Button btnSendChat;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            headerPanel = new Panel();
            lblRoomName = new Label();
            lblPlayers = new Label();
            lblLock = new Label();
            lblAction = new Label();
            lblTitle = new Label();
            pnlSearch = new Panel();
            lb_pass = new Label();
            lb_roomcode = new Label();
            txtRoomCode = new TextBox();
            txtPassword = new TextBox();
            btnSearchJoin = new Button();
            btnCreateRoom = new Button();
            btnBack = new Button();
            pnlRoomList = new Panel();
            roomsPanel = new FlowLayoutPanel();
            roomPanel1 = new Panel();
            roomPanel2 = new Panel();
            roomPanel3 = new Panel();
            pnlHelp = new Panel();
            btn_refresh = new Button();
            lblHelp = new Label();
            headerPanel.SuspendLayout();
            pnlSearch.SuspendLayout();
            pnlRoomList.SuspendLayout();
            pnlHelp.SuspendLayout();
            SuspendLayout();
            // 
            // headerPanel
            // 
            headerPanel.BackColor = Color.FromArgb(74, 50, 25);
            headerPanel.Controls.Add(lblRoomName);
            headerPanel.Controls.Add(lblPlayers);
            headerPanel.Controls.Add(lblLock);
            headerPanel.Controls.Add(lblAction);
            headerPanel.Location = new Point(100, 152);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(1170, 54);
            headerPanel.TabIndex = 0;
            // 
            // lblRoomName
            // 
            lblRoomName.AutoSize = true;
            lblRoomName.Font = new Font("Courier New", 12F, FontStyle.Bold);
            lblRoomName.ForeColor = Color.Gold;
            lblRoomName.Location = new Point(40, 15);
            lblRoomName.Name = "lblRoomName";
            lblRoomName.Size = new Size(118, 23);
            lblRoomName.TabIndex = 0;
            lblRoomName.Text = "ROOM NAME";
            // 
            // lblPlayers
            // 
            lblPlayers.AutoSize = true;
            lblPlayers.Font = new Font("Courier New", 12F, FontStyle.Bold);
            lblPlayers.ForeColor = Color.Gold;
            lblPlayers.Location = new Point(293, 15);
            lblPlayers.Name = "lblPlayers";
            lblPlayers.Size = new Size(94, 23);
            lblPlayers.TabIndex = 1;
            lblPlayers.Text = "PLAYERS";
            // 
            // lblLock
            // 
            lblLock.AutoSize = true;
            lblLock.Font = new Font("Courier New", 12F, FontStyle.Bold);
            lblLock.ForeColor = Color.Gold;
            lblLock.Location = new Point(956, 13);
            lblLock.Name = "lblLock";
            lblLock.Size = new Size(58, 23);
            lblLock.TabIndex = 2;
            lblLock.Text = "LOCK";
            // 
            // lblAction
            // 
            lblAction.AutoSize = true;
            lblAction.Font = new Font("Courier New", 12F, FontStyle.Bold);
            lblAction.ForeColor = Color.Gold;
            lblAction.Location = new Point(1033, 13);
            lblAction.Name = "lblAction";
            lblAction.Size = new Size(82, 23);
            lblAction.TabIndex = 3;
            lblAction.Text = "ACTION";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Courier New", 28.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.Gold;
            lblTitle.Location = new Point(543, 1);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(275, 53);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "ROOM LIST";
            // 
            // pnlSearch
            // 
            pnlSearch.BackColor = Color.FromArgb(101, 67, 51);
            pnlSearch.Controls.Add(lb_pass);
            pnlSearch.Controls.Add(lb_roomcode);
            pnlSearch.Controls.Add(txtRoomCode);
            pnlSearch.Controls.Add(txtPassword);
            pnlSearch.Controls.Add(btnSearchJoin);
            pnlSearch.Controls.Add(btnCreateRoom);
            pnlSearch.Controls.Add(btnBack);
            pnlSearch.Location = new Point(100, 72);
            pnlSearch.Name = "pnlSearch";
            pnlSearch.Size = new Size(1170, 83);
            pnlSearch.TabIndex = 2;
            // 
            // lb_pass
            // 
            lb_pass.AutoSize = true;
            lb_pass.Font = new Font("Courier New", 12F, FontStyle.Bold);
            lb_pass.ForeColor = Color.Gold;
            lb_pass.Location = new Point(319, 13);
            lb_pass.Name = "lb_pass";
            lb_pass.Size = new Size(238, 23);
            lb_pass.TabIndex = 6;
            lb_pass.Text = "PASSWORD (OPTIONAL)";
            // 
            // lb_roomcode
            // 
            lb_roomcode.AutoSize = true;
            lb_roomcode.Font = new Font("Courier New", 12F, FontStyle.Bold);
            lb_roomcode.ForeColor = Color.Gold;
            lb_roomcode.Location = new Point(21, 13);
            lb_roomcode.Name = "lb_roomcode";
            lb_roomcode.Size = new Size(130, 23);
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
            btnSearchJoin.Location = new Point(634, 39);
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
            btnCreateRoom.Location = new Point(800, 38);
            btnCreateRoom.Name = "btnCreateRoom";
            btnCreateRoom.Size = new Size(180, 35);
            btnCreateRoom.TabIndex = 3;
            btnCreateRoom.Text = "CREATE ROOM";
            btnCreateRoom.UseVisualStyleBackColor = false;
            // 
            // btnBack
            // 
            btnBack.BackColor = Color.FromArgb(178, 34, 34);
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btnBack.ForeColor = Color.White;
            btnBack.Location = new Point(986, 38);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(140, 35);
            btnBack.TabIndex = 4;
            btnBack.Text = "BACK";
            btnBack.UseVisualStyleBackColor = false;
            btnBack.Click += btnBack_Click;
            // 
            // pnlRoomList
            // 
            pnlRoomList.BackColor = Color.Transparent;
            pnlRoomList.Controls.Add(roomsPanel);
            pnlRoomList.Location = new Point(100, 200);
            pnlRoomList.Name = "pnlRoomList";
            pnlRoomList.Size = new Size(1170, 400);
            pnlRoomList.TabIndex = 3;
            // 
            // roomsPanel
            // 
            roomsPanel.AutoScroll = true;
            roomsPanel.AutoScrollMargin = new Size(0, 10);
            roomsPanel.BackColor = Color.FromArgb(101, 67, 51);
            roomsPanel.Dock = DockStyle.Fill;
            roomsPanel.FlowDirection = FlowDirection.TopDown;
            roomsPanel.Location = new Point(0, 0);
            roomsPanel.Margin = new Padding(0);
            roomsPanel.Name = "roomsPanel";
            roomsPanel.Size = new Size(1170, 400);
            roomsPanel.TabIndex = 0;
            roomsPanel.WrapContents = false;
            // 
            // roomPanel1
            // 
            roomPanel1.Location = new Point(3, 3);
            roomPanel1.Name = "roomPanel1";
            roomPanel1.Size = new Size(200, 100);
            roomPanel1.TabIndex = 0;
            // 
            // roomPanel2
            // 
            roomPanel2.Location = new Point(3, 109);
            roomPanel2.Name = "roomPanel2";
            roomPanel2.Size = new Size(200, 100);
            roomPanel2.TabIndex = 1;
            // 
            // roomPanel3
            // 
            roomPanel3.Location = new Point(3, 215);
            roomPanel3.Name = "roomPanel3";
            roomPanel3.Size = new Size(200, 100);
            roomPanel3.TabIndex = 2;
            // 
            // pnlHelp
            // 
            pnlHelp.BackColor = Color.FromArgb(74, 50, 25);
            pnlHelp.BorderStyle = BorderStyle.FixedSingle;
            pnlHelp.Controls.Add(btn_refresh);
            pnlHelp.Controls.Add(lblHelp);
            pnlHelp.Location = new Point(100, 620);
            pnlHelp.Name = "pnlHelp";
            pnlHelp.Size = new Size(1170, 60);
            pnlHelp.TabIndex = 4;
            // 
            // btn_refresh
            // 
            btn_refresh.BackColor = Color.DarkOrchid;
            btn_refresh.FlatStyle = FlatStyle.Flat;
            btn_refresh.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btn_refresh.ForeColor = Color.White;
            btn_refresh.Location = new Point(985, 13);
            btn_refresh.Name = "btn_refresh";
            btn_refresh.Size = new Size(152, 35);
            btn_refresh.TabIndex = 3;
            btn_refresh.Text = "REFRESH";
            btn_refresh.UseVisualStyleBackColor = false;
            btn_refresh.Click += btn_refresh_Click;
            // 
            // lblHelp
            // 
            lblHelp.AutoSize = true;
            lblHelp.Font = new Font("Courier New", 11F, FontStyle.Bold);
            lblHelp.ForeColor = Color.Gold;
            lblHelp.Location = new Point(20, 20);
            lblHelp.Name = "lblHelp";
            lblHelp.Size = new Size(549, 22);
            lblHelp.TabIndex = 0;
            lblHelp.Text = "💡 Enter room code and password (if any) to join.";
            // 
            // JoinRoomForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.FromArgb(160, 82, 45);
            ClientSize = new Size(1378, 720);
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
            pnlHelp.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private void InitializeGlobalChatComponents()
        {
            this.pnlGlobalChat = new Panel();
            this.pnlChatHeader = new Panel();
            this.pnlChatMessages = new Panel();
            this.pnlChatInput = new Panel();
            this.lblChatTitle = new Label();
            this.lblOnlineCount = new Label();
            this.txtChatInput = new TextBox();
            this.btnSendChat = new Button();

            // 
            // pnlGlobalChat - Panel chính
            // 
            this.pnlGlobalChat.BackColor = Color.FromArgb(74, 50, 25);
            this.pnlGlobalChat.BorderStyle = BorderStyle.FixedSingle;
            this.pnlGlobalChat.Controls.Add(this.pnlChatInput);
            this.pnlGlobalChat.Controls.Add(this.pnlChatMessages);
            this.pnlGlobalChat.Controls.Add(this.pnlChatHeader);
            this.pnlGlobalChat.Location = new Point(1080, 110);
            this.pnlGlobalChat.Name = "pnlGlobalChat";
            this.pnlGlobalChat.Size = new Size(280, 500);
            this.pnlGlobalChat.TabIndex = 10;

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
            this.lblChatTitle.Size = new Size(142, 21);
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
            this.lblOnlineCount.Size = new Size(89, 18);
            this.lblOnlineCount.TabIndex = 1;
            this.lblOnlineCount.Text = "🟢 0 online";

            // 
            // pnlChatMessages
            // 
            this.pnlChatMessages.AutoScroll = true;
            this.pnlChatMessages.BackColor = Color.FromArgb(74, 50, 25);
            this.pnlChatMessages.Location = new Point(0, 50);
            this.pnlChatMessages.Name = "pnlChatMessages";
            this.pnlChatMessages.Size = new Size(278, 392);
            this.pnlChatMessages.TabIndex = 1;

            // 
            // pnlChatInput
            // 
            this.pnlChatInput.BackColor = Color.FromArgb(101, 67, 51);
            this.pnlChatInput.Controls.Add(this.btnSendChat);
            this.pnlChatInput.Controls.Add(this.txtChatInput);
            this.pnlChatInput.Dock = DockStyle.Bottom;
            this.pnlChatInput.Location = new Point(0, 442);
            this.pnlChatInput.Name = "pnlChatInput";
            this.pnlChatInput.Size = new Size(278, 56);
            this.pnlChatInput.TabIndex = 2;

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
            this.txtChatInput.KeyPress += new KeyPressEventHandler(this.TxtChatInput_KeyPress);

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
            this.btnSendChat.Click += new EventHandler(this.BtnSendChat_Click);
            this.btnSendChat.Paint += new PaintEventHandler(this.Button_Paint);

            // 
            // Thêm pnlGlobalChat vào Form
            // 
            this.Controls.Add(this.pnlGlobalChat);

            // ✅ Điều chỉnh layout các panel khác
            // Thu nhỏ để nhường chỗ cho Global Chat
            this.pnlRoomList.Size = new Size(960, 430);
            this.pnlSearch.Size = new Size(960, 80);
            this.pnlHelp.Size = new Size(960, 60);
        }

        // ===== UTILITY =====
        private void SetupRoomPanel(Panel panel, string roomName, string players, string lockIcon, string action, Color btnColor)
        {
            var lblName = new Label();
            var lblPlayers = new Label();
            var lblLock = new Label();
            var btnJoin = new Button();

            panel.BackColor = Color.FromArgb(101, 67, 51);
            panel.BorderStyle = BorderStyle.None;
            panel.Size = new Size(1130, 60);
            panel.Margin = new Padding(20, 8, 20, 8);


            lblName.AutoSize = true;
            lblName.Font = new Font("Courier New", 12F, FontStyle.Bold);
            lblName.ForeColor = Color.Gold;
            lblName.Location = new Point(20, 18);
            lblName.Text = roomName;

            lblPlayers.AutoSize = true;
            lblPlayers.Font = new Font("Courier New", 12F, FontStyle.Bold);
            lblPlayers.ForeColor = Color.Gold;
            lblPlayers.Location = new Point(320, 18);
            lblPlayers.Text = players;

            lblLock.AutoSize = true;
            lblLock.Font = new Font("Arial", 16F);
            lblLock.Location = new Point(850, 14);
            lblLock.Text = lockIcon;

            btnJoin.BackColor = btnColor;
            btnJoin.FlatStyle = FlatStyle.Flat;
            btnJoin.Font = new Font("Courier New", 12F, FontStyle.Bold);
            btnJoin.ForeColor = Color.White;
            btnJoin.Location = new Point(980, 10);
            btnJoin.Size = new Size(120, 40);
            btnJoin.Text = action;

            panel.Controls.Add(lblName);
            panel.Controls.Add(lblPlayers);
            panel.Controls.Add(lblLock);
            panel.Controls.Add(btnJoin);
        }

        private Label lb_pass;
        private Label lb_roomcode;
        private Button btn_refresh;
    }
}
