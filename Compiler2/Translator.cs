using System.Globalization;
using LanguageParser.Expressions;
using LanguageParser.Visitors;

namespace Compiler2;

public class Translator : ExpressionWalker
{
    private readonly Scope _root;
    private readonly AssemblyBuilder _builder = new();
    private Function _context;

    private Translator(Scope scope)
    {
        _root = scope;
        _context = scope.Context;
    }

    private void Translate()
    {
        Visit(_root.Expression);
    }

    public override void VisitConstant(ConstantExpression expression)
    {
        using var _ = _builder.SetContext(_context);

        if (expression.IsBool)
        {
            _builder.AddOpLoadConstant(bool.Parse(expression.Lexeme));
        }
        else if (expression.IsNumber)
        {
            _builder.AddOpLoadConstant(
                double.Parse(expression.Lexeme, NumberStyles.Float, CultureInfo.InvariantCulture));
        }
        else if (expression.IsString)
        {
            _builder.AddOpLoadConstant(expression.Lexeme);
        }
        else
        {
            _builder.AddOpLoadVariable(expression.Lexeme);
        }
    }
    
    
}