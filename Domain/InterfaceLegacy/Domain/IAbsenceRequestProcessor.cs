using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceRequestProcessor
	{
		void ProcessAbsenceRequest(IUnitOfWork unitOfWork, IAbsenceRequest absenceRequest, IPersonRequest personRequest);
		void ApproveAbsenceRequestWithValidators(IPersonRequest personRequest, IAbsenceRequest absenceRequest,
			IUnitOfWork unitOfWork, IEnumerable<IAbsenceRequestValidator> validators);
	}
}