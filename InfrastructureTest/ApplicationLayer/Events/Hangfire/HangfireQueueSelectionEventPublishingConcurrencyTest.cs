using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire
{
	[TestFixture]
	[RealHangfireTest]
	public class HangfireQueueSelectionEventPublishingConcurrencyTest : IExtendSystem
	{
		public HangfireUtilities Hangfire;
		public IEventPublisher Publisher;
		public ConcurrencyRunner Run;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<QueuingHandler>();
		}

		[Test]
		public void ShouldEnqueueToDefinedQueuesConcurrently()
		{
			1000.Times(() =>
			{
				Run.InParallel(() =>
				{
					Publisher.Publish(new DefaultQueueEvent());
				});
				Run.InParallel(() =>
				{
					Publisher.Publish(new CriticalScheduleChangesTodayQueueEvent());
				});
			});
			Run.Wait();

			Hangfire.NumberOfJobsInQueue(Queues.Default).Should().Be(1000);
			Hangfire.NumberOfJobsInQueue(Queues.CriticalScheduleChangesToday).Should().Be(1000);
		}
	}

}