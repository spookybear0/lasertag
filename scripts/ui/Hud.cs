using Godot;
using System;

public partial class Hud : Control {
	public Label roleLabel;
	public TextureRect roleIcon;
	public Label roleHitpoints;
	
	public Label scoreValue;
	public Label livesValue;
	public Label shotsValue;
	public Label missilesValue;
	public Label specialsValue;

	public Label teamLabel;

	public Texture2D commander_texture;
	public Texture2D heavy_texture;
	public Texture2D scout_texture;
	public Texture2D ammo_texture;
	public Texture2D medic_texture;

	public override void _Ready() {
		roleLabel = GetNode<Label>("LeftPanel/Margin/VBox/Role/Label");
		roleIcon = GetNode<TextureRect>("LeftPanel/Margin/VBox/Role/Icon");
		roleHitpoints = GetNode<Label>("LeftPanel/Margin/VBox/Role/Hitpoints");

		scoreValue = GetNode<Label>("LeftPanel/Margin/VBox/Split/Value/Score");
		livesValue = GetNode<Label>("LeftPanel/Margin/VBox/Split/Value/Lives");
		shotsValue = GetNode<Label>("LeftPanel/Margin/VBox/Split/Value/Shots");
		missilesValue = GetNode<Label>("LeftPanel/Margin/VBox/Split/Value/Missiles");
		specialsValue = GetNode<Label>("LeftPanel/Margin/VBox/Split/Value/Specials");

		teamLabel = GetNode<Label>("LeftPanel/Margin/VBox/TeamLabel");


		commander_texture = GD.Load<Texture2D>("res://assets/roles/commander_big.png");
		heavy_texture = GD.Load<Texture2D>("res://assets/roles/heavy_big.png");
		scout_texture = GD.Load<Texture2D>("res://assets/roles/scout_big.png");
		ammo_texture = GD.Load<Texture2D>("res://assets/roles/ammo_big.png");
		medic_texture = GD.Load<Texture2D>("res://assets/roles/medic_big.png");

		GameManager.Instance.hud = GetNode<Hud>(GetPath());

		if (Multiplayer.GetUniqueId() == 1) {
			GetParent<CanvasLayer>().Visible = false;
		}
	}

	public void UpdatePlayerInfo(SM5Player player) { // rarely used
		roleHitpoints.Text = player.roleBehavior.hitPoints.ToString() + "/" + player.roleBehavior.initialHitPoints.ToString();
		
		scoreValue.Text = player.roleBehavior.score.ToString();
		livesValue.Text = player.roleBehavior.lives.ToString();
		shotsValue.Text = player.roleBehavior.shots.ToString();
		missilesValue.Text = player.roleBehavior.missiles.ToString();
		specialsValue.Text = player.roleBehavior.specialPoints.ToString();

		teamLabel.Text = player.team.ToString() + " Team";
	}

	public void UpdateRoleInfo(SM5Player player) {
		Texture2D texture = scout_texture;

		switch (player.role) {
			case Role.Commander:
				texture = commander_texture;
				break;
			case Role.Heavy:
				texture = heavy_texture;
				break;
			case Role.Scout:
				texture = scout_texture;
				break;
			case Role.Ammo:
				texture = ammo_texture;
				break;
			case Role.Medic:
				texture = medic_texture;
				break;
		}

		roleIcon.Texture = texture;
		roleLabel.Text = player.role.ToString();
		roleHitpoints.Text = player.roleBehavior.hitPoints.ToString() + "/" + player.roleBehavior.initialHitPoints.ToString();

		UpdatePlayerInfo(player);
	}
}
