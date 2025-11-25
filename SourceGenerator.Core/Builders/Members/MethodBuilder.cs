using Microsoft.CodeAnalysis;
using SourceGenerator.Core.Builders.Members.Abstract;
using SourceGenerator.Core.Primitives;
using SourceGenerator.Core.Writer;

namespace SourceGenerator.Core.Builders.Members;

public class MethodBuilder : IMethodBodyBuilder, ITypeMemberBuilder
{
    public MethodBuilder(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public TypeDescriptor? ReturnType { get; set; } = null;

    public IList<GenericTypeBuilder> Generics { get; set; } = [];

    public IList<ParameterBuilder> Parameters { get; set; } = [];

    public bool IsStatic { get; set; } = false;

    public bool IsOverride { get; set; } = false;

    public bool IsNew { get; set; } = false;

    public Action<CodeWriter>? EmitBody { get; set; }

    public Accessibility Accessibility { get; set; } = Accessibility.Public;
}