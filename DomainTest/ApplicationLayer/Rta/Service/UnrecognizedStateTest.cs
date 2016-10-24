using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
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
				.WithRule();

			Target.SaveState(new StateForTest
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
				.WithRule();

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "newStateCode",
				StateDescription = "a new description"
			});

			Database.StateCodes.Select(x => x.Name).Should().Contain("a new description");
		}

		[Test]
		public void ShouldNotAddStateCodeUnlessStateReceived()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithRule("someStateCode");

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Database.StateCodes.Select(x => x.StateCode).Should().Have.SameValuesAs("someStateCode");
		}

		[Test]
		public void ShouldAddStateCodeAsNameWhenCheckingForActivityChange()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithRule();
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});
			StateGroups.LoadAll().Single(x => x.DefaultStateGroup).ClearStates();

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Database.StateCodes.Select(x => x.Name).Should().Contain("statecode");
		}

		[Test]
		public void ShouldUpdateReadModelWithDefaultState()
		{
			Database
				.WithAgent("usercode")
				.WithRule(Guid.NewGuid(), "loggedout", null, "Logged Out");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "unrecognized-loggedout"
			});

			Database.StoredState.StateGroupId.Should().Be(StateGroups.LoadAll().Single(x => x.DefaultStateGroup).Id.Value);
			Database.PersistedReadModel.StateName.Should().Be("Logged Out");
		}


	}
}