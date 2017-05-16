using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class UnrecognizedStateTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public FakeRtaStateGroupRepository StateGroups;

		[Test]
		public void ShouldAddStateCodeToDatabase()
		{
			Database
				.WithAgent("usercode")
				.WithStateGroup(null, "default", true);

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "newStateCode"
			});

			Database.StateCodes.Select(x => x.StateCode).Should().Contain("newStateCode");
		}

		[Test]
		public void ShouldAddStateCodeWithDescription()
		{
			Database
				.WithAgent("usercode")
				.WithStateGroup(null, "default", true);

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "newStateCode",
				StateDescription = "a new description"
			});

			Database.StateCodes.Select(x => x.Name).Should().Contain("a new description");
		}

		[Test]
		// this test seems a bit unclear, but saved us today
		public void ShouldNotAddStateCodeUnlessStateReceived()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithMappedRule("someStateCode");

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Database.StateCodes.Select(x => x.StateCode).Should().Have.SameValuesAs("someStateCode");
		}
		
		[Test]
		public void ShouldUpdateReadModelWithDefaultState()
		{
			Database
				.WithAgent("usercode")
				.WithMappedRule(Guid.NewGuid(), "loggedout", null, "Logged Out");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "unrecognized-loggedout"
			});

			Database.StoredState.StateGroupId.Should().Be(StateGroups.LoadAll().Single(x => x.DefaultStateGroup).Id.Value);
			Database.PersistedReadModel.StateName.Should().Be("Logged Out");
		}


	}
}