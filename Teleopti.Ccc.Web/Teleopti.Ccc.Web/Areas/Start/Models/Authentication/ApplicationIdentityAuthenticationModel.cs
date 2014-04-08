using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public class ApplicationIdentityAuthenticationModel : IAuthenticationModel
	{
		private readonly IAuthenticator _authenticator;
		public string DataSourceName { get; set; }

		public ApplicationIdentityAuthenticationModel(IAuthenticator authenticator)
		{
			_authenticator = authenticator;
		}

		public AuthenticateResult AuthenticateUser()
		{
			return _authenticator.AuthenticateApplicationIdentityUser(DataSourceName);
		}

		public void SaveAuthenticateResult(AuthenticateResult result)
		{
			_authenticator.SaveAuthenticateResult("", result);
		}
	}
}