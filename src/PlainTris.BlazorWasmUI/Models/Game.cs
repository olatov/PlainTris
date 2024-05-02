namespace PlainTris.Models;

public delegate void Notify();

public sealed class Game
{
    public int[][] Matrix { get; set; } = new int[][] {};

    public int Width => Matrix[0].Length;

    public int Height => Matrix.Length;

    public Piece? Piece { get; private set; } = new Piece();

    public int Score { get; private set; } = 0;

    public int Lines { get; private set; } = 0;

    public int Level => Lines / 10 + 1;

    private int _tickCounter = 0;

    public event Notify NewPieceCreated;

    private readonly IEnumerator<int> _colorProvider;

    private readonly IEnumerator<int> _pieceProvider;

    public Game()
    {
        _colorProvider = Colors().GetEnumerator();
        _pieceProvider = Pieces().GetEnumerator();
    }

    public void MoveLeft()
    {
        if (Piece is null)
        {
            return;
        }

        if (CanPlace(Piece.Position.Y, Piece.Position.X - 1))
        {
            Piece.Position = (Math.Max(0, Piece.Position.X - 1), Piece.Position.Y);
        }
    }

    public void MoveRight()
    {
        if (Piece is null)
        {
            return;
        }

        if (CanPlace(Piece.Position.Y, Piece.Position.X + 1))
        {
            Piece.Position = (Math.Min(Width - Piece.Width, Piece.Position.X + 1), Piece.Position.Y);
        }
    }

    public void RotateRight()
    {
        if (Piece is null)
        {
            return;
        }

        Piece.RotateRight();
        if (!CanPlace(Piece.Position.Y, Piece.Position.X))
        {
            Piece.RotateLeft();
        }
    }

    public void Tick()
    {
        _tickCounter++;

        ShiftDown();

        if (_tickCounter % 2 == 0)
        {
            if (Piece is null)
            {
                CreateNewPiece();

                if (!CanPlace(Piece!.Position.Y, Piece!.Position.X))
                {
                    _isGameOver = true;
                    PlacePiece();
                }

                return;
            }

            if (!CanPlace(Piece.Position.Y + 1, Piece.Position.X))
            {
                PlacePiece();
                RemoveLines();
                Piece = null;

                Score++;

                return;
            }

            Piece.Position = (Piece.Position.X, Piece.Position.Y + 1);
        }
    }

    public string GetColor(int row, int col)
    {
        if (Piece is not null
            && row >= Piece.Position.Y
            && row < (Piece.Position.Y + Piece.Height)
            && col >= Piece.Position.X
            && col < (Piece.Position.X + Piece.Width))
        {
            if (Piece.Matrix[row - Piece.Position.Y][col - Piece.Position.X] != 0)
            {
                return _colors[Piece.Color];
            }
        }

        return _colors[Matrix[row][col]];
    }

    private IEnumerable<int> Colors()
    {
        var colorStack = new Stack<int>();

        while (true)
        {
            if (colorStack.Count > 1 && colorStack.TryPop(out var result))
            {
                yield return result;
            }
            else
            {
                var fill = Enumerable.Range(1, _colors.Length - 1)
                    .Where(x => !colorStack.Contains(x))
                    .OrderBy(_ => Random.Shared.Next());

                foreach (var index in fill)
                {
                    colorStack.Push(index);
                }
            }
        }
    }

    private IEnumerable<int> Pieces()
    {
        var pieceStack = new Stack<int>();

        while (true)
        {
            if (pieceStack.TryPop(out var result))
            {
                yield return result;
            }
            else
            {
                var fill = Enumerable.Range(0, _pieces.Length)
                    .OrderBy(_ => Random.Shared.Next());

                foreach (var index in fill)
                {
                    pieceStack.Push(index);
                }
            }
        }
    }


    private bool _isGameOver = false;

    public bool IsGameOver
    {
        get => _isGameOver;
    }

    public void Reset()
    {
        Matrix = new int[20][];
        foreach (var rowIndex in Enumerable.Range(0, Height))
        {

            Matrix[rowIndex] = new int[10];
            foreach (var colIndex in Enumerable.Range(0, Matrix[rowIndex].Length))
            {
                Matrix[rowIndex][colIndex] = 0;
            };
        }

        Score = 0;
        Lines = 0;
        _isGameOver = false;
        _tickCounter = 0;

        CreateNewPiece();
    }

