using System;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[MultiDatabaseTest]
	public class DatabaseReaderAgentStateTest
	{
		public IAgentStateReadModelReader Reader;
		public IAgentStateReadModelPersister Writer;
		public MutableNow Now;

		[Test]
		public void ShouldGetCurrentActualAgentState()
		{
			var state = new AgentStateReadModelForTest { PersonId = Guid.NewGuid() };
			Writer.PersistActualAgentReadModel(state);
			var target = Reader;

			var result = target.GetCurrentActualAgentState(state.PersonId);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGetNullCurrentActualAgentStateIfNotFound()
		{
			var target = Reader;

			var result = target.GetCurrentActualAgentState(Guid.NewGuid());

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldGetCurrentActualAgentStates()
		{
			var writer = Writer;
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			writer.PersistActualAgentReadModel(new AgentStateReadModelForTest { PersonId = personId1 });
			writer.PersistActualAgentReadModel(new AgentStateReadModelForTest { PersonId = personId2 });
			var target = Reader;

			var result = target.GetActualAgentStates();

			result.Where(x => x.PersonId == personId1).Should().Have.Count.EqualTo(1);
			result.Where(x => x.PersonId == personId2).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldGetCurrentActualAgentStatesWithAllData()
		{
			var writer = Writer;
			var state = new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				AlarmId = Guid.NewGuid(),
				AlarmName = "alarm",
				RuleStartTime = "2014-11-11 10:33".Utc(),
				BatchId = "2014-11-11 10:34".Utc(),
				BusinessUnitId = Guid.NewGuid(),
				Color = 3,
				NextStart = "2014-11-11 10:35".Utc(),
				OriginalDataSourceId = "1",
				PlatformTypeId = Guid.NewGuid(),
				ReceivedTime = "2014-11-11 10:36".Utc(),
				Scheduled = "schedule",
				ScheduledId = Guid.NewGuid(),
				ScheduledNext = "next",
				ScheduledNextId = Guid.NewGuid(),
				StaffingEffect = 1,
				Adherence = (int) Adherence.Neutral,
				State = "state",
				StateCode = "statecode",
				StateId = Guid.NewGuid(),
				StateStartTime = "2014-11-11 10:37".Utc(),
			};
			writer.PersistActualAgentReadModel(state);
			var target = Reader;

			var result = target.GetActualAgentStates().Single();

			result.PersonId.Should().Be(state.PersonId);
			result.AlarmId.Should().Be(state.AlarmId);
			result.AlarmName.Should().Be(state.AlarmName);
			result.RuleStartTime.Should().Be(state.RuleStartTime);
			result.BatchId.Should().Be(state.BatchId);
			result.BusinessUnitId.Should().Be(state.BusinessUnitId);
			result.Color.Should().Be(state.Color);
			result.NextStart.Should().Be(state.NextStart);
			result.OriginalDataSourceId.Should().Be(state.OriginalDataSourceId);
			result.PlatformTypeId.Should().Be(state.PlatformTypeId);
			result.ReceivedTime.Should().Be(state.ReceivedTime);
			result.Scheduled.Should().Be(state.Scheduled);
			result.ScheduledId.Should().Be(state.ScheduledId);
			result.ScheduledNext.Should().Be(state.ScheduledNext);
			result.ScheduledNextId.Should().Be(state.ScheduledNextId);
			result.StaffingEffect.Should().Be(state.StaffingEffect);
			result.Adherence.Should().Be(state.Adherence);
			result.State.Should().Be(state.State);
			result.StateCode.Should().Be(state.StateCode);
			result.StateId.Should().Be(state.StateId);
			result.StateStartTime.Should().Be(state.StateStartTime);
		}

		[Test]
		public void ShouldReadActualAgentStateWithoutBusinessUnit()
		{
			var writer = Writer;
			writer.PersistActualAgentReadModel(new AgentStateReadModelForTest());
			setBusinessUnitInDbToNull();
			var reader = Reader;

			reader.GetActualAgentStates().Single()
				.BusinessUnitId
				.Should().Be(Guid.Empty);
		}
		private static void setBusinessUnitInDbToNull()
		{
			using (var connection = new SqlConnection(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix))
			{
				connection.Open();
				using (var command = new SqlCommand("UPDATE Rta.ActualAgentState SET BusinessUnitId=NULL", connection))
					command.ExecuteNonQuery();
			}
		}

		[Test]
		public void ShouldReadNullValuesWhenClosingSnapshot()
		{
			var personId = Guid.NewGuid();
			var state = new AgentStateReadModel
			{
				PersonId = personId,
				ReceivedTime = "2015-03-06 15:19".Utc(),
				OriginalDataSourceId = "6"
			};
			Writer.PersistActualAgentReadModel(state);

			Reader.GetMissingAgentStatesFromBatch("2015-03-06 15:20".Utc(), "6")
				.Single(x => x.PersonId == personId).Should().Not.Be.Null();
		}

		[Test]
		public void SHouldReadValuesWhenClosingSnapshot()
		{
			var personId = Guid.NewGuid();
			var agentStateReadModel = new AgentStateReadModel
			{
				BusinessUnitId = Guid.NewGuid(),
				PersonId = personId,
				StateCode = "phone",
				PlatformTypeId = Guid.NewGuid(),
				State = "Ready",
				StateId = Guid.NewGuid(),
				Scheduled = "Phone",
				ScheduledId = Guid.NewGuid(),
				StateStartTime = "2015-03-06 15:00".Utc(),
				ScheduledNext = "Break",
				ScheduledNextId = Guid.NewGuid(),
				NextStart = "2015-03-06 06:00".Utc(),
				ReceivedTime = "2015-03-06 15:19".Utc(),
				OriginalDataSourceId = "6"
			};
			Writer.PersistActualAgentReadModel(agentStateReadModel);

			var result = Reader.GetMissingAgentStatesFromBatch("2015-03-06 15:20".Utc(), "6")
				.Single(x => x.PersonId == personId);

			result.BusinessUnitId.Should().Be(agentStateReadModel.BusinessUnitId);
			result.StateCode.Should().Be(agentStateReadModel.StateCode);
			result.PlatformTypeId.Should().Be(agentStateReadModel.PlatformTypeId);
			result.State.Should().Be(agentStateReadModel.State);
			result.StateId.Should().Be(agentStateReadModel.StateId);
			result.Scheduled.Should().Be(agentStateReadModel.Scheduled);
			result.ScheduledId.Should().Be(agentStateReadModel.ScheduledId);
			result.StateStartTime.Should().Be(agentStateReadModel.StateStartTime);
			result.ScheduledNext.Should().Be(agentStateReadModel.ScheduledNext);
			result.ScheduledNextId.Should().Be(agentStateReadModel.ScheduledNextId);
			result.NextStart.Should().Be(agentStateReadModel.NextStart);
		}

		[Test]
		public void ShouldNotCrashWhenBusinessUnitIdIsNull()
		{
			var personId = Guid.NewGuid();
			var sql = string.Format(@"					
						insert into rta.ActualAgentState (PersonId,  PlatformTypeId, ReceivedTime, BatchId, OriginalDataSourceId, BusinessUnitId)
						values('{0}', '{1}', '{2}', '{2}', '2', null)", personId, Guid.NewGuid(), "2015-04-21 8:00");
			using (var connection = new SqlConnection(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix))
			{
				connection.Open();
				using (var command = new SqlCommand(sql, connection))
					command.ExecuteNonQuery();
			}
			
			Assert.DoesNotThrow(() => Reader.GetMissingAgentStatesFromBatch("2015-04-21 8:15".Utc(), "2"));
		}
	}
}