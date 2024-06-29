using Sandbox;
using Sandbox.Services;

public sealed class HamsterController : Component, Component.ITriggerListener
{
	[Property] SceneManager SceneManager { get; set; }
	[Property] public float KillHeight { get; set; } = 500;

	private void PlaySound( SoundEvent e, float pitch, float volume )
	{
		var sound = Sound.Play( e, Transform.World.Position );
		sound.Pitch = pitch;
		sound.Volume = volume;
	}

	async void AsyncRespawn()
	{
		await Task.DelayRealtimeSeconds( .5f );
		Respawn();
	}

	private void Respawn()
	{
		SceneManager.MazeSpawner.Maze();
		SceneManager.GoalSpawner.Goal();
		Transform.Position = SceneManager.SpawnTarget.Transform.Position;

		Stats.SetValue( "highscore", SceneManager.Golds );

		if ( Components.TryGet<Rigidbody>( out var rigidbody ) )
		{
			rigidbody.Velocity = 0;
			rigidbody.AngularVelocity = 0;
		}
	}

	protected override void OnUpdate()
	{
		if ( Transform.Position.z < -KillHeight
			|| Transform.Position.z > KillHeight )
		{
			if ( SceneManager.DeathSound != null ) PlaySound( SceneManager.DeathSound, 1.5f, 1 );
			Respawn();
		}

		if ( SceneManager.Golds > SceneManager.HighScore ) SceneManager.HighScore = SceneManager.Golds;
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		if ( other.GameObject == SceneManager.GoalTarget )
		{
			if ( SceneManager.WinSound != null ) PlaySound( SceneManager.WinSound, 2, .1f );
			SceneManager.Golds += 100;
			SceneManager.Wins += 1;
			AsyncRespawn();
		}

		if ( other.Tags.Has( "gold" ) )
		{
			if ( SceneManager.PickupSound != null ) PlaySound( SceneManager.PickupSound, 2, 1.5f );
			SceneManager.Golds += 5;
			other.GameObject.Destroy();
		}
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{
	}
}
