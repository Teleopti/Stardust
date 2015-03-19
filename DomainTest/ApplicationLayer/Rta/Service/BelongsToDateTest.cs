﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783)]
	[Toggle(Toggles.RTA_CalculatePercentageInAgentTimezone_31236)]
	public class BelongsToDateTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public IRta Target;

		[Test]
		public void ShouldPublishWithBelongsToDateFromCurrentSchedule()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-02-19".Date(), "2015-02-20 1:00", "2015-02-20 7:00")
				.WithAlarm("phone", phone, 0)
				;
			Now.Is("2015-02-20 2:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone",
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-19".Date());
		}

		[Test]
		public void ShouldPublishWithBelongsToDateFromShiftEnded()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-02-20".Date(), "2015-02-20 8:00", "2015-02-20 17:00")
				.WithAlarm("phone", null, 0)
				;
			Now.Is("2015-02-20 18:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone",
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-20".Date());
		}

		[Test]
		public void ShouldPublishWithBelongsToDateFromShiftStarting()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-02-20".Date(), "2015-02-20 8:00", "2015-02-20 17:00")
				.WithAlarm("phone", null, 0)
				;
			Now.Is("2015-02-20 7:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone",
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-20".Date());
		}

		[Test]
		public void ShouldNotPublishWhenNoScheduleNear()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-02-20".Date(), "2015-02-20 8:00", "2015-02-20 17:00")
				.WithSchedule(personId, phone, "2015-02-20".Date(), "2015-02-20 20:00", "2015-02-20 21:00")
				.WithAlarm("phone", null, 0)
				;

			Now.Is("2015-02-20 18:01");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone",
			});
			Now.Is("2015-02-20 18:59");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone",
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single()
				.BelongsToDate.Should().Be(null);
		}

		[Test]
		public void ShouldNotPublishIfNoSchedule()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithAlarm("phone", null, 0);
			Now.Is("2015-02-20 18:01");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone",
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single()
				.BelongsToDate.Should().Be(null);
		}

		[Test]
		public void ShouldPublishOutOfAdherenceEventWithBelongsToDate()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-02-19".Date(), "2015-02-20 1:00", "2015-02-20 7:00")
				.WithAlarm("phone", phone, -1)
				;
			Now.Is("2015-02-20 2:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone",
			});

			Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-19".Date());
		}

		[Test]
		[Toggle(Toggles.RTA_NeutralAdherence_30930)]
		public void ShouldPublishNeutralAdherenceEventWithBelongsToDate()
		{
			var personId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, admin, "2015-02-19".Date(), "2015-02-20 1:00", "2015-02-20 7:00")
				.WithAlarm("admin", admin, 0, Adherence.Neutral)
				;
			Now.Is("2015-02-20 2:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin",
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-19".Date());
		}

		[Test]
		[Toggle(Toggles.RTA_NeutralAdherence_30930)]
		public void ShouldPublishUnknownAdherenceEventWithBelongsToDate()
		{
			var personId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, admin, "2015-02-19".Date(), "2015-02-20 1:00", "2015-02-20 7:00")
				.WithAlarm("admin", admin, 0, Adherence.In)
				;
			Now.Is("2015-02-20 2:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin",
			});
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "someOtherCode",
			});

			Publisher.PublishedEvents.OfType<PersonUnknownAdherenceEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-19".Date());
		}

		[Test]
		public void ShouldPublishShiftStartEventWithBelongsToDate()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2015-02-19".Date(), "2015-02-20 1:00", "2015-02-20 7:00")
				;

			Now.Is("2015-02-20 2:00");
			Target.CheckForActivityChange(personId, businessUnitId);

			Publisher.PublishedEvents.OfType<PersonShiftStartEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-19".Date());
		}

		[Test]
		public void ShouldPublishShiftEndEventWithBelongsToDate()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, Guid.NewGuid(), "2015-02-19".Date(), "2015-02-20 1:00", "2015-02-20 7:00")
				;

			Now.Is("2015-02-20 2:00");
			Target.CheckForActivityChange(personId, businessUnitId);
			Now.Is("2015-02-20 8:00");
			Target.CheckForActivityChange(personId, businessUnitId);

			Publisher.PublishedEvents.OfType<PersonShiftEndEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-19".Date());
		}

		[Test]
		public void ShouldPublishActivityStartEventWithBelongsToDate()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2015-02-19".Date(), "2015-02-20 1:00", "2015-02-20 7:00")
				;

			Now.Is("2015-02-20 2:00");
			Target.CheckForActivityChange(personId, businessUnitId);

			Publisher.PublishedEvents.OfType<PersonActivityStartEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-19".Date());
		}

		[Test]
		public void ShouldPublishStateChangedEventWithBelongsToDate()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-02-19".Date(), "2015-02-20 1:00", "2015-02-20 7:00")
				;
			Now.Is("2015-02-20 2:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone",
			});

			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-19".Date());
		}
	}
}