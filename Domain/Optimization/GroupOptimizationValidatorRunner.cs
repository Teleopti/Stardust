using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		private delegate ValidatorResult ValidateDaysOffMoveDelegate(IScheduleMatrixPro matrix, IList<DateOnly> datesToCheck, ISchedulingOptions schedulingOptions);

		public bool Run(IScheduleMatrixPro matrix, IList<DateOnly> daysOffToRemove, IList<DateOnly> daysOffToAdd, ISchedulingOptions schedulingOptions)
		{
			IDictionary<ValidateDaysOffMoveDelegate, IAsyncResult> runnableList = new Dictionary<ValidateDaysOffMoveDelegate, IAsyncResult>();

			ValidateDaysOffMoveDelegate toRun = _groupDayOffOptimizerValidateDayOffToRemove.Validate;
			IAsyncResult result = toRun.BeginInvoke(matrix, daysOffToRemove, schedulingOptions, null, null);
			runnableList.Add(toRun, result);

			toRun = _groupDayOffOptimizerValidateDayOffToAdd.Validate;
			result = toRun.BeginInvoke(matrix, daysOffToAdd, schedulingOptions, null, null);
			runnableList.Add(toRun, result);

			toRun = 

			//Sync all threads
			IList<ValidatorResult> results = new List<ValidatorResult>();
			try
			{
				foreach (KeyValuePair<ValidateDaysOffMoveDelegate, IAsyncResult> thread in runnableList)
				{
					results.Add(thread.Key.EndInvoke(thread.Value));
				}
			}
			catch (Exception e)
			{
				Trace.WriteLine(e.Message);
				throw;
			}

			foreach (var validatorResult in results)
			{
				if (!validatorResult.Success)
					return false;
				if (validatorResult.DaysToLock.HasValue)
				{
					foreach (var matrixPro in validatorResult.MatrixList)
					{
						matrixPro.LockPeriod(validatorResult.DaysToLock.Value);
					}
				}
			}

			return true;
		}
	}

	public class ValidatorResult
	{
		public bool Success { get; set; }
		public DateOnlyPeriod? DaysToLock { get; set; }
		public IList<IScheduleMatrixPro> MatrixList { get; set; }

		public ValidatorResult()
		{
			MatrixList = new List<IScheduleMatrixPro>();
		}
	}
}