using Sandbox;
using System;

public sealed class RotationController : Component
{
	[Property, Range( 0, 90, 0 )] public float RotationClamp { get; set; } = 20;
	[Property, Range( 0, 50, 0 )] public float ResetSpeed { get; set; } = 10;
	[Property, Range( 0, 20, 0 )] public int M_Sensitivity { get; set; } = 10;
	[Property, Range( 0, 20, 0 )] public int KB_Sensitivity { get; set; } = 10;
	[Property] public bool IsResetting { get; set; } = false;

	private Angles Angle;
	private float Roll;
	private float Pitch;
	private float Yaw;

	private void Controls()
	{
		var DeltaX = Input.MouseDelta.x * M_Sensitivity / 100;
		var DeltaY = Input.MouseDelta.y * M_Sensitivity / 100;
		var kbSensitivity = KB_Sensitivity / 8;

		if ( Input.Down( "Tilt Forward" ) ) DeltaY -= kbSensitivity;
		if ( Input.Down( "Tilt Backward" ) ) DeltaY += kbSensitivity;
		if ( Input.Down( "Tilt Left" ) ) DeltaX -= kbSensitivity;
		if ( Input.Down( "Tilt Right" ) ) DeltaX += kbSensitivity;

		if ( Input.Down( "Spin Button" ) ) Angle.yaw += DeltaX;
		if ( Input.Down( "Spin Left" ) ) Angle.yaw -= kbSensitivity;
		if ( Input.Down( "Spin Right" ) ) Angle.yaw += kbSensitivity;

		if ( !( Input.Down( "Spin Button" )
			|| Input.Down( "Spin Left" )
			|| Input.Down( "Spin Right" ) ) )
		{
			if ( Yaw > 135 || Yaw < -135 ) { Pitch += DeltaY; Roll -= DeltaX; } // north
			if ( Yaw < 45 && Yaw > -45 ) { Pitch -= DeltaY; Roll += DeltaX; } // south
			if ( Yaw < -45 && Yaw > -135 ) { Pitch += DeltaX; Roll += DeltaY; } // east
			if ( Yaw > 45 && Yaw < 135 ) { Pitch -= DeltaX; Roll -= DeltaY; } // west
		}

		Angle.roll = Roll.Clamp( -RotationClamp, RotationClamp );
		Angle.pitch = Pitch.Clamp( -RotationClamp, RotationClamp );
	}

	public void RotationReset()
	{
		float targetYaw = Yaw;
		float targetPitch = 0f;
		float targetRoll = 0f;

		if ( Yaw > 135 || Yaw < -135 ) targetYaw = 180f; // north
		else if ( Yaw < 45 && Yaw > -45 ) targetYaw = 0f; // south
		else if ( Yaw < -45 && Yaw > -135 ) targetYaw = -90f; // east
		else if ( Yaw > 45 && Yaw < 135 ) targetYaw = 90f; // west

		Angle.yaw = Yaw.LerpDegreesTo( targetYaw, Time.Delta * ResetSpeed );
		Angle.pitch = Pitch.LerpDegreesTo( targetPitch, Time.Delta * ResetSpeed );
		Angle.roll = Roll.LerpDegreesTo( targetRoll, Time.Delta * ResetSpeed );

		if ( AreAnglesClose( Angle.yaw, targetYaw ) &&
			AreAnglesClose( Angle.pitch, targetPitch ) &&
			AreAnglesClose( Angle.roll, targetRoll ) ) IsResetting = false;
	}

	bool AreAnglesClose( float angle1, float angle2, float tol = 0.01f )
	{
		float diff = angle1 - angle2;
		while ( diff < -180f ) diff += 360f;
		while ( diff > 180f ) diff -= 360f;
		return MathF.Abs( diff ) < tol;
	}

	protected override void OnFixedUpdate()
	{
		Angle = Transform.Rotation.Angles();
		Roll = Angle.roll;
		Pitch = Angle.pitch;
		Yaw = Angle.yaw;

		if ( Input.Down( "Rotation Reset" ) ) IsResetting = true; 

		if ( !IsResetting ) Controls();
		else RotationReset();

		Transform.Rotation = Angle.ToRotation();
	}
}
