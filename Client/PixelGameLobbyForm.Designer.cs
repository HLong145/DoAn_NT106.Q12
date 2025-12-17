namespace DoAn_NT106.Client
{
    partial class GameLobbyForm
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
            playersPanel = new Panel();
            player1Panel = new Panel();
            player1NameLabel = new Label();
            player1StatusLabel = new Label();
            player2Panel = new Panel();
            player2NameLabel = new Label();
            player2StatusLabel = new Label();
            roomCodePanel = new Panel();
            roomCodeTitleLabel = new Label();
            roomCodeValueLabel = new Label();
            copyCodeButton = new Button();
            chatPanel = new Panel();
            chatMessagesPanel = new Panel();
            chatTitleLabel = new Label();
            sendButton = new Button();
            messageTextBox = new TextBox();
            controlsPanel = new Panel();
            leaveRoomButton = new Button();
            startGameButton = new Button();
            notReadyButton = new Button();
            gameInfoPanel = new Panel();
            timeLabel = new Label();
            nodeLabel = new Label();
            mapLabel = new Label();
            chooseMapButton = new Button();
            playersPanel.SuspendLayout();
            player1Panel.SuspendLayout();
            player2Panel.SuspendLayout();
            roomCodePanel.SuspendLayout();
            chatPanel.SuspendLayout();
            controlsPanel.SuspendLayout();
            gameInfoPanel.SuspendLayout();
            SuspendLayout();
            // 
            // playersPanel
            // 
            playersPanel.BackColor = Color.FromArgb(160, 82, 45);
            playersPanel.Controls.Add(player1Panel);
            playersPanel.Controls.Add(player2Panel);
            playersPanel.Controls.Add(roomCodePanel);
            playersPanel.Controls.Add(copyCodeButton);
            playersPanel.Location = new Point(12, 12);
            playersPanel.Margin = new Padding(3, 2, 3, 2);
            playersPanel.Name = "playersPanel";
            playersPanel.Size = new Size(220, 270);
            playersPanel.TabIndex = 0;
            playersPanel.Paint += Panel_Paint;
            // 
            // player1Panel
            // 
            player1Panel.BackColor = Color.FromArgb(101, 67, 51);
            player1Panel.BorderStyle = BorderStyle.FixedSingle;
            player1Panel.Controls.Add(player1NameLabel);
            player1Panel.Controls.Add(player1StatusLabel);
            player1Panel.Location = new Point(9, 11);
            player1Panel.Margin = new Padding(3, 2, 3, 2);
            player1Panel.Name = "player1Panel";
            player1Panel.Size = new Size(202, 34);
            player1Panel.TabIndex = 0;
            // 
            // player1NameLabel
            // 
            player1NameLabel.Font = new Font("Courier New", 11F, FontStyle.Bold);
            player1NameLabel.ForeColor = Color.Gold;
            player1NameLabel.Location = new Point(4, 8);
            player1NameLabel.Name = "player1NameLabel";
            player1NameLabel.Size = new Size(105, 19);
            player1NameLabel.TabIndex = 0;
            player1NameLabel.Text = "Player 1";
            // 
            // player1StatusLabel
            // 
            player1StatusLabel.Font = new Font("Courier New", 11F, FontStyle.Bold);
            player1StatusLabel.ForeColor = Color.LimeGreen;
            player1StatusLabel.Location = new Point(114, 8);
            player1StatusLabel.Name = "player1StatusLabel";
            player1StatusLabel.Size = new Size(79, 19);
            player1StatusLabel.TabIndex = 1;
            player1StatusLabel.Text = "#Ready";
            player1StatusLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // player2Panel
            // 
            player2Panel.BackColor = Color.FromArgb(101, 67, 51);
            player2Panel.BorderStyle = BorderStyle.FixedSingle;
            player2Panel.Controls.Add(player2NameLabel);
            player2Panel.Controls.Add(player2StatusLabel);
            player2Panel.Location = new Point(9, 52);
            player2Panel.Margin = new Padding(3, 2, 3, 2);
            player2Panel.Name = "player2Panel";
            player2Panel.Size = new Size(202, 34);
            player2Panel.TabIndex = 1;
            // 
            // player2NameLabel
            // 
            player2NameLabel.Font = new Font("Courier New", 11F, FontStyle.Bold);
            player2NameLabel.ForeColor = Color.Gold;
            player2NameLabel.Location = new Point(4, 8);
            player2NameLabel.Name = "player2NameLabel";
            player2NameLabel.Size = new Size(105, 19);
            player2NameLabel.TabIndex = 0;
            player2NameLabel.Text = "Player 2";
            // 
            // player2StatusLabel
            // 
            player2StatusLabel.Font = new Font("Courier New", 11F, FontStyle.Bold);
            player2StatusLabel.ForeColor = Color.Red;
            player2StatusLabel.Location = new Point(114, 8);
            player2StatusLabel.Name = "player2StatusLabel";
            player2StatusLabel.Size = new Size(79, 19);
            player2StatusLabel.TabIndex = 1;
            player2StatusLabel.Text = "#Not Ready";
            player2StatusLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // roomCodePanel
            // 
            roomCodePanel.BackColor = Color.FromArgb(101, 67, 51);
            roomCodePanel.BorderStyle = BorderStyle.FixedSingle;
            roomCodePanel.Controls.Add(roomCodeTitleLabel);
            roomCodePanel.Controls.Add(roomCodeValueLabel);
            roomCodePanel.Location = new Point(9, 98);
            roomCodePanel.Margin = new Padding(3, 2, 3, 2);
            roomCodePanel.Name = "roomCodePanel";
            roomCodePanel.Size = new Size(202, 53);
            roomCodePanel.TabIndex = 2;
            roomCodePanel.Paint += Panel_Paint;
            // 
            // roomCodeTitleLabel
            // 
            roomCodeTitleLabel.Font = new Font("Courier New", 10F, FontStyle.Bold);
            roomCodeTitleLabel.ForeColor = Color.Gold;
            roomCodeTitleLabel.Location = new Point(4, 4);
            roomCodeTitleLabel.Name = "roomCodeTitleLabel";
            roomCodeTitleLabel.Size = new Size(192, 19);
            roomCodeTitleLabel.TabIndex = 0;
            roomCodeTitleLabel.Text = "ROOM CODE";
            roomCodeTitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // roomCodeValueLabel
            // 
            roomCodeValueLabel.BackColor = Color.FromArgb(80, 60, 40);
            roomCodeValueLabel.Font = new Font("Courier New", 14F, FontStyle.Bold);
            roomCodeValueLabel.ForeColor = Color.White;
            roomCodeValueLabel.Location = new Point(4, 26);
            roomCodeValueLabel.Name = "roomCodeValueLabel";
            roomCodeValueLabel.Size = new Size(192, 19);
            roomCodeValueLabel.TabIndex = 1;
            roomCodeValueLabel.Text = "ABC123";
            roomCodeValueLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // copyCodeButton
            // 
            copyCodeButton.BackColor = Color.FromArgb(139, 69, 19);
            copyCodeButton.FlatStyle = FlatStyle.Flat;
            copyCodeButton.Font = new Font("Courier New", 12F, FontStyle.Bold);
            copyCodeButton.ForeColor = Color.Gold;
            copyCodeButton.Location = new Point(9, 163);
            copyCodeButton.Margin = new Padding(3, 2, 3, 2);
            copyCodeButton.Name = "copyCodeButton";
            copyCodeButton.Size = new Size(201, 26);
            copyCodeButton.TabIndex = 3;
            copyCodeButton.Text = "COPY CODE";
            copyCodeButton.UseVisualStyleBackColor = false;
            copyCodeButton.Click += CopyCodeButton_Click;
            copyCodeButton.Paint += Button_Paint;
            // 
            // chatPanel
            // 
            chatPanel.BackColor = Color.FromArgb(160, 82, 45);
            chatPanel.Controls.Add(chatMessagesPanel);
            chatPanel.Controls.Add(chatTitleLabel);
            chatPanel.Controls.Add(sendButton);
            chatPanel.Controls.Add(messageTextBox);
            chatPanel.Location = new Point(250, 12);
            chatPanel.Margin = new Padding(3, 2, 3, 2);
            chatPanel.Name = "chatPanel";
            chatPanel.Size = new Size(350, 400);
            chatPanel.TabIndex = 2;
            chatPanel.Paint += Panel_Paint;
            // 
            // chatMessagesPanel
            // 
            chatMessagesPanel.AutoScroll = true;
            chatMessagesPanel.BackColor = Color.FromArgb(101, 67, 51);
            chatMessagesPanel.Font = new Font("Segoe UI", 10.2F);
            chatMessagesPanel.Location = new Point(10, 45);
            chatMessagesPanel.Margin = new Padding(3, 2, 3, 2);
            chatMessagesPanel.Name = "chatMessagesPanel";
            chatMessagesPanel.Size = new Size(325, 300);
            chatMessagesPanel.TabIndex = 0;
            chatMessagesPanel.Paint += Panel_Paint;
            // 
            // chatTitleLabel
            // 
            chatTitleLabel.Font = new Font("Courier New", 12F, FontStyle.Bold);
            chatTitleLabel.ForeColor = Color.Gold;
            chatTitleLabel.Location = new Point(10, 10);
            chatTitleLabel.Name = "chatTitleLabel";
            chatTitleLabel.Size = new Size(100, 25);
            chatTitleLabel.TabIndex = 1;
            chatTitleLabel.Text = "CHAT";
            // 
            // sendButton
            // 
            sendButton.BackColor = Color.FromArgb(139, 69, 19);
            sendButton.FlatStyle = FlatStyle.Flat;
            sendButton.Font = new Font("Courier New", 9F, FontStyle.Bold);
            sendButton.ForeColor = Color.Gold;
            sendButton.Location = new Point(270, 355);
            sendButton.Margin = new Padding(3, 2, 3, 2);
            sendButton.Name = "sendButton";
            sendButton.Size = new Size(65, 25);
            sendButton.TabIndex = 2;
            sendButton.Text = "SEND";
            sendButton.UseVisualStyleBackColor = false;
            sendButton.Click += sendButton_Click;
            sendButton.Paint += Button_Paint;
            // 
            // messageTextBox
            // 
            messageTextBox.BackColor = Color.FromArgb(101, 67, 51);
            messageTextBox.Font = new Font("Courier New", 12F, FontStyle.Bold);
            messageTextBox.ForeColor = Color.Gold;
            messageTextBox.Location = new Point(10, 355);
            messageTextBox.Margin = new Padding(3, 2, 3, 2);
            messageTextBox.Name = "messageTextBox";
            messageTextBox.PlaceholderText = "Type message...";
            messageTextBox.Size = new Size(250, 26);
            messageTextBox.TabIndex = 1;
            messageTextBox.KeyPress += messageTextBox_KeyPress;
            // 
            // controlsPanel
            // 
            controlsPanel.BackColor = Color.FromArgb(160, 82, 45);
            controlsPanel.Controls.Add(leaveRoomButton);
            controlsPanel.Controls.Add(startGameButton);
            controlsPanel.Controls.Add(notReadyButton);
            controlsPanel.Location = new Point(12, 288);
            controlsPanel.Margin = new Padding(3, 2, 3, 2);
            controlsPanel.Name = "controlsPanel";
            controlsPanel.Size = new Size(220, 126);
            controlsPanel.TabIndex = 1;
            controlsPanel.Paint += Panel_Paint;
            // 
            // leaveRoomButton
            // 
            leaveRoomButton.BackColor = Color.FromArgb(139, 69, 19);
            leaveRoomButton.FlatStyle = FlatStyle.Flat;
            leaveRoomButton.Font = new Font("Goudy Stout", 10.8F, FontStyle.Bold);
            leaveRoomButton.ForeColor = Color.Gold;
            leaveRoomButton.Location = new Point(10, 86);
            leaveRoomButton.Margin = new Padding(3, 2, 3, 2);
            leaveRoomButton.Name = "leaveRoomButton";
            leaveRoomButton.Size = new Size(200, 30);
            leaveRoomButton.TabIndex = 2;
            leaveRoomButton.Text = "LEAVE ROOM";
            leaveRoomButton.UseVisualStyleBackColor = false;
            leaveRoomButton.Click += leaveRoomButton_Click;
            leaveRoomButton.Paint += Button_Paint;
            // 
            // startGameButton
            // 
            startGameButton.BackColor = Color.FromArgb(139, 69, 19);
            startGameButton.FlatStyle = FlatStyle.Flat;
            startGameButton.Font = new Font("Goudy Stout", 10.8F, FontStyle.Bold);
            startGameButton.ForeColor = Color.Gold;
            startGameButton.Location = new Point(10, 50);
            startGameButton.Margin = new Padding(3, 2, 3, 2);
            startGameButton.Name = "startGameButton";
            startGameButton.Size = new Size(200, 30);
            startGameButton.TabIndex = 1;
            startGameButton.Text = "START GAME";
            startGameButton.UseVisualStyleBackColor = false;
            startGameButton.Click += startGameButton_Click;
            startGameButton.Paint += Button_Paint;
            // 
            // notReadyButton
            // 
            notReadyButton.BackColor = Color.FromArgb(139, 69, 19);
            notReadyButton.FlatStyle = FlatStyle.Flat;
            notReadyButton.Font = new Font("Goudy Stout", 10.8F, FontStyle.Bold);
            notReadyButton.ForeColor = Color.Gold;
            notReadyButton.Location = new Point(10, 14);
            notReadyButton.Margin = new Padding(3, 2, 3, 2);
            notReadyButton.Name = "notReadyButton";
            notReadyButton.Size = new Size(200, 30);
            notReadyButton.TabIndex = 0;
            notReadyButton.Text = "NOT READY";
            notReadyButton.UseVisualStyleBackColor = false;
            notReadyButton.Click += notReadyButton_Click;
            notReadyButton.Paint += Button_Paint;
            notReadyButton.KeyDown += notReadyButton_KeyDown;
            // 
            // gameInfoPanel
            // 
            gameInfoPanel.BackColor = Color.FromArgb(160, 82, 45);
            gameInfoPanel.Controls.Add(timeLabel);
            gameInfoPanel.Controls.Add(nodeLabel);
            gameInfoPanel.Controls.Add(mapLabel);
            gameInfoPanel.Controls.Add(chooseMapButton);
            gameInfoPanel.Location = new Point(12, 420);
            gameInfoPanel.Margin = new Padding(3, 2, 3, 2);
            gameInfoPanel.Name = "gameInfoPanel";
            gameInfoPanel.Size = new Size(588, 80);  // ✅ TĂNG từ 50 → 80
            gameInfoPanel.TabIndex = 3;
            gameInfoPanel.Paint += Panel_Paint;
            // 
            // timeLabel
            // 
            timeLabel.Font = new Font("Courier New", 9F, FontStyle.Bold);
            timeLabel.ForeColor = Color.Gold;
            timeLabel.Location = new Point(400, 45);  // ✅ DỊCH xuống
            timeLabel.Name = "timeLabel";
            timeLabel.Size = new Size(180, 20);
            timeLabel.TabIndex = 0;
            timeLabel.Text = "Time: 10:00";
            // 
            // nodeLabel
            // 
            nodeLabel.Font = new Font("Courier New", 9F, FontStyle.Bold);
            nodeLabel.ForeColor = Color.Gold;
            nodeLabel.Location = new Point(200, 45);  // ✅ DỊCH xuống
            nodeLabel.Name = "nodeLabel";
            nodeLabel.Size = new Size(180, 20);
            nodeLabel.TabIndex = 1;
            nodeLabel.Text = "Node: Battle Royale";
            // 
            // mapLabel
            // 
            mapLabel.Font = new Font("Courier New", 9F, FontStyle.Bold);
            mapLabel.ForeColor = Color.Gold;
            mapLabel.Location = new Point(10, 45);  // ✅ DỊCH xuống
            mapLabel.Name = "mapLabel";
            mapLabel.Size = new Size(180, 20);
            mapLabel.TabIndex = 2;
            mapLabel.Text = "Map: Forest Arena";
            // 
            // chooseMapButton
            // 
            chooseMapButton.BackColor = Color.FromArgb(139, 69, 19);
            chooseMapButton.FlatStyle = FlatStyle.Flat;
            chooseMapButton.Font = new Font("Courier New", 8F, FontStyle.Bold);
            chooseMapButton.ForeColor = Color.Gold;
            chooseMapButton.Location = new Point(200, 12);
            chooseMapButton.Name = "chooseMapButton";
            chooseMapButton.Size = new Size(150, 26);
            chooseMapButton.TabIndex = 3;
            chooseMapButton.Text = "Choose Battleground";
            chooseMapButton.UseVisualStyleBackColor = false;
            chooseMapButton.Click += chooseMapButton_Click;
            chooseMapButton.Paint += Button_Paint;
            // 
            // GameLobbyForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(255, 192, 128);
            ClientSize = new Size(614, 481);
            Controls.Add(gameInfoPanel);
            Controls.Add(controlsPanel);
            Controls.Add(chatPanel);
            Controls.Add(playersPanel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GameLobbyForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Game Lobby";
            playersPanel.ResumeLayout(false);
            player1Panel.ResumeLayout(false);
            player2Panel.ResumeLayout(false);
            roomCodePanel.ResumeLayout(false);
            chatPanel.ResumeLayout(false);
            chatPanel.PerformLayout();
            controlsPanel.ResumeLayout(false);
            gameInfoPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Panel playersPanel;
        private Panel player1Panel;
        private Panel player2Panel;
        private Label player1NameLabel;
        private Label player1StatusLabel;
        private Label player2NameLabel;
        private Label player2StatusLabel;
        private Panel roomCodePanel;
        private Label roomCodeTitleLabel;
        private Label roomCodeValueLabel;
        private Button copyCodeButton;

        private Panel chatPanel;
        private Panel chatMessagesPanel;
        private Label chatTitleLabel;
        private TextBox messageTextBox;
        private Button sendButton;

        private Panel controlsPanel;
        private Button notReadyButton;
        private Button startGameButton;
        private Button leaveRoomButton;

        private Panel gameInfoPanel;
        private Label mapLabel;
        private Label nodeLabel;
        private Label timeLabel;
        private Button chooseMapButton;
    }
}
