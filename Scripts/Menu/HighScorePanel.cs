using Godot;
using System;

public partial class HighScorePanel : Panel
{
    [Export] private HighScoreList highScoreList;
    [Export] private Label scoresLabel;
    [Export] private ScrollContainer scrollContainer;

    private const int listSize = 40;

    public void UpdateVisuals()
    {
        scrollContainer.ScrollVertical = 0;
        scoresLabel.Text = "";

        Godot.Collections.Array<int> scores = highScoreList.GameRuleHighScores;
        int scoreCount = scores == null ? 0 : scores.Count;

        for (int i = 0; i < listSize; i++)
        {
            scoresLabel.Text += (i + 1) + (i > 8 ? ". " : ".  ");
            scoresLabel.Text += i >= scoreCount ? "-" : scores[i];

            if (i != listSize - 1)
                scoresLabel.Text += "\n";
        }
    }
}
