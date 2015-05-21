using NUnit.Framework;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	//todo: tenant fill with test from authenticationcontrollertest and core here

	[TenantTest]
	public class IdentityLogonTest
	{
		public TenantAuthenticationFake TenantAuthentication;
		public AuthenticateController Target;

		[Test]
		public void ShouldAcceptAccessWithoutTenantCredentials()
		{
			TenantAuthentication.NoAccess();

			Assert.DoesNotThrow(() =>
				Target.IdentityLogon(new IdentityLogonModel{Identity = RandomName.Make() }));
		}
	}
}