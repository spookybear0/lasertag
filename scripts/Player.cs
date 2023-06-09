using Godot;
using System;

public partial class Player : CharacterBody3D {
	public const float speed = 6f;
	public const float speedWhileJumping = 4f;
	public const float jumpVelocity = 6f;
	public float gravity = 20f;//ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	public float sensitivity = 1f;

	public SM5Player sm5Player;

	[Export]
	public int multiplayerId;

	private Camera3D camera;
	private AnimationPlayer animationPlayer;
	private Godot.Timer shootTimer;
	private bool canShoot = true;
	private Laser laser;
	private PanelContainer pauseMenu;

	public override void _EnterTree() {
		multiplayerId = Name.ToString().Replace("Player", "").ToInt();
		SetMultiplayerAuthority(multiplayerId);
	}

	public override void _Ready() {
		// animation stuff

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.AnimationFinished += AnimationFinished;

		// laser stuff

		laser = GetNode<Laser>("Camera/Pistol/Laser");
		laser.Visible = false;

		// sm5 player stuff

		sm5Player = GetNode<SM5Player>("SM5Player");

		// close menu by default for all players (INCLUDING SERVER)
		GameManager.Instance.inMenu = false;

		PanelContainer pauseMenu = GetNode<PanelContainer>("/root/Scene/PauseCanvas/PauseMenu");
		pauseMenu.GetParent<CanvasLayer>().Visible = false;
		pauseMenu.MouseFilter = Control.MouseFilterEnum.Stop;
		
		setMouseCapture(false);

		if (!IsMultiplayerAuthority()) {
			return;
		}

		GameManager.Instance.localPlayer = this;

		camera = GetNode<Camera3D>("Camera");
		
		camera.MakeCurrent();
		setMouseCapture(true);
	}

	public void setMouseCapture(bool capture) {
		if (capture)
			Input.MouseMode = Input.MouseModeEnum.Captured;
		else
			Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (!IsMultiplayerAuthority())
			return;
		
		if (@event is InputEventMouseMotion && !GameManager.Instance.inMenu) {
			InputEventMouseMotion mousemotion = (@event as InputEventMouseMotion);
			RotateY((float)(-mousemotion.Relative.X * sensitivity * 0.005));
			camera.RotateX((float)(-mousemotion.Relative.Y * sensitivity * 0.005));
			camera.Rotation = new Vector3(Mathf.Clamp(camera.Rotation.X, -Mathf.Pi / 2, Mathf.Pi / 2), camera.Rotation.Y, camera.Rotation.Z);
		}

		if (Input.IsActionJustPressed("shoot") && canShoot) {
			GD.Print("shoot");
			Shoot();
		}
	}

	public override void _PhysicsProcess(double delta) {
		if (!IsMultiplayerAuthority())
			return;
		
		Vector3 velocity = Velocity;

		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
			velocity.Y = jumpVelocity;

		Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero) {
			if (!IsOnFloor()) {
				velocity.X = direction.X * speedWhileJumping;
				velocity.Z = direction.Z * speedWhileJumping;
			}
			else {
				velocity.X = direction.X * speed;
				velocity.Z = direction.Z * speed;
			}
		}
		else {
			if (IsOnFloor()) {
				velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
			}
			else {
				velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
			}
		}

		Velocity = velocity;

		if (animationPlayer.CurrentAnimation == "shoot") {} // pass
		else if (inputDir != Vector2.Zero && IsOnFloor())
			animationPlayer.Play("move");
		else
			animationPlayer.Play("idle");


		MoveAndSlide();
	}

	public void Shoot() {
		Rpc("playShootEffects");

		// shoot delay
		shootTimer = new Godot.Timer();
		shootTimer.WaitTime = 0.15f;
		shootTimer.OneShot = true;
		shootTimer.Connect("timeout", new Callable(this, "resetShootTimer"));
		AddChild(shootTimer);
		shootTimer.Start();
		canShoot = false;

		// raycast

		GodotObject collider = laser.rayCast.GetCollider();
		if (collider != null) {
			Player other = collider as Player;
			if (other != null) {
				sm5Player.roleBehavior.RpcId(multiplayerId, "Zap", sm5Player.roleBehavior);
				//other.sm5Player.roleBehavior.Zap(sm5Player.roleBehavior);
			}
		}
	}

	public void resetShootTimer() {
		shootTimer.QueueFree();
		shootTimer = null;
		canShoot = true;
	}

	// call local and remote
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=true)]
	public void playShootEffects() {
		animationPlayer.Stop();
		animationPlayer.Play("shoot");
		laser.Visible = true;
	}

	public void AnimationFinished(StringName animName) {
		if (animName == "shoot") {
			animationPlayer.Play("idle");
			laser.Visible = false;
		}
	}
}
