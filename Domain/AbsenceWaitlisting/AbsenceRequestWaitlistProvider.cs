﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AbsenceWaitlisting
{
	
	public class AbsenceRequestWaitlistProvider : IAbsenceRequestWaitlistProvider
	{
		private readonly IPersonRequestRepository _personRequestRepository;

		public AbsenceRequestWaitlistProvider(IPersonRequestRepository personRequestRepository)
		{
			_personRequestRepository = personRequestRepository;
		}

		public int GetPositionInWaitlist (IAbsenceRequest absenceRequest)
		{
			var personRequest = absenceRequest.Parent as PersonRequest;

			if (personRequest != null && personRequest.IsWaitlisted)
			{
				var queryAbsenceRequestsPeriod = absenceRequest.Period.ChangeEndTime (TimeSpan.FromSeconds (-1));
				var waitlistedRequests = GetWaitlistedRequests(queryAbsenceRequestsPeriod, absenceRequest.Person.WorkflowControlSet).ToList();
				var index = waitlistedRequests.FindIndex(perRequest => perRequest.Id == personRequest.Id);
				if (index > -1)
				{
					return index +1;
				}	
			}

			return 0;
		}

		public IEnumerable<IPersonRequest> GetWaitlistedRequests(DateTimePeriod period, IWorkflowControlSet workflowControlSet)
		{
			var requestTypes = new[] { RequestType.AbsenceRequest };
			var requestFilter = new RequestFilter() { Period = period, RequestTypes = requestTypes, ExcludeRequestsOnFilterPeriodEdge = true };
			
			var waitlistedRequests = from request in _personRequestRepository.FindAllRequests (requestFilter)
				where requestShouldBeProcessed (request, workflowControlSet)
				orderby request.CreatedOn ascending
				select request;

			return waitlistedRequests.ToList();
		}
		
		private bool requestShouldBeProcessed(IPersonRequest request, IWorkflowControlSet workflowControlSet)
		{

			if (workflowControlSet == null)
			{
				return false;
			}
			
			if (request.IsWaitlisted || request.IsNew)
			{
				return request.Person.WorkflowControlSet?.Id == workflowControlSet.Id;

			}

			return false;
			

		}
	}
}
