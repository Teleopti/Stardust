using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
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

		public void SaveAuthenticateResult(string userName, AuthenticateResult result)
		{
			var personId = result.Person == null ? null : result.Person.Id;
			var model = _loginAttemptModelFactory.Create(userName, personId, result.Successful);

			_persister.SaveLoginAttempt(model);
		}
	}
}