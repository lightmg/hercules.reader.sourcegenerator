using System;

namespace Vostok.Hercules.Serializer.Sample.Models;

public interface IEntity
{
    Guid Id { get; set; }

    DateTimeOffset Created { get; set; }
}