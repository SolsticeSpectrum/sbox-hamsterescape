using Sandbox;
using System.Collections.Generic;
using System.Text;

public static class L
{
	public const string DefaultCode = "en";

	public static readonly IReadOnlyList<Entry> Available = new[]
	{
		new Entry( "en", "English" ),
		new Entry( "cs", "Čeština" ),
		new Entry( "de", "Deutsch" ),
		new Entry( "es", "Español" ),
		new Entry( "fr", "Français" ),
		new Entry( "it", "Italiano" ),
		new Entry( "ru", "Русский" ),
		new Entry( "be", "Беларуская" ),
		new Entry( "uk", "Українська" ),
	};

	public record Entry( string Code, string Title );

	private static readonly Dictionary<string, string> phrases = new( System.StringComparer.OrdinalIgnoreCase );
	private static string current;

	public static string CurrentCode => current ??= Normalize( Application.LanguageCode );

	public static void SetLanguage( string code )
	{
		code = Normalize( code );
		if ( code == current && phrases.Count > 0 ) return;
		current = code;
		Reload();
	}

	public static string T( string key ) => Render( Lookup( key ), null );

	public static string T( string key, Dictionary<string, object> data ) => Render( Lookup( key ), data );

	public static string Key( string action ) => InputBindings.GetDisplayKey( action );

	public static Dictionary<string, object> Keys( params string[] actions )
	{
		var dict = new Dictionary<string, object>( actions.Length );
		foreach ( var name in actions )
			dict[name.Replace( " ", "" )] = Key( name );
		return dict;
	}

	private static string Normalize( string code )
	{
		if ( string.IsNullOrWhiteSpace( code ) ) return DefaultCode;
		foreach ( var e in Available )
			if ( string.Equals( e.Code, code, System.StringComparison.OrdinalIgnoreCase ) ) return e.Code;
		return DefaultCode;
	}

	private static void Reload()
	{
		phrases.Clear();
		LoadInto( DefaultCode );
		if ( current != DefaultCode ) LoadInto( current );
	}

	private static void LoadInto( string code )
	{
		var path = $"localization/{code}.json";
		if ( !FileSystem.Mounted.FileExists( path ) ) return;

		try
		{
			var entries = FileSystem.Mounted.ReadJson<Dictionary<string, string>>( path );
			if ( entries == null ) return;

			foreach ( var kv in entries )
				phrases[kv.Key] = kv.Value;
		}
		catch ( System.Exception e )
		{
			Log.Warning( $"couldn't read localization file {path}: {e.Message}" );
		}
	}

	private static string Lookup( string key )
	{
		if ( phrases.Count == 0 ) Reload();
		return phrases.TryGetValue( key, out var value ) ? value : key;
	}

	private static string Render( string value, Dictionary<string, object> data )
	{
		if ( data is null || value is null || !value.Contains( '{' ) ) return value;

		var sb = new StringBuilder( value );
		foreach ( var kv in data )
			sb.Replace( "{" + kv.Key + "}", kv.Value?.ToString() ?? string.Empty );
		return sb.ToString();
	}
}
