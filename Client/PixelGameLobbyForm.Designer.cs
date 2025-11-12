namespace PixelGameLobby
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
            player2Panel = new Panel();
            player1NameLabel = new Label();
            player1StatusLabel = new Label();
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

            SuspendLayout();

            // 
            // playersPanel
            // 
            playersPanel.BackColor = Color.FromArgb(160, 82, 45);
            playersPanel.Location = new Point(14, 16);
            playersPanel.Size = new Size(251, 360);
            playersPanel.Controls.Add(player1Panel);
            playersPanel.Controls.Add(player2Panel);
            playersPanel.Controls.Add(roomCodePanel);
            playersPanel.Controls.Add(copyCodeButton);
            playersPanel.Paint += Panel_Paint;

            // 
            // player1Panel
            // 
            player1Panel.BackColor = Color.FromArgb(101, 67, 51);
            player1Panel.BorderStyle = BorderStyle.FixedSingle;
            player1Panel.Location = new Point(10, 15);
            player1Panel.Size = new Size(230, 45);
            player1Panel.Controls.Add(player1NameLabel);
            player1Panel.Controls.Add(player1StatusLabel);

            // 
            // player1NameLabel
            // 
            player1NameLabel.Text = "Player 1";
            player1NameLabel.ForeColor = Color.Gold;
            player1NameLabel.Font = new Font("Courier New", 11, FontStyle.Bold);
            player1NameLabel.Location = new Point(5, 10);
            player1NameLabel.Size = new Size(120, 25);

            // 
            // player1StatusLabel
            // 
            player1StatusLabel.Text = "#Ready";
            player1StatusLabel.ForeColor = Color.LimeGreen;
            player1StatusLabel.Font = new Font("Courier New", 11, FontStyle.Bold);
            player1StatusLabel.Location = new Point(130, 10);
            player1StatusLabel.Size = new Size(90, 25);
            player1StatusLabel.TextAlign = ContentAlignment.MiddleRight;

            // 
            // player2Panel
            // 
            player2Panel.BackColor = Color.FromArgb(101, 67, 51);
            player2Panel.BorderStyle = BorderStyle.FixedSingle;
            player2Panel.Location = new Point(10, 70);
            player2Panel.Size = new Size(230, 45);
            player2Panel.Controls.Add(player2NameLabel);
            player2Panel.Controls.Add(player2StatusLabel);

            // 
            // player2NameLabel
            // 
            player2NameLabel.Text = "Player 2";
            player2NameLabel.ForeColor = Color.Gold;
            player2NameLabel.Font = new Font("Courier New", 11, FontStyle.Bold);
            player2NameLabel.Location = new Point(5, 10);
            player2NameLabel.Size = new Size(120, 25);

            // 
            // player2StatusLabel
            // 
            player2StatusLabel.Text = "#Not Ready";
            player2StatusLabel.ForeColor = Color.Red;
            player2StatusLabel.Font = new Font("Courier New", 11, FontStyle.Bold);
            player2StatusLabel.Location = new Point(130, 10);
            player2StatusLabel.Size = new Size(90, 25);
            player2StatusLabel.TextAlign = ContentAlignment.MiddleRight;

            // 
            // roomCodePanel
            // 
            roomCodePanel.BackColor = Color.FromArgb(101, 67, 51);
            roomCodePanel.BorderStyle = BorderStyle.FixedSingle;
            roomCodePanel.Location = new Point(10, 130);
            roomCodePanel.Size = new Size(230, 70);
            roomCodePanel.Controls.Add(roomCodeTitleLabel);
            roomCodePanel.Controls.Add(roomCodeValueLabel);
            roomCodePanel.Paint += Panel_Paint;

            // 
            // roomCodeTitleLabel
            // 
            roomCodeTitleLabel.Text = "ROOM CODE";
            roomCodeTitleLabel.ForeColor = Color.Gold;
            roomCodeTitleLabel.Font = new Font("Courier New", 10, FontStyle.Bold);
            roomCodeTitleLabel.Location = new Point(5, 5);
            roomCodeTitleLabel.Size = new Size(220, 25);
            roomCodeTitleLabel.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // roomCodeValueLabel
            // 
            roomCodeValueLabel.Text = "ABC123";
            roomCodeValueLabel.ForeColor = Color.White;
            roomCodeValueLabel.BackColor = Color.FromArgb(80, 60, 40);
            roomCodeValueLabel.Font = new Font("Courier New", 14, FontStyle.Bold);
            roomCodeValueLabel.Location = new Point(5, 35);
            roomCodeValueLabel.Size = new Size(220, 25);
            roomCodeValueLabel.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // copyCodeButton
            // 
            copyCodeButton.Text = "COPY CODE";
            copyCodeButton.BackColor = Color.FromArgb(139, 69, 19);
            copyCodeButton.FlatStyle = FlatStyle.Flat;
            copyCodeButton.Font = new Font("Courier New", 12, FontStyle.Bold);
            copyCodeButton.ForeColor = Color.Gold;
            copyCodeButton.Location = new Point(10, 210);
            copyCodeButton.Size = new Size(230, 35);
            copyCodeButton.Click += CopyCodeButton_Click;
            copyCodeButton.Paint += Button_Paint;

            // 
            // chatPanel
            // 
            chatPanel.BackColor = Color.FromArgb(160, 82, 45);
            chatPanel.Location = new Point(286, 16);
            chatPanel.Size = new Size(400, 533);
            chatPanel.Controls.Add(chatMessagesPanel);
            chatPanel.Controls.Add(chatTitleLabel);
            chatPanel.Controls.Add(sendButton);
            chatPanel.Controls.Add(messageTextBox);
            chatPanel.Paint += Panel_Paint;

            // 
            // chatMessagesPanel
            // 
            chatMessagesPanel.AutoScroll = true;
            chatMessagesPanel.BackColor = Color.FromArgb(101, 67, 51);
            chatMessagesPanel.Font = new Font("Segoe UI", 10.2F);
            chatMessagesPanel.Location = new Point(11, 60);
            chatMessagesPanel.Size = new Size(371, 400);
            chatMessagesPanel.Paint += Panel_Paint;

            // 
            // chatTitleLabel
            // 
            chatTitleLabel.Font = new Font("Courier New", 12F, FontStyle.Bold);
            chatTitleLabel.ForeColor = Color.Gold;
            chatTitleLabel.Location = new Point(11, 13);
            chatTitleLabel.Size = new Size(114, 33);
            chatTitleLabel.Text = "CHAT";

            // 
            // sendButton
            // 
            sendButton.BackColor = Color.FromArgb(139, 69, 19);
            sendButton.FlatStyle = FlatStyle.Flat;
            sendButton.Font = new Font("Courier New", 9F, FontStyle.Bold);
            sendButton.ForeColor = Color.Gold;
            sendButton.Location = new Point(309, 473);
            sendButton.Size = new Size(74, 33);
            sendButton.Text = "SEND";
            sendButton.Click += sendButton_Click;
            sendButton.Paint += Button_Paint;

            // 
            // messageTextBox
            // 
            messageTextBox.BackColor = Color.FromArgb(101, 67, 51);
            messageTextBox.Font = new Font("Courier New", 12F, FontStyle.Bold);
            messageTextBox.ForeColor = Color.Gold;
            messageTextBox.Location = new Point(11, 473);
            messageTextBox.PlaceholderText = "Type message...";
            messageTextBox.Size = new Size(285, 30);
            messageTextBox.KeyPress += messageTextBox_KeyPress;

            // 
            // controlsPanel
            // 
            controlsPanel.BackColor = Color.FromArgb(160, 82, 45);
            controlsPanel.Location = new Point(14, 384);
            controlsPanel.Size = new Size(251, 168);
            controlsPanel.Controls.Add(leaveRoomButton);
            controlsPanel.Controls.Add(startGameButton);
            controlsPanel.Controls.Add(notReadyButton);
            controlsPanel.Paint += Panel_Paint;

            // 
            // notReadyButton
            // 
            notReadyButton.BackColor = Color.FromArgb(139, 69, 19);
            notReadyButton.FlatStyle = FlatStyle.Flat;
            notReadyButton.Font = new Font("Goudy Stout", 10.8F, FontStyle.Bold);
            notReadyButton.ForeColor = Color.Gold;
            notReadyButton.Location = new Point(11, 18);
            notReadyButton.Size = new Size(229, 40);
            notReadyButton.Text = "NOT READY";
            notReadyButton.Click += notReadyButton_Click;
            notReadyButton.Paint += Button_Paint;

            // 
            // startGameButton
            // 
            startGameButton.BackColor = Color.FromArgb(139, 69, 19);
            startGameButton.FlatStyle = FlatStyle.Flat;
            startGameButton.Font = new Font("Goudy Stout", 10.8F, FontStyle.Bold);
            startGameButton.ForeColor = Color.Gold;
            startGameButton.Location = new Point(11, 66);
            startGameButton.Size = new Size(229, 40);
            startGameButton.Text = "START GAME";
            startGameButton.Click += startGameButton_Click;
            startGameButton.Paint += Button_Paint;

            // 
            // leaveRoomButton
            // 
            leaveRoomButton.BackColor = Color.FromArgb(139, 69, 19);
            leaveRoomButton.FlatStyle = FlatStyle.Flat;
            leaveRoomButton.Font = new Font("Goudy Stout", 10.8F, FontStyle.Bold);
            leaveRoomButton.ForeColor = Color.Gold;
            leaveRoomButton.Location = new Point(11, 114);
            leaveRoomButton.Size = new Size(229, 40);
            leaveRoomButton.Text = "LEAVE ROOM";
            leaveRoomButton.Click += leaveRoomButton_Click;
            leaveRoomButton.Paint += Button_Paint;

            // 
            // gameInfoPanel
            // 
            gameInfoPanel.BackColor = Color.FromArgb(160, 82, 45);
            gameInfoPanel.Location = new Point(14, 560);
            gameInfoPanel.Size = new Size(672, 67);
            gameInfoPanel.Controls.Add(timeLabel);
            gameInfoPanel.Controls.Add(nodeLabel);
            gameInfoPanel.Controls.Add(mapLabel);
            gameInfoPanel.Paint += Panel_Paint;

            // 
            // mapLabel / nodeLabel / timeLabel
            // 
            mapLabel.Font = new Font("Courier New", 9F, FontStyle.Bold);
            mapLabel.ForeColor = Color.Gold;
            mapLabel.Location = new Point(11, 20);
            mapLabel.Size = new Size(206, 27);
            mapLabel.Text = "Map: Forest Arena";

            nodeLabel.Font = new Font("Courier New", 9F, FontStyle.Bold);
            nodeLabel.ForeColor = Color.Gold;
            nodeLabel.Location = new Point(229, 20);
            nodeLabel.Size = new Size(206, 27);
            nodeLabel.Text = "Node: Battle Royale";

            timeLabel.Font = new Font("Courier New", 9F, FontStyle.Bold);
            timeLabel.ForeColor = Color.Gold;
            timeLabel.Location = new Point(457, 20);
            timeLabel.Size = new Size(206, 27);
            timeLabel.Text = "Time: 10:00";

            // 
            // GameLobbyForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(255, 192, 128);
            ClientSize = new Size(702, 641);
            Controls.Add(gameInfoPanel);
            Controls.Add(controlsPanel);
            Controls.Add(chatPanel);
            Controls.Add(playersPanel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GameLobbyForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Game Lobby";
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
    }
}
