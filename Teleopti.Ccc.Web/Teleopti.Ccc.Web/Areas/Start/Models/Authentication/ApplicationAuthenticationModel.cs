using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public class ApplicationAuthenticationModel : IAuthenticationModel
	{
		private readonly IAuthenticator _authenticator;
		public string DataSourceName { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }

		public ApplicationAuthenticationModel(IAuthenticator authenticator)
		{
			_authenticator = authenticator;
		}

		public AuthenticateResult AuthenticateUser()
		{
			return _authenticator.AuthenticateApplicationUser(DataSourceName, UserName, Password);
		}
	}
}