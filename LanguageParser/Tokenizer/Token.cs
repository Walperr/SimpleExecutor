using System.Collections.Immutable;
using LanguageParser.Common;
using LanguageParser.Interfaces;

namespace LanguageParser.Tokenizer;

public sealed class Token : ISyntaxElement
{
    private Token(SyntaxKind kind, string lexeme, int start, ImmutableArray<Token> leadingTrivia,
        ImmutableArray<Token> trailingTrivia)
    {
        Kind = kind;
        Lexeme = lexeme;
        Range = new StringRange(start, lexeme.Length);
        LeadingTrivia = leadingTrivia;
        TrailingTrivia = trailingTrivia;
    }

    internal Token(SyntaxKind kind, string lexeme, int start)
    {
        Kind = kind;
        Lexeme = lexeme;
        Range = new StringRange(start, lexeme.Length);
        LeadingTrivia = ImmutableArray<Token>.Empty;
        TrailingTrivia = ImmutableArray<Token>.Empty;
    }

    public Token WithTrivia(ImmutableArray<Token> leading, ImmutableArray<Token> trailing)
    {
        return new Token(Kind, Lexeme, Range.Start, leading, trailing);
    }
    
    public ImmutableArray<Token> TrailingTrivia { get; }

    public ImmutableArray<Token> LeadingTrivia { get; }

    public string Lexeme { get; }

    public StringRange Range { get; }
    public SyntaxKind Kind { get; }

    public override string ToString()
    {
        return $"{Kind} '{Lexeme}'";
    }

    public bool IsToken => Kind is >= SyntaxKind.WhiteSpace and <= SyntaxKind.Semicolon;
    
    public bool IsString => Kind == SyntaxKind.StringLiteral;

    public bool IsNumber => Kind == SyntaxKind.NumberLiteral;

    public bool IsKeyWord => Kind is >= SyntaxKind.If and <= SyntaxKind.False;
}