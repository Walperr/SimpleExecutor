using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Lexer;
using LanguageParser.Visitors;

namespace LanguageParser.Expressions;

public sealed class VariableExpression : ExpressionBase
{
    public Token TypeToken { get; }
    public Token? OpenBracket { get; }
    public Token? CloseBracket { get; }
    public Token NameToken { get; }
    public ExpressionBase? AssignmentExpression { get; }

    public bool IsArrayVariable => OpenBracket is not null && CloseBracket is not null;

    internal VariableExpression(Token typeToken, Token? openBracket, Token? closeBracket, Token nameToken, BinaryExpression? assignmentExpression = null) : base(SyntaxKind.VariableExpression)
    {
        TypeToken = typeToken;
        OpenBracket = openBracket;
        CloseBracket = closeBracket;
        NameToken = nameToken;
        AssignmentExpression = assignmentExpression;
    }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return TypeToken;
        if (OpenBracket is not null)
            yield return OpenBracket;
        if (CloseBracket is not null)
            yield return CloseBracket;
        
        yield return NameToken;
        
        if (AssignmentExpression is not null)
            yield return AssignmentExpression;
    }

    public override void Visit(ExpressionVisitor visitor)
    {
        visitor.VisitVariable(this);
    }

    public override T Visit<T, TState>(ExpressionVisitor<T, TState> visitor, TState state)
    {
        return visitor.VisitVariable(this, state);
    }
}