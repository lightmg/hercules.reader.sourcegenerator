using System;
using System.Collections.Generic;
using Vostok.Hercules.Serializer.Generator;
using Vostok.Hercules.Serializer.Sample.Converters;

namespace Vostok.Hercules.Serializer.Sample.Models;

[GenerateHerculesReader]
public class User : IEntity
{
    [HerculesTag("id")] 
    public Guid Id { get; set; }

    [HerculesTag("created")]
    [HerculesConverter(typeof(StaticConverters), nameof(StaticConverters.ParseDateTimeOffsetFromUnixMicroseconds))]
    public DateTimeOffset Created { get; set; }

    [HerculesTimestampTag]
    [HerculesConverter(typeof(InstanceConverters), nameof(InstanceConverters.ParseTimestamp))]
    public long TimestampTicks { get; set; }

    [HerculesTag("fullName")] 
    public string FullName { get; set; } = null!;

    [HerculesTag("phone")] 
    public string Phone { get; set; } = null!;

    [HerculesTag("age"), HerculesConverter(typeof(InstanceConverters), nameof(InstanceConverters.ParseSomething))]
    public int Age { get; set; }

    [HerculesTag("city")] 
    public string? City { get; set; }

    [HerculesTag("categories")] 
    public ICollection<Category> FavoriteCategories { get; set; } = new List<Category>();

    [HerculesTag("invitedBy")] 
    public User? InvitedBy { get; set; }

    [HerculesTag("invitedBy")] 
    public User AnotherUser { get; set; } = null!;
    
    [HerculesTag("emails")]
    public string[] Emails2 { get; set; } = [];

    [HerculesTag("emails")]
    public ICollection<string> Emails { get; set; } = new List<string>();

    [HerculesTag("cat")]
    public Category Category { get; set; } = null!;

    [HerculesTag("emails")]
    [HerculesConverter(typeof(InstanceConverters), nameof(InstanceConverters.ExtractDomain))]
    public HashSet<string> EmailDomains { get; set; } = new();
}