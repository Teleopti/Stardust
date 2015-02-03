using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public class WindowsAuthenticationModel : IAuthenticationModel
	{
		private readonly IIdentityLogon _identityLogon;
		private readonly ILogLogonAttempt _logLogonAttempt;
		public string DataSourceName { get; set; }

		public WindowsAuthenticationModel(IIdentityLogon identityLogon, ILogLogonAttempt logLogonAttempt)
		{
			_identityLogon = identityLogon;
			_logLogonAttempt = logLogonAttempt;
		}

		public AuthenticateResult AuthenticateUser()
		{
			return _identityLogon.LogonWindowsUser(DataSourceName);
		}

		public void SaveAuthenticateResult(AuthenticateResult result)
		{
			_logLogonAttempt.SaveAuthenticateResult(string.Empty, result);
		}
	}
}