using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.States;
using FakeDatabase = Teleopti.Ccc.TestCommon.FakeRepositories.FakeDatabase;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.ReadModels
{
	[DomainTest]
	public class CurrentScheduleReadModelUpdaterTest
	{
		public FakeDatabase Database;
		public MutableNow Now;
		public ScheduleChangeProcessor Target;
		public FakeCurrentScheduleReadModelPersister Persister;

		[Test]
		public void ShouldContainSchedule()
		{
			var user = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent(user, "user")
				.WithActivity(phone, "phone", "p", Color.Green)
				.WithAssignment("2017-01-25")
				.WithAssignedActivity("2017-01-25 08:00", "2017-01-25 17:00");
			Now.Is("2017-01-25 12:00");

			Target.Handle(new TenantMinuteTickEvent());

			var activity = Persister.Read(0).Single(x => x.PersonId == user).Schedule.Single();
			activity.PersonId.Should().Be(user);
			activity.PayloadId.Should().Be(phone);
			activity.BelongsToDate.Should().Be("2017-01-25".Date());
			activity.Name.Should().Be("phone");
			activity.ShortName.Should().Be("p");
			activity.DisplayColor.Should().Be(Color.Green.ToArgb());
			activity.EndDateTime.Should().Be("2017-01-25 17:00".Utc());
			activity.StartDateTime.Should().Be("2017-01-25 08:00".Utc());
		}

		[Test]
		public void ShouldContainScheduleForAllPersons()
		{
			var phone1 = Guid.NewGuid();
			var phone2 = Guid.NewGuid();
			var user1 = Guid.NewGuid();
			var user2 = Guid.NewGuid();
			Database
				.WithAgent(user1, "user1")
				.WithActivity(phone1)
				.WithAssignment("2017-01-25")
				.WithAssignedActivity("2017-01-25 08:00", "2017-01-25 17:00")
				.WithBusinessUnit(Guid.NewGuid())
				.WithAgent(user2, "user2")
				.WithActivity(phone2)
				.WithAssignment("2017-01-25")
				.WithAssignedActivity("2017-01-25 09:00", "2017-01-25 18:00");
			Now.Is("2017-01-25 12:00");

			Target.Handle(new TenantMinuteTickEvent());

			var result = Persister.Read(0);
			result.Single(x => x.PersonId == user1).Schedule.Single().PayloadId.Should().Be(phone1);
			result.Single(x => x.PersonId == user2).Schedule.Single().PayloadId.Should().Be(phone2);
		}

		[Test]
		public void ShouldContainAllActivities()
		{
			var user = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var email = Guid.NewGuid();
			Database
				.WithAgent(user, "user")
				.WithAssignment("2017-01-25")
				.WithActivity(phone)
				.WithAssignedActivity("2017-01-25 08:00", "2017-01-25 12:00")
				.WithActivity(email)
				.WithAssignedActivity("2017-01-25 12:00", "2017-01-25 17:00");
			Now.Is("2017-01-25 12:00");

			Target.Handle(new TenantMinuteTickEvent());

			Persister.Read(0).Single(x => x.PersonId == user).Schedule.Select(x => x.PayloadId)
				.Should()
				.Have.SameValuesAs(phone, email);
		}

		[Test]
		public void ShouldContainThreeDays()
		{
			var user = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent(user, "user")
				.WithActivity(phone)
				.WithAssignment("2017-01-23")
				.WithAssignedActivity("2017-01-23 08:00", "2017-01-23 17:00")
				.WithAssignment("2017-01-24")
				.WithAssignedActivity("2017-01-24 08:00", "2017-01-24 17:00")
				.WithAssignment("2017-01-25")
				.WithAssignedActivity("2017-01-25 08:00", "2017-01-25 17:00")
				.WithAssignment("2017-01-26")
				.WithAssignedActivity("2017-01-26 08:00", "2017-01-26 17:00")
				.WithAssignment("2017-01-27")
				.WithAssignedActivity("2017-01-27 08:00", "2017-01-27 17:00");
			Now.Is("2017-01-25 12:00");

			Target.Handle(new TenantMinuteTickEvent());

			Persister.Read(0).Single(x => x.PersonId == user).Schedule.Select(x => x.BelongsToDate)
				.Should()
				.Have.SameValuesAs("2017-01-24".Date(), "2017-01-25".Date(), "2017-01-26".Date());
		}

		[Test]
		public void ShouldPersistEmptySchedule()
		{
			var user = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent(user, "user")
				.WithActivity(phone)
				.WithAssignment("2017-01-25")
				.WithAssignedActivity("2017-01-25 08:00", "2017-01-25 17:00");
			Now.Is("2017-01-25 12:00");

			Target.Handle(new TenantMinuteTickEvent());
			Database.ClearAssignments(user);
			Target.Handle(new ScheduleChangedEvent { PersonId = user });
			Target.Handle(new TenantMinuteTickEvent());

			Persister.Read(0).Single(x => x.PersonId == user).Schedule.Should().Be.Empty();
		}

		[Test]
		public void ShouldContainChangedSchedule()
		{
			var user = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var email = Guid.NewGuid();
			Database
				.WithAgent(user, "user")
				.WithActivity(phone)
				.WithAssignment("2017-01-25")
				.WithAssignedActivity("2017-01-25 08:00", "2017-01-25 17:00");
			Now.Is("2017-01-25 12:00");

			Target.Handle(new TenantMinuteTickEvent());
			Database
				.ClearAssignments(user)
				.WithActivity(email)
				.WithAssignment("2017-01-25")
				.WithAssignedActivity("2017-01-25 08:00", "2017-01-25 17:00");
			Target.Handle(new ScheduleChangedEvent {PersonId = user});
			Target.Handle(new TenantMinuteTickEvent());

			var activity = Persister.Read(0).Single(x => x.PersonId == user).Schedule.Single();
			activity.PayloadId.Should().Be(email);
		}

		[Test]
		public void ShouldNotUpdateScheduleUntilNotified()
		{
			var user = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var email = Guid.NewGuid();
			Database
				.WithAgent(user, "user")
				.WithActivity(phone)
				.WithAssignment("2017-01-25")
				.WithAssignedActivity("2017-01-25 08:00", "2017-01-25 17:00");
			Now.Is("2017-01-25 12:00");

			Target.Handle(new TenantMinuteTickEvent());
			Database
				.ClearAssignments(user)
				.WithActivity(email)
				.WithAssignment("2017-01-25")
				.WithAssignedActivity("2017-01-25 08:00", "2017-01-25 17:00");
			Target.Handle(new TenantMinuteTickEvent());

			var activity = Persister.Read(0).Single(x => x.PersonId == user).Schedule.Single();
			activity.PayloadId.Should().Be(phone);
		}

		[Test]
		public void ShouldUpdateEveryDay()
		{
			var user = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent(user, "user")
				.WithActivity(phone)
				.WithAssignment("2017-01-26")
				.WithAssignedActivity("2017-01-26 08:00", "2017-01-26 17:00")
				.WithAssignment("2017-01-27")
				.WithAssignedActivity("2017-01-27 08:00", "2017-01-27 17:00");

			Now.Is("2017-01-25 00:00");
			Target.Handle(new TenantDayTickEvent());
			Target.Handle(new TenantMinuteTickEvent());
			Now.Is("2017-01-26 00:00");
			Target.Handle(new TenantDayTickEvent());
			Target.Handle(new TenantMinuteTickEvent());

			Persister.Read(1).Single(x => x.PersonId == user).Schedule.Select(x => x.BelongsToDate)
				.Should().Contain("2017-01-27".Date());
		}

		[Test]
		public void ShouldUpdateWithNewSchedule()
		{
			var user = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent(user, "user")
				.WithActivity(phone);
			Now.Is("2017-01-25 12:00");
			Target.Handle(new TenantMinuteTickEvent());

			Database
				.WithAssignment("2017-01-25")
				.WithAssignedActivity("2017-01-25 08:00", "2017-01-25 17:00");
			Target.Handle(new ScheduleChangedEvent { PersonId = user });
			Target.Handle(new TenantMinuteTickEvent());

			Persister.Read(0).Single(x => x.PersonId == user).Schedule.Select(x => x.BelongsToDate)
				.Should().Contain("2017-01-25".Date());
		}

		[Test]
		public void ShouldUpdatePersonWithoutBusinessUnit()
		{
			var user = Guid.NewGuid();
			Database
				.WithPerson(user, "user");
			Now.Is("2017-01-25 12:00");

			Target.Handle(new ScheduleChangedEvent { PersonId = user });
			Target.Handle(new TenantMinuteTickEvent());

			Persister.GetInvalid().Should().Not.Contain(user);
		}
	}
}