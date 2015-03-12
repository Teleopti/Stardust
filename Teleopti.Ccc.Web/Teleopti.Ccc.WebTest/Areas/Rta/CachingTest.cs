using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[RtaTest]
	[TestFixture]
	public class CachingTest
	{
		public FakeRtaDatabase Database;
		public FakeAllBusinessUnitsUnitOfWorkAspect UnitOfWorkAspect;
		public IRta Target;

		[Test, Ignore("Cant include mbcache in test yet")]
		public void ShouldNotOpenUnitOfWorkWhenGettingFromCache()
		{
			Database
				.WithUser("usercode")
				.WithAlarm("phone", null);
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
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