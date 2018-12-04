using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class UnrecognizedStateTest
	{
		public FakeDatabase Database;
		public Rta Target;
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
		
	}
}