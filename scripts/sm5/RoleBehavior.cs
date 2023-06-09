using Godot;
using System;

// TODOs
// bases
// specials

public abstract partial class RoleBehavior : Node3D {
    [Export]
    public abstract bool downed { get; set; }
    [Export]
    public abstract bool dead { get; set; }

    [Export]
    public abstract int shotPower { get; set; }
    [Export]
    public abstract int initialHitPoints { get; set; }
    [Export]
    public abstract int hitPoints { get; set; }

    [Export]
    public abstract int shotsInitial { get; set; }
    [Export]
    public abstract int shotsResupply { get; set; }
    [Export]
    public abstract int shotsMax { get; set; }
    [Export]
    public abstract int shots { get; set; }

    [Export]
    public abstract int missilesInitial { get; set; }
    [Export]
    public abstract int missiles { get; set; }

    [Export]
    public abstract int livesInitial { get; set; }
    [Export]
    public abstract int livesResupply { get; set; }
    [Export]
    public abstract int livesMax { get; set; }
    [Export]
    public abstract int lives { get; set; }

    [Export]
    public abstract int score { get; set; }
    [Export]
    public abstract int specialPoints { get; set; }
    [Export]
    public abstract Team team { get; set; }

    public virtual void ResupplyShots(RoleBehavior other) {
        // default does nothing since we aren't ammo
    }

    public virtual void OnResupplyShots(RoleBehavior other) { // getting resupplied
        shots += shotsResupply;
        if (shots > shotsMax) {
            shots = shotsMax;
        }
    }

    public virtual void ResupplyLives(RoleBehavior other) {
        // default does nothing since we aren't medic
    }

    public virtual void OnResupplyLives(RoleBehavior other) { // getting resupplied
        lives += livesResupply;
        if (lives > livesMax) {
            lives = livesMax;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=true)]
    public virtual void Zap(RoleBehavior other) { // shoot them
        other.OnZapped(this);
    }

    public virtual void OnZapped(RoleBehavior other) { // when we get shot
        hitPoints -= other.shotPower;
        if (hitPoints <= 0) {
            hitPoints = 0;
            downed = true;
            lives--;
            score -= 20;
            if (team == other.team) {
                other.score -= 100;
            }
            else {
                other.score += 100;
                other.specialPoints += 1;
            }
            
            if (lives <= 0) {
                dead = true;
            }
        }
    }

    public virtual void Missile(RoleBehavior other) {
        other.OnMissiled(this);
    }

    public virtual void OnMissiled(RoleBehavior other) {
        // remove two lives instantly
        lives -= 2;
        downed = true;

        other.score += 500;
        other.specialPoints += 2;
        score -= 100;

        if (lives <= 0) {
            dead = true;
        }
    }

    public abstract void UseSpecial();
}

// commander

public partial class CommanderBehavior : RoleBehavior {
    private int _hitPoints = 3; // dynamic will change
    private int _shots = 30; // dynamic will change
    private int _missiles = 5; // dynamic will change
    private int _lives = 15; // dynamic will change
    private int _score = 0; // dynamic will change
    private int _specialPoints = 0; // dynamic will change
    private Team _team = Team.None; // dynamic will change

    // normal fields

    [Export]
    public override bool downed { get; set; } = false;
    [Export]
    public override bool dead { get; set; } = false;

    [Export]
    public override int shotPower { get; set; } = 2;
    [Export]
    public override int initialHitPoints { get; set; } = 3;
    [Export]
    public override int hitPoints {
        get {
            return _hitPoints;
        }
        set {
            _hitPoints = value;
            GameManager.Instance.hud.roleHitpoints.Text = value.ToString() + "/" + initialHitPoints.ToString();
        }
    }

    [Export]
    public override int shotsInitial { get; set; } = 30;
    [Export]
    public override int shotsResupply { get; set; } = 5;
    [Export]
    public override int shotsMax { get; set; } = 60;
    [Export]
    public override int shots {
        get {
            return _shots;
        }
        set {
            _shots = value;
            GameManager.Instance.hud.shotsValue.Text = value.ToString();
        }
    }

