using Godot;
using System;
using System.Collections.Generic;

public partial class SM5Game : Node3D {
    public List<Player> players = new List<Player>();
    public List<Player> firePlayers = new List<Player>();
    public List<Player> earthPlayers = new List<Player>();
    [Export]
    public double gameTime = 900f; // 15 minutes
    [Export]
    public bool gameStarted = false;

    public override void _Ready() {
        GameManager.Instance.gameController = this;
    }

    public override void _Process(double delta) {
        if (gameStarted) {
            gameTime -= delta;
            if (gameTime <= 0) {
                EndGame();
            }
        }
    }

    public void StartGame() {
        // TODO: Implement this method
        gameStarted = true;

        EmitSignal(SignalName.GameStarted);
    }

    public void EndGame() {
        // TODO: Implement this method

        EmitSignal(SignalName.GameEnded);
    }

    [Signal]
    public delegate void GameStartedEventHandler();

    [Signal]
    public delegate void GameEndedEventHandler();
}