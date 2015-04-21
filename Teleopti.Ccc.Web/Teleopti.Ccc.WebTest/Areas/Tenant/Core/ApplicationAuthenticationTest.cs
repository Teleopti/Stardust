using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class ApplicationAuthenticationTest
	{
		[Test]
		public void NonExistingUserShouldFail()
		{
			var target = new ApplicationAuthentication(MockRepository.GenerateMock<IApplicationUserTenantQuery>(),
				new SuccessfulPasswordPolicy(), MockRepository.GenerateMock<IDataSourceConfigurationProvider>(), () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now());
			var res = target.Logon("nonExisting", string.Empty);

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void UserWithNonExistingLogonNameShouldFail()
		{
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserTenantQuery>();
			var personInfo = new PersonInfo();
			findApplicationQuery.Expect(x => x.Find(RandomName.Make())).Return(personInfo);
			var target = new ApplicationAuthentication(findApplicationQuery,
				new SuccessfulPasswordPolicy(), MockRepository.GenerateMock<IDataSourceConfigurationProvider>(), () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now());

			var res = target.Logon("nonExisting", string.Empty);

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void IncorrectPasswordShouldFail()
		{
			const string userName = "validUserName";

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserTenantQuery>();
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(RandomName.Make(), "thePassword");
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);

			var target = new ApplicationAuthentication(findApplicationQuery,
				new SuccessfulPasswordPolicy(), MockRepository.GenerateMock<IDataSourceConfigurationProvider>(), () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now());
			var res = target.Logon(userName, "invalidPassword");

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void ShouldSucceedIfValidCredentials()
		{
			const string userName = "validUserName";
			const string password = "somePassword";
			var dataSourceConfiguration = new DataSourceConfiguration();
			var personInfo = new PersonInfo { Id = Guid.NewGuid()};
			personInfo.SetApplicationLogonCredentials(RandomName.Make(), password);
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserTenantQuery>();
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);
			var dataSourceProvider = MockRepository.GenerateStub<IDataSourceConfigurationProvider>();
			dataSourceProvider.Stub(x => x.ForTenant(personInfo.Tenant)).Return(dataSourceConfiguration);
			var target = new ApplicationAuthentication(findApplicationQuery,
				new SuccessfulPasswordPolicy(), dataSourceProvider, () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now());
			
			var res = target.Logon(userName, password);

			res.Success.Should().Be.True();
			res.Tenant.Should().Be.EqualTo(personInfo.Tenant);
			res.PersonId.Should().Be.EqualTo(personInfo.Id);
			res.DataSourceConfiguration.Should().Be.SameInstanceAs(dataSourceConfiguration);
		}

		[Test]
		public void ShouldFailIfNoDatasource()
		{
			const string userName = "validUserName";
			const string password = "somePassword";
			var personInfo = new PersonInfo { Id = Guid.NewGuid() };
			personInfo.SetApplicationLogonCredentials(RandomName.Make(), password);

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserTenantQuery>();
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);
			var nhibHandler = MockRepository.GenerateMock<IDataSourceConfigurationProvider>();
			nhibHandler.Stub(x => x.ForTenant(personInfo.Tenant)).Return(null);
			var target = new ApplicationAuthentication(findApplicationQuery,
				new SuccessfulPasswordPolicy(), nhibHandler, () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now());

			var res = target.Logon(userName, password);

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.NoDatasource);
		}
	}
}