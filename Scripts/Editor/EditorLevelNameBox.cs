using Godot;
using System;

public partial class EditorLevelNameBox : LineEdit
{
    [Export] private CommonGameSettings commonGameSettings;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Text = commonGameSettings.CustomLevelName;
    }

    public void UpdateLevelName(string newText)
    {
        int oldColumn = CaretColumn;
        int oldLength = newText.Length;

        foreach (char forbiddenChar in GameConstants.forbiddenLevelNameChars)
        {
            if (newText.Contains(forbiddenChar))
            {
                newText = newText.Replace(forbiddenChar.ToString(), "");
            }
        }

        int newLength = newText.Length;

        commonGameSettings.CustomLevelName = newText;

        if (oldLength != newLength)
        {
            Text = newText;
            CaretColumn = oldColumn;
        }
    }
}
