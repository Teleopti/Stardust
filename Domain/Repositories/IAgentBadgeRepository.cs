using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
		ICollection<AgentBadge> Find(IEnumerable<Guid> personIdList, int badgeType, bool isExternal);

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
		/// <param name="IsExternal">Indicates whether the badge type is external.</param>
		/// <returns></returns>
		AgentBadge Find(IPerson person, int badgeType, bool IsExternal);

		/// <summary>
		/// Find badge for the agent and specific badge type within period.
		/// </summary>
		/// <param name="person">The agent to get badge.</param>
		/// <param name="badgeType">Badge type.</param>
		/// <param name="isExternal">Indicates whether the badge type is external.</param>
		/// <param name="period">the period should be within.</param>
		/// <returns></returns>
		AgentBadge Find(IPerson person, int badgeType, bool isExternal, DateOnlyPeriod period);
	}
}