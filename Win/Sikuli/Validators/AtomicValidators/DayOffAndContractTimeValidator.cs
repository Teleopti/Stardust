using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators
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

		public SikuliValidationResult Validate()
		{
			bool contractTimeResult = true;
			bool daysOffResult = true;
			var result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			IList<IPerson> persons = _schedulerState.FilteredPersonDictionary.Values.ToList();

			foreach (var person in persons)
			{
				var range = _schedulerState.Schedules[person];
				var currentContractTime = range.CalculatedContractTimeHolder;
				var targetContractTime = range.CalculatedTargetTimeHolder;

				if (currentContractTime != targetContractTime)
					contractTimeResult = false;

				var currentDaysOff = range.CalculatedScheduleDaysOff;
				var targetDayOffs = range.CalculatedTargetScheduleDaysOff;

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
