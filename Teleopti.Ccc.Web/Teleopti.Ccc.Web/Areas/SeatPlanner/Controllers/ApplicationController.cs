using System.Globalization;
using System.Web.Mvc;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
//using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Utilities;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;


namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers
{
    public class ApplicationController : Controller
    {

		private readonly IPrincipalAuthorization _principalAuthorization;
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IPersonRepository _personRepository;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;

		public ApplicationController(IPrincipalAuthorization principalAuthorization, ICurrentTeleoptiPrincipal currentTeleoptiPrincipal, IPersonRepository personRepository, IIanaTimeZoneProvider ianaTimeZoneProvider)
		{
			_principalAuthorization = principalAuthorization;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_personRepository = personRepository;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
		}

        // GET: SeatPlanner/Application
        public ActionResult Index()
        {
            return View();
        }

		[UnitOfWorkAction, HttpGet, OutputCache(NoStore = true, Duration = 0)]
		public JsonResult NavigationContent()
		{
			var principal = _currentTeleoptiPrincipal.Current();
			return Json(new
			{
				UserName = principal.Identity.Name,
				PersonId = ((IUnsafePerson)principal).Person.Id,
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpGet, OutputCache(NoStore = true, Duration = 0)]
		public JsonResult Permissions()
		{
			return Json(new
			{
				//IsAddFullDayAbsenceAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence),
				
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpGet,OutputCache(Duration = 0,NoStore = true)]
		public ActionResult Resources()
		{
            var path = Request.MapPath("~/Areas/SeatPlanner/Content/Translation/TranslationTemplate.txt");
			var template = System.IO.File.ReadAllText(path);

			var userTexts = JsonConvert.SerializeObject(new
				{
					UserTexts.Resources.Home,
					UserTexts.Resources.SignOut,
					UserTexts.Resources.Settings,
					UserTexts.Resources.Next,
					UserTexts.Resources.Previous,
					UserTexts.Resources.LoadingThreeDots,
					UserTexts.Resources.Apply,
					UserTexts.Resources.Cancel,
					UserTexts.Resources.InvalidEndDate,
					UserTexts.Resources.Remove,
					UserTexts.Resources.ConfirmRemoval,
					UserTexts.Resources.Show,
					UserTexts.Resources.WaitingThreeDots,
					UserTexts.Resources.FunctionNotAvailable,
					UserTexts.Resources.SuccessWithExclamation,
					UserTexts.Resources.FailedWithExclamation,
					UserTexts.Resources.PleaseTryAgainWithExclamation,
					UserTexts.Resources.PleaseRefreshThePageWithExclamation,
					UserTexts.Resources.ServerUnavailable,
					UserTexts.Resources.StartDateMustBeSmallerThanEndDate,
					UserTexts.Resources.StartDateMustBeGreaterThanToday,
					UserTexts.Resources.Start,
					UserTexts.Resources.Send,
					UserTexts.Resources.Save,
					UserTexts.Resources.DateFromGreaterThanDateTo,

					DateAndTimeFormatExtensions.FixedDateFormat,
					DateAndTimeFormatExtensions.FixedDateTimeFormat,
					DateAndTimeFormatExtensions.FixedTimeFormat,
					DateAndTimeFormatExtensions.FixedDateFormatForMoment,
					DateAndTimeFormatExtensions.FixedDateTimeFormatForMoment,
					DateAndTimeFormatExtensions.FixedDateTimeWithSecondsFormatForMoment,
					DateAndTimeFormatExtensions.FixedTimeFormatForMoment,

					DateFormat = DateAndTimeFormatExtensions.DateFormat(),
					DateTimeFormat = DateAndTimeFormatExtensions.DateTimeFormat(),
					TimeFormat = DateAndTimeFormatExtensions.TimeFormat(),

					DateFormatForMoment = DateAndTimeFormatExtensions.DateFormatForMoment(),
					DateTimeFormatForMoment = DateAndTimeFormatExtensions.DateTimeFormatForMoment(),
					TimeFormatForMoment = DateAndTimeFormatExtensions.TimeFormatForMoment(),

					LanguageCode = CultureInfo.CurrentCulture.IetfLanguageTag,
					FirstDayOfWeek = (int) CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek,
					ShowMeridian = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains("t")
				}, Formatting.Indented);

			template = string.Format(template, userTexts);

			return new ContentResult {Content = template, ContentType = "text/javascript"};
		}


    }


}

