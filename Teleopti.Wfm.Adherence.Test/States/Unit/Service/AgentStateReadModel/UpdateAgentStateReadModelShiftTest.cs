using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service.AgentStateReadModel
{
	[TestFixture]
	[RtaTest]
	public class UpdateAgentStateReadModelShiftTest
	{
		public FakeDatabase Database;
		public MutableNow Now;
		public Rta Target;
		public FakeAgentStateReadModelPersister ReadModels;

		[Test]
		public void ShouldPersistShift()
		{
			var person = Guid.NewGuid();
			Now.Is("2016-05-30 09:00");
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, Color.Green, "2016-05-30 09:00", "2016-05-30 10:00");

			Target.CheckForActivityChanges(Database.TenantName());

			var shift = ReadModels.Models.Single(x => x.PersonId == person)
				.Shift.Single();
			shift.StartTime.Should().Be("2016-05-30 09:00".Utc());
			shift.EndTime.Should().Be("2016-05-30 10:00".Utc());
			shift.Color.Should().Be(Color.Green.ToArgb());
		}

		[Test]
		public void ShouldPersistShiftWithActivityName()
		{
			var person = Guid.NewGuid();
			Now.Is("2016-05-30 09:00");
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, "Phone", "2016-05-30 09:00", "2016-05-30 10:00");

			Target.CheckForActivityChanges(Database.TenantName());

			var shift = ReadModels.Models.Single(x => x.PersonId == person)
				.Shift.Single();
			shift.Name.Should().Be("Phone");
		}

		[Test]
		public void ShouldPersistTwoActivities()
		{
			var person = Guid.NewGuid();
			Now.Is("2016-05-30 09:00");
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, Color.Green, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithSchedule(person, Color.Red, "2016-05-30 10:00", "2016-05-30 11:00");

			Target.CheckForActivityChanges(Database.TenantName());

			var shift = ReadModels.Models.Single(x => x.PersonId == person)
				.Shift;
			shift.Count().Should().Be(2);
			shift.First().Color.Should().Be(Color.Green.ToArgb());
			shift.First().StartTime.Should().Be("2016-05-30 09:00".Utc());
			shift.First().EndTime.Should().Be("2016-05-30 10:00".Utc());
			shift.Last().Color.Should().Be(Color.Red.ToArgb());
			shift.Last().StartTime.Should().Be("2016-05-30 10:00".Utc());
			shift.Last().EndTime.Should().Be("2016-05-30 11:00".Utc());
		}

		[Test]
		public void ShouldExcludeActivityBeforeTimeWindow()
		{
			var person = Guid.NewGuid();
			Now.Is("2016-05-30 11:30");
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, Color.Green, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithSchedule(person, Color.Red, "2016-05-30 10:00", "2016-05-30 11:00");

			Target.CheckForActivityChanges(Database.TenantName());

			ReadModels.Models.Single(x => x.PersonId == person)
				.Shift.Single().Color.Should().Be(Color.Red.ToArgb());
		}

		[Test]
		public void ShouldExcludeActivityAfterTimeWindow()
		{
			var person = Guid.NewGuid();
			Now.Is("2016-05-30 06:30");
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, Color.Green, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithSchedule(person, Color.Red, "2016-05-30 10:00", "2016-05-30 11:00");

			Target.CheckForActivityChanges(Database.TenantName());

			ReadModels.Models.Single(x => x.PersonId == person)
				.Shift.Single().Color.Should().Be(Color.Green.ToArgb());
		}

		[Test]
		public void ShouldPersistActivityEnteringTimeWindow()
		{
			var person = Guid.NewGuid();
			Now.Is("2016-05-30 09:55");
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, Color.Yellow, "2016-05-30 9:00", "2016-05-30 13:00")
				.WithSchedule(person, Color.Green, "2016-05-30 13:00", "2016-05-30 14:00");

			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2016-05-30 10:05");
			Target.CheckForActivityChanges(Database.TenantName());

			ReadModels.Models.Single(x => x.PersonId == person)
				.Shift.Last().Color.Should().Be(Color.Green.ToArgb());
		}

		[Test]
		public void ShouldPersistShiftEnteringTimeWindow()
		{
			var person = Guid.NewGuid();
			Now.Is("2016-05-30 09:55");
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, Color.Green, "2016-05-30 13:00", "2016-05-30 14:00");

			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2016-05-30 10:05");
			Target.CheckForActivityChanges(Database.TenantName());

			ReadModels.Models.Single(x => x.PersonId == person)
				.Shift.Single().Color.Should().Be(Color.Green.ToArgb());
		}

		[Test]
		public void ShouldNotPersistIfNoActivityEnteringTimeWindow()
		{
			var person = Guid.NewGuid();
			Now.Is("2016-05-30 09:55");
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, Color.Green, "2016-05-30 13:00", "2016-05-30 14:00");

			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2016-05-30 09:59");
			Target.CheckForActivityChanges(Database.TenantName());

			ReadModels.Models.Single(x => x.PersonId == person)
				.ReceivedTime.Should().Be("2016-05-30 09:55".Utc());
		}

		[Test]
		public void ShouldNotPersistIfNothingChanges()
		{
			var person = Guid.NewGuid();
			Now.Is("2016-05-30 10:00");
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, Color.Green, "2016-05-30 10:00", "2016-05-30 11:00");

			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2016-05-30 10:01");
			Target.CheckForActivityChanges(Database.TenantName());

			ReadModels.Models.Single(x => x.PersonId == person)
				.ReceivedTime.Should().Be("2016-05-30 10:00".Utc());
		}

		[Test]
		public void ShouldPersistAllKindsOfScheduleChanges()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var shortbreak = Guid.NewGuid();
			Now.Is("2016-05-30 10:00");
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 10:00", "2016-05-30 11:00")
				.WithSchedule(person, shortbreak, "2016-05-30 11:00", "2016-05-30 12:00")
				.WithSchedule(person, Color.Orange, "2016-05-30 12:00", "2016-05-30 13:00");

			Target.CheckForActivityChanges(Database.TenantName());
			Database.ClearAssignments(person)
				.WithSchedule(person, phone, "2016-05-30 10:00", "2016-05-30 11:00")
				.WithSchedule(person, shortbreak, "2016-05-30 11:00", "2016-05-30 12:00")
				.WithSchedule(person, Color.Pink, "2016-05-30 12:00", "2016-05-30 13:00");
			Target.CheckForActivityChanges(Database.TenantName());

			ReadModels.Models.Single(x => x.PersonId == person)
				.Shift.Last().Color.Should().Be(Color.Pink.ToArgb());
		}

		[Test]
		public void ShouldNotPersistIfNothingChangesAfterAChange()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var shortbreak = Guid.NewGuid();
			Now.Is("2016-05-30 10:00");
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 10:00", "2016-05-30 11:00")
				.WithSchedule(person, shortbreak, "2016-05-30 11:00", "2016-05-30 12:00")
				.WithSchedule(person, Color.Orange, "2016-05-30 12:00", "2016-05-30 13:00");
			Target.CheckForActivityChanges(Database.TenantName());

			Database
				.ClearAssignments(person)
				.WithSchedule(person, phone, "2016-05-30 10:00", "2016-05-30 11:00")
				.WithSchedule(person, shortbreak, "2016-05-30 11:00", "2016-05-30 12:00")
				.WithSchedule(person, Color.Pink, "2016-05-30 12:00", "2016-05-30 13:00");
			Now.Is("2016-05-30 10:01");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2016-05-30 10:02");
			Target.CheckForActivityChanges(Database.TenantName());

			ReadModels.Models.Single(x => x.PersonId == person)
				.ReceivedTime.Should().Be("2016-05-30 10:01".Utc());
		}
	}
}