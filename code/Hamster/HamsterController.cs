using Sandbox;
using Sandbox.Services;

public sealed class HamsterController : Component
{
	[Property] public SceneManager SceneManager { get; set; }

	private void PlaySound( SoundEvent e, float pitch, float volume )
	{
		var sound = Sound.Play( e, WorldPosition );
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
		SceneManager.RotationController.SnapToIdentity();
		WorldPosition = SceneManager.SpawnTarget.WorldPosition;

		Stats.SetValue( "highscore", SceneManager.Golds );

		if ( Components.TryGet<Rigidbody>( out var rigidbody ) )
		{
			rigidbody.Velocity = 0;
			rigidbody.AngularVelocity = 0;
		}
	}

	public void PickupGold()
	{
		if ( SceneManager.PickupSound != null ) PlaySound( SceneManager.PickupSound, 2, 2.5f );
		SceneManager.Golds += 5;
	}

	public void ReachGoal()
	{
		if ( SceneManager.WinSound != null ) PlaySound( SceneManager.WinSound, 2, .1f );
		SceneManager.Golds += 100;
		SceneManager.Wins += 1;
		AsyncRespawn();
	}

	protected override void OnUpdate()
	{
		if ( WorldPosition.z < -SceneManager.KillHeight
			|| WorldPosition.z > SceneManager.KillHeight )
		{
			if ( SceneManager.DeathSound != null ) PlaySound( SceneManager.DeathSound, 1.5f, 1.5f );
			Respawn();
		}

		if ( SceneManager.Golds > SceneManager.HighScore ) SceneManager.HighScore = SceneManager.Golds;
	}
}
