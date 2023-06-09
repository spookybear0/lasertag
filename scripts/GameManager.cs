using Godot;
using System;

public partial class GameManager : Node3D {
	private static GameManager instance;

	public static GameManager Instance {
		get {
			if (instance == null) {
				instance = new GameManager();
			}
			return instance;
		}
	}

	public bool inMenu = false;
	public SM5Game gameController = new SM5Game();
	public Player localPlayer;
	public Hud hud;
}
