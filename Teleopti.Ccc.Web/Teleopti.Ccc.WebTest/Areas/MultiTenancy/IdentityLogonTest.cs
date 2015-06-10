using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	[TenantTest]
	public class IdentityLogonTest
	{
		public TenantAuthenticationFake TenantAuthentication;
		public AuthenticateController Target;
		public IdentityUserQueryFake IdentityUserQuery;
		public DataSourceConfigurationProviderFake DataSourceConfigurationProvider;
		public TenantUnitOfWorkFake TenantUnitOfWork;
		public LogLogonAttemptFake LogLogonAttempt;

		[Test]
		public void ShouldAcceptAccessWithoutTenantCredentials()
		{
			TenantAuthentication.NoAccess();

			Assert.DoesNotThrow(() =>
				Target.IdentityLogon(new IdentityLogonModel{Identity = RandomName.Make() }));
		}

		[Test]
		public void NonExistingUserShouldFail()
		{
			var identity = RandomName.Make();
			var result = Target.IdentityLogon(new IdentityLogonModel{Identity = identity}).Result<TenantAuthenticationResult>();
			result.Success.Should().Be.False();
			result.FailReason.Should().Be.EqualTo(string.Format(Resources.LogOnFailedIdentityNotFound, identity));
		}

		[Test]
		public void ShouldSucceedIfValidCredentials()
		{
			var identity = RandomName.Make();
			var datasourceConfiguration = new DataSourceConfiguration();
			var personInfo = new PersonInfo(new Tenant(RandomName.Make()), Guid.NewGuid());
			personInfo.SetIdentity(identity);
			IdentityUserQuery.Has(personInfo);
			DataSourceConfigurationProvider.Has(personInfo.Tenant, datasourceConfiguration);

			var result = Target.IdentityLogon(new IdentityLogonModel { Identity = identity }).Result<TenantAuthenticationResult>();

			result.Success.Should().Be.True();
			result.Tenant.Should().Be.EqualTo(personInfo.Tenant.Name);
			result.PersonId.Should().Be.EqualTo(personInfo.Id);
			result.DataSourceConfiguration.Should().Be.SameInstanceAs(datasourceConfiguration);
			result.TenantPassword.Should().Be.EqualTo(personInfo.TenantPassword);
		}

		[Test]
		public void ShouldFailIfNoDatasource()
		{
			var identity = RandomName.Make();
			var personInfo = new PersonInfo();
			personInfo.SetIdentity(identity);
			IdentityUserQuery.Has(personInfo);
			DataSourceConfigurationProvider.Has(new Tenant("another tenant"), new DataSourceConfiguration());

			var result = Target.IdentityLogon(new IdentityLogonModel { Identity = identity }).Result<TenantAuthenticationResult>();

			result.Success.Should().Be.False();
			result.FailReason.Should().Be.EqualTo(Resources.NoDatasource);
		}

		[Test]
		public void ShouldLogIdentityLogonSuccessful()
		{
			var identity = RandomName.Make();
			var datasourceConfiguration = new DataSourceConfiguration();
			var personInfo = new PersonInfo(new Tenant(RandomName.Make()), Guid.NewGuid());
			personInfo.SetIdentity(identity);
			IdentityUserQuery.Has(personInfo);
			DataSourceConfigurationProvider.Has(personInfo.Tenant, datasourceConfiguration);

			Target.IdentityLogon(new IdentityLogonModel { Identity = identity }).Result<TenantAuthenticationResult>();

			LogLogonAttempt.PersonId.Should().Be.EqualTo(personInfo.Id);
			LogLogonAttempt.Successful.Should().Be.True();
			LogLogonAttempt.UserName.Should().Be.EqualTo(identity);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		[Test]
		public void ShouldLogIdentityLogonUnsuccessful()
		{
			var identity = RandomName.Make();

			Target.IdentityLogon(new IdentityLogonModel { Identity = identity }).Result<TenantAuthenticationResult>();

			LogLogonAttempt.PersonId.Should().Be.EqualTo(Guid.Empty);
			LogLogonAttempt.Successful.Should().Be.False();
			LogLogonAttempt.UserName.Should().Be.EqualTo(identity);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}
	}
}