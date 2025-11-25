using Microsoft.CodeAnalysis;

namespace SourceGenerator.Core.Builders.Members.Abstract;

public interface ITypeMemberBuilder
{
    Accessibility Accessibility { get; set; }
}