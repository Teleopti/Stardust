using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class PersonContractShiftBagHint : ISchedulePreHint
	{
		private readonly IRuleSetProjectionService _ruleSetProjectionService;

		public PersonContractShiftBagHint(IRuleSetProjectionService ruleSetProjectionService)
		{
			_ruleSetProjectionService = ruleSetProjectionService;
		}
		
		public void FillResult(HintResult hintResult, ScheduleHintInput input)
		{
			var people = input.People;
			var range = input.Period;

			var shiftBagShiftLengths = new Dictionary<Guid,ShiftBagShiftLengths>();
			
			foreach (var person in people)
			{
				var longestShift = TimeSpan.MinValue;
				var shortestShift = TimeSpan.MaxValue;
				var period = person.PersonPeriods(range).FirstOrDefault();
				var contract = period?.PersonContract.Contract;

				if (period == null || contract == null || ((IDeleteTag)contract).IsDeleted|| contract.EmploymentType == EmploymentType.HourlyStaff || period.RuleSetBag == null || ((IDeleteTag)period.RuleSetBag).IsDeleted)
				{
					continue;
				}

				if (period.RuleSetBag.Id.HasValue && shiftBagShiftLengths.ContainsKey(period.RuleSetBag.Id.Value))
				{
					shortestShift = shiftBagShiftLengths[period.RuleSetBag.Id.Value].ShortestShift;
					longestShift = shiftBagShiftLengths[period.RuleSetBag.Id.Value].LongestShift;
				}
				else
				{
					foreach (var ruleSet in period.RuleSetBag.RuleSetCollection)
					{
						foreach (var workShift in _ruleSetProjectionService.ProjectionCollection(ruleSet, null))
						{
							var contractTime = workShift.ContractTime;
							if (contractTime > longestShift)
							{
								longestShift = contractTime;
							}

							if (contractTime < shortestShift)
							{
								shortestShift = contractTime;
							}
						}
					}

					if (period.RuleSetBag.Id.HasValue)
					{
						shiftBagShiftLengths.Add(period.RuleSetBag.Id.Value,new ShiftBagShiftLengths(){ShortestShift = shortestShift,LongestShift = longestShift});
					}
				}

				var virtualSchedulePeriod = person.VirtualSchedulePeriod(range.StartDate);
				var targetTime = virtualSchedulePeriod.PeriodTarget();
				var lowerTarget = targetTime - contract.NegativePeriodWorkTimeTolerance;
				var upperTarget = targetTime + contract.PositivePeriodWorkTimeTolerance;
				var workDays = virtualSchedulePeriod.Workdays();


				if (virtualSchedulePeriod.Contract == null || workDays * longestShift.Ticks >= lowerTarget.Ticks && workDays * shortestShift.Ticks <= upperTarget.Ticks) continue;
				if (virtualSchedulePeriod.AverageWorkTimePerDay == virtualSchedulePeriod.Contract.WorkTime.AvgWorkTimePerDay)
				{
					hintResult.Add(new PersonHintError(person)
					{
						ErrorResource = nameof(Resources.ShiftsInShiftBagCanNotFulFillContractTime),
						ErrorResourceData = new object[] {period.RuleSetBag.Description.Name, contract.Description.Name}
							.ToList()
					}, GetType());
				}
				else
				{
					hintResult.Add(new PersonHintError(person)
					{
						ErrorResource = nameof(Resources.ShiftsInShiftBagCanNotFulFillOverriddenTargetTime),
						ErrorResourceData = new object[] {period.RuleSetBag.Description.Name}
							.ToList()
					}, GetType());
				}
			}
		}

		private class ShiftBagShiftLengths
		{
			public TimeSpan ShortestShift { get; set; }
			public TimeSpan LongestShift { get; set; }
		}
	}
}