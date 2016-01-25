namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadgeWithRankTransactionRepository : IRepository<IAgentBadgeWithRankTransaction>
	{
		/// <summary>
		/// Find badge for the agent and specific badge type.
		/// </summary>
		/// <param name="person">The agent to get badge.</param>
		/// <param name="badgeType">Badge type.</param>
		/// <param name="calculateDate">The calculated date</param>
		/// <returns></returns>
		IAgentBadgeWithRankTransaction Find(IPerson person, BadgeType badgeType, DateOnly calculateDate);

		/// <summary>
		/// reset all badges for all agents
		/// </summary>
		void ResetAgentBadges();
	}
}