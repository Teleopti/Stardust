using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
	public interface ITeamBlockMissingDayOffHandler
    {
        void Execute(IList<IScheduleMatrixPro> matrixList, SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService);
        event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
    }

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
	public class TeamBlockMissingDayOffHandler : ITeamBlockMissingDayOffHandler
    {
	    private readonly IMissingDayOffBestSpotDecider _missingDayOffBestSpotDecider;
	    private readonly IMatrixDataListCreator _matrixDataListCreator;
        private readonly IMatrixDataWithToFewDaysOff _matrixDataWithToFewDaysOff;
        private readonly ISplitSchedulePeriodToWeekPeriod _splitSchedulePeriodToWeekPeriod;
		
        public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public TeamBlockMissingDayOffHandler(IMissingDayOffBestSpotDecider missingDayOffBestSpotDecider, IMatrixDataListCreator matrixDataListCreator, IMatrixDataWithToFewDaysOff matrixDataWithToFewDaysOff, ISplitSchedulePeriodToWeekPeriod splitSchedulePeriodToWeekPeriod)
        {
			_missingDayOffBestSpotDecider = missingDayOffBestSpotDecider;
			_matrixDataListCreator = matrixDataListCreator;
            _matrixDataWithToFewDaysOff = matrixDataWithToFewDaysOff;
            _splitSchedulePeriodToWeekPeriod = splitSchedulePeriodToWeekPeriod;
        }
        
        public void Execute(IList<IScheduleMatrixPro> matrixList, SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
        {
	        var matrixDataList = _matrixDataListCreator.Create(matrixList, schedulingOptions);
	        var cancel = false;
            IList<IMatrixData> workingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixDataList);
            foreach (var workingItem in workingList)
            {
				if (cancel) return;
                fixThisMatrix(workingItem, schedulingOptions, rollbackService, ()=>cancel=true);
            }
        }

        private void fixThisMatrix(IMatrixData workingItem, SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService, Action cancelAction)
        {
            var tempWorkingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(new List<IMatrixData> { workingItem });
	        var matrix = tempWorkingList[0].Matrix;
			var splitedWeeksFromSchedulePeriod = _splitSchedulePeriodToWeekPeriod.Split(matrix.SchedulePeriod.DateOnlyPeriod, matrix.Person.FirstDayOfWeek);
			var alreadyAnalyzedDates = new List<DateOnly>();
			var cancel = false;
            while (tempWorkingList.Count > 0)
            {
				if (cancel) return;

				var resultingDate = _missingDayOffBestSpotDecider.Find(workingItem, splitedWeeksFromSchedulePeriod, alreadyAnalyzedDates);
				if (!resultingDate.HasValue)
					break;

				var schedulingResult = assignDayOff(resultingDate.Value, tempWorkingList[0], schedulingOptions.DayOffTemplate, rollbackService, () =>
				{
					cancel = true;
					cancelAction();
				});
				if(!schedulingResult)
					alreadyAnalyzedDates.Add(resultingDate.Value);
				else
				{
					IScheduleDayData dayData;
					tempWorkingList[0].TryGetValue(resultingDate.Value, out dayData);
					dayData.IsDayOff = true;
					tempWorkingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(tempWorkingList);
				}    
            }
        }

        private bool assignDayOff(DateOnly date, IMatrixData matrixData, IDayOffTemplate dayOffTemplate, ISchedulePartModifyAndRollbackService rollbackService, Action cancelAction)
        {
            var scheduleDayPro = matrixData.Matrix.GetScheduleDayByKey(date);
            if (!matrixData.Matrix.UnlockedDays.Contains(scheduleDayPro)) return false;
            IScheduleDay scheduleDay = scheduleDayPro.DaySchedulePart();
            scheduleDay.CreateAndAddDayOff(dayOffTemplate);
            rollbackService.Modify(scheduleDay);
            var eventArgs = new SchedulingServiceSuccessfulEventArgs(scheduleDay, cancelAction);
            var progressResult = onDayScheduled(eventArgs);
            if (progressResult.ShouldCancel)
                return false;

            return true;
        }

        private CancelSignal onDayScheduled(SchedulingServiceBaseEventArgs args)
        {
            var handler = DayScheduled;
            if (handler != null)
            {
                handler(this, args);
				if (args.Cancel) return new CancelSignal{ShouldCancel = true};
            }
			return new CancelSignal();
        }
    }
}
