using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	public class EventsOrderTest
	{
		public FakeRtaDatabase database;
		public FakeEventPublisher publisher;
		public MutableNow now;
		public Domain.ApplicationLayer.Rta.Service.Rta target;

		[Test]
		public void ShouldPublishShiftStartBeforeActivityStart()
		{
			var personId = Guid.NewGuid();
			database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2014-10-20 10:00", "2014-10-20 11:00");
			now.Is("2014-10-20 10:00");

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var before = publisher.PublishedEvents.IndexOf(publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single());
			var after = publisher.PublishedEvents.IndexOf(publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single());
			before.Should().Be.LessThan(after);
		}

		[Test]
		public void ShouldPublishActivityStartBeforeAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("phone", phone, 0);
			now.Is("2014-10-20 10:00");

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var before = publisher.PublishedEvents.IndexOf(publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single());
			var after = publisher.PublishedEvents.IndexOf(publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single());
			before.Should().Be.LessThan(after);
		}

		[Test]
		public void ShouldPublishStateChangeBeforeAdherence()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("phone", activityId, 1);
			now.Is("2014-10-20 10:00");

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var before = publisher.PublishedEvents.IndexOf(publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single());
			var after = publisher.PublishedEvents.IndexOf(publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Single());
			before.Should().Be.LessThan(after);
		}
	}
}