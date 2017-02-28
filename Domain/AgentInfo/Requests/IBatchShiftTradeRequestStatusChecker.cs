using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IBatchShiftTradeRequestStatusChecker : IShiftTradeRequestStatusChecker
	{
		void StartBatch(IEnumerable<IPersonRequest> personRequests);
		void EndBatch();
	}
}