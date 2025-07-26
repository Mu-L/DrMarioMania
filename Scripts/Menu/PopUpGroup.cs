using Godot;
using System;

public partial class PopUpGroup : Control
{
    [Export] private Label titleLabel;
    [Export] private Label descLabel;
    [Export] private AudioStreamPlayer audioPlayer;
    [Export] private AnimationPlayer aniPlayer;
    [Export] private Button okButton;
    [Export] private BaseScreenManager screenMan;
    [Export] private ScrollContainer scrollContainer;

    // last focused node before opening the popup
    private Control lastFocusNode;

    private bool isOpen = false;
    public bool IsOpen { get { return isOpen; } }

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        if (screenMan != null)
            okButton.Pressed += () => screenMan.GoBack();
    }

    public void ShowPopUp(string title, string desc, bool alignDescToLeft)
    {
        scrollContainer.ScrollVertical = 0;
        titleLabel.Text = title;
        descLabel.Text = desc;

        descLabel.HorizontalAlignment = alignDescToLeft ? HorizontalAlignment.Left : HorizontalAlignment.Center;

        aniPlayer.Play("Show");
        audioPlayer.Play();

        isOpen = true;

        lastFocusNode = GetViewport().GuiGetFocusOwner();
        okButton.GrabFocus();
    }

    public void HidePopUp()
    {
        aniPlayer.Play("Hide");
        lastFocusNode.GrabFocus();

        isOpen = false;
    }
}
