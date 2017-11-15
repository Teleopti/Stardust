using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class PersonActivityStartEventTest
	{
		public FakeDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPublishEvent()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishIfNextActivityHasStarted()
		{
			var personId = Guid.NewGuid();
			var activityId1 = Guid.NewGuid();
			var activityId2 = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId1, "2014-10-20 10:00", "2014-10-20 10:15")
				.WithSchedule(personId, activityId2, "2014-10-20 10:15", "2014-10-20 11:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.CheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2014-10-20 10:05");
			Target.CheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2014-10-20 10:15");
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			var events = Publisher.PublishedEvents.OfType<PersonActivityStartEvent>();
			events.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldPublishWithActivityInfo()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "phone", "2014-10-20 10:00", "2014-10-20 11:00")
				;
			Now.Is("2014-10-20 10:02");

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.StartTime.Should().Be("2014-10-20 10:00".Utc());
			@event.Name.Should().Be("phone");
		}
		
	}
}