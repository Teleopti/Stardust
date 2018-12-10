using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public interface ITeamBlockSwap
	{
		bool Swap(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2, ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, 
					DateOnlyPeriod selectedPeriod, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}

	public class TeamBlockSwap : ITeamBlockSwap
	{
		private readonly ITeamBlockSwapDayValidator _teamBlockSwapDayValidator;
		private readonly ISwapServiceNew _swapServiceNew;
		private readonly ITeamBlockSwapValidator _teamBlockSwapValidator;
		private readonly ITeamBlockOptimizationLimits _teamBlockOptimizationLimits;
		private readonly ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;

		public TeamBlockSwap(ISwapServiceNew swapServiceNew, ITeamBlockSwapValidator teamBlockSwapValidator, ITeamBlockSwapDayValidator teamBlockSwapDayValidator, 
													ITeamBlockOptimizationLimits teamBlockOptimizationLimits,
													ITeamBlockShiftCategoryLimitationValidator teamBlockShiftCategoryLimitationValidator)
		{
			_teamBlockSwapDayValidator = teamBlockSwapDayValidator;
			_swapServiceNew = swapServiceNew;
			_teamBlockSwapValidator = teamBlockSwapValidator;
			_teamBlockOptimizationLimits = teamBlockOptimizationLimits;
			_teamBlockShiftCategoryLimitationValidator = teamBlockShiftCategoryLimitationValidator;
		}

		public bool Swap(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2, ISchedulePartModifyAndRollbackService rollbackService, IScheduleDictionary scheduleDictionary, 
						DateOnlyPeriod selectedPeriod, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			if (!_teamBlockSwapValidator.ValidateCanSwap(teamBlockInfo1, teamBlockInfo2)) return false;
			var scheduleDays1 = extractScheduleDays(teamBlockInfo1, scheduleDictionary, selectedPeriod).ToList();
			var scheduleDays2 = extractScheduleDays(teamBlockInfo2, scheduleDictionary, selectedPeriod).ToList();
			var index = scheduleDays1.Count;
			var daysToSwap = new List<IScheduleDay>();
			var swappedDays = new List<IScheduleDay>();

			for (var i = 0; i < index; i++)
			{	
				daysToSwap.Clear();

				var day1 = scheduleDays1[i];
				var day2 = scheduleDays2[i];

				if (!_teamBlockSwapDayValidator.ValidateSwapDays(day1, day2)) return false;

				daysToSwap.Add(day1);
				daysToSwap.Add(day2);

				var result = _swapServiceNew.Swap(daysToSwap, scheduleDictionary);
				swappedDays.AddRange(result);
			}

			rollbackService.ClearModificationCollection();
			var modifyResults = rollbackService.ModifyParts(swappedDays);
			if (modifyResults.Any())
			{
				rollbackService.Rollback();
				return false;
			}

			var firstBlockOk = _teamBlockOptimizationLimits.Validate(teamBlockInfo1, optimizationPreferences, dayOffOptimizationPreferenceProvider);
			var secondBlockOk = _teamBlockOptimizationLimits.Validate(teamBlockInfo2, optimizationPreferences, dayOffOptimizationPreferenceProvider);

			if (!(firstBlockOk && secondBlockOk))
			{
				rollbackService.Rollback();
				return false;
			}

			firstBlockOk = _teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(teamBlockInfo1);
			secondBlockOk = _teamBlockOptimizationLimits.ValidateMinWorkTimePerWeek(teamBlockInfo2);

			if (!(firstBlockOk && secondBlockOk))
			{
				rollbackService.Rollback();
				return false;
			}

			if (!_teamBlockShiftCategoryLimitationValidator.Validate(teamBlockInfo1, teamBlockInfo2, optimizationPreferences))
			{
				rollbackService.Rollback();
				return false;
			}

			rollbackService.ClearModificationCollection();

			return true;
		}


		private IEnumerable<IScheduleDay> extractScheduleDays(ITeamBlockInfo teamBlockInfo, IScheduleDictionary scheduleDictionary, DateOnlyPeriod selectedPeriod)
		{
			var scheduleDays = new List<IScheduleDay>();
			var period = teamBlockInfo.BlockInfo.BlockPeriod;
			var teamBlock1GroupMembers = teamBlockInfo.TeamInfo.GroupMembers.ToList();

			for (var i = 0; i < teamBlock1GroupMembers.Count; i++)
			{
				foreach (var dateOnly in period.DayCollection())
				{
					if(!selectedPeriod.Contains(dateOnly)) continue;
					var person = teamBlock1GroupMembers[i];
					var scheduleDay = scheduleDictionary[person].ScheduledDay(dateOnly);
					scheduleDays.Add(scheduleDay);
				}
			}

			return scheduleDays;
		}
	}
}
