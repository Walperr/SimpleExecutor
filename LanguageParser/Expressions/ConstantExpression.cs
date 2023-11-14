using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Tokenizer;

namespace LanguageParser.Expressions;

public sealed class ConstantExpression : ExpressionBase, ISyntaxElement
{
    internal ConstantExpression(SyntaxKind kind, Token token) : base(kind)
    {
        Token = token;
    }

    public Token Token { get; }

    public bool IsNumber => Kind == SyntaxKind.Number;

    public bool IsString => Kind == SyntaxKind.String;

    public string Lexeme => Token.Lexeme;

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return Token;
    }
}