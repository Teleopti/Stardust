using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class PersonShiftBagHint : IScheduleHint
	{
		public void FillResult(HintResult hintResult, HintInput input)
		{
			var people = input.People;
			var range = input.Period;
			foreach (var person in people)
			{
				var periods = person.PersonPeriods(range);
				foreach (var period in periods)
				{
					if (period.RuleSetBag == null)
					{
						hintResult.Add(new PersonHintError(person)
						{
							ValidationError = Resources.MissingShiftBagForPeriod
						}, GetType());
					}
					else if (((IDeleteTag)period.RuleSetBag).IsDeleted)
					{
						hintResult.Add(new PersonHintError(person)
						{
							ValidationError = string.Format(Resources.DeletedShiftBagAssigned, period.RuleSetBag.Description.Name)
						}, GetType());
					}
				}
			}
		}
	}
}