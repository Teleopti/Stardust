using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class UpdateAgentStateReadModelActivityTest
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		
		[Test]
		public void ShouldPersistWithCurrentActivity()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "Phone", "2014-10-20 10:00", "2014-10-20 11:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.PersistedReadModel.Activity.Should().Be("Phone");
		}

		[Test]
		public void ShouldPersistNextStartWithNullWhenNoActivity()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId);

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Database.PersistedReadModel.NextActivityStartTime.Should().Be(null);
		}

		[Test]
		public void ShouldNotPersistWithCurrentActivityIfNoSchedule()
		{
			Database
				.WithAgent("usercode")
				;
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.PersistedReadModel.Activity.Should().Be(null);
		}

		[Test]
		public void ShouldNotPersistWithCurrentActivityIfFutureSchedule()
		{
			var state = new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 11:00", "2014-10-20 12:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(state);

			Database.PersistedReadModel.Activity.Should().Be(null);
		}

		[Test]
		public void ShouldPersistWithNextActivity()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "Lunch", "2014-10-20 10:00", "2014-10-20 11:00")
				.WithSchedule(personId, Guid.NewGuid(), "Phone", "2014-10-20 11:00", "2014-10-20 12:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.PersistedReadModel.NextActivity.Should().Be("Phone");
		}

		[Test]
		public void ShouldNotPersistWithNextActivityFromNextShift()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "Phone", "2014-10-20 10:00", "2014-10-20 11:00")
				.WithSchedule(personId, Guid.NewGuid(), "Phone", "2014-10-21 10:00", "2014-10-21 11:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.PersistedReadModel.NextActivity.Should().Be(null);
		}

		[Test]
		public void ShouldPersistWithNextActivityFromFutureShift()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "Phone", "2014-10-20 11:00", "2014-10-20 12:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.PersistedReadModel.NextActivity.Should().Be("Phone");
		}

	}
}
