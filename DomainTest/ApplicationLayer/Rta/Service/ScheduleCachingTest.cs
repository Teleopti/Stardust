using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Domain.FeatureFlags.Toggles.RTA_ScheduleQueryOptimizationFilteredCache_40260)]
	public class ScheduleCachingTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public MutableNow Now;
		public IIoCTestContext Context;

		[Test]
		public void ShouldCacheSchedule()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, "phone", "2016-09-06 14:00", "2016-09-06 16:00")
				;

			Now.Is("2016-09-06 15:00");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.StoredState.Schedule.Single().Name.Should().Be.EqualTo("phone");
		}

		[Test]
		public void ShouldInvalidateOldSchedule()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, "phone", "2016-09-06 14:00", "2016-09-06 16:00")
				;

			Now.Is("2016-09-01 15:00");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Context.SimulateRestart();
			Now.Is("2016-09-06 15:00");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			Database.StoredState.Schedule.Single().Name.Should().Be.EqualTo("phone");
		}

		[Test]
		public void ShouldInvalidateChangedSchedule()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, "phone", "2016-09-06 14:00", "2016-09-06 16:00")
				;

			Now.Is("2016-09-06 15:00");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Database
				.ClearSchedule(personId)
				.WithSchedule(personId, "admin", "2016-09-06 14:00", "2016-09-06 16:00");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			Database.StoredState.Schedule.Single().Name.Should().Be.EqualTo("admin");

		}






		[Test]
		public void ShouldIncludeEarlierShiftWhenBetweenShifts()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, "phone", "2016-09-11 00:00", "2016-09-11 06:00")
				.WithSchedule(personId, "phone", "2016-09-11 08:00", "2016-09-11 12:00")
				.WithSchedule(personId, "admin", "2016-09-11 12:00", "2016-09-11 17:00")
				;

			Now.Is("2016-09-11 07:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2016-09-12 07:00");
			Target.CheckForActivityChanges(Database.TenantName());

			var actual = Database.StoredState.Schedule.Select(x => x.StartDateTime);
			actual.Should().Not.Contain("2016-09-11 00:00".Utc());
			actual.Should().Contain("2016-09-11 08:00".Utc());
			actual.Should().Contain("2016-09-11 12:00".Utc());
		}

		[Test]
		public void ShouldOnlyIncludeUpcommingShiftWhenBetweenShifts()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, "phone", "2016-09-12 08:00", "2016-09-12 12:00")
				.WithSchedule(personId, "admin", "2016-09-12 12:00", "2016-09-12 17:00")
				.WithSchedule(personId, "phone", "2016-09-13 08:00", "2016-09-13 17:00")
				;

			Now.Is("2016-09-11 00:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2016-09-12 00:00");
			Target.CheckForActivityChanges(Database.TenantName());

			var actual = Database.StoredState.Schedule.Select(x => x.StartDateTime);
			actual.Should().Contain("2016-09-12 08:00".Utc());
			actual.Should().Contain("2016-09-12 12:00".Utc());
			actual.Should().Not.Contain("2016-09-13 08:00".Utc());
		}

		[Test]
		public void ShouldOnlyIncludeCurrentShift()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, "phone", "2016-09-11 08:00", "2016-09-11 12:00")
				.WithSchedule(personId, "admin", "2016-09-11 12:00", "2016-09-11 17:00")
				.WithSchedule(personId, "phone", "2016-09-12 08:00", "2016-09-12 12:00")
				.WithSchedule(personId, "admin", "2016-09-12 12:00", "2016-09-12 17:00")
				.WithSchedule(personId, "phone", "2016-09-13 08:00", "2016-09-13 12:00")
				.WithSchedule(personId, "admin", "2016-09-13 12:00", "2016-09-13 17:00")
				;

			Now.Is("2016-09-11 09:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2016-09-12 09:00");
			Target.CheckForActivityChanges(Database.TenantName());

			var actual = Database.StoredState.Schedule.Select(x => x.StartDateTime);
			actual.Should().Not.Contain("2016-09-11 08:00".Utc());
			actual.Should().Not.Contain("2016-09-11 12:00".Utc());
			actual.Should().Contain("2016-09-12 08:00".Utc());
			actual.Should().Contain("2016-09-12 12:00".Utc());
			actual.Should().Not.Contain("2016-09-13 08:00".Utc());
			actual.Should().Not.Contain("2016-09-13 12:00".Utc());
		}

		[Test]
		public void ShouldIncludePastShiftInTimeWindow()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, "phone", "2016-09-12 03:00", "2016-09-12 04:00")
				.WithSchedule(personId, "admin", "2016-09-12 05:00", "2016-09-12 06:15")
				.WithSchedule(personId, "phone", "2016-09-12 06:15", "2016-09-12 06:30")
				;

			Now.Is("2016-09-11 07:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2016-09-12 07:00");
			Target.CheckForActivityChanges(Database.TenantName());

			var actual = Database.StoredState.Schedule.Select(x => x.StartDateTime);
			actual.Should().Not.Contain("2016-09-12 03:00".Utc());
			actual.Should().Contain("2016-09-12 05:00".Utc());
			actual.Should().Contain("2016-09-12 06:15".Utc());
		}

		[Test]
		public void ShouldIncludeAnyShiftStartingInTimeWindow()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, "phone", "2016-09-12 08:00", "2016-09-12 08:30")
				.WithSchedule(personId, "admin", "2016-09-12 08:30", "2016-09-12 09:00")
				.WithSchedule(personId, "phone", "2016-09-12 09:30", "2016-09-12 12:00")
				.WithSchedule(personId, "admin", "2016-09-12 12:00", "2016-09-12 17:00")
				.WithSchedule(personId, "phone", "2016-09-13 08:00", "2016-09-13 17:00")
				;

			Now.Is("2016-09-11 07:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2016-09-12 07:00");
			Target.CheckForActivityChanges(Database.TenantName());

			var actual = Database.StoredState.Schedule.Select(x => x.StartDateTime);
			actual.Should().Contain("2016-09-12 08:00".Utc());
			actual.Should().Contain("2016-09-12 08:30".Utc());
			actual.Should().Contain("2016-09-12 09:30".Utc());
			actual.Should().Contain("2016-09-12 12:00".Utc());
			actual.Should().Not.Contain("2016-09-13 08:00".Utc());
		}

		[Test]
		public void ShouldIncludeNextShiftWhenStartingInTimeWindowWhenCurrentShiftEnds()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, "phone", "2016-09-12 08:00", "2016-09-12 17:00")
				.WithSchedule(personId, "admin", "2016-09-12 19:00", "2016-09-12 23:00")
				.WithSchedule(personId, "phone", "2016-09-13 08:00", "2016-09-13 17:00")
				;

			Now.Is("2016-09-11 07:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2016-09-12 07:00");
			Target.CheckForActivityChanges(Database.TenantName());

			var actual = Database.StoredState.Schedule.Select(x => x.StartDateTime);
			actual.Should().Contain("2016-09-12 08:00".Utc());
			actual.Should().Contain("2016-09-12 19:00".Utc());
			actual.Should().Not.Contain("2016-09-13 08:00".Utc());
		}




		[Test]
		public void ShouldInvalidateJumpsOverShift()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, "phone", "2016-09-11 08:00", "2016-09-11 17:00")
				.WithSchedule(personId, "phone", "2016-09-12 08:00", "2016-09-12 17:00")
				.WithSchedule(personId, "admin", "2016-09-13 08:00", "2016-09-13 17:00")
				;

			Now.Is("2016-09-12 07:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2016-09-12 18:00");
			Target.CheckForActivityChanges(Database.TenantName());

			var actual = Database.StoredState.Schedule.Select(x => x.StartDateTime);
			actual.Should().Not.Contain("2016-09-11 08:00".Utc());
			actual.Should().Contain("2016-09-12 08:00".Utc());
			actual.Should().Contain("2016-09-13 08:00".Utc());
		}

		[Test]
		public void ShouldInvalidateJumpsBetweenShifts()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, "phone", "2016-09-12 01:00", "2016-09-12 08:00")
				.WithSchedule(personId, "phone", "2016-09-12 10:00", "2016-09-12 18:00")
				.WithSchedule(personId, "phone", "2016-09-12 20:00", "2016-09-13 23:00")
				;

			Now.Is("2016-09-12 02:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2016-09-12 11:00");
			Target.CheckForActivityChanges(Database.TenantName());

			var actual = Database.StoredState.Schedule.Select(x => x.StartDateTime);
			actual.Should().Not.Contain("2016-09-12 01:00".Utc());
			actual.Should().Contain("2016-09-12 10:00".Utc());
			actual.Should().Contain("2016-09-12 20:00".Utc());
		}
	}
}