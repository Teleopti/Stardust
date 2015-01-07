using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IAgentBadgeWithRankRepository : IRepository<IAgentBadgeWithRank>
	{
		/// <summary>
		/// Find badge of specific badge type and persons
		/// </summary>
		/// <param name="personIdList"></param>
		/// <param name="badgeType"></param>
		/// <returns></returns>
		ICollection<IAgentBadgeWithRank> Find(IEnumerable<Guid> personIdList, BadgeType badgeType);

		/// <summary>
		/// Find badge of specific persons
		/// </summary>
		/// <param name="personIdList"></param>
		/// <returns></returns>
		ICollection<IAgentBadgeWithRank> Find(IEnumerable<Guid> personIdList);

		/// <summary>
		/// Find badges for the agent.
		/// </summary>
		/// <param name="person">The agent to get badge.</param>
		/// <returns></returns>
		ICollection<IAgentBadgeWithRank> Find(IPerson person);

		/// <summary>
		/// Find badge for the agent and specific badge type.
		/// </summary>
		/// <param name="person">The agent to get badge.</param>
		/// <param name="badgeType">Badge type.</param>
		/// <returns></returns>
		IAgentBadgeWithRank Find(IPerson person, BadgeType badgeType);
	}
}