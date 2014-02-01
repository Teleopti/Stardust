

using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	public interface ISeniorityTeamBlockSwapper
	{
		bool SwapAndValidate(ITeamBlockInfo mostSeniorTeamBlock, ITeamBlockInfo blockToSwapWith,
		                     ISchedulePartModifyAndRollbackService rollbackService,
		                     IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences,
		                     ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator);
	}

	public class SeniorityTeamBlockSwapper : ISeniorityTeamBlockSwapper
	{
		private readonly ITeamBlockSwapper _teambBlockSwapper;
		private readonly ISeniorityTeamBlockSwapValidator _seniorityTeamBlockSwapValidator;

		public SeniorityTeamBlockSwapper(ITeamBlockSwapper teambBlockSwapper,
		                                 ISeniorityTeamBlockSwapValidator seniorityTeamBlockSwapValidator)
		{
			_teambBlockSwapper = teambBlockSwapper;
			_seniorityTeamBlockSwapValidator = seniorityTeamBlockSwapValidator;
		}

		public bool SwapAndValidate(ITeamBlockInfo mostSeniorTeamBlock, ITeamBlockInfo blockToSwapWith,
		                            ISchedulePartModifyAndRollbackService rollbackService,
		                            IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences,
		                            ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator)
		{
			bool success = trySwapAndValidate(mostSeniorTeamBlock, blockToSwapWith, rollbackService, scheduleDictionary,
			                                  optimizationPreferences, teamBlockRestrictionOverLimitValidator);
			if (!success)
			{
				rollbackService.Rollback();
				return false;
			}			

			return true;
		}

		private bool trySwapAndValidate(ITeamBlockInfo mostSeniorTeamBlock, ITeamBlockInfo blockToSwapWith,
		                                ISchedulePartModifyAndRollbackService rollbackService,
		                                IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences,
		                                ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator)
		{
			if (!_teambBlockSwapper.TrySwap(mostSeniorTeamBlock, blockToSwapWith, rollbackService, scheduleDictionary))
				return false;

			if (!_seniorityTeamBlockSwapValidator.Validate(mostSeniorTeamBlock, optimizationPreferences))
				return false;

			if (!_seniorityTeamBlockSwapValidator.Validate(blockToSwapWith, optimizationPreferences))
				return false;

			if (!teamBlockRestrictionOverLimitValidator.Validate(mostSeniorTeamBlock, optimizationPreferences))
				return false;

			if (!teamBlockRestrictionOverLimitValidator.Validate(blockToSwapWith, optimizationPreferences))
				return false;

			return true;
		}
	}
}