using System;
using System.Globalization;
using System.Web.Mvc;
using System.Web.UI;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	[NoCacheFilterMvc]
	public class ApplicationController : Controller
	{
		private readonly IAuthorization _authorization;
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly IToggleManager _toggles;
		private readonly IUserTimeZone _userTimeZone;
		private readonly INow _now;
		private readonly ICurrentDataSource _currentDataSource;

		public ApplicationController(IAuthorization authorization, ICurrentTeleoptiPrincipal currentTeleoptiPrincipal, IIanaTimeZoneProvider ianaTimeZoneProvider, IToggleManager toggles, IUserTimeZone userTimeZone, INow now, ICurrentDataSource currentDataSource)
		{
			_authorization = authorization;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
			_toggles = toggles;
			_userTimeZone = userTimeZone;
			_now = now;
			_currentDataSource = currentDataSource;
		}

		public ViewResult Index()
		{
			return new ViewResult();
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
				UserTexts.Resources.TryOutTheNewImprovedMyTeam,

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

				TimeZoneOffsetMinutes = _userTimeZone.TimeZone().GetUtcOffset(_now.UtcDateTime()).TotalMinutes,

				LanguageCode = CultureInfo.CurrentCulture.IetfLanguageTag,
				FirstDayOfWeek = (int) CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek,
				ShowMeridian = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains("t"),

				MyTeam_MakeTeamScheduleConsistent_31897 = _toggles.IsEnabled(Toggles.MyTeam_MakeTeamScheduleConsistent_31897),
				
				RTA_AdherenceDetails_34267 = _toggles.IsEnabled(Toggles.RTA_AdherenceDetails_34267),
				WfmTeamSchedule_PrepareForRelease_37752 = _toggles.IsEnabled(Toggles.WfmTeamSchedule_PrepareForRelease_37752)

			}, Formatting.Indented);

			template = string.Format(template, userTexts);

			return new ContentResult { Content = template, ContentType = "text/javascript" };
		}

		[HttpGet]
		public JsonResult FullDayAbsenceRequestTimeSetting()
		{
			TimeSpanSetting fullDayAbsenceRequestStartTimeSetting;
			TimeSpanSetting fullDayAbsenceRequestEndTimeSetting;
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				fullDayAbsenceRequestStartTimeSetting = new GlobalSettingDataRepository(uow).FindValueByKey("FullDayAbsenceRequestStartTime",
					new TimeSpanSetting(new TimeSpan(0, 0, 0)));
				fullDayAbsenceRequestEndTimeSetting = new GlobalSettingDataRepository(uow).FindValueByKey("FullDayAbsenceRequestEndTime",
					new TimeSpanSetting(new TimeSpan(23, 59, 0)));
			}
			return Json(new
			{
				Start = fullDayAbsenceRequestStartTimeSetting,
				End = fullDayAbsenceRequestEndTimeSetting
			}, JsonRequestBehavior.AllowGet);
		}
	}
}