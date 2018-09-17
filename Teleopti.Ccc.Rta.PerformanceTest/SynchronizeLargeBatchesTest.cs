using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Rta.PerformanceTest.Code;


namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[RtaPerformanceTest]
	public class SynchronizeLargeBatchesTest
	{
		public StatesSender States;
		public TestCommon.PerformanceTest.PerformanceTest PerformanceTest;
		public WithUnitOfWork WithUnitOfWork;
		public WithReadModelUnitOfWork WithReadModelUnitOfWork;
		public IKeyValueStorePersister KeyValueStore;
		public IRtaEventStoreTestReader Events;

		[Test]
		public void MeasurePerformance()
		{
			PerformanceTest.Measure("1mKUHvBlk5wIk0LDZESO2prWvRuimhpjiWaSvoKk2gsE", "SynchronizeLargeBatchesTest", () =>
			{
			    States.SendAllAsSmallBatches();				
				waitForSyncronize();			
			});
		}

		private void waitForSyncronize()
		{
			while (true)
			{
				
				var latestStoredRtaEventId = WithUnitOfWork.Get(() => Events.LoadLastIdForTest());
				var latestSynchronizedRtaEvent = WithReadModelUnitOfWork.Get(() => KeyValueStore.Get("LatestSynchronizedRTAEvent", 0));
				if (latestStoredRtaEventId == latestSynchronizedRtaEvent)
					break;
				Thread.Sleep(20);					
			}
		}
	}
}