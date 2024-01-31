using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Text;
using Compiler.Exceptions;
using LanguageParser.Common;

namespace Compiler;

public class AssemblyBuilder
{
    private readonly Dictionary<Function, ByteCodeBuilder> _builders = new();
    private readonly SortedList<int, object> _constants = new();

    private readonly Stack<Function> _contextStack = new();

    private readonly Dictionary<Function, List<Marker>> _markers = new();
    private readonly List<Type> _types = new();
    private Function? _context;

    public AssemblyBuilder()
    {
        _types.Add(PrimitiveTypes.Struct);
        _types.Add(PrimitiveTypes.Boolean);
        _types.Add(PrimitiveTypes.Byte);
        _types.Add(PrimitiveTypes.Int16);
        _types.Add(PrimitiveTypes.Int32);
        _types.Add(PrimitiveTypes.Int64);
        _types.Add(PrimitiveTypes.Single);
        _types.Add(PrimitiveTypes.Double);
        _types.Add(PrimitiveTypes.Array);
        _types.Add(PrimitiveTypes.String);
        _types.Add(PrimitiveTypes.Empty);
    }
    
    private int AddConstant(bool value)
    {
        int id;
        if (!_constants.ContainsValue(value))
        {
            id = _constants.Count;
            _constants.Add(id, value);
        }
        else
        {
            id = _constants.FirstOrDefault(c => value.Equals(c.Value)).Key;
        }

        return id;
    }

    private int AddConstant(double value)
    {
        int id;
        if (!_constants.ContainsValue(value))
        {
            id = _constants.Count;
            _constants.Add(id, value);
        }
        else
        {
            id = _constants.FirstOrDefault(c => value.Equals(c.Value)).Key;
        }

        return id;
    }

    private int AddConstant(string value)
    {
        int id;
        if (!_constants.ContainsValue(value))
        {
            id = _constants.Count;
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
        if (_context is not null)
            _contextStack.Push(_context);

        _context = function;

        return Disposable.Create(() =>
        {
            if (!_contextStack.TryPop(out _context))
                _context = null;
        });
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

    public void AddOpCallFunction(string name, IEnumerable<Type> parameters, Scope? scope)
    {
        ThrowIfContextIsNull();

        if (scope is null)
            throw new CompilerException($"Function {name} not found in current context", new StringRange());

        var function = scope.GetFunctionIncludingAncestors(name, parameters) ??
                       throw new CompilerException($"Function {name} not found in current context", new StringRange());

        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpCall, function.ID);
    }

    public void AddOpReturn()
    {
        ThrowIfContextIsNull();

        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpReturn);
    }

