using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Rta.PerformanceTest.Code;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[RtaPerformanceTest]
	[Explicit]
	public class QueueWorkLargeBatchesTest
	{
		public StatesSender States;
		public HangfireUtilities Hangfire;

		[Test]
		public void MeasurePerformance()
		{
			States.SendAllAsLargeBatches();
			Hangfire.WaitForQueue();
		}
    }
}
