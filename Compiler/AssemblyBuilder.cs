using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Text;
using Compiler.Exceptions;
using LanguageParser.Common;

namespace Compiler;

public class AssemblyBuilder
{
    private readonly string _name;
    private readonly Version _version;
    private readonly string _developer;
    private readonly Dictionary<Function, ByteCodeBuilder> _builders = new();
    private readonly SortedList<int, object> _constants = new();

    private readonly Stack<Function> _contextStack = new();

    private readonly Dictionary<Function, List<Marker>> _markers = new();
    private readonly HashSet<Type> _types = new();
    private Function? _context;

    public AssemblyBuilder(string name, string? developer = null, Version? version = null)
    {
        _name = name;
        _developer = developer ?? "default company";
        _version = version ?? new Version(0, 0, 0, 0);

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
        return _builders.GetValueOrDefault(_context!) ?? (_builders[_context!] = new ByteCodeBuilder(this));
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

    public void BuildNew(Stream outStream, bool fixAddresses = false)
    {
        using var outWriter = new BinaryWriter(outStream);
        
        outWriter.Write(_name);
        outWriter.Write(_developer);
        outWriter.Write(_version.Major);
        outWriter.Write(_version.Minor);
        outWriter.Write(_version.Build);
        outWriter.Write(_version.Revision);

        const int mainEntryPoint = 0;
        
        outWriter.Write(mainEntryPoint);
        
        outWriter.Write(_constants.Count);

        outWriter.Write(_types.Count);

        const int referencedTypesCount = 0;

        outWriter.Write(referencedTypesCount);

        outWriter.Write(_builders.Count);

        const int referencedFunctionsCount = 0;

        outWriter.Write(referencedFunctionsCount);

        var codeSizePosition = outStream.Position;

        outWriter.Write(default(int)); // reserve for code size
        outWriter.Write(default(int)); // reserve for constants ptr
        outWriter.Write(default(int)); // reserve for typedef ptr
        outWriter.Write(default(int)); // reserve for typeref ptr
        outWriter.Write(default(int)); // reserve for funcdef ptr
        outWriter.Write(default(int)); // reserve for funcref ptr
        outWriter.Write(default(int)); // reserve for code ptr

        var constantsPtr = (int)outStream.Position;
        
        foreach (var (_, value) in _constants)
        {
            switch (value)
            {
                case bool boolean:
                    outWriter.Write(PrimitiveTypes.Boolean.ID);
                    outWriter.Write(boolean);
                    break;
                case double @double:
                    outWriter.Write(PrimitiveTypes.Double.ID);
                    outWriter.Write(@double);
                    break;
                case string @string:
                    outWriter.Write(PrimitiveTypes.String.ID);
                    outWriter.Write(@string);
                    break;
            }
        }

        var typedefPtr = (int)outStream.Position;

        foreach (var type in _types)
        {
            outWriter.Write(type.ID);
            outWriter.Write(string.Empty); // empty namespace
            outWriter.Write(type.Name);
            outWriter.Write(type.IsPrimitive);
            outWriter.Write((byte)type.PrimitiveType);
            outWriter.Write(type.Size);
            outWriter.Write(type.Fields.Length);
            foreach (var field in type.Fields)
            {
                outWriter.Write(field.Type.ID);
                outWriter.Write(field.Name);
            }
        }

        var typerefPtr = (int)outStream.Position;
        var funcdefPtr = (int)outStream.Position;

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        
        writer.Write((byte)Opcodes.OpCall);
        writer.Write(mainEntryPoint);
        writer.Write((byte)Opcodes.OpHalt);

        foreach (var (function, builder) in _builders.OrderBy(p => p.Key.ID))
        {
            var offset = (int)stream.Position;
            builder.Build(writer, fixAddresses);

            outWriter.Write(function.ID);
            outWriter.Write(string.Empty); // empty namespace
            outWriter.Write(function.Name);
            
            outWriter.Write(function.ReturnType!.ID);
            
            outWriter.Write(offset);
            
            outWriter.Write(function.ParametersCount);
            outWriter.Write(function.VariablesCount);
            foreach (var variable in function.Variables)
            {
                outWriter.Write(variable.Type.ID);
                outWriter.Write(variable.Name);
            }
        }

        var funcrefPtr = (int)outStream.Position;
        var codePtr = (int)outStream.Position;

        outStream.Position = codeSizePosition;
        outWriter.Write((int)stream.Length);
        
        outWriter.Write(constantsPtr);
        outWriter.Write(typedefPtr);
        outWriter.Write(typerefPtr);
        outWriter.Write(funcdefPtr);
        outWriter.Write(funcrefPtr);
        outWriter.Write(codePtr);

        outStream.Position = codePtr;
        stream.Position = 0;
        
        stream.CopyTo(outStream);
    }

    public static AssemblyBuilder LoadAssembly(Stream stream)
    {
        using var reader = new BinaryReader(stream);

        var name = reader.ReadString();
        var developer = reader.ReadString();
        var version = new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

        var builder = new AssemblyBuilder(name, developer, version);

        var entryPoint = reader.ReadInt32(); // entry point

        var constantsCount = reader.ReadInt32();
        var typesCount = reader.ReadInt32();
        var referencedTypesCount = reader.ReadInt32();
        var functionsCount = reader.ReadInt32();
        var referencedFunctionsCount = reader.ReadInt32();

        var codeSize = reader.ReadInt32();
        var constantsPtr = reader.ReadInt32();
        var typedefPtr = reader.ReadInt32();
        var typerefPtr = reader.ReadInt32();
        var funcdefPtr = reader.ReadInt32();
        var funcrefPtr = reader.ReadInt32();
        var codePtr = reader.ReadInt32();

        for (int i = 0; i < constantsCount; i++)
        {
            var type = reader.ReadInt32();

            if (type == PrimitiveTypes.Boolean.ID) 
                builder.AddConstant(reader.ReadBoolean());
            else if (type == PrimitiveTypes.Double.ID)
                builder.AddConstant(reader.ReadDouble());
            else if (type == PrimitiveTypes.String.ID)
                builder.AddConstant(reader.ReadString());
            else
                throw new InvalidDataException("Invalid constant type");
        }

        for (int i = 0; i < typesCount; i++)
        {
            var id = reader.ReadInt32();
            var @namespace = reader.ReadString();
            var typeName = reader.ReadString();
            var isPrimitive = reader.ReadBoolean();
            var primitiveType = reader.ReadByte();
            var size = reader.ReadInt32();
            var fieldsCount = reader.ReadInt32();

            var type = new Type
            {
                ID = id,
                Name = typeName,
                IsPrimitive = isPrimitive,
                PrimitiveType = (PrimitiveType)primitiveType,
                Size = size,
                Fields = new Variable[fieldsCount],
                IsGenericType = id == PrimitiveTypes.Array.ID
            };

            for (int j = 0; j < fieldsCount; j++)
            {
                var fieldType = reader.ReadInt32();
                var fieldName = reader.ReadString();

                type.Fields[j] = new Variable(PrimitiveTypes.None with { ID = fieldType }, fieldName);
            }
            
            builder._types.Add(type);
        }

        var fields = from type in builder._types
            from field in type.Fields
            where field.Type.Name == PrimitiveTypes.None.Name
            select field;
        
        foreach (var field in fields)
            field.Type = builder._types.First(t => t.ID == field.Type.ID);

        var functions = new List<(int ip, Function function)>();

        for (int i = 0; i < functionsCount; i++)
        {
            var id = reader.ReadInt32();
            var @namespace = reader.ReadString();
            var functionName = reader.ReadString();
            var returnTypeID = reader.ReadInt32();
            var ip = reader.ReadInt32();
            var parametersCount = reader.ReadInt32();
            var variablesCount = reader.ReadInt32();

            var function = new Function(functionName, id, parametersCount);

            function.SetReturnType(builder._types.First(t => t.ID == returnTypeID));

            for (int j = 0; j < variablesCount; j++)
            {
                var variableType = reader.ReadInt32();
                var variableName = reader.ReadString();

                function.AddVariable(new Variable(builder._types.First(t => t.ID == variableType), variableName));
            }

            functions.Add((ip, function));
        }

        var opCall = reader.ReadByte();
        var mainID = reader.ReadInt32();
        var opHalt = reader.ReadByte();

        var address = sizeof(int) + 2 * sizeof(byte);

        if (mainID != entryPoint)
            throw new InvalidDataException("Wrong entry point");

        for (var i = 0; i < functions.Count; i++)
        {
            var (ip, function) = functions[i];
            var nextIp = i < functionsCount - 1 ? functions[i + 1].ip : codeSize;
            
            using var _ = builder.SetContext(function);

            var instructionBuilder = builder.GetBuilder();

            while (address < nextIp)
            {
                var opcode = (Opcodes)reader.ReadByte();

                if (opcode is Opcodes.OpCast or Opcodes.OpFieldSet or Opcodes.OpFieldLoad)
                {
                    var argument1 = reader.ReadInt32();
                    var argument2 = reader.ReadInt32();
                    
                    instructionBuilder.AddOperation(opcode, argument1, argument2);

                    address += sizeof(byte) + sizeof(int) * 2;
                }
                else if (opcode is Opcodes.OpLoadConst or Opcodes.OpLocalLoad or Opcodes.OpLocalSet or Opcodes.OpCall)
                {
                    var argument1 = reader.ReadInt32();
                    
                    instructionBuilder.AddOperation(opcode, argument1);
                    
                    address += sizeof(byte) + sizeof(int);
                }
                else if (opcode is Opcodes.OpJump or Opcodes.OpJumpIfFalse or Opcodes.OpJumpIfTrue)
                {
                    var argument1 = reader.ReadInt32();
                    
                    instructionBuilder.AddOperation(opcode, argument1 - ip - sizeof(int)); //todo: make conversion between address and instruction number
                    
                    address += sizeof(byte) + sizeof(int);
                }
                else
                {
                    instructionBuilder.AddOperation(opcode);
                    
                    address += sizeof(byte);
                }
            }
        }

        return builder;
    }
    
    public void Build(Stream outStream)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        using var outWriter = new BinaryWriter(outStream);
        
        writer.Write(_constants.Count);
        
        foreach (var (_, value) in _constants)
        {
            switch (value)
            {
                case bool boolean:
                    writer.Write(sizeof(bool));
                    writer.Write(boolean);
                    break;
                case double @double:
                    writer.Write(sizeof(double));
                    writer.Write(@double);
                    break;
                case string @string:
                    writer.Write(@string.Length * sizeof(char));
                    writer.Write(@string);
                    break;
            }
        }

        var constantsSize = (int)stream.Length;
        
        writer.Write(_types.Count);
        
        foreach (var type in _types)
        {
            writer.Write(type.ID);
            writer.Write(type.IsPrimitive);
            writer.Write((byte)type.PrimitiveType);
            writer.Write(type.Size);
            writer.Write(type.Fields.Length);
            foreach (var field in type.Fields)
                writer.Write(field.Type.ID);
        }

        var typesSize = (int)stream.Position - constantsSize;
        
        outWriter.Write(constantsSize);
        outWriter.Write(typesSize);
        outWriter.Write(default(int));
        outWriter.Write(default(int));

        stream.Position = 0;
        stream.CopyTo(outStream);
        stream.SetLength(0);

        outWriter.Write(_builders.Count);
        
        writer.Write((byte)Opcodes.OpCall);
        writer.Write(0);
        writer.Write((byte)Opcodes.OpHalt);
        
        foreach (var (function, builder) in _builders.OrderBy(p => p.Key.ID))
        {
            var offset = (int)stream.Position;
            builder.Build(writer, true);

            outWriter.Write(function.ID);
            outWriter.Write(offset);
            outWriter.Write(function.ParametersCount);
            outWriter.Write(function.VariablesCount);
            foreach (var variable in function.Variables)
                outWriter.Write(variable.Type.ID);
        }


        var functionsSize = (int)outStream.Position - 4 * sizeof(int) - constantsSize - typesSize;

        outStream.Position = 2 * sizeof(int);
        outWriter.Write(functionsSize);
        outWriter.Write(stream.Position);

        stream.Position = 0;
        outStream.Position = outStream.Length; 
        stream.CopyTo(outStream);
    }

