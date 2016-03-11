using System;
using System.Text.RegularExpressions;
using System.Web;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class WebApplicationAreaResolver
	{
		
		public static string GetWebApplicationArea(HttpRequestBase request, String applicationAbsolutePath)
		{
			var returnUrl = getReturnUrlFromQueryString(request);
			return String.IsNullOrEmpty (returnUrl) ? null : stripApplicationRelativeUrlFromReturnUrl(returnUrl, applicationAbsolutePath);
		}

		private static string getReturnUrlFromQueryString(HttpRequestBase request)
		{
			var redirectUrl = request.QueryString.Get("redirectUrl");
			if (String.IsNullOrEmpty(redirectUrl))
			{
				return null;
			}

			var parsedQueryString = HttpUtility.ParseQueryString(redirectUrl);
			var wctx = parsedQueryString.Get("wctx");

			if (String.IsNullOrEmpty(wctx))
			{
				return null;
			}

			var wctxParsedQueryString = HttpUtility.ParseQueryString(wctx);
			var returnUrl = wctxParsedQueryString.Get("ru");

			return returnUrl;
		}

		private static string stripApplicationRelativeUrlFromReturnUrl(string returnUrl, string applicationAbsolutePath)
		{
			if (!String.IsNullOrEmpty(applicationAbsolutePath))
			{
				var regex = new Regex(Regex.Escape(applicationAbsolutePath));
				returnUrl = regex.Replace(returnUrl, "", 1);

			}

			return returnUrl;
		}
	}
}