    public void AddOpJump(int address)
    {
        ThrowIfContextIsNull();

        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpJump, address);
    }

    public void AddOpJumpTrue(int address)
    {
        ThrowIfContextIsNull();

        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpJumpIfTrue, address);
    }

    public void AddOpJumpFalse(int address)
    {
        ThrowIfContextIsNull();

        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpJumpIfFalse, address);
    }

    public void AddOpCast(Type from, Type to)
    {
        ThrowIfContextIsNull();

        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpCast, from.ID, to.ID);
    }

    public void AddOpAdd()
    {
        ThrowIfContextIsNull();

        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpFloatAdd2);
    }

    public void AddOpSubtract()
    {
        ThrowIfContextIsNull();

        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpFloatSubtract2);
    }

    public void AddOpMultiply()
    {
        ThrowIfContextIsNull();

        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpFloatMultiply2);
    }

    public void AddOpDivide()
    {
        ThrowIfContextIsNull();

        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpFloatDivide2);
    }

    public void AddOpSetField(Type type, int variableNumber)
    {
        ThrowIfContextIsNull();

        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpFieldSet, type.ID, variableNumber);
    }

    public void AddOpCompare(string @operator)
    {
        ThrowIfContextIsNull();

        var builder = GetBuilder();

        switch (@operator)
        {
            case ">":
                builder.AddOperation(Opcodes.OpCompareGreaterF2);
                break;
            case "<":
                builder.AddOperation(Opcodes.OpCompareLessF2);
                break;
            case ">=":
            case "<=":
                throw new NotImplementedException();
        }
    }

    public void AddOpAnd()
    {
        ThrowIfContextIsNull();

        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpAnd);
    }

    public void AddOpOr()
    {
        ThrowIfContextIsNull();

        var builder = GetBuilder();

        builder.AddOperation(Opcodes.OpOr);
    }

    public void AddOpEquals(Type type)
    {
        ThrowIfContextIsNull();

        var builder = GetBuilder();

        switch (type.Size)
        {
            case 1:
                builder.AddOperation(Opcodes.OpCompareEquals);
                break;
            case 2:
                builder.AddOperation(Opcodes.OpCompareEquals2);
                break;
            case 4:
                builder.AddOperation(Opcodes.OpCompareEquals4);
                break;
            case 8:
                builder.AddOperation(Opcodes.OpCompareEquals8);
                break;
            default:
                throw new NotImplementedException();
        }
    }
    
    public Marker GetMarker()
    {
        return new Marker(this);
    }

    public (byte[] code, byte[] constants, byte[] types, byte[] functions) Build()
    {
        using var codeStream = new MemoryStream();
        using var codeWriter = new BinaryWriter(codeStream);

        using var constantsStream = new MemoryStream();
        using var constantsWriter = new BinaryWriter(constantsStream);

        using var typesStream = new MemoryStream();
        using var typesWriter = new BinaryWriter(typesStream);

        using var functionsStream = new MemoryStream();
        using var functionsWriter = new BinaryWriter(functionsStream);
        
        constantsWriter.Write(_constants.Count);
        
        foreach (var (_, value) in _constants)
        {
            switch (value)
            {
                case bool boolean:
                    constantsWriter.Write(sizeof(bool));
                    constantsWriter.Write(boolean);
                    break;
                case double @double:
                    constantsWriter.Write(sizeof(double));
                    constantsWriter.Write(@double);
                    break;
                case string @string:
                    constantsWriter.Write(@string.Length * sizeof(char));
                    constantsWriter.Write(@string);
                    break;
            }
        }

        typesWriter.Write(_types.Count);
        
        foreach (var type in _types)
        {
            typesWriter.Write(type.ID);
            typesWriter.Write(type.IsPrimitive);
            typesWriter.Write((byte)type.PrimitiveType);
            typesWriter.Write(type.Size);
            typesWriter.Write(type.Fields.Length);
            foreach (var field in type.Fields)
                typesWriter.Write(field.Type.ID);
        }

        codeWriter.Write(Opcodes.OpCall);
        codeWriter.Write(0);
        codeWriter.Write(Opcodes.OpHalt);
        
        var offset = (int)codeStream.Length;
        
        functionsWriter.Write(_builders.Count);

        foreach (var (function, builder) in _builders.OrderBy(p => p.Key.ID))
        {
            offset = (int)codeStream.Position;
            var bytes = builder.Build(offset);

            // offset += bytes.Length;

            functionsWriter.Write(function.ID);
            functionsWriter.Write((int)codeStream.Length);
            functionsWriter.Write(function.ParametersCount);
            functionsWriter.Write(function.VariablesCount);
            foreach (var variable in function.Variables)
                functionsWriter.Write(variable.Type.ID);
            
            codeWriter.Write(bytes);
        }

        return (code: codeStream.ToArray(), constants: constantsStream.ToArray(), types: typesStream.ToArray(), functions: functionsStream.ToArray());
    }

    public string BuildIlPreview()
    {
        var sb = new StringBuilder();

        return sb.ToString();
    }

    public sealed class Marker : IDisposable
    {
        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly ByteCodeBuilder _builder;
        private readonly Function _context;
        private readonly List<Marker> _markers;

        internal Marker(AssemblyBuilder assemblyBuilder)
        {
            assemblyBuilder.ThrowIfContextIsNull();

            _assemblyBuilder = assemblyBuilder;
            _context = assemblyBuilder._context;

            _builder = assemblyBuilder.GetBuilder();

            Value = _builder.GetMarker();

            if (!assemblyBuilder._markers.TryGetValue(assemblyBuilder._context, out var markers))
                assemblyBuilder._markers[assemblyBuilder._context] = markers = new List<Marker>();

            _markers = markers;
            markers.Add(this);
        }

        public int Value { get; private set; }

        public void Dispose()
        {
            _markers.Remove(this);

            if (!_markers.Any())
                _assemblyBuilder._markers.Remove(_context);
        }

        public void SetOperation(byte opcode)
        {
            _builder.InsertOperation(opcode, Value);

            foreach (var marker in _markers.Where(m => m.Value >= Value).ToArray())
                marker.Value++;
        }

        public void SetOperation(byte opcode, int argument)
        {
            _builder.InsertOperation(opcode, argument, Value);

            foreach (var marker in _markers.Where(m => m.Value >= Value).ToArray())
                marker.Value++;
        }

        public void SetOperation(byte opcode, int argument1, int argument2)
        {
            _builder.InsertOperation(opcode, argument1, argument2, Value);

            foreach (var marker in _markers.Where(m => m.Value >= Value).ToArray())
                marker.Value++;
        }
    }

    private class ByteCodeBuilder
    {
        private readonly List<Operation> _operations = new();

        public int GetMarker()
        {
            return _operations.Count;
        }

        public byte[] Build(int offset)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            for (var i = 0; i < _operations.Count; i++)
            {
                var operation = _operations[i];
                switch (operation)
                {
                    case { Argument1: null, Argument2: null }:
                        writer.Write(operation.Opcode);
                        break;

                    case { Argument1: not null, Argument2: null }:
                        writer.Write(operation.Opcode);
                        if (operation.Opcode is Opcodes.OpJump or Opcodes.OpJumpIfFalse or Opcodes.OpJumpIfTrue)
                        {
                            var targetInstruction = operation.Argument1.Value;
                            var address = (int)stream.Position + offset + 4;
                            for (int j = i + 1; j < targetInstruction; j++)
                            {
                                var op = _operations[j];
                                address += 1;
                                if (op.Argument1 is not null)
                                    address += 4;
                                if (op.Argument2 is not null)
                                    address += 4;
                            }
                            var argument = address;
                            writer.Write(argument);
                        }
                        else
                        {
                            writer.Write(operation.Argument1.Value);
                        }
                        break;

                    case { Argument1: not null, Argument2: not null }:
                        writer.Write(operation.Opcode);
                        writer.Write(operation.Argument1.Value);
                        
                        if (operation.Opcode is Opcodes.OpCast)
                            writer.Write(operation.Argument2.Value);
                        else
                            writer.Write(operation.Argument2.Value);
                        break;
                }
            }

            return stream.ToArray();
        }

        public void InsertOperation(byte opcode, int index)
        {
            _operations.Insert(index, new Operation(opcode));
        }

        public void InsertOperation(byte opcode, int argument, int index)
        {
            _operations.Insert(index, new Operation(opcode, argument));
        }

        public void InsertOperation(byte opcode, int argument1, int argument2, int index)
        {
            _operations.Insert(index, new Operation(opcode, argument1, argument2));
        }

        public void AddOperation(byte opcode)
        {
            _operations.Add(new Operation(opcode));
        }

        public void AddOperation(byte opcode, int argument)
        {
            _operations.Add(new Operation(opcode, argument));
        }

        public void AddOperation(byte opcode, int argument1, int argument2)
        {
            _operations.Add(new Operation(opcode, argument1, argument2));
        }

        public class Operation
        {
            public Operation(byte opcode)
            {
                Opcode = opcode;
            }

            public Operation(byte opcode, int argument1, int argument2)
            {
                Opcode = opcode;
                Argument1 = argument1;
                Argument2 = argument2;
            }

            public Operation(byte opcode, int argument)
            {
                Opcode = opcode;
                Argument1 = argument;
            }

            public byte Opcode { get; }
            
            public int? Argument1 { get; }
            
            public int? Argument2 { get; }
        }
    }
}