    public string Build(bool fixAddresses = false)
    {
        var sb = new StringBuilder();

        const string locals = "locals:";
        const string byteCode = "code:";
        
        foreach (var (function, builder) in _builders.OrderBy(p => p.Key.ID))
        {
            sb.Append(function.ID);
            sb.Append(new[] { ':', '\t' });

            Debug.Assert(function.ReturnType is not null);

            sb.Append(function.ReturnType.Name);
            sb.Append(' ');
            sb.Append(function.Name);
            sb.Append('(');

            for (int i = 0; i < function.ParametersCount; i++)
            {
                var parameter = function.Parameters.ElementAt(i);

                sb.Append(parameter.Type.Name);
                if (i != function.ParametersCount - 1)
                    sb.Append(',');
            }

            sb.Append(new[] { ')', '\n' });

            if (function.VariablesCount > 0)
                sb.AppendLine(locals);

            for (int i = 0; i < function.VariablesCount; i++)
            {
                var variable = function.Variables.ElementAt(i);

                sb.Append('\t');
                sb.Append(i);
                sb.Append(new[] { ':', ' ' });
                sb.Append(variable.Type.Name);
                sb.Append(' ');
                sb.Append(variable.Name);
                sb.Append('\n');
            }
            
            sb.Append('\n');

            sb.AppendLine(byteCode);
            
            builder.Build(sb, fixAddresses);
            
            sb.Append('\n');
        }

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

        public void SetOperation(Opcodes opcode)
        {
            _builder.InsertOperation(opcode, Value);

            foreach (var marker in _markers.Where(m => m.Value >= Value).ToArray())
                marker.Value++;
        }

        public void SetOperation(Opcodes opcode, int argument)
        {
            _builder.InsertOperation(opcode, argument, Value);

            foreach (var marker in _markers.Where(m => m.Value >= Value).ToArray())
                marker.Value++;
        }

        public void SetOperation(Opcodes opcode, int argument1, int argument2)
        {
            _builder.InsertOperation(opcode, argument1, argument2, Value);

            foreach (var marker in _markers.Where(m => m.Value >= Value).ToArray())
                marker.Value++;
        }
    }

