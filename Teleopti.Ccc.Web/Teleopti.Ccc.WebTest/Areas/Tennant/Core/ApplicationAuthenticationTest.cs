using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Tennant.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Tennant.Core
{
	public class ApplicationAuthenticationTest
	{
		[Test]
		public void NonExistingUserShouldFail()
		{
			var target = new ApplicationAuthentication(MockRepository.GenerateMock<IApplicationUserQuery>(),
				new PasswordVerifier(new OneWayEncryption(), () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now()), new SuccessfulPasswordPolicy(), MockRepository.GenerateMock<INHibernateConfigurationsHandler>());
			var res = target.Logon("nonExisting", string.Empty);

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void IncorrectPasswordShouldFail()
		{
			const string userName = "validUserName";

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var personInfo = new PersonInfo {Password = "thePassword"};
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(new PasswordPolicyForUser(personInfo));

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now()),
				new SuccessfulPasswordPolicy(), MockRepository.GenerateMock<INHibernateConfigurationsHandler>());
			var res = target.Logon(userName, "invalidPassword");

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void ShouldSucceedIfValidCredentials()
		{
			const string userName = "validUserName";
			const string password = "somePassword";
			var personInfo = new PersonInfo {Password = EncryptPassword.ToDbFormat(password), Id = Guid.NewGuid()};
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(new PasswordPolicyForUser(personInfo));
			var nhibHandler = MockRepository.GenerateMock<INHibernateConfigurationsHandler>();
			nhibHandler.Stub(x => x.GetConfigForName(personInfo.Tennant)).Return("aencryptedconfig");
			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now()),
				new SuccessfulPasswordPolicy(), nhibHandler);
			
			var res = target.Logon(userName, password);

			res.Success.Should().Be.True();
			res.Tennant.Should().Be.EqualTo(personInfo.Tennant);
			res.PersonId.Should().Be.EqualTo(personInfo.Id);
			res.DataSourceEncrypted.Should().Be.EqualTo("aencryptedconfig");
		}

		[Test]
		public void ShouldFailIfNoDatasource()
		{
			const string userName = "validUserName";
			const string password = "somePassword";
			var personInfo = new PersonInfo { Password = EncryptPassword.ToDbFormat(password), Id = Guid.NewGuid() };
			var queryResult = new PasswordPolicyForUser(personInfo);
			
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(queryResult);
			var nhibHandler = MockRepository.GenerateMock<INHibernateConfigurationsHandler>();
			nhibHandler.Stub(x => x.GetConfigForName(queryResult.PersonInfo.Tennant)).Return("");
			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now()),
				new SuccessfulPasswordPolicy(), nhibHandler);

			var res = target.Logon(userName, password);

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.NoDatasource);
		}
	}
}