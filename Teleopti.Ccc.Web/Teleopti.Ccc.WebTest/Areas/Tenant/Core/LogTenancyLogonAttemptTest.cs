using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Tenant.Core;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class LogTenancyLogonAttemptTest
	{
		[Test]
		public void ShouldLogWhenPersonIsNull()
		{
			var loginAttemptModelFactory = MockRepository.GenerateMock<ILoginAttemptModelFactory>();
			var persister = MockRepository.GenerateMock<IPersistLogonAttempt>();
			const string userName = "username";
			var authResult = new AuthenticateResult{Person=null};
			var loginAttemptModel = new LoginAttemptModel();
			loginAttemptModelFactory.Expect(x => x.Create(userName, null, authResult.Successful)).Return(loginAttemptModel);

			var target = new LogTenancyLogonAttempt(loginAttemptModelFactory, persister);
			target.SaveAuthenticateResult(userName, authResult);

			persister.AssertWasCalled(x=> x.SaveLoginAttempt(loginAttemptModel));
		}

		[Test]
		public void ShouldLogWhenPersonExists()
		{
			var loginAttemptModelFactory = MockRepository.GenerateMock<ILoginAttemptModelFactory>();
			var persister = MockRepository.GenerateMock<IPersistLogonAttempt>();
			const string userName = "username";
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var authResult = new AuthenticateResult { Person = person };
			var loginAttemptModel = new LoginAttemptModel();
			loginAttemptModelFactory.Expect(x => x.Create(userName, person.Id.Value, authResult.Successful)).Return(loginAttemptModel);

			var target = new LogTenancyLogonAttempt(loginAttemptModelFactory, persister);
			target.SaveAuthenticateResult(userName, authResult);

			persister.AssertWasCalled(x => x.SaveLoginAttempt(loginAttemptModel));
		}
	}
}