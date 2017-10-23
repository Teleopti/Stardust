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
	[EnabledBy(FeatureFlags.Toggles.MyTimeWeb_WaitListPositionEnhancement_46301)]
	public class AbsenceRequestWaitlistProviderFor46301 : IAbsenceRequestWaitlistProvider
	{
		private readonly IPersonRequestRepository _personRequestRepository;

		public AbsenceRequestWaitlistProviderFor46301(IPersonRequestRepository personRequestRepository)
		{
			_personRequestRepository = personRequestRepository;
		}

		public IEnumerable<IPersonRequest> GetWaitlistedRequests(DateTimePeriod period, IWorkflowControlSet workflowControlSet)
		{
			return getRequestsInWaitlist(period, workflowControlSet, null);
		}

		public int GetPositionInWaitlist(IAbsenceRequest absenceRequest)
		{
			var personRequest = absenceRequest.Parent as PersonRequest;
			if (personRequest == null || !personRequest.IsWaitlisted) return 0;

			var queryAbsenceRequestsPeriod = absenceRequest.Period.ChangeEndTime(TimeSpan.FromSeconds(-1));
			var requestsInWaitlist = getRequestsInWaitlist(queryAbsenceRequestsPeriod, absenceRequest.Person.WorkflowControlSet, getBudgetGroup(absenceRequest)).ToList();
			var index = requestsInWaitlist.FindIndex(perRequest => perRequest.Id == personRequest.Id);
			return index > -1 ? index + 1 : 0;
		}

		private IEnumerable<IPersonRequest> getRequestsInWaitlist(DateTimePeriod period, IWorkflowControlSet workflowControlSet, IBudgetGroup budgetGroup)
		{
			var requestTypes = new[] { RequestType.AbsenceRequest };
			var requestFilter = new RequestFilter
			{
				Period = period,
				RequestTypes = requestTypes,
				ExcludeRequestsOnFilterPeriodEdge = true
			};

			int count;
			var waitlistedRequests =
				from request in _personRequestRepository.FindAbsenceAndTextRequests(requestFilter, out count, true)
				where requestShouldBeProcessed(request, budgetGroup)
				select request;

			var processOrder = workflowControlSet.AbsenceRequestWaitlistProcessOrder;
			return processOrder == WaitlistProcessOrder.BySeniority
				? waitlistedRequests.OrderByDescending(x => x.Person.Seniority).ThenBy(x => x.CreatedOn)
				: waitlistedRequests.OrderBy(x => x.CreatedOn);
		}

		private bool requestShouldBeProcessed(IPersonRequest request, IBudgetGroup budgetGroup)
		{
			if (getBudgetGroup(request.Request) != budgetGroup)
				return false;

			if (request.IsWaitlisted)
				return true;

			if (!request.IsPending)
				return false;

			var isAutoGrant =
				request.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest)request.Request)
					.AbsenceRequestProcess.GetType() == typeof(GrantAbsenceRequest);
			return isAutoGrant;
		}

		private IBudgetGroup getBudgetGroup(IRequest request)
		{
			return request.Person.PersonPeriods(request.Period.ToDateOnlyPeriod(request.Person.PermissionInformation.DefaultTimeZone())).FirstOrDefault()?.BudgetGroup;
		}
	}
}