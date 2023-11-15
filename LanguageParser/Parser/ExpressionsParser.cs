using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Interfaces;
using LanguageParser.Tokenizer;

namespace LanguageParser.Parser;

public sealed class ExpressionsParser
{
    private readonly List<ParseException> _errors = new();
    private readonly IStream<Token> _tokens;

    private ExpressionsParser(IStream<Token> tokens)
    {
        _tokens = tokens;
    }

    public static Result<ParseException, Program> Parse(string text)
    {
        var tokens = Tokenizer.Tokenizer.Tokenize(text);

        var parser = new ExpressionsParser(tokens);

        var expressions = new List<ExpressionBase>();

        while (parser._tokens.Current.Kind is not SyntaxKind.EOF)
        {
            ExpressionBase? expression;

            try
            {
                expression = parser.ParseOuterExpression();
            }
            catch (Exception e)
            {
                return new UnhandledParserException(e);
            }

            if (expression is null)
                return parser._errors.First();

            expressions.Add(expression);
        }

        return new Program(expressions);
    }

    private ExpressionBase? ParseFunctionInvocation(ExpressionBase function)
    {
        if (TryParseCommaSeparated(SyntaxKind.OpenParenthesis, SyntaxKind.CloseParenthesis, out var open, out var args,
                out var close))
            return new InvocationExpression(function, open, args, close);

        return null;
    }

    private bool TryParseCommaSeparated(SyntaxKind open, SyntaxKind close, [NotNullWhen(true)] out Token? openToken,
        [NotNullWhen(true)] out IList<ExpressionBase>? args, [NotNullWhen(true)] out Token? closeToken)
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
            var expr = ParseExpression();
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

    private ExpressionBase? ParseOuterExpression()
    {
        var expression = ParseExpression();
        
        var token = EatToken(SyntaxKind.Semicolon);

        return token is not null ? expression : null;
    }

    private ExpressionBase? ParseExpression()
    {
        return ParseSubExpression(Precedence.Expression);
    }

    private ExpressionBase? ParseSubExpression(Precedence precedence)
    {
        var leftOperand = ParseTerm();
        return ParseExpressionContinued(leftOperand, precedence);
    }

    private ExpressionBase? ParseExpressionContinued(ExpressionBase? leftOperand, Precedence precedence)
    {
        if (leftOperand is null)
            return null;

        while (true)
        {
            var tokenKind = _tokens.Current.Kind;
            SyntaxKind operatorKind;

            if (Syntax.IsBinaryExpression(tokenKind))
                operatorKind = Syntax.ConvertToBinaryExpression(tokenKind);
            else
                break;

            var newPrecedence = GetPrecedence(operatorKind);
            if (newPrecedence < precedence) // operator with less precedence than current (1 * 2 + 3)
                break; //                                                      and stopped there ^

            var operatorToken = EatToken(tokenKind);
            if (operatorToken is null)
                return null;

            var leftPrecedence = GetPrecedence(leftOperand.Kind);
            if (newPrecedence > leftPrecedence)
                throw new InvalidOperationException();

            var subExpression = ParseSubExpression(newPrecedence);
            if (subExpression is null)
                return null;

            var operatorExpression = new ConstantExpression(SyntaxKind.Operator, operatorToken);
            if (subExpression is BinaryExpression bin && GetPrecedence(bin.Kind) == newPrecedence)
            {
                leftOperand = new BinaryExpression(operatorKind, leftOperand, operatorExpression, bin.Left);
                leftOperand = new BinaryExpression(bin.Kind, leftOperand, bin.Operator, bin.Right);
            }
            else
            {
                leftOperand = new BinaryExpression(operatorKind, leftOperand, operatorExpression, subExpression);
            }
        }

        return leftOperand;
    }

    private ExpressionBase? ParseTerm()
    {
        return ParsePostfixExpression(ParseTermWithoutPostfix());
    }

    private ExpressionBase? ParsePostfixExpression(ExpressionBase? expression)
    {
        if (expression is null)
            return null;

        if (_tokens.Current.Kind is not SyntaxKind.OpenParenthesis)
            return expression;

        var function = ParseFunctionInvocation(expression);

        return function;
    }

    private ExpressionBase? ParseTermWithoutPostfix()
    {
        switch (_tokens.Current.Kind)
        {
            case SyntaxKind.Number:
                return new ConstantExpression(_tokens.Current.Kind, EatToken());
            case SyntaxKind.OpenParenthesis:
                return ParseParenthesizedExpression();
            case SyntaxKind.Word:
                return new ConstantExpression(_tokens.Current.Kind, EatToken());
            case SyntaxKind.String:
                return new ConstantExpression(SyntaxKind.String, EatToken());
            case SyntaxKind.EOF:
                _errors.Add(new UnexpectedEofException(_tokens.Current.Range));
                return null;
            default:
                _errors.Add(new UnexpectedTokenException(_tokens.Current));
                return null;
        }
    }


    private ExpressionBase? ParseParenthesizedExpression()
    {
        var open = EatToken(SyntaxKind.OpenParenthesis);
        if (open is null)
            return null;
        var expression = ParseExpression();
        if (expression is null)
            return null;
        var close = EatToken(SyntaxKind.CloseParenthesis);

        if (close is null)
            return null;

        return new ParenthesizedExpression(open, expression, close);
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

    private static Precedence GetPrecedence(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.EqualityOperator => Precedence.Equality,
            SyntaxKind.OrExpression => Precedence.ConditionalOr,
            SyntaxKind.AndExpression => Precedence.ConditionalAnd,
            SyntaxKind.RelationalExpression => Precedence.Relational,
            SyntaxKind.AddExpression or SyntaxKind.SubtractExpression => Precedence.Addition,
            SyntaxKind.MultiplyExpression or SyntaxKind.DivideExpression => Precedence.Multiplication,
            SyntaxKind.ParenthesizedExpression or
                SyntaxKind.Number or
                SyntaxKind.String or
                SyntaxKind.Word or
                SyntaxKind.InvocationExpression => Precedence.Primary,
            _ => throw new InvalidEnumArgumentException()
        };
    }

    private enum Precedence : uint
    {
        Expression = 0,
        ConditionalOr,
        ConditionalAnd,
        Equality,
        Relational,
        Addition,
        Multiplication,
        Primary
    }
}