using Godot;
using System;
using static PowerUpEnums;

public static class PowerUpConstants
{
    // 1 in # chance of being a special (rainbow) power-up
    public static int specialChance = 5;

    // power-ups available in the default items gameplay style
    public static readonly PowerUp[] itemStylePowerUps = { PowerUp.Thwomp, PowerUp.Shell, PowerUp.Plus, PowerUp.Bomb, PowerUp.PillBlaster, PowerUp.VirusBlaster, PowerUp.PushDown };
    public static readonly PowerUp[] itemStyleSpecialPowerUps = { PowerUp.Thwomp, PowerUp.Shell, PowerUp.Plus, PowerUp.Bomb, PowerUp.PillBlaster, PowerUp.PushDown };
}
