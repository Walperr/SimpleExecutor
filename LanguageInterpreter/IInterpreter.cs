using System.Diagnostics.CodeAnalysis;
using LanguageParser.Common;

namespace LanguageInterpreter;

public interface IInterpreter
{
    void Initialize(string source);
    Result<SyntaxException, object> Interpret(CancellationToken? token);
    
    SyntaxException? Error { get; }
    
    [MemberNotNullWhen(true, nameof(Error))]
    public bool HasErrors => Error is not null;
}