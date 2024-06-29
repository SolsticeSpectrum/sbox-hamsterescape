using Sandbox;
using System;

public sealed class RotationController : Component
{
	[Property, Range( 0, 90, 0 )] public float RotationClamp { get; set; } = 20;
	[Property, Range( 0, 20, 0 )] public int M_Sensitivity { get; set; } = 10;
	[Property, Range( 0, 20, 0 )] public int KB_Sensitivity { get; set; } = 10;

	protected override void OnFixedUpdate()
	{
		var Angles = Transform.Rotation.Angles();
		var Roll = Angles.roll;
		var Pitch = Angles.pitch;
		var Yaw = Angles.yaw;
		var DeltaX = Input.MouseDelta.x * M_Sensitivity / 100;
		var DeltaY = Input.MouseDelta.y * M_Sensitivity / 100;
		var kbSensitivity = KB_Sensitivity / 8;

		if ( Input.Down( "Tilt Forward" ) ) DeltaY -= kbSensitivity;
		if ( Input.Down( "Tilt Backward" ) ) DeltaY += kbSensitivity;
		if ( Input.Down( "Tilt Left" ) ) DeltaX -= kbSensitivity;
		if ( Input.Down( "Tilt Right" ) ) DeltaX += kbSensitivity;

		if ( Input.Down( "Spin Button" ) ) Angles.yaw += DeltaX;
		if ( Input.Down( "Spin Left" ) ) Angles.yaw -= kbSensitivity;
		if ( Input.Down( "Spin Right" ) ) Angles.yaw += kbSensitivity;

		if ( !( Input.Down( "Spin Button" )
			|| Input.Down( "Spin Left" )
			|| Input.Down( "Spin Right" ) ) ) 
		{
			if ( Yaw > 135 || Yaw < -135 ) { Pitch += DeltaY; Roll -= DeltaX; } // north
			if ( Yaw < 45 && Yaw > -45 ) { Pitch -= DeltaY; Roll += DeltaX; } // south
			if ( Yaw < -45 && Yaw > -135 ) { Pitch += DeltaX; Roll += DeltaY; } // east
			if ( Yaw > 45 && Yaw < 135 ) { Pitch -= DeltaX; Roll -= DeltaY; } // west
		}

		Angles.roll = Roll.Clamp( -RotationClamp, RotationClamp );
		Angles.pitch = Pitch.Clamp( -RotationClamp, RotationClamp );

		Transform.Rotation = Angles.ToRotation();
	}
}
