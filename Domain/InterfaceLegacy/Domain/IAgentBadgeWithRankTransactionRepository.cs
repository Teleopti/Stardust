using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAgentBadgeWithRankTransactionRepository : IRepository<IAgentBadgeWithRankTransaction>
	{
		/// <summary>
		/// Find badge for the agent and specific badge type.
		/// </summary>
		/// <param name="person">The agent to get badge.</param>
		/// <param name="badgeType">Badge type.</param>
		/// <param name="calculateDate">The calculated date</param>
		/// <param name="isExternal">to identify whether the gamification imported from outside or not</param>
		/// <returns></returns>
		IAgentBadgeWithRankTransaction Find(IPerson person, int badgeType, DateOnly calculateDate, bool isExternal);

		IList<IAgentBadgeWithRankTransaction> Find(IEnumerable<IPerson> personCollection, DateOnlyPeriod period);

		IList<IAgentBadgeWithRankTransaction> Find(IPerson person, int badgeType, DateOnlyPeriod period, bool isExternal);

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