using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
	        var cancel = false;
            var matrixDataList = _matrixDataListCreator.Create(matrixList, schedulingOptions);
            IList<IMatrixData> workingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixDataList);
            foreach (var workingItem in workingList)
            {
				if (cancel) return;
                fixThisMatrix(workingItem,schedulingOptions,rollbackService, ()=> cancel=true);
            }
        }

        private void fixThisMatrix(IMatrixData workingItem, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService, Action cancelAction)
        {
            var tempWorkingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(workingItem);
            var scheduleDayCollection = tempWorkingList[0].ScheduleDayDataCollection;
            var startIndex = 0;
            var endIndex = 6;
	        var cancel = false;
            var alreadyAnalyzedDates = new List<DateOnly>();
            while (tempWorkingList.Count > 0)
            {
				if (cancel) return;
                if (startIndex >= scheduleDayCollection.Count)
                {
                    startIndex = 0;
                    endIndex = 6;
                }
                var newScheduleDayCollection = trimTheList(scheduleDayCollection, startIndex, endIndex, alreadyAnalyzedDates);
                startIndex = endIndex + 1;
                endIndex = endIndex + 7;
                DateOnly? resultingDate = _bestSpotForAddingDayOffFinder.Find(newScheduleDayCollection);
                if (!resultingDate.HasValue) break ;
                alreadyAnalyzedDates.Add(resultingDate.Value );
                var result = assignDayOff(resultingDate.Value, tempWorkingList[0], schedulingOptions.DayOffTemplate, rollbackService,
	                () =>
	                {
		                cancel = true;
		                cancelAction();
	                });
                if (!result) break;
                tempWorkingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(tempWorkingList.ToArray());
            }
        }

        private IList<IScheduleDayData> trimTheList(ReadOnlyCollection<IScheduleDayData> scheduleDayCollection, int startIndex, int endIndex, List<DateOnly> alreadyAnalyzedDates)
        {
            var result = new List<IScheduleDayData>();
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (i >= scheduleDayCollection.Count) break;
                if (alreadyAnalyzedDates.Contains(scheduleDayCollection[i].DateOnly)) continue;
                result.Add(scheduleDayCollection[i]);
            }
            return result;
        }

        private bool assignDayOff(DateOnly date, IMatrixData matrixData, IDayOffTemplate dayOffTemplate, ISchedulePartModifyAndRollbackService rollbackService, Action cancelAction)
        {
            var scheduleDayPro = matrixData.Matrix.GetScheduleDayByKey(date);
            if (!matrixData.Matrix.UnlockedDays.Contains(scheduleDayPro)) return false;
            IScheduleDay scheduleDay = scheduleDayPro.DaySchedulePart();
            scheduleDay.CreateAndAddDayOff(dayOffTemplate);
            rollbackService.Modify(scheduleDay); 
            var eventArgs = new SchedulingServiceSuccessfulEventArgs(scheduleDay, cancelAction);
            OnDayScheduled(eventArgs);
            if (eventArgs.Cancel)
                return false;

            return true;
        }

        protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs args)
        {
            var handler = DayScheduled;
            if (handler != null)
            {
                handler(this, args);
            }
        }
    }
}
