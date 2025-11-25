using Microsoft.CodeAnalysis;
using SourceGenerator.Core.Builders.Members.Abstract;
using SourceGenerator.Core.Builders.Types.Abstract;
using SourceGenerator.Core.Writer;

namespace SourceGenerator.Core.Builders.Members;

public class ConstructorBuilder : IMethodBodyBuilder, ITypeMemberBuilder
{
    public ConstructorBuilder(IInitializabeTypeBuilder typeBuilder) : this(typeBuilder.Name)
    {
    }

    public ConstructorBuilder(string declaringTypeName)
    {
        DeclaringTypeName = declaringTypeName;
        EmitBody = _ => { };
    }

    public string DeclaringTypeName { get; }

    public IList<ParameterBuilder> Parameters { get; } = [];

    public IDictionary<string, string> BaseCtorArgs { get; } = new Dictionary<string, string>();

    public Action<CodeWriter>? EmitBody { get; set; }

    public Accessibility Accessibility { get; set; } = Accessibility.Public;
}