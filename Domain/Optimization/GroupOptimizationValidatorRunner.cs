using System;
using System.Collections.Generic;
using System.Diagnostics;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IGroupOptimizationValidatorRunner
	{
		bool Run(IPerson person, IList<DateOnly> daysOffToRemove, IList<DateOnly> daysOffToAdd, bool useSameDaysOff);
	}

	public class GroupOptimizationValidatorRunner : IGroupOptimizationValidatorRunner
	{
		private readonly IGroupDayOffOptimizerValidateDayOffToRemove _groupDayOffOptimizerValidateDayOffToRemove;
		private readonly IGroupDayOffOptimizerValidateDayOffToAdd _groupDayOffOptimizerValidateDayOffToAdd;
		private readonly IGroupOptimizerValidateProposedDatesInSameMatrix _groupOptimizerValidateProposedDatesInSameMatrix;
		private readonly IGroupOptimizerValidateProposedDatesInSameGroup _groupOptimizerValidateProposedDatesInSameGroup;

		public GroupOptimizationValidatorRunner(IGroupDayOffOptimizerValidateDayOffToRemove groupDayOffOptimizerValidateDayOffToRemove, 
			IGroupDayOffOptimizerValidateDayOffToAdd groupDayOffOptimizerValidateDayOffToAdd,
			IGroupOptimizerValidateProposedDatesInSameMatrix groupOptimizerValidateProposedDatesInSameMatrix,
			IGroupOptimizerValidateProposedDatesInSameGroup groupOptimizerValidateProposedDatesInSameGroup)
		{
			_groupDayOffOptimizerValidateDayOffToRemove = groupDayOffOptimizerValidateDayOffToRemove;
			_groupDayOffOptimizerValidateDayOffToAdd = groupDayOffOptimizerValidateDayOffToAdd;
			_groupOptimizerValidateProposedDatesInSameMatrix = groupOptimizerValidateProposedDatesInSameMatrix;
			_groupOptimizerValidateProposedDatesInSameGroup = groupOptimizerValidateProposedDatesInSameGroup;
		}

		private delegate ValidatorResult ValidateDaysOffMoveDelegate(IPerson person, IList<DateOnly> dates, bool useSameDaysOff);

		public bool Run(IPerson person, IList<DateOnly> daysOffToRemove, IList<DateOnly> daysOffToAdd, bool useSameDaysOff)
		{
			IDictionary<ValidateDaysOffMoveDelegate, IAsyncResult> runnableList = new Dictionary<ValidateDaysOffMoveDelegate, IAsyncResult>();

			ValidateDaysOffMoveDelegate toRun = _groupDayOffOptimizerValidateDayOffToRemove.Validate;
			IAsyncResult result = toRun.BeginInvoke(person, daysOffToRemove, useSameDaysOff, null, null);
			runnableList.Add(toRun, result);

			toRun = _groupDayOffOptimizerValidateDayOffToAdd.Validate;
			result = toRun.BeginInvoke(person, daysOffToAdd, useSameDaysOff, null, null);
			runnableList.Add(toRun, result);

			toRun = _groupOptimizerValidateProposedDatesInSameMatrix.Validate;
			result = toRun.BeginInvoke(person, daysOffToAdd, useSameDaysOff, null, null);
			runnableList.Add(toRun, result);

			toRun = _groupOptimizerValidateProposedDatesInSameGroup.Validate;
			result = toRun.BeginInvoke(person, daysOffToAdd, useSameDaysOff, null, null);
			runnableList.Add(toRun, result);

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
						if (matrixPro.SelectedPeriod.Contains(validatorResult.DaysToLock.Value))
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IList<IScheduleMatrixPro> MatrixList { get; set; }

		public ValidatorResult()
		{
			MatrixList = new List<IScheduleMatrixPro>();
		}
	}
}