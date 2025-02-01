using System;

namespace Vostok.Hercules.Serializer.Sample.Converters;

public class InstanceConverters
{
    public int ParseSomething(long someValue) => 
        (int)someValue;

    public string LongToString(long someValue) => 
        someValue.ToString();

    public string ExtractDomain(string email) => 
        email + "domain";

    public long ParseTimestamp(DateTimeOffset timestamp) => 
        timestamp.Ticks;
}