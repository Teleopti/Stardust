using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public class ApplicationAuthenticationModel
	{
		private readonly IAuthenticator _authenticator;
		private readonly ILogLogonAttempt _logLogonAttempt;
		public string DataSourceName { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }

		public ApplicationAuthenticationModel(IAuthenticator authenticator, ILogLogonAttempt logLogonAttempt)
		{
			_authenticator = authenticator;
			_logLogonAttempt = logLogonAttempt;
		}

		public AuthenticateResult AuthenticateUser()
		{
			return _authenticator.AuthenticateApplicationUser(DataSourceName, UserName, Password);
		}

		public void SaveAuthenticateResult(AuthenticateResult result)
		{
			_logLogonAttempt.SaveAuthenticateResult(UserName, result);
		}
	}
}