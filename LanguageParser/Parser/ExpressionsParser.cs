using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Interfaces;
using LanguageParser.Tokenizer;

namespace LanguageParser.Parser;

public sealed class ExpressionsParser
{
    private readonly List<SyntaxException> _errors = new();
    private readonly IStream<Token> _tokens;

    internal ExpressionsParser(IStream<Token> tokens)
    {
        _tokens = tokens;
    }

    public static Result<SyntaxException, ExpressionBase> Parse(string text)
    {
        var result = PreParser.PreParse((TokenStream) Tokenizer.Tokenizer.Tokenize(text));

        if (result.IsError)
            return result.Error;

        var tokens = result.Value;

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
            return new SyntaxException("Expression is not consumed entirely", tokens.Current.Range);
        }

        return expression;
    }

    private ExpressionBase? ParseFunctionInvocation(ExpressionBase function)
    {
        if (TryParseSeparated(SyntaxKind.OpenParenthesis, SyntaxKind.CloseParenthesis, SyntaxKind.Comma,
                out var open,
                out var args,
                out var close))
            return new InvocationExpression(function, open, args, close);

        return null;
    }

    private bool TryParseSeparated(SyntaxKind open, SyntaxKind close, SyntaxKind separator,
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

            if (_tokens.Current.Kind == close)
                    break;

            if (EatToken(separator) is null)
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

    internal SyntaxException Error => _errors.First();

    internal ExpressionBase? ParseExpression()
    {
        var expression = ParseSubExpression(Precedence.Expression);
        if (_tokens.Current.Kind is SyntaxKind.Semicolon)
            EatToken();
        
        return expression;
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
            var tk = _tokens.Current.Kind;

            SyntaxKind opKind;

            if (Syntax.IsBinaryExpression(tk))
            {
                opKind = Syntax.ConvertToBinaryExpression(tk);
            }
            else
            {
                break;
            }

            var newPrecedence = GetPrecedence(opKind);

            // Check the precedence to see if we should "take" this operator
            if (newPrecedence < precedence)
            {
                break;
            }

            // Same precedence, but not right-associative -- deal with this "later"
            if (newPrecedence == precedence)
            {
                break;
            }

            // We'll "take" this operator, as precedence is tentatively OK.
            var opToken = EatToken(tk);

            if (opToken is null)
                return null;

            var leftPrecedence = GetPrecedence(leftOperand.Kind);
            if (newPrecedence > leftPrecedence)
            {
                throw new InvalidOperationException();
            }

            Debug.Assert(Syntax.IsBinaryExpression(tk));
            
            var constantExpression = new ConstantExpression(SyntaxKind.Operator, opToken);
            
            var rightOperand = ParseSubExpression(newPrecedence);
            
            if (rightOperand is null)
                return null;
            
            leftOperand = new BinaryExpression(opKind, leftOperand, constantExpression, rightOperand);
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
            case SyntaxKind.Number:
                return ParseVariable(SyntaxKind.Number);
            case SyntaxKind.String:
                return ParseVariable(SyntaxKind.String);
            case SyntaxKind.Bool:
                return ParseVariable(SyntaxKind.Bool);
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

    private ExpressionBase? ParseVariable(SyntaxKind kind)
    {
        var typeToken = EatToken(kind);
        if (typeToken is null)
            return null;

        var nameToken = EatToken(SyntaxKind.Word);
        if (nameToken is null)
            return null;
        
        if (_tokens.Current.Kind is SyntaxKind.Semicolon)
            return new VariableExpression(typeToken, nameToken);

        _tokens.Recede();
        
        var value = ParseExpression();
        if (value is BinaryExpression { Kind: SyntaxKind.AssignmentExpression } assignment)
            return new VariableExpression(typeToken, nameToken, assignment);
        
        _errors.Add(new SyntaxException("Expected assignment", _tokens.Current.Range));
        return null;
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
        else if (EatToken(SyntaxKind.Semicolon) is null)
            return null;

        ExpressionBase? condition = null;
        if (_tokens.Current.Kind is not SyntaxKind.Semicolon)
        {
            condition = ParseExpression();
            if (condition is null)
                return null;
        } else if (EatToken(SyntaxKind.Semicolon) is null)
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

        var trueBranch = ParseExpression();
        if (trueBranch is null)
            return null;
        
        if (_tokens.Current.Kind is not SyntaxKind.Else)
            return new IfExpression(ifToken, open, condition, close, trueBranch);

        var elseToken = EatToken();

        var falseBranch = ParseExpression();
        if (falseBranch is null)
            return null;

        return new IfExpression(ifToken, open, condition, close, trueBranch, elseToken, falseBranch);
    }

    private ExpressionBase? ParseScopeExpression()
    {
        var openBrace = EatToken(SyntaxKind.OpenBrace);
        if (openBrace is null)
            return null;

        var expressions = new List<ExpressionBase>();

        while (_tokens.Current.Kind != SyntaxKind.CloseBrace)
        {
            var expression = ParseExpression();
            if (expression is null)
                return null;

            expressions.Add(expression);
        }

        var closeBrace = EatToken(SyntaxKind.CloseBrace);
        if (closeBrace is null)
            return null;

        return new ScopeExpression(openBrace, expressions, closeBrace);
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
            SyntaxKind.AssignmentExpression => Precedence.Assignment,
            SyntaxKind.EqualityExpression => Precedence.Equality,
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
                SyntaxKind.ScopeExpression or 
                SyntaxKind.True or 
                SyntaxKind.False or
                SyntaxKind.VariableExpression => Precedence.Primary,
            _ => throw new InvalidEnumArgumentException()
        };
    }

    private enum Precedence : uint
    {
        Expression = 0,
        Assignment,
        ConditionalOr,
        ConditionalAnd,
        Equality,
        Relational,
        Addition,
        Multiplication,
        Primary
    }
}