

using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
	public interface ITeamBlockSameTimeZoneValidator
	{
		bool Validate(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2);
	}

	public class TeamBlockSameTimeZoneValidator : ITeamBlockSameTimeZoneValidator
	{
		public bool Validate(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2)
		{
			var teamBlock1Members = teamBlockInfo1.TeamInfo.GroupMembers.ToList();
			var teamBlock2Members = teamBlockInfo2.TeamInfo.GroupMembers.ToList();
			var roleModelPerson = teamBlock1Members[0];

			if (!checkTeamMembers(roleModelPerson, teamBlock1Members))
				return false;

			if (!checkTeamMembers(roleModelPerson, teamBlock2Members))
				return false;

			return true;
		}

		private bool checkTeamMembers(IPerson roleModelPerson, IEnumerable<IPerson> teamBlockMembers)
		{
			foreach (var teamBlockMember in teamBlockMembers)
			{
				if (!teamBlockMember.PermissionInformation.DefaultTimeZone().Id.Equals(roleModelPerson.PermissionInformation.DefaultTimeZone().Id))
					return false;
			}

			return true;
		}
	}
}