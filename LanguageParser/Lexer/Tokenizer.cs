using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using LanguageParser.Common;
using LanguageParser.Interfaces;

namespace LanguageParser.Lexer;

public sealed class Tokenizer
{
    private readonly CharStream _charStream;
    private readonly ImmutableArray<Token>.Builder _triviaBuilder = ImmutableArray.CreateBuilder<Token>();

    private ImmutableArray<Token> _leadingTrivia;

    private Tokenizer(string text)
    {
        _charStream = new CharStream(text);
    }

    public static IStream<Token> Tokenize(string s)
    {
        return new TokenStream(new Tokenizer(s).ReadAll());
    }
    
    private Token Read()
    {
        var leading = _leadingTrivia.IsDefault ? ReadTrivia() : _leadingTrivia;
        var token = ReadToken();
        var trailing = ReadTrivia();

        var result = token.WithTrivia(leading, trailing);

        _leadingTrivia = trailing;

        return result;
    }

    private Token ReadToken()
    {
        switch (_charStream.Current)
        {
            case '(':
                return SimpleToken(SyntaxKind.OpenParenthesis);
            case ')':
                return SimpleToken(SyntaxKind.CloseParenthesis);
            case '{':
                return SimpleToken(SyntaxKind.OpenBrace);
            case '}':
                return SimpleToken(SyntaxKind.CloseBrace);
            case ',':
                return SimpleToken(SyntaxKind.Comma);
            case '"' or '\'':
                return StringToken();
            // case '+':
            //     return SimpleToken(SyntaxKind.Plus);
            // case '-':
            //     return SimpleToken(SyntaxKind.Minus);
            case '*':
                return SimpleToken(SyntaxKind.Asterisk);
            case '/':
                return SimpleToken(SyntaxKind.Slash);
            case ';':
                return SimpleToken(SyntaxKind.Semicolon);
            case var c:
            {
                if (IsOperator(c))
                    return ReadToken(IsOperator, GetOperatorByLexeme);

                if (!IsSpecial(c))
                    return IsDigitOrDot(_charStream.Current)
                        ? ReadToken(IsDigitOrDot, _ => SyntaxKind.NumberLiteral)
                        : AdjustKeyword(ReadToken(IsWord, _ => SyntaxKind.Word));
                
                var token = new Token(SyntaxKind.Error, c.ToString(), _charStream.Index);
                _charStream.Advance();
                return token;

            }
        }
    }

    private static Token AdjustKeyword(Token token)
    {
        return token.Lexeme switch
        {
            "if" => new Token(SyntaxKind.If, token.Lexeme, token.Range.Start),
            "else" => new Token(SyntaxKind.Else, token.Lexeme, token.Range.Start),
            "repeat" => new Token(SyntaxKind.Repeat, token.Lexeme, token.Range.Start),
            "times" => new Token(SyntaxKind.Times, token.Lexeme, token.Range.Start),
            "while" => new Token(SyntaxKind.While, token.Lexeme, token.Range.Start),
            "for" => new Token(SyntaxKind.For, token.Lexeme, token.Range.Start),
            "number" => new Token(SyntaxKind.Number, token.Lexeme, token.Range.Start),
            "string" => new Token(SyntaxKind.String, token.Lexeme, token.Range.Start),
            "bool" => new Token(SyntaxKind.Bool, token.Lexeme, token.Range.Start),
            "or" => new Token(SyntaxKind.ConditionalOrOperator, token.Lexeme, token.Range.Start),
            "and" => new Token(SyntaxKind.ConditionalAndOperator, token.Lexeme, token.Range.Start),
            "true" => new Token(SyntaxKind.True, token.Lexeme, token.Range.Start),
            "false" => new Token(SyntaxKind.False, token.Lexeme, token.Range.Start),
            _ => token
        };
    }

    private SyntaxKind GetOperatorByLexeme(string lexeme)
    {
        return lexeme switch
        {
            "/" => SyntaxKind.Slash,
            "*" => SyntaxKind.Asterisk,
            "+" => SyntaxKind.Plus,
            "-" => SyntaxKind.Minus,
            "++" => SyntaxKind.PlusPlus,
            "--" => SyntaxKind.MinusMinus,
            "and" => SyntaxKind.ConditionalAndOperator,
            "or" => SyntaxKind.ConditionalOrOperator,
            "&&" => SyntaxKind.ConditionalAndOperator,
            "||" => SyntaxKind.ConditionalOrOperator,
            "=" => SyntaxKind.AssignmentOperator,
            "==" => SyntaxKind.EqualityOperator,
            "%" => SyntaxKind.Percent,
            "=>" => throw new Exception("Arrow"),
            _ => SyntaxKind.Operator
        };
    }

