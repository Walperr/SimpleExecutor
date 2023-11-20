using LanguageParser.Common;
using LanguageParser.Interfaces;

namespace LanguageParser.Lexer;

internal sealed class TokenStream : IStream<Token>
{
    private readonly Token[] _tokens;

    public TokenStream(IEnumerable<Token> tokens)
    {
        _tokens = tokens.ToArray();
    }

    public IReadOnlyList<Token> Tokens => _tokens;

    public int Length => _tokens.Length;

    public Token EOF => _tokens.Length > 0
        ? new Token(SyntaxKind.EOF, "", _tokens[^1].Range.End)
        : new Token(SyntaxKind.EOF, "", 0);

    public int Index { get; private set; }

    public Token Current => Index < Length ? _tokens[Index] : EOF;
    public Token Next => Index + 1 < Length ? _tokens[Index + 1] : EOF;

    public bool CanAdvance => Index + 1 < _tokens.Length;

    public void Advance()
    {
        Index++;
    }

    public bool CanRecede => Index > 0;
    public void Recede()
    {
        if (CanRecede)
            Index--;
    }
}