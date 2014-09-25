using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadgeRepository : IRepository<IAgentBadge>
	{
		/// <summary>
		/// Find badge of specific badge type and persons
		/// </summary>
		/// <param name="personIdList"></param>
		/// <param name="badgeType"></param>
		/// <returns></returns>
		ICollection<IAgentBadge> Find(IEnumerable<Guid> personIdList, BadgeType badgeType);

		/// <summary>
		/// Find badges for the agent.
		/// </summary>
		/// <param name="person">The agent to get badge.</param>
		/// <returns></returns>
		ICollection<IAgentBadge> Find(IPerson person);

		/// <summary>
		/// Find badge for the agent and specific badge type.
		/// </summary>
		/// <param name="person">The agent to get badge.</param>
		/// <param name="badgeType">Badge type.</param>
		/// <returns></returns>
		IAgentBadge Find(IPerson person, BadgeType badgeType);

		/// <summary>
		/// reset all badges for all agents
		/// </summary>
		void ResetAgentBadges();
	}
}