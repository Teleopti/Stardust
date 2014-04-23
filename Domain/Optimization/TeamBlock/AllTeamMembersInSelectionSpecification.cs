using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface IAllTeamMembersInSelectionSpecification
	{
		bool IsSatifyBy(ITeamInfo teamInfo, IList<IPerson> selectedPersons);
	}

	public class AllTeamMembersInSelectionSpecification : IAllTeamMembersInSelectionSpecification
	{
		
		public bool IsSatifyBy(ITeamInfo teamInfo, IList< IPerson> selectedPersons)
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