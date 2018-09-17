using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Wfm.Adherence.Test.Domain.Service
{
	[TestFixture]
	[RtaTest]
	public class CaseInsensitiveTest
	{
		public FakeDatabase Database;
		public Ccc.Domain.RealTimeAdherence.Domain.Service.Rta Target;
		public FakeAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldHandleExternalLogon()
		{
			Database
				.WithAgent("USERCODE");

			Target.ProcessState(new StateForTest
			{
				StateCode = "somethin",
				UserCode = "usercode"
			});

			Persister.Models.Single().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldHandleStateCode()
		{
			var stateGroupId = Guid.NewGuid();
			Database
				.WithAgent("usercode")
				.WithStateGroup(null, "Im_default!", true)
				.WithStateCode("something")
				.WithStateGroup(stateGroupId,null)
				.WithStateCode("STATECODE");

			Target.ProcessState(new StateForTest
			{
				StateCode = "statecode",
				UserCode = "usercode"
			});

			Persister.Models.Single().StateGroupId.Should().Be(stateGroupId);
		}
	}
}