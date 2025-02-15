﻿using System;
using System.Collections.Generic;
using System.IO;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Serializer.Generator;
using Vostok.Hercules.Serializer.Sample.Converters;
using Vostok.Hercules.Serializer.Sample.Models;

namespace Vostok.Hercules.Serializer.Sample;

public static class Program
{
    public static void Main(string[] args)
    {
        using (File.OpenRead("")) ;

        var catBuilderProvider = new CategoryBuilderProvider();
        var userBuilderProvider = new UserBuilderProvider(catBuilderProvider);
        var userBuilder = userBuilderProvider.Get();
    }

    private static void DoStuff()
    {
        using (GetDisposable()) ;
    }

    private static IDisposable GetDisposable() => null!;

    internal class CategoryBuilderProvider : IHerculesEventBuilderProvider<Category>
    {
        public IHerculesEventBuilder<Category> Get() =>
            new CategoryBuilder(new DateTimeOffsetHerculesConverter());
    }

    internal class UserBuilderProvider(IHerculesEventBuilderProvider<Category> categoryBuilderProvider)
        : IHerculesEventBuilderProvider<User>
    {
        public IHerculesEventBuilder<User> Get() =>
            new UserBuilder(this, this, categoryBuilderProvider, new InstanceConverters());
    }

    public class EventBuilder : DummyHerculesTagsBuilder, IHerculesEventBuilder<User>
    {
        public IHerculesEventBuilder<User> SetTimestamp(DateTimeOffset timestamp) =>
            this;

        public User BuildEvent() =>
            throw new NotImplementedException();

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<short> values) =>
            this;

        public new IHerculesTagsBuilder AddVectorOfContainers(string key,
            IReadOnlyList<Action<IHerculesTagsBuilder>> containers)
        {
            base.AddVectorOfContainers()
        }

        public IHerculesTagsBuilder AddContainer(string key, Action<IHerculesTagsBuilder> valueBuilder)
        {
            
            if (key == "category")
            {
                var catbuilder = new CategoryBuilder(new());
                valueBuilder(catbuilder);
                _ = catbuilder.BuildEvent();
            }

            return this;
        }
    }
}