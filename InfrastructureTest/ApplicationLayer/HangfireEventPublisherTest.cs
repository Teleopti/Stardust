using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[TestFixture]
	public class HangfireEventPublisherTest
	{
		[Test]
		public void ShouldEnqueue()
		{
			var jobClient = new FakeHangfireEventClient();
			var target = new HangfireEventPublisher(jobClient, null);

			target.Publish(new Event());

			jobClient.WasEnqueued.Should().Be.True();
		}

		[Test]
		public void ShouldSerializeTheEvent()
		{
			var jobClient = new FakeHangfireEventClient();
			var serializer = new ToStringSerializer();
			var target = new HangfireEventPublisher(jobClient, serializer);

			target.Publish(new Event());

			jobClient.SerializedEvent.Should().Be.EqualTo(serializer.SerializeObject(new Event()));
		}

		[Test]
		public void ShouldPassEventType()
		{
			var jobClient = new FakeHangfireEventClient();
			var target = new HangfireEventPublisher(jobClient, null);

			target.Publish(new Event());

			jobClient.EventType.Should().Be.EqualTo(new Event().GetType().AssemblyQualifiedName);
		}

		[Test]
		public void ShouldPassTypeNameAsDisplayName()
		{
			var jobClient = new FakeHangfireEventClient();
			var target = new HangfireEventPublisher(jobClient, null);

			target.Publish(new Event());

			jobClient.DisplayName.Should().Be.EqualTo(new Event().GetType().Name);
		}
	}

	public class FakeHangfireEventClient : IHangfireEventClient
	{
		public bool WasEnqueued { get; set; }
		public string SerializedEvent { get; set; }
		public string EventType { get; set; }
		public string DisplayName { get; set; }

		public void Enqueue(string displayName, string eventType, string serializedEvent)
		{
			WasEnqueued = true;
			DisplayName = displayName;
			EventType = eventType;
			SerializedEvent = serializedEvent;
		}
	}
}