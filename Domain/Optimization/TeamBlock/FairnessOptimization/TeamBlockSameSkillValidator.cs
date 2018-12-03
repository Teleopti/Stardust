

using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
	public interface ITeamBlockSameSkillValidator
	{
		bool ValidateSameSkill(ITeamBlockInfo teamBlock1, ITeamBlockInfo teamBlock2);
	}

	public class TeamBlockSameSkillValidator : ITeamBlockSameSkillValidator
	{
		private readonly ITeamBlockPersonsSkillChecker _teamBlockPersonsSkillChecker;

		public TeamBlockSameSkillValidator(ITeamBlockPersonsSkillChecker teamBlockPersonsSkillChecker)
		{
			_teamBlockPersonsSkillChecker = teamBlockPersonsSkillChecker;
		}

		public bool ValidateSameSkill(ITeamBlockInfo teamBlock1, ITeamBlockInfo teamBlock2)
		{
			var teamBlock1Members = teamBlock1.TeamInfo.GroupMembers.ToList();
			var teamBlock2Members = teamBlock2.TeamInfo.GroupMembers.ToList();
			var roleModelPerson = teamBlock1Members[0];
			
			foreach (var dateOnly in teamBlock1.BlockInfo.BlockPeriod.DayCollection())
			{
				var roleModelPersonPeriod = roleModelPerson.Period(dateOnly);

				if (!checkTeamMembers(roleModelPersonPeriod, dateOnly, teamBlock2Members))
					return false;

				if (!checkTeamMembers(roleModelPersonPeriod, dateOnly, teamBlock1Members))
					return false;

			}

			return true;
		}

		private bool checkTeamMembers(IPersonPeriod roleModelPersonPeriod, DateOnly dateOnly, IEnumerable<IPerson> teamBlockMembers)
		{
			foreach (var teamBlockMember in teamBlockMembers)
			{
				var memberPersonPeriod = teamBlockMember.Period(dateOnly);
				if (!_teamBlockPersonsSkillChecker.PersonsHaveSameSkills(roleModelPersonPeriod, memberPersonPeriod))
					return false;
			}

			return true;
		}
	}
}