using NUnit.Framework;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;
using Teleopti.Wfm.Adherence.Test.States.Unit.Service;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.Service
{
	[TestFixture]
	[DatabaseTest]
	public class QueueFloodTest
	{
		public Rta Target;
		public IStateQueueHealthChecker Checker;

		[Test]
		public void ShouldStopEnqueueAfter100Batches()
		{
			100.Times(() =>
			{
				Checker.Reset();
				Target.Enqueue(new BatchForTest());
			});
			
			Assert.Throws<StateQueueHealthException>(() =>
			{
				Checker.Reset();
				Target.Enqueue(new BatchForTest());
			});
		}

	}
}