using System;
using System.Configuration;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class ReturnController : Controller
	{
		public ViewResult Hash()
		{
			return View(
				new ReturnModel
				{
					UseRelative = ConfigurationManager.AppSettings.ReadValue("UseRelativeConfiguration"),
					ApplicationArea = WebApplicationAreaResolver.GetWebApplicationArea(Request, Url.Content("~/"))
				});
		}
	}

	public class ReturnModel
	{
		public bool UseRelative { get; set; }
		public string ApplicationArea { get; set; }
	}

	public static class UriStringExtensions
	{
		private static readonly Lazy<bool> useRelativeConfiguration = new Lazy<bool>(()=>ConfigurationManager.AppSettings.ReadValue("UseRelativeConfiguration", () => false));

		public static Uri ReplaceWithRelativeUriWhenEnabled(this string uri)
		{
			var completeUri = new Uri(uri);
			if (useRelativeConfiguration.Value)
			{
				completeUri = new Uri(completeUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)).MakeRelativeUri(completeUri);
			}
			return completeUri;
		}

		public static Uri ReplaceWithCustomEndpointHostOrLocalhost(this string uri)
		{
			var completeUri = new Uri(uri);
			if (useRelativeConfiguration.Value)
			{
				completeUri = new Uri(new Uri(ConfigurationManager.AppSettings["CustomEndpointHost"] ?? "http://localhost/"),
					new Uri(completeUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped))
						.MakeRelativeUri(completeUri));
			}
			return completeUri;
		}
	}
}