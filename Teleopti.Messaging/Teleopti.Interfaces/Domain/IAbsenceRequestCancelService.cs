using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IAbsenceRequestCancelService
	{
		void CancelAbsenceRequest(IAbsenceRequest absenceRequest);

		void CancelAbsenceRequestsFromPersonAbsence(IPersonAbsence personAbsence);
	}
}