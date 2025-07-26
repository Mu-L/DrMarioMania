using Godot;
using Godot.Collections;

public partial class OfficialCustomLevelList : Resource
{
    // Stores of internally stored custom levels and cannot be modified (unless copied to the user's own custom level list)

    [Export] private Array<string> levels;
	public Array<string> Levels { get { return levels; } }

    public string GetLevelName(int id)
    {
        string[] codeSections = levels[id].Split('/');

        string[] basicSettingChunks = codeSections[0].Split(';');

        return basicSettingChunks[1];
    }
}