using System;
using Vostok.Hercules.Serializer.Generator.Mapping.Abstract;
using Vostok.Hercules.Serializer.Generator.Mapping.Flat;
using Vostok.Hercules.Serializer.Generator.Services;

namespace Vostok.Hercules.Serializer.Generator.Mapping.Timestamp;

internal class TimestampMapProvider : BaseMapProvider
{
    public static TimestampTagMap Create(TagMapTarget target, TagMapConverter? converter, MappingGeneratorContext ctx)
    {
        if (converter.HasValue && converter.Value.InType != typeof(DateTimeOffset))
            ctx.AddDiagnostic(DiagnosticDescriptors.InvalidTimestampTagType, target.Symbol,
                typeof(DateTimeOffset), converter.Value.InType
            );

        var source = new TagMapTimestampSource();
        return new TimestampTagMap(source, target, converter);
    }
}