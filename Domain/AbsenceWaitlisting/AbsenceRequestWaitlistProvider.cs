using System.Collections.Generic;
using System.Linq;
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
