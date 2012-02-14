using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class ScheduleMatrixProFactory
    {
        /// <summary>
        /// Creates the specified ScheduleMatrix.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public static ScheduleMatrixPro Create(DateOnlyPeriod period)
        {
            IPerson person = PersonFactory.CreatePerson("Testor");
            return Create(period, person);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static ScheduleMatrixPro Create(DateOnlyPeriod period, IPerson person)
        {
            ISchedulingResultStateHolder stateHolder = new SchedulingResultStateHolder();

            DateTimePeriod dayPeriod = period.ToDateTimePeriod(new CccTimeZoneInfo(TimeZoneInfo.Utc));
            IScheduleDateTimePeriod scheduleDateTimePeriod = new ScheduleDateTimePeriod(dayPeriod);
            IScenario scenario = new Scenario("Scenario");
            IScheduleDictionary scheduleDictionary = new ScheduleDictionaryForTest(scenario, scheduleDateTimePeriod, new Dictionary<IPerson, IScheduleRange>());
            IScheduleParameters parameters = new ScheduleParameters(scenario, person, dayPeriod);
            IScheduleRange range = new ScheduleRange(scheduleDictionary, parameters);
            ((ScheduleDictionaryForTest)scheduleDictionary).AddTestItem(person, range);
            stateHolder.Schedules = scheduleDictionary;

            return Create(period, stateHolder, person, null);
        }


        /// <summary>
        /// Creates the specified ScheduleMatrix.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="stateHolder">The state holder.</param>
        /// <param name="person">The person.</param>
        /// <param name="schedulePeriod">The schedule period.</param>
        /// <returns></returns>
        public static ScheduleMatrixPro Create(DateOnlyPeriod period, ISchedulingResultStateHolder stateHolder, IPerson person, IVirtualSchedulePeriod schedulePeriod)
        {
            IFullWeekOuterWeekPeriodCreator periodCreator = new FullWeekOuterWeekPeriodCreator(period, person);
            return new ScheduleMatrixPro(stateHolder, periodCreator, schedulePeriod);
        }

    }
}
