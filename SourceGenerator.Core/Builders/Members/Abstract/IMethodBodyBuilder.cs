using SourceGenerator.Core.Writer;

namespace SourceGenerator.Core.Builders.Members.Abstract;

public interface IMethodBodyBuilder
{
    Action<CodeWriter>? EmitBody { get; set; }
}