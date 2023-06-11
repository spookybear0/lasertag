using Godot;
using System;

// TODOs
// bases
// specials

public partial class RoleBehavior : Node3D {
    // other stuff

    Timer downedTimer;
    Timer downedSafeTimer;

    // base will be scout will update later
    // dynamic will change

    private int _hitPoints = 1;
    private int _shots = 30;
    private int _missiles = 5;
    private int _lives = 15;
    private int _score = 0;
    private int _specialPoints = 0;
    private Team _team = Team.None;

    [Export]
    public bool downed { get; set; }
    [Export]
    public bool dead { get; set; }
    [Export]
    public bool safe { get; set; } // from first 4 seconds of downed
    [Export]
    public bool downFromShotResub { get; set; }
    [Export]
    public bool downFromLifeResub { get; set; }

    [Export]
    public float shotDelay { get; set; }

    [Export]
    public int shotPower { get; set; }
    [Export]
    public int initialHitPoints { get; set; }
    [Export]
    public int hitPoints {
        get {
            return _hitPoints;
        }
        set {
            _hitPoints = value;
            if (!IsMultiplayerAuthority()) return;
            GameManager.Instance.hud.roleHitpoints.Text = value.ToString() + "/" + initialHitPoints.ToString();
        }
    }

    [Export]
    public int shotsInitial { get; set; }
    [Export]
    public int shotsResupply { get; set; }
    [Export]
    public int shotsMax { get; set; }
    [Export]
    public int shots {
        get {
            return _shots;
        }
        set {
            _shots = value;
            if (!IsMultiplayerAuthority()) return;
            GameManager.Instance.hud.shotsValue.Text = value.ToString();
        }
    }

    [Export]
    public int missilesInitial { get; set; }
    [Export]
    public int missiles {
        get {
            return _missiles;
        }
        set {
            _missiles = value;
            if (!IsMultiplayerAuthority()) return;
            GameManager.Instance.hud.missilesValue.Text = value.ToString();
        }
    }

    [Export]
    public int livesInitial { get; set; }
    [Export]
    public int livesResupply { get; set; }
    [Export]
    public int livesMax { get; set; }
    [Export]
    public int lives {
        get {
            return _lives;
        }
        set {
            _lives = value;
            if (!IsMultiplayerAuthority()) return;
            GameManager.Instance.hud.livesValue.Text = value.ToString();
        }
    }

    [Export]
    public int score {
        get {
            return _score;
        }
        set {
            _score = value;
            if (!IsMultiplayerAuthority()) return;
            GameManager.Instance.hud.scoreValue.Text = value.ToString();
        }
    }
    [Export]
    public int specialPoints {
        get {
            return _specialPoints;
        }
        set {
            _specialPoints = value;
            if (value > 99) {
                _specialPoints = 99;
            }
            if (!IsMultiplayerAuthority()) return;
            GameManager.Instance.hud.specialsValue.Text = _specialPoints.ToString();
        }
    }
    [Export]
    public Team team {
        get {
            return _team;
        }
        set {
            _team = value;
            if (!IsMultiplayerAuthority()) return;
            GameManager.Instance.hud.teamLabel.Text = value.ToString() + " Team";
        }
    }
    // default to scout
    // wont be used in UI, handled by SM5Player
    [Export]
    public Role role { get; set; } = Role.Scout; 

    [Export]
    public long multiplayerId { get; set; }

    
    // role/ability specific stuff

    [Export]
    public int specialAbilityCost { get; set; }

    [Export]
    public bool rapidFireEnabled { get; set; }
    
    public void Shoot() {
        if (role != Role.Ammo)
            shots -= 1;
        
        if (shots <= 0) {
            shots = 0;
            return;
        }
    }

