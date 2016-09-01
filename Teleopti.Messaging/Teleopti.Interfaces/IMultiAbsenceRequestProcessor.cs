using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces
{
	public interface IMultiAbsenceRequestProcessor	
	{
		void ProcessAbsenceRequest(List<IPersonRequest> personRequests);
	}
}