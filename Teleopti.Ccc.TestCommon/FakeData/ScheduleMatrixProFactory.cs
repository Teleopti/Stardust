using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class ScheduleMatrixProFactory
    {
        public static ScheduleMatrixPro Create(DateOnlyPeriod period, IPerson person, ICurrentAuthorization currentAuthorization = null)
        {
            ISchedulingResultStateHolder stateHolder = new SchedulingResultStateHolder();

            DateTimePeriod dayPeriod = period.ToDateTimePeriod(TimeZoneInfo.Utc);
            IScheduleDateTimePeriod scheduleDateTimePeriod = new ScheduleDateTimePeriod(dayPeriod);
            IScenario scenario = new Scenario("Scenario");
            var scheduleDictionary = new ScheduleDictionaryForTest(scenario, scheduleDateTimePeriod, new Dictionary<IPerson, IScheduleRange>(), currentAuthorization);
            IScheduleParameters parameters = new ScheduleParameters(scenario, person, dayPeriod);
            IScheduleRange range = new ScheduleRange(scheduleDictionary, parameters, new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()), CurrentAuthorization.Make());
            scheduleDictionary.AddTestItem(person, range);
            stateHolder.Schedules = scheduleDictionary;

            return Create(period, stateHolder, person, person.VirtualSchedulePeriod(period.StartDate));
        }

        public static ScheduleMatrixPro Create(DateOnlyPeriod period, ISchedulingResultStateHolder stateHolder, IPerson person, IVirtualSchedulePeriod schedulePeriod)
        {
            IFullWeekOuterWeekPeriodCreator periodCreator = new FullWeekOuterWeekPeriodCreator(period, person);
            return new ScheduleMatrixPro(stateHolder.Schedules, periodCreator, schedulePeriod);
        }
    }
}
