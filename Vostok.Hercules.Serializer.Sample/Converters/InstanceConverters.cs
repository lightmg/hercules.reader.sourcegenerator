﻿namespace Vostok.Hercules.Serializer.Sample.Converters;

public class InstanceConverters
{
    public int ParseSomething(long someValue) => 
        (int)someValue;
}