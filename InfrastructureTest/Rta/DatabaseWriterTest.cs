using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[Category("LongRunning")]
	public class DatabaseWriterTest : IActualAgentStateReadWriteTest
	{
		[Test]
		public void ShouldPersistActualAgentState()
		{
			var state = new ActualAgentStateForTest();
			var target = new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler());

			target.PersistActualAgentState(new ActualAgentStateForTest());

			var result = new DatabaseReader(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler(), new Now()).GetCurrentActualAgentState(state.PersonId);
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistActualAgentStateWithBusinessUnit()
		{
			var businessUnitId = Guid.NewGuid();
			var state = new ActualAgentStateForTest {BusinessUnitId = businessUnitId};
			var target = createDatabaseWriter();

			target.PersistActualAgentState(state);

			createReader().GetCurrentActualAgentState(state.PersonId)
				.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPersistActualAgentStateWithNullValues()
		{
			var personId = Guid.NewGuid();
			var target = new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler());

			target.PersistActualAgentState(new ActualAgentState
			{
				PersonId = personId,
				BusinessUnitId = Guid.NewGuid(),
				PlatformTypeId = Guid.NewGuid(),
				OriginalDataSourceId = null,
				ReceivedTime = "2015-01-02 10:00".Utc(),
				BatchId = null,

				StateCode = null,
				StateId = null,
				StateStart = null,
				State = null,

				ScheduledId = null,
				Scheduled = null,
				ScheduledNextId = null,
				ScheduledNext = null,
				NextStart = null,

				AlarmId = null,
				AlarmName = null,
				AlarmStart = null,
				StaffingEffect = null,
				Color = null,
			});

			var result = new DatabaseReader(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler(), new Now()).GetCurrentActualAgentState(personId);
			result.Should().Not.Be.Null();
		}

		private static DatabaseWriter createDatabaseWriter()
		{
			return new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler());
		}

		private static DatabaseReader createReader()
		{
			return new DatabaseReader(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler(), new Now());
		}

	}
}