using System;
using System.Security.Policy;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public class TeamAdherenceTest
	{
		[Test]
		public void ShouldMapOutOfAdherenceBasedOnPositiveStaffingEffect()
		{
			var inAdherence = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "ready",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var outOfAdherence = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedoff",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var sender = new FakeMessageSender();
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(inAdherence)
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, inAdherence.Timestamp.AddHours(-1), inAdherence.Timestamp.AddHours(1))
				.WithAlarm("ready", phone, 0)
				.WithAlarm("loggedoff", phone, 1)
				.Done();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(inAdherence.Timestamp), sender);

			target.SaveExternalUserState(inAdherence);
			target.SaveExternalUserState(outOfAdherence);

			sender.LastTeamNotification.DeserializeBindaryData<TeamAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldMapOutOfAdherenceBasedOnNegativeStaffingEffect()
		{
			var inAdherence = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "ready",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var outOfAdherence = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedoff",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var sender = new FakeMessageSender();
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(inAdherence)
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, inAdherence.Timestamp.AddHours(-1), inAdherence.Timestamp.AddHours(1))
				.WithAlarm("ready", phone, 0)
				.WithAlarm("loggedoff", phone, -1)
				.Done();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(inAdherence.Timestamp), sender);

			target.SaveExternalUserState(inAdherence);
			target.SaveExternalUserState(outOfAdherence);

			sender.LastTeamNotification.DeserializeBindaryData<TeamAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldAggregateAdherenceFor2PersonsInATeam()
		{
			var outOfAdherence1 = new ExternalUserStateForTest
			{
				UserCode = "one",
				StateCode = "loggedoff",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var outOfAdherence2 = new ExternalUserStateForTest
			{
				UserCode = "two",
				StateCode = "loggedoff",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var sender = new FakeMessageSender();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(outOfAdherence1)
				.WithUser("one", personId1, null, teamId, null)
				.WithUser("two", personId2, null, teamId, null)
				.WithSchedule(personId1, phone, outOfAdherence1.Timestamp.AddHours(-1), outOfAdherence1.Timestamp.AddHours(1))
				.WithSchedule(personId2, phone, outOfAdherence2.Timestamp.AddHours(-1), outOfAdherence2.Timestamp.AddHours(1))
				.WithAlarm("ready", phone, 0)
				.WithAlarm("loggedoff", phone, -1)
				.Done();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(outOfAdherence1.Timestamp), sender);

			target.SaveExternalUserState(outOfAdherence1);
			target.SaveExternalUserState(outOfAdherence2);

			sender.LastTeamNotification.DeserializeBindaryData<TeamAdherenceMessage>().OutOfAdherence.Should().Be(2);
		}

		[Test]
		public void ShouldAggregateAdherenceFor2PersonsInDifferentTeams()
		{
			var outOfAdherence1 = new ExternalUserStateForTest
			{
				UserCode = "one",
				StateCode = "loggedoff",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var outOfAdherence2 = new ExternalUserStateForTest
			{
				UserCode = "two",
				StateCode = "loggedoff",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var sender = new FakeMessageSender();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(outOfAdherence1)
				.WithUser("one", personId1, null, Guid.NewGuid(), null)
				.WithUser("two", personId2, null, Guid.NewGuid(), null)
				.WithSchedule(personId1, phone, outOfAdherence1.Timestamp.AddHours(-1), outOfAdherence1.Timestamp.AddHours(1))
				.WithSchedule(personId2, phone, outOfAdherence2.Timestamp.AddHours(-1), outOfAdherence2.Timestamp.AddHours(1))
				.WithAlarm("ready", phone, 0)
				.WithAlarm("loggedoff", phone, -1)
				.Done();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(outOfAdherence1.Timestamp), sender);

			target.SaveExternalUserState(outOfAdherence1);
			target.SaveExternalUserState(outOfAdherence2);

			sender.LastTeamNotification.DeserializeBindaryData<TeamAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldSetSiteIdAsDomainReferenceId()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var sender = new FakeMessageSender();
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, null, null, siteId)
				.Done();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), sender);

			target.SaveExternalUserState(state);

			sender.LastTeamNotification.DomainReferenceId
				.Should().Be.EqualTo(siteId.ToString());
		}
	}
}