    private class ByteCodeBuilder
    {
        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly List<Operation> _operations = new();

        public ByteCodeBuilder(AssemblyBuilder assemblyBuilder)
        {
            _assemblyBuilder = assemblyBuilder;
        }

        public int GetMarker()
        {
            return _operations.Count;
        }

        public void Build(BinaryWriter writer, bool fixAddresses = false)
        {
            var startAddress = (int)writer.BaseStream.Position;
            for (var i = 0; i < _operations.Count; i++)
            {
                var operation = _operations[i];
                switch (operation)
                {
                    case { Argument1: null, Argument2: null }:
                        writer.Write((byte)operation.Opcode);
                        break;

                    case { Argument1: not null, Argument2: null }:
                        writer.Write((byte)operation.Opcode);
                        if (operation.Opcode is Opcodes.OpJump or Opcodes.OpJumpIfFalse or Opcodes.OpJumpIfTrue)
                        {
                            var targetInstruction = operation.Argument1.Value;
                            if (!fixAddresses)
                            {
                                writer.Write(targetInstruction);
                                continue;
                            }

                            var address = startAddress;
                            for (int j = 0; j < targetInstruction; j++)
                            {
                                var op = _operations[j];
                                address += sizeof(byte);
                                if (op.Argument1 is not null)
                                    address += sizeof(int);
                                if (op.Argument2 is not null)
                                    address += sizeof(int);
                            }
                            writer.Write(address);
                        }
                        else
                        {
                            writer.Write(operation.Argument1.Value);
                        }
                        break;

                    case { Argument1: not null, Argument2: not null }:
                        writer.Write((byte)operation.Opcode);
                        writer.Write(operation.Argument1.Value);
                        writer.Write(operation.Argument2.Value);
                        break;
                }
            }
        }

