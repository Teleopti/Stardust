using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IMissingDaysOffScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		bool Execute(IList<IScheduleMatrixPro> matrixList, SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService);
	}

	public class MissingDaysOffScheduler : IMissingDaysOffScheduler
	{
		private readonly IBestSpotForAddingDayOffFinder _bestSpotForAddingDayOffFinder;
		private readonly IMatrixDataListInSteadyState _matrixDataListInSteadyState;
		private readonly IMatrixDataListCreator _matrixDataListCreator;
		private readonly IMatrixDataWithToFewDaysOff _matrixDataWithToFewDaysOff;
		private readonly ICurrentAuthorization _authorization;

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public MissingDaysOffScheduler(IBestSpotForAddingDayOffFinder bestSpotForAddingDayOffFinder,
			IMatrixDataListInSteadyState matrixDataListInSteadyState, IMatrixDataListCreator matrixDataListCreator, 
			IMatrixDataWithToFewDaysOff matrixDataWithToFewDaysOff, ICurrentAuthorization authorization)
		{
			_bestSpotForAddingDayOffFinder = bestSpotForAddingDayOffFinder;
			_matrixDataListInSteadyState = matrixDataListInSteadyState;
			_matrixDataListCreator = matrixDataListCreator;
			_matrixDataWithToFewDaysOff = matrixDataWithToFewDaysOff;
			_authorization = authorization;
		}

		public bool Execute(IList<IScheduleMatrixPro> matrixList, SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
		{
			var cancel = false;
			var matrixDataList = _matrixDataListCreator.Create(matrixList, schedulingOptions);
			var useSameDaysOffOnAll = _matrixDataListInSteadyState.IsListInSteadyState(matrixDataList);
			var workingList = _matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixDataList);
			Action cancelAction = () => cancel = true;
			while (workingList.Count > 0)
			{
				if (cancel) return false;
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
						if (cancel) return false;
						var result = assignDayOff(resultingDate.Value, matrixData, schedulingOptions.DayOffTemplate, rollbackService, cancelAction);
						if (!result)
							return false;
					}
				}
				else
				{
					var result = assignDayOff(resultingDate.Value, workingList[0], schedulingOptions.DayOffTemplate, rollbackService, cancelAction);
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

		private bool assignDayOff(DateOnly date, IMatrixData matrixData, IDayOffTemplate dayOffTemplate, ISchedulePartModifyAndRollbackService rollbackService, Action cancelAction)
		{
            var scheduleDayPro = matrixData.Matrix.GetScheduleDayByKey(date);
            if (!matrixData.Matrix.UnlockedDays.Contains(scheduleDayPro)) return false;
            IScheduleDay scheduleDay = scheduleDayPro.DaySchedulePart();
            scheduleDay.CreateAndAddDayOff(dayOffTemplate);

			var personAssignment = scheduleDay.PersonAssignment();
			if (!_authorization.Current().IsPermitted(personAssignment.FunctionPath, scheduleDay.DateOnlyAsPeriod.DateOnly, scheduleDay.Person)) return false;

			rollbackService.Modify(scheduleDay); var eventArgs = new SchedulingServiceSuccessfulEventArgs(scheduleDay,cancelAction);
            OnDayScheduled(eventArgs);
            if (eventArgs.Cancel)
                return false;

            return true;
		}

		protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs args)
		{
			DayScheduled?.Invoke(this, args);
		}
	}
}