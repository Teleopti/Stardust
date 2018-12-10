using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service.AgentStateReadModel
{
	[TestFixture]
	[RtaTest]
	public class UnrecognizedStateTest
	{
		public FakeDatabase Database;
		public Rta Target;
		public FakeRtaStateGroupRepository StateGroups;
		public FakeAgentStateReadModelPersister ReadModels;

		[Test]
		public void ShouldUpdateReadModelWithDefaultState()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent(personId, "usercode")
				.WithStateGroup(null, "Logged Out", true)
				.WithStateCode("loggedout")
				;

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "unrecognized-loggedout"
			});

			Database.StoredState.StateGroupId.Should().Be(StateGroups.LoadAll().Single(x => x.DefaultStateGroup).Id.Value);
			ReadModels.Models.Single(x => x.PersonId == personId)
				.StateName.Should().Be("Logged Out");
		}

	}
}