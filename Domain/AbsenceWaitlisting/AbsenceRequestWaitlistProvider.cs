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
			if (absenceRequest.IsWaitlisted())
			{
				var personRequest = absenceRequest.Parent as PersonRequest;

				var waitlistedRequests = GetWaitlistedRequests(absenceRequest.Period, absenceRequest.Person.WorkflowControlSet).ToList();
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
			var requestFilter = new RequestFilter() { Period = period, RequestTypes = requestTypes };

			var waitlistedRequests = from request in _personRequestRepository.FindAllRequests(requestFilter)
									 where requestShouldBeProcessed(request, workflowControlSet)
									 orderby request.CreatedOn ascending
									 select request;

			return waitlistedRequests;
		}
		
		private static bool requestShouldBeProcessed(IPersonRequest request, IWorkflowControlSet workflowControlSet)
		{
			return !request.IsApproved
					&& request.Person.WorkflowControlSet == workflowControlSet
					&& !request.WasManuallyDenied;
		}
	}
}
