using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ClassicLegacy
{
    public class WorkShiftBackToLegalStateServicePro: IWorkShiftBackToLegalStateServicePro
    {
        private readonly IWorkShiftBackToLegalStateStep _workShiftBackToLegalStateStep;
        private readonly IWorkShiftMinMaxCalculator _workShiftRangeCalculator;

		public WorkShiftBackToLegalStateServicePro(
            IWorkShiftBackToLegalStateStep workShiftBackToLegalStateStep,
            IWorkShiftMinMaxCalculator workShiftRangeCalculator)
        {
            _workShiftBackToLegalStateStep = workShiftBackToLegalStateStep;
            _workShiftRangeCalculator = workShiftRangeCalculator;
        }

		public bool Execute(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
        {
            RemovedDays.Clear();
			RemovedSchedules.Clear();
            _workShiftRangeCalculator.ResetCache();

            //for each week
            var weekCount = _workShiftRangeCalculator.WeekCount(matrix);
            for (var weekIndex = 0; weekIndex < weekCount; weekIndex++)
            {
				var skipThisWeek = false;
				if (weekIndex == 0 || weekIndex == weekCount - 1)
					skipThisWeek = new WorkShiftMinMaxCalculatorSkipWeekCheck().SkipWeekCheck(matrix, firstDateInWeekIndex(weekIndex, matrix));
				if(skipThisWeek)
					continue;

                while (!_workShiftRangeCalculator.IsWeekInLegalState(weekIndex, matrix, schedulingOptions))
                {
                    var removedDay = _workShiftBackToLegalStateStep.ExecuteWeekStep(weekIndex, matrix, rollbackService);
                    if (removedDay == null)
                        return false;
                    RemovedDays.Add(removedDay.DateOnlyAsPeriod.DateOnly);
					RemovedSchedules.Add(removedDay);
                }
			}

            while (_workShiftRangeCalculator.PeriodLegalStateStatus(matrix, schedulingOptions) > 0)
            {
				var removedDay = _workShiftBackToLegalStateStep.ExecutePeriodStep(false, matrix, rollbackService);
				if (removedDay == null)
                    return false;
				RemovedDays.Add(removedDay.DateOnlyAsPeriod.DateOnly);
				RemovedSchedules.Add(removedDay);
            }
            return true;
        }

        public IList<DateOnly> RemovedDays { get; } = new List<DateOnly>();

		public IList<IScheduleDay> RemovedSchedules { get; } = new List<IScheduleDay>();

		private static DateOnly firstDateInWeekIndex(int weekIndex, IScheduleMatrixPro matrix)
		{
			return matrix.FullWeeksPeriodDays[weekIndex * 7].Day;
		}
    }
	
	
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_NoWhiteSpotWhenTargetDayoffIsBroken_77941)]
	public class WorkShiftBackToLegalStateServiceProOLD : IWorkShiftBackToLegalStateServicePro
    {
        private readonly IWorkShiftBackToLegalStateStep _workShiftBackToLegalStateStep;
        private readonly IWorkShiftMinMaxCalculator _workShiftRangeCalculator;
        private readonly IList<DateOnly> _removedDays = new List<DateOnly>();
		private readonly IList<IScheduleDay> _removedSchedules = new List<IScheduleDay>();

        public WorkShiftBackToLegalStateServiceProOLD(
            IWorkShiftBackToLegalStateStep workShiftBackToLegalStateStep,
            IWorkShiftMinMaxCalculator workShiftRangeCalculator)
        {
            _workShiftBackToLegalStateStep = workShiftBackToLegalStateStep;
            _workShiftRangeCalculator = workShiftRangeCalculator;
        }

		public bool Execute(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
        {
            _removedDays.Clear();
			_removedSchedules.Clear();
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
                        return false;
                    _removedDays.Add(removedDay.DateOnlyAsPeriod.DateOnly);
					_removedSchedules.Add(removedDay);
                }
            }

            // whole period
            int legalStateStatus;
            while ((legalStateStatus = _workShiftRangeCalculator.PeriodLegalStateStatus(matrix, schedulingOptions)) != 0)
            {
                bool raise = legalStateStatus < 0;
				IScheduleDay removedDay = _workShiftBackToLegalStateStep.ExecutePeriodStep(raise, matrix, rollbackService);
				if (removedDay == null)
                    return false;
				_removedDays.Add(removedDay.DateOnlyAsPeriod.DateOnly);
				_removedSchedules.Add(removedDay);
            }
            return true;
        }

       

        public IList<DateOnly> RemovedDays
        {
            get { return _removedDays; }
        }

    	public IList<IScheduleDay> RemovedSchedules
    	{
			get {return _removedSchedules; }
    	}

		private static DateOnly firstDateInWeekIndex(int weekIndex, IScheduleMatrixPro matrix)
		{
			return matrix.FullWeeksPeriodDays[weekIndex * 7].Day;
		}
    }
}
