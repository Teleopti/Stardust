using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public class PersonSkillValidator : IPersonSkillValidator
	{
		public IEnumerable<PersonValidationError> GetPeopleMissingSkill(ICollection<IPerson> people, DateOnlyPeriod range)
		{
			return (from person in people
				let periods = person.PersonPeriods(range)
				where periods.Any(period => !period.PersonSkillCollection.Any())
				select new PersonValidationError(person)
				{
					ValidationError = "Person does not have skills for all or parts of the planning period."
				}).ToList();
		}
	}
}