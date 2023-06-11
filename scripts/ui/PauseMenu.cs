using Godot;
using System;
using System.Collections.Generic;

public partial class PauseMenu : PanelContainer {
	private LineEdit sensitivityText;
	private OptionButton roleOption;
	private OptionButton teamOption;
	private Button resumeButton;

	private Button startButton;
	private Button endButton;

	public override void _Ready() {
		sensitivityText = GetNode<LineEdit>("MarginContainer/VBoxContainer/SettingsSplit/Right/SensitivityText");
		roleOption = GetNode<OptionButton>("MarginContainer/VBoxContainer/SettingsSplit/Right/RoleOption");
		teamOption = GetNode<OptionButton>("MarginContainer/VBoxContainer/SettingsSplit/Right/TeamOption");
		resumeButton = GetNode<Button>("MarginContainer/VBoxContainer/ResumeButton");

		startButton = GetNode<Button>("MarginContainer/VBoxContainer/GameControlSplit/Left/StartButton");
		endButton = GetNode<Button>("MarginContainer/VBoxContainer/GameControlSplit/Right/EndButton");


		sensitivityText.Connect("text_changed", new Callable(this, nameof(OnSensitivityTextChanged)));
		roleOption.Connect("item_selected", new Callable(this, nameof(OnRoleOptionSelected)));
		teamOption.Connect("item_selected", new Callable(this, nameof(OnTeamOptionSelected)));
		resumeButton.Connect("pressed", new Callable(this, nameof(OnResumeButtonPressed)));

		startButton.Connect("pressed", new Callable(this, nameof(OnStartButtonPressed)));
		endButton.Connect("pressed", new Callable(this, nameof(OnEndButtonPressed)));

		if (MultiplayerManager.Instance.isServer) {
			GameManager.Instance.inMenu = false;
		}
		else {
			CloseMenu();
		}
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (Godot.Input.IsActionJustPressed("menu")) {
			if (GameManager.Instance.inMenu) {
				CloseMenu();
			}
			else {
				OpenMenu();
			}
		}
	}

	public void OpenMenu() {
		if (MultiplayerManager.Instance.isServer) {
			return;
		}

		GameManager.Instance.inMenu = true;

		Player player = GameManager.Instance.localPlayer;

		GetParent<CanvasLayer>().Visible = true;
		MouseFilter = MouseFilterEnum.Stop;

		player.setMouseCapture(false);
	}

	public void CloseMenu() {
		if (MultiplayerManager.Instance.isServer) {
			return;
		}

		GameManager.Instance.inMenu = false;

		Player player = GameManager.Instance.localPlayer;

		GetParent<CanvasLayer>().Visible = false;
		MouseFilter = MouseFilterEnum.Ignore;

		if (player != null)
			player.setMouseCapture(true);
	}

	public void OnSensitivityTextChanged(string newText) {
		if (newText.Length > 0) {
			float newSensitivity = newText.ToFloat();

			if (newSensitivity > 0) {
				Player player = GameManager.Instance.localPlayer;

				player.sensitivity = newSensitivity;
			}
		}
	}

	public void OnRoleOptionSelected(int index) {
		Player player = GameManager.Instance.localPlayer;
		Role role = Role.Scout;

		switch (index) {
			case 0:
				role = Role.Commander;
				break;
			case 1:
				role = Role.Heavy;
				break;
			case 2:
				role = Role.Scout;
				break;
			case 3:
				role = Role.Ammo;
				break;
			case 4:
				role = Role.Medic;
				break;
		}

		GD.Print(role);
		GD.Print(player);
		GD.Print(player.sm5Player);

		player.sm5Player.ChangeRole(role);
	}

	public void OnTeamOptionSelected(int index) {
		Player player = GameManager.Instance.localPlayer;
		Team team = Team.None;

		switch (index) {
			case 0:
				team = Team.Fire;
				break;
			case 1:
				team = Team.Earth;
				break;
		}

		player.sm5Player.ChangeTeam(team);
	}

	public void OnResumeButtonPressed() {
		CloseMenu();
	}

	public void OnStartButtonPressed() {
		GameManager.Instance.gameController.StartGame();
	}

	public void OnEndButtonPressed() {
		GameManager.Instance.gameController.EndGame();
	}
}