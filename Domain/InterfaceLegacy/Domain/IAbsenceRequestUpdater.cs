using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceRequestUpdater
	{
		bool UpdateAbsenceRequest(IPersonRequest personRequest, IAbsenceRequest absenceRequest,
			IUnitOfWork unitOfWork,
			ISchedulingResultStateHolder schedulingResultStateHolder);

	}
}