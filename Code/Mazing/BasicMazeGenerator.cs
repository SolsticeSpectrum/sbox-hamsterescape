using System;

public abstract class BasicMazeGenerator
{
	public int RowCount => rows;
	public int ColumnCount => columns;

	private readonly int rows;
	private readonly int columns;
	private readonly MazeCell[,] grid;

	public BasicMazeGenerator( int rows, int columns )
	{
		this.rows = Math.Max( 1, Math.Abs( rows ) );
		this.columns = Math.Max( 1, Math.Abs( columns ) );

		grid = new MazeCell[this.rows, this.columns];
		for ( int row = 0; row < this.rows; row++ )
		{
			for ( int col = 0; col < this.columns; col++ )
			{
				grid[row, col] = new MazeCell();
			}
		}
	}

	public abstract void GenerateMaze();

	public MazeCell GetMazeCell( int row, int column )
	{
		if ( row < 0 || column < 0 || row >= rows || column >= columns )
		{
			Log.Error( $"maze cell out of range, row {row} col {column}" );
			throw new ArgumentOutOfRangeException();
		}
		return grid[row, column];
	}
}
