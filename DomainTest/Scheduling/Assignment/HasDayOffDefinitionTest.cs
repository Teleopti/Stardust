using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class HasDayOffDefinitionTest
    {
        private MockRepository _mocks;
        private HasDayOffDefinition _target;
        private IScheduleDay _scheduleDay;
        private IDateOnlyAsDateTimePeriod _dateOnlyPeriod;
        private DateOnly _dateOnly;
        private IPerson _person;
        private IPersonPeriod _personPeriod;
        private IPersonContract _personContract;
        private IContract _contract;
        private IContractSchedule _contractSchedule;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _target = new HasDayOffDefinition(_scheduleDay);
            _dateOnly = new DateOnly(2010, 12, 20);
            _dateOnlyPeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            _person = _mocks.StrictMock<IPerson>();
            _personPeriod = _mocks.StrictMock<IPersonPeriod>();
            _personContract = _mocks.StrictMock<IPersonContract>();
            _contract = _mocks.StrictMock<IContract>();
            _contractSchedule = _mocks.StrictMock<IContractSchedule>();
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIsScheduleDayIsNull()
        {
            _scheduleDay = null;
            _target = new HasDayOffDefinition(_scheduleDay);
        }

        
        [Test]
        public void ShouldReturnFalseIfNoPersonPeriod()
        {
            Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyPeriod);
            Expect.Call(_dateOnlyPeriod.DateOnly).Return(_dateOnly);
            Expect.Call(_scheduleDay.Person).Return(_person);
            Expect.Call(_person.Period(_dateOnly)).Return(null);
            _mocks.ReplayAll();
            Assert.That(_target.IsDayOff(),Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfNoPersonContract()
        {
            Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyPeriod);
            Expect.Call(_dateOnlyPeriod.DateOnly).Return(_dateOnly);
            Expect.Call(_scheduleDay.Person).Return(_person);
            Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
            Expect.Call(_personPeriod.PersonContract).Return(null);
            _mocks.ReplayAll();
            Assert.That(_target.IsDayOff(), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfNoContract()
        {
            Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyPeriod);
            Expect.Call(_dateOnlyPeriod.DateOnly).Return(_dateOnly);
            Expect.Call(_scheduleDay.Person).Return(_person);
            Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
            Expect.Call(_personPeriod.PersonContract).Return(_personContract);
            Expect.Call(_personContract.Contract).Return(null);
            _mocks.ReplayAll();
            Assert.That(_target.IsDayOff(), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfHourlyStaffed()
        {
            Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyPeriod);
            Expect.Call(_dateOnlyPeriod.DateOnly).Return(_dateOnly);
            Expect.Call(_scheduleDay.Person).Return(_person);
            Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
            Expect.Call(_personPeriod.PersonContract).Return(_personContract);
            Expect.Call(_personContract.Contract).Return(_contract);
            Expect.Call(_contract.EmploymentType).Return(EmploymentType.HourlyStaff);
            _mocks.ReplayAll();
            Assert.That(_target.IsDayOff(), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfNoContractSchedule()
        {
            Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyPeriod);
            Expect.Call(_dateOnlyPeriod.DateOnly).Return(_dateOnly);
            Expect.Call(_scheduleDay.Person).Return(_person);
            Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
            Expect.Call(_personPeriod.PersonContract).Return(_personContract);
            Expect.Call(_personContract.Contract).Return(_contract);
            Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
            Expect.Call(_personContract.ContractSchedule).Return(null);
            _mocks.ReplayAll();
            Assert.That(_target.IsDayOff(), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallContractScheduleIsWorkday()
        {
            Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyPeriod);
            Expect.Call(_dateOnlyPeriod.DateOnly).Return(_dateOnly);
            Expect.Call(_scheduleDay.Person).Return(_person);
            Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
            Expect.Call(_personPeriod.PersonContract).Return(_personContract);
            Expect.Call(_personContract.Contract).Return(_contract);
            Expect.Call(_contract.EmploymentType)
				.Return(EmploymentType.FixedStaffNormalWorkTime);
            Expect.Call(_personContract.ContractSchedule).Return(_contractSchedule);
			Expect.Call(_person.SchedulePeriodStartDate(_dateOnly))
				.Return(_dateOnly);
            Expect.Call(_contractSchedule.IsWorkday(_dateOnly, _dateOnly))
				.Return(false);
            _mocks.ReplayAll();
            Assert.That(_target.IsDayOff(), Is.True);
            _mocks.VerifyAll();
        }
    }
}