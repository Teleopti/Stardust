using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Rta.WebService;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core
{
	[TestFixture]
	public class TeleoptiRtaServiceTest
	{
		[Test]
		public void ShouldProcessValidState()
		{
			var state = new ExternalUserStateForTest();
			var target = new TeleoptiRtaServiceForTest(state);

			var result = target.SaveExternalUserState(state);

			result.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldThrowIfAuthenticationKeyIsIncorrect()
		{
			var state = new ExternalUserStateForTest { AuthenticationKey = "something" };
			var target = new TeleoptiRtaServiceForTest(state);

			state.AuthenticationKey += " else";

			Assert.Throws(typeof(FaultException), () => target.SaveExternalUserState(state));
		}

		[Test]
		public void ShouldAcceptIfThirdAndFourthLetterOfAuthenticationKeyIsCorrupted_BecauseOfEncodingIssuesWithThe3rdLetterOfTheDefaultKeyAndWeAreNotAllowedToChangeTheDefault()
		{
			var target = new TeleoptiRtaServiceForTest();
			var state = new ExternalUserStateForTest { AuthenticationKey = TeleoptiRtaService.DefaultAuthenticationKey };
			state.AuthenticationKey = state.AuthenticationKey.Remove(2, 2).Insert(2, "_");

			target.SaveExternalUserState(state);
		}

		[Test]
		public void ShouldNotProcessStatesThatsToOld()
		{
			var christmas = new DateTime(2001, 12, 24, 15, 0, 0);
			var state = new ExternalUserStateForTest() { Timestamp = christmas.AddHours(1) };
			var target = new TeleoptiRtaServiceForTest(state, new ThisIsNow(christmas));

			var result = target.SaveExternalUserState(state);

			result.Should().Not.Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotProcessStatesWithTimeStampNewerThanNow()
		{
			var christmas = new DateTime(2001, 12, 24, 15, 0, 0);
			var now = MockRepository.GenerateStub<INow>();
			now.Expect(n => n.UtcDateTime()).Return(christmas);
			var state = new ExternalUserStateForTest { Timestamp = christmas.Subtract(TimeSpan.FromHours(1)) };
			var target = new TeleoptiRtaServiceForTest(state, now);

			var result = target.SaveExternalUserState(state);

			result.Should().Not.Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotProcessIfNoSourceId()
		{
			var state = new ExternalUserStateForTest();
			var target = new TeleoptiRtaServiceForTest(state);
			state.SourceId = string.Empty;

			var result = target.SaveExternalUserState(state);

			result.Should().Not.Be.EqualTo(1);
		}

		[Test]
		public void ShouldPersistActualAgentState()
		{
			var state = new ExternalUserStateForTest();
			var database = new FakeRtaDatabase();
			database.AddTestData(state.SourceId, state.UserCode, Guid.NewGuid(), Guid.NewGuid());
			var target = new TeleoptiRtaServiceForTest(database, state);

			target.SaveExternalUserState(state);

			database.PersistedActualAgentState.StateCode.Should().Be(state.StateCode);
		}

		[Test]
		public void ShouldNotProcessStateIfNoPlatformId()
		{
			var state = new ExternalUserStateForTest();
			var target = new TeleoptiRtaServiceForTest(state);
			state.PlatformTypeId = string.Empty;

			var result = target.SaveExternalUserState(state);

			result.Should().Not.Be.EqualTo(1);
		}

		[Test]
		public void ShouldCutStateCodeIfToLong()
		{
			var state = new ExternalUserStateForTest();
			var database = new FakeRtaDatabase();
			database.AddTestData(state.SourceId, state.UserCode, Guid.NewGuid(), Guid.NewGuid());
			var target = new TeleoptiRtaServiceForTest(database, state);

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
			var target = new TeleoptiRtaServiceForTest(database, state);

			state.IsLoggedOn = false;
			target.SaveExternalUserState(state);

			database.PersistedActualAgentState.StateCode.Should().Be(TeleoptiRtaService.LogOutStateCode);
		}

		[Test]
		public void ShouldProcessExternalUserStatesInBatch()
		{
			var state1 = new ExternalUserStateForTest();
			var state2 = new ExternalUserStateForTest();
			var target = new TeleoptiRtaServiceForTest(state1);

			var result = target.SaveBatchExternalUserState(new[] { state1, state2 });

			result.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldThrowIfTooManyExternalUserStatesInBatch()
		{
			const int tooManyStates = 200;
			var externalStates = new Collection<ExternalUserStateForTest>();
			for (var i = 0; i < tooManyStates; i++)
				externalStates.Add(new ExternalUserStateForTest());
			var state = new ExternalUserStateForTest();
			var target = new TeleoptiRtaServiceForTest(state);

			Assert.Throws(typeof(FaultException), () => target.SaveBatchExternalUserState(externalStates));
		}

		[Test]
		public void ShouldPersistActualAgentStateWhenScheduleIsUpdated()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var state = new ExternalUserStateForTest();
			var database = new FakeRtaDatabase();
			database.AddTestData(state.SourceId, state.UserCode, personId, businessUnitId);
			var target = new TeleoptiRtaServiceForTest(database, state);

			target.GetUpdatedScheduleChange(personId, businessUnitId, state.Timestamp);

			database.PersistedActualAgentState.PersonId.Should().Be(personId);
		}
	}
}