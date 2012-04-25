using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
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

        public bool Execute(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
        {
            _removedDays.Clear();
            _workShiftRangeCalculator.ResetCache();

            //for each week
            int weekCount = _workShiftRangeCalculator.WeekCount(matrix);
            for (int weekIndex = 0; weekIndex < weekCount; weekIndex++)
            {
                while (!_workShiftRangeCalculator.IsWeekInLegalState(weekIndex, matrix, schedulingOptions))
                {
                    DateOnly? removedDay = _workShiftBackToLegalStateStep.ExecuteWeekStep(weekIndex, matrix);
                    if (!removedDay.HasValue)
                        return false;
                    _removedDays.Add(removedDay.Value);
                }
            }

            // whole period
            int legalStateStatus;
            while ((legalStateStatus = _workShiftRangeCalculator.PeriodLegalStateStatus(matrix, schedulingOptions)) != 0)
            {
                bool raise = legalStateStatus < 0;
                DateOnly? removedDay = _workShiftBackToLegalStateStep.ExecutePeriodStep(raise, matrix);
                if (!removedDay.HasValue)
                    return false;
                _removedDays.Add(removedDay.Value);
            }
            return true;
        }

        //public bool Execute( IScheduleMatrixPro matrix)
        //{
        //    _removedDays.Clear();
        //    _workShiftRangeCalculator.ResetCache();
            
        //    //for each week
        //    int weekCount = _workShiftRangeCalculator.WeekCount(matrix);
        //    for (int weekIndex = 0; weekIndex < weekCount; weekIndex++)
        //    {
        //        while(!_workShiftRangeCalculator.IsWeekInLegalState(weekIndex, matrix))
        //        {
        //            DateOnly? removedDay = _workShiftBackToLegalStateStep.ExecuteWeekStep(weekIndex, matrix);
        //            if (!removedDay.HasValue)
        //                return false;
        //            _removedDays.Add(removedDay.Value);
        //        }
        //    }

        //    // whole period
        //    int legalStateStatus;
        //    while ((legalStateStatus = _workShiftRangeCalculator.PeriodLegalStateStatus(matrix)) != 0)
        //    {
        //        bool raise = legalStateStatus < 0;
        //        DateOnly? removedDay = _workShiftBackToLegalStateStep.ExecutePeriodStep(raise, matrix);
        //        if (!removedDay.HasValue)
        //            return false;
        //        _removedDays.Add(removedDay.Value);
        //    }
        //    return true;
        //}

        public IList<DateOnly> RemovedDays
        {
            get { return _removedDays; }
        }
    }
}
