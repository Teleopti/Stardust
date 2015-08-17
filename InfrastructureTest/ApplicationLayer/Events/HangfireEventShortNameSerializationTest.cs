using System;
using System.Collections.Generic;
using Castle.Core.Internal;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[InfrastructureTest]
	[Toggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	public class HangfireEventShortNameSerializationTest : ISetup
	{
		public FakeHangfireEventClient JobClient;
		public IEventPublisher Client;
		public IHangfireEventProcessor Server;
		public IJsonSerializer Serializer;
		public IJsonDeserializer Deserializer;
		public FakeHandler Handler;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeHangfireEventClient>().For<IHangfireEventClient>();
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
			Client.Publish(new PersonStateChangedEvent {Datasource = "datasource"});

			Server.Process(null, typeof(PersonStateChangedEvent).AssemblyQualifiedName, JobClient.SerializedEvent, typeof(FakeHandler).AssemblyQualifiedName);

			Handler.GotEvent.Datasource.Should().Be("datasource");
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