using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public interface IRedirector
	{
		RedirectResult SignInRedirect();
		RedirectResult SignOutRedirect(string returnUrl);
	}
}