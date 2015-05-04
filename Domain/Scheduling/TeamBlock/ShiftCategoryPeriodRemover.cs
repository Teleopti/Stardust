using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IShiftCategoryPeriodRemover
	{
		IList<IScheduleDayPro> RemoveShiftCategoryOnPeriod(IShiftCategoryLimitation limitation, ISchedulingOptions schedulingOptions, IScheduleMatrixPro scheduleMatrixPro, IOptimizationPreferences optimizationPreferences);
	}

	public class ShiftCategoryPeriodRemover : IShiftCategoryPeriodRemover
	{
		private readonly ITeamBlockRemoveShiftCategoryOnBestDateService _teamBlockRemoveShiftCategoryOnBestDateService;

		public ShiftCategoryPeriodRemover(ITeamBlockRemoveShiftCategoryOnBestDateService teamBlockRemoveShiftCategoryOnBestDateService)
		{
			_teamBlockRemoveShiftCategoryOnBestDateService = teamBlockRemoveShiftCategoryOnBestDateService;
		}

		public IList<IScheduleDayPro> RemoveShiftCategoryOnPeriod(IShiftCategoryLimitation limitation, ISchedulingOptions schedulingOptions, IScheduleMatrixPro scheduleMatrixPro, IOptimizationPreferences optimizationPreferences)
		{
			IList<IScheduleDayPro> result = new List<IScheduleDayPro>();
			while (isShiftCategoryOverPeriodLimit(limitation, scheduleMatrixPro))
			{
				var thisResult = _teamBlockRemoveShiftCategoryOnBestDateService.Execute(limitation.ShiftCategory, schedulingOptions, scheduleMatrixPro, scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod, optimizationPreferences);
				if (thisResult != null)
				{
					schedulingOptions.NotAllowedShiftCategories.Add(limitation.ShiftCategory);
					result.Add(thisResult);
				}
				else
				{
					break;
				}
			}

			return result;
		}

		private bool isShiftCategoryOverPeriodLimit(IShiftCategoryLimitation shiftCategoryLimitation, IScheduleMatrixPro scheduleMatrixPro)
		{
			var categoryCounter = 0;

			foreach (var scheduleDay in scheduleMatrixPro.EffectivePeriodDays)
			{
				if (_teamBlockRemoveShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(scheduleDay, shiftCategoryLimitation.ShiftCategory))categoryCounter++;
			}

			return (categoryCounter > shiftCategoryLimitation.MaxNumberOf);
		}
	}
}
