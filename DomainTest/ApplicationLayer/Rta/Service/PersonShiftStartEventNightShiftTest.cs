using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class PersonShiftStartEventNightShiftTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPublishWithNightshift()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2016-06-08 21:00", "2016-06-09 05:00");

			Now.Is("2016-06-08 22:00".Utc());
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logout"
			});

			Publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single()
				.Nightshift.Should().Be(true);
		}
		
		[Test]
		public void ShouldNotPublishWithNightshiftForDayShift()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2016-06-08 08:00", "2016-06-08 17:00");

			Now.Is("2016-06-08 09:00".Utc());
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logout"
			});

			Publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single()
				.Nightshift.Should().Be(false);
		}

		[Test]
		public void ShouldPublishWithNightshiftWithMultipleActivities()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2016-06-08 21:00", "2016-06-08 23:00")
				.WithSchedule(personId, Guid.NewGuid(), "2016-06-08 23:00", "2016-06-09 05:00");

			Now.Is("2016-06-08 22:00".Utc());
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logout"
			});

			Publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single()
				.Nightshift.Should().Be(true);
		}
	}
}