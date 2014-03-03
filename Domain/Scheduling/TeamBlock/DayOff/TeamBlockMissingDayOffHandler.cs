using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff
{
    public interface ITeamBlockMissingDayOffHandler
    {
        void Execute(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService);
        event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
    }

    public class TeamBlockMissingDayOffHandler : ITeamBlockMissingDayOffHandler
    {
        private readonly IBestSpotForAddingDayOffFinder _bestSpotForAddingDayOffFinder;
        private readonly IMatrixDataListCreator _matrixDataListCreator;
        private readonly IMatrixDataWithToFewDaysOff _matrixDataWithToFewDaysOff;
        private readonly SplitSchedulePeriodToWeekPeriod _splitSchedulePeriodToWeekPeriod;
        private readonly IValidNumberOfDayOffInAWeekSpecification _validNumberOfDayOffInAWeekSpecification;

        public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

        public TeamBlockMissingDayOffHandler(IBestSpotForAddingDayOffFinder bestSpotForAddingDayOffFinder, IMatrixDataListCreator matrixDataListCreator, IMatrixDataWithToFewDaysOff matrixDataWithToFewDaysOff, SplitSchedulePeriodToWeekPeriod splitSchedulePeriodToWeekPeriod, IValidNumberOfDayOffInAWeekSpecification validNumberOfDayOffInAWeekSpecification)
        {
            _bestSpotForAddingDayOffFinder = bestSpotForAddingDayOffFinder;
            _matrixDataListCreator = matrixDataListCreator;
            _matrixDataWithToFewDaysOff = matrixDataWithToFewDaysOff;
            _splitSchedulePeriodToWeekPeriod = splitSchedulePeriodToWeekPeriod;
            _validNumberOfDayOffInAWeekSpecification = validNumberOfDayOffInAWeekSpecification;
        }
        
        public void Execute(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
        {
            var matrixDataList = _matrixDataListCreator.Create(matrixList, schedulingOptions);
            IList<IMatrixData> workingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixDataList);
            foreach (var workingItem in workingList)
            {
                fixThisMatrix(workingItem, schedulingOptions, rollbackService);
            }
        }

        private void fixThisMatrix(IMatrixData workingItem, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
        {
            var tempWorkingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(new List<IMatrixData> { workingItem });
            var splitedWeeksFromSchedulePeriod = _splitSchedulePeriodToWeekPeriod.Split(tempWorkingList[0].Matrix.SchedulePeriod.DateOnlyPeriod);
            int averageNumberOfDayOffPerWeek = tempWorkingList[0].Matrix.SchedulePeriod.DaysOff() / splitedWeeksFromSchedulePeriod.Count ;
            var alreadyAnalyzedDates = new List<DateOnly>();
            while (tempWorkingList.Count > 0)
            {
                var resultingDateFound = true;
                var schedulingResult = true;
                foreach (var weekPeriod in splitedWeeksFromSchedulePeriod)
                {
                    if (_validNumberOfDayOffInAWeekSpecification.IsSatisfied(tempWorkingList[0].Matrix, weekPeriod, averageNumberOfDayOffPerWeek)) continue  ;
                    resultingDateFound = true;
                    var scheduleDayCollection = getScheduleDayDataBasedOnPeriod(tempWorkingList[0].ScheduleDayDataCollection, weekPeriod,alreadyAnalyzedDates);
                    DateOnly? resultingDate = _bestSpotForAddingDayOffFinder.Find(scheduleDayCollection);
                    if (!resultingDate.HasValue)
                    {
                        resultingDateFound = false ;
                        continue;
                    }
                    alreadyAnalyzedDates.Add(resultingDate.Value);
                    schedulingResult = assignDayOff(resultingDate.Value, tempWorkingList[0], schedulingOptions.DayOffTemplate, rollbackService);
                    if (!_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(tempWorkingList).Any()) break;
                        
                }
                if (!resultingDateFound || !schedulingResult) break;
                tempWorkingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(tempWorkingList);
            }
        }

        private IList<IScheduleDayData> getScheduleDayDataBasedOnPeriod(IEnumerable<IScheduleDayData> scheduleDayDataCollection, DateOnlyPeriod weekPeriod, List<DateOnly> alreadyAnalyzedDates)
        {
            var filteredList = new List<IScheduleDayData>();
            foreach (var scheduleDayData in scheduleDayDataCollection)
            {
                var scheduleDayDate = scheduleDayData.DateOnly;
                if (weekPeriod.Contains(scheduleDayDate) && !alreadyAnalyzedDates.Contains(scheduleDayDate))
                    filteredList.Add(scheduleDayData );
            }
            return filteredList;
        }


        private bool assignDayOff(DateOnly date, IMatrixData matrixData, IDayOffTemplate dayOffTemplate, ISchedulePartModifyAndRollbackService rollbackService)
        {
            var scheduleDayPro = matrixData.Matrix.GetScheduleDayByKey(date);
            if (!matrixData.Matrix.UnlockedDays.Contains(scheduleDayPro)) return false;
            IScheduleDay scheduleDay = scheduleDayPro.DaySchedulePart();
            scheduleDay.CreateAndAddDayOff(dayOffTemplate);
            rollbackService.Modify(scheduleDay);
            var eventArgs = new SchedulingServiceSuccessfulEventArgs(scheduleDay);
            OnDayScheduled(eventArgs);
            if (eventArgs.Cancel)
                return false;

            return true;
        }

        protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
        {
            EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
            if (temp != null)
            {
                temp(this, scheduleServiceBaseEventArgs);
            }
        }
    }
}
