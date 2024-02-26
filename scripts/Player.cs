using Godot;
using System;

public partial class Player : CharacterBody3D {
	public const float speed = 5f;
	public const float speedWhileJumping = 3f;
	public const float jumpVelocity = 5.5f;
	public float gravity = 22f;
	public float sensitivity = 1f;
	public float shotDelay = 0.2f;

	public SM5Player sm5Player;
	public SM5Game sm5Game;

	[Export]
	public int multiplayerId;
	[Export]
	public string playerName;

	private Camera3D camera;
	private AnimationPlayer animationPlayer;
	private Godot.Timer shootTimer;
	private bool canShoot = true;
	private Laser laser;
	private PanelContainer pauseMenu;
	private Label3D nameLabel;
	private Sprite3D roleIcon;
	private MeshInstance3D mesh;
	private DirectionalLight3D flashlight;
	private Timer rainbowTimer;

	// materials
	private Material earthMaterial;
	private Material earthDownedMaterial;
	private Material fireMaterial;
	private Material fireDownedMaterial;
	private Material fireNukeMaterial;
	private Material earthNukeMaterial;

	public override void _EnterTree() {
		multiplayerId = Name.ToString().Replace("Player", "").ToInt();
		SetMultiplayerAuthority(multiplayerId);

		// sm5 player stuff

		sm5Player = GetNode<SM5Player>("SM5Player");
		sm5Player.SetMultiplayerAuthority(multiplayerId);
	}

	public override void _Ready() {
		nameLabel = GetNode<Label3D>("AboveHead/NameLabel");
		roleIcon = GetNode<Sprite3D>("AboveHead/RoleIcon");
		mesh = GetNode<MeshInstance3D>("MeshInstance3D");
		flashlight = GetNode<DirectionalLight3D>("Camera/Pistol/Flashlight");
		sm5Game = GameManager.Instance.gameController;

		sm5Game.players.Add(this);

		playerName = GameManager.Instance.playerName;
		nameLabel.Text = playerName;

		roleIcon.Texture = GD.Load<Texture2D>("res://assets/roles/" + sm5Player.role.ToString().ToLower() + "_big.png");

		GameManager.Instance.players.Add(this);

		sm5Player.roleBehavior.player = this;


		sm5Player = GetNode<SM5Player>("SM5Player");
		// lambda
		sm5Player.roleBehavior.OnDowned += () => { Rpc("OnDowned"); };
		sm5Player.roleBehavior.OnRespawned += () => { Rpc("OnRespawned"); };

		// animation stuff

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.AnimationFinished += AnimationFinished;

		// setup materials

		earthMaterial = GD.Load<Material>("res://assets/materials/earth.tres");
		earthDownedMaterial = GD.Load<Material>("res://assets/materials/earth_downed.tres");
		fireMaterial = GD.Load<Material>("res://assets/materials/fire.tres");
		fireDownedMaterial = GD.Load<Material>("res://assets/materials/fire_downed.tres");
		earthNukeMaterial = GD.Load<Material>("res://assets/materials/rainbow_earth.tres");
		fireNukeMaterial = GD.Load<Material>("res://assets/materials/rainbow_fire.tres");

		// laser stuff

		laser = GetNode<Laser>("Camera/Pistol/Laser");
		laser.Visible = false;

		// close menu by default for all players (INCLUDING SERVER)
		GameManager.Instance.inMenu = false;

		PanelContainer pauseMenu = GetNode<PanelContainer>("/root/Scene/PauseCanvas/PauseMenu");
		pauseMenu.GetParent<CanvasLayer>().Visible = false;
		pauseMenu.MouseFilter = Control.MouseFilterEnum.Stop;
		
		setMouseCapture(false);

        GD.Print("Player name LLL: " + GameManager.Instance.playerName);

		if (!IsMultiplayerAuthority()) {
			return;
		}

		Position = GetNode<Marker3D>("/root/Scene/RedBaseSpawn").GlobalTransform.Origin;

		GameManager.Instance.localPlayer = this;
        GD.Print("Player name: " + GameManager.Instance.playerName);
        playerName = GameManager.Instance.playerName;
        nameLabel.Text = playerName;

		// camera and cull mask
		// allows us to not see text above our head
		// but still see other player's text

		camera = GetNode<Camera3D>("Camera");
		camera.MakeCurrent();

		// mouse capture

		setMouseCapture(true);

        // camera stuff for above head text

        nameLabel.Layers = 2;
        roleIcon.Layers = 2;
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

		// not using rapid fire, so we have to press the button every time
		// rapid fire is handled in _Process for holding down the button
		if (!sm5Player.roleBehavior.rapidFireEnabled && Input.IsActionJustPressed("shoot")
			&& canShoot && !sm5Player.roleBehavior.downed && !GameManager.Instance.inMenu) {
			Shoot();
		}

		// flashlight

		if (Input.IsActionJustPressed("flashlight") && !GameManager.Instance.inMenu) {
			flashlight.Visible = !flashlight.Visible;
		}

		// ability

		if (Input.IsActionJustPressed("ability") && !sm5Player.roleBehavior.downed && !GameManager.Instance.inMenu) {
			sm5Player.roleBehavior.UseSpecial();
		}
	}

