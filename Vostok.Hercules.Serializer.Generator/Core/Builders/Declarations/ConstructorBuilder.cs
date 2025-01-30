using System.Collections.Generic;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;

public class ConstructorBuilder : BaseMethodBuilder
{
    public ConstructorBuilder(TypeBuilder typeBuilder) : this(typeBuilder.Name)
    {
    }

    public ConstructorBuilder(string declaringTypeName)
    {
        DeclaringTypeName = declaringTypeName;
        EmitBody = _ => { };
    }

    public string DeclaringTypeName { get; }

    public IList<ParameterBuilder> Parameters { get; } = [];

    public IList<(ParameterBuilder parameter, PropertyBuilder property)> Initalizers { get; } = [];
}