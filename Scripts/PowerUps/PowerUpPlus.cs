using Godot;

public partial class PowerUpPlus : BaseShootPowerUp
{
    [Export] private Godot.Collections.Array<Sprite2D> projectiles;
    private Vector2[] projectileDirections = { Vector2.Left, Vector2.Right, Vector2.Up, Vector2.Down };
    protected float[] projectileEndPositions;
    protected Vector2I[] gridPositions = new Vector2I[4];
    protected Vector2I[] lastGridPositions = new Vector2I[4];
    protected bool[] finishedProjectiles = { false, false, false, false };
    public override Texture2D Texture
    {
        set
        {
            sprite.Texture = value;

            foreach (Sprite2D proj in projectiles)
            {
                proj.Texture = value;
            }
        }
    }

    private int remainingProjectiles = 0;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        for (int i = 0; i < projectiles.Count; i++)
        {
		    lastGridPositions[i] = InitialGridPos;
            projectiles[i].Material = sprite.Material;
            projectiles[i].Frame = sprite.Frame;
        }

        sprite.Visible = false;

        projectileEndPositions = new float[4]{ jarMan.JarLeftPos, jarMan.JarRightPos, jarMan.JarTopPos, jarMan.JarBottomPos };
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        speed += acceleration * (float)delta;
        if (speed > maxSpeed)
            speed = maxSpeed;

        remainingProjectiles = 0;

        for (int i = 0; i < projectiles.Count; i++)
        {
            if (finishedProjectiles[i])
                continue;

            remainingProjectiles++;
            
            projectiles[i].Position += projectileDirections[i] * speed * (float)delta;
            gridPositions[i] = WorldPosToGridPos(projectiles[i].GlobalPosition);

            if (gridPositions[i] != lastGridPositions[i])
            {
                if (DestroyTilesBetweenPositions(lastGridPositions[i], gridPositions[i], true))
                {
                    sfxMan.Play("SingleHit");
                }
            }
            
            lastGridPositions[i] = gridPositions[i];

            float endPos = projectileEndPositions[i];

            if (projectileDirections[i].Y == 0)
            {
                float x = projectiles[i].GlobalPosition.X;

                if (projectileDirections[i].X < 0)
                {
                    if (x <= endPos)
                        finishedProjectiles[i] = true;
                }
                else
                {
                    if (x >= endPos)
                        finishedProjectiles[i] = true;
                }
            }
            else
            {
                float y = projectiles[i].GlobalPosition.Y;

                if (projectileDirections[i].Y < 0)
                {
                    if (y <= endPos)
                        finishedProjectiles[i] = true;
                }
                else
                {
                    if (y >= endPos)
                        finishedProjectiles[i] = true;
                }
            }

            if (finishedProjectiles[i])
                projectiles[i].Visible = false;
        }

        if (remainingProjectiles == 0)
        {
            FinishPowerUp();
        }
        
    }
}
