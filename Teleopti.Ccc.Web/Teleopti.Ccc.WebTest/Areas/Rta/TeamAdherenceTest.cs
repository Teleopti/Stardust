using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;

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
				StateCode = "ready"
			};
			var outOfAdherence = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedoff"
			};
			var sender = new FakeMessageSender();
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(inAdherence)
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("ready", phone, 0)
				.WithAlarm("loggedoff", phone, 1)
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(inAdherence);
			target.SaveState(outOfAdherence);

			sender.LastTeamNotification.DeserializeBindaryData<TeamAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldMapOutOfAdherenceBasedOnNegativeStaffingEffect()
		{
			var inAdherence = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			};
			var outOfAdherence = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedoff"
			};
			var sender = new FakeMessageSender();
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(inAdherence)
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("ready", phone, 0)
				.WithAlarm("loggedoff", phone, -1)
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(inAdherence);
			target.SaveState(outOfAdherence);

			sender.LastTeamNotification.DeserializeBindaryData<TeamAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldAggregateAdherenceFor2PersonsInATeam()
		{
			var outOfAdherence1 = new ExternalUserStateForTest
			{
				UserCode = "one",
				StateCode = "loggedoff"
			};
			var outOfAdherence2 = new ExternalUserStateForTest
			{
				UserCode = "two",
				StateCode = "loggedoff"
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
				.WithSchedule(personId1, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithSchedule(personId2, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("ready", phone, 0)
				.WithAlarm("loggedoff", phone, -1)
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(outOfAdherence1);
			target.SaveState(outOfAdherence2);

			sender.LastTeamNotification.DeserializeBindaryData<TeamAdherenceMessage>().OutOfAdherence.Should().Be(2);
		}

		[Test]
		public void ShouldAggregateAdherenceFor2PersonsInDifferentTeams()
		{
			var outOfAdherence1 = new ExternalUserStateForTest
			{
				UserCode = "one",
				StateCode = "loggedoff"
			};
			var outOfAdherence2 = new ExternalUserStateForTest
			{
				UserCode = "two",
				StateCode = "loggedoff"
			};
			var sender = new FakeMessageSender();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(outOfAdherence1)
				.WithUser("one", personId1, null, Guid.NewGuid(), null)
				.WithUser("two", personId2, null, Guid.NewGuid(), null)
				.WithSchedule(personId1, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithSchedule(personId2, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("ready", phone, 0)
				.WithAlarm("loggedoff", phone, -1)
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(outOfAdherence1);
			target.SaveState(outOfAdherence2);

			sender.LastTeamNotification.DeserializeBindaryData<TeamAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldSetSiteIdAsDomainReferenceId()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			var sender = new FakeMessageSender();
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, null, null, siteId)
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(state);

			sender.LastTeamNotification.DomainReferenceId.Should().Be.EqualTo(siteId.ToString());
		}
	}
}
