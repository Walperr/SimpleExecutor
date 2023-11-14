using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Tokenizer;

namespace LanguageParser.Expressions;

public sealed class FunctionExpression : ExpressionBase
{
    public Token Token { get; }

    internal FunctionExpression(Token token) : base(token.Kind)
    {
        Token = token;
    }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return Token;
    }
}