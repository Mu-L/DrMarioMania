using Godot;
using System;

public partial class VirusRingVirus : Sprite2D
{
    [Export] private VirusRing virusRing;
    public int FrameX
    {
        get
        {
            return Frame % Hframes;
        }
        set
        {
            Frame = value + Hframes * (colour - 1);
        }
    }

    private int colour = 1;
    public int Colour
    {
        get
        {
            return colour;
        }

        set
        {
            colour = value;
            // setting frame Y based of colour/value
            Frame = (Frame % Hframes) + Hframes * (value - 1);
        }
    }
    private Texture2D virusTexture;
    public Texture2D VirusTexture
    {
        set
        {
            virusTexture = value;
            Texture = value;
        }
        get
        {
            return virusTexture;
        }
    }
    private Texture2D vanishTexture;
    public Texture2D VanishTexture
    {
        set
        {
            vanishTexture = value;
        }
        get
        {
            return vanishTexture;
        }
    }

    private bool isStunned = false;
    public bool IsStunned { get { return isStunned; } }
    private bool isDefeated = false;
    public bool IsDefeated { get { return isDefeated; } }
    private bool landed = true;
    private float stunTimer;
    private float restunTimer;
    private float aniTimer;
    private float stunPosY;
    private const float stunDuration = 3.0f;
    // Duration before the stun animation can be played again
    private const float restunDuration = 0.5f;
    private const float aniFlinchDuration = 0.05f;
    private const float aniVanishDuration = 0.1f;
    private const float stunVelY = -200.0f;
    private const float stunGravity = 1700.0f;
    private float unroundedPosY;
    private float velY;

    private int origHframes = 0;
    private int origVframes = 0;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        origHframes = Hframes;
        origVframes = Vframes;
        SetProcess(false);
    }

    public void ResetState()
    {
        SetProcess(false);

        landed = true;
        isStunned = false;
        isDefeated = false;

        if (origHframes != 0)
        {
            Hframes = origHframes;
            Vframes = origVframes;
        }

        Texture = virusTexture;
    }
    
    // stuns virus, flipping it onto its back
    // there's a cooldown for the animation (restunTimer)
    public void Stun(bool defeat)
    {
        if (defeat)
            isDefeated = defeat;

        if (isStunned)
        {
            if (restunTimer > 0)
                return;
        }

        if (landed)
        {
            stunPosY = Position.Y;
            unroundedPosY = stunPosY;
        }

        velY = stunVelY;
        FrameX = 4;

        isStunned = true;
        landed = false;

        stunTimer = stunDuration;
        aniTimer = 0;
        restunTimer = restunDuration;

        SetProcess(true);
    }

    public void Unstun()
    {
        Position = new Vector2(Position.X, stunPosY);
        restunTimer = 0;
        isStunned = false;
        virusRing.DecrementStunnedVirusColours();


        SetProcess(false);

        if (isDefeated)
            Visible = false;
        else
            FrameX = virusRing.SpriteFrame;
    }

    private void Vanish()
    {
        // change into vanish texture
        Texture = vanishTexture;
        aniTimer = aniVanishDuration;

        Frame = 0;
        Hframes = 1;
        Vframes = 1;

        virusRing.SfxMan.Play("VirusVanish");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        // vanish texture disappear
        if (Texture == vanishTexture)
        {
            aniTimer -= (float)delta;

            if (aniTimer <= 0)
            {
                Unstun();
            }
            return;
        }

        // decrease stunTimer
        stunTimer -= (float)delta;

        // decrease restun timer until zero
        if (restunTimer > 0)
            restunTimer -= (float)delta;

        // unstun when stunTimer reaches zero
        if (stunTimer <= 0)
        {
            if (isDefeated)
                Vanish();
            else
                Unstun();
            return;
        }

        // if landed, do flinch animation
        if (landed)
        {
            aniTimer -= (float)delta;

            if (aniTimer <= 0)
            {
                aniTimer += aniFlinchDuration;
                FrameX = FrameX == 4 ? 5 : 4;
            }
        }
        // if not landed, apply y velocity and gravity
        else
        {
            unroundedPosY += velY * (float)delta;
            Position = new Vector2(Position.X, (int)unroundedPosY);

            velY += stunGravity * (float)delta;
            
            if (velY >= 0 && unroundedPosY >= stunPosY)
            {
                landed = true;
                virusRing.SfxMan.Play("VirusStunLand");
                Position = new Vector2(Position.X, stunPosY);
            }
        }
    }
}
