using System;
using System.Text.Json;

namespace PixelGameLobby
{
    public partial class GameLobbyForm
    {
        // This method should be called when BOTH_CHARACTERS_READY broadcast arrives.
        private void HandleBothCharactersReady(JsonElement data)
        {
            try
            {
                string msgRoomCode = GetStringOrNull(data, "roomCode");
                if (!string.IsNullOrEmpty(msgRoomCode) && msgRoomCode != roomCode)
                    return;

                Console.WriteLine("[GameLobby] BOTH_CHARACTERS_READY received");

                // If current user is host (Player1), open CharacterSelectForm so host can confirm and then open battle.
                if (isHost)
                {
                    // Mark flags to prevent confirmation dialog
                    hasLeft = true;
                    isLeaving = true;

                    // Determine opponent name if included
                    string p1Char = data.TryGetProperty("player1Character", out var p1El) ? (p1El.GetString() ?? "") : "";
                    string p2Char = data.TryGetProperty("player2Character", out var p2El) ? (p2El.GetString() ?? "") : "";

                    // Pass player number = 1 for host
                    string opponent = opponentName ?? "Opponent";

                    var selectForm = new DoAn_NT106.CharacterSelectForm(username, token, roomCode, opponent, isHost: true, selectedMap, 1);

                    selectForm.FormClosed += (s, args) =>
                    {
                        if (selectForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                        {
                            hasLeft = false;
                            isLeaving = false;
                            this.Show();
                        }
                        else
                        {
                            this.Close();
                        }
                    };

                    selectForm.Show();
                    this.Hide();
                }
                else
                {
                    // If not host, do nothing - Player2 already in CharacterSelectForm
                    Console.WriteLine("[GameLobby] BOTH_CHARACTERS_READY - non-host, ignoring");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLobby] HandleBothCharactersReady error: {ex.Message}");
            }
        }
    }
}
