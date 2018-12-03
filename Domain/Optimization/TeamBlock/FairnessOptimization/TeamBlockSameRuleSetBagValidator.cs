using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
	public interface ITeamBlockSameRuleSetBagValidator
	{
		bool ValidateSameRuleSetBag(ITeamBlockInfo teamBlock1, ITeamBlockInfo teamBlock2);
	}

	public class TeamBlockSameRuleSetBagValidator : ITeamBlockSameRuleSetBagValidator
	{
		public bool ValidateSameRuleSetBag(ITeamBlockInfo teamBlock1, ITeamBlockInfo teamBlock2)
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
				if (roleModelPersonPeriod.RuleSetBag == null || memberPersonPeriod.RuleSetBag == null)
					return false;

				if (!roleModelPersonPeriod.RuleSetBag.Equals(memberPersonPeriod.RuleSetBag))
					return false;
			}

			return true;
		}
	}
}