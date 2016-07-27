using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public class RequestViewModelMapper : IRequestViewModelMapper
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

		public RequestViewModel Map(RequestViewModel requestViewModel, IPersonRequest request)
		{
			var team = request.Person.MyTeam(new DateOnly(request.Request.Period.StartDateTime));
			requestViewModel.Id = request.Id.GetValueOrDefault();
			requestViewModel.Subject = request.GetSubject(new NoFormatting());
			requestViewModel.Message = request.GetMessage(new NoFormatting());
			requestViewModel.DenyReason = request.DenyReason;
			requestViewModel.TimeZone = _ianaTimeZoneProvider.WindowsToIana(request.Person.PermissionInformation.DefaultTimeZone().Id);
			requestViewModel.PeriodStartTime = TimeZoneHelper.ConvertFromUtc(request.Request.Period.StartDateTime, request.Person.PermissionInformation.DefaultTimeZone());
			requestViewModel.PeriodEndTime = TimeZoneHelper.ConvertFromUtc(request.Request.Period.EndDateTime, request.Person.PermissionInformation.DefaultTimeZone());
			requestViewModel.CreatedTime = request.CreatedOn.HasValue
				? TimeZoneHelper.ConvertFromUtc(request.CreatedOn.Value, request.Person.PermissionInformation.DefaultTimeZone())
				: (DateTime?)null;
			requestViewModel.UpdatedTime = request.UpdatedOn.HasValue
				? TimeZoneHelper.ConvertFromUtc(request.UpdatedOn.Value, request.Person.PermissionInformation.DefaultTimeZone())
				: (DateTime?)null;
			requestViewModel.AgentName = _personNameProvider.BuildNameFromSetting(request.Person.Name);
			requestViewModel.PersonId = request.Person.Id.GetValueOrDefault();
			requestViewModel.Seniority = request.Person.Seniority;
			requestViewModel.Type = request.Request.RequestType;
			requestViewModel.TypeText = request.Request.RequestTypeDescription;
			requestViewModel.StatusText = request.StatusText;
			requestViewModel.Status = getRequestStatus(request);
			requestViewModel.Payload = request.Request.RequestPayloadDescription;
			requestViewModel.Team = team?.SiteAndTeam;
			requestViewModel.IsFullDay = isFullDay(request);

			var absenceRequest = request.Request as IAbsenceRequest;

			if (absenceRequest != null )
			{
				requestViewModel.PersonAccountApprovalSummary = getPersonalAccountApprovalSummary (absenceRequest);
			}


			return requestViewModel;
		}


		private PersonAccountApprovalSummary getPersonalAccountApprovalSummary(IAbsenceRequest absenceRequest)
		{
			var canApprove = false;
			var personAbsenceAccount = _personAbsenceAccountProvider.Find(absenceRequest.Person);
			if (personAbsenceAccount != null)
			{
				var accountForPersonAbsence = personAbsenceAccount.Find (absenceRequest.Absence);
				var affectedAccounts = accountForPersonAbsence?.Find (new DateOnlyPeriod (new DateOnly (absenceRequest.Period.StartDateTime),
						DateOnly.MaxValue));

				if (affectedAccounts != null)
				{
					canApprove = checkApprovalStatus(absenceRequest, affectedAccounts);
				}
			}
			return new PersonAccountApprovalSummary
			{
				Color = (canApprove ? Color.Green : Color.Red).ToHtml()

			};

		}

		private static bool checkApprovalStatus (IAbsenceRequest absenceRequest, IEnumerable<IAccount> affectedAccounts)
		{

			var canApprove = true;

			foreach (var account in affectedAccounts)
			{
				//get timespan within account period and compare to remaining timespan.
				var timeSpanOfAbsenceRequest = absenceRequest.Period.Intersection (account.Period().ToDateTimePeriod (TimeZoneInfo.Utc));
				if (timeSpanOfAbsenceRequest.HasValue)
				{
					var timeSpanOfIntersection = timeSpanOfAbsenceRequest.Value.EndDateTime -
												 timeSpanOfAbsenceRequest.Value.StartDateTime;

					if (account.Remaining < timeSpanOfIntersection)
					{
						canApprove = false;
					}
				}
			}
			return canApprove;
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