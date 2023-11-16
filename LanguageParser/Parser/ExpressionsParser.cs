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
                return parser._errors.Last();

            expressions.Add(expression);
        }

        return new Program(expressions);
    }

    private ExpressionBase? ParseFunctionInvocation(ExpressionBase function)
    {
        if (TryParseSeparated(SyntaxKind.OpenParenthesis, SyntaxKind.CloseParenthesis, SyntaxKind.Comma,
                false,
                out var open,
                out var args,
                out var close))
            return new InvocationExpression(function, open, args, close);

        return null;
    }

    private bool TryParseSeparated(SyntaxKind open, SyntaxKind close, SyntaxKind separator,
        bool isLastArgEmpty,
        [NotNullWhen(true)] out Token? openToken,
        [NotNullWhen(true)] out IList<ExpressionBase>? args,
        [NotNullWhen(true)] out Token? closeToken)
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
            return true;
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

            if (!isLastArgEmpty)
                if (_tokens.Current.Kind == close)
                    break;

            if (EatToken(separator) is null)
            {
                args = null;
                closeToken = null;
                return false;
            }

            if (!isLastArgEmpty) 
                continue;
            
            if (_tokens.Current.Kind == close)
                break;
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
            case SyntaxKind.NumberLiteral:
                return new ConstantExpression(_tokens.Current.Kind, EatToken());
            case SyntaxKind.OpenParenthesis:
                return ParseParenthesizedExpression();
            case SyntaxKind.OpenBrace:
                return ParseScopeExpression();
            case SyntaxKind.If:
                return ParseIfExpression();
            case SyntaxKind.While:
                return ParseWhileExpression();
            case SyntaxKind.Repeat:
                return ParseRepeatExpression();
            case SyntaxKind.For:
                return ParseForExpression();
            case SyntaxKind.Word:
                return new ConstantExpression(_tokens.Current.Kind, EatToken());
            case SyntaxKind.StringLiteral:
                return new ConstantExpression(SyntaxKind.StringLiteral, EatToken());
            case SyntaxKind.True:
                return new ConstantExpression(SyntaxKind.True, EatToken());
            case SyntaxKind.False:
                return new ConstantExpression(SyntaxKind.False, EatToken());
            case SyntaxKind.EOF:
                _errors.Add(new UnexpectedEofException(_tokens.Current.Range));
                return null;
            default:
                _errors.Add(new UnexpectedTokenException(_tokens.Current));
                return null;
        }
    }

    private ExpressionBase? ParseForExpression()
    {
        var forToken = EatToken(SyntaxKind.For);
        if (forToken is null)
            return null;

        var openToken = EatToken(SyntaxKind.OpenParenthesis);
        if (openToken is null)
            return null;
        
        ExpressionBase? initialization = null;
        
        if (_tokens.Current.Kind is not SyntaxKind.Semicolon)
        {
            initialization = ParseExpression();

            if (initialization is null)
                return null;
        }
        
        if (EatToken(SyntaxKind.Semicolon) is null)
            return null;

        ExpressionBase? condition = null;
        if (_tokens.Current.Kind is not SyntaxKind.Semicolon)
        {
            condition = ParseExpression();
            if (condition is null)
                return null;
        }
        
        if (EatToken(SyntaxKind.Semicolon) is null)
            return null;

        ExpressionBase? step = null;
        
        if (_tokens.Current.Kind is not SyntaxKind.CloseParenthesis)
        {
            step = ParseExpression();
            if (step is null)
                return null;
        }

        var closeToken = EatToken(SyntaxKind.CloseParenthesis);
        if (closeToken is null)
            return null;

        var body = ParseExpression();
        if (body is null)
            return null;

        return new ForExpression(forToken, openToken, initialization, condition, step, closeToken, body);
    }

    private ExpressionBase? ParseRepeatExpression()
    {
        var repeatToken = EatToken(SyntaxKind.Repeat);
        if (repeatToken is null)
            return null;

        var body = ParseExpression();
        if (body is null) 
            return null;

        var untilToken = EatToken(SyntaxKind.Until);
        if (untilToken is null)
            return null;

        var openToken = EatToken(SyntaxKind.OpenParenthesis);
        if (openToken is null)
            return null;

        var condition = ParseExpression();
        if (condition is null)
            return null;

        var closeToken = EatToken(SyntaxKind.CloseParenthesis);
        if (closeToken is null)
            return null;

        return new RepeatExpression(repeatToken, body, untilToken, openToken, condition, closeToken);
    }

    private ExpressionBase? ParseWhileExpression()
    {
        var whileToken = EatToken(SyntaxKind.While);
        if (whileToken is null)
            return null;

        var openToken = EatToken(SyntaxKind.OpenParenthesis);
        if (openToken is null)
            return null;

        var condition = ParseExpression();
        if (condition is null)
            return null;

        var closeToken = EatToken(SyntaxKind.CloseParenthesis);
        if (closeToken is null)
            return null;

        var body = ParseExpression();
        if (body is null)
            return null;

        return new WhileExpression(whileToken, openToken, condition, closeToken, body);
    }

    private ExpressionBase? ParseIfExpression()
    {
        var ifToken = EatToken(SyntaxKind.If);
        if (ifToken is null)
            return null;

        var open = EatToken(SyntaxKind.OpenParenthesis);
        if (open is null)
            return null;

        var condition = ParseExpression();
        if (condition is null)
            return null;

        var close = EatToken(SyntaxKind.CloseParenthesis);
        if (close is null)
            return null;

        var thenToken = EatToken(SyntaxKind.Then);
        if (thenToken is null)
            return null;

        var trueBranch = ParseExpression();
        if (trueBranch is null)
            return null;

        var token = EatToken(SyntaxKind.Semicolon);

        if (token is null)
            return null;
        
        if (_tokens.Current.Kind is not SyntaxKind.Else)
        {
            _tokens.Recede();
            return new IfExpression(ifToken, open, condition, close, thenToken, trueBranch);
        }
        
        var elseToken = EatToken();

        var falseBranch = ParseExpression();
        if (falseBranch is null)
            return null;

        return new IfExpression(ifToken, open, condition, close, thenToken, trueBranch, elseToken, falseBranch);
    }

    private ExpressionBase? ParseScopeExpression()
    {
        if (TryParseSeparated(SyntaxKind.OpenBrace, SyntaxKind.CloseBrace, SyntaxKind.Semicolon,
                true,
                out var open,
                out var expressions,
                out var close))
            
            return new ScopeExpression(open, expressions, close);

        return null;
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
                SyntaxKind.NumberLiteral or
                SyntaxKind.StringLiteral or
                SyntaxKind.Word or
                SyntaxKind.InvocationExpression or
                SyntaxKind.IfExpression or
                SyntaxKind.WhileExpression or
                SyntaxKind.RepeatExpression or
                SyntaxKind.ForExpression or
                SyntaxKind.ScopeExpression => Precedence.Primary,
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