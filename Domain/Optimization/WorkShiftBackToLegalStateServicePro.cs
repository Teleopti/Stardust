﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class WorkShiftBackToLegalStateServicePro : IWorkShiftBackToLegalStateServicePro
    {
        private readonly IWorkShiftBackToLegalStateStep _workShiftBackToLegalStateStep;
        private readonly IWorkShiftMinMaxCalculator _workShiftRangeCalculator;
        private readonly IList<DateOnly> _removedDays = new List<DateOnly>();
		private readonly IList<IScheduleDay> _removedSchedules = new List<IScheduleDay>();

        public WorkShiftBackToLegalStateServicePro(
            IWorkShiftBackToLegalStateStep workShiftBackToLegalStateStep,
            IWorkShiftMinMaxCalculator workShiftRangeCalculator)
        {
            _workShiftBackToLegalStateStep = workShiftBackToLegalStateStep;
            _workShiftRangeCalculator = workShiftRangeCalculator;
        }

		public bool Execute(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
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
					skipThisWeek = skipWeekCheck(matrix, firstDateInWeekIndex(weekIndex, matrix));

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

		private static bool skipWeekCheck(IScheduleMatrixPro matrix, DateOnly dateToCheck)
		{
			var contract = matrix.SchedulePeriod.Contract;
			var weekPeriod = DateHelper.GetWeekPeriod(dateToCheck, matrix.Person.FirstDayOfWeek);
			IPersonPeriod period = matrix.Person.Period(matrix.SchedulePeriod.DateOnlyPeriod.StartDate);

			if (weekPeriod.Contains(matrix.SchedulePeriod.DateOnlyPeriod.StartDate.AddDays(-1)))
			{
				IPersonPeriod previousPeriod = matrix.Person.PreviousPeriod(period);
				if (previousPeriod != null)
				{
					if (contract.WorkTimeDirective.MaxTimePerWeek != previousPeriod.PersonContract.Contract.WorkTimeDirective.MaxTimePerWeek)
						return true;
				}

				IVirtualSchedulePeriod schedulePeriod =
					matrix.Person.VirtualSchedulePeriod(matrix.SchedulePeriod.DateOnlyPeriod.StartDate.AddDays(-1));
				if (!schedulePeriod.IsValid)
					return true;
			}
			if (weekPeriod.Contains(matrix.SchedulePeriod.DateOnlyPeriod.EndDate.AddDays(1)))
			{
				IPersonPeriod nextPeriod = matrix.Person.NextPeriod(period);
				if (nextPeriod != null)
				{
					if (contract.WorkTimeDirective.MaxTimePerWeek != nextPeriod.PersonContract.Contract.WorkTimeDirective.MaxTimePerWeek)
						return true;
				}
			}

			return false;
		}

		private static DateOnly firstDateInWeekIndex(int weekIndex, IScheduleMatrixPro matrix)
		{
			return matrix.FullWeeksPeriodDays[weekIndex * 7].Day;
		}
    }
}
