using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class IdentityAuthenticationTest
	{
		[Test]
		public void NonExistingUserShouldFail()
		{
			var identityUserQuery = MockRepository.GenerateMock<IIdentityUserQuery>();
			
			identityUserQuery.Stub(x => x.FindUserData("nonExisting")).Return(null);
			var target = new IdentityAuthentication(identityUserQuery, MockRepository.GenerateMock<IDataSourceConfigurationProvider>(), null);
			var res = target.Logon("nonExisting");
			
			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(string.Format(Resources.LogOnFailedIdentityNotFound, "nonExisting"));
		}

		
		[Test]
		public void ShouldSucceedIfValidCredentials()
		{
			const string identity = "validUser";
			var datasourceConfiguration = new DataSourceConfiguration();

			var queryResult = new PersonInfo {Id = Guid.NewGuid()};
			var findIdentityQuery = MockRepository.GenerateMock<IIdentityUserQuery>();
			var nhibHandler = MockRepository.GenerateMock<IDataSourceConfigurationProvider>();
			var pwPolicyLoader = MockRepository.GenerateMock<ILoadPasswordPolicyService>();
			nhibHandler.Stub(x => x.ForTenant(queryResult.Tenant)).Return(datasourceConfiguration); 
			findIdentityQuery.Expect(x => x.FindUserData(identity)).Return(queryResult);
			pwPolicyLoader.Expect(x => x.DocumentAsString).Return("somepolicy");
			var target = new IdentityAuthentication(findIdentityQuery, nhibHandler, pwPolicyLoader);
			var res = target.Logon(identity);

			res.Success.Should().Be.True();
			res.Tenant.Should().Be.EqualTo(queryResult.Tenant);
			res.PersonId.Should().Be.EqualTo(queryResult.Id);
			res.DataSourceConfiguration.Should().Be.SameInstanceAs(datasourceConfiguration);
			res.PasswordPolicy.Should().Be.EqualTo("somepolicy");
		}

		[Test]
		public void ShouldFailIfNoDatasource()
		{
			const string identity = "validUser";

			var queryResult = new PersonInfo { Id = Guid.NewGuid() };
			var findIdentityQuery = MockRepository.GenerateMock<IIdentityUserQuery>();
			var nhibHandler = MockRepository.GenerateMock<IDataSourceConfigurationProvider>();
			nhibHandler.Stub(x => x.ForTenant(queryResult.Tenant)).Return(null);
			findIdentityQuery.Expect(x => x.FindUserData(identity)).Return(queryResult);

			var target = new IdentityAuthentication(findIdentityQuery, nhibHandler, null);
			var res = target.Logon(identity);
			
			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.NoDatasource);
		}
	}
}