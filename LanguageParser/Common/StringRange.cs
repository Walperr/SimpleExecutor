namespace LanguageParser.Common;

public readonly struct StringRange : IEquatable<StringRange>
{
    public StringRange(int start, int length)
    {
        Start = start;
        Length = length;
    }

    public int Start { get; }
    public int Length { get; }

    public int End => Start + Length;

    public StringRange? Intersection(StringRange other)
    {
        var start = Math.Max(Start, other.Start);
        var end = Math.Min(End, other.End);

        return start >= end
            ? null
            : new StringRange(start, end - start);
    }

    public StringRange Union(StringRange other)
    {
        var start = Math.Min(Start, other.Start);
        var end = Math.Max(End, other.End);

        return new StringRange(start, end - start);
    }

    public bool CollidesWith(int offset)
    {
        return offset >= Start && offset <= End;
    }

    public bool Equals(StringRange other)
    {
        return Start == other.Start && Length == other.Length;
    }

    public override bool Equals(object? obj)
    {
        return obj is StringRange other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, Length);
    }

    public static bool operator ==(StringRange left, StringRange right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StringRange left, StringRange right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"Start: {Start} End: {End} Length: {Length}";
    }
}