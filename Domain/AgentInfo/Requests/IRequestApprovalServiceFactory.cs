using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IRequestApprovalServiceFactory
	{
		IRequestApprovalService MakeRequestApprovalService(IScheduleDictionary scheduleDictionary, IScenario scenario, IPersonRequest personRequest, IDictionary<string, object> commandDatas);
	}
}