using Godot;
using System;

public partial class JarGroup : Node2D
{
    [Export] private JarManager jarMan;
    public JarManager JarMan { get { return jarMan; } }
}
