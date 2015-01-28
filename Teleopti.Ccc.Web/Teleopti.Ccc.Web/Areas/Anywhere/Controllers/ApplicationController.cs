using System;
using System.Dynamic;
using System.Globalization;
using System.Web.Mvc;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class ApplicationController : Controller
	{
		private readonly IPrincipalAuthorization _principalAuthorization;
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IPersonRepository _personRepository;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly IToggleManager _toggles;

		public ApplicationController(IPrincipalAuthorization principalAuthorization, ICurrentTeleoptiPrincipal currentTeleoptiPrincipal, IPersonRepository personRepository, IIanaTimeZoneProvider ianaTimeZoneProvider, IToggleManager toggles)
		{
			_principalAuthorization = principalAuthorization;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_personRepository = personRepository;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
			_toggles = toggles;
		}

		public ViewResult Index()
		{
			return new ViewResult();
		}

		[UnitOfWorkAction, HttpGet, OutputCache(NoStore = true, Duration = 0)]
		public JsonResult NavigationContent()
		{
			var principal = _currentTeleoptiPrincipal.Current();
			return Json(new
			{
				UserName = principal.Identity.Name,
				PersonId = ((IUnsafePerson)principal).Person.Id,
				IsMyTimeAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb),
				IsRealTimeAdherenceAvailable =
					_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview),
				IsTeamScheduleAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules),
				IanaTimeZone = _ianaTimeZoneProvider.WindowsToIana(principal.Regional.TimeZone.Id)
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

		[HttpGet, OutputCache(Duration = 0, NoStore = true)]
		public ActionResult Resources()
		{
			var path = Request.MapPath("~/Areas/Anywhere/Content/Translation/TranslationTemplate.txt");
			var template = System.IO.File.ReadAllText(path);

			var userTexts = JsonConvert.SerializeObject(new
				{
					UserTexts.Resources.Home,
					UserTexts.Resources.SignOut,
					UserTexts.Resources.Settings,
					UserTexts.Resources.ContractTime,
					UserTexts.Resources.WorkTime,
					UserTexts.Resources.Next,
					UserTexts.Resources.Previous,
					UserTexts.Resources.LoadingThreeDots,
					UserTexts.Resources.AddFullDayAbsence,
					UserTexts.Resources.AddActivity,
					UserTexts.Resources.Apply,
					UserTexts.Resources.Cancel,
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
					UserTexts.Resources.CannotCreateSecondShiftWhenAddingActivity,
					UserTexts.Resources.OutOfAdherence,
					UserTexts.Resources.RealTimeAdherence,
					UserTexts.Resources.Reports,
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
					UserTexts.Resources.ServerUnavailable,
					UserTexts.Resources.ModifySchedule,
					UserTexts.Resources.Adherence,
					UserTexts.Resources.LastUpdated,
					UserTexts.Resources.StartDateMustBeSmallerThanEndDate,
					UserTexts.Resources.Planned,
					UserTexts.Resources.Actual,
					UserTexts.Resources.Start,
					UserTexts.Resources.Send,
					UserTexts.Resources.FilterColon,
					UserTexts.Resources.AbsenceBackToWork,
					UserTexts.Resources.Save,
					UserTexts.Resources.UpdatingAbsence,
					UserTexts.Resources.DateFromGreaterThanDateTo,
					UserTexts.Resources.BackToWorkCannotBeGreaterThanAbsenceEnd,
					UserTexts.Resources.BackToWorkTextPrompt,

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
					FirstDayOfWeek = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek,
					ShowMeridian = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains("t"),

					RTA_ChangeScheduleInAgentStateView_29934 = _toggles.IsEnabled(Toggles.RTA_ChangeScheduleInAgentStateView_29934),
					RTA_SeePercentageAdherenceForOneAgent_30783 = _toggles.IsEnabled(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783),
					RTA_SeeAdherenceDetailsForOneAgent_31285 = _toggles.IsEnabled(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285),
					RTA_NotifyViaSMS_31567 = _toggles.IsEnabled(Toggles.RTA_NotifyViaSMS_31567),
					RTA_NoBroker_31237 = _toggles.IsEnabled(Toggles.RTA_NoBroker_31237),

				}, Formatting.Indented);

			template = string.Format(template, userTexts);

			return new ContentResult { Content = template, ContentType = "text/javascript" };
		}
	}
}