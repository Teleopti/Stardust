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
	public class CaseInsensitiveTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public FakeAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldHandleExternalLogon()
		{
			Database
				.WithAgent("USERCODE");

			Target.SaveState(new StateForTest
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

			Target.SaveState(new StateForTest
			{
				StateCode = "statecode",
				UserCode = "usercode"
			});

			Persister.Models.Single().StateGroupId.Should().Be(stateGroupId);
		}
	}
}