    private void Down() {
        if (downed) {
            downedTimer.Stop();
        }

        downed = true;

        downedTimer = new Timer();
        downedTimer.WaitTime = 8;
        downedTimer.OneShot = true;
        downedTimer.Connect("timeout", new Callable(this, "DownedTimerTimeout"));
        AddChild(downedTimer);
        downedTimer.Start();

        safe = true;

        downedSafeTimer = new Timer();
        downedSafeTimer.WaitTime = 4;
        downedSafeTimer.OneShot = true;
        downedSafeTimer.Connect("timeout", new Callable(this, "DownedSafeTimerTimeout"));
        AddChild(downedSafeTimer);
        downedSafeTimer.Start();

        EmitSignal(SignalName.OnDowned);
    }

    private void DownedTimerTimeout() {
        downed = false;
        downFromShotResub = false;
        downFromLifeResub = false;
        hitPoints = initialHitPoints;

        EmitSignal(SignalName.OnRespawned);
    }

    private void DownedSafeTimerTimeout() {
        safe = false;
    }

    public void ResupplyShots(RoleBehavior other) {
        if (!other.downed) {
            other.RpcId(other.multiplayerId, "OnResupplyShots", multiplayerId);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)] // TODO: make anticheat
    public void OnResupplyShots(long mId) { // getting resupplied
        RoleBehavior other = RoleBehaviorFromMultiplayerId(mId);

        shots += shotsResupply;
        if (shots > shotsMax) {
            shots = shotsMax;
        }
        
        downFromShotResub = true;
        rapidFireEnabled = false;
    }

    private void ResupplyLives(RoleBehavior other) {
        if (!other.downed) {
            other.RpcId(other.multiplayerId, "OnResupplyLives", multiplayerId);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)] // TODO: make anticheat
    public void OnResupplyLives(long mId) { // getting resupplied
        RoleBehavior other = RoleBehaviorFromMultiplayerId(mId);

        lives += livesResupply;
        if (lives > livesMax) {
            lives = livesMax;
        }

        downFromLifeResub = true;
        rapidFireEnabled = false;
    }

    public void Zap(RoleBehavior other) { // shoot them
        if (other.downed && other.safe) {
            return; // first 4 seconds of downed, safe
        }

        // check if resupplying
        // check if downed, has to be fully up to get resupplied
        // allow doubles by checking if they are downed from a resupply
        if (team == other.team && (role == Role.Ammo || role == Role.Medic)) {
            if (other.downed && other.safe) {
                if (role == Role.Ammo && other.downFromLifeResub) {
                    ResupplyShots(other);
                }
                else if (role == Role.Medic && other.downFromShotResub) {
                    ResupplyLives(other);
                }
            }
            else {
                if (role == Role.Ammo) {
                    ResupplyShots(other);
                }
                else if (role == Role.Medic) {
                    ResupplyLives(other);
                }
            }
            return;
        }

        other.RpcId(other.multiplayerId, "OnZapped", multiplayerId);
        if (team == other.team) {
            score -= 100;
        }
        else {
            score += 100;
            specialPoints += 1;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)] // TODO: make anticheat
    public void OnZapped(long mId) { // when we get shot
        RoleBehavior other = RoleBehaviorFromMultiplayerId(mId);

        hitPoints -= other.shotPower;
        if (hitPoints <= 0) {
            hitPoints = 0;
            Down();
            lives--;
            if (lives < 0) {
                lives = 0;
            }
            score -= 20;
            
            if (lives <= 0) {
                dead = true;
            }
        }
    }

    public void Missile(RoleBehavior other) {
        if (missiles > 0) {
            other.RpcId(other.multiplayerId, "OnMissiled", multiplayerId);
            missiles--;

            score += 500;
            specialPoints += 2;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)] // TODO: make anticheat
    public void OnMissiled(long mId) {
        RoleBehavior other = RoleBehaviorFromMultiplayerId(mId);
        // remove two lives instantly
        lives -= 2;
        Down();

        score -= 100;

        if (lives <= 0) {
            dead = true;
        }
    }

    public void UseSpecial() {
        if (specialPoints < specialAbilityCost) {
            return;
        }

        switch (role) {
            case Role.Ammo: {
                ShotBoost();
                break;
            }
            case Role.Medic: {
                LifeBoost();
                break;
            }
            case Role.Commander: {
                Nuke();
                break;
            }
            case Role.Scout: {
                RapidFire();
                break;
            }
            // heavy has none
        }

        // subtract the cost
        specialPoints -= specialAbilityCost;
    }

