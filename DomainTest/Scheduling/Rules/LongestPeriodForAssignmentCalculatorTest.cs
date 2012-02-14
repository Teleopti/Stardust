using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class LongestPeriodForAssignmentCalculatorTest
    {
        private ILongestPeriodForAssignmentCalculator target;

        [SetUp]
        public void Setup()
        {
            target = new LongestPeriodForAssignmentCalculator(); 
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyLongestAssignmentPeriod()
        {
            //copy/paste from old code. 
            var nightlyRest = new TimeSpan(8, 0, 0);
            var contract = new Contract("for test");
            contract.WorkTimeDirective = new WorkTimeDirective(new TimeSpan(40, 0, 0),
                                                               nightlyRest,
                                                               new TimeSpan(50, 0, 0));

            //var start = new DateTime(2007, 8, 2, 8, 30, 0, DateTimeKind.Utc);
            //var end = new DateTime(2007, 8, 2, 17, 30, 0, DateTimeKind.Utc);
            var range = new DateTimePeriod(2007, 8, 1, 2007, 8, 5);

            var scenario = ScenarioFactory.CreateScenarioAggregate();
            //var category = ShiftCategoryFactory.CreateShiftCategory("myCategory");
            //var activity = ActivityFactory.CreateActivity("Phone");
            var person = PersonFactory.CreatePerson();
            person.PermissionInformation.SetDefaultTimeZone(new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));

            var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
            var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(range), underlyingDictionary);

            var dayOff = new DayOffTemplate(new Description("test"));
            dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(1));
            dayOff.Anchor = new TimeSpan(8, 30, 0);
            var personDayOff = new PersonDayOff(person, scenario, dayOff, new DateOnly(2007, 8, 3));

            person.AddPersonPeriod(new PersonPeriod(
                                      new DateOnly(1900, 1, 1),
                                      new PersonContract(contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
                                      new Team()));
            var scheduleRange =
                new ScheduleRange(dic, new ScheduleParameters(scenario, person, range));
            underlyingDictionary.Add(person, scheduleRange);

            scheduleRange.Add(personDayOff);

            var expected = new DateTimePeriod(new DateTime(2007, 8, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 2, 13, 30, 0, DateTimeKind.Utc));
            var result = target.PossiblePeriod(scheduleRange, new DateOnly(2007, 8, 2));
            Assert.AreEqual(expected, result);
        }
    }
}
