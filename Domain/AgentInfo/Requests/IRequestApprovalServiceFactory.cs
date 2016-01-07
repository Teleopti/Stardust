using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IRequestApprovalServiceFactory
	{
		IRequestApprovalService MakeRequestApprovalServiceScheduler(IScheduleDictionary scheduleDictionary, IScenario scenario);
	}
}