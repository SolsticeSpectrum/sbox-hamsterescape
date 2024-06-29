using Sandbox;
using System;

public sealed class MazeSpawner : Component
{
	[Property] public GameObject Wall { get; set; }
	[Property] public GameObject Pillar { get; set; }
	[Property] public GameObject Plane { get; set; }
	[Property] public GameObject Pickup { get; set; }
	[Property, Range( 0, 20, 5 )] public int Rows { get; set; } = 5;
	[Property, Range( 0, 20, 5 )] public int Columns { get; set; } = 5;
	[Property, Range( 0, 500, 5 )] public int CellWidth { get; set; } = 5;
	[Property, Range( 0, 500, 5 )] public int CellHeight { get; set; } = 5;
	[Property, Range( 0, 150, 5 )] public float WallOffset { get; set; } = 100.5f;
	[Property, Range( 0, 150, 5 )] public float PillarOffset { get; set; } = 125.5f;
	[Property] public bool AddGaps { get; set; } = true;

	public enum MazeGenerationAlgorithm
	{
		PureRecursive
	}

	public MazeGenerationAlgorithm Algorithm = MazeGenerationAlgorithm.PureRecursive;
	private BasicMazeGenerator mMazeGenerator = null;

	protected override void OnStart()
	{
		Maze();
	}

	public void Maze()
	{
		float cellW = CellWidth + ( AddGaps ? .2f : 0 );
		float cellH = CellHeight + ( AddGaps ? .2f : 0 );
		float cellWD = cellW / 2;
		float cellHD = cellH / 2;

		for ( int i = 0; i < GameObject.Children.Count; i++ )
		{
			if ( GameObject.Children[i] == null ) continue;
			if ( GameObject.Children[i] != Plane ) GameObject.Children[i].Destroy();
		}

		switch ( Algorithm )
		{
			case MazeGenerationAlgorithm.PureRecursive:
				mMazeGenerator = new RecursiveMazeAlgorithm( Rows, Columns );
				break;
		}

		mMazeGenerator.GenerateMaze();

		for ( int row = 0; row < Rows; row++ )
		{
			for ( int column = 0; column < Columns; column++ )
			{
				float x = column * cellW - WallOffset;
				float z = row * cellH - WallOffset;
				MazeCell cell = mMazeGenerator.GetMazeCell( row, column );
				GameObject tmp;

				if ( cell.WallRight )
				{
					tmp = Wall.Clone();
					tmp.SetParent( GameObject );
					tmp.Transform.LocalPosition = new Vector3( x + cellWD, z, 0 );  // right
					tmp.Transform.LocalRotation = new Angles( 0, 90, 0 );
				}

				if ( cell.WallFront )
				{
					tmp = Wall.Clone();
					tmp.SetParent( GameObject );
					tmp.Transform.LocalPosition = new Vector3( x, z + cellHD, 0 );  // front
					tmp.Transform.LocalRotation = new Angles( 0, 0, 0 );
				}

				if ( cell.WallLeft )
				{
					tmp = Wall.Clone();
					tmp.SetParent( GameObject );
					tmp.Transform.LocalPosition = new Vector3( x - cellWD, z, 0 );  // left
					tmp.Transform.LocalRotation = new Angles( 0, 270, 0 );
				}

				if ( cell.WallBack )
				{
					tmp = Wall.Clone();
					tmp.SetParent( GameObject );
					tmp.Transform.LocalPosition = new Vector3( x, z - cellHD, 0 );  // back
					tmp.Transform.LocalRotation = new Angles( 0, 180, 0 );
				}
			}
		}

		for ( int row = 0; row < Rows + 2; row++ )
		{
			for ( int column = 0; column < Columns + 2; column++ )
			{
				float x = column * cellW - PillarOffset;
				float z = row * cellH - PillarOffset;

				GameObject pillar = Pillar.Clone();
				pillar.SetParent( GameObject );
				pillar.Transform.LocalPosition = new Vector3( x - cellWD, z - cellHD, 0 );
				pillar.Transform.LocalRotation = new Angles( 0, 0, 0 );

				bool place = Game.Random.Int( 8 ) == 0;
				if ( row < Rows + 1 && column < Columns + 1 && place )
				{
					float pX = column * cellW - WallOffset;
					float pZ = row * cellH - WallOffset;

					GameObject pickup = Pickup.Clone();
					pickup.SetParent( GameObject );
					pickup.Transform.LocalPosition = new Vector3( pX - cellWD, pZ - cellHD, 13 );
					pickup.Transform.LocalRotation = new Angles( 0, 0, 0 );
				}
			}
		}
	}
}
