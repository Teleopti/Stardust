using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{
	public interface IAbsenceRequestProcessor
	{
		void ProcessAbsenceRequest(IUnitOfWork unitOfWork, IAbsenceRequest absenceRequest, IPersonRequest personRequest);

	    void ApproveAbsenceRequestWithValidators(IPersonRequest personRequest, IAbsenceRequest absenceRequest,
	        IUnitOfWork unitOfWork, IEnumerable<IAbsenceRequestValidator> validators);



	}
}