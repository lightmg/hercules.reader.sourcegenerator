using System.Collections.Generic;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Members;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Types.Abstract;
using Vostok.Hercules.Serializer.Generator.Core.Primitives;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Types;

public class ClassBuilder : StatefulTypeBuilder, IInitializabeTypeBuilder
{
    public ClassBuilder(string ns, string name, ReferencedType? baseType = null) : base(ns, name)
    {
        BaseType = baseType;
    }

    public ReferencedType? BaseType { get; }

    public IList<ConstructorBuilder> Constructors { get; } = [];
}