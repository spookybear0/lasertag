using Godot;
using System;

public partial class Scene : Node3D {
	public override void _Ready() {
		GetNode<MultiplayerManager>("MultiplayerManager").StartServerCamera();
	}

	public override void _Process(double delta) {

	}

    public override void _UnhandledInput(InputEvent @event) {
		if (Godot.Input.IsActionJustPressed("toggle_fullscreen")) {
			if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen)
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
			else
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		}
    }
}
