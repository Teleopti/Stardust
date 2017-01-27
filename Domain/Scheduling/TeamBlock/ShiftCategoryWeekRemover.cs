﻿
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IShiftCategoryWeekRemover
	{
		IList<IScheduleDayPro> Remove(IShiftCategoryLimitation shiftCategoryLimitation, ISchedulingOptions schedulingOptions, IScheduleMatrixPro scheduleMatrixPro, IOptimizationPreferences optimizationPreferences);
	}

	public class ShiftCategoryWeekRemover : IShiftCategoryWeekRemover
	{
		private readonly ITeamBlockRemoveShiftCategoryOnBestDateService _teamBlockRemoveShiftCategoryOnBestDateService;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;

		public ShiftCategoryWeekRemover(ITeamBlockRemoveShiftCategoryOnBestDateService teamBlockRemoveShiftCategoryOnBestDateService, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
		{
			_teamBlockRemoveShiftCategoryOnBestDateService = teamBlockRemoveShiftCategoryOnBestDateService;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
		}

		public IList<IScheduleDayPro> Remove(IShiftCategoryLimitation shiftCategoryLimitation, ISchedulingOptions schedulingOptions,  IScheduleMatrixPro scheduleMatrixPro, IOptimizationPreferences optimizationPreferences)
		{
			IList<IScheduleDayPro> days = scheduleMatrixPro.FullWeeksPeriodDays;
			IList<IScheduleDayPro> result = new List<IScheduleDayPro>();
			for (var o = 0; o < days.Count; o += 7)
			{
				var categoryCounter = 0;
				for (var i = 0; i < 7; i++)
				{
					if (_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(days[o + i], shiftCategoryLimitation.ShiftCategory)) categoryCounter++;
				}
				while (categoryCounter > shiftCategoryLimitation.MaxNumberOf)
				{
					var periodForWeek = new DateOnlyPeriod(days[o].Day, days[o].Day.AddDays(6));
					var thisResult = _teamBlockRemoveShiftCategoryOnBestDateService.Execute(shiftCategoryLimitation.ShiftCategory, schedulingOptions, scheduleMatrixPro, periodForWeek, optimizationPreferences, _schedulePartModifyAndRollbackService);

					if (thisResult != null) result.Add(thisResult);
					else break;
					
					categoryCounter = 0;
					for (var i = 0; i < 7; i++)
					{
						if (_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(days[o + i], shiftCategoryLimitation.ShiftCategory)) categoryCounter++;
					}
				}
			}

			return result;	
		}


	}
}
