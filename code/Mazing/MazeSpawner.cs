using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class MazeSpawner : Component
{
	[Property] public GameObject Wall { get; set; }
	[Property] public GameObject Pillar { get; set; }
	[Property] public GameObject Plane { get; set; }
	[Property] public GameObject Pickup { get; set; }
	[Property, Range( 1, 20 )] public int Rows { get; set; } = 6;
	[Property, Range( 1, 20 )] public int Columns { get; set; } = 6;
	[Property, Range( 10, 500 ), Step( 5 )] public int CellWidth { get; set; } = 50;
	[Property, Range( 10, 500 ), Step( 5 )] public int CellHeight { get; set; } = 50;
	[Property] public Vector2 MazeOffset { get; set; } = Vector2.Zero;
	[Property] public bool AddGaps { get; set; } = true;
	[Property, Range( 0, 5 )] public int Exits { get; set; } = 2;

	private BasicMazeGenerator generator;
	private SceneManager manager;
	private float cellW;
	private float cellH;
	private float originX;
	private float originY;
	private readonly HashSet<(int col, int row)> rowExits = new();
	private readonly HashSet<(int col, int row)> colExits = new();

	protected override void OnStart() => Maze();

	public void Maze()
	{
		cellW = CellWidth + ( AddGaps ? 0.2f : 0 );
		cellH = CellHeight + ( AddGaps ? 0.2f : 0 );
		originX = -( Columns * cellW ) * 0.5f + MazeOffset.x;
		originY = -( Rows * cellH ) * 0.5f + MazeOffset.y;

		ClearChildren();

		manager ??= Scene.GetAllComponents<SceneManager>().FirstOrDefault();
		var algorithm = manager?.Algorithm ?? MazeAlgorithms.Default;
		generator = Game.TypeLibrary.GetType( algorithm )
			?.Create<BasicMazeGenerator>( new object[] { Rows, Columns } );
		generator?.GenerateMaze();
		PickExits();

		SpawnPillars();
		SpawnWalls();
		SpawnPickups();
	}

	// Exits random gaps on each of 4 perimeter sides
	private void PickExits()
	{
		rowExits.Clear();
		colExits.Clear();

		foreach ( var c in Pick( Columns, Exits ) ) rowExits.Add( (c, 0) );
		foreach ( var c in Pick( Columns, Exits ) ) rowExits.Add( (c, Rows) );
		foreach ( var r in Pick( Rows, Exits ) ) colExits.Add( (0, r) );
		foreach ( var r in Pick( Rows, Exits ) ) colExits.Add( (Columns, r) );
	}

	private static IEnumerable<int> Pick( int outOf, int count )
	{
		var pool = new List<int>( outOf );
		for ( int i = 0; i < outOf; i++ ) pool.Add( i );
		int take = Math.Min( count, pool.Count );
		for ( int i = 0; i < take; i++ )
		{
			int idx = Game.Random.Int( 0, pool.Count - 1 );
			yield return pool[idx];
			pool.RemoveAt( idx );
		}
	}

	private void SpawnPillars()
	{
		for ( int row = 0; row <= Rows; row++ )
		{
			for ( int col = 0; col <= Columns; col++ )
			{
				Spawn( Pillar, PillarPos( col, row ), 0 );
			}
		}
	}

	private void SpawnWalls()
	{
		// horizontal, span world X
		for ( int row = 0; row <= Rows; row++ )
		{
			for ( int col = 0; col < Columns; col++ )
			{
				if ( rowExits.Contains( (col, row) ) ) continue;
				if ( row > 0 && row < Rows && !generator.GetMazeCell( row, col ).WallBack ) continue;
				Spawn( Wall, Midpoint( PillarPos( col, row ), PillarPos( col + 1, row ) ), 90 );
			}
		}

		// vertical, span world Y
		for ( int row = 0; row < Rows; row++ )
		{
			for ( int col = 0; col <= Columns; col++ )
			{
				if ( colExits.Contains( (col, row) ) ) continue;
				if ( col > 0 && col < Columns && !generator.GetMazeCell( row, col ).WallLeft ) continue;
				Spawn( Wall, Midpoint( PillarPos( col, row ), PillarPos( col, row + 1 ) ), 0 );
			}
		}
	}

	private void SpawnPickups()
	{
		for ( int row = 0; row < Rows; row++ )
		{
			for ( int col = 0; col < Columns; col++ )
			{
				if ( Game.Random.Int( 8 ) != 0 ) continue;

				var center = CellCenter( col, row );
				Spawn( Pickup, new Vector3( center.x, center.y, 13 ), 0 );
			}
		}
	}

	private Vector3 PillarPos( int col, int row )
		=> new( originX + col * cellW, originY + row * cellH, 0 );

	private Vector3 CellCenter( int col, int row )
		=> new( originX + ( col + 0.5f ) * cellW, originY + ( row + 0.5f ) * cellH, 0 );

	private static Vector3 Midpoint( Vector3 a, Vector3 b ) => ( a + b ) * 0.5f;

	private void Spawn( GameObject prefab, Vector3 localPos, float yaw )
	{
		var go = prefab.Clone();
		go.SetParent( GameObject );
		go.LocalPosition = localPos;
		go.LocalRotation = new Angles( 0, yaw, 0 );
	}

	private void ClearChildren()
	{
		for ( int i = 0; i < GameObject.Children.Count; i++ )
		{
			var child = GameObject.Children[i];
			if ( child == null || child == Plane ) continue;
			child.Destroy();
		}
	}
}
