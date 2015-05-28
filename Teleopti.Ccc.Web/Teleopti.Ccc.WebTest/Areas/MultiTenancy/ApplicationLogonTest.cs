using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core;
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
		public DataSourceConfigurationProviderFake DataSourceConfigurationProvider;

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

		[Test]
		public void ShouldFailIfNoDatasource()
		{
			var logonName = RandomName.Make();
			var password = RandomName.Make();
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, password);
			ApplicationUserQuery.Has(personInfo);
			DataSourceConfigurationProvider.Has(new Tenant("another tenant"), new DataSourceConfiguration());

			var result = Target.ApplicationLogon(new ApplicationLogonModel { UserName = logonName, Password = password }).Result<TenantAuthenticationResult>();

			result.Success.Should().Be.False();
			result.FailReason.Should().Be.EqualTo(Resources.NoDatasource);
		}

		[Test]
		public void ShouldSucceedIfValidCredentials()
		{
			var logonName = RandomName.Make();
			var password = RandomName.Make();
			var tenant = new Tenant(RandomName.Make());
			var personInfo = new PersonInfo(tenant, Guid.NewGuid());
			var dataSourceConfiguration = new DataSourceConfiguration();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, password);
			ApplicationUserQuery.Has(personInfo);
			DataSourceConfigurationProvider.Has(tenant, dataSourceConfiguration);

			var res = Target.ApplicationLogon(new ApplicationLogonModel { UserName = logonName, Password = password }).Result<TenantAuthenticationResult>();

			res.Success.Should().Be.True();
			res.Tenant.Should().Be.EqualTo(personInfo.Tenant.Name);
			res.PersonId.Should().Be.EqualTo(personInfo.Id);
			res.DataSourceConfiguration.Should().Be.SameInstanceAs(dataSourceConfiguration);
			res.TenantPassword.Should().Be.EqualTo(personInfo.TenantPassword);
		}
	}
}