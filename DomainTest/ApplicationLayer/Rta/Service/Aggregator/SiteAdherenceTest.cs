﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator;
using Teleopti.Ccc.Domain.Common.Time;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service.Aggregator
{
	[RtaTest]
	[TestFixture]
	public class SiteAdherenceTest
	{
		public FakeRtaDatabase Database;
		public FakeMessageSender Sender;
		public MutableNow Now;
		public IRta Target;

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
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("ready", phone, 0)
				.WithAlarm("loggedoff", phone, 1)
				;
			Now.Is("2014-10-20 9:00");
			
			Target.SaveState(inAdherence);
			Target.SaveState(outOfAdherence);

			Sender.LastSiteNotification.DeserializeBindaryData<SiteAdherenceMessage>().OutOfAdherence.Should().Be(1);
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
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("ready", phone, 0)
				.WithAlarm("loggedoff", phone, -1)
				;
			Now.Is("2014-10-20 9:00");

			Target.SaveState(inAdherence);
			Target.SaveState(outOfAdherence);

			Sender.LastSiteNotification.DeserializeBindaryData<SiteAdherenceMessage>().OutOfAdherence.Should().Be(1);
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
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("one", personId1, null, null, siteId)
				.WithUser("two", personId2, null, null, siteId)
				.WithSchedule(personId1, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithSchedule(personId2, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("ready", phone, 0)
				.WithAlarm("loggedoff", phone, -1)
				;
			Now.Is("2014-10-20 9:00");

			Target.SaveState(outOfAdherence1);
			Target.SaveState(outOfAdherence2);

			Sender.LastSiteNotification.DeserializeBindaryData<SiteAdherenceMessage>().OutOfAdherence.Should().Be(2);
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
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("one", personId1, null, null, Guid.NewGuid())
				.WithUser("two", personId2, null, null, Guid.NewGuid())
				.WithSchedule(personId1, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithSchedule(personId2, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("ready", phone, 0)
				.WithAlarm("loggedoff", phone, -1)
				;
			Now.Is("2014-10-20 9:00");

			Target.SaveState(outOfAdherence1);
			Target.SaveState(outOfAdherence2);

			Sender.LastSiteNotification.DeserializeBindaryData<SiteAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}
	}
}