using Godot;
using System;
using System.Threading.Tasks;

public partial class NotificationBox : Control
{
    [Export] private AnimationPlayer aniPlayer;
    [Export] private AudioStreamPlayer audioPlayer;
    [Export] private Label label;

    ulong lastShowTime;

    public async void ShowMessage(string text, int milliseconds = 2000)
    {
        ulong showTime = Time.GetTicksMsec();
        lastShowTime = showTime;
        
        label.Text = text;
        audioPlayer.Play(0);

        aniPlayer.Stop();
        aniPlayer.Play("Show");

        await Task.Delay(milliseconds);

        if (lastShowTime == showTime)
            aniPlayer.Play("Hide");
    }
}
