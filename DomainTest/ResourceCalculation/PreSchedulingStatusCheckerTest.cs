using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class PreSchedulingStatusCheckerTest
    {
        private PreSchedulingStatusChecker _target;
        private MockRepository _mocks;
        private IScheduleDay _part;
        private SchedulingOptions _schedulingOptions;
        private readonly DateOnly _scheduleDateOnly = new DateOnly(2009,2,2);
        private IPerson _person;
        private TimeZoneInfo _timeZoneInfo;
        private IVirtualSchedulePeriod _schedulePeriod;
        private IPersonPeriod _personPeriod;

        [SetUp]
        public void Setup()
        {
            _schedulingOptions = new SchedulingOptions();
            _target = new PreSchedulingStatusChecker();
            _mocks = new MockRepository();
            _part = _mocks.StrictMock<IScheduleDay>();
            
            _person = _mocks.StrictMock<IPerson>();
            _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
            _personPeriod = _mocks.StrictMock<IPersonPeriod>();
            _schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

            var p = new DateOnlyAsDateTimePeriod(_scheduleDateOnly, _timeZoneInfo);
            Expect.Call(_part.DateOnlyAsPeriod).Return(p).Repeat.Any();
            Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
        }

        [Test]
        public void CanCreatePreSchedulingStatusChecker()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void SchedulePartWithPersonWithoutSchedulePeriodReturnsFalseOnCheck()
        {
            var virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            var personPeriod = _mocks.StrictMock<IPersonPeriod>();
            using (_mocks.Record())
            {
                Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(virtualSchedulePeriod);
                Expect.Call(virtualSchedulePeriod.IsValid).Return(false).Repeat.Twice();
                Expect.Call(_person.Period(_scheduleDateOnly)).Return(personPeriod);
				Expect.Call(_person.Id).Return(Guid.Empty).Repeat.Any();
			}

			var ret =_target.CheckStatus(_part, _schedulingOptions);
            Assert.IsFalse(ret);
        }

        [Test]
        public void SchedulePartWithPersonWithoutPersonPeriodReturnsFalseOnCheck()
        {
            using (_mocks.Record())
            {
                Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.IsValid).Return(false).Repeat.Twice();
                Expect.Call(_person.Period(_scheduleDateOnly)).Return(null);
				Expect.Call(_person.Id).Return(Guid.Empty).Repeat.Any();
			}

			var ret = _target.CheckStatus(_part,  _schedulingOptions);
            Assert.IsFalse(ret);
        }

        [Test]
        public void VerifyCorrectEmploymentTypeFixedStaffCheck()
        {
            _schedulingOptions.ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff;

            var personContract = _mocks.StrictMock<IPersonContract>();
            var contract = _mocks.StrictMock<IContract>();
            using (_mocks.Record())
            {
                Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod);
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.Any();
                Expect.Call(_person.Period(_scheduleDateOnly)).Return(_personPeriod);
                Expect.Call(_personPeriod.PersonContract).Return(personContract);
                Expect.Call(personContract.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.HourlyStaff);
				Expect.Call(_person.Id).Return(Guid.Empty).Repeat.Any();

			}

			var ret = _target.CheckStatus(_part, _schedulingOptions);
            Assert.IsFalse(ret);
        }

        [Test]
        public void VerifyCorrectEmploymentTypeHourlyStaffCheck()
        {
            _schedulingOptions.ScheduleEmploymentType = ScheduleEmploymentType.HourlyStaff;

            var personContract = _mocks.StrictMock<IPersonContract>();
            var contract = _mocks.StrictMock<IContract>();
            using (_mocks.Record())
            {
                Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod);
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.Any();
                Expect.Call(_person.Period(_scheduleDateOnly)).Return(_personPeriod);
                Expect.Call(_personPeriod.PersonContract).Return(personContract);
                Expect.Call(personContract.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffDayWorkTime);
				Expect.Call(_person.Id).Return(Guid.Empty).Repeat.Any();

			}

			var ret = _target.CheckStatus(_part,  _schedulingOptions);
            Assert.IsFalse(ret);
        }

        [Test]
        public void PersonAssignmentsCheckWithMainReturnsFalse()
        {
            var personContract = _mocks.StrictMock<IPersonContract>();
            var contract = _mocks.StrictMock<IContract>();

	        using (_mocks.Record())
	        {
		        Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod);
		        Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.Twice();
		        Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod());
		        Expect.Call(_person.Period(_scheduleDateOnly)).Return(_personPeriod);
		        Expect.Call(_personPeriod.PersonContract).Return(personContract);
		        Expect.Call(personContract.Contract).Return(contract);
		        Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffDayWorkTime);
		        Expect.Call(_part.IsScheduled()).Return(true);
				Expect.Call(_person.Id).Return(Guid.Empty).Repeat.Any();
			}

			var ret = _target.CheckStatus(_part, _schedulingOptions);
            Assert.IsFalse(ret);
        }

        [Test]
        public void PartHavingDayOffReturnsFalse()
        {
            var personContract = _mocks.StrictMock<IPersonContract>();
            var contract = _mocks.StrictMock<IContract>();

            using (_mocks.Record())
            {
                Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.Twice();
                Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod());
                Expect.Call(_person.Period(_scheduleDateOnly)).Return(_personPeriod);
                Expect.Call(_personPeriod.PersonContract).Return(personContract);
                Expect.Call(personContract.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffDayWorkTime);
                Expect.Call(_part.PersonAssignment()).Return(null).Repeat.AtLeastOnce();
                Expect.Call(_part.IsScheduled()).Return(true);
				Expect.Call(_person.Id).Return(Guid.Empty).Repeat.Any();
			}

			var ret = _target.CheckStatus(_part, _schedulingOptions);
            Assert.IsFalse(ret);
        }

        [Test]
        public void PersonPeriodWithoutRuleSetBagReturnsFalse()
        {
            var personContract = _mocks.StrictMock<IPersonContract>();
            var contract = _mocks.StrictMock<IContract>();

            using (_mocks.Record())
            {
                Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.Twice();
                Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod());
                Expect.Call(_person.Period(_scheduleDateOnly)).Return(_personPeriod);
                Expect.Call(_personPeriod.PersonContract).Return(personContract);
                Expect.Call(personContract.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffDayWorkTime);
                Expect.Call(_part.PersonAssignment()).Return(null).Repeat.AtLeastOnce();
				Expect.Call(_part.IsScheduled()).Return(false);
                Expect.Call(_personPeriod.RuleSetBag).Return(null);
				Expect.Call(_person.Id).Return(Guid.Empty).Repeat.Any();
			}

			var ret = _target.CheckStatus(_part, _schedulingOptions);
            Assert.IsFalse(ret);
        }

        [Test]
        public void WhenPeriodsAreOkAndNotAssignedAndRuleSetBagIsOkCheckReturnsTrue()
        {
            var personContract = _mocks.StrictMock<IPersonContract>();
            var contract = _mocks.StrictMock<IContract>();
            var ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
            using (_mocks.Record())
            {
                Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod);
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.Any();
                Expect.Call(_person.Period(_scheduleDateOnly)).Return(_personPeriod);
                Expect.Call(_personPeriod.PersonContract).Return(personContract);
                Expect.Call(personContract.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffDayWorkTime);
                Expect.Call(_part.PersonAssignment()).Return(null).Repeat.AtLeastOnce();
				Expect.Call(_part.IsScheduled()).Return(false);
                Expect.Call(_personPeriod.RuleSetBag).Return(ruleSetBag);
				Expect.Call(_person.Id).Return(Guid.Empty).Repeat.Any();
			}

			var ret = _target.CheckStatus(_part, _schedulingOptions);
            Assert.IsTrue(ret);
        }
    }


  
}
