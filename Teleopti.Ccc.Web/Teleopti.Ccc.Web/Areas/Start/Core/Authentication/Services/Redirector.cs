namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	using System.Web.Mvc;

	public class Redirector : IRedirector
	{
		private readonly UrlHelper _helper;

		public Redirector(UrlHelper helper)
		{
			_helper = helper;
		}

		#region IRedirector Members

		public RedirectResult SignInRedirect()
		{
			var originArea = _helper.RequestContext.RouteData.Values["origin"];
			var routeUrl = originArea != null
			               	? _helper.RouteUrl(new { action = string.Empty, controller = string.Empty, area = originArea })
			               	: _helper.RouteUrl(new { action = string.Empty, controller = string.Empty });

			return new RedirectResult(routeUrl, false);
		}

		public RedirectResult SignOutRedirect(string returnUrl)
		{
			if (SafeLocalUrl(returnUrl))
			{
				return new RedirectResult(returnUrl, false);
			}

			var routeUrl = _helper.RouteUrl(new { controller = string.Empty, action = string.Empty });
			return new RedirectResult(routeUrl, false);
		}

		#endregion

		private bool SafeLocalUrl(string returnUrl)
		{
			return !string.IsNullOrEmpty(returnUrl) && _helper.IsLocalUrl(returnUrl) && returnUrl.StartsWith("/") &&
			       !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\");
		}
	}
}