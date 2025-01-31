using System;
using Vostok.Hercules.Serializer.Generator.Core.Writer;

namespace Vostok.Hercules.Serializer.Generator.Core.Builders.Members.Abstract;

public interface IMethodBodyBuilder
{
    Action<CodeWriter>? EmitBody { get; set; }
}