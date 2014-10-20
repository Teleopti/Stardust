using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core
{
	[TestFixture]
	public class PersistStateTest
	{
		[Test]
		public void ShouldPersistActualAgentState()
		{
			var state = new ExternalUserStateForTest();
			var database = new FakeRtaDatabase();
			database.AddTestData(state.SourceId, state.UserCode, Guid.NewGuid(), Guid.NewGuid());
			var target = TeleoptiRtaServiceForTest.MakeBasedOnState(state, database);

			target.SaveExternalUserState(state);

			database.PersistedActualAgentState.StateCode.Should().Be(state.StateCode);
		}

		[Test]
		public void ShouldCutStateCodeIfToLong()
		{
			var state = new ExternalUserStateForTest();
			var database = new FakeRtaDatabase();
			database.AddTestData(state.SourceId, state.UserCode, Guid.NewGuid(), Guid.NewGuid());
			var target = TeleoptiRtaServiceForTest.MakeBasedOnState(state, database);

			state.StateCode = "a really really really really looooooooong statecode that should be trimmed somehow for whatever reason";
			target.SaveExternalUserState(state);

			database.PersistedActualAgentState.StateCode.Should().Be(state.StateCode.Substring(0, 25));
		}

		[Test]
		public void ShouldPersistStateCodeToLoggedOutIfNotLoggedIn()
		{
			var state = new ExternalUserStateForTest();
			var database = new FakeRtaDatabase();
			database.AddTestData(state.SourceId, state.UserCode, Guid.NewGuid(), Guid.NewGuid());
			var target = TeleoptiRtaServiceForTest.MakeBasedOnState(state, database);

			state.IsLoggedOn = false;
			target.SaveExternalUserState(state);

			database.PersistedActualAgentState.StateCode.Should().Be(TeleoptiRtaService.LogOutStateCode);
		}

		[Test]
		public void ShouldPersistActualAgentStateWhenScheduleIsUpdated()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var state = new ExternalUserStateForTest();
			var database = new FakeRtaDatabase();
			database.AddTestData(state.SourceId, state.UserCode, personId, businessUnitId);
			var target = TeleoptiRtaServiceForTest.MakeBasedOnState(state, database);

			target.GetUpdatedScheduleChange(personId, businessUnitId, state.Timestamp);

			database.PersistedActualAgentState.PersonId.Should().Be(personId);
		}

	}
}