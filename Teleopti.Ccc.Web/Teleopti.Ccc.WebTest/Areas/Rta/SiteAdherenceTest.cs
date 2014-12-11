using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public class SiteAdherenceTest
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

			sender.LastSiteNotification.DeserializeBindaryData<SiteAdherenceMessage>().OutOfAdherence.Should().Be(1);
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

			sender.LastSiteNotification.DeserializeBindaryData<SiteAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldAggregateAdherenceFor2PersonsOnASite()
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
			var siteId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(outOfAdherence1)
				.WithUser("one", personId1, null, null, siteId)
				.WithUser("two", personId2, null, null, siteId)
				.WithSchedule(personId1, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithSchedule(personId2, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("ready", phone, 0)
				.WithAlarm("loggedoff", phone, -1)
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(outOfAdherence1);
			target.SaveState(outOfAdherence2);

			sender.LastSiteNotification.DeserializeBindaryData<SiteAdherenceMessage>().OutOfAdherence.Should().Be(2);
		}


		[Test]
		public void ShouldAggregateAdherenceFor2PersonsDifferentSites()
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
				.WithUser("one", personId1, null, null, Guid.NewGuid())
				.WithUser("two", personId2, null, null, Guid.NewGuid())
				.WithSchedule(personId1, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithSchedule(personId2, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("ready", phone, 0)
				.WithAlarm("loggedoff", phone, -1)
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(outOfAdherence1);
			target.SaveState(outOfAdherence2);

			sender.LastSiteNotification.DeserializeBindaryData<SiteAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}
	}
}