using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
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
	[Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783)]
	public class PersonInAdherenceEventTest
	{
		public FakeRtaDatabase database;
		public FakeEventPublisher publisher;
		public MutableNow now;
		public FakeCurrentDatasource dataSource;
		public IRta target;

		[Test]
		public void ShouldPublishPersonInAdherenceEventWhenNoStaffingEffect()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			database
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("statecode", activityId, 0);
			now.Is("2014-10-20 9:00");

			target.SaveState(state);

			var @event = publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}
		
		[Test]
		public void ShouldNotPublishEventIfStillInAdherence()
		{
			var state1 = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode1"
			};
			var state2 = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode2"
			};
			var activityId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			database
				.WithDefaultsFromState(state1)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("statecode1", activityId, 0)
				.WithAlarm("statecode2", activityId, 0);
			now.Is("2014-10-20 9:00");

			target.SaveState(state1);
			target.SaveState(state2);

			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishTheTimeWhenPersonInAdherence()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			database
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithAlarm("statecode", activityId, 0);
			now.Is("2014-10-20 9:00");

			target.SaveState(state);

			var @event = publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single();
			@event.Timestamp.Should().Be("2014-10-20 9:00".Utc());
		}

		[Test]
		public void ShouldPublishEventWithLogOnInfo()
		{
			var businessUnitId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			database
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-11-11 10:00", "2014-11-11 12:00")
				.WithAlarm("statecode", activityId, 0);
			dataSource.FakeName("datasource");
			now.Is("2014-11-11 11:00");

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = (ILogOnInfo) publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single();
			@event.BusinessUnitId.Should().Be(businessUnitId);
			@event.Datasource.Should().Be("datasource");
		}

		[Test]
		public void ShouldPublishWithTeamId()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			database
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, null, teamId, null)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("statecode", activityId, 0);
			now.Is("2014-10-20 9:00");

			target.SaveState(state);

			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single()
				.TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldPublishWithsiteId()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			database
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, null, null, siteId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("statecode", activityId, 0);
			now.Is("2014-10-20 9:00");

			target.SaveState(state);

			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single()
				.SiteId.Should().Be(siteId);
		}

	}
}