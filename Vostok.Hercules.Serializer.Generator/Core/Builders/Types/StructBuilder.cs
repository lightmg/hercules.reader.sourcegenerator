using System.Collections.Generic;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Members;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Types.Abstract;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Types;

public class StructBuilder : StatefulTypeBuilder, IInitializabeTypeBuilder
{
    public StructBuilder(string ns, string name) : base(ns, name)
    {
    }

    public bool IsMutable { get; set; } = true;

    public IList<ConstructorBuilder> Constructors { get; } = [];
}