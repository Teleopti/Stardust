using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff
{
	public class TeamBlockMissingDayOffHandling
	{
		private readonly IMissingDayOffBestSpotDecider _missingDayOffBestSpotDecider;
		private readonly MatrixDataListCreator _matrixDataListCreator;
		private readonly IMatrixDataWithToFewDaysOff _matrixDataWithToFewDaysOff;
		private readonly ISplitSchedulePeriodToWeekPeriod _splitSchedulePeriodToWeekPeriod;

		public TeamBlockMissingDayOffHandling(IMissingDayOffBestSpotDecider missingDayOffBestSpotDecider, MatrixDataListCreator matrixDataListCreator, IMatrixDataWithToFewDaysOff matrixDataWithToFewDaysOff, ISplitSchedulePeriodToWeekPeriod splitSchedulePeriodToWeekPeriod)
		{
			_missingDayOffBestSpotDecider = missingDayOffBestSpotDecider;
			_matrixDataListCreator = matrixDataListCreator;
			_matrixDataWithToFewDaysOff = matrixDataWithToFewDaysOff;
			_splitSchedulePeriodToWeekPeriod = splitSchedulePeriodToWeekPeriod;
		}

		public void Execute(ISchedulingCallback schedulingCallback, IList<IScheduleMatrixPro> matrixList, SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
		{
			var matrixDataList = _matrixDataListCreator.Create(matrixList, schedulingOptions);
			IList<IMatrixData> workingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixDataList);
			foreach (var workingItem in workingList)
			{
				if (schedulingCallback.IsCancelled) return;
				fixThisMatrix(schedulingCallback, workingItem, schedulingOptions, rollbackService);
			}
		}

		private void fixThisMatrix(ISchedulingCallback schedulingCallback, IMatrixData workingItem, SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
		{
			var tempWorkingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(new List<IMatrixData> { workingItem });
			var matrix = tempWorkingList[0].Matrix;
			var splitedWeeksFromSchedulePeriod = _splitSchedulePeriodToWeekPeriod.Split(matrix.SchedulePeriod.DateOnlyPeriod, matrix.Person.FirstDayOfWeek);
			var alreadyAnalyzedDates = new List<DateOnly>();
			while (tempWorkingList.Count > 0)
			{
				if (schedulingCallback.IsCancelled) return;

				var resultingDate = _missingDayOffBestSpotDecider.Find(workingItem, splitedWeeksFromSchedulePeriod, alreadyAnalyzedDates);
				if (!resultingDate.HasValue)
					break;

				var schedulingResult = assignDayOff(schedulingCallback, resultingDate.Value, tempWorkingList[0], schedulingOptions.DayOffTemplate, rollbackService);
				if (!schedulingResult)
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

		private bool assignDayOff(ISchedulingCallback schedulingCallback, DateOnly date, IMatrixData matrixData, IDayOffTemplate dayOffTemplate, ISchedulePartModifyAndRollbackService rollbackService)
		{
			var scheduleDayPro = matrixData.Matrix.GetScheduleDayByKey(date);
			if (!matrixData.Matrix.UnlockedDays.Contains(scheduleDayPro)) return false;
			IScheduleDay scheduleDay = scheduleDayPro.DaySchedulePart();
			scheduleDay.CreateAndAddDayOff(dayOffTemplate);
			rollbackService.Modify(scheduleDay);
			schedulingCallback.Scheduled(new SchedulingCallbackInfo(scheduleDay, true));
			if (schedulingCallback.IsCancelled)
				return false;

			return true;
		}
	}
}