﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class ApplicationAuthenticationTest
	{
		[Test]
		public void NonExistingUserShouldFail()
		{
			var target = new ApplicationAuthentication(MockRepository.GenerateMock<IApplicationUserQuery>(),
				MockRepository.GenerateMock<IDataSourceConfigurationProvider>(), () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now(), new SuccessfulPasswordPolicy());
			var res = target.Logon("nonExisting", string.Empty);

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}


		[Test]
		public void UserWithNonExistingLogonNameShouldFail()
		{
			var userName = RandomName.Make();
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var personInfo = new PersonInfo();
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);
			var target = new ApplicationAuthentication(findApplicationQuery,
				MockRepository.GenerateMock<IDataSourceConfigurationProvider>(), () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now(), new SuccessfulPasswordPolicy());

			var res = target.Logon(userName, RandomName.Make());

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void IncorrectPasswordShouldFail()
		{
			const string userName = "validUserName";

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), "thePassword");
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);

			var target = new ApplicationAuthentication(findApplicationQuery,
				MockRepository.GenerateMock<IDataSourceConfigurationProvider>(), () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now(), new SuccessfulPasswordPolicy());
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
			var personInfo = new PersonInfo(new Tenant(RandomName.Make()), Guid.NewGuid());
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), password);
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);
			var dataSourceProvider = MockRepository.GenerateStub<IDataSourceConfigurationProvider>();
			dataSourceProvider.Stub(x => x.ForTenant(personInfo.Tenant)).Return(dataSourceConfiguration);
			var target = new ApplicationAuthentication(findApplicationQuery,
				dataSourceProvider, () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now(), new SuccessfulPasswordPolicy());
			
			var res = target.Logon(userName, password);

			res.Success.Should().Be.True();
			res.Tenant.Should().Be.EqualTo(personInfo.Tenant);
			res.PersonId.Should().Be.EqualTo(personInfo.Id);
			res.DataSourceConfiguration.Should().Be.SameInstanceAs(dataSourceConfiguration);
			res.TenantPassword.Should().Be.EqualTo(personInfo.TenantPassword);
		}

		[Test]
		public void ShouldFailIfNoDatasource()
		{
			const string userName = "validUserName";
			const string password = "somePassword";
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), password);

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);
			var nhibHandler = MockRepository.GenerateMock<IDataSourceConfigurationProvider>();
			nhibHandler.Stub(x => x.ForTenant(personInfo.Tenant)).Return(null);
			var target = new ApplicationAuthentication(findApplicationQuery,
				nhibHandler, () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now(), new SuccessfulPasswordPolicy());

			var res = target.Logon(userName, password);

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.NoDatasource);
		}
	}
}