using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public class PersonSchedulePeriodValidator : IPersonSchedulePeriodValidator
	{
		public IEnumerable<PersonValidationError> GetPeopleMissingSchedulePeriod(ICollection<IPerson> people, DateOnlyPeriod range)
		{
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
						while (start < range.EndDate)
						{
							var period = schedulePeriod.GetSchedulePeriod(start);
							if (period.HasValue)
								containedPeriods.Add(period.Value);
							
							start = period?.EndDate.AddDays(1) ?? nextStartDate(schedulePeriod, start);
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

		private static DateOnly nextStartDate(ISchedulePeriod schedulePeriod, DateOnly currentStartDate)
		{
			switch (schedulePeriod.PeriodType)
			{
				case SchedulePeriodType.Month:
					return currentStartDate.AddMonths(new GregorianCalendar(), schedulePeriod.Number);
				case SchedulePeriodType.Week:
					return currentStartDate.AddDays(7 * schedulePeriod.Number);
				case SchedulePeriodType.Day:
					return currentStartDate.AddDays(schedulePeriod.Number);
				case SchedulePeriodType.ChineseMonth:
					return currentStartDate.AddMonths(new ChineseLunisolarCalendar(), schedulePeriod.Number);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}