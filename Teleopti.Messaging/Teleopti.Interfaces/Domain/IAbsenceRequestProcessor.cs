using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{
	public interface IAbsenceRequestProcessor
	{
		void ProcessAbsenceRequest( IUnitOfWork unitOfWork, IAbsenceRequest absenceRequest, IPersonRequest personRequest);
	}
}