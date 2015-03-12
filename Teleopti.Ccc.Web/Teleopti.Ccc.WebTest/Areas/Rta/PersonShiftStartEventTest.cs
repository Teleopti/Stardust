using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285)]
	public class PersonShiftStartEventTest
	{
		public FakeRtaDatabase database;
		public FakeEventPublisher publisher;
		public MutableNow now;
		public FakeCurrentDatasource dataSource;
		public IRta target;

		[Test]
		public void ShouldPublishEvent()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00");

			now.Is("2014-10-19 17:02".Utc());
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logout"
			});
			now.Is("2014-10-20 10:00".Utc());
			target.CheckForActivityChange(personId, businessUnitId);

			var @event = publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishIfPersonNeverHasSignedIn()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00");
			now.Is("2014-10-20 10:02");

			target.CheckForActivityChange(personId, businessUnitId);

			var @event = publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishWhenNextShiftStarts()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-19 10:00", "2014-10-19 11:00")
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00");

			now.Is("2014-10-19 10:59");
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logout"
			});
			now.Is("2014-10-19 11:01");
			target.CheckForActivityChange(personId, businessUnitId);
			now.Is("2014-10-20 10:00");
			target.CheckForActivityChange(personId, businessUnitId);

			var @event = publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Last();
			@event.ShiftStartTime.Should().Be("2014-10-20 10:00".Utc());
		}

		[Test]
		public void ShouldPublishWithShiftStartTime()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 09:00", "2014-10-20 10:00")
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00");
			now.Is("2014-10-20 10:02");

			target.CheckForActivityChange(personId, businessUnitId);

			var @event = publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.ShiftStartTime.Should().Be("2014-10-20 09:00".Utc());
		}

		[Test]
		public void ShouldPublishWithShiftEndTime()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithSchedule(personId, activityId, "2014-10-20 11:00", "2014-10-20 12:00");
			now.Is("2014-10-20 10:02");

			target.CheckForActivityChange(personId, businessUnitId);

			var @event = publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.ShiftEndTime.Should().Be("2014-10-20 12:00".Utc());
		}
		
		[Test]
		public void ShouldPublishEventWithLogOnInfo()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00");
			dataSource.FakeName("datasource");

			now.Is("2014-10-19 17:02".Utc());
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logout"
			});
			now.Is("2014-10-20 10:00".Utc());
			target.CheckForActivityChange(personId, businessUnitId);

			var @event = (ILogOnInfo)publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.BusinessUnitId.Should().Be(businessUnitId);
			@event.Datasource.Should().Be("datasource");
		}

	}
}