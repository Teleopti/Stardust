using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[InfrastructureTest]
	public class PublishRtaEventsToHangfireTest : ISetup
	{
		public IEventPublisher Target;
		public FakeHangfireEventClient Hangfire;
		public FakeServiceBusSender Bus;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeHangfireEventClient>().For<IHangfireEventClient>();
			system.UseTestDouble<FakeServiceBusSender>().For<IServiceBusSender>();
		}

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

		[Test]
		public void ShouldRunSiteOutOfAdherenceReadModelUpdaterOnHangfire()
		{
			Target.Publish(new PersonOutOfAdherenceEvent());

			Hangfire.HandlerTypes.Where(x => x.Contains(typeof (SiteOutOfAdherenceReadModelUpdater).Name))
				.Should()
				.Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldRunTeamOutOfAdherenceReadModelUpdaterOnHangfire()
		{
			Target.Publish(new PersonOutOfAdherenceEvent());

			Hangfire.HandlerTypes.Where(x => x.Contains(typeof(TeamOutOfAdherenceReadModelUpdater).Name))
				.Should()
				.Have.Count.EqualTo(1);
		}

		[Test]
		[Toggle(Toggles.RTA_AdherenceDetails_34267)]
		public void ShouldRunAdherenceDetailsReadModelUpdaterOnHangfire()
		{
			Target.Publish(new PersonActivityStartEvent());

			Hangfire.HandlerTypes.Where(x => x.Contains(typeof(AdherenceDetailsReadModelUpdater).Name))
				.Should()
				.Have.Count.EqualTo(1);
		}
	}
}
