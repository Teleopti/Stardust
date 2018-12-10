using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service.AgentStateReadModel
{
	[RtaTest]
	[TestFixture]
	public class UpdateAgentStateReadModelActivityTest
	{
		public FakeDatabase Database;
		public MutableNow Now;
		public Rta Target;
		public FakeAgentStateReadModelPersister ReadModels;

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

			ReadModels.Models.Single(x => x.PersonId == personId)
				.Activity.Should().Be("Phone");
		}

		[Test]
		public void ShouldPersistNextStartWithNullWhenNoActivity()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId);

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			ReadModels.Models.Single(x => x.PersonId == personId)
				.NextActivityStartTime.Should().Be(null);
		}

		[Test]
		public void ShouldNotPersistWithCurrentActivityIfNoSchedule()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent(personId, "usercode")
				;
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			ReadModels.Models.Single(x => x.PersonId == personId)
				.Activity.Should().Be(null);
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

			ReadModels.Models.Single(x => x.PersonId == personId)
				.Activity.Should().Be(null);
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

			ReadModels.Models.Single(x => x.PersonId == personId)
				.NextActivity.Should().Be("Phone");
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

			ReadModels.Models.Single(x => x.PersonId == personId)
				.NextActivity.Should().Be(null);
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

			ReadModels.Models.Single(x => x.PersonId == personId)
				.NextActivity.Should().Be("Phone");
		}

	}
}
