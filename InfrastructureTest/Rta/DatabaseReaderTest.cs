﻿using System;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[Category("LongRunning")]
	public class DatabaseReaderTest : IActualAgentStateReadWriteTest
	{
		[Test]
		public void ShouldGetCurrentActualAgentState()
		{
			var state = new ActualAgentStateForTest { PersonId = Guid.NewGuid() };
			new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler()).PersistActualAgentState(state);
			var target = new DatabaseReader(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler(), new Now());

			var result = target.GetCurrentActualAgentState(state.PersonId);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGetNullCurrentActualAgentStateIfNotFound()
		{
			var target = new DatabaseReader(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler(), new Now());

			var result = target.GetCurrentActualAgentState(Guid.NewGuid());

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldGetCurrentActualAgentStates()
		{
			var writer = new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler());
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			writer.PersistActualAgentState(new ActualAgentStateForTest { PersonId = personId1 });
			writer.PersistActualAgentState(new ActualAgentStateForTest { PersonId = personId2 });
			var target = new DatabaseReader(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler(), new Now());

			var result = target.GetActualAgentStates();

			result.Where(x => x.PersonId == personId1).Should().Have.Count.EqualTo(1);
			result.Where(x => x.PersonId == personId2).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldGetCurrentActualAgentStatesWithAllData()
		{
			var writer = new DatabaseWriter(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler());
			var state = new ActualAgentStateForTest
			{
				PersonId = Guid.NewGuid(),
				AlarmId = Guid.NewGuid(),
				AlarmName = "alarm",
				AlarmStart = "2014-11-11 10:33".Utc(),
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
				State = "state",
				StateCode = "statecode",
				StateId = Guid.NewGuid(),
				StateStart = "2014-11-11 10:37".Utc(),
			};
			writer.PersistActualAgentState(state);
			var target = new DatabaseReader(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler(), new Now());

			var result = target.GetActualAgentStates().Single();

			result.PersonId.Should().Be(state.PersonId);
			result.AlarmId.Should().Be(state.AlarmId);
			result.AlarmName.Should().Be(state.AlarmName);
			result.AlarmStart.Should().Be(state.AlarmStart);
			result.BatchId.Should().Be(state.BatchId);
			//result.BusinessUnit.Should().Be(state.BusinessUnit);
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
			result.State.Should().Be(state.State);
			result.StateCode.Should().Be(state.StateCode);
			result.StateId.Should().Be(state.StateId);
			result.StateStart.Should().Be(state.StateStart);
		}

		[Test]
		public void ShouldReadBelongsToDate()
		{
			var personId = Guid.NewGuid();
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var layer = new ProjectionChangedEventLayer
				{
					StartDateTime = "2014-11-07 10:00".Utc(),
					EndDateTime = "2014-11-07 10:00".Utc()
				};
				var repository = new ScheduleProjectionReadOnlyRepository(new FixedCurrentUnitOfWork(uow));
				repository.AddProjectedLayer(new DateOnly("2014-11-07".Utc()), Guid.NewGuid(), personId, layer);
				uow.PersistAll();
			}
			var target = new DatabaseReader(new DatabaseConnectionFactory(), new FakeDatabaseConnectionStringHandler(), new ThisIsNow("2014-11-07 06:00"));

			var result = target.GetCurrentSchedule(personId);

			result.Single().BelongsToDate.Should().Be(new DateOnly("2014-11-07".Utc()));
		}
	}

	[ActualAgentStateReadWriteTest]
	public interface IActualAgentStateReadWriteTest
	{
	}

	public class ActualAgentStateReadWriteTestAttribute : Attribute, ITestAction
	{
		public void BeforeTest(TestDetails testDetails)
		{
		}

		public void AfterTest(TestDetails testDetails)
		{
			applySql("DELETE FROM RTA.ActualAgentState");
		}

		public ActionTargets Targets { get { return ActionTargets.Test; } }

		private void applySql(string sql)
		{
			using (var connection = new SqlConnection(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix))
			{
				connection.Open();
				using (var command = new SqlCommand(sql, connection))
					command.ExecuteNonQuery();
			}
		}

	}
}