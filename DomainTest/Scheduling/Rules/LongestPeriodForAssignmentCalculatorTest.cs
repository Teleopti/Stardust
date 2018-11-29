using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[DomainTest]
    public class LongestPeriodForAssignmentCalculatorTest
    {
        private ILongestPeriodForAssignmentCalculator target;

        [SetUp]
        public void Setup()
        {
            target = new LongestPeriodForAssignmentCalculator(); 
        }


        [Test]
        public void VerifyLongestAssignmentPeriod()
        {
            //copy/paste from old code. 
            var nightlyRest = new TimeSpan(8, 0, 0);
            var contract = new Contract("for test");
			contract.WorkTimeDirective = new WorkTimeDirective(new TimeSpan(0, 0, 0), new TimeSpan(40, 0, 0),
                                                               nightlyRest,
                                                               new TimeSpan(50, 0, 0));

            var range = new DateTimePeriod(2007, 8, 1, 2007, 8, 5);

            var scenario = ScenarioFactory.CreateScenarioAggregate();
            var person = PersonFactory.CreatePerson();
            person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));

            var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
            var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(range), underlyingDictionary);

            var dayOff = new DayOffTemplate(new Description("test"));
            dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(1));
            dayOff.Anchor = new TimeSpan(8, 30, 0);

            person.AddPersonPeriod(new PersonPeriod(
                                      new DateOnly(1900, 1, 1),
                                      new PersonContract(contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
                                      new Team()));
            var scheduleRange =
                new ScheduleRange(dic, new ScheduleParameters(scenario, person, range), new PersistableScheduleDataPermissionChecker(new FullPermission()), new FullPermission());
            underlyingDictionary.Add(person, scheduleRange);

            scheduleRange.Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, new DateOnly(2007, 8, 3), dayOff));

            var expected = new DateTimePeriod(new DateTime(2007, 8, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 2, 13, 30, 0, DateTimeKind.Utc));
            var result = target.PossiblePeriod(scheduleRange, new DateOnly(2007, 8, 2));
            Assert.AreEqual(expected, result);
        }
    }
}
