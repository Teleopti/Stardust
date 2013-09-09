using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for PersonDayOff domain object
    /// </summary>
    public static class PersonDayOffFactory
    {
        /// <summary>
        /// Creates an agent Day Off container
        /// </summary>
        /// <returns></returns>
        public static IPersonDayOff CreatePersonDayOff()
        {
            return CreatePersonDayOff(PersonFactory.CreatePerson(),
                                    ScenarioFactory.CreateScenarioAggregate(),
                                    new DateTime(2007, 1, 1),TimeSpan.FromHours(4),TimeSpan.FromHours(1),TimeSpan.FromHours(10));
        }

        /// <summary>
        /// Creates the agent day off.
        /// </summary>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="length">The length.</param>
        /// <param name="flexibility">The flexibility.</param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-12
        /// </remarks>
        public static IPersonDayOff CreatePersonDayOff(DateTime startDateTime, TimeSpan length, TimeSpan flexibility, TimeSpan anchor)
        {
            return CreatePersonDayOff(PersonFactory.CreatePerson(),
                                    ScenarioFactory.CreateScenarioAggregate(),
                                    startDateTime,length,flexibility,anchor);
        }

		public static IPersonDayOff CreatePersonDayOff(DateTime startDateTime, TimeSpan length, TimeSpan flexibility, TimeSpan anchor, string payrollCode)
		{
			return CreatePersonDayOff(PersonFactory.CreatePerson(),
									ScenarioFactory.CreateScenarioAggregate(),
									startDateTime, length, flexibility, anchor, payrollCode);
		}

		private static IPersonDayOff CreatePersonDayOff(IPerson person, IScenario scenario, DateTime startDateTime, TimeSpan length, TimeSpan flexibility, TimeSpan anchor, string payrollCode)
    	{
			IDayOffTemplate dayOff = DayOffFactory.CreateDayOff(new Description("test"));

			dayOff.Anchor = anchor;
			dayOff.PayrollCode = payrollCode;
			dayOff.SetTargetAndFlexibility(length, flexibility);
			return CreatePersonDayOff(person, scenario, new DateOnly(startDateTime.Date), dayOff);
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