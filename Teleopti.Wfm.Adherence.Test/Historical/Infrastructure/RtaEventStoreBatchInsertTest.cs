using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.States.Events;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Historical.Infrastructure
{
	[UnitOfWorkTest]
	public class RtaEventStoreBatchInsertTest
	{
		public IRtaEventStore Target;
		public IRtaEventStoreTester Tester;

		[Test]
		public void ShouldInsertMany()
		{
			Target.Add(new[]
			{
				new PersonStateChangedEvent(),
				new PersonStateChangedEvent()
			}, DeadLockVictim.No, RtaEventStoreVersion.StoreVersion);

			var result = Tester.LoadAllForTest();

			result.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldInsertSuperMany()
		{
			var events = Enumerable.Range(0, 1000).Select(x => new PersonStateChangedEvent());
			Target.Add(events, DeadLockVictim.No, RtaEventStoreVersion.StoreVersion);

			var result = Tester.LoadAllForTest();

			result.Should().Have.Count.EqualTo(1000);
		}

		[Test]
		[Setting("RtaEventStoreBatchSize", 1)]
		public void ShouldInsertInConfiguredBatches()
		{
			Target.Add(new[]
			{
				new PersonStateChangedEvent(),
				new PersonStateChangedEvent()
			}, DeadLockVictim.No, RtaEventStoreVersion.StoreVersion);

			var result = Tester.LoadAllForTest();

			result.Should().Have.Count.EqualTo(2);
		}
	}
}