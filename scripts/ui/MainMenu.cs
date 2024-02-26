using Godot;
using System;

public partial class MainMenu : Control {
    private Button hostButton;
    private Button joinButton;
    private LineEdit addressText;
    private LineEdit portText;
    private LineEdit nameText;
    private MultiplayerManager multiplayerManager;

    private PackedScene mainScene;

	public override void _Ready() {
        mainScene = ResourceLoader.Load<PackedScene>("res://scenes/main.tscn");

		hostButton = GetNode<Button>("MarginContainer/VBoxContainer/HostButton");
        joinButton = GetNode<Button>("MarginContainer/VBoxContainer/JoinButton");
        addressText = GetNode<LineEdit>("MarginContainer/VBoxContainer/AddressText");
        portText = GetNode<LineEdit>("MarginContainer/VBoxContainer/PortText");
        nameText = GetNode<LineEdit>("MarginContainer/VBoxContainer/NameText");

        multiplayerManager = GetNode<MultiplayerManager>("/root/MultiplayerManager");

        if (OS.HasFeature("server") || OS.HasFeature("standalone")) {
            OnHostButtonPressed();
        }

        hostButton.Connect("pressed", new Callable(this, nameof(OnHostButtonPressed)));
        joinButton.Connect("pressed", new Callable(this, nameof(OnJoinButtonPressed)));
	}

	public override void _Process(double delta) {
		
	}

    private void OnHostButtonPressed() {
        multiplayerManager.isServer = true;
        multiplayerManager.port = portText.Text.ToInt();
        GameManager.Instance.playerName = nameText.Text;

        GetTree().ChangeSceneToPacked(mainScene);
        multiplayerManager.BeginMultiplayerConnection();
    }

    private void OnJoinButtonPressed() {
        multiplayerManager.isServer = false;
        multiplayerManager.address = addressText.Text;
        multiplayerManager.port = portText.Text.ToInt();
        GD.Print("setting playerName to " + nameText.Text);
        GameManager.Instance.playerName = nameText.Text;

        GetTree().ChangeSceneToPacked(mainScene);
        multiplayerManager.BeginMultiplayerConnection();
    }
}
