using System;
using System.Data.SqlTypes;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class PersistStateTest
	{
		[Test]
		public void ShouldPersistActualAgentState()
		{
			var state = new ExternalUserStateForTest();
			var database = new FakeRtaDatabase().WithDataFromState(state).Make();
			var target = RtaForTest.MakeBasedOnState(state, database);

			target.SaveState(state);

			database.PersistedAgentStateReadModel.StateCode.Should().Be(state.StateCode);
		}

		[Test]
		public void ShouldCutStateCodeIfToLong()
		{
			var state = new ExternalUserStateForTest();
			var database = new FakeRtaDatabase().WithDataFromState(state).Make();
			var target = RtaForTest.MakeBasedOnState(state, database);

			state.StateCode = "a really really really really looooooooong statecode that should be trimmed somehow for whatever reason";
			target.SaveState(state);

			database.PersistedAgentStateReadModel.StateCode.Should().Be(state.StateCode.Substring(0, 25));
		}

		[Test]
		public void ShouldPersistStateCodeToLoggedOutIfNotLoggedIn()
		{
			var state = new ExternalUserStateForTest();
			var database = new FakeRtaDatabase().WithDataFromState(state).Make();
			var target = RtaForTest.MakeBasedOnState(state, database);

			state.IsLoggedOn = false;
			target.SaveState(state);

			database.PersistedAgentStateReadModel.StateCode.Should().Be(Web.Areas.Rta.Rta.LogOutStateCode);
		}

		[Test]
		public void ShouldPersistActualAgentStateWhenScheduleIsUpdated()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var state = new ExternalUserStateForTest();
			var database = new FakeRtaDatabase()
				.WithSource(state.SourceId)
				.WithUser(state.UserCode, personId, businessUnitId)
				.Make();
			var target = RtaForTest.MakeBasedOnState(state, database);

			target.CheckForActivityChange(personId, businessUnitId);

			database.PersistedAgentStateReadModel.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPersistActualAgentStateWithNextStartSetToNull()
		{

			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var state = new ExternalUserStateForTest();
			var database = new FakeRtaDatabase()
				.WithSource(state.SourceId)
				.WithUser(state.UserCode, personId, businessUnitId)
				.Make();
			var target = RtaForTest.MakeBasedOnState(state, database);

			target.CheckForActivityChange(personId, businessUnitId);

			database.PersistedAgentStateReadModel.NextStart.Should().Be(null);
		}
	}
}