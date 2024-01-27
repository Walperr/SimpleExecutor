namespace Compiler;

public sealed class ConstantsWriter
{
    private readonly SortedList<ushort, object> _constants = new();

    public ushort AddConstant(bool value)
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

    public ushort AddConstant(double value)
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

    public ushort AddConstant(string value)
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

    public byte[] GetConstantsBuffer()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        foreach (var (_, value) in _constants)
        {
            switch (value)
            {
                case bool boolean:
                    writer.Write(sizeof(bool));
                    writer.Write(boolean);
                    break;
                case double number:
                    writer.Write(sizeof(double));
                    writer.Write(number);
                    break;
                case string str:
                    writer.Write(str.Length * sizeof(char));
                    writer.Write(str);
                    break;
            }
        }

        return stream.GetBuffer();
    }
}