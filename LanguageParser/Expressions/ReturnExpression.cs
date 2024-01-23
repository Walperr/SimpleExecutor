using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Lexer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class ReturnExpression : ExpressionBase
{
    public Token ReturnToken { get; }
    public ExpressionBase? ReturnValue { get; }

    internal ReturnExpression(Token returnToken, ExpressionBase? returnValue = null) : base(SyntaxKind.ReturnExpression)
    {
        ReturnToken = returnToken;
        ReturnValue = returnValue;
    }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return ReturnToken;
        
        if (ReturnValue is not null)
            yield return ReturnValue;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        visitor.VisitReturn(this);
    }

    public override T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state)
    {
        return visitor.VisitReturn(this, state);
    }
}