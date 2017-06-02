using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
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
						ValidationError = Resources.MissingSchedulePeriodForPlanningPeriod
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
					if (containedPeriods.All(x => x.StartDate != range.StartDate) || containedPeriods.All(x => x.EndDate != (person.TerminalDate ?? range.EndDate)))
						list.Add(new PersonValidationError(person)
						{
							ValidationError = Resources.NoFullSchedulePeriod
						});
				}
			}
			return list;
		}
	}
}