using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class PersonContractScheduleValidator
	{
		public IEnumerable<PersonValidationError> GetPeopleMissingContractSchedule(IEnumerable<IPerson> people, DateOnlyPeriod range)
		{
			var list = new List<PersonValidationError>();
			foreach (var person in people)
			{
				var periods = person.PersonPeriods(range);
				foreach (var period in periods.Where(x => ((IDeleteTag)x.PersonContract.ContractSchedule).IsDeleted))
				{
					list.Add(new PersonValidationError(person)
					{
						ValidationError = string.Format(Resources.DeletedContractScheduleAssignedForPlanningPeriod, period.PersonContract.ContractSchedule.Description.Name)
					});
				}
			}
			return list;
		}
	}
}