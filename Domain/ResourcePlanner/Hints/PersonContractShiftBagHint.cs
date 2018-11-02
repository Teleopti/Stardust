using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class PersonContractShiftBagHint : IScheduleHint
	{
		private readonly IRuleSetProjectionService _ruleSetProjectionService;

		public PersonContractShiftBagHint(IRuleSetProjectionService ruleSetProjectionService)
		{
			_ruleSetProjectionService = ruleSetProjectionService;
		}
		
		public void FillResult(HintResult hintResult, HintInput input)
		{
			var people = input.People;
			var range = input.Period;
			foreach (var person in people)
			{
				var longestShift = TimeSpan.MinValue;
				var shortestShift = TimeSpan.MaxValue;
				var period = person.PersonPeriods(range).FirstOrDefault();
				var contract = period?.PersonContract.Contract;

				if (period == null || contract == null || period.RuleSetBag == null)
				{
					continue;
				}

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


				var virtualSchedulePeriod = person.VirtualSchedulePeriod(range.StartDate);
				var targetTime = virtualSchedulePeriod.PeriodTarget();
				var lowerTarget = targetTime - contract.NegativePeriodWorkTimeTolerance;
				var upperTarget = targetTime + contract.PositivePeriodWorkTimeTolerance;
				var workDays = virtualSchedulePeriod.Workdays();

				if (workDays * longestShift.Ticks < lowerTarget.Ticks || workDays * shortestShift.Ticks > upperTarget.Ticks)
				{
					hintResult.Add(new PersonHintError(person)
					{
						ErrorResource = nameof(Resources.ShiftsInShiftBagCanNotFulFillContractTime),
						ErrorResourceData = new object[] {period.RuleSetBag.Description.Name, contract.Description.Name}
							.ToList()
					}, GetType());
				}
			}
		}
	}
}