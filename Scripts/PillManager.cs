using Godot;
using System;
using System.Threading.Tasks;
using static PowerUpEnums;

public partial class PillManager : Node
{
	// handles movement, rotation and activation of a player's falling pill

	[ExportGroup("Pills")]
	[Export] private PillActive activePill;
	[Export] private Pill nextPill;
	[Export] private Pill holdPill;
	[Export] private Pill powerUpPill;

	[ExportGroup("Movement")]
	[Export] private float initialLowFallSpeed;
	[Export] private float initialMedFallSpeed;
	[Export] private float initialHiFallSpeed;
	private float fallSpeed;
	[Export] private float softDropSpeed;
	[Export] private int pillsToIncreaseFallSpeed;
	[Export] private float fallSpeedIncreaseAmount;

	[ExportGroup("Locking")]
	[Export] private float initialLockSpeed;
	private float lockSpeed;

	[ExportGroup("Resources")]
	[Export] private ThemeList themeList;
	private PlayerGameSettings PlayerGameSettings { get { return jarMan.PlayerGameSettings; } }
	private CommonGameSettings CommonGameSettings { get { return jarMan.CommonGameSettings; } }

	[ExportGroup("References")]
	[Export] private JarManager jarMan;
	[Export] private PowerUpMeter powerUpMeter;
	public SfxManager SfxMan { get; set; }
	private JarMario Mario { get { return jarMan.UIMan.Mario; } }

	private int PlayerID { get { return jarMan.PlayerID; } }

	public float SoftDropSpeed { get { return softDropSpeed; } }
	public Pill ActivePill { get { return activePill; } }

	private enum pillStates { Controlling, Throwing }
	private pillStates currentState = pillStates.Throwing;
	public bool IsThrowingPill { get { return currentState == pillStates.Throwing; } }

	// Throw-related

	// Value between 0 to 1 that represents how far the pill is in the throwing animation (0 = still in mario's hand, 1 = just landed in jar))
	private float throwProgress = 0;
	// Same as above 
	private float throwSpinProgress = 0;

	// The max height offset the pill reaches at its peak when being thrown
	private float throwHeight = 44;

	// Speed of pill throw / how quickly throwProgress reaches 1
	private float throwSpeed;
	// How fast the pill rotates while being thrown
	private float throwRotateSpeed;
	// Start and ending positions of a pill throw
	private Vector2 throwStartPos;
	private Vector2 throwEndPos;
	private Vector2 throwHoldStartPos;
	private Vector2 throwHoldEndPos;
	// The pill being thrown
	private Pill throwingPill;
	// The second pill being thrown
	private Pill throwingPill2 = null;

	// Timers
	private double fallTimer = 1;
	private double moveTimer = 1;
	private double softDropTimer = 0;
	private double lockTimer = 1;
	private int lockResets = 0;

	// Tilemap-related
	private Vector2I tileSize;
	private int jarWidth;

	// Other
	private Vector2I startGridPos;
	private bool wasHoldUsed = false;
	private bool debugMode = false;
	private bool canSoftDrop = true;

	private int pillsUsed = 0;
	private bool canUseDebugMode = false;

	private bool IsActionJustPressed(string action)
	{
		return jarMan.IsActionJustPressed(action);
	}

	private bool IsActionPressed(string action)
	{
		return jarMan.IsActionPressed(action);
	}

	private bool readyCalled = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (readyCalled)
			return;
		
		// Get tile size and jar's width
		tileSize = jarMan.JarCellSize;
		jarWidth = jarMan.JarSize.X;

		// Get the starting position in jar grid tile units (top-middle of jar)
		startGridPos = jarMan.JarOrigin + Vector2I.Right * Mathf.Clamp(jarWidth / 2 - 1, 0, jarWidth);
		// Set activePill's grid position to startGridPos
		activePill.GridPos = startGridPos;

		// Update activePill's actual posiiton based on its GridPos 
		UpdateActivePillPosition();

		// Set throwEndPos to activePill's current position (based on startGridPos)
		throwEndPos = activePill.Position;
		// Set same for throwHoldEndPos but using holdPill's orig pos
		throwHoldEndPos = holdPill.OrigPos;

