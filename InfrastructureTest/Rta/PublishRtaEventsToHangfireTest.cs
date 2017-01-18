using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[InfrastructureTest]
	public class PublishRtaEventsToHangfireTest
	{
		public IEventPublisher Target;
		public FakeHangfireEventClient Hangfire;
		public FakeServiceBusSender Bus;

		[Test]
		public void ShouldRunAdherencePercentageReadModelUpdaterOnHangfire()
		{
			Target.Publish(new PersonInAdherenceEvent());

			Hangfire.HandlerTypes.Where(x => x.Contains(typeof (AdherencePercentageReadModelUpdater).Name))
				.Should()
				.Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotPublishPersonStateChangeToTheBus()
		{
			Target.Publish(new PersonStateChangedEvent());

			Bus.WasEnqueued.Should().Be.False();
		}
	}
}
