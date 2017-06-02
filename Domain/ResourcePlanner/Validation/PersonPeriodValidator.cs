using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class PersonPeriodValidator
	{
		public IEnumerable<PersonValidationError> GetPeopleMissingPeriod(ICollection<IPerson> people, DateOnlyPeriod range)
		{
			var list = new List<PersonValidationError>();
			foreach (var person in people)
			{
				var periods = person.PersonPeriods(range);
				if (!periods.Any(personPeriod => personPeriod.StartDate <= range.StartDate))
					list.Add(new PersonValidationError(person)
					{
						ValidationError = Resources.MissingPersonPeriodForPlanningPeriod
					});
			}
			return list;
		}
	}
}