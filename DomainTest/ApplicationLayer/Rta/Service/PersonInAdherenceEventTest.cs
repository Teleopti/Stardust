﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class PersonInAdherenceEventTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPublishWhenNoStaffingEffect()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithRule("statecode", activityId, 0);
			Now.Is("2014-10-20 9:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single()
				.PersonId.Should().Be(personId);
		}
		
		[Test]
		public void ShouldNotPublishIfStillInAdherence()
		{
			var activityId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithRule("statecode1", activityId, 0)
				.WithRule("statecode2", activityId, 0);
			Now.Is("2014-10-20 9:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode1"
			});
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode2"
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishTimeOfStateChange()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithRule(null, phone, -1)
				.WithRule("phone", phone, 0);
			Now.Is("2014-10-20 9:05");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single()
				.Timestamp.Should().Be("2014-10-20 9:05".Utc());
		}

		[Test]
		public void ShouldPublishTimeOfActivityChange()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithRule(null, phone, 0)
				.WithRule("phone", phone, 0);
			Now.Is("2014-10-20 9:05");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single()
				.Timestamp.Should().Be("2014-10-20 9:00".Utc());
		}

		[Test]
		public void ShouldPublishEventWithBusinesUnitId()
		{
			var businessUnitId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-11-11 10:00", "2014-11-11 12:00")
				.WithRule("statecode", activityId, 0);
			Now.Is("2014-11-11 11:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single()
				.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPublishWithTeamId()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, null, teamId, null)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithRule("statecode", activityId, 0);
			Now.Is("2014-10-20 9:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single()
				.TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldPublishWithSiteId()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, null, null, siteId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithRule("statecode", activityId, 0);
			Now.Is("2014-10-20 9:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single()
				.SiteId.Should().Be(siteId);
		}

		[Test]
		public void ShouldPublishWhenShiftEnds()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2015-11-25 8:00", "2015-11-25 12:00")
				.WithRule("logged off", phone, -1)
				.WithRule("logged off", null, 0)
				;
			Now.Is("2015-11-24 17:00");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logged off"
			});
			Now.Is("2015-11-25 8:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Publisher.Clear();

			Now.Is("2015-11-25 12:01");
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single().Timestamp.Should().Be("2015-11-25 12:00".Utc());
		}

		[Test]
		public void ShouldPublishWhenInAfterShiftEnds()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2015-11-25 8:00", "2015-11-25 12:00")
				.WithRule("phone", phone, 0)
				.WithRule("phone", null, 1)
				.WithRule("logged off", phone, -1)
				.WithRule("logged off", null, 0)
				;
			Now.Is("2015-11-25 8:00");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Publisher.Clear();

			Now.Is("2015-11-25 12:01");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logged off"
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single().Timestamp.Should().Be("2015-11-25 12:01".Utc());
		}

	}
}