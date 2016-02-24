using System;
using System.Collections.Generic;
using Castle.Core.Internal;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[InfrastructureTest]
	public class HangfireEventShortNameSerializationTest : ISetup
	{
		public FakeHangfireEventClient JobClient;
		public IEventPublisher Client;
		public HangfireEventProcessor Server;
		public IJsonSerializer Serializer;
		public IJsonDeserializer Deserializer;
		public FakeHandler Handler;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<FakeHandler>();
		}

		[Test]
		public void ShouldSerializePersonStateChangedEvent()
		{
			Client.Publish(new PersonStateChangedEvent());

			var actual = Deserializer.DeserializeObject<Dictionary<string, string>>(JobClient.SerializedEvent);
			actual.Keys.ForEach(k =>
			{
				k.Length.Should().Be.LessThan(3);
			});
		}

		[Test]
		public void ShouldDeserializePersonStateChangedEvent()
		{
			Client.Publish(new PersonStateChangedEvent {Timestamp = "2015-08-17 15:40".Utc()});

			Server.Process(null, RandomName.Make(), typeof(PersonStateChangedEvent).AssemblyQualifiedName, JobClient.SerializedEvent, typeof(FakeHandler).AssemblyQualifiedName);

			Handler.GotEvent.Timestamp.Should().Be("2015-08-17 15:40".Utc());
		}

		[Test]
		public void ShouldNormallyNotSerializeLikeThat()
		{
			var serialized = Serializer.SerializeObject(new SomethingWithPropertiesLikePersonStateChangedEvent());

			var actual = Deserializer.DeserializeObject<Dictionary<string, string>>(serialized);
			actual.Keys.Should().Have.SameValuesAs(new[] {"PersonId"});
		}
		
		public class SomethingWithPropertiesLikePersonStateChangedEvent
		{
			public Guid PersonId { get; set; }
		}
		
		public class FakeHandler :
			IRunOnHangfire,
			IHandleEvent<PersonStateChangedEvent>
		{
			public PersonStateChangedEvent GotEvent;

			public void Handle(PersonStateChangedEvent @event)
			{
				GotEvent = @event;
			}
		}
	}
}