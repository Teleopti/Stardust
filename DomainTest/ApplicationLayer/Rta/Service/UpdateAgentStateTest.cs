using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class UpdateAgentStateTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public MutableNow Now;

		[Test]
		public void ShouldPersistStateCode()
		{
			Database
				.WithUser("usercode");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.StoredState.StateCode.Should().Be("phone");
		}
		
		[Test]
		// no schedule == schedule is updated? :-)
		public void ShouldPersistWhenScheduleIsUpdated()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId);

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Database.StoredState.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPersistScheduleWhenUpdating()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, "phone", "2016-09-06 14:00", "2016-09-06 15:00")
				;

			Now.Is("2016-09-06 15:00");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.StoredState.Schedule.Single().Name.Should().Be.EqualTo("phone");
		}
	}
}