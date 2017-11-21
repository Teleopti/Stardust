using System.Linq;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class PersonPeriodHint : IScheduleHint
	{
		public void FillResult(HintResult hintResult, HintInput input)
		{
			var people = input.People;
			var range = input.Period;
			foreach (var person in people)
			{
				var periods = person.PersonPeriods(range);
				if (!periods.Any(personPeriod => personPeriod.StartDate <= range.StartDate))
				{
					hintResult.Add(new PersonHintError(person)
					{
						ErrorResource = nameof(Resources.MissingPersonPeriodForPeriod)
					}, GetType());
				}
			}
		}
	}
}