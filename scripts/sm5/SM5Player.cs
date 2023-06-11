using Godot;
using System;

public partial class SM5Player : Node3D {
    private Role _role = Role.Scout;

    [Export]
	public Role role {
        get {
            return _role;
        }
        set {
            _role = value;
            roleBehavior.UpdateRoleSpecificFields(_role);
        }
    }
    [Export]
    public RoleBehavior roleBehavior;
    [Export]
    public Team team = Team.None;

    public Player player;

    public override void _EnterTree() {
        player = GetParent<Player>();

        roleBehavior = GetNode<RoleBehavior>("RoleBehavior");
        roleBehavior.multiplayerId = player.multiplayerId;
        roleBehavior.SetMultiplayerAuthority(player.multiplayerId);
    }

    public override void _Ready() {

    }
    
    public void ChangeRole(Role newRole) {
        role = newRole;
        roleBehavior.UpdateRoleSpecificFields(role);

        GetNode<Hud>("/root/Scene/HudCanvas/Hud").UpdateRoleInfo(this);

        player.Rpc("OnRoleUpdate", (int)role);
    }

    public void ChangeTeam(Team newTeam) {
        team = newTeam;
        roleBehavior.team = team;

        GetNode<Hud>("/root/Scene/HudCanvas/Hud").UpdateRoleInfo(this);

        player.Rpc("OnTeamUpdate", (int)team);
    }
}