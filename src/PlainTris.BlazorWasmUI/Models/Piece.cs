namespace PlainTris.Models;

public sealed class Piece
{
    public int[][] Matrix { get; set; } = new int[][] {};

    public int Color { get; set; }

    public (int X, int Y) Position { get; set; }

    public int Width
    {
        get => Matrix.Select(x => Matrix[0].Length - x.Reverse().TakeWhile(v => v == 0).Count()).Max();
    }

    public int Height
    {
        get => Matrix.Length;
    }

    public void RotateRight()
    {
        var newMatrix = new int[4][];

        foreach (var rowIndex in Enumerable.Range(0, 4))
        {
            newMatrix[rowIndex] = new int[4];
            foreach (var colIndex in Enumerable.Range(0, 4))
            {
                newMatrix[rowIndex][colIndex] = Matrix[3 - colIndex][rowIndex];
            };
        }

        Matrix = newMatrix;

        Normalize();
    }

    public void RotateLeft()
    {
        var newMatrix = new int[4][];

        foreach (var rowIndex in Enumerable.Range(0, 4))
        {
            newMatrix[rowIndex] = new int[4];
            foreach (var colIndex in Enumerable.Range(0, 4))
            {
                newMatrix[rowIndex][colIndex] = Matrix[colIndex][3 - rowIndex];
            };
        }

        Matrix = newMatrix;

        Normalize();
    }

    public void Normalize()
    {
        while (Matrix[0].All(x => x == 0))
        {
            foreach (var rowIndex in Enumerable.Range(0, 3))
            {
                Matrix[rowIndex] = Matrix[rowIndex + 1];
            }
            Matrix[3] = new int[] {0, 0, 0, 0};
        }

        while (Matrix.Select(x => x[0]).All(x => x == 0))
        {
            foreach (var rowIndex in Enumerable.Range(0, 4))
            {
                foreach (var colIndex in Enumerable.Range(0, 3))
                {
                    Matrix[rowIndex][colIndex] = Matrix[rowIndex][colIndex + 1];
                }

                Matrix[rowIndex][3] = 0;
            }
        }
    }
}