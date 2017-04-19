using System;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IAgentGroupModelPersister
	{
		void Persist(AgentGroupModel agentGroupModel);
		void Delete(Guid agentGroupId);
		void DeleteLastPeriod(Guid agentGroupId);
	}
}