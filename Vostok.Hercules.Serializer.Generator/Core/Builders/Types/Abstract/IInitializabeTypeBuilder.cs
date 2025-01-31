using System.Collections.Generic;
using Vostok.Hercules.Serializer.Generator.Core.Builders.Members;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Types.Abstract;

public interface IInitializabeTypeBuilder : ITypeBuilder
{
    public IList<ConstructorBuilder> Constructors { get; }
}