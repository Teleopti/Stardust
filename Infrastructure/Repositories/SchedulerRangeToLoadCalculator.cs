using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Calculates the loading period for scheduler
    /// </summary>
    public class SchedulerRangeToLoadCalculator
    {
        private readonly Person _person;
        private readonly DateTimePeriod _requestedDateTimePeriod;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerRangeToLoadCalculator"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="requestedDateTimePeriod">The requested date time period.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-19
        /// </remarks>
        public SchedulerRangeToLoadCalculator(Person person, DateTimePeriod requestedDateTimePeriod)
        {
            if (person == null) throw new ArgumentNullException("person");
            _person = person;
            _requestedDateTimePeriod = requestedDateTimePeriod;
        }

        /// <summary>
        /// Gets the scheduler range to load.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-19
        /// </remarks>
        public DateTimePeriod? SchedulerRangeToLoad()
        {
            IList<SchedulePeriod> validPeriods = _person.PersonSchedulePeriods(_requestedDateTimePeriod);

            if (validPeriods.Count == 0)
                return null;

            CultureInfo culture = _person.PermissionInformation.Culture();

            SchedulePeriod minSchedulePeriod =
                new SchedulePeriod(new DateTime(3000, 1, 1, 0, 0, 0, DateTimeKind.Utc), SchedulePeriodType.Day, 1, TimeSpan.FromHours(1), 1);
            SchedulePeriod maxSchedulePeriod =
                new SchedulePeriod(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc), SchedulePeriodType.Day, 1, TimeSpan.FromHours(1), 1);

            foreach (SchedulePeriod schedulePeriod in validPeriods)
            {
                if (schedulePeriod.DateFrom < minSchedulePeriod.DateFrom)
                    minSchedulePeriod = schedulePeriod;
                if (schedulePeriod.DateFrom > maxSchedulePeriod.DateFrom)
                    maxSchedulePeriod = schedulePeriod;
            }

            DateTime firstPeriodsFirstDateUtc = DateHelper.GetFirstDateInWeek(minSchedulePeriod.GetSchedulePeriod(_requestedDateTimePeriod.StartDateTime).Value.StartDateTime, culture);
            DateTime firstPeriodsFirstDateLocal =
                new DateTime(firstPeriodsFirstDateUtc.Year, firstPeriodsFirstDateUtc.Month, firstPeriodsFirstDateUtc.Day,
                             0, 0, 0, DateTimeKind.Local);

            DateTime lastPeriodsLastDateUtc = DateHelper.GetLastDateInWeek(maxSchedulePeriod.GetSchedulePeriod(_requestedDateTimePeriod.EndDateTime).Value.EndDateTime.AddDays(-1), culture);
            DateTime lastPeriodsLastDateLocal =
                new DateTime(lastPeriodsLastDateUtc.Year, lastPeriodsLastDateUtc.Month, lastPeriodsLastDateUtc.Day,
                             0, 0, 0, DateTimeKind.Local).AddDays(1);

            return TimeZoneHelper.NewDateTimePeriodFromLocalDateTime(firstPeriodsFirstDateLocal, lastPeriodsLastDateLocal,
                                                                  _person.PermissionInformation.DefaultTimeZone());
        }
    }
}
