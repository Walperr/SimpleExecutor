using LanguageParser.Common;
using LanguageParser.Interfaces;
using LanguageParser.Tokenizer;

namespace LanguageParser.Expressions;

public sealed class VariableExpression : ExpressionBase
{
    public Token TypeToken { get; }
    public Token NameToken { get; }
    public ExpressionBase? AssignmentExpression { get; }

    internal VariableExpression(Token typeToken, Token nameToken, BinaryExpression? assignmentExpression = null) : base(SyntaxKind.VariableExpression)
    {
        TypeToken = typeToken;
        NameToken = nameToken;
        AssignmentExpression = assignmentExpression;
    }

    public override IEnumerable<ISyntaxElement> GetAllElements()
    {
        yield return TypeToken;
        yield return NameToken;
        
        if (AssignmentExpression is not null)
            yield return AssignmentExpression;
    }
}