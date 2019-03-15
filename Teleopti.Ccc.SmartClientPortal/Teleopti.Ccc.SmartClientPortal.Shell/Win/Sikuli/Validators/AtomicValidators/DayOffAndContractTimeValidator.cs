using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.AtomicValidators
{
	internal class DayOffAndContractTimeValidator : IAtomicValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;

		public DayOffAndContractTimeValidator(ISchedulerStateHolder schedulerState)
		{
			_schedulerState = schedulerState;
		}

		public string Description
		{
			get
			{
				return "The number of current day offs and current contract times must be equal to the target for each person.";
			}
		}

		public SikuliValidationResult Validate(ITimeZoneGuard timeZoneGuard)
		{
			bool contractTimeResult = true;
			bool daysOffResult = true;
			var result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			IList<IPerson> persons = _schedulerState.FilteredCombinedAgentsDictionary.Values.ToList();
			var requestedPeriod = _schedulerState.RequestedPeriod.DateOnlyPeriod;

			foreach (var person in persons)
			{
				var range = _schedulerState.Schedules[person];
				var currentContractTime = range.CalculatedContractTimeHolderOnPeriod(requestedPeriod);
				var targetContractTime = range.CalculatedTargetTimeHolder(requestedPeriod);

				if (currentContractTime != targetContractTime)
					contractTimeResult = false;

				var currentDaysOff = range.CalculatedScheduleDaysOffOnPeriod(_schedulerState.RequestedPeriod.DateOnlyPeriod);
				var targetDayOffs = range.CalculatedTargetScheduleDaysOff(_schedulerState.RequestedPeriod.DateOnlyPeriod);

				if (currentDaysOff != targetDayOffs)
					daysOffResult = false;

			}

			result.Result = (contractTimeResult && daysOffResult) ?
				SikuliValidationResult.ResultValue.Pass :
				SikuliValidationResult.ResultValue.Fail;
			result.Details.AppendLine(contractTimeResult ? "Contract time : OK" : "Contract time : FAIL");
			result.Details.AppendLine(daysOffResult ? "Day offs : OK" : "Contract time : FAIL");
			return result;
		}
	}
}
