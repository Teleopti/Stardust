using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IMissingDaysOffScheduler
	{
		bool Execute(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService);
	}

	public class MissingDaysOffScheduler : IMissingDaysOffScheduler
	{
		private readonly IBestSpotForAddingDayOffFinder _bestSpotForAddingDayOffFinder;
		private readonly IMatrixDataListInSteadyState _matrixDataListInSteadyState;
		private readonly IMatrixDataListCreator _matrixDataListCreator;
		private readonly IMatrixDataWithToFewDaysOff _matrixDataWithToFewDaysOff;

		public MissingDaysOffScheduler(IBestSpotForAddingDayOffFinder bestSpotForAddingDayOffFinder,
			IMatrixDataListInSteadyState matrixDataListInSteadyState, IMatrixDataListCreator matrixDataListCreator, 
			IMatrixDataWithToFewDaysOff matrixDataWithToFewDaysOff)
		{
			_bestSpotForAddingDayOffFinder = bestSpotForAddingDayOffFinder;
			_matrixDataListInSteadyState = matrixDataListInSteadyState;
			_matrixDataListCreator = matrixDataListCreator;
			_matrixDataWithToFewDaysOff = matrixDataWithToFewDaysOff;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public bool Execute(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
		{
			var matrixDataList = _matrixDataListCreator.Create(matrixList, schedulingOptions);
			bool useSameDaysOffOnAll = _matrixDataListInSteadyState.IsListInSteadyState(matrixDataList);
			IList<IMatrixData> workingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixDataList);
			while (workingList.Count > 0)
			{
				DateOnly? resultingDate = _bestSpotForAddingDayOffFinder.Find(workingList[0].ScheduleDayDataCollection);
				if (!resultingDate.HasValue)
				{
					//rollback?
					return false;
				}
				if (useSameDaysOffOnAll)
				{
					foreach (var matrixData in workingList)
					{
						assignDayOff(resultingDate.Value, matrixData, schedulingOptions.DayOffTemplate, rollbackService);
					}
				}
				else
				{
					assignDayOff(resultingDate.Value, workingList[0], schedulingOptions.DayOffTemplate, rollbackService);
				}

				matrixList = extractMatrixes(workingList);
				matrixDataList = _matrixDataListCreator.Create(matrixList, schedulingOptions);
				workingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixDataList);
			}

			return true;
		}

		private static IList<IScheduleMatrixPro> extractMatrixes(IList<IMatrixData> workingList)
		{
			var ret = new List<IScheduleMatrixPro>();
			foreach (var matrixData in workingList)
			{
				ret.Add(matrixData.Matrix);
			}

			return ret;
		}

		private static void assignDayOff(DateOnly date, IMatrixData matrixData, IDayOffTemplate dayOffTemplate, ISchedulePartModifyAndRollbackService rollbackService)
		{
			IScheduleDay scheduleDay = matrixData.Matrix.GetScheduleDayByKey(date).DaySchedulePart();
			scheduleDay.CreateAndAddDayOff(dayOffTemplate);
			rollbackService.Modify(scheduleDay);
		}
	}
}