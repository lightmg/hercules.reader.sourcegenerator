using SourceGenerator.Core.Builders.Members;
using SourceGenerator.Core.Builders.Types.Abstract;
using SourceGenerator.Core.Primitives;

namespace SourceGenerator.Core.Builders.Types;

public class ClassBuilder : StatefulTypeBuilder, IInitializabeTypeBuilder
{
    public ClassBuilder(string ns, string name, TypeDescriptor? baseType = null) : base(ns, name)
    {
        BaseType = baseType;
    }

    public TypeDescriptor? BaseType { get; }

    public IList<ConstructorBuilder> Constructors { get; } = [];

    public ClassModifier Modifier { get; set; }
}