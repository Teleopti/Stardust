using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{
	public interface IAbsenceRequestWaitlistProcessor
	{
		void ProcessAbsenceRequestWaitlist (IUnitOfWork unitOfWork, DateTimePeriod period, IWorkflowControlSet workflowControlSet);
	}
}