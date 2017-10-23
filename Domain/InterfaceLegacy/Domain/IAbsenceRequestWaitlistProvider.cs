using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceRequestWaitlistProvider
	{
		IList<IPersonRequest> GetWaitlistedRequests(DateTimePeriod period);
		int GetPositionInWaitlist (IAbsenceRequest absenceRequest);
	}
}