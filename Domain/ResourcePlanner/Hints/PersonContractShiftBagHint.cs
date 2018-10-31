using System.Linq;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class PersonContractShiftBagHint : IScheduleHint
	{
		public void FillResult(HintResult hintResult, HintInput input)
		{
			var people = input.People;
			var range = input.Period;
			foreach (var person in people)
			{
				var longestShift = 0d;
				var shortestShift = 10000d;
				var period = person.PersonPeriods(range).FirstOrDefault();
				var contract = period?.PersonContract.Contract;

				if (period == null || contract == null || period.RuleSetBag == null)
				{
					continue;
				}

				foreach (var ruleSet in period.RuleSetBag.RuleSetCollection)
				{
					foreach (var workShift in ruleSet.TemplateGenerator.Generate())
					{
						var shiftLength = workShift.Projection.ContractTime().TotalMinutes;
						if (shiftLength > longestShift)
						{
							longestShift = shiftLength;
						}

						if (shiftLength < shortestShift)
						{
							shortestShift = shiftLength;
						}
					}
				}


				var virtualSchedulePeriod = person.VirtualSchedulePeriod(range.StartDate);
				var targetTime = virtualSchedulePeriod.PeriodTarget().TotalMinutes;
				var lowerTarget = targetTime - contract.NegativePeriodWorkTimeTolerance.TotalMinutes;
				var upperTarget = targetTime + contract.PositivePeriodWorkTimeTolerance.TotalMinutes;
				var workDays = virtualSchedulePeriod.Workdays();

				if (workDays * longestShift < lowerTarget || workDays * shortestShift > upperTarget)
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