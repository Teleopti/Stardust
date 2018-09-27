﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AbsenceWaitlisting
{
	[DisabledBy(FeatureFlags.Toggles.MyTimeWeb_WaitListPositionEnhancement_46301)]
	public class AbsenceRequestWaitlistProvider : IAbsenceRequestWaitlistProvider
	{
		private readonly IPersonRequestRepository _personRequestRepository;

		public AbsenceRequestWaitlistProvider(IPersonRequestRepository personRequestRepository)
		{
			_personRequestRepository = personRequestRepository;
		}

		public IList<IPersonRequest> GetWaitlistedRequests(DateTimePeriod period)
		{
			var waitListIds = _personRequestRepository.GetWaitlistRequests(period).ToList();
			return _personRequestRepository.Find(waitListIds).ToList();
		}

		public int GetPositionInWaitlist(IAbsenceRequest absenceRequest)
		{
			if (!(absenceRequest.Parent is PersonRequest personRequest) || !personRequest.IsWaitlisted) return 0;

			var queryAbsenceRequestsPeriod = absenceRequest.Period.ChangeEndTime(TimeSpan.FromSeconds(-1));
			var requestsInWaitlist =
				getRequestsInWaitlist(queryAbsenceRequestsPeriod, absenceRequest.Person.WorkflowControlSet).ToList();
			var index = requestsInWaitlist.FindIndex(perRequest => perRequest.Id == personRequest.Id);
			return index > -1 ? index + 1 : 0;
		}

		private IEnumerable<IPersonRequest> getRequestsInWaitlist(DateTimePeriod period,
			IWorkflowControlSet workflowControlSet)
		{
			var requestTypes = new[] {RequestType.AbsenceRequest};
			var requestFilter = new RequestFilter
			{
				Period = period,
				RequestTypes = requestTypes,
				ExcludeRequestsOnFilterPeriodEdge = true
			};

			int count;
			var waitlistedRequests =
				from request in _personRequestRepository.FindAbsenceAndTextRequests(requestFilter, out count, true)
				where requestShouldBeProcessed(request, workflowControlSet)
				select request;

			var processOrder = workflowControlSet.AbsenceRequestWaitlistProcessOrder;
			return processOrder == WaitlistProcessOrder.BySeniority
				? waitlistedRequests.OrderByDescending(x => x.Person.Seniority).ThenBy(x => x.CreatedOn)
				: waitlistedRequests.OrderBy(x => x.CreatedOn);
		}

		private bool requestShouldBeProcessed(IPersonRequest request, IWorkflowControlSet workflowControlSet)
		{
			return (request.IsWaitlisted || request.IsNew) &&
				   workflowControlSet != null &&
				   request.Person.WorkflowControlSet?.Id == workflowControlSet.Id;
		}
	}
}