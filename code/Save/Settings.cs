using Sandbox;
using System;
using System.Collections.Generic;

public static class Settings
{
	private const string File = "settings.txt";

	private static float defaultVolume;
	private static int defaultMouseSensitivity;
	private static int defaultKeyboardSensitivity;
	private static string defaultLanguage;
	private static Type defaultAlgorithm;

	public static void Load( Layout layout )
	{
		var sm = layout?.SceneManager;
		var rc = sm?.RotationController;
		if ( sm == null || rc == null ) return;

		defaultVolume = layout.Volume;
		defaultMouseSensitivity = rc.MouseSensitivity;
		defaultKeyboardSensitivity = rc.KeyboardSensitivity;
		defaultLanguage = L.CurrentCode;
		defaultAlgorithm = sm.Algorithm;

		if ( !FileSystem.Data.FileExists( File ) ) return;

		foreach ( var line in FileSystem.Data.ReadAllText( File ).Split( '\n' ) )
		{
			var parts = line.Split( ':' );
			if ( parts.Length != 2 ) continue;

			var key = parts[0].Trim();
			var value = parts[1].Trim();

			switch ( key )
			{
				case "mouse_sensitivity": rc.MouseSensitivity = value.ToInt(); break;
				case "keyboard_sensitivity": rc.KeyboardSensitivity = value.ToInt(); break;
				case "music_volume": layout.Volume = value.ToFloat(); break;
				case "language": L.SetLanguage( value ); break;
				case "algorithm": sm.Algorithm = MazeAlgorithms.Resolve( value ); break;
			}
		}
	}

	public static void Reset( Layout layout )
	{
		var sm = layout?.SceneManager;
		var rc = sm?.RotationController;
		if ( sm == null || rc == null ) return;

		layout.Volume = defaultVolume;
		rc.MouseSensitivity = defaultMouseSensitivity;
		rc.KeyboardSensitivity = defaultKeyboardSensitivity;
		L.SetLanguage( defaultLanguage );
		sm.Algorithm = defaultAlgorithm;
	}

	public static void Save( Layout layout )
	{
		var sm = layout?.SceneManager;
		var rc = sm?.RotationController;
		if ( sm == null || rc == null ) return;

		var lines = new List<string>
		{
			"mouse_sensitivity:" + rc.MouseSensitivity,
			"keyboard_sensitivity:" + rc.KeyboardSensitivity,
			"music_volume:" + layout.Volume,
			"language:" + L.CurrentCode,
			"algorithm:" + sm.Algorithm.Name,
		};

		FileSystem.Data.WriteAllText( File, string.Join( "\n", lines ) );
	}

	public static void Delete()
	{
		if ( FileSystem.Data.FileExists( File ) ) FileSystem.Data.DeleteFile( File );
	}
}
