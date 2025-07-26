using Godot;
using System;

public partial class BaseShootPowerUp : BasePowerUp
{
    [Export] protected float acceleration;
    [Export] protected float maxSpeed;
    
    protected float speed = 0;
}
