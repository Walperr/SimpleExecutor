
using System.Globalization;
using LanguageParser.Expressions;
using Xunit.Abstractions;

namespace LanguageInterpreter;

public static class Evaluator
{
    private const string PrintFunction = "print";
    
    public static void InvokeExpression(InvocationExpression invocationExpression, ITestOutputHelper testOutputHelper)
    {
        var functionName = ((FunctionExpression)invocationExpression.Function).Token.Lexeme;

        if (functionName != PrintFunction)
            throw new Exception($"Unknown function {functionName}");
        
        foreach (var value in invocationExpression.Arguments.Select(GetValue).Where(value => value is not null))
            testOutputHelper.WriteLine(value?.ToString());
    }
    
    public static void InvokeExpression(InvocationExpression invocationExpression)
    {
        var functionName = ((FunctionExpression)invocationExpression.Function).Token.Lexeme;

        if (functionName != PrintFunction)
            throw new Exception($"Unknown function {functionName}");
        
        foreach (var value in invocationExpression.Arguments.Select(GetValue).Where(value => value is not null))
            Console.WriteLine(value);
    }

    private static object? GetValue(ExpressionBase argument)
    {
        if (argument is not ConstantExpression constant) 
            return null;
        
        if (constant.IsString)
            return constant.Lexeme;
        
        if (!constant.IsNumber)
            return null;
        
        if (constant.Lexeme.Contains('.'))
            return double.Parse(constant.Lexeme, CultureInfo.InvariantCulture);
                
        return int.Parse(constant.Lexeme, CultureInfo.InvariantCulture);

    }
}