using Godot;
using System;
using System.Collections.Generic;

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

	public override void _Ready() {
		instance = this;
	}


	public bool inMenu = false;
	public SM5Game gameController;
	public Player localPlayer;
	public List<Player> players = new List<Player>();
	public string playerName;
	public Hud hud;
}
