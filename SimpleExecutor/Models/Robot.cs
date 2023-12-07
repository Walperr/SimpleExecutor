using System;
using ReactiveUI;
using SkiaSharp;

namespace SimpleExecutor.Models;

public sealed class Robot : ReactiveObject
{
    private Direction _direction = Direction.Down;
    private PointI _position;

    public int Width => Tiles.GetLength(0);

    public int Height => Tiles.GetLength(1);

    public PointI Position
    {
        get => _position;
        set => this.RaiseAndSetIfChanged(ref _position, value);
    }

    public SKColor Color { get; set; } = SKColors.Blue;

    public SKColor Background { get; set; } = SKColors.White;

    public SKColor?[,] Tiles { get; private set; } = new SKColor?[256, 256];

    public void Move(int length)
    {
        var dir = DirectionI;

        var pos = Position;

        for (var i = 0; i < length; i++)
        {
            if (pos.X >= Width || pos.Y >=  Height || pos.X < 0 || pos.Y < 0)
                continue;
            
            Tiles[pos.X, pos.Y] = Color;
            pos += dir;
        }

        Position = pos;
    }

    public PointI DirectionI
    {
        get
        {
            return _direction switch
            {
                Direction.Up => new PointI(0, -1),
                Direction.Down => new PointI(0, 1),
                Direction.Left => new PointI(-1, 0),
                Direction.Right => new PointI(1, 0),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public void Jump(int x, int y)
    {
        x = Math.Min(Math.Max(0, x), Width - 1);
        y = Math.Min(Math.Max(0, y), Height - 1);

        Tiles[Position.X, Position.Y] = Color;
        
        Position = new PointI(x, y);
    }

    public void TurnLeft()
    {
        _direction = _direction switch
        {
            Direction.Up => Direction.Left,
            Direction.Down => Direction.Right,
            Direction.Left => Direction.Down,
            Direction.Right => Direction.Up,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void TurnRight()
    {
        _direction = _direction switch
        {
            Direction.Up => Direction.Right,
            Direction.Down => Direction.Left,
            Direction.Left => Direction.Up,
            Direction.Right => Direction.Down,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void SetSize(int width, int height)
    {
        Tiles = new SKColor?[width, height];
        Position = default;
    }

    public void Reset()
    {
        SetSize(256, 256);
        _direction = Direction.Down;
        Position = default;
        Background = SKColors.White;
        Color = SKColors.Blue;
    }

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}

public readonly struct PointI
{
    public int X { get; }

    public int Y { get; }

    public PointI(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PointI point)
            return false;

        return X == point.X && Y == point.Y;
    }

    public bool Equals(PointI other)
    {
        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(PointI a, PointI b)
    {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(PointI a, PointI b)
    {
        return !(a == b);
    }

    public static PointI operator +(PointI a, PointI b)
    {
        return new PointI(a.X + b.X, a.Y + b.Y);
    }

    public static PointI operator -(PointI a, PointI b)
    {
        return new PointI(a.X - b.X, a.Y - b.Y);
    }

    public static PointI operator +(PointI a)
    {
        return a;
    }

    public static PointI operator -(PointI a)
    {
        return new PointI(-a.X, -a.Y);
    }

    public static PointI operator *(PointI a, int b)
    {
        return new PointI(a.X * b, a.Y * b);
    }
}