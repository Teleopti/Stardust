using System;
using System.Globalization;
using System.Web.Mvc;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Extensions;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	[NoCacheFilterMvc]
	public class ApplicationController : Controller
	{
		private readonly IAuthorization _authorization;
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly INow _now;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly IGlobalSettingDataRepository _settingDataRepository;

		public ApplicationController(IAuthorization authorization, ICurrentTeleoptiPrincipal currentTeleoptiPrincipal,
			IIanaTimeZoneProvider ianaTimeZoneProvider, IUserTimeZone userTimeZone, INow now,
			ICurrentDataSource currentDataSource, IGlobalSettingDataRepository settingDataRepository)
		{
			_authorization = authorization;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
			_userTimeZone = userTimeZone;
			_now = now;
			_currentDataSource = currentDataSource;
			_settingDataRepository = settingDataRepository;
		}

		public ViewResult Index()
		{
			return new ViewResult();
		}

		public ActionResult Redirect()
		{
			const int waitingTimeInSecond = 5;

			var principal = _currentTeleoptiPrincipal.Current();
			var currentUserName = principal.Identity.Name;
			var currentBusinessUnit = (principal.Identity as ITeleoptiIdentity)?.BusinessUnit.Name;
			var resAnywhereMigrated = string.Format(UserTexts.Resources.AnywhereMigrated, "<a href='./WFM'>", "</a>");
			var resPageWillBeRedirected = string.Format(UserTexts.Resources.PageWillBeRedirected,
				"<span id='pendingTime'>" + waitingTimeInSecond + "</span>");

			return new ViewResult
			{
				ViewData = new ViewDataDictionary
				{
					{"WaitingTimeInSecond", waitingTimeInSecond},
					{"CurrentUserName", currentUserName},
					{"CurrentBusinessUnit", currentBusinessUnit},
					{"ResTeamSchedule", UserTexts.Resources.Schedules},
					{"ResRta", UserTexts.Resources.RealTimeAdherence},
					{"ResReports", UserTexts.Resources.Reports},
					{"ResSignOut", UserTexts.Resources.SignOut},
					{"ResMyTeamMigrated", resAnywhereMigrated},
					{"ResPageWillBeRedirected", resPageWillBeRedirected}
				},
				ViewName = "Redirect"
			};
		}

		[UnitOfWork, HttpGet]
		public virtual JsonResult NavigationContent()
		{
			var principal = _currentTeleoptiPrincipal.Current();
			return Json(new
			{
				UserName = principal.Identity.Name,
				PersonId = ((IUnsafePerson)principal).Person.Id,
				IsMyTimeAvailable = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb),
				IsRealTimeAdherenceAvailable =
					_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview),
				IsTeamScheduleAvailable = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules),
				IanaTimeZone = _ianaTimeZoneProvider.WindowsToIana(principal.Regional.TimeZone.Id)
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult Permissions()
		{
			var currentName = _currentDataSource.CurrentName();
			var isSmsLinkAvailable = DefinedLicenseDataFactory.HasLicense(currentName) &&
									 DefinedLicenseDataFactory.GetLicenseActivator(currentName).EnabledLicenseOptionPaths.Contains(
										 DefinedLicenseOptionPaths.TeleoptiCccSmsLink);

			return Json(new
			{
				IsAddFullDayAbsenceAvailable = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence),
				IsAddIntradayAbsenceAvailable = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence),
				IsRemoveAbsenceAvailable = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RemoveAbsence),
				IsAddActivityAvailable = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddActivity),
				IsMoveActivityAvailable = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MoveActivity),
				IsSmsLinkAvailable = isSmsLinkAvailable
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[UnitOfWork]
		public virtual ActionResult Resources()
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
				UserTexts.Resources.Schedules,
				UserTexts.Resources.NoAbsenceToRemoveInCurrentShift,
				UserTexts.Resources.NoHierarchiesExist,
				UserTexts.Resources.TryOutTheNewImprovedTeams,

				DateTimeFormatExtensions.FixedDateFormat,
				DateTimeFormatExtensions.FixedDateTimeFormat,
				DateTimeFormatExtensions.FixedTimeFormat,
				DateTimeFormatExtensions.FixedDateFormatForMoment,
				DateTimeFormatExtensions.FixedDateTimeFormatForMoment,
				DateTimeFormatExtensions.FixedDateTimeWithSecondsFormatForMoment,
				DateTimeFormatExtensions.FixedTimeFormatForMoment,

				DateFormat = DateTimeFormatExtensions.LocalizedDateFormat,
				DateTimeFormat = DateTimeFormatExtensions.LocalizedDateTimeFormat,
				TimeFormat = DateTimeFormatExtensions.LocalizedTimeFormat,

				DateFormatForMoment = DateTimeFormatExtensions.LocalizedDateFormatForMoment,
				DateTimeFormatForMoment = DateTimeFormatExtensions.LocalizedDateTimeFormatForMoment,
				TimeFormatForMoment = DateTimeFormatExtensions.LocalizedTimeFormatForMoment,

				TimeZoneOffsetMinutes = _userTimeZone.TimeZone().GetUtcOffset(_now.UtcDateTime()).TotalMinutes,

				LanguageCode = CultureInfo.CurrentCulture.IetfLanguageTag,
				FirstDayOfWeek = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek,
				ShowMeridian = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains("t")
			}, Formatting.Indented);

			template = string.Format(template, userTexts);

			return new ContentResult { Content = template, ContentType = "text/javascript" };
		}

		[HttpGet, UnitOfWork]
		public virtual JsonResult FullDayAbsenceRequestTimeSetting()
		{
			TimeSpanSetting fullDayAbsenceRequestStartTimeSetting =
				_settingDataRepository.FindValueByKey("FullDayAbsenceRequestStartTime", new TimeSpanSetting(new TimeSpan(0, 0, 0)));
			TimeSpanSetting fullDayAbsenceRequestEndTimeSetting =
				_settingDataRepository.FindValueByKey("FullDayAbsenceRequestEndTime", new TimeSpanSetting(new TimeSpan(23, 59, 0)));
			return Json(new
			{
				Start = fullDayAbsenceRequestStartTimeSetting,
				End = fullDayAbsenceRequestEndTimeSetting
			}, JsonRequestBehavior.AllowGet);
		}
	}
}