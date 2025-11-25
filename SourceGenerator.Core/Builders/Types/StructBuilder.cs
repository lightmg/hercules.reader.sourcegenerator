using SourceGenerator.Core.Builders.Members;
using SourceGenerator.Core.Builders.Types.Abstract;

namespace SourceGenerator.Core.Builders.Types;

public class StructBuilder : StatefulTypeBuilder, IInitializabeTypeBuilder
{
    public StructBuilder(string ns, string name) : base(ns, name)
    {
    }

    public bool IsMutable { get; set; } = true;

    public IList<ConstructorBuilder> Constructors { get; } = [];
}