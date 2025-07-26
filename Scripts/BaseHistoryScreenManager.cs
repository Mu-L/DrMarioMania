using Godot;
using System;
using System.Collections.Generic;

public partial class BaseHistoryScreenManager : BaseScreenManager
{
    protected List<int> screenHistory = new List<int>();

    protected int prevScreen = 0;

    public override void SetScreen(int nextScreen)
	{
        screenHistory.Add(nextScreen);

		base.SetScreen(nextScreen);
	}

    protected void PopHistory()
    {
        if (screenHistory.Count != 0)
		    screenHistory.RemoveAt(screenHistory.Count - 1);

        if (screenHistory.Count > 0)
            prevScreen = screenHistory[screenHistory.Count - 1];
    }
}
