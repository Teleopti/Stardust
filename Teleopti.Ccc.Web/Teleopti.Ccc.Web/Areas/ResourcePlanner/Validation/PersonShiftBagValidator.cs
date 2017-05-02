using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.UserTexts;
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
				foreach (var period in periods)
				{
					if (period.RuleSetBag == null)
						list.Add(new PersonValidationError(person)
						{
							ValidationError = Resources.MissingShiftBagForPlanningPeriod
						});
					else if (((IDeleteTag)period.RuleSetBag).IsDeleted)
						list.Add(new PersonValidationError(person)
						{
							ValidationError = string.Format(Resources.DeletedShiftBagAssignedForPlanningPeriod, period.RuleSetBag.Description.Name)
						});
				}
			}
			return list;
		}
	}
}