    private void RemoveLines()
    {
        var removedLines = 0;

        foreach (var rowIndex in Enumerable.Range(0, Height))
        {
            if (Matrix[rowIndex].All(x => x != 0))
            {
                Matrix[rowIndex] = Enumerable.Range(0, Width).Select(_ => 0).ToArray();
                removedLines++;
            }
        }

        if (removedLines > 0)
        {
            Lines += removedLines;

            var bonus = removedLines switch
            {
                1 => 0,
                2 => 5,
                3 => 15,
                4 => 35,
                _ => removedLines * 10
            };

            Score += 5 * removedLines + bonus;
        }

    }

    private void ShiftDown()
    {
        foreach (var rowIndex in Enumerable.Range(0, Height).Reverse())
        {
            if (Matrix[rowIndex].All(x => x == 0))
            {
                foreach (var newRowIndex in Enumerable.Range(1, rowIndex).Reverse())
                {
                    Matrix[newRowIndex] = Matrix[newRowIndex - 1];
                }

                Matrix[0] = Enumerable.Range(0, Width).Select(_ => 0).ToArray();
            }
        }
    }

    private void CreateNewPiece()
    {
        _pieceProvider.MoveNext();
        Piece = _pieces[_pieceProvider.Current];
        _colorProvider.MoveNext();
        Piece.Color = _colorProvider.Current;

        foreach (var _ in Enumerable.Range(0, Random.Shared.Next(3)))
        {
            Piece.RotateRight();
        }

        var _colOffset = (Width - Piece.Width) / 2;

        Piece.Position = (_colOffset, 0);

        NewPieceCreated?.Invoke();
    }

    private bool CanPlace(int row, int col)
    {
        if (col < 0 || Piece is null)
        {
            return false;
        }

        foreach (var rowIndex in Enumerable.Range(0, Piece.Height))
        {
            foreach (var colIndex in Enumerable.Range(0, Piece.Width))
            {
                if (Piece.Matrix[rowIndex][colIndex] != 0)
                {
                    if (rowIndex + row >= Height
                        || colIndex + col > Width - 1
                        || Matrix[rowIndex + row][colIndex + col] != 0)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private void PlacePiece()
    {
        foreach (var rowIndex in Enumerable.Range(0, Piece.Height))
        {
            foreach (var colIndex in Enumerable.Range(0, Piece.Width))
            {
                if (Piece.Matrix[rowIndex][colIndex] != 0)
                {
                    Matrix[rowIndex + Piece.Position.Y][colIndex + Piece.Position.X] = Piece.Color;
                }
            }
        }
    }

    private readonly string[] _colors = {
        "",
        "Tomato",
        "yellow",
        "limegreen",
        "royalblue",
        "aqua",
        "orange",
        "Lavender",
        "Violet",
        "HotPink"
    };

    private readonly Piece[] _pieces = {
        new()
        {
            Matrix = new[]
            {
                new [] { 0, 0, 0, 0 },
                new [] { 1, 1, 1, 1 },
                new [] { 0, 0, 0, 0 },
                new [] { 0, 0, 0, 0 },
            },
        },
        new()
        {
            Matrix = new[]
            {
                new [] { 1, 1, 0, 0 },
                new [] { 1, 1, 0, 0 },
                new [] { 0, 0, 0, 0 },
                new [] { 0, 0, 0, 0 },
            },
        },
        new()
        {
            Matrix = new[]
            {
                new [] { 1, 1, 1, 0 },
                new [] { 0, 1, 0, 0 },
                new [] { 0, 0, 0, 0 },
                new [] { 0, 0, 0, 0 },
            },
        },
        new()
        {
            Matrix = new[]
            {
                new [] { 0, 1, 1, 0 },
                new [] { 1, 1, 0, 0 },
                new [] { 0, 0, 0, 0 },
                new [] { 0, 0, 0, 0 },
            },
        },
        new()
        {
            Matrix = new[]
            {
                new [] { 1, 1, 0, 0 },
                new [] { 0, 1, 1, 0 },
                new [] { 0, 0, 0, 0 },
                new [] { 0, 0, 0, 0 },
            },
        },
        new()
        {
            Matrix = new[]
            {
                new [] { 1, 1, 1, 0 },
                new [] { 1, 0, 0, 0 },
                new [] { 0, 0, 0, 0 },
                new [] { 0, 0, 0, 0 },
            },
        },
        new()
        {
            Matrix = new[]
            {
                new [] { 1, 1, 1, 0 },
                new [] { 0, 0, 1, 0 },
                new [] { 0, 0, 0, 0 },
                new [] { 0, 0, 0, 0 },
            },
        }
    };
}