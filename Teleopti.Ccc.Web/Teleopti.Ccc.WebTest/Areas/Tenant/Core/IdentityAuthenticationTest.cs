using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Tenant.Core;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class IdentityAuthenticationTest
	{
		[Test]
		public void NonExistingUserShouldFail()
		{
			var identityUserQuery = MockRepository.GenerateMock<IIdentityUserQuery>();
			identityUserQuery.Stub(x => x.FindUserData("nonExisting")).Return(null);
			var target = new IdentityAuthentication(identityUserQuery, MockRepository.GenerateMock<INHibernateConfigurationsHandler>());
			var res = target.Logon("nonExisting");
			
			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(string.Format(Resources.LogOnFailedIdentityNotFound, "nonExisting"));
		}

		
		[Test]
		public void ShouldSucceedIfValidCredentials()
		{
			const string identity = "validUser";

			var queryResult = new PersonInfo {Id = Guid.NewGuid()};
			var findIdentityQuery = MockRepository.GenerateMock<IIdentityUserQuery>();
			var nhibHandler = MockRepository.GenerateMock<INHibernateConfigurationsHandler>();
			nhibHandler.Stub(x => x.GetConfigForName(queryResult.Tennant)).Return("aencryptedconfig"); 
			findIdentityQuery.Expect(x => x.FindUserData(identity)).Return(queryResult);

			var target = new IdentityAuthentication(findIdentityQuery,nhibHandler);
			var res = target.Logon(identity);

			res.Success.Should().Be.True();
			res.Tennant.Should().Be.EqualTo(queryResult.Tennant);
			res.PersonId.Should().Be.EqualTo(queryResult.Id);
		}

		[Test]
		public void ShouldFailIfNoDatasource()
		{
			const string identity = "validUser";

			var queryResult = new PersonInfo { Id = Guid.NewGuid() };
			var findIdentityQuery = MockRepository.GenerateMock<IIdentityUserQuery>();
			var nhibHandler = MockRepository.GenerateMock<INHibernateConfigurationsHandler>();
			nhibHandler.Stub(x => x.GetConfigForName(queryResult.Tennant)).Return("");
			findIdentityQuery.Expect(x => x.FindUserData(identity)).Return(queryResult);

			var target = new IdentityAuthentication(findIdentityQuery, nhibHandler);
			var res = target.Logon(identity);
			
			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.NoDatasource);
		}
	}
}