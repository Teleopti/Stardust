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
    public class PersonIsUserSpecificationTest
    {
        private PersonIsUserSpecification _target;
        private Team _team;
        private Person _personWithPersonPeriod;
        private Person _personWithoutPersonPeriodWithTerminalDate;
        private Person _personWithoutPersonPeriodWithoutTerminalDate;
        private DateOnly _terminalDate;

        [SetUp]
        public void Setup()
        {
            _personWithPersonPeriod = new Person();
            _team= new Team();
            IPersonPeriod personPeriod =
                PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 01, 01), _team);
            _personWithPersonPeriod.AddPersonPeriod(personPeriod);
            
            _personWithoutPersonPeriodWithTerminalDate = new Person();
            _terminalDate = new DateOnly(2003, 01, 01);
            _personWithoutPersonPeriodWithTerminalDate.TerminalDate = _terminalDate;

            _personWithoutPersonPeriodWithoutTerminalDate = new Person();
        }

        [Test]
        public void VerifyWithDateBeforeTerminalDate()
        {
            DateOnly queryDate = _terminalDate.AddDays(-1);
            _target = new PersonIsUserSpecification(queryDate);

            Assert.IsFalse(_target.IsSatisfiedBy(_personWithPersonPeriod));
            Assert.IsTrue(_target.IsSatisfiedBy(_personWithoutPersonPeriodWithTerminalDate));
            Assert.IsTrue(_target.IsSatisfiedBy(_personWithoutPersonPeriodWithoutTerminalDate));
        }

        [Test]
        public void VerifyWithDateOnTerminalDate()
        {
            DateOnly queryDate = _terminalDate;
            _target = new PersonIsUserSpecification(queryDate);

            Assert.IsFalse(_target.IsSatisfiedBy(_personWithPersonPeriod));
            Assert.IsTrue(_target.IsSatisfiedBy(_personWithoutPersonPeriodWithTerminalDate));
            Assert.IsTrue(_target.IsSatisfiedBy(_personWithoutPersonPeriodWithoutTerminalDate));
        }

        [Test]
        public void VerifyWithDateAfterTerminalDate()
        {
            DateOnly queryDate = _terminalDate.AddDays(1);
            _target = new PersonIsUserSpecification(queryDate);

            Assert.IsFalse(_target.IsSatisfiedBy(_personWithPersonPeriod));
            Assert.IsFalse(_target.IsSatisfiedBy(_personWithoutPersonPeriodWithTerminalDate));
            Assert.IsTrue(_target.IsSatisfiedBy(_personWithoutPersonPeriodWithoutTerminalDate));
        }

    }
}