using System.Collections.Immutable;
using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Lexer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class ArrayInitializationExpression : ExpressionBase
{
    internal ArrayInitializationExpression(Token openBracket, IEnumerable<ExpressionBase> elements, Token closeBracket)
        : base(SyntaxKind.ArrayInitializationExpression)
    {
        OpenBracket = openBracket;
        Elements = elements.ToImmutableArray();
        CloseBracket = closeBracket;
    }

    public Token OpenBracket { get; }
    public ImmutableArray<ExpressionBase> Elements { get; }
    public Token CloseBracket { get; }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return OpenBracket;

        foreach (var element in Elements)
            yield return element;

        yield return CloseBracket;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        visitor.VisitArrayInitialization(this);
    }

    public override T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state)
    {
        return visitor.VisitArrayInitialization(this, state);
    }
}