		SetProcess(false);

		// Reset various speeds, states, etc
		ResetState();

		if (jarMan.PossibleColours.Count == 0)
			nextPill.Visible = false;

		readyCalled = true;
	}

	public void ResetState()
	{
		activePill.Visible = false;
		nextPill.Visible = true;
		holdPill.Visible = false;
		powerUpPill.Visible = false;

		activePill.ResetState();
		nextPill.ResetState();
		holdPill.ResetState();

		pillsUsed = 0;

		if (PlayerGameSettings.SpeedLevel == 2)
            fallSpeed = initialHiFallSpeed;
        else if (PlayerGameSettings.SpeedLevel == 0)
            fallSpeed = initialLowFallSpeed;
        else
            fallSpeed = initialMedFallSpeed;

		lockSpeed = initialLockSpeed;

		if (PlayerGameSettings.UseFallSpeedAsLockSpeed || fallSpeed < lockSpeed)
			lockSpeed = fallSpeed;

		softDropSpeed = PlayerGameSettings.FasterSoftDrop ? 30 : 20;

		// If player's fall speed level is LOW, decrease throwSpeed and throwRotateSpeed
		bool slowThrow = PlayerGameSettings.SpeedLevel == 0;

		throwSpeed = 60.0f / (slowThrow ? 42.0f : 21.0f);
		throwRotateSpeed = 60.0f / (slowThrow ? 8.0f : 4.0f);
	}

	public void RandomiseNextPillColours()
	{
		if (!nextPill.Visible)
			nextPill.Visible = true;
			
		nextPill.SetRandomSegmentColours(jarMan.PossibleColours, PlayerGameSettings.OnlySingleColourPills, jarMan.LocalRng);
	}

	private void ResetAllTimersAndResets()
	{
		fallTimer = 1;
		softDropTimer = 1;
		lockTimer = 1;
		lockResets = 0;
	}

	private void Hold()
	{
		ThrowNextPill(holdPill.Visible, true);

		if (activePill.IsPowerUp)
			activePill.SetPowerUpPreviewVisibility(false);

		wasHoldUsed = true;

		activePill.Visible = true;

		jarMan.ClearPreviewTiles();

		SfxMan.Play("Hold");
	}

	public void HideThrowingPills()
	{
		if (throwingPill != null)
			throwingPill.Visible = false;
		if (throwingPill2 != null)
			throwingPill2.Visible = false;
	}

	public void HideNextPill()
	{
		if (nextPill != null)
			nextPill.Visible = false;
	}

	public void HideAllPills()
	{
		if (activePill != null)
			activePill.Visible = false;
			
		if (nextPill != null)
			nextPill.Visible = false;

		if (holdPill != null)
			holdPill.Visible = false;

		if (powerUpPill != null)
			powerUpPill.Visible = false;
	}

	// usingHoldPill = the pill being thrown into the jar was the pill in the hold reserve
	// justHeld = if the player just put a pill into the hold reserve
	public void ThrowNextPill(bool usingHoldPill = false, bool justHeld = false)
	{
		// If calling before _Ready has been performed, do _Ready now
		if (!readyCalled)
			_Ready();

		if (!CommonGameSettings.IsMultiplayer && !usingHoldPill && !powerUpMeter.IsPowerUpReady)
			Mario.PlayAnimation("Throw");

		// If power-up is ready and not using a held pill, throw and set powerUpPill then queue a new power-ups
		if (powerUpMeter.IsPowerUpReady && !usingHoldPill)
		{
			powerUpPill.Visible = true;
			powerUpPill.SetPowerUp(powerUpMeter.NextPowerUp, powerUpMeter.NextColour);

			powerUpMeter.QueueNewPowerUp();

			throwingPill = powerUpPill;
		}
		else
			throwingPill = usingHoldPill ? holdPill : nextPill;

		// If player didn't hold and the player has powerups enabled and isn't their first pill, increment power-up meter level
		if (!justHeld && PlayerGameSettings.IsUsingPowerUps && pillsUsed != 0)
			powerUpMeter.IncrementLevel(1);

		if (justHeld)
		{
			throwHoldEndPos = holdPill.OrigPos;

			throwingPill2 = activePill;
			throwingPill2.ResetSwappedState();
		}
		else
		{
			throwingPill2 = null;
			wasHoldUsed = false;
		}

		// Set throwing pill(s) to vertical and swap their segments
		throwingPill.SetOrientation(true);
		throwingPill.SwapSegments();

		if (wasHoldUsed)
		{
			throwingPill2.SetOrientation(true);
			throwingPill2.SwapSegments();
		}

		throwStartPos = throwingPill.Position;
		throwHoldStartPos = activePill.Position;
		currentState = pillStates.Throwing;
		throwProgress = 0;
		throwSpinProgress = 0;

		SetProcess(true);
	}

	public void ActivateNextPill()
	{
		pillsUsed++;

		if (!CommonGameSettings.IsMultiplayer)
			Mario.ResetFrame();

		// increase fall speed 
		if (!PlayerGameSettings.NoFallSpeedIncrease && pillsUsed % pillsToIncreaseFallSpeed == 0 && fallSpeed < softDropSpeed)
		{
			fallSpeed += fallSpeedIncreaseAmount;
			
			SfxMan.Play("SpeedUp");

			if (fallSpeed > softDropSpeed)
				fallSpeed = softDropSpeed;

			if (PlayerGameSettings.UseFallSpeedAsLockSpeed)
				lockSpeed = fallSpeed;
			else if (lockSpeed < initialLockSpeed)
			{
				if (fallSpeed < initialLockSpeed)
					lockSpeed = fallSpeed;
				else
					lockSpeed = initialLockSpeed;
			}
		}
		
		// If thrown pill went into hold reserve and the reserve was previously empty, show the pill in the hold reserve
		if (!holdPill.Visible && wasHoldUsed)
			holdPill.Visible = true;
		
		// If power-up pill was visisble, hide it and reset its state
		if (powerUpPill.Visible)
		{
			powerUpPill.Visible = false;
			powerUpPill.ResetState();
		}
		
		// Store old active pill state before changing it
		int activeCentreColour = activePill.CentreSegmentColour;
		int activeSecondaryColour = activePill.SecondarySegmentColour;
		PowerUp activePowerUp = activePill.CurrentPowerUp;
		bool wasActivePowerUp = activePill.IsPowerUp;

		// Set active pill to match the pill that was thrown
		if (throwingPill.IsPowerUp)
			activePill.SetPowerUp(throwingPill.CurrentPowerUp, throwingPill.CentreSegmentColour);
		else
			activePill.SetSegmentColours(throwingPill.CentreSegmentColour, throwingPill.SecondarySegmentColour);

		// If hold was used, set the hold pill to match the last active pill
		if (wasHoldUsed)
		{
			if (wasActivePowerUp)
				holdPill.SetPowerUp(activePowerUp, activeCentreColour);
			else
				holdPill.SetSegmentColours(activeCentreColour, activeSecondaryColour);
		}
			
		// If activePill is vertical, swap orientation to horizontal
		if (activePill.IsVertical)
			activePill.SwapOrientation();

		// Reset activePill's grid position
		activePill.GridPos = startGridPos;

		// If pill thrown was the pill thrown from mario, randomise its segments
		if (throwingPill == nextPill)
			RandomiseNextPillColours();

		ResetAllTimersAndResets();

		activePill.Visible = true;

		// Check if the active pill isn't blocked by anything - proceed if clear, otherwise do a game over
		if (jarMan.IsCellFree(activePill.GridPos) && jarMan.IsCellFree(activePill.GridPos + Vector2I.Right))
		{
			currentState = pillStates.Controlling;
			UpdateActivePillPosition();
			SetProcess(true);
		}
		else
		{
			UpdateActivePillPosition();
			SetProcess(false);
			jarMan.GameOver();
		}
	}

	private void UpdateActivePillPosition()
	{
		activePill.GlobalPosition = jarMan.TilemapGlobalPos + activePill.GridPos * tileSize;
		activePill.GlobalPosition += tileSize / 2;

		if (currentState == pillStates.Controlling)
		{
			UpdateGhostPillPreview();

			if (activePill.IsPowerUp && activePill.PowerUpHasPreview)
            {
				if (activePill.LandPos != activePill.GridPos)
				{
					Vector2 ghostPos;
					ghostPos = jarMan.TilemapGlobalPos + activePill.LandPos * tileSize;
					ghostPos += tileSize / 2;
					
					activePill.SetGhostPowerUpPreviewGlobalPosition(ghostPos);
				}

				activePill.UpdateGhostPowerUpPreviewVisibility();
            }
		}
	}
	private void UpdateGhostPillPreview()
	{
		if (!CommonGameSettings.IsGhostPillEnabled)
			return;

		jarMan.ClearPreviewTiles();

		int downwardSteps = 0;
		
		Vector2I downPos = activePill.GridPos + Vector2I.Down;
		Vector2I secondaryOffset = activePill.IsVertical ? Vector2I.Up : Vector2I.Right;

		while (jarMan.IsCellFree(downPos) && (activePill.IsPowerUp || activePill.IsVertical || jarMan.IsCellFree(downPos + Vector2I.Right)))
		{
			downwardSteps++;
			downPos += Vector2I.Down;
		}

		Vector2I ghostPos = downPos + Vector2I.Up;

		if (downwardSteps > 0)
		{
			jarMan.CreateGhostPill(activePill, ghostPos);
		}

		activePill.LandPos = ghostPos;
	}

	private bool Rotate(int dir, bool bypassCollisions = false)
	{
		if (activePill.IsPowerUp)
			return false;
		
		bool isVertical = activePill.IsVertical;
		Vector2I targetPos = activePill.GridPos;
		bool onlySwap = false;

		// kicks
		// vertical -> horizontal
		if (isVertical)
		{
			// right of centre blocked - more target to left
			if (!jarMan.IsCellFree(activePill.GridPos + Vector2I.Right))
			{
				// if left of centre is blocked, cancel rotation
				if (!jarMan.IsCellFree(activePill.GridPos + Vector2I.Left))
					onlySwap = true;
				else
					targetPos += Vector2I.Left;

			}
		}
		// horizontal -> vertical
		else
		{
			// top of centre blocked - more target to right
			if (!jarMan.IsCellFree(activePill.GridPos + Vector2I.Up))
			{
				// if right + up of centre is blocked, move target down
				if (!jarMan.IsCellFree(activePill.GridPos + Vector2I.Right + Vector2I.Up))
				{
					// if down from centre is blocked, move target to right and down
					if (!jarMan.IsCellFree(activePill.GridPos + Vector2I.Down))
					{
						// if right and down from centre is blocked, cancel rotation
						if (!jarMan.IsCellFree(activePill.GridPos + Vector2I.Right + Vector2I.Down))
							onlySwap = true;
						else
							targetPos += Vector2I.Right + Vector2I.Down;
					}
					else
						targetPos += Vector2I.Down;
				}
				else
					targetPos += Vector2I.Right;
			}
		}


		if ((dir == 1 ? !activePill.IsVertical : activePill.IsVertical) || onlySwap)
			activePill.SwapSegments();

		if (!onlySwap)
			activePill.SwapOrientation();

		if (activePill.GridPos != targetPos)
		{
			activePill.GridPos = targetPos;
			UpdateActivePillPosition();
		}
		else
		{
			UpdateGhostPillPreview();
		}


		return true;
	}

	private bool Move(Vector2I dir, bool bypassCollisions = false)
	{
		Vector2I targetPos = activePill.GridPos + dir;
		Vector2I secondaryTargetPos = targetPos + (activePill.IsVertical ? Vector2I.Up : Vector2I.Right);

		if (bypassCollisions || (jarMan.IsCellFree(targetPos) && (activePill.IsPowerUp || jarMan.IsCellFree(secondaryTargetPos))))
		{
			activePill.GridPos = targetPos;
			UpdateActivePillPosition();
			return true;
		}

		return false;
	}

	private void HardDrop()
	{
		int downwardSteps = 0;

		Vector2I downPos = activePill.GridPos + Vector2I.Down;

		while (jarMan.IsCellFree(downPos) && (activePill.IsPowerUp || activePill.IsVertical || jarMan.IsCellFree(downPos + Vector2I.Right)))
		{
			downwardSteps++;
			downPos += Vector2I.Down;
		}

		activePill.GridPos = downPos + Vector2I.Up;
		UpdateActivePillPosition();
	}

	private bool IsGrounded()
	{
		return !jarMan.IsCellFree(activePill.GridPos + Vector2I.Down) || (!activePill.IsPowerUp && !activePill.IsVertical && !jarMan.IsCellFree(activePill.GridPos + Vector2I.Right + Vector2I.Down));
	}

	private void UpdateTimers(double delta)
	{
		bool gnd = IsGrounded();

		if (!gnd)
			fallTimer -= delta * fallSpeed;

		if (softDropTimer > 0)
			softDropTimer -= delta * softDropSpeed;

		if (gnd || lockTimer < 1)
			lockTimer -= delta * lockSpeed;
		
		if (IsActionPressed("MoveLeft") || IsActionPressed("MoveRight"))
			moveTimer -= delta * PlayerGameSettings.FirstMoveSpeed;
		else
			moveTimer = 1;
	}

	private void UpdateTimersOnGroundedAdjustment()
	{
		softDropTimer = 1;
		fallTimer = 1;
		if (lockResets < PlayerGameSettings.MaxLockResets)
		{
			lockTimer = 1;
			lockResets++;
		}
	}

	// Updates throwing pill movement (uncontrollable)
	private void TickThrow(double dt)
	{
		throwProgress += throwSpeed * (float)dt;
		throwSpinProgress += throwRotateSpeed * (float)dt;

		if (throwProgress >= 1)
		{
			throwingPill.Position = throwStartPos;
			throwingPill.SetOrientation(false);

			if (throwingPill.AreSegmentsSwapped)
				throwingPill.SwapSegments();

			if (throwingPill2 != null && throwingPill2.AreSegmentsSwapped)
				throwingPill2.SwapSegments();

			ActivateNextPill();
			return;
		}

		// Set thrown pill position along an upwards curve depending on throwProgress
		Vector2 newPos;

		newPos = throwStartPos * (1 - throwProgress) + throwEndPos * throwProgress;
		newPos.Y -= throwHeight * Mathf.Sin(throwProgress * Mathf.Pi);

		throwingPill.Position = newPos;

		// If throwingPill2 exists, move it into hold slot
		if (throwingPill2 != null)
		{
			// Set throwingPill2 position along a downwards curve depending on throwProgress
			Vector2 newPos2;

			newPos2 = throwHoldStartPos * (1 - throwProgress) + throwHoldEndPos * throwProgress;
			newPos2.Y += throwHeight * Mathf.Sin(throwProgress * Mathf.Pi);

			throwingPill2.Position = newPos2;
		}

		// Rotate pill after throwRotateRate intervals
		if (throwSpinProgress >= 1)
		{
			throwSpinProgress -= 1;

			if (throwingPill.IsVertical)
				throwingPill.SwapSegments();
			throwingPill.SwapOrientation();

			if (throwingPill2 != null)
			{
				if (throwingPill2.IsVertical)
					throwingPill2.SwapSegments();
				throwingPill2.SwapOrientation();
			}
		}
	}

	// Updates controllable pill movement
	private void TickControl(double dt)
	{
		if (IsActionJustPressed("Hold") && PlayerGameSettings.IsHoldEnabled)
		{
			if (!wasHoldUsed)
				Hold();
			return;
		}

		UpdateTimers(dt);

		// left/right movement
		if (!(IsActionPressed("MoveLeft") && IsActionPressed("MoveRight")))
		{
			if (IsActionJustPressed("MoveLeft") || (moveTimer <= 0 && IsActionPressed("MoveLeft")))
			{
				if (Move(Vector2I.Left))
				{
					SfxMan.Play("Move");

					if (IsGrounded())
						UpdateTimersOnGroundedAdjustment();
				}

				if (IsActionJustPressed("MoveLeft"))
					moveTimer = 1;
				else
					moveTimer = 1.0f * (PlayerGameSettings.FirstMoveSpeed / PlayerGameSettings.RepeatedMoveSpeed);
			}
			if (IsActionJustPressed("MoveRight") || (moveTimer <= 0 && IsActionPressed("MoveRight")))
			{
				if (Move(Vector2I.Right))
				{
					SfxMan.Play("Move");

					if (IsGrounded())
						UpdateTimersOnGroundedAdjustment();
				}

				if (IsActionJustPressed("MoveRight"))
					moveTimer = 1;
				else
					moveTimer = 1.0f * (PlayerGameSettings.FirstMoveSpeed / PlayerGameSettings.RepeatedMoveSpeed);
			}
		}

		// rotation
		if (!(IsActionPressed("RotateLeft") && IsActionPressed("RotateRight")))
		{
			if (IsActionJustPressed("RotateLeft"))
			{
				bool gnd = IsGrounded();
				int origY = activePill.GridPos.Y;
				if (Rotate(-1))
				{
					SfxMan.Play("Rotate");

					if (gnd)
					{
						UpdateTimersOnGroundedAdjustment();

						// if y position has changed after rotation, reset locking
						if (origY != activePill.GridPos.Y)
						{
							lockTimer = 1;
							lockResets = 0;
						}
					}
				}
			}
			if (IsActionJustPressed("RotateRight"))
			{
				bool gnd = IsGrounded();
				int origY = activePill.GridPos.Y;
				if (Rotate(1))
				{
					SfxMan.Play("Rotate");
					
					if (gnd)
					{
						UpdateTimersOnGroundedAdjustment();

						// if y position has changed after rotation, reset locking
						if (origY != activePill.GridPos.Y)
						{
							lockTimer = 1;
							lockResets = 0;
						}
					}
				}
		}
		}

		// gravity and dropping
		
		// place pill into tilemap once locking finishes, hard-dropped, or (started soft-droping or soft-dropping with InstantSoftDropLock) while grounded 
		if (((lockTimer <= 0 /* || IsActionJustPressed("SoftDrop") */ || (PlayerGameSettings.InstantSoftDropLock && IsActionPressed("SoftDrop"))) && IsGrounded()) || (IsActionJustPressed("HardDrop") && CommonGameSettings.IsHardDropEnabled))
		{
			if (IsActionJustPressed("HardDrop") && CommonGameSettings.IsHardDropEnabled)
				HardDrop();

			SetProcess(false);
			jarMan.AddPillToTilemap(activePill);
		}
		// dropp down either by soft drop or gravity
		else if ((IsActionPressed("SoftDrop") && softDropTimer <= 0 && canSoftDrop) || fallTimer <= 0)
		{
			if (Move(Vector2I.Down))
				ResetAllTimersAndResets();
		}
		else if (!canSoftDrop && !IsActionPressed("SoftDrop"))
		{
			canSoftDrop = true;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (canUseDebugMode && Input.IsActionJustPressed("ToggleDebugMode"))
			debugMode = !debugMode;

		if (debugMode)
		{
			if (IsActionJustPressed("MoveLeft"))
				Move(Vector2I.Left, true);
			if (IsActionJustPressed("MoveRight"))
				Move(Vector2I.Right, true);
			if (IsActionJustPressed("MoveUp"))
				Move(Vector2I.Up, true);
			if (IsActionJustPressed("MoveDown"))
				Move(Vector2I.Down, true);

			if (IsActionJustPressed("RotateLeft"))
				Rotate(-1);
			if (IsActionJustPressed("RotateRight") && !IsActionJustPressed("MoveUp"))
				Rotate(1);

			return;
		}

		if (currentState == pillStates.Throwing)
			TickThrow((float)delta);
		else
			TickControl((float)delta);
	}
}
