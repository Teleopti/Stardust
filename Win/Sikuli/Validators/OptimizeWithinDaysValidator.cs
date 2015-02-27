using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators
{
	internal class OptimizeWithinDaysValidator : ISikuliValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public OptimizeWithinDaysValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public string Description 
		{ 
			get { return "The daily standard deviation must be under the limit, the number of current day offs and" +
						 "current contract times must be equal to the target for each person."; } 
		}

		public SikuliValidationResult Validate(ITestDuration duration)
		{
			bool contractTimeResult = true;
			bool daysOffResult = true;
			const double limit = 4.65d;

			var result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			var std = ValidatorHelper.GetDailySumOfStandardDeviationsFullPeriod(_schedulerState, _totalSkill);

			bool stdDevSumResult = std < limit;
			
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

			result.Result = (stdDevSumResult && contractTimeResult && daysOffResult) ?
				SikuliValidationResult.ResultValue.Pass :
				SikuliValidationResult.ResultValue.Fail;
			result.AppendLimitValueLineToDetails("Daily StdDev sum", limit.ToString(CultureInfo.CurrentCulture), std.ToString(CultureInfo.CurrentCulture));
			result.Details.AppendLine(contractTimeResult ? "Contract time : OK" : "Contract time : FAIL");
			result.Details.AppendLine(daysOffResult ? "Day offs : OK" : "Contract time : FAIL");
			return result;
		}

		public static TimeSpan GetCurrentContractTime(IScheduleRange wholeRange, DateOnlyPeriod period)
		{
			return wholeRange.CalculatedContractTimeHolder.Value;
		}
	}
}