    [Export]
    public override int missilesInitial { get; set; } = 5;
    [Export]
    public override int missiles {
        get {
            return _missiles;
        }
        set {
            _missiles = value;
            GameManager.Instance.hud.missilesValue.Text = value.ToString();
        }
    }

    [Export]
    public override int livesInitial { get; set; } = 15;
    [Export]
    public override int livesResupply { get; set; } = 5;
    [Export]
    public override int livesMax { get; set; } = 30;
    [Export]
    public override int lives {
        get {
            return _lives;
        }
        set {
            _lives = value;
            GameManager.Instance.hud.livesValue.Text = value.ToString();
        }
    }

    [Export]
    public override int score {
        get {
            return _score;
        }
        set {
            _score = value;
            GameManager.Instance.hud.scoreValue.Text = value.ToString();
        }
    }
    [Export]
    public override int specialPoints {
        get {
            return _specialPoints;
        }
        set {
            _specialPoints = value;
            GameManager.Instance.hud.specialsValue.Text = value.ToString();
        }
    }
    [Export]
    public override Team team {
        get {
            return _team;
        }
        set {
            _team = value;
            GameManager.Instance.hud.teamLabel.Text = value.ToString() + " Team";
        }
    }

    public override void UseSpecial() {
        // TODO: nuke
    }
}

// heavy

public partial class HeavyBehavior : RoleBehavior {
    private int _hitPoints = 3; // dynamic will change
    private int _shots = 20; // dynamic will change
    private int _missiles = 5; // dynamic will change
    private int _lives = 10; // dynamic will change
    private int _score = 0; // dynamic will change
    private int _specialPoints = 0; // dynamic will change
    private Team _team = Team.None; // dynamic will change

    // normal fields

    [Export]
    public override bool downed { get; set; } = false;
    [Export]
    public override bool dead { get; set; } = false;

    [Export]
    public override int shotPower { get; set; } = 3;
    [Export]
    public override int initialHitPoints { get; set; } = 3;
    [Export]
    public override int hitPoints {
        get {
            return _hitPoints;
        }
        set {
            _hitPoints = value;
            GameManager.Instance.hud.roleHitpoints.Text = value.ToString() + "/" + initialHitPoints.ToString();
        }
    }

    [Export]
    public override int shotsInitial { get; set; } = 20;
    [Export]
    public override int shotsResupply { get; set; } = 5;
    [Export]
    public override int shotsMax { get; set; } = 40;
    [Export]
    public override int shots {
        get {
            return _shots;
        }
        set {
            _shots = value;
            GameManager.Instance.hud.shotsValue.Text = value.ToString();
        }
    }

    [Export]
    public override int missilesInitial { get; set; } = 5;
    [Export]
    public override int missiles {
        get {
            return _missiles;
        }
        set {
            _missiles = value;
            GameManager.Instance.hud.missilesValue.Text = value.ToString();
        }
    }

    [Export]
    public override int livesInitial { get; set; } = 10;
    [Export]
    public override int livesResupply { get; set; } = 3;
    [Export]
    public override int livesMax { get; set; } = 20;
    [Export]
    public override int lives {
        get {
            return _lives;
        }
        set {
            _lives = value;
            GameManager.Instance.hud.livesValue.Text = value.ToString();
        }
    }

    [Export]
    public override int score {
        get {
            return _score;
        }
        set {
            _score = value;
            GameManager.Instance.hud.scoreValue.Text = value.ToString();
        }
    }
    [Export]
    public override int specialPoints {
        get {
            return _specialPoints;
        }
        set {
            _specialPoints = value;
            GameManager.Instance.hud.specialsValue.Text = value.ToString();
        }
    }
    [Export]
    public override Team team {
        get {
            return _team;
        }
        set {
            _team = value;
            GameManager.Instance.hud.teamLabel.Text = value.ToString() + " Team";
        }
    }

    public override void UseSpecial() {
        // none
    }
}

// scout

public partial class ScoutBehavior : RoleBehavior {
    private int _hitPoints = 1; // dynamic will change
    private int _shots = 30; // dynamic will change
    private int _missiles = 0; // dynamic will change
    private int _lives = 15; // dynamic will change
    private int _score = 0; // dynamic will change
    private int _specialPoints = 0; // dynamic will change
    private Team _team = Team.None; // dynamic will change

