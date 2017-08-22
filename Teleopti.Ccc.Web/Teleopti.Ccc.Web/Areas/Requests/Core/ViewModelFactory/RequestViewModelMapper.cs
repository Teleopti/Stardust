using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public class RequestViewModelMapper 
		: IRequestViewModelMapper<AbsenceAndTextRequestViewModel>, IRequestViewModelMapper<ShiftTradeRequestViewModel>,
			IRequestViewModelMapper<OvertimeRequestViewModel>
	{

		private readonly IPersonNameProvider _personNameProvider;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly IPersonAbsenceAccountProvider _personAbsenceAccountProvider;

		public RequestViewModelMapper(IPersonNameProvider personNameProvider, IIanaTimeZoneProvider ianaTimeZoneProvider, IPersonAbsenceAccountProvider personAbsenceAccountProvider)
		{
			_personNameProvider = personNameProvider;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
			_personAbsenceAccountProvider = personAbsenceAccountProvider;
		}

		public AbsenceAndTextRequestViewModel Map(AbsenceAndTextRequestViewModel requestViewModel, IPersonRequest request, NameFormatSettings nameFormatSettings)
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

			return requestViewModel;
		}

		private void mapRequestViewModel(RequestViewModel requestViewModel, IPersonRequest request,
	NameFormatSettings nameFormatSettings)
		{
			var team = request.Person.MyTeam(new DateOnly(request.Request.Period.StartDateTime));
			requestViewModel.Id = request.Id.GetValueOrDefault();
			requestViewModel.Subject = request.GetSubject(new NoFormatting());
			requestViewModel.Message = request.GetMessage(new NoFormatting());
			requestViewModel.DenyReason = request.DenyReason;
			requestViewModel.TimeZone =
				_ianaTimeZoneProvider.WindowsToIana(request.Person.PermissionInformation.DefaultTimeZone().Id);
			requestViewModel.PeriodStartTime = TimeZoneHelper.ConvertFromUtc(request.Request.Period.StartDateTime,
				request.Person.PermissionInformation.DefaultTimeZone());
			requestViewModel.PeriodEndTime = TimeZoneHelper.ConvertFromUtc(request.Request.Period.EndDateTime,
				request.Person.PermissionInformation.DefaultTimeZone());
			requestViewModel.CreatedTime = request.CreatedOn.HasValue
				? TimeZoneHelper.ConvertFromUtc(request.CreatedOn.Value, request.Person.PermissionInformation.DefaultTimeZone())
				: (DateTime?)null;
			requestViewModel.UpdatedTime = request.UpdatedOn.HasValue
				? TimeZoneHelper.ConvertFromUtc(request.UpdatedOn.Value, request.Person.PermissionInformation.DefaultTimeZone())
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
			var absenceRequest = request.Request as IAbsenceRequest;

			if (absenceRequest != null)
			{
				requestViewModel.PersonAccountSummaryViewModel = getPersonalAccountApprovalSummary(absenceRequest);

				requestViewModel.IsFullDay = isFullDay(request);
			}
		}

		private PersonAccountSummaryViewModel getPersonalAccountApprovalSummary(IAbsenceRequest absenceRequest)
		{
			var personAccountSummaryDetails = new List<PersonAccountSummaryDetailViewModel>();

			var personAbsenceAccount = _personAbsenceAccountProvider.Find(absenceRequest.Person);
			if (personAbsenceAccount != null)
			{
				var accountForPersonAbsence = personAbsenceAccount.Find (absenceRequest.Absence);
				var affectedAccounts = accountForPersonAbsence?.Find (absenceRequest.Period.ToDateOnlyPeriod(TimeZoneHelper.CurrentSessionTimeZone));

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

		private static PersonAccountSummaryDetailViewModel getPersonalAccountPeriodViewModelsForRequest (IAbsenceRequest absenceRequest, IAccount account)
		{
			return  new PersonAccountSummaryDetailViewModel()
			{
				StartDate = TimeZoneHelper.ConvertFromUtc(account.StartDate.Date, absenceRequest.Person.PermissionInformation.DefaultTimeZone()),
				EndDate = TimeZoneHelper.ConvertFromUtc(account.Period().EndDate.Date, absenceRequest.Person.PermissionInformation.DefaultTimeZone()),
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

		private static RequestStatus getRequestStatus(IPersonRequest request)
		{

			//ROBTODO: review status - should we include waitlisted and cancelled in this ?
			return request.IsApproved
				? RequestStatus.Approved
				: request.IsPending
					? RequestStatus.Pending
					: request.IsDenied
						? RequestStatus.Denied
						: RequestStatus.New;
		}
		
		private static bool isFullDay(IPersonRequest request)
		{
			// ref: Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping.TextRequestFormMappingProfile
			var startTime = TimeZoneHelper.ConvertFromUtc(request.Request.Period.StartDateTime, request.Person.PermissionInformation.DefaultTimeZone());
			if (startTime.Hour != 0 || startTime.Minute != 0 || startTime.Second != 0) return false;
			var endTime = TimeZoneHelper.ConvertFromUtc(request.Request.Period.EndDateTime, request.Person.PermissionInformation.DefaultTimeZone());
			if (endTime.Hour != 23 || endTime.Minute != 59 || endTime.Second != 0) return false;
			return true;
		}
	}
}