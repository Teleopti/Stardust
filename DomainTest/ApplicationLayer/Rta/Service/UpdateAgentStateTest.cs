using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class UpdateAgentStateTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPersistStateCode()
		{
			Database
				.WithUser("usercode");

			Target.SaveState(new ExternalUserStateForTest
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

	}
}