        public void Build(StringBuilder sb, bool fixAddresses = false)
        {
            var curAddress = 0;
            for (var i = 0; i < _operations.Count; i++)
            {
                var operation = _operations[i];
                
                sb.Append('\t');
                sb.Append($"{curAddress:x4}");
                sb.Append(new[] { ':', ' ' });
                sb.Append(operation.Opcode.ToString().Replace("Op", string.Empty));
                sb.Append(new [] {'\t', '\t'});

                curAddress += sizeof(byte);

                if (operation.Argument1 is not null)
                {
                    switch (operation.Opcode)
                    {
                        case Opcodes.OpLoadConst:
                            sb.Append(_assemblyBuilder._constants[operation.Argument1.Value]);
                            break;
                        case Opcodes.OpLocalLoad or Opcodes.OpLocalSet:
                        {
                            var function = _assemblyBuilder._builders.FirstOrDefault(p => p.Value == this).Key;
                            sb.Append(function.Variables.ElementAt(operation.Argument1.Value).Name);
                            break;
                        }
                        case Opcodes.OpJump or Opcodes.OpJumpIfFalse or Opcodes.OpJumpIfTrue:
                        {
                            var targetInstruction = operation.Argument1.Value;
                            sb.Remove(sb.Length - 1, 1);
                            if (!fixAddresses)
                            {
                                sb.Append($"{targetInstruction:x4}\n");
                                continue;
                            }

                            var address = curAddress + sizeof(int);
                            for (int j = i + 1; j < targetInstruction; j++)
                            {
                                var op = _operations[j];
                                address += sizeof(byte);
                                if (op.Argument1 is not null)
                                    address += sizeof(int);
                                if (op.Argument2 is not null)
                                    address += sizeof(int);
                            }

                            sb.Append($"{address:x4}");
                            
                            break;
                        }
                        case Opcodes.OpCall:
                        {
                            var function = _assemblyBuilder._builders
                                .FirstOrDefault(p => p.Key.ID == operation.Argument1.Value).Key;
                            sb.Append(function.Name);
                            break;
                        }
                        default:
                            sb.Append(operation.Argument1);
                            break;
                    }
                    
                    curAddress += sizeof(int);
                }

                if (operation.Argument2 is not null)
                {
                    sb.Append(operation.Argument2);
                    curAddress += sizeof(int);
                }

                sb.Append('\n');
            }
        }

