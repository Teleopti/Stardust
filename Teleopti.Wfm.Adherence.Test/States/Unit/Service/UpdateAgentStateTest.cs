using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class UpdateAgentStateTest
	{
		public FakeDatabase Database;
		public Rta Target;
		public MutableNow Now;
		public FakeAgentStateReadModelPersister ReadModels;

		[Test]
		public void ShouldPersist()
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

			Database.StoredState.Should().Not.Be.Null();
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
		public void ShouldPersistWhenNotifiedOfPossibleScheduleChange()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				;
			Now.Is("2014-10-20 10:00");

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Database.StoredState.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistWithReceivedSystemTime()
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

			Database.StoredState.ReceivedTime.Should().Be("2014-10-20 10:00".Utc());
		}

		[Test]
		// no schedule == schedule is updated? :-)
		public void ShouldPersistWhenScheduleIsUpdated()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId);

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Database.StoredState.PersonId.Should().Be(personId);
		}

	}
}