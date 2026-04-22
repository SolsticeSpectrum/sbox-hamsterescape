public class PrimsMazeAlgorithm : BasicMazeGenerator
{
	public PrimsMazeAlgorithm( int rows, int columns ) : base( rows, columns )
	{
	}

	public override void GenerateMaze()
	{
		for ( int r = 0; r < RowCount; r++ )
		{
			for ( int c = 0; c < ColumnCount; c++ )
			{
				var cell = GetMazeCell( r, c );
				cell.WallRight = true;
				cell.WallFront = true;
				cell.WallLeft = true;
				cell.WallBack = true;
			}
		}

		var frontier = new List<(int row, int col, Direction dir)>();

		int startRow = Game.Random.Int( 0, RowCount - 1 );
		int startCol = Game.Random.Int( 0, ColumnCount - 1 );
		GetMazeCell( startRow, startCol ).IsVisited = true;
		Add( startRow, startCol, frontier );

		while ( frontier.Count > 0 )
		{
			int idx = Game.Random.Int( 0, frontier.Count - 1 );
			var wall = frontier[idx];
			frontier.RemoveAt( idx );

			var (nr, nc) = Neighbor( wall.row, wall.col, wall.dir );

			bool here = GetMazeCell( wall.row, wall.col ).IsVisited;
			bool there = GetMazeCell( nr, nc ).IsVisited;
			if ( here == there ) continue;

			Open( wall.row, wall.col, wall.dir );

			int row = there ? wall.row : nr;
			int col = there ? wall.col : nc;
			GetMazeCell( row, col ).IsVisited = true;
			Add( row, col, frontier );
		}
	}

	private void Add( int row, int col, List<(int, int, Direction)> frontier )
	{
		if ( col + 1 < ColumnCount ) frontier.Add( (row, col, Direction.Right) );
		if ( col - 1 >= 0 ) frontier.Add( (row, col, Direction.Left) );
		if ( row + 1 < RowCount ) frontier.Add( (row, col, Direction.Front) );
		if ( row - 1 >= 0 ) frontier.Add( (row, col, Direction.Back) );
	}

	private (int, int) Neighbor( int row, int col, Direction dir ) => dir switch
	{
		Direction.Right => (row, col + 1),
		Direction.Left => (row, col - 1),
		Direction.Front => (row + 1, col),
		Direction.Back => (row - 1, col),
		_ => (-1, -1),
	};

	private void Open( int row, int col, Direction dir )
	{
		var cell = GetMazeCell( row, col );
		switch ( dir )
		{
			case Direction.Right:
				cell.WallRight = false;
				GetMazeCell( row, col + 1 ).WallLeft = false;
				break;
			case Direction.Left:
				cell.WallLeft = false;
				GetMazeCell( row, col - 1 ).WallRight = false;
				break;
			case Direction.Front:
				cell.WallFront = false;
				GetMazeCell( row + 1, col ).WallBack = false;
				break;
			case Direction.Back:
				cell.WallBack = false;
				GetMazeCell( row - 1, col ).WallFront = false;
				break;
		}
	}
}
