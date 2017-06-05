using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class PersonPeriodValidator
	{
		public void FillPeopleMissingPeriod(ValidationResult validationResult, IEnumerable<IPerson> people, DateOnlyPeriod range)
		{
			foreach (var person in people)
			{
				var periods = person.PersonPeriods(range);
				if (!periods.Any(personPeriod => personPeriod.StartDate <= range.StartDate))
				{
					validationResult.Add(new PersonValidationError(person)
					{
						ValidationError = Resources.MissingPersonPeriodForPeriod
					}, GetType());
				}
			}
		}
	}
}