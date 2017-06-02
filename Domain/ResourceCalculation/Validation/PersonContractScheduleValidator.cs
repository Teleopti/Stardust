using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.Validation
{
	public class PersonContractScheduleValidator : IPersonContractScheduleValidator
	{
		public IEnumerable<PersonValidationError> GetPeopleMissingContractSchedule(ICollection<IPerson> people, DateOnlyPeriod range)
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