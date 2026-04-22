using Sandbox;
using System;

public sealed class RotationController : Component
{
	[Property, Range( 0, 90 )] public float RotationClamp { get; set; } = 20;
	[Property, Range( 0, 50 )] public float ResetSpeed { get; set; } = 10;
	[Property, Range( 0, 20 )] public int MouseSensitivity { get; set; } = 10;
	[Property, Range( 0, 20 )] public int KeyboardSensitivity { get; set; } = 10;
	[Property] public bool IsResetting { get; set; } = false;

	private Angles Angle;
	private float Roll;
	private float Pitch;
	private float Yaw;

	private void Controls()
	{
		var deltaX = Input.MouseDelta.x * MouseSensitivity / 100;
		var deltaY = Input.MouseDelta.y * MouseSensitivity / 100;
		var step = KeyboardSensitivity / 8;

		if ( Input.Down( "Tilt Forward" ) ) deltaY -= step;
		if ( Input.Down( "Tilt Backward" ) ) deltaY += step;
		if ( Input.Down( "Tilt Left" ) ) deltaX -= step;
		if ( Input.Down( "Tilt Right" ) ) deltaX += step;

		if ( Input.Down( "Spin Button" ) ) Angle.yaw += deltaX;
		if ( Input.Down( "Spin Left" ) ) Angle.yaw -= step;
		if ( Input.Down( "Spin Right" ) ) Angle.yaw += step;

		if ( !( Input.Down( "Spin Button" )
			|| Input.Down( "Spin Left" )
			|| Input.Down( "Spin Right" ) ) )
		{
			if ( Yaw > 135 || Yaw < -135 ) { Pitch += deltaY; Roll -= deltaX; } // north
			if ( Yaw < 45 && Yaw > -45 ) { Pitch -= deltaY; Roll += deltaX; } // south
			if ( Yaw < -45 && Yaw > -135 ) { Pitch += deltaX; Roll += deltaY; } // east
			if ( Yaw > 45 && Yaw < 135 ) { Pitch -= deltaX; Roll -= deltaY; } // west
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
		if ( Input.Pressed( "Rotation Reset" ) )
		{
			SnapToIdentity();
			return;
		}

		Angle = WorldRotation.Angles();
		Roll = Angle.roll;
		Pitch = Angle.pitch;
		Yaw = Angle.yaw;

		if ( !IsResetting ) Controls();
		else RotationReset();

		WorldRotation = Angle.ToRotation();
	}

	public void SnapToIdentity()
	{
		IsResetting = false;
		Angle = Angles.Zero;
		Roll = 0;
		Pitch = 0;
		Yaw = 0;
		WorldRotation = Rotation.Identity;
	}
}
