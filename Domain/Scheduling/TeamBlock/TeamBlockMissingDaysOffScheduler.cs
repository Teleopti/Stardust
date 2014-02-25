using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ITeamBlockMissingDaysOffScheduler
    {
        event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
        void Execute(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService);
    }

    public class TeamBlockMissingDaysOffScheduler : ITeamBlockMissingDaysOffScheduler
    {
        private readonly IBestSpotForAddingDayOffFinder _bestSpotForAddingDayOffFinder;
        private readonly IMatrixDataListCreator _matrixDataListCreator;
        private readonly IMatrixDataWithToFewDaysOff _matrixDataWithToFewDaysOff;

        public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

        public TeamBlockMissingDaysOffScheduler(IBestSpotForAddingDayOffFinder bestSpotForAddingDayOffFinder, IMatrixDataListCreator matrixDataListCreator,
            IMatrixDataWithToFewDaysOff matrixDataWithToFewDaysOff)
        {
            _bestSpotForAddingDayOffFinder = bestSpotForAddingDayOffFinder;
            _matrixDataListCreator = matrixDataListCreator;
            _matrixDataWithToFewDaysOff = matrixDataWithToFewDaysOff;
        }

        public void  Execute(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
        {
            var matrixDataList = _matrixDataListCreator.Create(matrixList, schedulingOptions);
            IList<IMatrixData> workingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixDataList);
            foreach (var workingItem in workingList)
            {
                fixThisMatrix(workingItem,schedulingOptions,rollbackService );
            }

        }

        private void fixThisMatrix(IMatrixData workingItem, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
        {
            var tempWorkingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(new List<IMatrixData>( ){workingItem });
            var scheduleDayCollection = tempWorkingList[0].ScheduleDayDataCollection;
            while (tempWorkingList.Count > 0)
            {
                DateOnly? resultingDate = _bestSpotForAddingDayOffFinder.Find(scheduleDayCollection);
                if (!resultingDate.HasValue) break ;
                var result = assignDayOff(resultingDate.Value, tempWorkingList[0], schedulingOptions.DayOffTemplate, rollbackService);
                if (!result) break;
                tempWorkingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(tempWorkingList);
                scheduleDayCollection= new ReadOnlyCollection<IScheduleDayData>(removeDateFromCollection(scheduleDayCollection.ToList(), resultingDate.Value));
            }
        }

        private List<IScheduleDayData> removeDateFromCollection(List<IScheduleDayData> scheduleDayCollection, DateOnly value)
        {
            var scheduleDay = scheduleDayCollection.FirstOrDefault(x => x.DateOnly.Equals(value));
            scheduleDayCollection.Remove(scheduleDay);
            return scheduleDayCollection;
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
