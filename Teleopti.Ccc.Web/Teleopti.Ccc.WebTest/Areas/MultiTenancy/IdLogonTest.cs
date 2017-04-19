using System;
using System.Linq;
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
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	[TenantTest]
	public class IdLogonTest
	{
		public TenantAuthenticationFake TenantAuthentication;
		public AuthenticateController Target;
		public IdUserQueryFake IdentityUserQuery;
		public TenantUnitOfWorkFake TenantUnitOfWork;
		public LogLogonAttemptFake LogLogonAttempt;

		[Test]
		public void ShouldAcceptAccessWithoutTenantCredentials()
		{
			TenantAuthentication.NoAccess();

			Assert.DoesNotThrow(() =>
				Target.IdLogon(new IdLogonModel{Id = Guid.NewGuid() }));
		}

		[Test]
		public void NonExistingUserShouldFail()
		{
			var id = Guid.NewGuid();
			var result = Target.IdLogon(new IdLogonModel{Id = id}).Result<TenantAuthenticationResult>();
			result.Success.Should().Be.False();
			result.FailReason.Should().Be.EqualTo(string.Format(Resources.LogOnFailedIdentityNotFound, id));
		}

		[Test]
		public void ShouldSucceedIfValidCredentials()
		{
			var tenant = new Tenant(RandomName.Make());
			tenant.DataSourceConfiguration.SetAnalyticsConnectionString(string.Format("Initial Catalog={0}", RandomName.Make()));
			tenant.DataSourceConfiguration.SetApplicationConnectionString(string.Format("Initial Catalog={0}", RandomName.Make()));
			var encryptedDataSourceConfiguration = new DataSourceConfigurationEncryption().EncryptConfig(tenant.DataSourceConfiguration);
			var personInfo = new PersonInfo(tenant, Guid.NewGuid());
			IdentityUserQuery.Has(personInfo);

			var result = Target.IdLogon(new IdLogonModel { Id = personInfo.Id }).Result<TenantAuthenticationResult>();

			result.Success.Should().Be.True();
			result.Tenant.Should().Be.EqualTo(personInfo.Tenant.Name);
			result.PersonId.Should().Be.EqualTo(personInfo.Id);
			result.DataSourceConfiguration.AnalyticsConnectionString.Should().Be.EqualTo(encryptedDataSourceConfiguration.AnalyticsConnectionString);
			result.DataSourceConfiguration.ApplicationConnectionString.Should().Be.EqualTo(encryptedDataSourceConfiguration.ApplicationConnectionString);
			result.DataSourceConfiguration.ApplicationConfig.Single().Value
				.Should().Be.EqualTo(encryptedDataSourceConfiguration.ApplicationConfig[tenant.DataSourceConfiguration.ApplicationConfig.Single().Key]);

			result.TenantPassword.Should().Be.EqualTo(personInfo.TenantPassword);
		}
		
	}
}