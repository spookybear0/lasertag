using Godot;
using System;

public partial class MultiplayerManager : Node3D {
	public int playerCount = 0;
	public int port;
	public string address;
	public bool isServer;
	public ENetMultiplayerPeer peer;

	private PackedScene playerClass;

	private static MultiplayerManager instance;

	public static MultiplayerManager Instance {
		get {
			if (instance == null) {
				instance = new MultiplayerManager();
			}
			return instance;
		}
	}

    public override void _Ready() {
        playerClass = (PackedScene)ResourceLoader.Load("res://objects/player.tscn");
		peer = new ENetMultiplayerPeer();
    }

	public void BeginMultiplayerConnection() {
        if (isServer) {
            GD.Print("Starting server...");
            StartServer();
        }
        else {
            GD.Print("Connecting to server...");
            StartClient();
        }

        Multiplayer.MultiplayerPeer = peer;
	}

    private void StartServer() {
        // Start the server

		peer.CreateServer(port, 32);

    	Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;

        SetupUPNP();
    }

    public void StartClient() {
        // Connect to the server
        peer.CreateClient(address, port);
    }

	// SERVER
    public void PeerConnected(long id) {
        GD.Print("Peer connected: " + id);
        // Create a new player
        Player player = playerClass.Instantiate<Player>();
        player.Position = new Vector3(0, 0, 0);
        player.Name = "Player" + id;
        player.multiplayerId = (int)id;
		player.SetMultiplayerAuthority((int)id);
        GetNode<Node3D>("/root/Scene").AddChild(player);
    }

	// SERVER
    public void PeerDisconnected(long id) {
        GD.Print("Player disconnected: " + id);
        // Remove the player
        Player player = GetNode<Node3D>("/root/Scene").GetNode<Player>("Player" + id);
        player.QueueFree();
    }

    public void SetupUPNP() {
        Upnp upnp = new Upnp();
        upnp.Discover();
        upnp.AddPortMapping(port, port, "lasertag", "TCP");
        upnp.AddPortMapping(port, port, "lasertag", "UDP");
    }

    public void StartServerCamera() {
        GetNode<Camera3D>("ServerCamera").MakeCurrent();
    }
}
