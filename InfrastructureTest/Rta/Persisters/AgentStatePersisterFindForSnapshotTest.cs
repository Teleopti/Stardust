using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
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
				SourceId = "6"
			};
			Persister.Upsert(state);

			var logons = Persister.FindForClosingSnapshot("2015-03-06 15:20".Utc(), "6", "loggedout");
			Persister.ReadForTest(logons)
				.Single(x => x.PersonId == personId).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReadValuesWhenClosingSnapshot()
		{
			var personId = Guid.NewGuid();
			var agentStateReadModel = new AgentStateForUpsert
			{
				BusinessUnitId = Guid.NewGuid(),
				PersonId = personId,
				StateCode = "phone",
				PlatformTypeId = Guid.NewGuid(),
				StateStartTime = "2015-03-06 15:00".Utc(),
				ReceivedTime = "2015-03-06 15:19".Utc(),
				SourceId = "6"
			};
			Persister.Upsert(agentStateReadModel);

			var logons = Persister.FindForClosingSnapshot("2015-03-06 15:20".Utc(), "6", "loggedout");
			var result = Persister.ReadForTest(logons)
				.Single(x => x.PersonId == personId);

			result.BusinessUnitId.Should().Be(agentStateReadModel.BusinessUnitId);
			result.StateCode.Should().Be(agentStateReadModel.StateCode);
			result.PlatformTypeId.Should().Be(agentStateReadModel.PlatformTypeId);
			result.StateStartTime.Should().Be(agentStateReadModel.StateStartTime);
		}

		[Test]
		public void ShouldFindPersonsForLogout()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Persister.Upsert(new AgentStateForUpsert
			{
				PersonId = person1,
				ReceivedTime = "2016-09-12 13:00".Utc(),
				SourceId = "6",
				StateCode = Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot,
				DataSourceId = 1,
				UserCode = "usercode1"
			});
			Persister.Upsert(new AgentStateForUpsert
			{
				PersonId = person2,
				ReceivedTime = "2016-09-12 13:00".Utc(),
				SourceId = "6",
				DataSourceId = 1,
				UserCode = "usercode2"
			});

			var result = Persister.FindForClosingSnapshot("2016-09-12 13:01".Utc(), "6", Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot)
				.Single();
			result.DataSourceId.Should().Be(1);
			result.UserCode.Should().Be("usercode2");
		}

	}
}