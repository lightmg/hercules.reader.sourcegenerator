using System;
using System.Linq;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Serializer.Sample.Models;

namespace Vostok.Hercules.Serializer.Sample;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = new UserBuilder(new User());
    }

    public class EventBuilder : DummyHerculesTagsBuilder, IHerculesEventBuilder<User>
    {
        public IHerculesEventBuilder<User> SetTimestamp(DateTimeOffset timestamp) => this;

        public User BuildEvent() => throw new NotImplementedException();
    }
}