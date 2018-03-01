using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Domain.Service.AgentStateReadModel
{
	[TestFixture]
	[RtaTest]
	public class UnrecognizedStateTest
	{
		public FakeDatabase Database;
		public Ccc.Domain.RealTimeAdherence.Domain.Service.Rta Target;
		public FakeRtaStateGroupRepository StateGroups;

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