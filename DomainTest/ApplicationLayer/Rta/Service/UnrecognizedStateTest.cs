using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class UnrecognizedStateTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public IRtaStateGroupRepository StateGroups;

		[Test]
		public void ShouldAddStateCodeToDatabase()
		{
			Database
				.WithUser("usercode")
				.WithRule();

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "newStateCode"
			});

			Database.StateCodes.Select(x => x.StateCode).Should().Contain("newStateCode");
		}

		[Test]
		public void ShouldUpdateReadModelWithDefaultState()
		{
			Database
				.WithUser("usercode")
				.WithRule(Guid.NewGuid(), "loggedout", null, "Logged Out");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "unrecognized-loggedout"
			});

			Database.StoredState.StateGroupId.Should().Be(StateGroups.LoadAll().Single(x => x.DefaultStateGroup).Id.Value);
			Database.PersistedReadModel.StateName.Should().Be("Logged Out");
		}
		
		[Test]
		public void ShouldUpdateReadModelWithDefaultRule()
		{	
			Database
				.WithUser("usercode")
				.WithRule(Guid.NewGuid(), "loggedout", null, "adhering");
			
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "unrecognized-loggedout"
			});

			Database.PersistedReadModel.RuleName.Should().Be("adhering");
		}
		
		[Test]
		public void ShouldAddStateCodeWithDescription()
		{
			Database
				.WithUser("usercode")
				.WithRule();

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "newStateCode",
				StateDescription = "a new description"
			});

			Database.StateCodes.Select(x => x.Name).Should().Contain("a new description");
		}

		[Test]
		public void ShouldNotAddStateCodeIfNotStateReceived()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithRule("someStateCode");

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Database.StateCodes.Select(x => x.StateCode).Should().Have.SameValuesAs("someStateCode");
		}

		[Test]
		public void ShouldAddStateCodeAsNameWhenCheckingForActivityChange()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithRule()
				.WithExistingAgentState(personId, "statecode");

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Database.StateCodes.Select(x => x.Name).Should().Contain("statecode");
		}

		[Test]
		public void ShouldNotAddDuplicatesInBatch()
		{
			Database
				.WithUser("usercode1")
				.WithUser("usercode2")
				.WithUser("usercode3")
				.WithRule();

			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForTest
				{
					UserCode = "usercode1",
					StateCode = "phone",
					BatchId = "2016-05-18 08:00".Utc()
				},
				new ExternalUserStateForTest
				{
					UserCode = "usercode2",
					StateCode = "phone",
					BatchId = "2016-05-18 08:00".Utc()
				}
			});
			
			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForTest
				{
					UserCode = "usercode3",
					StateCode = "phone",
					BatchId = "2016-05-18 08:05".Utc()
				}
			});

			Database.StateCodes.Where(x => x.StateCode == Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot)
				.Should().Have.Count.EqualTo(1);
		}
	}
}