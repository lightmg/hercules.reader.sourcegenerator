using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;

public class PropertyBuilder
{
    public PropertyBuilder(string name, ReferencedType type)
    {
        Name = name;
        Type = type;
    }

    public string Name { get; }

    public ReferencedType Type { get; }

    public ParameterKind Kind { get; set; } = ParameterKind.Property;

    public Accessibility Accessibility { get; set; } = Accessibility.Public;

    public bool ReadOnly { get; set; } = false;

    public static PropertyBuilder ReadOnlyField(string name, ReferencedType type,
        Accessibility accessibility = Accessibility.Private) =>
        new PropertyBuilder(name, type)
        {
            Accessibility = accessibility,
            Kind = ParameterKind.Field,
            ReadOnly = true
        };
}