        public void InsertOperation(Opcodes opcode, int index)
        {
            _operations.Insert(index, new Operation(opcode));
        }

        public void InsertOperation(Opcodes opcode, int argument, int index)
        {
            _operations.Insert(index, new Operation(opcode, argument));
        }

        public void InsertOperation(Opcodes opcode, int argument1, int argument2, int index)
        {
            _operations.Insert(index, new Operation(opcode, argument1, argument2));
        }

        public void AddOperation(Opcodes opcode)
        {
            _operations.Add(new Operation(opcode));
        }

        public void AddOperation(Opcodes opcode, int argument)
        {
            _operations.Add(new Operation(opcode, argument));
        }

        public void AddOperation(Opcodes opcode, int argument1, int argument2)
        {
            _operations.Add(new Operation(opcode, argument1, argument2));
        }

        public class Operation
        {
            public Operation(Opcodes opcode)
            {
                Opcode = opcode;
            }

            public Operation(Opcodes opcode, int argument1, int argument2)
            {
                Opcode = opcode;
                Argument1 = argument1;
                Argument2 = argument2;
            }

            public Operation(Opcodes opcode, int argument)
            {
                Opcode = opcode;
                Argument1 = argument;
            }

            public Opcodes Opcode { get; }
            
            public int? Argument1 { get; }
            
            public int? Argument2 { get; }
        }
    }
}