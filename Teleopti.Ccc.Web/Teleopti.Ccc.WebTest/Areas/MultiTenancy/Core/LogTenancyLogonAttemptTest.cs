using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class LogTenancyLogonAttemptTest
	{
		[Test]
		public void ShouldLogWhenPersonIsNull()
		{
			var loginAttemptModelFactory = MockRepository.GenerateMock<ILoginAttemptModelFactory>();
			var persister = MockRepository.GenerateMock<IPersistLogonAttempt>();
			const string userName = "username";
			var loginAttemptModel = new LoginAttemptModel();
			loginAttemptModelFactory.Expect(x => x.Create(userName, null, false)).Return(loginAttemptModel);

			var target = new LogTenancyLogonAttempt(loginAttemptModelFactory, persister);
			target.SaveAuthenticateResult(userName, null, false);

			persister.AssertWasCalled(x=> x.SaveLoginAttempt(loginAttemptModel));
		}

		[Test]
		public void ShouldLogWhenPersonExists()
		{
			var loginAttemptModelFactory = MockRepository.GenerateMock<ILoginAttemptModelFactory>();
			var persister = MockRepository.GenerateMock<IPersistLogonAttempt>();
			const string userName = "username";
			var personId = Guid.NewGuid();
			var loginAttemptModel = new LoginAttemptModel();
			loginAttemptModelFactory.Expect(x => x.Create(userName, personId, true)).Return(loginAttemptModel);

			var target = new LogTenancyLogonAttempt(loginAttemptModelFactory, persister);
			target.SaveAuthenticateResult(userName, personId, true);

			persister.AssertWasCalled(x => x.SaveLoginAttempt(loginAttemptModel));
		}
	}
}