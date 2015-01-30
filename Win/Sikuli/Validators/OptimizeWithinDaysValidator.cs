﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators
{
	public class OptimizeWithinDaysValidator : ISikuliValidator
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

		public SikuliValidationResult Validate()
		{
			bool stdDevSumResult;
			bool contractTimeResult = true;
			bool daysOffResult = true;

			SikuliValidationResult result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			var std = ValidatorHelper.GetDailySumOfStandardDeviationsFullPeriod(_schedulerState, _totalSkill);
			
			stdDevSumResult = std < 4.6d;
			
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
			const double limit = 4.6d;
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
