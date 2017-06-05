using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class PersonContractValidator
	{
		public void FillPeopleMissingContract(ValidationResult validationResult, IEnumerable<IPerson> people, DateOnlyPeriod range)
		{
			foreach (var person in people)
			{
				var periods = person.PersonPeriods(range);
				foreach (var period in periods.Where(x => ((IDeleteTag)x.PersonContract.Contract).IsDeleted))
				{
					validationResult.Add(new PersonValidationError(person)
					{
						ValidationError = string.Format(Resources.DeletedContractAssigned, period.PersonContract.Contract.Description.Name)
					}, GetType());
				}
			}
		}
	}
}