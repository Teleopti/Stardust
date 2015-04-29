using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IShiftCategoryPeriodRemover
	{
		IList<IScheduleDayPro> RemoveShiftCategoryOnPeriod(IShiftCategoryLimitation limitation, ISchedulingOptions schedulingOptions, IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator, IScheduleMatrixPro scheduleMatrixPro);
	}

	public class ShiftCategoryPeriodRemover : IShiftCategoryPeriodRemover
	{
		private readonly ITeamBlockRemoveShiftCategoryOnBestDateService _teamBlockRemoveShiftCategoryOnBestDateService;

		public ShiftCategoryPeriodRemover(ITeamBlockRemoveShiftCategoryOnBestDateService teamBlockRemoveShiftCategoryOnBestDateService)
		{
			_teamBlockRemoveShiftCategoryOnBestDateService = teamBlockRemoveShiftCategoryOnBestDateService;
		}

		public IList<IScheduleDayPro> RemoveShiftCategoryOnPeriod(IShiftCategoryLimitation limitation, ISchedulingOptions schedulingOptions, IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator, IScheduleMatrixPro scheduleMatrixPro)
		{

			IList<IScheduleDayPro> periodDays = new List<IScheduleDayPro>(scheduleMatrixPro.EffectivePeriodDays);
            var start = periodDays[0].Day;
            var end = periodDays[periodDays.Count - 1].Day;
            var period = new DateOnlyPeriod(start, end);

			IList<IScheduleDayPro> result = new List<IScheduleDayPro>();
			while (isShiftCategoryOverPeriodLimit(limitation, scheduleMatrixPro))
			{
				var thisResult = _teamBlockRemoveShiftCategoryOnBestDateService.Execute(limitation.ShiftCategory, schedulingOptions, scheduleMatrixValueCalculator, scheduleMatrixPro, period);
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
