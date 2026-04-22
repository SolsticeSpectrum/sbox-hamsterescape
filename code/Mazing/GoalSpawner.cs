using Sandbox;
using System;
using System.Collections.Generic;

public sealed class GoalSpawner : Component
{
	[Property] public List<float> XCoords { get; set; } = new List<float>
	{
		-125.4f,
		-75.2f,
		-25f,
		25.2f,
		75.4f,
		125.6f
	};
	[Property, Range( 0, 300 )] public float YCoord { get; set; } = 280;
	[Property, Range( 0, 50 )] public float ZCoord { get; set; } = 40;
	[Property] public bool Test { get; set; } = false;

	protected override void OnStart()
	{
		Goal();
	}

	public void Goal()
	{
		float x = XCoords[Game.Random.Int( XCoords.Count - 1 )];
		float y = ( Game.Random.Int( 1 ) == 0 ) ? -YCoord : YCoord;
		float z = -ZCoord;

		if ( y == -YCoord ) WorldRotation = Rotation.FromYaw( 180 );
		else WorldRotation = Rotation.Identity;

		GameObject.Enabled = false;
		WorldPosition = new Vector3( x, Test ? 200 : y, z );
		GameObject.Enabled = true;
	}
}
