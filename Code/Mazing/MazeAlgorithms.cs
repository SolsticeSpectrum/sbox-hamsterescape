using System;

public static class MazeAlgorithms
{
	public record Entry( Type Type, string Title );

	public static readonly IReadOnlyList<Entry> Available = new[]
	{
		new Entry( typeof( HilbertLookaheadMazeAlgorithm ), "Hilbert" ),
		new Entry( typeof( PrimsMazeAlgorithm ), "Prim's" ),
	};

	public static Type Default => Available[0].Type;

	public static Type Resolve( string name )
	{
		foreach ( var e in Available )
			if ( e.Type.Name == name ) return e.Type;
		return Default;
	}
}
