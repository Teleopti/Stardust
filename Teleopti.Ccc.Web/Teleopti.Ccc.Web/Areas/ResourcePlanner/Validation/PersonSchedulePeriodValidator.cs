using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public class PersonSchedulePeriodValidator : IPersonSchedulePeriodValidator
	{
		public IEnumerable<PersonValidationError> GetPeopleMissingSchedulePeriod(ICollection<IPerson> people, DateOnlyPeriod range)
		{
			var personIncrementor = new PeriodIncrementorFactory();
			var list = new List<PersonValidationError>();
			foreach (var person in people)
			{
				var schedulePeriods = person.PersonSchedulePeriods(range);
				
				if (!schedulePeriods.Any())
				{
					list.Add(new PersonValidationError(person)
					{
						ValidationError = "Has no schedule periods for the planning period."
					});
				}
				else
				{
					var containedPeriods = new List<DateOnlyPeriod>();
					foreach (var schedulePeriod in schedulePeriods)
					{
						var start = range.StartDate;
						var incrementor = personIncrementor.PeriodIncrementor(schedulePeriod.PeriodType,
							person.PermissionInformation.Culture());
						while (start < range.EndDate)
						{
							var period = schedulePeriod.GetSchedulePeriod(start);
							if (period.HasValue)
							{
								containedPeriods.Add(period.Value);
								if (period.Value.EndDate == person.TerminalDate)
									break;
							}

							start = (period?.EndDate ?? incrementor.Increase(start, schedulePeriod.Number)).AddDays(1);
						}
					}
					if (!containedPeriods.Any(x => x.StartDate >= range.StartDate && x.EndDate <= range.EndDate))
						list.Add(new PersonValidationError(person)
						{
							ValidationError = "No full schedule period contained in the planning period."
						});
				}
			}
			return list;
		}
	}
}