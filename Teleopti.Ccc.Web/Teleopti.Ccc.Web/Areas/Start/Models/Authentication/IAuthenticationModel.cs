using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public interface IAuthenticationModel
	{
		AuthenticateResult AuthenticateUser();
		void SaveAuthenticateResult(AuthenticateResult result);
	}
}