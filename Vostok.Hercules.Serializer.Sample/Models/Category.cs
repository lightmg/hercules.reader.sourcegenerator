using System;
using Vostok.Hercules.Serializer.Generator;

namespace Vostok.Hercules.Serializer.Sample.Models;

[GenerateHerculesReader]
public class Category : IEntity
{
    [HerculesTag("id")]
    public Guid Id { get; set; }

    [HerculesTag("created"), HerculesConverter(typeof(Category), nameof(Convert))]
    public DateTimeOffset Created { get; set; }

    [HerculesTag("name")]
    public string Name { get; set; } = null!;

    [HerculesTag("order")]
    public short Order { get; set; }
}