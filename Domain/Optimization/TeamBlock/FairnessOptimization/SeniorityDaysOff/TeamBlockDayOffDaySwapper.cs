using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	public interface ICancellable
	{
		void Cancel();
	}
	public interface ITeamBlockDayOffDaySwapper : ICancellable
	{
		bool TrySwap(DateOnly dateOnly, ITeamBlockInfo teamBlockSenior, ITeamBlockInfo teamBlockJunior,
					 ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences, IList<DateOnly> dayOffsToGiveAwa);
	}

	public class TeamBlockDayOffDaySwapper : ITeamBlockDayOffDaySwapper
	{
		private readonly ISwapServiceNew _swapServiceNew;
		private readonly ITeamBlockDayOffDaySwapDecisionMaker _teamBlockDayOffDaySwapDecisionMaker;
		private bool _cancel;
	    private readonly IPostSwapValidationForTeamBlock _postSwapValidationForTeamBlock;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;

		public TeamBlockDayOffDaySwapper(ISwapServiceNew swapServiceNew,
		                                 ITeamBlockDayOffDaySwapDecisionMaker teamBlockDayOffDaySwapDecisionMaker,
		                                 IPostSwapValidationForTeamBlock postSwapValidationForTeamBlock,
		                                 ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator)
		{
			_swapServiceNew = swapServiceNew;
			_teamBlockDayOffDaySwapDecisionMaker = teamBlockDayOffDaySwapDecisionMaker;
			_postSwapValidationForTeamBlock = postSwapValidationForTeamBlock;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
		}

		public bool TrySwap(DateOnly dateOnly, ITeamBlockInfo teamBlockSenior, ITeamBlockInfo teamBlockJunior,
							ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences, IList<DateOnly> dayOffsToGiveAway)
		{
            _cancel = false;
            var swapResult = trySwapAndValidate(dateOnly, teamBlockSenior, teamBlockJunior, rollbackService,
		                                        scheduleDictionary, optimizationPreferences, dayOffsToGiveAway);
            if (swapResult && !validate(teamBlockSenior,teamBlockJunior,optimizationPreferences ))
            {
                rollbackService.Rollback();
                return false;
            }
            rollbackService.ClearModificationCollection();
            return swapResult;
		}

        private bool trySwapAndValidate(DateOnly dateOnly, ITeamBlockInfo teamBlockSenior, ITeamBlockInfo teamBlockJunior,
                            ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences, IList<DateOnly> dayOffsToGiveAway)
        {
            var totalModifyList = new List<IScheduleDay>();
            var daysToSwap = _teamBlockDayOffDaySwapDecisionMaker.Decide(dateOnly, teamBlockSenior, teamBlockJunior, scheduleDictionary,
                                                        optimizationPreferences, dayOffsToGiveAway);
            if (daysToSwap == null)
                return false;
            var teamBlockSeniorGroupMembers = teamBlockSenior.TeamInfo.GroupMembers.ToList();
            var teamBlockJuniorGroupMembers = teamBlockJunior.TeamInfo.GroupMembers.ToList();
            for (int i = 0; i < teamBlockSeniorGroupMembers.Count(); i++)
            {
                if (_cancel) return false;
                var personSenior = teamBlockSeniorGroupMembers[i];
                var personJunior = teamBlockJuniorGroupMembers[i];
                var scheduleRangeSenior = scheduleDictionary[personSenior];
                var scheduleRangeJunior = scheduleDictionary[personJunior];
                var seniorScheduleDayToHaveDayOff = scheduleRangeSenior.ScheduledDay(daysToSwap.DateForSeniorDayOff);
                var juniorScheduleDayToRemoveDayOff = scheduleRangeJunior.ScheduledDay(daysToSwap.DateForSeniorDayOff);
                var seniorScheduleDayToRemoveDayOff = scheduleRangeSenior.ScheduledDay(daysToSwap.DateForRemovingSeniorDayOff);
                var juniorScheduleDayToHaveDayOff = scheduleRangeJunior.ScheduledDay(daysToSwap.DateForRemovingSeniorDayOff);
                totalModifyList.AddRange(_swapServiceNew.Swap(new List<IScheduleDay> { seniorScheduleDayToHaveDayOff, juniorScheduleDayToRemoveDayOff },
                                         scheduleDictionary));
                totalModifyList.AddRange(_swapServiceNew.Swap(new List<IScheduleDay> { seniorScheduleDayToRemoveDayOff, juniorScheduleDayToHaveDayOff },
                                         scheduleDictionary));
            }

            rollbackService.ClearModificationCollection();
            var modifyResults = rollbackService.ModifyParts(totalModifyList);

            if (modifyResults.Any())
            {
                rollbackService.Rollback();
                return false;
            }
            

            return true;
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

		public void Cancel()
		{
			_cancel = true;
		}
	}
}
