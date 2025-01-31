using Vostok.Hercules.Serializer.Generator.Core.Primitives;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Members;

public class ParameterBuilder
{
    public ParameterBuilder(string name, ReferencedType type)
    {
        Name = name;
        Type = type;
    }

    public string Name { get; }

    public ReferencedType Type { get; }

    public string? DefaultValue { get; set; }
}