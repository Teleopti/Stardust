using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class PersonSkillValidator : IScheduleValidator
	{
		public void FillResult(ValidationResult validationResult, IEnumerable<IPerson> people, DateOnlyPeriod range)
		{
			var validationErrors = from person in people
				let periods = person.PersonPeriods(range)
				where periods.Any(period => !period.PersonSkillCollection.Any())
				select new PersonValidationError(person)
				{
					ValidationError = Resources.MissingSkillsForPeriod
				};
			foreach (var validationError in validationErrors)
			{
				validationResult.Add(validationError, GetType());
			}
		}
	}
}