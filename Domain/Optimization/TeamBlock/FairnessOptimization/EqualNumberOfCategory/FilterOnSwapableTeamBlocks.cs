

using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IFilterOnSwapableTeamBlocks
	{
		IList<ITeamBlockInfo> Filter(IList<ITeamBlockInfo> teamBlockInfoList, ITeamBlockInfo teamBlockInfoToWorkWith);
	}

	public class FilterOnSwapableTeamBlocks : IFilterOnSwapableTeamBlocks
	{
		private readonly ITeamBlockPeriodValidator _teamBlockPeriodValidator;
		private readonly ITeamMemberCountValidator _teamMemberCountValidator;
		private readonly ITeamBlockContractTimeValidator _teamBlockContractTimeValidator;
		private readonly ITeamBlockSameSkillValidator _teamBlockSameSkillValidator;
		private readonly ITeamBlockSameRuleSetBagValidator _teamBlockSameRuleSetBagValidator;

		public FilterOnSwapableTeamBlocks(ITeamBlockPeriodValidator teamBlockPeriodValidator, ITeamMemberCountValidator teamMemberCountValidator, ITeamBlockContractTimeValidator teamBlockContractTimeValidator, ITeamBlockSameSkillValidator teamBlockSameSkillValidator, ITeamBlockSameRuleSetBagValidator teamBlockSameRuleSetBagValidator)
		{
			_teamBlockPeriodValidator = teamBlockPeriodValidator;
			_teamMemberCountValidator = teamMemberCountValidator;
			_teamBlockContractTimeValidator = teamBlockContractTimeValidator;
			_teamBlockSameSkillValidator = teamBlockSameSkillValidator;
			_teamBlockSameRuleSetBagValidator = teamBlockSameRuleSetBagValidator;
		}

		public IList<ITeamBlockInfo> Filter(IList<ITeamBlockInfo> teamBlockInfoList, ITeamBlockInfo teamBlockInfoToWorkWith)
		{
			var possibleTeamBlocksToSwapWith = new List<ITeamBlockInfo>();
			foreach (var teamBlockInfo in teamBlockInfoList)
			{
				if (!_teamBlockPeriodValidator.ValidatePeriod(teamBlockInfo, teamBlockInfoToWorkWith))
					continue;

				if (!_teamMemberCountValidator.ValidateMemberCount(teamBlockInfo, teamBlockInfoToWorkWith))
					continue;

				if (!_teamBlockContractTimeValidator.ValidateContractTime(teamBlockInfo, teamBlockInfoToWorkWith))
					continue;

				if (!_teamBlockSameSkillValidator.ValidateSameSkill(teamBlockInfo, teamBlockInfoToWorkWith))
					continue;

				if(!_teamBlockSameRuleSetBagValidator.ValidateSameRuleSetBag(teamBlockInfo, teamBlockInfoToWorkWith))
					continue;

				//inga lås i blocken

				possibleTeamBlocksToSwapWith.Add(teamBlockInfo);
			}

			return possibleTeamBlocksToSwapWith;
		}
	}
}