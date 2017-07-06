using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IRequestApprovalServiceFactory
	{
		IRequestApprovalService MakeRequestApprovalServiceScheduler(IScheduleDictionary scheduleDictionary, IScenario scenario, IPerson person);
		IRequestApprovalService MakeRequestApprovalServiceScheduler(IScheduleDictionary scheduleDictionary, IScenario scenario, IPersonRequest personRequest);
	}
}