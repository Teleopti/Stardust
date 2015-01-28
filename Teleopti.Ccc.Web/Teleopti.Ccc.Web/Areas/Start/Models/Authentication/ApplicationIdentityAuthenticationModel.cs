using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public class ApplicationIdentityAuthenticationModel : IAuthenticationModel
	{
		private readonly IAuthenticator _authenticator;
		private readonly ILogLogonAttempt _logLogonAttempt;
		public string DataSourceName { get; set; }

		public ApplicationIdentityAuthenticationModel(IAuthenticator authenticator, ILogLogonAttempt logLogonAttempt)
		{
			_authenticator = authenticator;
			_logLogonAttempt = logLogonAttempt;
		}

		public AuthenticateResult AuthenticateUser()
		{
			return _authenticator.AuthenticateApplicationIdentityUser(DataSourceName);
		}

		public void SaveAuthenticateResult(AuthenticateResult result)
		{
			_logLogonAttempt.SaveAuthenticateResult(string.Empty, result);
		}
	}
}