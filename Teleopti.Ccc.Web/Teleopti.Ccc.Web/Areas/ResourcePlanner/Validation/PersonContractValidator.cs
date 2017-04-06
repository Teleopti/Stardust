using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public class PersonContractValidator : IPersonContractValidator
	{
		public IEnumerable<PersonValidationError> GetPeopleMissingContract(ICollection<IPerson> people, DateOnlyPeriod range)
		{
			var list = new List<PersonValidationError>();
			foreach (var person in people)
			{
				var periods = person.PersonPeriods(range);
				if (periods.Any(x => ((IDeleteTag)x.PersonContract.Contract).IsDeleted))
					list.Add(new PersonValidationError(person)
					{
						ValidationError = "Assigned deleted contract for one or more person period within the planning period."
					});
			}
			return list;
		}
	}
}