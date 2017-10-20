﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AbsenceWaitlisting
{
	public class AbsenceRequestWaitlistProviderFor46301 : IAbsenceRequestWaitlistProvider
	{
		private readonly IPersonRequestRepository _personRequestRepository;

		public AbsenceRequestWaitlistProviderFor46301(IPersonRequestRepository personRequestRepository)
		{
			_personRequestRepository = personRequestRepository;
		}

		public IEnumerable<IPersonRequest> GetWaitlistedRequests(DateTimePeriod period, IWorkflowControlSet workflowControlSet)
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
				where requestShouldBeProcessed(request)
				select request;

			var processOrder = workflowControlSet.AbsenceRequestWaitlistProcessOrder;
			return processOrder == WaitlistProcessOrder.BySeniority
				? waitlistedRequests.OrderByDescending(x => x.Person.Seniority).ThenBy(x => x.CreatedOn)
				: waitlistedRequests.OrderBy(x => x.CreatedOn);
		}

		public int GetPositionInWaitlist(IAbsenceRequest absenceRequest)
		{
			var personRequest = absenceRequest.Parent as PersonRequest;

			if (personRequest == null || !personRequest.IsWaitlisted) return 0;

			var queryAbsenceRequestsPeriod = absenceRequest.Period.ChangeEndTime(TimeSpan.FromSeconds(-1));
			var waitlistedRequests =
				GetWaitlistedRequests(queryAbsenceRequestsPeriod, absenceRequest.Person.WorkflowControlSet).ToList();
			var index = waitlistedRequests.FindIndex(perRequest => perRequest.Id == personRequest.Id);
			return index > -1 ? index + 1 : 0;
		}

		private bool requestShouldBeProcessed(IPersonRequest request)
		{
			return request.IsWaitlisted || request.IsNew;
		}
	}
}