    // TODO: implement specials
    private void ShotBoost() {

    }

    private void LifeBoost() {

    }

    private void Nuke() {

    }

    private void RapidFire() {
        rapidFireEnabled = true;
        shotDelay = 0.05f;
    }

    public override void _Ready() {
        UpdateRoleSpecificFields(role);
        dead = false;
        downed = false;
        safe = false;
        rapidFireEnabled = false;
        downFromShotResub = false;
        downFromLifeResub = false;
    }

    public void UpdateRoleSpecificFields(Role newRole) {
        role = newRole;

        switch (newRole) {
            case Role.Commander: {
                shotDelay = 0.25f;
                shotPower = 2;
                initialHitPoints = 3;
                hitPoints = 3;
                shotsInitial = 30;
                shotsResupply = 5;
                shotsMax = 60;
                shots = 30;
                missilesInitial = 5;
                missiles = 5;
                livesInitial = 15;
                livesResupply = 4;
                livesMax = 30;
                lives = 15;
                specialAbilityCost = 20;
                break;
            }
            case Role.Heavy: {
                shotDelay = 0.35f;
                shotPower = 3;
                initialHitPoints = 3;
                hitPoints = 3;
                shotsInitial = 20;
                shotsResupply = 5;
                shotsMax = 40;
                shots = 20;
                missilesInitial = 5;
                missiles = 5;
                livesInitial = 10;
                livesResupply = 3;
                livesMax = 20;
                lives = 10;
                specialAbilityCost = 0;
                break;
            }
            default: { // Role.Scout
                shotDelay = 0.2f;
                shotPower = 1;
                initialHitPoints = 1;
                hitPoints = 1;
                shotsInitial = 30;
                shotsResupply = 10;
                shotsMax = 60;
                shots = 30;
                missilesInitial = 0;
                missiles = 0;
                livesInitial = 15;
                livesResupply = 5;
                livesMax = 30;
                lives = 15;
                specialAbilityCost = 15;
                break;
            }
            case Role.Ammo: {
                shotDelay = 0.2f;
                shotPower = 1;
                initialHitPoints = 1;
                hitPoints = 1;
                shotsInitial = 15;
                shotsResupply = 0;
                shotsMax = 15;
                shots = 15;
                missilesInitial = 0;
                missiles = 0;
                livesInitial = 10;
                livesResupply = 3;
                livesMax = 20;
                lives = 10;
                specialAbilityCost = 15;
                break;
            }
            case Role.Medic: {
                shotDelay = 0.2f;
                shotPower = 1;
                initialHitPoints = 1;
                hitPoints = 1;
                shotsInitial = 15;
                shotsResupply = 5;
                shotsMax = 30;
                shots = 15;
                missilesInitial = 0;
                missiles = 0;
                livesInitial = 20;
                livesResupply = 0;
                livesMax = 20;
                lives = 20;
                specialAbilityCost = 10;
                break;
            }
        }
    }

    private RoleBehavior RoleBehaviorFromMultiplayerId(long mId) {
        return GetNode<RoleBehavior>("/root/Scene/Player" + mId.ToString() + "/SM5Player/RoleBehavior");
    }

    // only downed and respawned are implemented so far

    [Signal]
    public delegate void OnDownedEventHandler();
    [Signal]
    public delegate void OnRespawnedEventHandler();
    [Signal]
    public delegate void OnZapEventHandler(RoleBehavior other); // zapping someone else
    [Signal]
    public delegate void OnMissileEventHandler(RoleBehavior other);
    [Signal]
    public delegate void OnSpecialEventHandler(); // general special
    [Signal]
    public delegate void OnShootEventHandler();
    [Signal]
    public delegate void OnGetResupplyLivesEventHandler(RoleBehavior other); // getting resupplied, not resupplying
    [Signal]
    public delegate void OnGetResupplyShotsEventHandler(RoleBehavior other); // getting resupplied, not resupplying
}