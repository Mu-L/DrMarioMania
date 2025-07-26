using Godot;
using System;
using System.Threading.Tasks;

public partial class BaseInstantPowerUp : BasePowerUp
{
    // "Instant" as in the power-up destroys tiles instantly without any delay due to an animation, unlike "shooter" power-ups (e.g. shells and thwomps)

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        sprite.Visible = false;
        DelayAndFinish();
	}

    private async void DelayAndFinish()
    {
        await Task.Delay(1000 / (int)jarMan.DestroyDisappearSpeed);
        
        if (IsInstanceValid(this))
            FinishPowerUp();
    }
}
