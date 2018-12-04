using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.Service.AgentState
{
	[TestFixture]
	[UnitOfWorkTest]
	public class AgentStatePersisterFindForSnapshotTest
	{
		public IAgentStatePersister Persister;
		public ICurrentAnalyticsUnitOfWork UnitOfWork;
		public MutableNow Now;

		[Test]
		public void ShouldReadNullValuesWhenClosingSnapshot()
		{
			var personId = Guid.NewGuid();
			var state = new AgentStateForUpsert
			{
				PersonId = personId,
				ReceivedTime = "2015-03-06 15:19".Utc(),
				SnapshotDataSourceId = 6,
				StateGroupId = Guid.NewGuid()
			};
			Persister.Upsert(state);

			var logons = Persister.FindForClosingSnapshot("2015-03-06 15:20".Utc(), 6, new[] {Guid.NewGuid()});
			Persister.ReadForTest(logons)
				.Single(x => x.PersonId == personId).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReadValuesWhenClosingSnapshot()
		{
			var personId = Guid.NewGuid();
			var state = new AgentStateForUpsert
			{
				BusinessUnitId = Guid.NewGuid(),
				PersonId = personId,
				StateStartTime = "2015-03-06 15:00".Utc(),
				ReceivedTime = "2015-03-06 15:19".Utc(),
				SnapshotDataSourceId = 6,
				StateGroupId = Guid.NewGuid()
			};
			Persister.Upsert(state);

			var logons = Persister.FindForClosingSnapshot("2015-03-06 15:20".Utc(), 6, new Guid[] { });
			var result = Persister.ReadForTest(logons)
				.Single(x => x.PersonId == personId);

			result.BusinessUnitId.Should().Be(state.BusinessUnitId);
			result.StateGroupId.Should().Be(state.StateGroupId);
			result.StateStartTime.Should().Be(state.StateStartTime);
		}

		[Test]
		public void ShouldFindPersonsForLogout()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var state = Guid.NewGuid();
			var loggedout = Guid.NewGuid();
			Persister.Upsert(new AgentStateForUpsert
			{
				PersonId = person1,
				ReceivedTime = "2016-09-12 13:00".Utc(),
				SnapshotDataSourceId = 1,
				StateGroupId = loggedout
			});
			Persister.Upsert(new AgentStateForUpsert
			{
				PersonId = person2,
				ReceivedTime = "2016-09-12 13:00".Utc(),
				SnapshotDataSourceId = 1,
				StateGroupId = state
			});

			Persister.FindForClosingSnapshot("2016-09-12 13:01".Utc(), 1, new[] {loggedout})
				.Single()
				.Should().Be(person2);
		}

		[Test]
		public void ShouldFindPersonsForLogoutNotInSnapshot()
		{
			var snapshotId = "2017-12-19 15:28:08".Utc();
			Persister.Upsert(new AgentStateForUpsert
			{
				PersonId = Guid.NewGuid(),
				ReceivedTime = snapshotId,
				SnapshotDataSourceId = 1,
				StateGroupId = Guid.NewGuid(),
				SnapshotId = snapshotId
			});

			Persister.FindForClosingSnapshot(snapshotId, 1, new[] {Guid.NewGuid()})
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldFindPersonsForLogoutNotInSnapshotWithPrecision_47282()
		{
			var person = Guid.NewGuid();
			var snapshotIds = from i in Enumerable.Range(0, 30) // measured 100% failure rate with 30 random numbers
				let m = new Random().Next(0, 9999999)
				let id = $"2017-12-19 15:28:08.{m}".Utc()
				select id;

			snapshotIds.ForEach(snapshotId =>
			{
				Persister.Delete(person, DeadLockVictim.Yes);
				Persister.Upsert(new AgentStateForUpsert
				{
					PersonId = person,
					ReceivedTime = snapshotId,
					SnapshotDataSourceId = 1,
					StateGroupId = Guid.NewGuid(),
					SnapshotId = snapshotId
				});
				
				Persister.FindForClosingSnapshot(snapshotId, 1, new[] {Guid.NewGuid()})
					.Should().Be.Empty();
			});
		}
	}
}