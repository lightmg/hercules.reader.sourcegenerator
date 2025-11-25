using SourceGenerator.Core.Builders.Members;
using SourceGenerator.Core.Primitives;

namespace SourceGenerator.Core.Builders.Types.Abstract;

public abstract class StatefulTypeBuilder : TypeBuilder
{
    protected StatefulTypeBuilder(string ns, string name) : base(ns, name)
    {
    }

    public IList<TypeDescriptor> Interfaces { get; } = [];

    public IList<GenericTypeBuilder> Generics { get; set; } = [];

    public IList<PropertyBuilder> Properties { get; } = [];

    public IList<MethodBuilder> Methods { get; } = [];
}