using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceRequestWaitlistProvider
	{
		IList<IPersonRequest> GetWaitlistedRequests(DateTimePeriod period);
		int GetPositionInWaitlist (IAbsenceRequest absenceRequest);
	}
}