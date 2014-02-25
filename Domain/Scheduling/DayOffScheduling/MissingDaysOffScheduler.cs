using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IMissingDaysOffScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		bool Execute(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService);
	}

	public class MissingDaysOffScheduler : IMissingDaysOffScheduler
	{
		private readonly IBestSpotForAddingDayOffFinder _bestSpotForAddingDayOffFinder;
		private readonly IMatrixDataListInSteadyState _matrixDataListInSteadyState;
		private readonly IMatrixDataListCreator _matrixDataListCreator;
		private readonly IMatrixDataWithToFewDaysOff _matrixDataWithToFewDaysOff;

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

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
						var result = assignDayOff(resultingDate.Value, matrixData, schedulingOptions.DayOffTemplate, rollbackService);
						if (!result)
							return false;
					}
				}
				else
				{
					var result = assignDayOff(resultingDate.Value, workingList[0], schedulingOptions.DayOffTemplate, rollbackService);
					if (!result)
						return false;
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

		private bool assignDayOff(DateOnly date, IMatrixData matrixData, IDayOffTemplate dayOffTemplate, ISchedulePartModifyAndRollbackService rollbackService)
		{
            var scheduleDayPro = matrixData.Matrix.GetScheduleDayByKey(date);
            if (!matrixData.Matrix.UnlockedDays.Contains(scheduleDayPro)) return false;
            IScheduleDay scheduleDay = scheduleDayPro.DaySchedulePart();
            scheduleDay.CreateAndAddDayOff(dayOffTemplate);

			var personAssignment = scheduleDay.PersonAssignment();
			var authorization = PrincipalAuthorization.Instance();
			if (!(authorization.IsPermitted(personAssignment.FunctionPath, scheduleDay.DateOnlyAsPeriod.DateOnly, scheduleDay.Person))) return false;

			rollbackService.Modify(scheduleDay); var eventArgs = new SchedulingServiceSuccessfulEventArgs(scheduleDay);
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