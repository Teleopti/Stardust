using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupOptimizationValidatorRunner
	{
		bool Run(IScheduleMatrixPro matrix, IList<DateOnly> daysOffToRemove, ISchedulingOptions schedulingOptions);
	}

	public class GroupOptimizationValidatorRunner : IGroupOptimizationValidatorRunner
	{
		private readonly IGroupDayOffOptimizerValidateDayOffToRemove _groupDayOffOptimizerValidateDayOffToRemove;

		public GroupOptimizationValidatorRunner(IGroupDayOffOptimizerValidateDayOffToRemove groupDayOffOptimizerValidateDayOffToRemove)
		{
			_groupDayOffOptimizerValidateDayOffToRemove = groupDayOffOptimizerValidateDayOffToRemove;
		}

		private delegate void ValidateDaysOffRemoveDelegate(IScheduleMatrixPro matrix, DateOnly dateOnly, ISchedulingOptions schedulingOptions);

		public bool Run(IScheduleMatrixPro matrix, IList<DateOnly> daysOffToRemove, ISchedulingOptions schedulingOptions)
		{
			IDictionary<ValidateDaysOffRemoveDelegate, IAsyncResult> runnableList = new Dictionary<ValidateDaysOffRemoveDelegate, IAsyncResult>();
			return _groupDayOffOptimizerValidateDayOffToRemove.Validate(matrix, daysOffToRemove[0], schedulingOptions);
		}
	}
}