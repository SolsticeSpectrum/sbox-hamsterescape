using Sandbox;
using System;

public sealed class PickupController : Component, Component.ITriggerListener
{
	private int offset = Game.Random.Int( 1, 5 );
	private float yaw = Game.Random.Rotation().Yaw();
	private float origin;

	protected override void OnStart()
	{
		origin = LocalPosition.z;
		LocalRotation = Rotation.FromYaw( yaw );
	}

	protected override void OnUpdate()
	{
		var angles = LocalRotation.Angles();
		angles.yaw += .2f;
		LocalRotation = angles.ToRotation();

		float bob = MathF.Sin( Time.Now * 2 + offset ) * 7;
		LocalPosition = new Vector3( LocalPosition.x,
			LocalPosition.y,
			origin + bob );
	}

	void Component.ITriggerListener.OnTriggerEnter( Collider other )
	{
		var hamster = other.GameObject.Components.Get<HamsterController>();
		if ( hamster is null ) return;
		hamster.PickupGold();
		GameObject.Destroy();
	}
}
