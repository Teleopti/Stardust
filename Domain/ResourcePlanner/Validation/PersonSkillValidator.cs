using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class PersonSkillValidator
	{
		public IEnumerable<PersonValidationError> GetPeopleMissingSkill(IEnumerable<IPerson> people, DateOnlyPeriod range)
		{
			return (from person in people
				let periods = person.PersonPeriods(range)
				where periods.Any(period => !period.PersonSkillCollection.Any())
				select new PersonValidationError(person)
				{
					ValidationError = Resources.MissingSkillsForPlanningPeriod
				}).ToList();
		}
	}
}