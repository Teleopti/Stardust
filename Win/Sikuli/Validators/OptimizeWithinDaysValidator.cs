using System;
using System.Collections;
using System.Collections.Generic;
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

		public SikuliValidationResult Validate()
		{
			bool stdDevSumResult;
			bool contractTimeResult = true;
			bool daysOffResult = true;

			SikuliValidationResult result = new SikuliValidationResult(true);
			var std = ValidatorHelper.GetDailySumOfStandardDeviationsFullPeriod(_schedulerState, _totalSkill);
			
			stdDevSumResult = std > 4.6;
			
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

			result.Result = stdDevSumResult && contractTimeResult && daysOffResult;

			result.Details.AppendLine("Details:");
			result.AppendLimitValueLine("Period daily StdDev sum", "4.6", std.ToString());
			result.Details.AppendLine(contractTimeResult ? "Contract time : OK" : "Contract time : FAIL");
			result.Details.AppendLine(daysOffResult ? "Day offs : OK" : "Contract time : FAIL");

			return result;
		}

		public static TimeSpan GetCurrentContractTime(IScheduleRange wholeRange, DateOnlyPeriod period)
		{
			//if (!wholeRange.CalculatedContractTimeHolder.HasValue)
			//{
			//	TimeSpan contractTime = TimeSpan.Zero;
			//	foreach (var scheduleDay in wholeRange.ScheduledDayCollection(period))
			//	{
			//		DateOnly dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
			//		IPerson person = scheduleDay.Person;
			//		if (!person.IsAgent(dateOnly))
			//			continue;

			//		contractTime = contractTime.Add(scheduleDay.ProjectionService().CreateProjection().ContractTime());
			//	}
			//	wholeRange.CalculatedContractTimeHolder = contractTime;
			//}
			return wholeRange.CalculatedContractTimeHolder.Value;
		}
	}
}
