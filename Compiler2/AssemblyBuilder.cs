using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using Compiler;

namespace Compiler2;

public class AssemblyBuilder
{
    private readonly SortedList<ushort, object> _constants = new();
    private readonly Dictionary<Function, ByteCodeBuilder> _builders = new();
    private Function? _context;

    private ushort AddConstant(bool value)
    {
        ushort id;
        if (!_constants.ContainsValue(value))
        {
            id = (ushort)_constants.Count;
            _constants.Add(id, value);
        }
        else
        {
            id = _constants.FirstOrDefault(c => value.Equals(c.Value)).Key;
        }

        return id;
    }

    private ushort AddConstant(double value)
    {
        ushort id;
        if (!_constants.ContainsValue(value))
        {
            id = (ushort)_constants.Count;
            _constants.Add(id, value);
        }
        else
        {
            id = _constants.FirstOrDefault(c => value.Equals(c.Value)).Key;
        }

        return id;
    }

    private ushort AddConstant(string value)
    {
        ushort id;
        if (!_constants.ContainsValue(value))
        {
            id = (ushort)_constants.Count;
            _constants.Add(id, value);
        }
        else
        {
            id = _constants.FirstOrDefault(c => value.Equals(c.Value)).Key;
        }

        return id;
    }
    
    public IDisposable SetContext(Function function)
    {
        _context = function;
        return Disposable.Create(() => _context = null);
    }
    
    public void AddOpLoadConstant(bool value)
    {
        if (_context is null)
            throw new InvalidOperationException("Context is unset");

        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpLoadConst, AddConstant(value));
    }

    public void AddOpLoadConstant(double value)
    {
        ThrowIfContextIsNull();

        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpLoadConst, AddConstant(value));
    }

    public void AddOpLoadConstant(string value)
    {
        ThrowIfContextIsNull();

        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpLoadConst, AddConstant(value));
    }

    private ByteCodeBuilder GetBuilder()
    {
        return _builders.GetValueOrDefault(_context!) ?? (_builders[_context!] = new ByteCodeBuilder());
    }

    public void AddOpLoadVariable(string name)
    {
        ThrowIfContextIsNull();

        var variable = _context.Variables.First(v => v.Name == name);
        
        var builder = GetBuilder();
        
        builder.AddOperation(Opcodes.OpLocalLoad, _context.GetVariableID(variable));
    }

    [MemberNotNull(nameof(_context))]
    private void ThrowIfContextIsNull()
    {
        if (_context is null)
            throw new InvalidOperationException("Context is unset");
    }

    public void AddOpSetVariable(string name)
    {
        ThrowIfContextIsNull();
        
        var variable = _context.Variables.First(v => v.Name == name);
        
        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpLocalSet, _context.GetVariableID(variable));
    }

    public void AddOpCallFunction(string name, IEnumerable<Type> parameters)
    {
        
    }

    public void AddOpReturn()
    {
        
    }

    public void AddOpJump(ushort address)
    {
        
    }

    public void AddOpJumpTrue(ushort address)
    {
        
    }

    public void AddOpJumpFalse(ushort address)
    {
        
    }

    public void AddOpCast(Type from, Type to)
    {
        
    }

    public void AddOpAdd()
    {
        
    }

    public void AddOpSubtract()
    {
        
    }

    public void AddOpMultiply()
    {
        
    }

    public void AddOpDivide()
    {
        
    }

    public void AddOpCompare(string @operator)
    {
        
    }

    public void AddOpAnd()
    {
        
    }

    public void AddOpOr()
    {
        
    }

    public MemoryStream Build()
    {
        throw new NotImplementedException();
    }
    
    private class ByteCodeBuilder
    {
        private readonly List<Operation> _operations = new();

        public void AddOperation(byte opcode)
        {
            _operations.Add(new Operation(opcode));
        }

        public void AddOperation<T>(byte opcode, T argument)
        {
            _operations.Add(new Operation<T>(opcode, argument));
        }

        public void AddOperation<T1, T2>(byte opcode, T1 argument1, T2 argument2)
        {
            _operations.Add(new Operation<T1, T2>(opcode, argument1, argument2));
        }
        
        private class Operation
        {
            public byte Opcode { get; }

            public Operation(byte opcode)
            {
                Opcode = opcode;
            }
        }
        
        private class Operation<T> : Operation
        {
            public T Argument { get; }

            public Operation(byte opcode, T argument) : base(opcode)
            {
                Argument = argument;
            }
        }
        
        private class Operation<T1, T2> : Operation
        {
            public T1 Argument1 { get; }
            public T2 Argument2 { get; }
            public Operation(byte opcode, T1 argument1, T2 argument2) : base(opcode)
            {
                Argument1 = argument1;
                Argument2 = argument2;
            }
        }
    }
}