    [Export]
    public override bool downed { get; set; } = false;
    [Export]
    public override bool dead { get; set; } = false;

    [Export]
    public override int shotPower { get; set; } = 1;
    [Export]
    public override int initialHitPoints { get; set; } = 1;
    [Export]
    public override int hitPoints {
        get {
            return _hitPoints;
        }
        set {
            _hitPoints = value;
            GameManager.Instance.hud.roleHitpoints.Text = value.ToString() + "/" + initialHitPoints.ToString();
        }
    }

    [Export]
    public override int shotsInitial { get; set; } = 30;
    [Export]
    public override int shotsResupply { get; set; } = 10;
    [Export]
    public override int shotsMax { get; set; } = 60;
    [Export]
    public override int shots {
        get {
            return _shots;
        }
        set {
            _shots = value;
            GameManager.Instance.hud.shotsValue.Text = value.ToString();
        }
    }
    
    [Export]
    public override int missilesInitial { get; set; } = 0;
    [Export]
    public override int missiles {
        get {
            return _missiles;
        }
        set {
            _missiles = value;
            GameManager.Instance.hud.missilesValue.Text = value.ToString();
        }
    }

    [Export]
    public override int livesInitial { get; set; } = 15;
    [Export]
    public override int livesResupply { get; set; } = 5;
    [Export]
    public override int livesMax { get; set; } = 30;
    [Export]
    public override int lives {
        get {
            return _lives;
        }
        set {
            _lives = value;
            GameManager.Instance.hud.livesValue.Text = value.ToString();
        }
    }

    [Export]
    public override int score {
        get {
            return _score;
        }
        set {
            _score = value;
            GameManager.Instance.hud.scoreValue.Text = value.ToString();
        }
    }
    [Export]
    public override int specialPoints {
        get {
            return _specialPoints;
        }
        set {
            _specialPoints = value;
            GameManager.Instance.hud.specialsValue.Text = value.ToString();
        }
    }
    [Export]
    public override Team team {
        get {
            return _team;
        }
        set {
            _team = value;
            GameManager.Instance.hud.teamLabel.Text = value.ToString() + " Team";
        }
    }

    public override void UseSpecial() {
        // TODO: rapid fire
    }
}

// ammo

public partial class AmmoBehavior : RoleBehavior {
    private int _hitPoints = 1; // dynamic will change
    private int _shots = 15; // dynamic will change
    private int _missiles = 0; // dynamic will change
    private int _lives = 10; // dynamic will change
    private int _score = 0; // dynamic will change
    private int _specialPoints = 0; // dynamic will change
    private Team _team = Team.None; // dynamic will change

    [Export]
    public override bool downed { get; set; } = false;
    [Export]
    public override bool dead { get; set; } = false;

    [Export]
    public override int shotPower { get; set; } = 1;
    [Export]
    public override int initialHitPoints { get; set; } = 1;
    [Export]
    public override int hitPoints {
        get {
            return _hitPoints;
        }
        set {
            _hitPoints = value;
            GameManager.Instance.hud.roleHitpoints.Text = value.ToString() + "/" + initialHitPoints.ToString();
        }
    }

    [Export]
    public override int shotsInitial { get; set; } = 15; // unlimited
    [Export]
    public override int shotsResupply { get; set; } = 0;
    [Export]
    public override int shotsMax { get; set; } = 15; // unlimited
    [Export]
    public override int shots {
        get {
            return _shots;
        }
        set {
            _shots = value;
            GameManager.Instance.hud.shotsValue.Text = value.ToString();
        }
    }

    [Export]
    public override int missilesInitial { get; set; } = 0;
    [Export]
    public override int missiles {
        get {
            return _missiles;
        }
        set {
            _missiles = value;
            GameManager.Instance.hud.missilesValue.Text = value.ToString();
        }
    }

    [Export]
    public override int livesInitial { get; set; } = 10;
    [Export]
    public override int livesResupply { get; set; } = 3;
    [Export]
    public override int livesMax { get; set; } = 20;
    [Export]
    public override int lives {
        get {
            return _lives;
        }
        set {
            _lives = value;
            GameManager.Instance.hud.livesValue.Text = value.ToString();
        }
    }

