using Microsoft.CodeAnalysis;
using SourceGenerator.Core.Builders.Members.Abstract;
using SourceGenerator.Core.Primitives;

namespace SourceGenerator.Core.Builders.Members;

public class PropertyBuilder : ITypeMemberBuilder
{
    public PropertyBuilder(string name, TypeDescriptor type)
    {
        Name = name;
        Type = type;
    }

    public string Name { get; }

    public TypeDescriptor Type { get; }

    public ParameterKind Kind { get; set; } = ParameterKind.Property;

    public Accessibility Accessibility { get; set; } = Accessibility.Public;

    public bool ReadOnly { get; set; } = false;

    public static PropertyBuilder ReadOnlyField(string name, TypeDescriptor typeDescriptor,
        Accessibility accessibility = Accessibility.Private) =>
        new PropertyBuilder(name, typeDescriptor)
        {
            Accessibility = accessibility,
            Kind = ParameterKind.Field,
            ReadOnly = true
        };
}