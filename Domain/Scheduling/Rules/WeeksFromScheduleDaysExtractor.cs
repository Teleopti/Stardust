using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public interface IWeeksFromScheduleDaysExtractor
	{
		IEnumerable<PersonWeek> CreateWeeksFromScheduleDaysExtractor(IEnumerable<IScheduleDay> scheduleDays);

		IEnumerable<PersonWeek> CreateWeeksFromScheduleDaysExtractor(IEnumerable<IScheduleDay> scheduleDays,
			bool addWeekBeforeAndAfterIfFirstOrLastDayInWeekIsIncluded);
	}

	public class WeeksFromScheduleDaysExtractor : IWeeksFromScheduleDaysExtractor
	{
		public IEnumerable<PersonWeek> CreateWeeksFromScheduleDaysExtractor(IEnumerable<IScheduleDay> scheduleDays,
			bool addWeekBeforeAndAfterIfFirstOrLastDayInWeekIsIncluded)
		{
			var personWeeks = new HashSet<PersonWeek>();
			foreach (var day in scheduleDays)
			{
				var person = day.Person;
				var dayDateOnly = day.DateOnlyAsPeriod.DateOnly;
				var firstDateInPeriodLocal = DateHelper.GetFirstDateInWeek(dayDateOnly, person.FirstDayOfWeek);
				var period = new DateOnlyPeriod(firstDateInPeriodLocal, firstDateInPeriodLocal.AddDays(6));
				personWeeks.Add(new PersonWeek(day.Person, period));

				if (addWeekBeforeAndAfterIfFirstOrLastDayInWeekIsIncluded && dayDateOnly.Equals(firstDateInPeriodLocal))
				{
					// only if we have a personperiod then
					var personPeriod = person.Period(firstDateInPeriodLocal.AddDays(-7));
					if (personPeriod == null)
						continue;

					var weekPeriod = new DateOnlyPeriod(firstDateInPeriodLocal.AddDays(-7), firstDateInPeriodLocal.AddDays(-1));
					personWeeks.Add(new PersonWeek(day.Person, weekPeriod));
				}

				if (addWeekBeforeAndAfterIfFirstOrLastDayInWeekIsIncluded && dayDateOnly.Equals(period.EndDate))
				{
					// only if we have a personperiod then
					var personPeriod = person.Period(period.EndDate.AddDays(7));
					if (personPeriod == null) continue;

					var weekPeriod = new DateOnlyPeriod(period.EndDate.AddDays(1), period.EndDate.AddDays(7));
					personWeeks.Add(new PersonWeek(day.Person, weekPeriod));
				}
			}

			return personWeeks;
		}

		public IEnumerable<PersonWeek> CreateWeeksFromScheduleDaysExtractor(IEnumerable<IScheduleDay> scheduleDays)
		{
			return CreateWeeksFromScheduleDaysExtractor(scheduleDays, false);
		}
	}
}