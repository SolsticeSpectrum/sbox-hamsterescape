using Sandbox;
using System;
using System.Collections.Generic;

public sealed class GoalSpawner : Component
{
	[Property] public List<float> X_Coords { get; set; } = new List<float>
	{
		-125.4f,
		-75.2f,
		-25f,
		25.2f,
		75.4f,
		125.6f
	};
	[Property, Range( 0, 300, 0 )] public float Y_Coord { get; set; } = 280;
	[Property, Range( 0, 50, 0 )] public float Z_Coord { get; set; } = 40;
	[Property] public bool Test { get; set; } = false;

	protected override void OnStart()
	{
		Goal();
	}

	public void Goal()
	{
		float xCoord = X_Coords[Game.Random.Int( X_Coords.Count - 1 )];

		float yCoord = ( Game.Random.Int( 1 ) == 0 ) ? -Y_Coord : Y_Coord;
		float zCoord = -Z_Coord;

		if ( yCoord == -Y_Coord ) Transform.Rotation = Rotation.FromYaw( 180 );
		else Transform.Rotation = Rotation.Identity;

		GameObject.Enabled = false;
		Transform.Position = new Vector3( xCoord, Test ? 200 : yCoord, zCoord );
		GameObject.Enabled = true;
	}
}
