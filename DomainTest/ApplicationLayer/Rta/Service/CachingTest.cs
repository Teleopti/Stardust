using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class CachingTest
	{
		public FakeRtaDatabase Database;
		public FakeAllBusinessUnitsUnitOfWorkAspect UnitOfWorkAspect;
		public IRta Target;

		[Test]
		public void ShouldOnlyOpenUnitOfWorkFirstTime()
		{
			Database
				.WithUser("usercode")
				.WithAlarm("phone", null);

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			UnitOfWorkAspect.Invoked.Should().Be.True();

			UnitOfWorkAspect.Invoked = false;
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			UnitOfWorkAspect.Invoked.Should().Be.False();
		}

	}
}