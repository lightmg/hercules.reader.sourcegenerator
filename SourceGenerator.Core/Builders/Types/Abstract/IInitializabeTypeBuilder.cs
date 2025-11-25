using SourceGenerator.Core.Builders.Members;

namespace SourceGenerator.Core.Builders.Types.Abstract;

public interface IInitializabeTypeBuilder : ITypeBuilder
{
    public IList<ConstructorBuilder> Constructors { get; }
}