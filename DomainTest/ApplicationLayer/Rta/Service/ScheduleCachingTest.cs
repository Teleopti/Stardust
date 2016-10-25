using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[RealPermissions]
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
				.WithAgent("usercode", personId)
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
		public void ShouldCacheEmptySchedule()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				;

			Now.Is("2016-09-28 13:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Database
				.WithSchedule(personId, "phone", "2016-09-28 08:00", "2016-09-28 17:00")
				.ClearSchedule(personId);
			Target.CheckForActivityChanges(Database.TenantName());

			Database.StoredState.Schedule.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldInvalidateOldSchedule()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
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
				.WithAgent("usercode", personId)
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
		public void ShouldInvalidateOldScheduleWithoutStateChange()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, "phone", "2016-09-06 14:00", "2016-09-06 16:00")
				.WithSchedule(personId, "admin", "2016-09-16 14:00", "2016-09-16 16:00")
				;

			Now.Is("2016-09-06 15:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2016-09-16 15:00");
			Target.CheckForActivityChanges(Database.TenantName());

			Database.StoredState.Schedule.Single().Name.Should().Be.EqualTo("admin");

		}

	}
}