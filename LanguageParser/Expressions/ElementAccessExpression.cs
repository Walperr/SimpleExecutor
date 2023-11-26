using System.Collections.Immutable;
using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Lexer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class ElementAccessExpression : ExpressionBase
{
    internal ElementAccessExpression(ExpressionBase expression,
        Token openBracket,
        IEnumerable<ExpressionBase> elements,
        Token closeBracket) : base(SyntaxKind.ElementAccessExpression)
    {
        Expression = expression;
        OpenBracket = openBracket;
        Elements = elements.ToImmutableArray();
        CloseBracket = closeBracket;
    }

    public ExpressionBase Expression { get; }
    public Token OpenBracket { get; }
    public ImmutableArray<ExpressionBase> Elements { get; }
    public Token CloseBracket { get; }

    public bool IsArrayDeclaration => Expression is ConstantExpression { IsTypeKeyword: true } && !Elements.Any();

    public bool IsArrayConstructor => Expression is ConstantExpression { IsTypeKeyword: true };

    public bool IsElementAccessor => !IsArrayConstructor && !IsArrayDeclaration;

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return Expression;
        yield return OpenBracket;
        foreach (var argument in Elements)
            yield return argument;
        yield return CloseBracket;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        visitor.VisitElementAccess(this);
    }

    public override T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state)
    {
        return visitor.VisitElementAccess(this, state);
    }
}