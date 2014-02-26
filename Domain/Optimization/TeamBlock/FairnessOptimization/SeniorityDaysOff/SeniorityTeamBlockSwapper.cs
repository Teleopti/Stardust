

using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	public interface ISeniorityTeamBlockSwapper
	{
		bool SwapAndValidate(ITeamBlockInfo mostSeniorTeamBlock, ITeamBlockInfo blockToSwapWith,
		                     ISchedulePartModifyAndRollbackService rollbackService,
		                     IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences);
	}

	public class SeniorityTeamBlockSwapper : ISeniorityTeamBlockSwapper
	{
		private readonly ITeamBlockSwapper _teambBlockSwapper;
	    private readonly IPostSwapValidationForTeamBlock _postSwapValidationForTeamBlock;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;

		public SeniorityTeamBlockSwapper(ITeamBlockSwapper teambBlockSwapper, 
										IPostSwapValidationForTeamBlock postSwapValidationForTeamBlock,
										ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator)
		{
			_teambBlockSwapper = teambBlockSwapper;
	        _postSwapValidationForTeamBlock = postSwapValidationForTeamBlock;
		    _teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
		}

		public bool SwapAndValidate(ITeamBlockInfo mostSeniorTeamBlock, ITeamBlockInfo blockToSwapWith,
		                            ISchedulePartModifyAndRollbackService rollbackService,
		                            IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences)
		{
			bool success = trySwapAndValidate(mostSeniorTeamBlock, blockToSwapWith, rollbackService, scheduleDictionary,
			                                  optimizationPreferences);
			if (!success)
			{
				rollbackService.Rollback();
				return false;
			}			

			return true;
		}

		private bool trySwapAndValidate(ITeamBlockInfo mostSeniorTeamBlock, ITeamBlockInfo blockToSwapWith,
		                                ISchedulePartModifyAndRollbackService rollbackService,
		                                IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences)
		{
			if (!_teambBlockSwapper.TrySwap(mostSeniorTeamBlock, blockToSwapWith, rollbackService, scheduleDictionary))
				return false;

		    return validate(mostSeniorTeamBlock,blockToSwapWith,optimizationPreferences );
		}

        private bool validate(ITeamBlockInfo mostSeniorTeamBlock, ITeamBlockInfo blockToSwapWith, IOptimizationPreferences optimizationPreferences)
        {

            if (!_postSwapValidationForTeamBlock.Validate(mostSeniorTeamBlock, optimizationPreferences))
                return false;

            if (!_postSwapValidationForTeamBlock.Validate(blockToSwapWith, optimizationPreferences))
                return false;

	        if (!_teamBlockShiftCategoryLimitationValidator.Validate(mostSeniorTeamBlock, blockToSwapWith,
		                                                             optimizationPreferences))
		        return false;

            return true;
        }
	}
}