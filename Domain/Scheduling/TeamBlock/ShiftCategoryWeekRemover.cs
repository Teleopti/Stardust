using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class ShiftCategoryWeekRemover
	{
		private readonly TeamBlockRemoveShiftCategoryOnBestDateService _teamBlockRemoveShiftCategoryOnBestDateService;

		public ShiftCategoryWeekRemover(TeamBlockRemoveShiftCategoryOnBestDateService teamBlockRemoveShiftCategoryOnBestDateService)
		{
			_teamBlockRemoveShiftCategoryOnBestDateService = teamBlockRemoveShiftCategoryOnBestDateService;
		}

		public IList<IScheduleDayPro> Remove(IShiftCategoryLimitation shiftCategoryLimitation, SchedulingOptions schedulingOptions,  IScheduleMatrixPro scheduleMatrixPro, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
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
					var thisResult = _teamBlockRemoveShiftCategoryOnBestDateService.Execute(shiftCategoryLimitation.ShiftCategory, schedulingOptions, scheduleMatrixPro, periodForWeek, schedulePartModifyAndRollbackService);

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
