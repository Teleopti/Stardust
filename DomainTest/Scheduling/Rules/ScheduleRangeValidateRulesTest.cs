using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class ScheduleRangeValidateRulesTest
    {
        private IScheduleRange _scheduleRange;
        private IScenario _scenario;  
        private TimeSpan _nightlyRest;
        private IContract _contract;
        private DateTimePeriod _schedulePeriod;
        private IPerson _person;
        private IScheduleDictionary scheduleDic;

        [SetUp]
        public void Setup()
        {
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _person = PersonFactory.CreatePerson();
            var dic = new Dictionary<IPerson, IScheduleRange>();
            _schedulePeriod = new DateTimePeriod(2007, 8, 1, 2007, 9, 1);
            scheduleDic = new ScheduleDictionaryForTest(_scenario,
                                                        new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2020,1, 1)),
                                                        dic);
            _scheduleRange = new ScheduleRange(scheduleDic, new ScheduleParameters(_scenario, _person, _schedulePeriod));
            dic[_person] = _scheduleRange;
            _nightlyRest = new TimeSpan(8, 0, 0);
            _contract = 
				new Contract("for test");
            _contract.WorkTimeDirective = new WorkTimeDirective(new TimeSpan(0, 0, 0), new TimeSpan(40, 0, 0),
                                                               _nightlyRest,
                                                               new TimeSpan(50, 0, 0));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyValidate()
        {
            AddPersonAssignmentsToSchedulePart();
            var schedulePart = ExtractedSchedule.CreateScheduleDay(scheduleDic, _person, new DateOnly(2007, 8, 3));
            // add another assigment too close to the last one
            IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person, new DateTimePeriod(
                                                             new DateTime(2007, 8, 3, 20, 0, 0, DateTimeKind.Utc),
                                                             new DateTime(2007, 8, 4, 02, 0, 0, DateTimeKind.Utc)));
            schedulePart.Add(ass);
            ITeam team = new Team();
            //team.WriteProtection = 10000;
            _person.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1),
                                       new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
                                       team));

            ScheduleDateTimePeriod per = new ScheduleDateTimePeriod(_schedulePeriod);

            ScheduleDictionary dic = new ScheduleDictionary(_scenario, per);
            dic.Modify(ScheduleModifier.Scheduler, schedulePart, NewBusinessRuleCollection.Minimum(), new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));


            var parts = dic.SchedulesForDay(new DateOnly(2007, 8, 1));
            Assert.AreSame(dic, parts.First().Owner);

            Assert.AreEqual(0,parts.First().BusinessRuleResponseCollection.Count);
        }

        private void AddPersonAssignmentsToSchedulePart()
        {
            IList<IPersonAssignment> assignments = new List<IPersonAssignment>();
            assignments.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person,
                                                                                  new DateTimePeriod(new DateTime(2007, 8, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 1, 17, 0, 0, DateTimeKind.Utc))));
            assignments.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person,
                                                                                  new DateTimePeriod(new DateTime(2007, 8, 2, 8, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 2, 17, 0, 0, DateTimeKind.Utc))));
            assignments.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person,
                                                                                  new DateTimePeriod(new DateTime(2007, 8, 3, 10, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 3, 19, 0, 0, DateTimeKind.Utc))));

            ((ScheduleRange)_scheduleRange).AddRange(assignments);
        }
    }
}
