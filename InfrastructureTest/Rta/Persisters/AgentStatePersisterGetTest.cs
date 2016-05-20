using System;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[AnalyticsUnitOfWorkTest]
	public class AgentStatePersisterGetTest
	{
		public IAgentStatePersister Persister;
		public ICurrentAnalyticsUnitOfWork UnitOfWork;
		public MutableNow Now;

		[Test]
		public void ShouldGetCurrentActualAgentState()
		{
			var state = new AgentStateForTest { PersonId = Guid.NewGuid() };
			Persister.Persist(state);

			var result = Persister.Get(state.PersonId);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGetNullCurrentActualAgentStateIfNotFound()
		{
			var result = Persister.Get(Guid.NewGuid());

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldGetCurrentActualAgentStates()
		{
			var writer = Persister;
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			writer.Persist(new AgentStateForTest { PersonId = personId1 });
			writer.Persist(new AgentStateForTest { PersonId = personId2 });

			var result = Persister.GetAll();

			result.Where(x => x.PersonId == personId1).Should().Have.Count.EqualTo(1);
			result.Where(x => x.PersonId == personId2).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldGetCurrentActualAgentStatesWithAllData()
		{
			var writer = Persister;
			var state = new AgentStateForTest
			{
				PersonId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid(),
				TeamId = Guid.NewGuid(),
				SiteId = Guid.NewGuid(),
				RuleId = Guid.NewGuid(),
				RuleStartTime = "2014-11-11 10:33".Utc(),
				BatchId = "2014-11-11 10:34".Utc(),
				SourceId = "1",
				PlatformTypeId = Guid.NewGuid(),
				ReceivedTime = "2014-11-11 10:36".Utc(),
				StaffingEffect = 1,
				Adherence = (int) Adherence.Neutral,
				StateCode = "statecode",
				StateGroupId = Guid.NewGuid(),
				StateStartTime = "2014-11-11 10:37".Utc(),
			};
			writer.Persist(state);

			var result = Persister.GetAll().Single();

			result.PersonId.Should().Be(state.PersonId);
			result.BusinessUnitId.Should().Be(state.BusinessUnitId);
			result.TeamId.Should().Be(state.TeamId);
			result.SiteId.Should().Be(state.SiteId);
			result.RuleId.Should().Be(state.RuleId);
			result.RuleStartTime.Should().Be(state.RuleStartTime);
			result.BatchId.Should().Be(state.BatchId);
			result.SourceId.Should().Be(state.SourceId);
			result.PlatformTypeId.Should().Be(state.PlatformTypeId);
			result.ReceivedTime.Should().Be(state.ReceivedTime);
			result.StaffingEffect.Should().Be(state.StaffingEffect);
			result.Adherence.Should().Be(state.Adherence);
			result.StateCode.Should().Be(state.StateCode);
			result.StateGroupId.Should().Be(state.StateGroupId);
			result.StateStartTime.Should().Be(state.StateStartTime);
		}

		[Test]
		public void ShouldReadActualAgentStateWithoutBusinessUnit()
		{
			Persister.Persist(new AgentStateForTest {BusinessUnitId = Guid.NewGuid()});
			UnitOfWork.Current()
				.FetchSession()
				.CreateSQLQuery("UPDATE Rta.ActualAgentState SET BusinessUnitId=NULL")
				.ExecuteUpdate();

			Persister.GetAll().Single()
				.BusinessUnitId
				.Should().Be(Guid.Empty);
		}

		[Test]
		public void ShouldReadNullValuesWhenClosingSnapshot()
		{
			var personId = Guid.NewGuid();
			var state = new AgentState
			{
				PersonId = personId,
				ReceivedTime = "2015-03-06 15:19".Utc(),
				SourceId = "6"
			};
			Persister.Persist(state);

			Persister.GetNotInSnapshot("2015-03-06 15:20".Utc(), "6")
				.Single(x => x.PersonId == personId).Should().Not.Be.Null();
		}

		[Test]
		public void SHouldReadValuesWhenClosingSnapshot()
		{
			var personId = Guid.NewGuid();
			var agentStateReadModel = new AgentState
			{
				BusinessUnitId = Guid.NewGuid(),
				PersonId = personId,
				StateCode = "phone",
				PlatformTypeId = Guid.NewGuid(),
				StateStartTime = "2015-03-06 15:00".Utc(),
				ReceivedTime = "2015-03-06 15:19".Utc(),
				SourceId = "6"
			};
			Persister.Persist(agentStateReadModel);

			var result = Persister.GetNotInSnapshot("2015-03-06 15:20".Utc(), "6")
				.Single(x => x.PersonId == personId);

			result.BusinessUnitId.Should().Be(agentStateReadModel.BusinessUnitId);
			result.StateCode.Should().Be(agentStateReadModel.StateCode);
			result.PlatformTypeId.Should().Be(agentStateReadModel.PlatformTypeId);
			result.StateStartTime.Should().Be(agentStateReadModel.StateStartTime);
		}

		[Test]
		public void ShouldNotCrashWhenBusinessUnitIdIsNull()
		{
			var personId = Guid.NewGuid();
			var sql = string.Format(@"					
						insert into rta.ActualAgentState (PersonId,  PlatformTypeId, ReceivedTime, BatchId, OriginalDataSourceId, BusinessUnitId)
						values('{0}', '{1}', '{2}', '{2}', '2', null)", personId, Guid.NewGuid(), "2015-04-21 8:00");
			using (var connection = new SqlConnection(InfraTestConfigReader.AnalyticsConnectionString))
			{
				connection.Open();
				using (var command = new SqlCommand(sql, connection))
					command.ExecuteNonQuery();
			}
			
			Assert.DoesNotThrow(() => Persister.GetNotInSnapshot("2015-04-21 8:15".Utc(), "2"));
		}
	}
}