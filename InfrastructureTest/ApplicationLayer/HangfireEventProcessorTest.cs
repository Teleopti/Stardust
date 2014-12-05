using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[TestFixture]
	public class HangfireEventProcessorTest
	{
		[Test]
		public void ShouldPublish()
		{
			var publisher = new FakeEventPublisher();
			var deserializer = new NewtonsoftJsonDeserializer();
			var target = new HangfireEventProcessor(publisher, deserializer);

			target.Process(null, typeof (Event).AssemblyQualifiedName, "{}");

			publisher.PublishedEvents.Single().Should().Be.OfType<Event>();
		}

		[Test]
		public void ShouldPublishGivenEventType()
		{
			var publisher = new FakeEventPublisher();
			var target = new HangfireEventProcessor(publisher, new NewtonsoftJsonDeserializer());

			target.Process(null, typeof(AnEvent).AssemblyQualifiedName, "{}");

			publisher.PublishedEvents.Single().Should().Be.OfType<AnEvent>();
		}

		[Test]
		public void ShouldDeserialize()
		{
			var publisher = new FakeEventPublisher();
			var target = new HangfireEventProcessor(publisher, new NewtonsoftJsonDeserializer());

			target.Process(null, typeof(AnEvent).AssemblyQualifiedName, "{ Data: 'Hello' }");

			publisher.PublishedEvents.OfType<AnEvent>().Single().Data.Should().Be("Hello");
		}

		public class AnEvent : Event
		{
			public string Data { get; set; }
		}
	}
}