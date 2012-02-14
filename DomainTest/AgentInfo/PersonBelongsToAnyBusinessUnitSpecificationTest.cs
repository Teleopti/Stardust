using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo
{
    /// <summary>
    /// Tests for PersonBelongsToAnyBusinessUnitSpecificationTest
    /// </summary>
    [TestFixture]
    public class PersonBelongsToAnyBusinessUnitSpecificationTest
    {
        private PersonBelongsToAnyBusinessUnitSpecification _target;
        private DateTimePeriod _periodInQuestion;
        private Team _team;
        private Person _person;

        [SetUp]
        public void Setup()
        {
            _person = new Person();
            _team= new Team();
            IPersonPeriod personPeriod2000_2001 =
                PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 01, 01), _team);
            IPersonPeriod personPeriod2002 =
                PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2002, 01, 02), _team);
            _person.AddPersonPeriod(personPeriod2000_2001);
            _person.AddPersonPeriod(personPeriod2002);
            _person.TerminalDate = new DateOnly(2003, 01, 01);

        }

        [Test]
        public void VerifyPeriodBeforeStartDate()
        {
            _periodInQuestion = new DateTimePeriod(new DateTime(1997, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                new DateTime(1999, 12, 31, 0, 0, 0, DateTimeKind.Utc));
            _target = new PersonBelongsToAnyBusinessUnitSpecification(_periodInQuestion);

            Assert.IsFalse(_target.IsSatisfiedBy(_person));
        }

        [Test]
        public void VerifyPeriodAfterTerminationDate()
        {
            _periodInQuestion = new DateTimePeriod(new DateTime(2003, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                                new DateTime(2003, 12, 31, 0, 0, 0, DateTimeKind.Utc));
            _target = new PersonBelongsToAnyBusinessUnitSpecification(_periodInQuestion);

            Assert.IsFalse(_target.IsSatisfiedBy(_person));
        }

        [Test]
        public void VerifyPeriodStartBeforeStartDateEndsAfterTerminationDate()
        {
            _periodInQuestion = new DateTimePeriod(new DateTime(1999, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                                new DateTime(2003, 12, 31, 0, 0, 0, DateTimeKind.Utc));
            _target = new PersonBelongsToAnyBusinessUnitSpecification(_periodInQuestion);

            Assert.IsTrue(_target.IsSatisfiedBy(_person));
        }

        [Test]
        public void VerifyPeriodStartAfterStartDateEndsAfterTerminationDate()
        {
            _periodInQuestion = new DateTimePeriod(new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                                new DateTime(2003, 12, 31, 0, 0, 0, DateTimeKind.Utc));
            _target = new PersonBelongsToAnyBusinessUnitSpecification(_periodInQuestion);

            Assert.IsTrue(_target.IsSatisfiedBy(_person));
        }

        [Test]
        public void VerifyPeriodStartAfterStartDateEndsBeforeTerminationDate()
        {
            _periodInQuestion = new DateTimePeriod(new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                                new DateTime(2002, 12, 31, 0, 0, 0, DateTimeKind.Utc));
            _target = new PersonBelongsToAnyBusinessUnitSpecification(_periodInQuestion);

            Assert.IsTrue(_target.IsSatisfiedBy(_person));
        }
    }
}