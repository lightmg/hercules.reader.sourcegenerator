using Microsoft.CodeAnalysis;

namespace SourceGenerator.Core.Builders.Types.Abstract;

public interface ITypeBuilder
{
    string Name { get; }

    string Namespace { get; }

    string FullName { get; }

    Accessibility Accessibility { get; set; }

    IEnumerable<string> Attributes { get; }

    IList<string> Usings { get; }
}