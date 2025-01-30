using System;
using Microsoft.CodeAnalysis;
using Vostok.Hercules.Serializer.Generator.Core.Writer;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Declarations;

public abstract class BaseMethodBuilder
{
    public Accessibility Accessibility { get; set; } = Accessibility.Public;

    public Action<CodeWriter>? EmitBody { get; set; }
}