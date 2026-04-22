using System;

// cached Action per enum value, avoids fresh lambda every render
public sealed class Navigator<T> where T : struct, Enum
{
	private readonly Action<T> target;
	private readonly Dictionary<T, Action> cache = new();

	public Navigator( Action<T> target )
	{
		this.target = target ?? throw new ArgumentNullException( nameof( target ) );
	}

	public Action To( T page )
	{
		if ( !cache.TryGetValue( page, out var action ) )
		{
			action = () => target( page );
			cache[page] = action;
		}
		return action;
	}
}
