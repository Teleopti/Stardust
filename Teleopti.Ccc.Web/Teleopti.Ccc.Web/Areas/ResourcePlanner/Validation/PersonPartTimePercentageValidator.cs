using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public class PersonPartTimePercentageValidator : IPersonPartTimePercentageValidator
	{
		public IEnumerable<PersonValidationError> GetPeopleMissingPartTimePercentage(ICollection<IPerson> people, DateOnlyPeriod range)
		{
			var list = new List<PersonValidationError>();
			foreach (var person in people)
			{
				var periods = person.PersonPeriods(range);
				foreach (var period in periods.Where(x => ((IDeleteTag)x.PersonContract.PartTimePercentage).IsDeleted))
				{
					list.Add(new PersonValidationError(person)
					{
						ValidationError = string.Format(Resources.DeletedPartTimePercentageAssignedForPlanningPeriod, period.PersonContract.PartTimePercentage.Description.Name)
					});
				}
			}
			return list;
		}
	}
}