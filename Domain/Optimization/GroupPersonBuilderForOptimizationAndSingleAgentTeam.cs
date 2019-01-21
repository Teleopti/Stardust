using System.Collections.Generic;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class GroupPersonBuilderForOptimizationAndSingleAgentTeam : IGroupPersonBuilderForOptimization
	{
		public Group BuildGroup(IEnumerable<IPerson> personsInOrganisation, IPerson person, DateOnly dateOnly)
		{
			return new Group(new List<IPerson> {person}, person.Name.ToString());
		}
	}
}