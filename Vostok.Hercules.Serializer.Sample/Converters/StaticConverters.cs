using System;

namespace Vostok.Hercules.Serializer.Sample.Converters;

public static class StaticConverters
{
    public static DateTimeOffset ParseDateTimeOffsetFromUnixMicroseconds(long unixMicroseconds) => 
        DateTimeOffset.UnixEpoch.AddMicroseconds(unixMicroseconds);
}