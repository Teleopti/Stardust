using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class PersonShiftBagValidator
	{
		public void FillPeopleMissingShiftBag(ValidationResult validationResult, IEnumerable<IPerson> people, DateOnlyPeriod range)
		{
			foreach (var person in people)
			{
				var periods = person.PersonPeriods(range);
				foreach (var period in periods)
				{
					if (period.RuleSetBag == null)
					{
						validationResult.Add(new PersonValidationError(person)
						{
							ValidationError = Resources.MissingShiftBagForPlanningPeriod
						}, GetType());
					}
					else if (((IDeleteTag)period.RuleSetBag).IsDeleted)
					{
						validationResult.Add(new PersonValidationError(person)
						{
							ValidationError = string.Format(Resources.DeletedShiftBagAssignedForPlanningPeriod, period.RuleSetBag.Description.Name)
						}, GetType());
					}
				}
			}
		}
	}
}