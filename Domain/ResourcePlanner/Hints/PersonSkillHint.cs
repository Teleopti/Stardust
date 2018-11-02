using System.Linq;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class PersonSkillHint : ISchedulePreHint
	{
		public void FillResult(HintResult hintResult, ScheduleHintInput input)
		{
			var people = input.People;
			var range = input.Period;
			var validationErrors = from person in people
				let periods = person.PersonPeriods(range)
				where periods.Any(period => !period.PersonSkillCollection.Any())
				select new PersonHintError(person)
				{
					ErrorResource = nameof(Resources.MissingSkillsForPeriod)
				};
			foreach (var validationError in validationErrors)
			{
				hintResult.Add(validationError, GetType());
			}
		}
	}
}