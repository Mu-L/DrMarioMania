using Godot;
using System;

public partial class JarMario : Sprite2D
{
    [Export] private AnimationPlayer aniPlayer;

    public void ResetFrame()
    {
        if (aniPlayer.IsPlaying())
            aniPlayer.Stop();
        
        Frame = 0;
    }

    public void PlayAnimation(StringName ani)
    {
        if (aniPlayer.IsPlaying())
            aniPlayer.Stop();
        
        aniPlayer.Play(ani);
    }
}
