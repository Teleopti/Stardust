using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
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
		public void ShouldPersist()
		{
			Database
				.WithUser("usercode")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.PersistedReadModel.Should().Not.Be.Null();
		}

		[Test]
		[Ignore("Reason mandatory for NUnit 3")]
		public void ShouldNotPersistWhenWrongDataSource()
		{
			Assert.Fail();
		}

		[Test]
		[Ignore("Reason mandatory for NUnit 3")]
		public void ShouldNotPersisthenWrongPerson()
		{
			Assert.Fail();
		}

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
		public void ShouldPersistWhenNotifiedOfPossibleScheduleChange()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				;
			Now.Is("2014-10-20 10:00");

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Database.StoredState.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistWithReceivedSystemTime()
		{
			Database
				.WithUser("usercode")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.StoredState.ReceivedTime.Should().Be("2014-10-20 10:00".Utc());
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