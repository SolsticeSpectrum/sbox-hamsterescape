using Sandbox;
using System;

public sealed class PickupController : Component
{
	private int rngOffset = Game.Random.Int( 1, 5 );
	private float rngYaw = Game.Random.Rotation().Yaw();
	private float initPos;

	protected override void OnStart()
	{
		initPos = Transform.LocalPosition.z;
		Transform.LocalRotation = Rotation.FromYaw( rngYaw );
	}

	protected override void OnUpdate()
	{
		var angles = Transform.LocalRotation.Angles();
		angles.yaw += .2f;
		Transform.LocalRotation = angles.ToRotation();

		float verticalOffset = MathF.Sin( Time.Now * 2 + rngOffset ) * 7;
		Transform.LocalPosition = new Vector3( Transform.LocalPosition.x, 
			Transform.LocalPosition.y, 
			initPos + verticalOffset );
	}
}
