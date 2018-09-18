using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Rta.PerformanceTest.Code;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;


namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[RtaPerformanceTest]
	public class SynchronizeLargeBatchesTest
	{
		public StatesSender States;
		public TestCommon.PerformanceTest.PerformanceTest PerformanceTest;
		public SynchronzieWaiter Waiter;

		[Test]
		public void MeasurePerformance()
		{
			PerformanceTest.Measure("1mKUHvBlk5wIk0LDZESO2prWvRuimhpjiWaSvoKk2gsE", "SynchronizeLargeBatchesTest", () =>
			{
				States.SendAllAsLargeBatches();
				Waiter.WaitForSyncronize();
			});
		}
	}
}