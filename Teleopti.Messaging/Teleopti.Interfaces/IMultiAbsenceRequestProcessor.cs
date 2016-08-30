using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces
{
	public interface IMultiAbsenceRequestProcessor	
	{
		void ProcessAbsenceRequest(List<IPersonRequest> personRequests);
		void ApproveAbsenceRequestWithValidators(List<IPersonRequest> personRequest,
												 IUnitOfWork unitOfWork, IEnumerable<IAbsenceRequestValidator> validators);
	}
}