using System;
using System.Collections.Generic;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Serializer.Sample.Models;

namespace Vostok.Hercules.Serializer.Sample;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = new UserBuilder(new());
    }

    public class EventBuilder : DummyHerculesTagsBuilder, IHerculesEventBuilder<User>
    {
        public IHerculesEventBuilder<User> SetTimestamp(DateTimeOffset timestamp) => this;

        public User BuildEvent() => throw new NotImplementedException();

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<short> values)
        {
            throw new NotImplementedException();
        }

        public IHerculesEventBuilder AddContainer(string key, Action<IHerculesTagsBuilder> valueBuilder)
        {
            if (key == "category")
            {
                var catbuilder = new CategoryBuilder(new());
                valueBuilder(catbuilder);
                _ = catbuilder.BuildEvent();
            }
        }
    }
}