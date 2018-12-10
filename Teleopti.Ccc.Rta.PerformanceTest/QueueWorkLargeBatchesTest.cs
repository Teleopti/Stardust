using System;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Rta.PerformanceTest.Code;
using Teleopti.Wfm.Adherence.States.Infrastructure;

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
				StateQueue.WaitForDequeue(TimeSpan.FromMinutes(15));
				Hangfire.WaitForQueue();
			});
		}

	}
}
