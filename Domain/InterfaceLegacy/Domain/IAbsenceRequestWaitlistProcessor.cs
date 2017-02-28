using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceRequestWaitlistProcessor
	{
		void ProcessAbsenceRequestWaitlist (IUnitOfWork unitOfWork, DateTimePeriod period, IWorkflowControlSet workflowControlSet);
	}
}