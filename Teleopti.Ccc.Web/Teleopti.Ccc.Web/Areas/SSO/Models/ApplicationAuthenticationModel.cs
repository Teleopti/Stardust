using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.SSO.Models
{
	public class ApplicationAuthenticationModel
	{
		private readonly ISsoAuthenticator _authenticator;
		private readonly ILogLogonAttempt _logLogonAttempt;
		public string UserName { get; set; }
		public string Password { get; set; }

		public ApplicationAuthenticationModel(ISsoAuthenticator authenticator, ILogLogonAttempt logLogonAttempt)
		{
			_authenticator = authenticator;
			_logLogonAttempt = logLogonAttempt;
		}

		public AuthenticateResult AuthenticateUser()
		{
			return _authenticator.AuthenticateApplicationUser(UserName, Password);
		}

		public void SaveAuthenticateResult(AuthenticateResult result)
		{
			_logLogonAttempt.SaveAuthenticateResult(UserName, result);
		}
	}
}