using LanguageParser.Interfaces;

namespace LanguageParser.Common;

internal sealed class CharStream : IStream<char>
{
    public CharStream(string text)
    {
        Text = text;
    }

    public string Text { get; }

    public int Index { get; private set; }
    public char Current => Index < Text.Length ? Text[Index] : '\0';
    public char Next => Index + 1 < Text.Length ? Text[Index + 1] : '\0';
    public bool CanAdvance => Index < Text.Length;

    public void Advance()
    {
        if (CanAdvance)
            Index++;
    }
}