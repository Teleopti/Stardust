using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IAbsenceRequestWaitlistProvider
	{
		IEnumerable<IPersonRequest> GetWaitlistedRequests(DateTimePeriod period, IWorkflowControlSet workflowControlSet);
		int GetPositionInWaitlist (IAbsenceRequest absenceRequest);
	}
}