public static class Tutorial
{
	private const string File = "tutorial.txt";
	private const string Finished = "finished";

	public static bool IsFinished
		=> FileSystem.Data.FileExists( File ) && FileSystem.Data.ReadAllText( File ) == Finished;

	public static void Finish()
	{
		if ( !FileSystem.Data.FileExists( File ) ) FileSystem.Data.WriteAllText( File, Finished );
	}

	public static void Reset()
	{
		if ( FileSystem.Data.FileExists( File ) ) FileSystem.Data.DeleteFile( File );
	}
}
