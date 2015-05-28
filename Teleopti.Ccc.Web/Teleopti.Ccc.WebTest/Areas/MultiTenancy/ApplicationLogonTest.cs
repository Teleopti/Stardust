using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	//todo: tenant fill with test from authenticationcontrollertest and core here

	[TenantTest]
	public class ApplicationLogonTest
	{
		public TenantAuthenticationFake TenantAuthentication;
		public AuthenticateController Target;
		public ApplicationUserQueryFake ApplicationUserQuery;

		[Test]
		public void ShouldAcceptAccessWithoutTenantCredentials()
		{
			TenantAuthentication.NoAccess();

			Assert.DoesNotThrow(() =>
				Target.ApplicationLogon(new ApplicationLogonModel {UserName = RandomName.Make(), Password = RandomName.Make()}));
		}

		[Test]
		public void NonExistingUserShouldFail()
		{
			var result = Target.ApplicationLogon(new ApplicationLogonModel {UserName = RandomName.Make(), Password = RandomName.Make()}).Result<TenantAuthenticationResult>();

			result.Success.Should().Be.False();
			result.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void IncorrectPasswordShouldFail()
		{
			var logonName = RandomName.Make();
			var personInfo= new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, RandomName.Make());
			ApplicationUserQuery.Has(personInfo);

			var result = Target.ApplicationLogon(new ApplicationLogonModel { UserName = logonName, Password = RandomName.Make() }).Result<TenantAuthenticationResult>();

			result.Success.Should().Be.False();
			result.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}
	}
}