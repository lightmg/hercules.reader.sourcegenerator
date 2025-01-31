using Microsoft.CodeAnalysis;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Members.Abstract;

public interface ITypeMemberBuilder
{
    Accessibility Accessibility { get; set; }
}