    [Export]
    public override int score {
        get {
            return _score;
        }
        set {
            _score = value;
            GameManager.Instance.hud.scoreValue.Text = value.ToString();
        }
    }
    [Export]
    public override int specialPoints {
        get {
            return _specialPoints;
        }
        set {
            _specialPoints = value;
            GameManager.Instance.hud.specialsValue.Text = value.ToString();
        }
    }
    [Export]
    public override Team team {
        get {
            return _team;
        }
        set {
            _team = value;
            GameManager.Instance.hud.teamLabel.Text = value.ToString() + " Team";
        }
    }

    public override void ResupplyShots(RoleBehavior other) {
        other.OnResupplyShots(this);
    }

    public override void Zap(RoleBehavior other) {
        if (team == other.team && !other.downed && !other.dead && other.GetType() != typeof(AmmoBehavior)) {
            other.ResupplyShots(this);
        }
        else {
            other.OnZapped(this);
        }
    }

    public override void UseSpecial() {
        // ammo boost
    }
}

// medic

public partial class MedicBehavior : RoleBehavior {
    private int _hitPoints = 1; // dynamic will change
    private int _shots = 15; // dynamic will change
    private int _missiles = 0; // dynamic will change
    private int _lives = 20; // dynamic will change
    private int _score = 0; // dynamic will change
    private int _specialPoints = 0; // dynamic will change
    private Team _team = Team.None; // dynamic will change

    [Export]
    public override bool downed { get; set; } = false;
    [Export]
    public override bool dead { get; set; } = false;

    [Export]
    public override int shotPower { get; set; } = 1;
    [Export]
    public override int initialHitPoints { get; set; } = 1;
    [Export]
    public override int hitPoints {
        get {
            return _hitPoints;
        }
        set {
            _hitPoints = value;
            GameManager.Instance.hud.roleHitpoints.Text = value.ToString() + "/" + initialHitPoints.ToString();
        }
    }

    [Export]
    public override int shotsInitial { get; set; } = 15;
    [Export]
    public override int shotsResupply { get; set; } = 5;
    [Export]
    public override int shotsMax { get; set; } = 30; // dynamic will change
    [Export]
    public override int shots {
        get {
            return _shots;
        }
        set {
            _shots = value;
            GameManager.Instance.hud.shotsValue.Text = value.ToString();
        }
    }

    [Export]
    public override int missilesInitial { get; set; } = 0;
    [Export]
    public override int missiles {
        get {
            return _missiles;
        }
        set {
            _missiles = value;
            GameManager.Instance.hud.missilesValue.Text = value.ToString();
        }
    }

    [Export]
    public override int livesInitial { get; set; } = 20;
    [Export]
    public override int livesResupply { get; set; } = 0; // cant heal self
    [Export]
    public override int livesMax { get; set; } = 20;
    [Export]
    public override int lives {
        get {
            return _lives;
        }
        set {
            _lives = value;
            GameManager.Instance.hud.livesValue.Text = value.ToString();
        }
    }

    [Export]
    public override int score {
        get {
            return _score;
        }
        set {
            _score = value;
            GameManager.Instance.hud.scoreValue.Text = value.ToString();
        }
    }
    [Export]
    public override int specialPoints {
        get {
            return _specialPoints;
        }
        set {
            _specialPoints = value;
            GameManager.Instance.hud.specialsValue.Text = value.ToString();
        }
    }
    [Export]
    public override Team team {
        get {
            return _team;
        }
        set {
            _team = value;
            GameManager.Instance.hud.teamLabel.Text = value.ToString() + " Team";
        }
    }

    public override void ResupplyLives(RoleBehavior other) {
        other.OnResupplyLives(this);
    }

    public override void Zap(RoleBehavior other) {
        if (team == other.team && !other.downed && !other.dead && other.GetType() != typeof(MedicBehavior)) {
            other.ResupplyLives(this);
        }
        else {
            other.OnZapped(this);
        }
    }

    public override void UseSpecial() {
        // life boost
    }
}