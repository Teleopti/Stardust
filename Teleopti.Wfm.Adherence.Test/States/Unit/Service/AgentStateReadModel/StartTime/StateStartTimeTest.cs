using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service.AgentStateReadModel.StartTime
{
	[RtaTest]
	[TestFixture]
	public class StateStartTimeTest
	{
		public FakeDatabase Database;
		public MutableNow Now;
		public Rta Target;
		public FakeAgentStateReadModelPersister ReadModels;

		[Test]
		public void ShouldHaveStateStartTimeWhenANewStateArrived()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithStateCode("phone");

			Now.Is("2015-12-10 8:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			ReadModels.Models.Single(x => x.PersonId == personId)
				.StateStartTime.Should().Be("2015-12-10 8:00".Utc());
		}

		[Test]
		public void ShouldNotChangeStateStartTimeWhenStateDoesNotChange()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithMappedRule("stateone", Guid.NewGuid());

			Now.Is("2015-12-10 8:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "stateone"
			});
			Now.Is("2015-12-10 8:30");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "stateone"
			});

			ReadModels.Models.Single(x => x.PersonId == personId)
				.StateStartTime.Should().Be("2015-12-10 8:00".Utc());
		}

		[Test]
		public void ShouldUpdateStateStartTimeWhenStateChanges()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithMappedRule("stateone", Guid.NewGuid())
				.WithMappedRule("statetwo", Guid.NewGuid());

			Now.Is("2015-12-10 8:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "stateone"
			});
			Now.Is("2015-12-10 8:30");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statetwo"
			});

			ReadModels.Models.Single(x => x.PersonId == personId)
				.StateStartTime.Should().Be("2015-12-10 8:30".Utc());
		}
	}
}