using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public static class EnumerablePersonExtensions
	{
		public static IPerson[] FixedStaffPeople(this IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			return agents.Where(
				p =>
					p.PersonPeriods(period)
						.Any(
							pp =>
								pp.PersonContract?.Contract != null && pp.PersonContract.Contract.EmploymentType != EmploymentType.HourlyStaff)).ToArray();
		}

		public static IPerson[] Filter(this IEnumerable<IPerson> agents, IEnumerable<Guid> agentIds)
		{
			var hashAgentIds = agentIds?.ToHashSet();
			return (agentIds == null ? 
				agents : 
				agents.Where(x => hashAgentIds.Contains(x.Id.Value))).ToArray();
		}
	}
}