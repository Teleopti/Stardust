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
		public TestCommon.PerformanceTest.PerformanceTest PerformanceTest;

		[Test]
		public void MeasurePerformance()
		{
			using (PerformanceTest.Measure("1mKUHvBlk5wIk0LDZESO2prWvRuimhpjiWaSvoKk2gsE", "QueueWorkLargeBatchesTest"))
			{
				States.SendAllAsLargeBatches();
				Hangfire.WaitForQueue();
			}
		}

	}
}
