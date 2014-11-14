using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class AdherenceAggregatorInitializeTest
	{
		[Test]
		public void ShouldReadPersistedAggregatedState()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "user2",
				StateCode = "loggedoff",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var sender = new FakeMessageSender();
			var teamId = Guid.NewGuid();
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("user1", person1, null, teamId, null)
				.WithUser("user2", person2, null, teamId, null)
				.WithSchedule(person1, phone, state.Timestamp.AddHours(-1), state.Timestamp.AddHours(1))
				.WithSchedule(person2, phone, state.Timestamp.AddHours(-1), state.Timestamp.AddHours(1))
				.WithAlarm("loggedoff", phone, -1)
				.Make();
			database.PersistActualAgentState(new ActualAgentState
			{
				PersonId = person1,
				StaffingEffect = -1
			});
			var service = new RtaForTest(database, new ThisIsNow(state.Timestamp), sender);

			service.Initialize();
			service.SaveState(state);

			sender.LastTeamNotification.DeserializeBindaryData<TeamAdherenceMessage>().OutOfAdherence.Should().Be(2);
		}

		[Test]
		public void ShouldNotSendMessagesWhenInitializing()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var sender = new FakeMessageSender();
			var teamId = Guid.NewGuid();
			var person1 = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", person1, null, teamId, null)
				.WithSchedule(person1, phone, state.Timestamp.AddHours(-1), state.Timestamp.AddHours(1))
				.WithAlarm("statecode", phone, -1)
				.Make();
			database.PersistActualAgentState(new ActualAgentState
			{
				PersonId = person1,
				StaffingEffect = -1
			});
			var service = new RtaForTest(database, new ThisIsNow(state.Timestamp), sender);

			service.Initialize();

			sender.AllNotifications.Should().Be.Empty();
		}
	}
}