    private bool IsWord(char c)
    {
        return !IsSpecial(c) && !char.IsWhiteSpace(c);
    }

    private bool IsSpecial(char c)
    {
        return c is '.' or '$' or ';' or '@' or '%' or ',' or '?' or '`' or '"' or '\'' or '\\' or '\0'
               || IsOperator(c)
               || IsParenthesis(c);
    }

    private bool IsParenthesis(char c)
    {
        return c is '{' or '}' or '(' or ')' or '[' or ']';
    }

    private bool IsOperator(char c)
    {
        return c is '+' or '-' or '*' or '/' or '~' or '=' or '&' or '|' or '!' or '<' or '>' or '%';
    }

    private bool IsDigitOrDot(char c)
    {
        return char.IsDigit(c) || c == '.';
    }

    private Token StringToken()
    {
        var openQuote = _charStream.Current;
        
        _charStream.Advance();

        var start = _charStream.Index;

        var builder = new StringBuilder();

        while (true)
        {
            switch (_charStream.Current)
            {
                case '\\':
                    _charStream.Advance();
                    switch (_charStream.Current)
                    {
                        case '"':
                            builder.Append('"');
                            break;
                        case '\'':
                            builder.Append('\'');
                            break;
                        case 'n':
                            builder.Append('\n');
                            break;
                        case 'r':
                            builder.Append('\r');
                            break;
                        default:
                            return new Token(SyntaxKind.Error, _charStream.Current.ToString(), _charStream.Index);
                    }
                    _charStream.Advance();
                    break;
                case var c when c == openQuote:
                    _charStream.Advance();
                    return new Token(SyntaxKind.StringLiteral, builder.ToString(), start);
                case '\0':
                    return new Token(SyntaxKind.Error, "EOF", _charStream.Index);
                default:
                    builder.Append(_charStream.Current);
                    _charStream.Advance();
                    break;
            }
        }
    }

    private Token SimpleToken(SyntaxKind kind)
    {
        var token = new Token(kind, Syntax.GetLexemeForToken(kind), _charStream.Index);
        _charStream.Advance();

        return token;
    }

    private ImmutableArray<Token> ReadTrivia()
    {
        var builder = _triviaBuilder;
        builder.Clear();

        while (IsTrivia(_charStream.Current))
        {
            switch (_charStream.Current)
            {
                case '\r' or '\n' or ';':
                    builder.Add(ReadNewLine());
                    break;
                case '/':
                    var comment = ReadComment();
                    if (comment is null)
                        return builder.ToImmutable();
                    builder.Add(comment);
                    break;
                default:
                    if (char.IsWhiteSpace(_charStream.Current))
                    {
                        var token = ReadToken(c => !IsNewLine(c) && char.IsWhiteSpace(c), _ => SyntaxKind.WhiteSpace);
                        builder.Add(token);
                    }
                    
                    break;
            }
        }

        return builder.ToImmutable();
    }

    private Token? ReadComment()
    {
        Debug.Assert(_charStream.Current == '/');

        if (_charStream.Next != '/')
            return null;

        var start = _charStream.Index;
        
        var builder = new StringBuilder();

        builder.Append(_charStream.Current);
        builder.Append(_charStream.Next);
        
        _charStream.Advance();
        _charStream.Advance();

        while (_charStream.CanAdvance && !IsNewLine(_charStream.Current))
        {
            builder.Append(_charStream.Current);
            _charStream.Advance();
        }

        return new Token(SyntaxKind.Comment, builder.ToString(), start);
    } 
    
    private Token ReadToken(Func<char, bool> predicate, Func<string, SyntaxKind> kindResolver)
    {
        var start = _charStream.Index;
        while (predicate.Invoke(_charStream.Current)) 
            _charStream.Advance();

        var lexeme = _charStream.Text[start.._charStream.Index];

        var kind = kindResolver(lexeme);

        return new Token(kind, lexeme, start);
    }
    
    private Token ReadNewLine()
    {
        if (_charStream.Current == '\r')
        {
            _charStream.Advance();
            if (_charStream.Current != '\n')
                return new Token(SyntaxKind.NewLine, "\r", _charStream.Index - 1);
            
            _charStream.Advance();
            return new Token(SyntaxKind.NewLine, "\r\n", _charStream.Index - 2);

        }

        _charStream.Advance();
        return new Token(SyntaxKind.NewLine, "\n", _charStream.Index - 1);
    }

    private bool IsTrivia(char c)
    {
        return IsNewLine(c) || char.IsWhiteSpace(c) || c == '/';
    }

    private bool IsNewLine(char c)
    {
        return c is '\r' or '\n';
    }

    public IEnumerable<Token> ReadAll()
    {
        while (_charStream.Current != '\0')
        {
            yield return Read();
        }
    }
}