using Sandbox;
using System;
using System.Collections.Generic;

public static class InputBindings
{
	public static int Version { get; private set; }

	public static bool IsTrapping { get; private set; }

	public static IReadOnlyList<string> RebindableActions { get; } = new[]
	{
		"Tilt Forward",
		"Tilt Backward",
		"Tilt Left",
		"Tilt Right",
		"Spin Button",
		"Spin Left",
		"Spin Right",
		"Rotation Reset",
		"Leaderboard",
	};

	public static string LocalizationKey( string action )
		=> "controls." + action.ToLowerInvariant().Replace( ' ', '_' );

	// font has arrow glyphs at U+E000..U+E003, swap in for arrow pad strings
	public static string GetDisplayKey( string action )
	{
		var raw = Input.GetButtonOrigin( action ) ?? "";
		return raw.ToUpperInvariant() switch
		{
			"UP" => "\uE000",
			"RIGHT" => "\uE001",
			"DOWN" => "\uE002",
			"LEFT" => "\uE003",
			_ => raw,
		};
	}

	private static bool cancelled;
	private static Action callback;

	public static void Rebind( string action, Action onDone = null )
	{
		var instance = IGameInstance.Current;
		if ( instance is null )
		{
			onDone?.Invoke();
			return;
		}

		cancelled = false;
		callback = onDone;
		IsTrapping = true;

		instance.TrapButtons( keys =>
		{
			// empty keys means esc cancel
			if ( !cancelled && keys is { Length: > 0 } )
			{
				instance.SetBind( action, keys[0] );
				instance.SaveBinds();
				Version++;
			}
			else
			{
				// swallow esc so Layout doesn't read next frame
				Sandbox.Input.EscapePressed = false;
			}

			cancelled = false;
			IsTrapping = false;
			callback?.Invoke();
			callback = null;
		} );
	}

	public static void CancelRebind()
	{
		if ( !IsTrapping ) return;

		cancelled = true;
		IsTrapping = false;
		callback?.Invoke();
		callback = null;
	}

	public static void ResetAll()
	{
		var instance = IGameInstance.Current;
		if ( instance is null ) return;

		instance.ResetBinds();
		instance.SaveBinds();
		Version++;
	}
}
