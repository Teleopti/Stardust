using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces
{
	public interface IMultiAbsenceRequestsUpdater
	{
		void UpdateAbsenceRequest(List<IPersonRequest> personRequests);
	}
}