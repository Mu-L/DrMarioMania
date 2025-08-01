using Godot;
using System;

public partial class GameSettingButtonInGroup : Button
{
	[Export] private int value;

	public int GetValue(){ return value; }
	public void SetValue(int v) { value = v; }
}
