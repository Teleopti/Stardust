using System.Globalization;
using System.Web.Mvc;
using Newtonsoft.Json;
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

			var userTexts = JsonConvert.SerializeObject(new
				{
					UserTexts.Resources.Home,
					UserTexts.Resources.LogOut,
					UserTexts.Resources.Settings,
					UserTexts.Resources.ContractTime,
					UserTexts.Resources.WorkTime,
					UserTexts.Resources.Next,
					UserTexts.Resources.Previous,
					UserTexts.Resources.LoadingThreeDots,
					UserTexts.Resources.AddFullDayAbsence,
					UserTexts.Resources.Apply,
					UserTexts.Resources.InvalidEndDate,
					UserTexts.Resources.Forecasted,
					UserTexts.Resources.Scheduled,
					UserTexts.Resources.Difference,
					UserTexts.Resources.ESL,
					UserTexts.Resources.HourShort,

					CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
					MomentShortDatePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToUpper(),
					ShortTimePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Replace("tt", "a"),
					LanguageCode = CultureInfo.CurrentCulture.IetfLanguageTag,
					FirstDayOfWeek = (int) CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek
				}, Formatting.Indented);

			template = string.Format(template, userTexts);

			return new ContentResult {Content = template, ContentType = "text/javascript"};
		}
	}
}