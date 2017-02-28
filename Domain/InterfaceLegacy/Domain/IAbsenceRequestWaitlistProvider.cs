using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceRequestWaitlistProvider
	{
		IEnumerable<IPersonRequest> GetWaitlistedRequests(DateTimePeriod period, IWorkflowControlSet workflowControlSet);
		int GetPositionInWaitlist (IAbsenceRequest absenceRequest);
	}
}