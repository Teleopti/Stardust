using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class PersonContractScheduleValidator : IScheduleValidator
	{
		public void FillResult(ValidationResult validationResult, IEnumerable<IPerson> people, DateOnlyPeriod range)
		{
			foreach (var person in people)
			{
				var periods = person.PersonPeriods(range);
				foreach (var period in periods.Where(x => ((IDeleteTag)x.PersonContract.ContractSchedule).IsDeleted))
				{
					validationResult.Add(new PersonValidationError(person)
					{
						ValidationError = string.Format(Resources.DeletedContractScheduleAssigned, period.PersonContract.ContractSchedule.Description.Name)
					}, GetType());
				}
			}
		}

		public bool AlsoRunInDesktop => true;
	}
}