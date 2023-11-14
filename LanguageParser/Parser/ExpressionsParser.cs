using System.Diagnostics.CodeAnalysis;
using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Interfaces;
using LanguageParser.Tokenizer;

namespace LanguageParser.Parser;

public sealed class ExpressionsParser
{
    private readonly IStream<Token> _tokens;

    private readonly List<ParseException> _errors = new();

    private ExpressionsParser(IStream<Token> tokens)
    {
        _tokens = tokens;
    }

    public static Result<ParseException, TopLevelExpression> Parse(string text)
    {
        var tokens = Tokenizer.Tokenizer.Tokenize(text);

        var parser = new ExpressionsParser(tokens);

        ExpressionBase? expression;

        try
        {
            expression = parser.ParseExpression();
        }
        catch (Exception e)
        {
            return new UnhandledParserException(e);
        }

        if (expression is null)
            return parser._errors.First();

        if (tokens.Current.Kind != SyntaxKind.EOF)
        {
            return new ParseException("Expression is not consumed entirely", tokens.Current.Range);
        }

        return new TopLevelExpression(expression, null);
    }

    private ExpressionBase? ParseExpression()
    {
        var function = ParseFunctionName();

        if (function is null)
        {
            _errors.Add(new ExpectedOtherTokenException(_tokens.Current, SyntaxKind.Word));
            return null;
        }

        return ParseFunctionInvocation(function);
    }

    private ExpressionBase? ParseFunctionInvocation(ExpressionBase function)
    {
        if (TryParseCommaSeparated(SyntaxKind.OpenParenthesis, SyntaxKind.CloseParenthesis, out var open, out var args,
                out var close))
            return new InvocationExpression(function, open, args, close);

        return null;
    }

    private bool TryParseCommaSeparated(SyntaxKind open, SyntaxKind close, [NotNullWhen(true)] out Token? openToken, [NotNullWhen(true)] out IList<ExpressionBase>? args, [NotNullWhen(true)] out Token? closeToken)
    {
        openToken = EatToken(open);
        if (openToken is null)
        {
            closeToken = null;
            args = null;
            return false;
        }

        if (_tokens.Current.Kind == close)
        {
            closeToken = EatToken();
            args = Array.Empty<ExpressionBase>();
        }

        args = new List<ExpressionBase>();
        while (true)
        {
            var expr = ParseSimpleExpression();
            if (expr is null)
            {
                args = null;
                closeToken = null;
                return false;
            }
            
            args.Add(expr);

            if (_tokens.Current.Kind == close)
                break;

            if (EatToken(SyntaxKind.Comma) is null)
            {
                args = null;
                closeToken = null;
                return false;
            }
        }

        closeToken = EatToken(close);
        if (closeToken is null)
        {
            args = null;
            return false;
        }

        return true;
    }

    private ExpressionBase? ParseSimpleExpression()
    {
        var current = _tokens.Current;
        if (current.IsString || current.IsNumber)
        {
            _tokens.Advance();
            return new ConstantExpression(current.Kind, current);
        }

        _errors.Add(new ExpectedOtherTokenException(current, SyntaxKind.Number, SyntaxKind.String));
        return null;
    }

    private Token EatToken()
    {
        var current = _tokens.Current;
        _tokens.Advance();
        return current;
    }

    private Token? EatToken(SyntaxKind kind)
    {
        var current = _tokens.Current;
        if (current.Kind == kind)
        {
            _tokens.Advance();
            return current;
        }
        
        _errors.Add(new ExpectedOtherTokenException(current, kind));
        return null;
    }

    private ExpressionBase? ParseFunctionName()
    {
        var current = _tokens.Current;
        if (current.Kind != SyntaxKind.Word) 
            return null;
        
        if (_tokens.CanAdvance)
            _tokens.Advance();

        return new FunctionExpression(current);

    }
}