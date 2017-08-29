using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.Services
{
	public class ShiftTradeRequestStatusCheckerForTestAlwaysRefer : IBatchShiftTradeRequestStatusChecker
	{
		public void Check(IShiftTradeRequest shiftTradeRequest)
		{
			shiftTradeRequest.Refer(new PersonRequestAuthorizationCheckerForTest());
		}

		public void StartBatch(IEnumerable<IPersonRequest> personRequests)
		{
		}

		public void EndBatch()
		{
		}
	}
}