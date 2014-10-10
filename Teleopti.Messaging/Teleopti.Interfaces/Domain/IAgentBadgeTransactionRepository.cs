using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadgeTransactionRepository : IRepository<IAgentBadgeTransaction>
	{
		/// <summary>
		/// Find badge transactions for the agent.
		/// </summary>
		/// <param name="person">The agent to get badge.</param>
		/// <returns></returns>
		ICollection<IAgentBadgeTransaction> Find(IPerson person);

		/// <summary>
		/// Find badge transactions for the agent and specific badge type.
		/// </summary>
		/// <param name="person">The agent to get badge.</param>
		/// <param name="badgeType">Badge type.</param>
		/// <returns></returns>
		ICollection<IAgentBadgeTransaction> Find(IPerson person, BadgeType badgeType);

		/// <summary>
		/// Find badge for the agent and specific badge type.
		/// </summary>
		/// <param name="person">The agent to get badge.</param>
		/// <param name="badgeType">Badge type.</param>
		/// <param name="calculateDate">The calculated date</param>
		/// <returns></returns>
		IAgentBadgeTransaction Find(IPerson person, BadgeType badgeType, DateOnly calculateDate);

		/// <summary>
 		/// reset all badges for all agents
 		/// </summary>
 		void ResetAgentBadges();
	}
}