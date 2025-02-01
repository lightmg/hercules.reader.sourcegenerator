using System.Collections.Generic;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Members;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Types.Abstract;

public abstract class StatefulTypeBuilder : TypeBuilder
{
    protected StatefulTypeBuilder(string ns, string name) : base(ns, name)
    {
    }

    public IList<ReferencedType> Interfaces { get; } = [];

    public IList<GenericTypeBuilder> Generics { get; set; } = [];

    public IList<PropertyBuilder> Properties { get; } = [];

    public IList<MethodBuilder> Methods { get; } = [];
}