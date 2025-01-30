using System;
using Vostok.Hercules.Serializer.Generator;

namespace Vostok.Hercules.Serializer.Sample.Converters;

public class DateTimeOffsetHerculesConverter : IHerculesConverter<DateTimeOffset, long>
{
    public DateTimeOffset Deserialize(long herculesValue) =>
        DateTimeOffset.FromUnixTimeMilliseconds(herculesValue);
}