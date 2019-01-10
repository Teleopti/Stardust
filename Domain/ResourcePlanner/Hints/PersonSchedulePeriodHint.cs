using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class PersonSchedulePeriodHint : ISchedulePreHint
	{
		private static readonly PeriodIncrementorFactory personIncrementor = new PeriodIncrementorFactory();

		public void FillResult(HintResult hintResult, ScheduleHintInput input)
		{
			var people = input.People;
			var range = input.Period;
			foreach (var person in people)
			{
				var schedulePeriods = person.PersonSchedulePeriods(range);
				
				if (!schedulePeriods.Any())
				{
					hintResult.Add(new PersonHintError(person)
					{
						ErrorResource = nameof(Resources.MissingSchedulePeriodForPeriod)
					}, GetType());
				}
				else
				{
					var containedPeriods = new List<DateOnlyPeriod>();
					foreach (var schedulePeriod in schedulePeriods)
					{
						var start = range.StartDate;
						var incrementor = personIncrementor.PeriodIncrementor(schedulePeriod.PeriodType,
							person.PermissionInformation.Culture());
						while (start < range.EndDate || 
							   (schedulePeriod.PeriodType==SchedulePeriodType.Day && start == range.EndDate))
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
					if (containedPeriods.All(x => x.StartDate != range.StartDate) ||
						containedPeriods.All(x => x.EndDate != (person.TerminalDate ?? range.EndDate)))
					{
						hintResult.Add(new PersonHintError(person)
						{
							ErrorResource = nameof(Resources.NoMatchingSchedulePeriod)
						}, GetType());
					}
				}
			}
		}
	}
}