using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.DomainTest.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class AssignmentsToCloseBaseTest
    {
        private IContract _contract;
        private TimeSpan _nightlyRest;
        private IPerson _person;
        private IScenario _scenario;
        private DateTimePeriod _schedulePeriod;
        private IScheduleRange _scheduleRange;
        private AssignmentsToCloseBase _target;




        [SetUp]
        public void Setup()
        {
            _scenario = ScenarioFactory.CreateScenarioAggregate();

            _person = PersonFactory.CreatePerson();
            _nightlyRest = new TimeSpan(8, 0, 0);
            _contract = ContractFactory.CreateContract("for test");
            _contract.WorkTimeDirective = new WorkTimeDirective(new TimeSpan(40, 0, 0),
                                                                _nightlyRest,
                                                                new TimeSpan(50, 0, 0));
            _schedulePeriod = new DateTimePeriod(2007, 8, 1, 2007, 9, 1);
            var dic = new ScheduleDictionaryForTest(_scenario, new ScheduleDateTimePeriod(_schedulePeriod),
                                                    new Dictionary<IPerson, IScheduleRange>());
            _scheduleRange = new ScheduleRange(dic, new ScheduleParameters(_scenario, _person, _schedulePeriod));
            _target = new NightlyRestRule();
        }

        [Test]
        public void CanCreateRule()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyCanFindLongestDateTimePeriodForAssignment()
        {
            addPersonAssignmentsForLongest();
            _person.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1),
                                       new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
                                       TeamFactory.CreateSimpleTeam()));

            var pointInTime = new DateOnly(2007, 8, 2);
            var start = new DateTime(2007, 8, 2, 1, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2007, 8, 3, 2, 0, 0, DateTimeKind.Utc);

            var expected = new DateTimePeriod(start, end);

            var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, pointInTime);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void VerifyCanFindLongestDateTimePeriodForAssignmentUtcPlus()
        {
            addPersonAssignmentsForLongest();
            _person.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1),
                                       new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
                                       TeamFactory.CreateSimpleTeam()));
            _person.PermissionInformation.SetDefaultTimeZone(new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));

            var pointInTime = new DateOnly(2007, 8, 2);
            var start = new DateTime(2007, 8, 2, 1, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2007, 8, 3, 2, 0, 0, DateTimeKind.Utc);

            var expected = new DateTimePeriod(start, end);

            var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, pointInTime);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void VerifyCanFindLongestDateTimePeriodForAssignmentUtcMinus()
        {
            addPersonAssignmentsForLongest();
            _person.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1),
                                       new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
                                       TeamFactory.CreateSimpleTeam()));
            _person.PermissionInformation.SetDefaultTimeZone(new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")));

            var pointInTime = new DateOnly(2007, 8, 2);
            var start = new DateTime(2007, 8, 2, 1, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2007, 8, 3, 2, 0, 0, DateTimeKind.Utc);

            var expected = new DateTimePeriod(start, end);

            var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, pointInTime);
            Assert.AreEqual(expected, result);
        }


        [Test]
        public void VerifyFindLongestWhenNoPersonAssignmentsReturnsStartAndEndOfPeriod()
        {
            _person.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1),
                                       new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
                                       TeamFactory.CreateSimpleTeam()));

            var pointInTime = new DateOnly(2007, 8, 7);
            var start = new DateTime(2007, 8, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2007, 9, 1, 0, 0, 0, DateTimeKind.Utc);

            var expected = new DateTimePeriod(start, end);

            var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, pointInTime);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void VerifyFindLongestWhenNoPersonPeriodReturnsCorrect()
        {
            addPersonAssignmentsToSchedulePart();


            var pointInTime = new DateOnly(2007, 8, 7);
            var start = new DateTime(2007, 8, 7, 12, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2007, 8, 7, 12, 0, 0, DateTimeKind.Utc);

            var expected = new DateTimePeriod(start, end);

            var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, pointInTime);
            Assert.AreEqual(expected, result);
            Assert.IsNotEmpty(_target.ErrorMessage);
        }

        [Test]
        public void VerifyLongestWhenAlreadyScheduledAtTwelveOnTheSameDay()
        {
            _person.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1),
                                       new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
                                       TeamFactory.CreateSimpleTeam()));
            addPersonAssignmentsToSchedulePart();

            var pointInTime = new DateOnly(2007, 8, 2);
            var start = new DateTime(2007, 8, 2, 12, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2007, 8, 2, 12, 0, 0, DateTimeKind.Utc);

            var expected = new DateTimePeriod(start, end);

            var result = _target.LongestDateTimePeriodForAssignment(_scheduleRange, pointInTime);
            Assert.AreEqual(expected, result);

        }


        [Test]
        public void VerifyValidateReturnsFalseWhenRestTooShort()
        {
            addPersonAssignmentsToSchedulePart();

            // add another assigment too close to the last one
            IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person, new DateTimePeriod(
                                                             new DateTime(2007, 8, 4, 01, 0, 0, DateTimeKind.Utc),
                                                             new DateTime(2007, 8, 4, 12, 0, 0, DateTimeKind.Utc)));
            ((Schedule)_scheduleRange).Add(ass);
            _person.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1),
                                       new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
                                       TeamFactory.CreateSimpleTeam()));


            var result = _target.Validate(_scheduleRange, null, new DateOnly(2007, 8, 4));
            Assert.IsFalse(result);
            Assert.IsFalse(String.IsNullOrEmpty(_target.ErrorMessage));
        }

        /// <summary>
        /// Verifies the validate returns true when rest is long enough.
        /// </summary>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-06-18    
        /// /// </remarks>
        [Test]
        public void VerifyValidateReturnsTrueWhenRestIsLongEnough()
        {
            _person.AddPersonPeriod(new PersonPeriod(
                                       new DateOnly(1900, 1, 1),
                                       new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
                                       TeamFactory.CreateSimpleTeam()));

            addPersonAssignmentsToSchedulePart();


            var result = _target.Validate(_scheduleRange, null, new DateOnly(2007, 8, 1));
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyMandatory()
        {
            Assert.IsFalse(_target.IsMandatory);
        }


        private void addPersonAssignmentsToSchedulePart()
        {
            IList<IPersonAssignment> assignments = new List<IPersonAssignment>();
            assignments.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person,
                                                                                  new DateTimePeriod(new DateTime(2007, 8, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 1, 17, 0, 0, DateTimeKind.Utc))));
            assignments.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person,
                                                                                  new DateTimePeriod(new DateTime(2007, 8, 2, 8, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 2, 17, 0, 0, DateTimeKind.Utc))));
            assignments.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person,
                                                                                  new DateTimePeriod(new DateTime(2007, 8, 3, 10, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 3, 19, 0, 0, DateTimeKind.Utc))));

            ((Schedule)_scheduleRange).AddRange(assignments);
        }

        private void addPersonAssignmentsForLongest()
        {
            IList<IPersonAssignment> assignments = new List<IPersonAssignment>();
            assignments.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person,
                                                                                  new DateTimePeriod(new DateTime(2007, 8, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 1, 17, 0, 0, DateTimeKind.Utc))));
            assignments.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person,
                                                                                  new DateTimePeriod(new DateTime(2007, 8, 3, 10, 0, 0, DateTimeKind.Utc), new DateTime(2007, 8, 3, 19, 0, 0, DateTimeKind.Utc))));

            ((Schedule)_scheduleRange).AddRange(assignments);
        }

       
    }
}