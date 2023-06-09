using Godot;
using System;

public partial class Laser : Node3D {
	public RayCast3D rayCast;
	private CsgBox3D laserMask;


	public override void _Ready() {
		rayCast = GetNode<RayCast3D>("RayCast3D");
		laserMask = GetNode<CsgBox3D>("Combiner/LaserMask");
	}

	public override void _Process(double delta) {
		GodotObject obj = rayCast.GetCollider();
		Vector3 hitpoint = rayCast.GetCollisionPoint();

		if (obj != null)
			laserMask.Size = new Vector3(1, hitpoint.DistanceTo(laserMask.GlobalTransform.Origin)*2, 1);	
		else
			laserMask.Size = new Vector3(1, 1, 1);
	}
}
