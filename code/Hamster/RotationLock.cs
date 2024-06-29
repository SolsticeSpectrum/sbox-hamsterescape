using Sandbox;

public sealed class RotationLock : Component
{
	private Rigidbody rb { get; set; }

	protected override void OnStart()
	{
		rb = Components.GetInParent<Rigidbody>();
	}

	protected override void OnUpdate()
	{
		float yaw = Transform.Rotation.Yaw();
		float dest = rb.Velocity.WithZ( 0f ).EulerAngles.yaw;
		yaw = yaw.LerpDegreesTo( dest, Time.Delta * 3f );
		Transform.Rotation = new Angles( 0, yaw, 0 );
	}
}
