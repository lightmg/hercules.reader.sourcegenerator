using System.Collections.Generic;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;

public class MethodBuilder : BaseMethodBuilder
{
    public MethodBuilder(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public ReferencedType? ReturnType { get; set; } = null;

    public IList<GenericTypeBuilder> Generics { get; set; } = [];

    public IList<ParameterBuilder> Parameters { get; set; } = [];

    public bool IsStatic { get; set; } = false;
}