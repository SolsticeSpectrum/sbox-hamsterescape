using System;

// https://jkbhasler.github.io/2025/02/17/hilbert-lookahead

public class HilbertLookaheadMazeAlgorithm : BasicMazeGenerator
{
	public HilbertLookaheadMazeAlgorithm( int rows, int columns ) : base( rows, columns )
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

		var order = Gilbert( RowCount, ColumnCount );
		var index = new int[RowCount, ColumnCount];
		for ( int i = 0; i < order.Count; i++ )
			index[order[i].row, order[i].col] = i;

		var higher = new List<Direction>( 4 );

		// last cell in curve has nowhere higher to go, everything drains to it
		for ( int i = 0; i < order.Count - 1; i++ )
		{
			var (r, c) = order[i];

			higher.Clear();
			if ( c + 1 < ColumnCount && index[r, c + 1] > i ) higher.Add( Direction.Right );
			if ( c - 1 >= 0 && index[r, c - 1] > i ) higher.Add( Direction.Left );
			if ( r + 1 < RowCount && index[r + 1, c] > i ) higher.Add( Direction.Front );
			if ( r - 1 >= 0 && index[r - 1, c] > i ) higher.Add( Direction.Back );
			if ( higher.Count == 0 ) continue;

			Open( r, c, higher[Game.Random.Int( 0, higher.Count - 1 )] );
		}
	}

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

	// generalized hilbert curve for any rectangle 
	// returns cells in curve order with consecutive entries always closest to grid
	private static List<(int row, int col)> Gilbert( int rows, int cols )
	{
		var result = new List<(int, int)>( rows * cols );
		if ( cols >= rows )
			Gen( 0, 0, cols, 0, 0, rows, result );
		else
			Gen( 0, 0, 0, rows, cols, 0, result );
		return result;
	}

	private static void Gen( int x, int y, int ax, int ay, int bx, int by, List<(int, int)> result )
	{
		int w = Math.Abs( ax + ay );
		int h = Math.Abs( bx + by );
		int dax = Math.Sign( ax ), day = Math.Sign( ay );
		int dbx = Math.Sign( bx ), dby = Math.Sign( by );

		if ( h == 1 )
		{
			for ( int i = 0; i < w; i++ )
			{
				result.Add( (y, x) );
				x += dax; y += day;
			}
			return;
		}
		if ( w == 1 )
		{
			for ( int i = 0; i < h; i++ )
			{
				result.Add( (y, x) );
				x += dbx; y += dby;
			}
			return;
		}

		int ax2 = Div( ax, 2 ), ay2 = Div( ay, 2 );
		int bx2 = Div( bx, 2 ), by2 = Div( by, 2 );
		int w2 = Math.Abs( ax2 + ay2 );
		int h2 = Math.Abs( bx2 + by2 );

		if ( 2 * w > 3 * h )
		{
			if ( (w2 % 2) != 0 && w > 2 ) { ax2 += dax; ay2 += day; }
			Gen( x, y, ax2, ay2, bx, by, result );
			Gen( x + ax2, y + ay2, ax - ax2, ay - ay2, bx, by, result );
		}
		else
		{
			if ( (h2 % 2) != 0 && h > 2 ) { bx2 += dbx; by2 += dby; }
			Gen( x, y, bx2, by2, ax2, ay2, result );
			Gen( x + bx2, y + by2, ax, ay, bx - bx2, by - by2, result );
			Gen( x + (ax - dax) + (bx2 - dbx), y + (ay - day) + (by2 - dby),
				-bx2, -by2, -(ax - ax2), -(ay - ay2), result );
		}
	}

	// floor division because C# / truncates
	private static int Div( int a, int b )
	{
		int q = a / b;
		int r = a % b;
		if ( r != 0 && (r < 0) != (b < 0) ) q--;
		return q;
	}
}
