using Godot;
using System;

public partial class BaseScreen : Control
{
    // Base class for a screen
    [Export] public Control InitialHoverNode { get; set; }
    public Control LastHoverNode { get; set; } = null;

    public void ResetLastHoverNode()
    {
        LastHoverNode = null;
    }
}
