using Godot;
using System;

public partial class Page : Control
{
    [Export] private string title;
    public string Title { get { return title; } }
}
