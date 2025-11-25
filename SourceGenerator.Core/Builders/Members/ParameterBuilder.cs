using SourceGenerator.Core.Primitives;

namespace SourceGenerator.Core.Builders.Members;

public class ParameterBuilder
{
    public ParameterBuilder(string name, TypeDescriptor type)
    {
        Name = name;
        Type = type;
    }

    public string Name { get; }

    public TypeDescriptor Type { get; }

    public string? DefaultValue { get; set; }
}