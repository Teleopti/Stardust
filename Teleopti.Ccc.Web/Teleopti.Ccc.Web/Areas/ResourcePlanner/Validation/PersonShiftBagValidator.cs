using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public class PersonShiftBagValidator : IPersonShiftBagValidator
	{
		public IEnumerable<PersonValidationError> GetPeopleMissingShiftBag(ICollection<IPerson> people, DateOnlyPeriod range)
		{
			var list = new List<PersonValidationError>();
			foreach (var person in people)
			{
				var periods = person.PersonPeriods(range);
				if (periods.Any(x => x.RuleSetBag == null))
					list.Add(new PersonValidationError(person)
					{
						ValidationError = "Missing shift bag for one or more person period within the planning period."
					});
				else if (periods.Any(x => ((IDeleteTag)x.RuleSetBag).IsDeleted))
					list.Add(new PersonValidationError(person)
					{
						ValidationError = "Assigned deleted shift bag for one or more person period within the planning period."
					});
			}
			return list;
		}
	}
}