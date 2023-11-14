using LanguageParser.Tokenizer;

namespace LanguageParser.Common;

public class ParseException : Exception
{
    internal ParseException(string message, StringRange range) : base(message)
    {
        Range = range;
    }

    public StringRange Range { get; }
}

public sealed class ExpectedOtherTokenException : ParseException
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

public sealed class UnexpectedTokenException : ParseException
{
    internal UnexpectedTokenException(Token token) : base($"Unexpected token: '{token.Lexeme}'", token.Range)
    {
    }
}

public sealed class UnexpectedEofException : ParseException
{
    internal UnexpectedEofException(StringRange range) : base("Unexpected end of file", range)
    {
    }
}

public sealed class UnhandledParserException : ParseException
{
    internal UnhandledParserException(Exception exception) : base(exception.Message, default)
    {
    }
}