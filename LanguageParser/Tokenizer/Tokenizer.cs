using System.Collections.Immutable;
using System.Text;
using LanguageParser.Common;
using LanguageParser.Interfaces;

namespace LanguageParser.Tokenizer;

internal sealed class Tokenizer
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
            case ',':
                return SimpleToken(SyntaxKind.Comma);
            case '"' or '\'':
                return StringToken();
            case var c:
            {
                if (IsOperator(c))
                    return ReadToken(IsOperator, GetOperatorByLexeme);

                if (!IsSpecial(c))
                    return IsDigitOrDot(_charStream.Current)
                        ? ReadToken(IsDigitOrDot, _ => SyntaxKind.Number)
                        : ReadToken(IsWord, _ => SyntaxKind.Word);
                
                var token = new Token(SyntaxKind.Error, c.ToString(), _charStream.Index);
                _charStream.Advance();
                return token;

            }
        }
    }

    private SyntaxKind GetOperatorByLexeme(string lexeme)
    {
        return lexeme switch
        {
            "/" => throw new Exception("Slash"),
            "*" => throw new Exception("Asterisk"),
            "+" => throw new Exception("Plus"),
            "-" => throw new Exception("Minus"),
            "&&" => throw new Exception("LogicalAnd"),
            "||" => throw new Exception("LogicalOr"),
            "=" => throw new Exception("Equals sign"),
            "=>" => throw new Exception("Arrow")
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
        return c is '+' or '-' or '*' or '/' or '~' or '=' or '&' or '|' or '!' or '<' or '>';
    }

    private bool IsDigitOrDot(char c)
    {
        return char.IsDigit(c) || c == '.';
    }

    private Token StringToken()
    {
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
                case '"':
                    _charStream.Advance();
                    return new Token(SyntaxKind.String, builder.ToString(), start);
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
                case '\r' or '\n':
                    builder.Add(NewLine());
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

    private Token ReadToken(Func<char, bool> predicate, Func<string, SyntaxKind> kindResolver)
    {
        var start = _charStream.Index;
        while (predicate.Invoke(_charStream.Current)) 
            _charStream.Advance();

        var lexeme = _charStream.Text[start.._charStream.Index];

        var kind = kindResolver(lexeme);

        return new Token(kind, lexeme, start);
    }
    
    private Token NewLine()
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
        return IsNewLine(c) || char.IsWhiteSpace(c);
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