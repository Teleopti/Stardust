using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAgentBadgeTransactionRepository : IRepository<IAgentBadgeTransaction>
	{
		/// <summary>
		/// Find badge for the agent and specific badge type.
		/// </summary>
		/// <param name="person">The agent to get badge.</param>
		/// <param name="badgeType">Badge type.</param>
		/// <param name="calculateDate">The calculated date</param>
		/// <returns></returns>
		IAgentBadgeTransaction Find(IPerson person, int badgeType, DateOnly calculateDate);

		IList<IAgentBadgeTransaction> Find(IEnumerable<IPerson> personCollection, DateOnlyPeriod period);

		/// <summary>
		/// remove all badges for all agents within period
		/// </summary>
		/// <param name="period"></param>
		void Remove(DateOnlyPeriod period);

		/// <summary>
		/// reset all badges for all agents
		/// </summary>
		void ResetAgentBadges();
	}
}