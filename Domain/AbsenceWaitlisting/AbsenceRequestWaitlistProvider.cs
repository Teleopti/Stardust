using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AbsenceWaitlisting
{
	public class AbsenceRequestWaitlistProvider : IAbsenceRequestWaitlistProvider
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPersonRepository _personRepository;

		public AbsenceRequestWaitlistProvider(IPersonRequestRepository personRequestRepository, IPersonRepository personRepository)
		{
			_personRequestRepository = personRequestRepository;
			_personRepository = personRepository;
		}

		public IList<IPersonRequest> GetWaitlistedRequests(DateTimePeriod period)
		{
			var waitListIds = _personRequestRepository.GetWaitlistRequests(period).ToList();
			return _personRequestRepository.Find(waitListIds).ToList();
		}

		public int GetPositionInWaitlist(IAbsenceRequest absenceRequest)
		{
			if (!(absenceRequest.Parent is PersonRequest personRequest) || !personRequest.IsWaitlisted) return 0;

			var requestsInWaitlist = getRequestsInWaitlist(absenceRequest).ToList();
			var index = requestsInWaitlist.FindIndex(perRequest => perRequest.Id == personRequest.Id);
			return index > -1 ? index + 1 : 0;
		}

		private IEnumerable<IPersonRequest> getRequestsInWaitlist(IAbsenceRequest absenceRequest)
		{
			var requestTypes = new[] { RequestType.AbsenceRequest };
			var period = absenceRequest.Period.ChangeEndTime(TimeSpan.FromSeconds(-1));
			var requestFilter = new RequestFilter
			{
				Period = period,
				RequestTypes = requestTypes,
				ExcludeRequestsOnFilterPeriodEdge = true,
				RequestFilters = new Dictionary<RequestFilterField, string>
				{
					{ RequestFilterField.Status, $"{(int)PersonRequestStatus.Pending} {(int)PersonRequestStatus.Waitlisted}" }
				}
			};

			var allAndTextRequests =
				_personRequestRepository.FindAbsenceAndTextRequests(requestFilter, out _, true).ToList();

			var personIds = allAndTextRequests.Select(a => a.Person.Id.Value).Distinct().ToList();


			var budgetGroupNames = _personRepository.FindBudgetGroupNameForPeople(personIds,
					TimeZoneHelper.ConvertFromUtc(period.StartDateTime, absenceRequest.Person.PermissionInformation.DefaultTimeZone()).Date)
				.ToDictionary(x => x.PersonId, x => x.BudgetGroupName);

			var waitlistedRequests = allAndTextRequests.Where(request =>
			{
				budgetGroupNames.TryGetValue(request.Person.Id.Value, out string requestBudgetGroupName);
				budgetGroupNames.TryGetValue(absenceRequest.Person.Id.Value, out string currentUserRequestBudgetGroupName);
				return requestShouldBeProcessed(request, requestBudgetGroupName, currentUserRequestBudgetGroupName);
			});

			var processOrder = absenceRequest.Person.WorkflowControlSet.AbsenceRequestWaitlistProcessOrder;
			return processOrder == WaitlistProcessOrder.BySeniority
				? waitlistedRequests.OrderByDescending(x => x.Person.Seniority).ThenBy(x => x.CreatedOn)
				: waitlistedRequests.OrderBy(x => x.CreatedOn);
		}

		private static bool requestShouldBeProcessed(IPersonRequest request, string requestBudgetGroupName, string currentUserBudgetGroupName)
		{
			if (currentUserBudgetGroupName != requestBudgetGroupName)
				return false;

			if (request.IsWaitlisted)
				return true;

			if (request.Person.WorkflowControlSet == null)
				return false;

			var isAutoGrant =
				request.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest)request.Request)
					.AbsenceRequestProcess.GetType() == typeof(GrantAbsenceRequest);
			return isAutoGrant;
		}
	}
}