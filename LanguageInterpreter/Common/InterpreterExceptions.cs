using LanguageParser.Common;
using LanguageParser.Expressions;
using LanguageParser.Interfaces;

namespace LanguageInterpreter.Common;

public class InterpreterException : SyntaxException
{
    protected internal InterpreterException(string message, StringRange range) : base(message, range)
    {
    }
}

public sealed class IncompatibleOperandsException : InterpreterException
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

public sealed class ExpectedOtherTypeException : InterpreterException
{
    internal ExpectedOtherTypeException(ISyntaxElement element, Type actualType, Type expected) : base(
        $"Expected type {expected}, got {actualType}", element.Range)
    {
    }
}

public sealed class UnexpectedOperandsException : InterpreterException
{
    internal UnexpectedOperandsException(ConstantExpression @operator, Type actualType) : base(
        $"Operands of '{@operator.Lexeme}' cannot be {actualType}",
        @operator.Range)
    {
    }
}

public sealed class UninitializedVariableException : InterpreterException
{
    internal UninitializedVariableException(string variableName, StringRange range) : base(
        $"Variable {variableName} is uninitialized", range)
    {
    }
}

public sealed class UndeclaredVariableException : InterpreterException
{
    internal UndeclaredVariableException(string variableName, StringRange range) : base(
        $"Variable {variableName} is undeclared", range)
    {
    }
}

public sealed class UndeclaredFunctionException : InterpreterException
{
    internal UndeclaredFunctionException(string functionName, StringRange range) : base(
        $"Function {functionName} is undeclared", range)
    {
    }
}

public sealed class WrongFunctionArgumentException : InterpreterException
{
    internal WrongFunctionArgumentException(Type argumentType, string functionName, StringRange range) : base(
        $"wrong argument of type {argumentType} in function {functionName}", range)
    {
    }
}

public sealed class UnexpectedOperatorException : InterpreterException
{
    internal UnexpectedOperatorException(string operatorLexeme, StringRange range) : base(
        $"Unexpected operator '{operatorLexeme}'", range)
    {
    }
}

public sealed class UnhandledInterpreterException : InterpreterException
{
    internal UnhandledInterpreterException(Exception exception) : base(exception.Message, default)
    {
    }
}