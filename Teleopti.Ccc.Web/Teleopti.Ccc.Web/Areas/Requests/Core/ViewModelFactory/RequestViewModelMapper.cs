using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Core;


namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public class RequestViewModelMapper : IRequestViewModelMapper<AbsenceAndTextRequestViewModel>,
		IRequestViewModelMapper<ShiftTradeRequestViewModel>,
		IRequestViewModelMapper<OvertimeRequestViewModel>
	{
		private readonly IPersonNameProvider _personNameProvider;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly IPersonAbsenceAccountProvider _personAbsenceAccountProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IAgentScheduleViewModelProvider _agentScheduleViewModelProvider;

		private static readonly NoFormatting textFormatter = new NoFormatting();

		public RequestViewModelMapper(IPersonNameProvider personNameProvider, IIanaTimeZoneProvider ianaTimeZoneProvider,
			IPersonAbsenceAccountProvider personAbsenceAccountProvider, IUserTimeZone userTimeZone, IScheduleStorage scheduleStorage, 
			ICurrentScenario currentScenario, IAgentScheduleViewModelProvider agentScheduleViewModelProvider)
		{
			_personNameProvider = personNameProvider;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
			_personAbsenceAccountProvider = personAbsenceAccountProvider;
			_userTimeZone = userTimeZone;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_agentScheduleViewModelProvider = agentScheduleViewModelProvider;
		}

		public AbsenceAndTextRequestViewModel Map(AbsenceAndTextRequestViewModel requestViewModel, IPersonRequest request,
			NameFormatSettings nameFormatSettings)
		{
			mapRequestViewModel(requestViewModel, request, nameFormatSettings);

			mapAbsenceRequestSpecificFields(requestViewModel, request);

			return requestViewModel;
		}

		public ShiftTradeRequestViewModel Map(ShiftTradeRequestViewModel requestViewModel, IPersonRequest request,
			NameFormatSettings nameFormatSettings)
		{
			mapRequestViewModel(requestViewModel, request, nameFormatSettings);

			return requestViewModel;
		}

		public OvertimeRequestViewModel Map(OvertimeRequestViewModel requestViewModel, IPersonRequest request,
			NameFormatSettings nameFormatSettings)
		{
			mapRequestViewModel(requestViewModel, request, nameFormatSettings);

			requestViewModel.OvertimeTypeDescription = (request.Request as IOvertimeRequest)?.MultiplicatorDefinitionSet.Name;

			requestViewModel.BrokenRules = NewBusinessRuleCollection.GetRuleDescriptionsFromFlag(request.BrokenBusinessRules.GetValueOrDefault(0));

			return requestViewModel;
		}

		private void mapRequestViewModel(RequestViewModel requestViewModel, IPersonRequest request,
			NameFormatSettings nameFormatSettings)
		{
			var timeZone = request.Person.PermissionInformation.DefaultTimeZone();
			var team = request.Person.MyTeam(new DateOnly(request.Request.Period.StartDateTimeLocal(timeZone)));
			requestViewModel.Id = request.Id.GetValueOrDefault();
			requestViewModel.Subject = request.GetSubject(textFormatter);
			requestViewModel.Message = request.GetMessage(textFormatter);
			requestViewModel.DenyReason = Resources.ResourceManager.GetString(request.DenyReason) ?? request.DenyReason;
			requestViewModel.TimeZone =
				_ianaTimeZoneProvider.WindowsToIana(timeZone.Id);
			requestViewModel.PeriodStartTime = request.Request.Period.StartDateTimeLocal(timeZone);
			requestViewModel.PeriodEndTime = request.Request.Period.EndDateTimeLocal(timeZone);
			requestViewModel.CreatedTime = request.CreatedOn.HasValue
				? TimeZoneHelper.ConvertFromUtc(request.CreatedOn.Value, timeZone)
				: (DateTime?)null;
			requestViewModel.UpdatedTime = request.UpdatedOn.HasValue
				? TimeZoneHelper.ConvertFromUtc(request.UpdatedOn.Value, timeZone)
				: (DateTime?)null;
			requestViewModel.AgentName = _personNameProvider.BuildNameFromSetting(request.Person.Name, nameFormatSettings);
			requestViewModel.PersonId = request.Person.Id.GetValueOrDefault();
			requestViewModel.Seniority = request.Person.Seniority;
			requestViewModel.Type = request.Request.RequestType;
			requestViewModel.TypeText = request.Request.RequestTypeDescription;
			requestViewModel.StatusText = request.StatusText;
			requestViewModel.Status = getRequestStatus(request);
			requestViewModel.Payload = request.Request.RequestPayloadDescription;
			requestViewModel.Team = team?.SiteAndTeam;
		}

		private void mapAbsenceRequestSpecificFields (AbsenceAndTextRequestViewModel requestViewModel, IPersonRequest request)
		{
			if (request.Request is IAbsenceRequest absenceRequest)
			{
				var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(absenceRequest.Person, new ScheduleDictionaryLoadOptions(false, false),
					absenceRequest.Period, _currentScenario.Current());

				requestViewModel.PersonAccountSummaryViewModel = getPersonalAccountApprovalSummary(absenceRequest);

				requestViewModel.IsFullDay = isFullDay(request);
				var scheduleRange = schedules[absenceRequest.Person];

				requestViewModel.Shifts = getSchedules(scheduleRange, absenceRequest.Person, absenceRequest.Period.ToDateOnlyPeriod(request.Person.PermissionInformation.DefaultTimeZone()));
			}
		}

		private IEnumerable<TeamScheduleAgentScheduleViewModel> getSchedules(IScheduleRange schedules, IPerson person, DateOnlyPeriod period)
		{
			var scheduleDays = schedules.ScheduledDayCollection(period);
			return scheduleDays.Select(date => _agentScheduleViewModelProvider.GetAgentScheduleViewModel(person, date));
		}

		private PersonAccountSummaryViewModel getPersonalAccountApprovalSummary(IAbsenceRequest absenceRequest)
		{
			var personAccountSummaryDetails = new List<PersonAccountSummaryDetailViewModel>();

			var personAbsenceAccount = _personAbsenceAccountProvider.Find(absenceRequest.Person);
			if (personAbsenceAccount != null)
			{
				var accountForPersonAbsence = personAbsenceAccount.Find (absenceRequest.Absence);
				var affectedAccounts =
					accountForPersonAbsence?.Find(absenceRequest.Period.ToDateOnlyPeriod(_userTimeZone.TimeZone()));

				if (affectedAccounts != null)
				{

					var sortedAccounts = affectedAccounts.OrderBy (account => account.StartDate);

					personAccountSummaryDetails.AddRange (
						sortedAccounts.Select (account => getPersonalAccountPeriodViewModelsForRequest (absenceRequest, account)));
				}
			}
			return new PersonAccountSummaryViewModel
			{
				PersonAccountSummaryDetails =  personAccountSummaryDetails
			};
		}

		private static PersonAccountSummaryDetailViewModel getPersonalAccountPeriodViewModelsForRequest(
			IAbsenceRequest absenceRequest, IAccount account)
		{
			return new PersonAccountSummaryDetailViewModel
			{
				StartDate = account.StartDate.Date,
				EndDate = account.Period().EndDate.Date,
				RemainingDescription = convertTimeSpanToString(account.Remaining, absenceRequest.Absence.Tracker),
				TrackingTypeDescription = getDescriptionOfTrackerTimeSpan(absenceRequest.Absence.Tracker.GetType())
			};
		}

		private static string convertTimeSpanToString(TimeSpan ts, ITracker tracker)
		{
			var result = string.Empty;
			
			var classTypeOfTracker = tracker.GetType();
			if (classTypeOfTracker == Tracker.CreateDayTracker().GetType())
			{
				result = ts.TotalDays.ToString(CultureInfo.CurrentCulture);
			}
			else if (classTypeOfTracker == Tracker.CreateTimeTracker().GetType())
			{
				result = TimeHelper.GetLongHourMinuteTimeString(ts, CultureInfo.CurrentCulture);
			}

			return result;
		}

		private static string getDescriptionOfTrackerTimeSpan(Type trackerClassType)
		{
			var trackerType = "";

			if (trackerClassType == Tracker.CreateDayTracker().GetType())
			{
				trackerType = Resources.Days;
			}
			else if (trackerClassType == Tracker.CreateTimeTracker().GetType())
			{
				trackerType = Resources.Hours;
			}
			return trackerType;

		}

		private static PersonRequestStatus getRequestStatus(IPersonRequest request)
		{
			if (request.IsApproved || request.IsAutoAproved)
			{
				return PersonRequestStatus.Approved;
			}

			if (request.IsPending)
			{
				return PersonRequestStatus.Pending;
			}

			if(request.IsWaitlisted)
			{
				return PersonRequestStatus.Waitlisted;
			}

			if(request.IsDenied)
			{
				return PersonRequestStatus.Denied;
			}

			if (request.IsAutoDenied)
			{
				return PersonRequestStatus.AutoDenied;
			}

			if (request.IsCancelled)
			{
				return PersonRequestStatus.Cancelled;
			}

			return PersonRequestStatus.New;
		}

		private static bool isFullDay(IPersonRequest request)
		{
			var timeZone = request.Person.PermissionInformation.DefaultTimeZone();
			var startTime = request.Request.Period.StartDateTimeLocal(timeZone);
			if (startTime.Hour != 0 || startTime.Minute != 0 || startTime.Second != 0) return false;
			var endTime = request.Request.Period.EndDateTimeLocal(timeZone);
			if (endTime.Hour != 23 || endTime.Minute != 59 || endTime.Second != 0) return false;
			return true;
		}
	}
}