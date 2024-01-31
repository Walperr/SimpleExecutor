using LanguageParser.Expressions;
using LanguageParser.Lexer;

namespace LanguageParser.Common;

public class SyntaxException : Exception
{
    protected internal SyntaxException(string message, StringRange range) : base(message)
    {
        Range = range;
    }

    public StringRange Range { get; }
}

public sealed class ExpectedOtherTokenException : SyntaxException
{
    internal ExpectedOtherTokenException(Token token, params SyntaxKind[] expected) : base(
        GetErrorMessage(token, expected), token.Range)
    {
        ExpectedTokens = expected.Select(Syntax.GetLexemeForToken).ToArray();
    }

    public string[] ExpectedTokens { get; }

    private static string GetErrorMessage(Token token, IReadOnlyCollection<SyntaxKind> expected)
    {
        var tokens = string.Join(" or ", expected.Select(e => "'" + Syntax.GetLexemeForToken(e) + "'"));
        return $"Expected other token{(expected.Count > 1 ? "s" : "")}: {tokens}, got '{token.Lexeme}'";
    }
}

public sealed class UnexpectedTokenException : SyntaxException
{
    public UnexpectedTokenException(Token token) : base($"Unexpected token: '{token.Lexeme}'", token.Range)
    {
    }
}

public sealed class UnexpectedEofException : SyntaxException
{
    internal UnexpectedEofException(StringRange range) : base("Unexpected end of file", range)
    {
    }
}

public sealed class UnhandledParserException : SyntaxException
{
    internal UnhandledParserException(Exception exception) : base(exception.Message, default)
    {
    }
}

public sealed class VariableAlreadyDeclaredException : SyntaxException
{
    public VariableAlreadyDeclaredException(string variableName, StringRange range) : base(
        $"Variable already declared in this scope. {variableName}", range)
    {
    }
}

public sealed class FunctionAlreadyDeclaredException : SyntaxException
{
    public FunctionAlreadyDeclaredException(string functionName, StringRange range) : base($"Function already declared in this scope. {functionName}", range)
    {
    }
}

public sealed class UnexpectedExpressionException : SyntaxException
{
    public UnexpectedExpressionException(ExpressionBase expression) : base("Unexpected expression", expression.Range)
    {
    }
}