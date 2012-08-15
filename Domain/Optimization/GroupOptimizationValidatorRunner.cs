using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupOptimizationValidatorRunner
	{
		bool Run(IScheduleMatrixPro matrix, IList<DateOnly> daysOffToRemove, IList<DateOnly> daysOffToAdd, ISchedulingOptions schedulingOptions);
	}

	public class GroupOptimizationValidatorRunner : IGroupOptimizationValidatorRunner
	{
		private readonly IGroupDayOffOptimizerValidateDayOffToRemove _groupDayOffOptimizerValidateDayOffToRemove;
		private readonly IGroupDayOffOptimizerValidateDayOffToAdd _groupDayOffOptimizerValidateDayOffToAdd;

		public GroupOptimizationValidatorRunner(IGroupDayOffOptimizerValidateDayOffToRemove groupDayOffOptimizerValidateDayOffToRemove, 
			IGroupDayOffOptimizerValidateDayOffToAdd groupDayOffOptimizerValidateDayOffToAdd)
		{
			_groupDayOffOptimizerValidateDayOffToRemove = groupDayOffOptimizerValidateDayOffToRemove;
			_groupDayOffOptimizerValidateDayOffToAdd = groupDayOffOptimizerValidateDayOffToAdd;
		}

		private delegate void ValidateDaysOffRemoveDelegate(IScheduleMatrixPro matrix, DateOnly dateOnly, ISchedulingOptions schedulingOptions);

		public bool Run(IScheduleMatrixPro matrix, IList<DateOnly> daysOffToRemove, IList<DateOnly> daysOffToAdd, ISchedulingOptions schedulingOptions)
		{
			IDictionary<ValidateDaysOffRemoveDelegate, IAsyncResult> runnableList = new Dictionary<ValidateDaysOffRemoveDelegate, IAsyncResult>();
			bool success =_groupDayOffOptimizerValidateDayOffToRemove.Validate(matrix, daysOffToRemove, schedulingOptions);
			if (!success)
				return false;
			success = _groupDayOffOptimizerValidateDayOffToAdd.Validate(matrix, daysOffToAdd, schedulingOptions);
			if (!success)
				return false;

			return true;
		}
	}
}