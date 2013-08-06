using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class PersonDayOffFactory
    {
        public static IPersonDayOff CreatePersonDayOff()
        {
            return CreatePersonDayOff(PersonFactory.CreatePerson(),
                                    ScenarioFactory.CreateScenarioAggregate(),
                                    new DateTime(2007, 1, 1),TimeSpan.FromHours(4),TimeSpan.FromHours(1),TimeSpan.FromHours(10));
        }

        public static IPersonDayOff CreatePersonDayOff(DateTime startDateTime, TimeSpan length, TimeSpan flexibility, TimeSpan anchor)
        {
            return CreatePersonDayOff(PersonFactory.CreatePerson(),
                                    ScenarioFactory.CreateScenarioAggregate(),
                                    startDateTime,length,flexibility,anchor);
        }

    	public static IPersonDayOff CreatePersonDayOff(IPerson person, IScenario scenario, DateTime startDateTime, TimeSpan length, TimeSpan flexibility, TimeSpan anchor)
        {
            IDayOffTemplate dayOff = DayOffFactory.CreateDayOff(new Description("test"));
            
            dayOff.Anchor = anchor;
            dayOff.SetTargetAndFlexibility(length, flexibility);
            return CreatePersonDayOff(person, scenario, new DateOnly(startDateTime.Date), dayOff);
        }

        public static IPersonDayOff CreatePersonDayOff(IPerson person, IScenario scenario, DateOnly dateOnly, IDayOffTemplate dayOffTemplate)
        {
            return new PersonDayOff(person, scenario, dayOffTemplate, new DateOnly(dateOnly));
        }
    }
}