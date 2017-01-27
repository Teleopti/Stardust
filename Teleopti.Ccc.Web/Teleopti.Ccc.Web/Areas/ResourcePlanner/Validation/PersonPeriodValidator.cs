using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public class PersonPeriodValidator : IPersonPeriodValidator
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
						ValidationError = "Has no person period for all or parts the planning period."
					});
			}
			return list;
		}
	}
}