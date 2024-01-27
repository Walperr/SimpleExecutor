namespace Compiler;

public sealed record Type
{
    public required string Name { get; init; }

    public required ushort ID { get; init; }

    public required bool IsPrimitive { get; init; }

    public required PrimitiveType PrimitiveType { get; init; }

    public required Type[] Fields { get; init; }

    public required uint Size { get; init; }

    public required bool IsGenericType { get; init; }

    public Type? GenericArgument { get; init; }
}

public enum PrimitiveType : byte
{
    Struct,
    Boolean,
    Byte,
    Int16,
    Int32,
    Int64,
    Single,
    Double,
    Array,
    String
}

public static class PrimitiveTypes
{
    public static readonly Type Boolean = new()
    {
        Name = "Boolean",
        ID = (byte)PrimitiveType.Boolean,
        IsPrimitive = true,
        PrimitiveType = PrimitiveType.Boolean,
        Size = 1,
        Fields = System.Array.Empty<Type>(),
        IsGenericType = false
    };

    public static readonly Type Byte = new()
    {
        Name = "Byte",
        ID = (byte)PrimitiveType.Byte,
        IsPrimitive = true,
        PrimitiveType = PrimitiveType.Byte,
        Size = 1,
        Fields = System.Array.Empty<Type>(),
        IsGenericType = false
    };

    public static readonly Type Int16 = new()
    {
        Name = "Int16",
        ID = (byte)PrimitiveType.Int16,
        IsPrimitive = true,
        PrimitiveType = PrimitiveType.Int16,
        Size = 2,
        Fields = System.Array.Empty<Type>(),
        IsGenericType = false
    };

    public static readonly Type Int32 = new()
    {
        Name = "Int32",
        ID = (byte)PrimitiveType.Int32,
        IsPrimitive = true,
        PrimitiveType = PrimitiveType.Int32,
        Size = 4,
        Fields = System.Array.Empty<Type>(),
        IsGenericType = false
    };

    public static readonly Type Int64 = new()
    {
        Name = "Int64",
        ID = (byte)PrimitiveType.Int64,
        IsPrimitive = true,
        PrimitiveType = PrimitiveType.Int64,
        Size = 8,
        Fields = System.Array.Empty<Type>(),
        IsGenericType = false
    };

    public static readonly Type Single = new()
    {
        Name = "Single",
        ID = (byte)PrimitiveType.Single,
        IsPrimitive = true,
        PrimitiveType = PrimitiveType.Single,
        Size = 4,
        Fields = System.Array.Empty<Type>(),
        IsGenericType = false
    };

    public static readonly Type Double = new()
    {
        Name = "Double",
        ID = (byte)PrimitiveType.Double,
        IsPrimitive = true,
        PrimitiveType = PrimitiveType.Double,
        Size = 8,
        Fields = System.Array.Empty<Type>(),
        IsGenericType = false
    };

    public static readonly Type Array = new()
    {
        Name = "Array",
        ID = (byte)PrimitiveType.Array,
        IsPrimitive = true,
        PrimitiveType = PrimitiveType.Array,
        Size = 6,
        Fields = new[] { Int16, Int32 },
        IsGenericType = true,
        GenericArgument = null
    };

    public static readonly Type String = new()
    {
        Name = "String",
        ID = (byte)PrimitiveType.String,
        IsPrimitive = true,
        PrimitiveType = PrimitiveType.String,
        Size = 4,
        Fields = new[] { Int32 },
        IsGenericType = false
    };

    public static readonly Type Struct = new()
    {
        Name = "Struct",
        ID = (byte)PrimitiveType.Struct,
        IsPrimitive = false,
        PrimitiveType = PrimitiveType.Struct,
        Size = 17,
        Fields = new[]
        {
            Int16, Byte, Byte, Int32, Int32, Int32
        },
        IsGenericType = false
    };

    public static readonly Type Empty = new()
    {
        Name = "Empty",
        ID = 10,
        IsPrimitive = true,
        PrimitiveType = PrimitiveType.Struct,
        Size = 0,
        Fields = System.Array.Empty<Type>(),
        IsGenericType = false
    };
}