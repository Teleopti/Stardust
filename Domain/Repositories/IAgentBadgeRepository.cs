using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAgentBadgeRepository
	{
		/// <summary>
		/// Find badge of specific badge type and persons
		/// </summary>
		/// <param name="personIdList"></param>
		/// <param name="badgeType"></param>
		/// <returns></returns>
		ICollection<AgentBadge> Find(IEnumerable<Guid> personIdList, BadgeType badgeType);

		/// <summary>
		/// Find badge of specific persons
		/// </summary>
		/// <param name="personIdList"></param>
		/// <returns></returns>
		ICollection<AgentBadge> Find(IEnumerable<Guid> personIdList);
		
		/// <summary>
		/// Find badge for the agent and specific badge type.
		/// </summary>
		/// <param name="person">The agent to get badge.</param>
		/// <param name="badgeType">Badge type.</param>
		/// <returns></returns>
		AgentBadge Find(IPerson person, BadgeType badgeType);
	}
}