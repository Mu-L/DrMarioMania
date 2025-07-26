using Godot;
using System;
using System.Collections.Generic;

public partial class VirusRing : Control
{
    [Export] private VirusRingVirus firstVirus;
    [Export] private Marker2D centreMarker;
    [Export] private Sprite2D ringBg;
    [Export] private Sprite2D ringOverlay;
    [Export] private TextureRect ringShadow;
    protected SfxManager sfxMan;
    public SfxManager SfxMan { set { sfxMan = value; } get { return sfxMan; } }
    public Sprite2D RingBg { get { return ringBg; } }
    public Sprite2D RingOverlay { get { return ringOverlay; } }
    public TextureRect RingShadow { get { return ringShadow; } }
    public Texture2D VirusTexture
    {
        set
        {
            if (viruses.Count == 0)
            {
                firstVirus.VirusTexture = value;
            }
            else
            {
                foreach (VirusRingVirus virus in viruses)
                {
                    virus.VirusTexture = value;
                }
            }
        }
    }
    public Texture2D VirusVanishTexture
    {
        set
        {
            if (viruses.Count == 0)
            {
                firstVirus.VanishTexture = value;
            }
            else
            {
                foreach (VirusRingVirus virus in viruses)
                {
                    virus.VanishTexture = value;
                }
            }
        }
    }
    private Vector2 CentrePos { get { return centreMarker.Position; } }
    private List<VirusRingVirus> viruses = new List<VirusRingVirus>();
    private float aniTimer = 0;
    private long stepMoveCount = 0;
    private long stepFrameCount = 0;
    private float stepDuration = 0.2f;
    private float laughDuration = 0.1f;
    private float stepDistance = 0.075f;
    private const float distanceFromCentre = 18;
    private int stunnedVirusColours = 0;
    private int spriteFrame;
    public int SpriteFrame { get { return spriteFrame; } }
    private bool isLaughing = false;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (viruses.Count == 0)
        {
            firstVirus.Visible = false;
            SetProcess(false);
        }
	}

    public void SetPauseState(bool pause)
    {
        SetProcess(!pause);

        foreach (VirusRingVirus virus in viruses)
        {
            if (virus.IsStunned)
                virus.SetProcess(!pause);
        }
    }

    public void StunVirus(int colour, bool isDefeated)
    {
        foreach (VirusRingVirus virus in viruses)
        {
            if (virus.Colour == colour)
            {
                if (!virus.IsStunned)
                    stunnedVirusColours++;

                virus.Stun(isDefeated);
                
                break;
            }
        }
    }

    public void DecrementStunnedVirusColours()
    {
        stunnedVirusColours--;
    }

    public void DefeatVirus(int colour)
    {

    }

    public void VirusLaugh()
    {
        isLaughing = true;
        spriteFrame = 1;
        aniTimer = laughDuration;

        foreach (VirusRingVirus virus in viruses)
        {
            if (virus.IsStunned && !virus.IsDefeated)
                virus.Unstun();
        }

        UpdateVirusLaugh();
    }

    public void DeleteAllViruses()
    {
        for (int i = 1; i < viruses.Count; i++)
        {
            viruses[i].QueueFree();
        }

        viruses.Clear();
        firstVirus.ResetState();
        firstVirus.Visible = false;
        isLaughing = false;
        stunnedVirusColours = 0;
        aniTimer = 0;
        stepFrameCount = 0;
        stepMoveCount = 0;
        SetProcess(false);
    }

    public void CreateViruses(List<int> colours)
    {
        colours = new List<int>(colours);
        colours.Sort();

        DeleteAllViruses();
        SetProcess(true);

        firstVirus.Visible = true;
        firstVirus.Colour = colours[0];
        viruses.Add(firstVirus);


        for (int i = 1; i < colours.Count; i++)
        {
            VirusRingVirus newVirus = firstVirus.Duplicate() as VirusRingVirus;
            firstVirus.GetParent().AddChild(newVirus);

            newVirus.Colour = colours[i];
            newVirus.VirusTexture = firstVirus.VirusTexture;
            newVirus.VanishTexture = firstVirus.VanishTexture;

            viruses.Add(newVirus);
        }

        MoveChild(ringOverlay, GetChildCount() - 1);
    }

    private void UpdateVirusStep()
    {
        stepFrameCount++;

        if (stunnedVirusColours == 0)
            stepMoveCount++;

        spriteFrame = (int)((stepFrameCount - 1) % 4);

        if (spriteFrame == 3)
            spriteFrame = 1;

        for (int i = 0; i < viruses.Count; i++)
        {
            if (viruses[i].IsStunned || viruses[i].IsDefeated)
                continue;

            double offset = ((-stepMoveCount / 2) * stepDistance) + (Mathf.Pi / viruses.Count) * i * 2.0f;

            viruses[i].Position = CentrePos + new Vector2((float)Mathf.Cos(offset), (float)Mathf.Sin(offset)) * distanceFromCentre;
            viruses[i].Position = (Vector2I)viruses[i].Position;
                    
            viruses[i].FrameX = spriteFrame;
        }
    }

    private void UpdateVirusLaugh()
    {
        foreach (VirusRingVirus virus in viruses)
        {
            if (!virus.IsStunned && !virus.IsDefeated)
            {
                virus.FrameX = spriteFrame;
            }
        }

        spriteFrame = spriteFrame == 1 ? 3 : 1;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        aniTimer -= (float)delta;

        if (aniTimer <= 0)
        {
            if (isLaughing)
            {
                aniTimer += laughDuration;

                UpdateVirusLaugh();
            }
            else
            {
                aniTimer += stepDuration;

                UpdateVirusStep();
            }
        }
    }
}
