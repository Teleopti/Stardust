using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class LogTenancyLogonAttempt : ILogLogonAttempt
	{
		private readonly ILoginAttemptModelFactory _loginAttemptModelFactory;
		private readonly IPersistLogonAttempt _persister;

		public LogTenancyLogonAttempt(ILoginAttemptModelFactory loginAttemptModelFactory, IPersistLogonAttempt persister)
		{
			_loginAttemptModelFactory = loginAttemptModelFactory;
			_persister = persister;
		}

		public void SaveAuthenticateResult(string userName, Guid? personId, bool successful)
		{
			_persister.SaveLoginAttempt(_loginAttemptModelFactory.Create(userName, personId, successful));
		}
	}
}