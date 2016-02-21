using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{
	public interface IAbsenceRequestUpdater
	{
		bool UpdateAbsenceRequest(IPersonRequest personRequest, IAbsenceRequest absenceRequest, IUnitOfWork unitOfWork, ISchedulingResultStateHolder schedulingResultStateHolder);
	}
}