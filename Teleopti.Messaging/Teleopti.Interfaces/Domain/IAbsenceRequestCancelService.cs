using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IAbsenceRequestCancelService
	{
		void CancelAbsenceRequest(IAbsenceRequest absenceRequest);
		void CancelAbsenceRequestsFromPersonAbsences(IEnumerable<IPersonAbsence> personAbsences);
	}
}