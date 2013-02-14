using System;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class ApplicationController : Controller
	{
		private readonly IPrincipalAuthorization _principalAuthorization;
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		
		public ApplicationController(IPrincipalAuthorization principalAuthorization, ICurrentTeleoptiPrincipal currentTeleoptiPrincipal)
		{
			_principalAuthorization = principalAuthorization;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
		}

		public FilePathResult Index()
		{
            return new FilePathResult("~/Areas/Anywhere/Content/Templates/index.html", "text/html");
		}

		[HttpGet,OutputCache(NoStore = true,Duration = 0)]
		public JsonResult NavigationContent()
		{
			return Json(new
			            	{
			            		UserName = _currentTeleoptiPrincipal.Current().Identity.Name,
								IsMyTimeAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb),
								IsMobileReportsAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MobileReports),
			            	}, JsonRequestBehavior.AllowGet);
		}

		[HttpGet,OutputCache(Duration = 0,NoStore = true)]
		public ActionResult Resources()
		{
            var path = Request.MapPath("~/Areas/Anywhere/Content/Translation/TranslationTemplate.txt");
			var template = System.IO.File.ReadAllText(path);
			const string itemFormat = "{0}: '{1}'";

			var userTexts = new[]
			                	{
			                		string.Format(itemFormat, "Home",UserTexts.Resources.Home),
			                		string.Format(itemFormat, "LogOut",UserTexts.Resources.LogOut),
			                		string.Format(itemFormat, "Settings", UserTexts.Resources.Settings),
			                		string.Format(itemFormat, "ContractTime",UserTexts.Resources.ContractTime),
			                		string.Format(itemFormat, "WorkTime",UserTexts.Resources.WorkTime),
			                		string.Format(itemFormat, "Next",UserTexts.Resources.Next),
			                		string.Format(itemFormat, "Previous",UserTexts.Resources.Previous),
			                		string.Format(itemFormat, "LoadingThreeDots",UserTexts.Resources.LoadingThreeDots),

									string.Format(itemFormat, "ShortDatePattern", CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern),
									string.Format(itemFormat, "ShortTimePattern", CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Replace("tt","a")),
									string.Format(itemFormat, "LanguageCode", CultureInfo.CurrentCulture.IetfLanguageTag),
									string.Format(itemFormat, "FirstDayOfWeek", (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek),
			                	};

			template = string.Format(template, string.Join("," + Environment.NewLine, userTexts));
			
			return new ContentResult {Content = template, ContentType = "text/javascript"};
		}
	}
}