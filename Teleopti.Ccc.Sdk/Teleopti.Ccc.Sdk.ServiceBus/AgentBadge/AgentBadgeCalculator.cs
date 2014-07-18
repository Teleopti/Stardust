using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class AgentBadgeCalculator
	{
		protected static IList<IPerson> addBadge(IEnumerable<IPerson> allPersons, IEnumerable<Tuple<Guid, int>> agentsThatShouldGetBadge)
		{
			var personsThatGotABadge = new List<IPerson>();
			if (agentsThatShouldGetBadge != null)
			{
				foreach (
					var person in
						agentsThatShouldGetBadge.Select(agent => allPersons.Single(x => x.Id.Value == agent.Item1)).Where(a => a != null))
				{
					person.AddBadge(new Domain.Common.AgentBadge { BronzeBadge = 1 });
					personsThatGotABadge.Add(person);
				}
			}
			return personsThatGotABadge;
		}
	}
}