	public override void _Process(double delta) {
		if (!IsMultiplayerAuthority())
			return;

		// check if using rapid fire, if so, we can hold down the shoot button
		if (sm5Player.roleBehavior.rapidFireEnabled && Input.IsActionPressed("shoot") 
			&& canShoot && !sm5Player.roleBehavior.downed && !GameManager.Instance.inMenu) {
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
		if (sm5Player.roleBehavior.shots <= 0)
			return; // no shots

		Rpc("playShootEffects");

		// shoot delay
		shootTimer = new Godot.Timer();
		shootTimer.WaitTime = shotDelay;
		shootTimer.OneShot = true;
		shootTimer.Connect("timeout", new Callable(this, "resetShootTimer"));
		AddChild(shootTimer);
		shootTimer.Start();
		canShoot = false;

		sm5Player.roleBehavior.Shoot();

		// raycast

		GodotObject collider = laser.rayCast.GetCollider();
		if (collider != null) {
			Player other = collider as Player;
			if (other != null) {
				sm5Player.roleBehavior.Zap(other.sm5Player.roleBehavior);
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

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=true)]
	public void OnRoleUpdate(int roleInt) {
		Role role = (Role)roleInt;
		roleIcon.Texture = GD.Load<Texture2D>("res://assets/roles/" + role.ToString().ToLower() + "_big.png");

		shotDelay = sm5Player.roleBehavior.shotDelay;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=true)]
	public void OnTeamUpdate(int teamInt) {
		Team oldTeam = sm5Player.team;

		Team team = (Team)teamInt;

		if (oldTeam == Team.Fire) {
			sm5Game.firePlayers.Remove(this);
		}
		else if (oldTeam == Team.Earth) {
			sm5Game.earthPlayers.Remove(this);
		}

		if (team == Team.Fire) {
			if (sm5Player.roleBehavior.downed) {
				mesh.MaterialOverride = fireDownedMaterial;
			}
			else {
				mesh.MaterialOverride = fireMaterial;
			}
			sm5Game.firePlayers.Add(this);
		}
		else if (team == Team.Earth) {
			if (sm5Player.roleBehavior.downed) {
				mesh.MaterialOverride = earthDownedMaterial;
			}
			else {
				mesh.MaterialOverride = earthMaterial;
			}
			sm5Game.earthPlayers.Add(this);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal=true)]
	public void OnDowned() {
		if (sm5Player.team == Team.Fire) {
			mesh.MaterialOverride = fireDownedMaterial;
		}
		else if (sm5Player.team == Team.Earth) {
			mesh.MaterialOverride = earthDownedMaterial;
		}
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal=true)]
	public void OnRespawned() {
		if (sm5Player.team == Team.Fire) {
			mesh.MaterialOverride = fireMaterial;
		}
		else if (sm5Player.team == Team.Earth) {
			mesh.MaterialOverride = earthMaterial;
		}	
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal=true)]
	public void StartNuking() {
		if (sm5Player.team == Team.Fire) {
			mesh.MaterialOverride = fireNukeMaterial;
		}
		else if (sm5Player.team == Team.Earth) {
			mesh.MaterialOverride = earthNukeMaterial;
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void OnNuked() { // getting nuked
		rainbowTimer = new Timer();
		rainbowTimer.WaitTime = 4f;
		rainbowTimer.OneShot = true;
		rainbowTimer.Connect("timeout", new Callable(this, "RainbowTimerTimeout"));
		AddChild(rainbowTimer);
		rainbowTimer.Start();

		if (sm5Player.team == Team.Fire) {
			mesh.MaterialOverride = fireNukeMaterial;
		}
		else if (sm5Player.team == Team.Earth) {
			mesh.MaterialOverride = earthNukeMaterial;
		}

		sm5Player.roleBehavior.OnNuke();
	}

	public void RainbowTimerTimeout() {
		Rpc("OnTeamUpdate", (int)sm5Player.team);
	}
}
