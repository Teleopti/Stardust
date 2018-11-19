using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Rta.PerformanceTest.Code;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Infrastructure.Service;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[RtaPerformanceTest]
	public class SynchronizeLargeBatchesTest
	{
		public StatesSender States;
		public TestCommon.PerformanceTest.PerformanceTest PerformanceTest;
		public SynchronizerWaiter Synchronizer;

		[Test]
		[OnlyRunIfEnabled(Toggles.RTA_ReviewHistoricalAdherence_74770)]
		public void MeasurePerformance()
		{
			PerformanceTest.Measure("1mKUHvBlk5wIk0LDZESO2prWvRuimhpjiWaSvoKk2gsE", "SynchronizeLargeBatchesTest", () =>
			{
				States.SendAllAsLargeBatches();
				Synchronizer.Wait(TimeSpan.FromMinutes(15));
			});
		}
	}
}