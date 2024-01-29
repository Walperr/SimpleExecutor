using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Interfaces;

namespace Compiler2.Exceptions;

public class CompilerException : SyntaxException
{
    protected internal CompilerException(string message, StringRange range) : base(message, range)
    {
    }
}

public sealed class IncompatibleOperandsException : CompilerException
{
    internal IncompatibleOperandsException(ConstantExpression @operator) : base(
        $"Incompatible left and right operands of '{@operator.Lexeme}'", @operator.Range)
    {
    }
    
    internal IncompatibleOperandsException(ConstantExpression @operator, Type expectedType) : base(
        $"Incompatible left and right operands of '{@operator.Lexeme}'. Expected type {expectedType}", @operator.Range)
    {
    }
}

public sealed class ExpectedOtherTypeException : CompilerException
{
    internal ExpectedOtherTypeException(ISyntaxElement element, Type actualType, Type expected) : base(
        $"Expected type {expected}, got {actualType}", element.Range)
    {
    }
}

public sealed class UnexpectedOperandsException : CompilerException
{
    internal UnexpectedOperandsException(ConstantExpression @operator, Type actualType) : base(
        $"Operands of '{@operator.Lexeme}' cannot be {actualType}",
        @operator.Range)
    {
    }
}

public sealed class UninitializedVariableException : CompilerException
{
    internal UninitializedVariableException(string variableName, StringRange range) : base(
        $"Variable {variableName} is uninitialized", range)
    {
    }
}

public sealed class UndeclaredVariableException : CompilerException
{
    internal UndeclaredVariableException(string variableName, StringRange range) : base(
        $"Variable {variableName} is undeclared", range)
    {
    }
}

public sealed class UndeclaredFunctionException : CompilerException
{
    internal UndeclaredFunctionException(string functionName, StringRange range) : base(
        $"Function {functionName} is undeclared", range)
    {
    }
}

public sealed class WrongFunctionArgumentException : CompilerException
{
    internal WrongFunctionArgumentException(Type argumentType, string functionName, StringRange range) : base(
        $"wrong argument of type {argumentType} in function {functionName}", range)
    {
    }
}

public sealed class UnexpectedOperatorException : CompilerException
{
    internal UnexpectedOperatorException(string operatorLexeme, StringRange range) : base(
        $"Unexpected operator '{operatorLexeme}'", range)
    {
    }
}

public sealed class UnhandledCompilerException : CompilerException
{
    internal UnhandledCompilerException(Exception exception) : base(exception.Message, default)
    {
    }
}