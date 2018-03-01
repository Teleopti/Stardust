using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Rta.PerformanceTest.Code;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[RtaPerformanceTest]
	public class QueueWorkLargeBatchesTest
	{
		public StatesSender States;
		public HangfireUtilities Hangfire;
		public StateQueueUtilities StateQueue;
		public TestCommon.PerformanceTest.PerformanceTest PerformanceTest;

		[Test]
		public void MeasurePerformance()
		{
			PerformanceTest.Measure("1mKUHvBlk5wIk0LDZESO2prWvRuimhpjiWaSvoKk2gsE", "QueueWorkLargeBatchesTest", () =>
			{
				States.SendAllAsLargeBatches();
				StateQueue.WaitForQueue();
				Hangfire.WaitForQueue();
			});
		}

	}
}
