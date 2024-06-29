using Sandbox;
using Sandbox.Services;

public sealed class SceneManager : Component, Component.ITriggerListener
{
	[Property] public string LeaderboardName { get; set; } = "highscores";
	[Property] public GameObject SpawnTarget { get; set; }
	[Property] public GameObject GoalTarget { get; set; }
	[Property] public MazeSpawner MazeSpawner { get; set; }
	[Property] public GoalSpawner GoalSpawner { get; set; }
	[Property] public SoundEvent PickupSound { get; set; }
	[Property] public SoundEvent WinSound { get; set; }
	[Property] public SoundEvent DeathSound { get; set; }
	[Property] public long HighScore { get; set; } = 0;
	[Property] public long Golds { get; set; } = 0;
	[Property] public int Wins { get; set; } = 0;

	public TimeSince TimeAlive { get; set; } = 0;
	public Leaderboards.Board Leaderboard;
	public int Update { get; set; } = 1;

	protected override void OnStart()
	{
		FetchLeaderboard();
	}

	protected override void OnDisabled()
	{
		Stats.SetValue( "highscore", Golds );
	}

	async void FetchLeaderboard()
	{
		while ( true )
		{
			Leaderboard = Leaderboards.Get( LeaderboardName );
			Leaderboard.MaxEntries = 6;
			await Leaderboard.Refresh();

			foreach ( var entry in Leaderboard.Entries )
				if ( entry.SteamId == Game.SteamId ) HighScore = ( long ) entry.Value;

			Update = 0;
			await Task.DelayRealtimeSeconds( 20 );
		}
	}
}
