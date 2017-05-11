using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface IAllTeamMembersInSelectionSpecification
	{
		bool IsSatifyBy(ITeamInfo teamInfo, IEnumerable<IPerson> selectedPersons);
	}

	public class AllTeamMembersInSelectionSpecification : IAllTeamMembersInSelectionSpecification
	{
		
		public bool IsSatifyBy(ITeamInfo teamInfo, IEnumerable< IPerson> selectedPersons)
		{
			foreach (var groupMember in teamInfo.GroupMembers)
			{
				if (!selectedPersons.Contains(groupMember))
					return false;
			}

			return true;

		}
	}
}