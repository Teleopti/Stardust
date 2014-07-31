using System.Globalization;
using System.Web.Mvc;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class ApplicationController : Controller
	{
		private readonly IPrincipalAuthorization _principalAuthorization;
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IPersonRepository _personRepository;

		public ApplicationController(IPrincipalAuthorization principalAuthorization, ICurrentTeleoptiPrincipal currentTeleoptiPrincipal, IPersonRepository personRepository)
		{
			_principalAuthorization = principalAuthorization;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_personRepository = personRepository;
		}

		public ViewResult Index()
		{
			return new ViewResult();
		}

		[UnitOfWorkAction, HttpGet, OutputCache(NoStore = true, Duration = 0)]
		public JsonResult NavigationContent()
		{
			return Json(new
			            	{
			            		UserName = _currentTeleoptiPrincipal.Current().Identity.Name,
								PersonId = _currentTeleoptiPrincipal.Current().GetPerson(_personRepository).Id,
								IsMyTimeAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb),
								IsRealTimeAdherenceAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview),
			            	}, JsonRequestBehavior.AllowGet);
		}

		[HttpGet, OutputCache(NoStore = true, Duration = 0)]
		public JsonResult Permissions()
		{
			return Json(new
			{
				IsAddFullDayAbsenceAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence),
				IsAddIntradayAbsenceAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence),
				IsRemoveAbsenceAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RemoveAbsence),
				IsAddActivityAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddActivity),
				IsMoveActivityAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MoveActivity)
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
					UserTexts.Resources.SignInAsAnotherUser,
					UserTexts.Resources.Settings,
					UserTexts.Resources.ContractTime,
					UserTexts.Resources.WorkTime,
					UserTexts.Resources.Next,
					UserTexts.Resources.Previous,
					UserTexts.Resources.LoadingThreeDots,
					UserTexts.Resources.AddFullDayAbsence,
					UserTexts.Resources.AddActivity,
					UserTexts.Resources.Apply,
					UserTexts.Resources.InvalidEndDate,
					UserTexts.Resources.Forecasted,
					UserTexts.Resources.Scheduled,
					UserTexts.Resources.Difference,
					UserTexts.Resources.ESL,
					UserTexts.Resources.HourShort,
					UserTexts.Resources.Remove,
					UserTexts.Resources.ConfirmRemoval,
					UserTexts.Resources.Show,
					UserTexts.Resources.FullDayAbsence,
					UserTexts.Resources.Activity,
					UserTexts.Resources.AddIntradayAbsence,
					UserTexts.Resources.IntradayAbsence,
					UserTexts.Resources.Absence,
					UserTexts.Resources.InvalidIntradayAbsenceTimes,
					UserTexts.Resources.InvalidEndTime,
					UserTexts.Resources.CannotCreateSecondShiftWhenAddingActivity,
					UserTexts.Resources.OutOfAdherence,
					UserTexts.Resources.RealTimeAdherence,
					UserTexts.Resources.WaitingThreeDots,
					UserTexts.Resources.InsufficientPermission,
					UserTexts.Resources.AgentName,
					UserTexts.Resources.Team,
					UserTexts.Resources.State,
					UserTexts.Resources.TimeInState,
					UserTexts.Resources.NextActivity,
					UserTexts.Resources.NextActivityStartTime,
					UserTexts.Resources.Alarm,
					UserTexts.Resources.TimeInAlarm,
					UserTexts.Resources.MoveActivity,
                    UserTexts.Resources.FunctionNotAvailable,
                    UserTexts.Resources.AddingIntradayAbsenceFor,
					UserTexts.Resources.AddingFulldayAbsenceFor,
					UserTexts.Resources.AddingActivityFor,
					UserTexts.Resources.MovingActivityFor,
					UserTexts.Resources.RemovingAbsenceFor,
					UserTexts.Resources.SuccessWithExclamation,
                    UserTexts.Resources.FailedWithExclamation,
					UserTexts.Resources.PleaseTryAgainWithExclamation,
					UserTexts.Resources.PleaseRefreshThePageWithExclamation,

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