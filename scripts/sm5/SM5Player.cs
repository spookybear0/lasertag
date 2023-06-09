using Godot;
using System;

public partial class SM5Player : Node3D {
    [Export]
	public Role role = Role.Scout;
    [Export]
    public RoleBehavior roleBehavior;
    [Export]
    public Team team = Team.None;

    public Player player;

    private MultiplayerSynchronizer synchronizer;

    public override void _Ready() {
        player = GetParent<Player>();

        roleBehavior = new ScoutBehavior();
        roleBehavior.Name = "RoleBehavior" + player.multiplayerId.ToString();

        AddChild(roleBehavior);

        synchronizer = GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer");
        SynchronizeProperties();
    }
    
    public void ChangeRole(Role newRole) {
        UnsynchronizeProperties();
        roleBehavior.QueueFree();

        role = newRole;
        switch (role) {
            case Role.Commander:
                roleBehavior = new CommanderBehavior();
                roleBehavior.Name = "RoleBehavior" + player.multiplayerId.ToString();
                break;
            case Role.Heavy:
                roleBehavior = new HeavyBehavior();
                roleBehavior.Name = "RoleBehavior" + player.multiplayerId.ToString();
                break;
            case Role.Scout:
                roleBehavior = new ScoutBehavior();
                roleBehavior.Name = "RoleBehavior" + player.multiplayerId.ToString();
                break;
            case Role.Ammo:
                roleBehavior = new AmmoBehavior();
                roleBehavior.Name = "RoleBehavior" + player.multiplayerId.ToString();
                break;
            case Role.Medic:
                roleBehavior = new MedicBehavior();
                roleBehavior.Name = "RoleBehavior" + player.multiplayerId.ToString();
                break;
        }

        roleBehavior.team = team;

        AddChild(roleBehavior);
        SynchronizeProperties();

        GetNode<Hud>("/root/Scene/HudCanvas/Hud").UpdateRoleInfo(this);
    }

    public void ChangeTeam(Team newTeam) {
        team = newTeam;
        roleBehavior.team = team;

        GetNode<Hud>("/root/Scene/HudCanvas/Hud").UpdateRoleInfo(this);
    }

    private void SynchronizeProperties() {
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:downed");
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:dead");
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:shotPower");
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:initialHitPoints");
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:hitPoints");
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:shotsInitial");
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:shotsResupply");
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:shotsMax");
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:shots");
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:missilesInitial");
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:livesInitial");
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:livesResupply");
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:livesMax");
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:lives");
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:score");
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:specialPoints");
        synchronizer.ReplicationConfig.AddProperty("RoleBehavior:team");
    }

    private void UnsynchronizeProperties() {
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:downed");
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:dead");
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:shotPower");
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:initialHitPoints");
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:hitPoints");
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:shotsInitial");
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:shotsResupply");
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:shotsMax");
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:shots");
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:missilesInitial");
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:livesInitial");
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:livesResupply");
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:livesMax");
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:lives");
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:score");
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:specialPoints");
        synchronizer.ReplicationConfig.RemoveProperty("RoleBehavior:team");
    }
}