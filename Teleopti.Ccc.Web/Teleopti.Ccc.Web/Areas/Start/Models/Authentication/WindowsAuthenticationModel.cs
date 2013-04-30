using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public class WindowsAuthenticationModel : IAuthenticationModel
	{
		private readonly IAuthenticator _authenticator;
		public string DataSourceName { get; set; }

		public WindowsAuthenticationModel(IAuthenticator authenticator)
		{
			_authenticator = authenticator;
		}

		public AuthenticateResult AuthenticateUser()
		{
			return _authenticator.AuthenticateWindowsUser(DataSourceName);
		}

		public void SaveAuthenticateResult(AuthenticateResult result)
		{
			_authenticator.SaveAuthenticateResult("",result);
		}
	}
}