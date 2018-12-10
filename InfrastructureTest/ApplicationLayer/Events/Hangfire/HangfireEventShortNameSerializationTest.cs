using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire
{
	[TestFixture]
	[InfrastructureTest]
	public class HangfireEventShortNameSerializationTest : IExtendSystem
	{
		public FakeHangfireEventClient JobClient;
		public IEventPublisher Publisher;
		public HangfireEventServerForTest Server;
		public IJsonSerializer Serializer;
		public IJsonDeserializer Deserializer;
		public FakeHandler Handler;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<HangfireEventServerForTest>();
			extend.AddService<FakeHandler>();
		}

		[Test]
		public void ShouldSerializePersonStateChangedEvent()
		{
			Publisher.Publish(new PersonStateChangedEvent());

			var actual = Deserializer.DeserializeObject<Dictionary<string, string>>(JobClient.SerializedEvent);
			actual.Keys.ForEach(k =>
			{
				k.Length.Should().Be.LessThan(3);
			});
		}

		[Test]
		public void ShouldDeserializePersonStateChangedEvent()
		{
			Publisher.Publish(new PersonStateChangedEvent {Timestamp = "2015-08-17 15:40".Utc()});

			Server.ProcessForTest(null, RandomName.Make(), typeof(PersonStateChangedEvent).AssemblyQualifiedName, JobClient.SerializedEvent, typeof(FakeHandler).AssemblyQualifiedName);

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