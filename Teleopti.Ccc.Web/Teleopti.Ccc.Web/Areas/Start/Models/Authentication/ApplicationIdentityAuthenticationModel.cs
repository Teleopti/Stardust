using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public class ApplicationIdentityAuthenticationModel : IAuthenticationModel
	{
		private readonly IIdentityLogon _identityLogon;
		private readonly ILogLogonAttempt _logLogonAttempt;
		public string DataSourceName { get; set; }

		public ApplicationIdentityAuthenticationModel(IIdentityLogon identityLogon, ILogLogonAttempt logLogonAttempt)
		{
			_identityLogon = identityLogon;
			_logLogonAttempt = logLogonAttempt;
		}

		public AuthenticateResult AuthenticateUser()
		{
			return _identityLogon.LogonApplicationIdentityUser(DataSourceName);
		}

		public void SaveAuthenticateResult(AuthenticateResult result)
		{
			_logLogonAttempt.SaveAuthenticateResult(string.Empty, result);
		}
	}
}