using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization.ClassicLegacy
{
	public class WorkShiftBackToLegalStateServicePro : IWorkShiftBackToLegalStateServicePro
    {
        private readonly IWorkShiftBackToLegalStateStep _workShiftBackToLegalStateStep;
        private readonly IWorkShiftMinMaxCalculator _workShiftRangeCalculator;
        private readonly IList<DateOnly> _removedDays = new List<DateOnly>();

        public WorkShiftBackToLegalStateServicePro(
            IWorkShiftBackToLegalStateStep workShiftBackToLegalStateStep,
            IWorkShiftMinMaxCalculator workShiftRangeCalculator)
        {
            _workShiftBackToLegalStateStep = workShiftBackToLegalStateStep;
            _workShiftRangeCalculator = workShiftRangeCalculator;
        }

		public void Execute(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
        {
            _removedDays.Clear();
            _workShiftRangeCalculator.ResetCache();

            //for each week
            int weekCount = _workShiftRangeCalculator.WeekCount(matrix);
            for (int weekIndex = 0; weekIndex < weekCount; weekIndex++)
            {
				bool skipThisWeek = false;
				if (weekIndex == 0 || weekIndex == weekCount - 1)
					skipThisWeek = new WorkShiftMinMaxCalculatorSkipWeekCheck().SkipWeekCheck(matrix, firstDateInWeekIndex(weekIndex, matrix));
				if(skipThisWeek)
					continue;

                while (!_workShiftRangeCalculator.IsWeekInLegalState(weekIndex, matrix, schedulingOptions))
                {
                    IScheduleDay removedDay = _workShiftBackToLegalStateStep.ExecuteWeekStep(weekIndex, matrix, rollbackService);
                    if (removedDay == null)
                        return;
                    _removedDays.Add(removedDay.DateOnlyAsPeriod.DateOnly);
                }
            }

            // whole period
            int legalStateStatus;
            while ((legalStateStatus = _workShiftRangeCalculator.PeriodLegalStateStatus(matrix, schedulingOptions)) != 0)
            {
                bool raise = legalStateStatus < 0;
				IScheduleDay removedDay = _workShiftBackToLegalStateStep.ExecutePeriodStep(raise, matrix, rollbackService);
				if (removedDay == null)
                    return;
				_removedDays.Add(removedDay.DateOnlyAsPeriod.DateOnly);
            }
        }
        
        public IList<DateOnly> RemovedDays
        {
            get { return _removedDays; }
        }

		private static DateOnly firstDateInWeekIndex(int weekIndex, IScheduleMatrixPro matrix)
		{
			return matrix.FullWeeksPeriodDays[weekIndex * 7].Day;
		}
    }
}
