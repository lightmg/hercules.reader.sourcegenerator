using System;
using Vostok.Hercules.Serializer.Generator;
using Vostok.Hercules.Serializer.Sample.Converters;

namespace Vostok.Hercules.Serializer.Sample.Models;

[GenerateHerculesReader]
public class Category : IEntity
{
    [HerculesTag("id")]
    public Guid Id { get; set; }

    [HerculesTag("created"), HerculesConverter(typeof(DateTimeOffsetHerculesConverter))]
    public DateTimeOffset Created { get; set; }

    [HerculesTag("name")]
    public string Name { get; set; } = null!;

    [HerculesTag("order")]
    public short Order { get; set; }
}