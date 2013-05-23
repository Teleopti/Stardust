using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class PreSchedulingStatusCheckerTest
    {
        private PreSchedulingStatusChecker _target;
        private MockRepository _mocks;
        private IScheduleDay _part;
        private ISchedulingOptions _schedulingOptions;
        private IWorkShiftFinderResult _finderResult;
        private DateTime _scheduleDate = new DateTime(2009,2,2,4,0,0,DateTimeKind.Utc);
        private DateTimePeriod _period;
        private readonly DateOnly _scheduleDateOnly = new DateOnly(2009,2,2);
        private IPerson _person;
        private TimeZoneInfo _TimeZoneInfo;
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
            _finderResult = new WorkShiftFinderResult(_person, _scheduleDateOnly);
            _period = new DateTimePeriod(_scheduleDate, _scheduleDate.AddDays(1));
            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
            _TimeZoneInfo = (zone);
            _personPeriod = _mocks.StrictMock<IPersonPeriod>();
            _schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

            var p = new DateOnlyAsDateTimePeriod(_scheduleDateOnly, _TimeZoneInfo);
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
            }
            
            var ret =_target.CheckStatus(_part, _finderResult, _schedulingOptions);
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
            }

			var ret = _target.CheckStatus(_part, _finderResult, _schedulingOptions);
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
                
            }

			var ret = _target.CheckStatus(_part, _finderResult, _schedulingOptions);
            Assert.IsFalse(ret);
            Assert.AreEqual(_person, _target.Person);
            Assert.AreEqual(_scheduleDateOnly, _target.ScheduleDateOnly);
            Assert.AreEqual(new DateTime(2009, 02,02, 04, 00, 00, 00, DateTimeKind.Utc), _target.ScheduleDayUtc);
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
                
            }

			var ret = _target.CheckStatus(_part, _finderResult, _schedulingOptions);
            Assert.IsFalse(ret);
        }

        [Test]
        public void PersonAssignmentsCheckWithNoPersonAssignmentReturnsTrue()
        {
            IList<IPersonAssignment> personAssignments = new List<IPersonAssignment>();
            var readOnlyAssignments = new ReadOnlyCollection<IPersonAssignment>(personAssignments);

            using (_mocks.Record())
            {
                Expect.Call(_part.PersonAssignmentCollection()).Return(readOnlyAssignments).Repeat.AtLeastOnce();
            }

            bool ret = PreSchedulingStatusChecker.CheckAssignments(_part);
            Assert.IsTrue(ret);
        }

        [Test]
        public void MoreThanOnePersonAssignmentsReturnTrue()
        {
            var personAssignment = _mocks.StrictMock<IPersonAssignment>();
            var personAssignment2 = _mocks.StrictMock<IPersonAssignment>();
            var period = new DateTimePeriod(_period.StartDateTime.AddHours(10), _period.StartDateTime.AddHours(12));
            IPersonalShift personalShift1 = PersonalShiftFactory.CreatePersonalShift(ActivityFactory.CreateActivity("Nursing"), period);
            IPersonalShift personalShift2 = PersonalShiftFactory.CreatePersonalShift(ActivityFactory.CreateActivity("Nursing"), period.MovePeriod(new TimeSpan(4,0,0)));
            IList<IPersonAssignment> personAssignments = new List<IPersonAssignment> { personAssignment, personAssignment2 };
            IList<IPersonalShift> personalShifts = new List<IPersonalShift>{personalShift1, personalShift2};
            var readOnlyCollection = new ReadOnlyCollection<IPersonalShift>(personalShifts);
            var readOnlyAssignments = new ReadOnlyCollection<IPersonAssignment>(personAssignments);

            using (_mocks.Record())
            {
                Expect.Call(_part.PersonAssignmentCollection()).Return(readOnlyAssignments).Repeat.AtLeastOnce();
                Expect.Call(personAssignment.PersonalShiftCollection).Return(readOnlyCollection).Repeat.AtLeastOnce();
                Expect.Call(personAssignment.ToMainShift()).Return(null).Repeat.AtLeastOnce();
            }

            bool ret = PreSchedulingStatusChecker.CheckAssignments(_part);
            Assert.IsTrue(ret);
        }

        [Test]
        public void PersonAssignmentsCheckWithNoMainReturnsTrue()
        {
            var personalShift = _mocks.StrictMock<IPersonalShift>();
            IList<IPersonalShift> personalShifts = new List<IPersonalShift> {personalShift};
            var readOnlyPersonalShifts = new ReadOnlyCollection<IPersonalShift>(personalShifts);

            var personAssignment = _mocks.StrictMock<IPersonAssignment>();
             IList<IPersonAssignment> personAssignments = new List<IPersonAssignment> {personAssignment};
            var readOnlyAssignments = new ReadOnlyCollection<IPersonAssignment>(personAssignments);

            using (_mocks.Record())
            {
                Expect.Call(_part.PersonAssignmentCollection()).Return(readOnlyAssignments).Repeat.AtLeastOnce();
                Expect.Call(personAssignment.ToMainShift()).Return(null);
                Expect.Call(personAssignment.PersonalShiftCollection).Return(readOnlyPersonalShifts);
            }
            
             bool ret = PreSchedulingStatusChecker.CheckAssignments(_part);
             Assert.IsTrue(ret);
        }

        [Test]
        public void PersonAssignmentsCheckWithMainReturnsFalse()
        {
             //_schedulingOptions.ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff;

            var personContract = _mocks.StrictMock<IPersonContract>();
            var contract = _mocks.StrictMock<IContract>();

            var personalShift = _mocks.StrictMock<IPersonalShift>();
            IList<IPersonalShift> personalShifts = new List<IPersonalShift> { personalShift };
            var readOnlyPersonalShifts = new ReadOnlyCollection<IPersonalShift>(personalShifts);

            var personAssignment = _mocks.StrictMock<IPersonAssignment>();
            IList<IPersonAssignment> personAssignments = new List<IPersonAssignment> { personAssignment };
            var readOnlyAssignments = new ReadOnlyCollection<IPersonAssignment>(personAssignments);

            var mainShift = _mocks.StrictMock<IMainShift>();
            using (_mocks.Record())
            {
                Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.Twice();
                Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod());
                Expect.Call(_person.Period(_scheduleDateOnly)).Return(_personPeriod);
                Expect.Call(_personPeriod.PersonContract).Return(personContract);
                Expect.Call(personContract.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffDayWorkTime);

                Expect.Call(_part.PersonAssignmentCollection()).Return(readOnlyAssignments).Repeat.AtLeastOnce();
                Expect.Call(personAssignment.ToMainShift()).Return(mainShift);
                Expect.Call(personAssignment.PersonalShiftCollection).Return(readOnlyPersonalShifts);
            }

			var ret = _target.CheckStatus(_part, _finderResult, _schedulingOptions);
            Assert.IsFalse(ret);
        }

        [Test]
        public void PartHavingDayOffReturnsFalse()
        {
            var personContract = _mocks.StrictMock<IPersonContract>();
            var contract = _mocks.StrictMock<IContract>();
            IList<IPersonAssignment> personAssignments = new List<IPersonAssignment>();
            var readOnlyAssignments = new ReadOnlyCollection<IPersonAssignment>(personAssignments);

            var personDayOff1 = _mocks.StrictMock<IPersonDayOff>();
            var personDayOff2 = _mocks.StrictMock<IPersonDayOff>();

            IList<IPersonDayOff> dayOffs = new List<IPersonDayOff>{personDayOff1, personDayOff2};
            var readOnlyPersonDayOffs = new ReadOnlyCollection<IPersonDayOff>(dayOffs);

            using (_mocks.Record())
            {
                Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.Twice();
                Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod());
                Expect.Call(_person.Period(_scheduleDateOnly)).Return(_personPeriod);
                Expect.Call(_personPeriod.PersonContract).Return(personContract);
                Expect.Call(personContract.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffDayWorkTime);
                Expect.Call(_part.PersonAssignmentCollection()).Return(readOnlyAssignments).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonDayOffCollection()).Return(readOnlyPersonDayOffs);
            }

			var ret = _target.CheckStatus(_part, _finderResult, _schedulingOptions);
            Assert.IsFalse(ret);
        }

        [Test]
        public void PersonPeriodWithoutRuleSetBagReturnsFalse()
        {
            var personContract = _mocks.StrictMock<IPersonContract>();
            var contract = _mocks.StrictMock<IContract>();
            IList<IPersonAssignment> personAssignments = new List<IPersonAssignment>();
            var readOnlyAssignments = new ReadOnlyCollection<IPersonAssignment>(personAssignments);

            IList<IPersonDayOff> dayOffs = new List<IPersonDayOff> ();
            var readOnlyPersonDayOffs = new ReadOnlyCollection<IPersonDayOff>(dayOffs);

            using (_mocks.Record())
            {
                Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.Twice();
                Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod());
                Expect.Call(_person.Period(_scheduleDateOnly)).Return(_personPeriod);
                Expect.Call(_personPeriod.PersonContract).Return(personContract);
                Expect.Call(personContract.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffDayWorkTime);
                Expect.Call(_part.PersonAssignmentCollection()).Return(readOnlyAssignments).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonDayOffCollection()).Return(readOnlyPersonDayOffs);
                Expect.Call(_personPeriod.RuleSetBag).Return(null);
            }

			var ret = _target.CheckStatus(_part, _finderResult, _schedulingOptions);
            Assert.IsFalse(ret);
        }

        [Test]
        public void WhenPeriodsAreOkAndNotAssignedAndRuleSetBagIsOkCheckReturnsTrue()
        {
            var personContract = _mocks.StrictMock<IPersonContract>();
            var contract = _mocks.StrictMock<IContract>();
            IList<IPersonAssignment> personAssignments = new List<IPersonAssignment>();
            var readOnlyAssignments = new ReadOnlyCollection<IPersonAssignment>(personAssignments);

            IList<IPersonDayOff> dayOffs = new List<IPersonDayOff>();
            var readOnlyPersonDayOffs = new ReadOnlyCollection<IPersonDayOff>(dayOffs);
            var ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
            using (_mocks.Record())
            {
                Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod);
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.Any();
                Expect.Call(_person.Period(_scheduleDateOnly)).Return(_personPeriod);
                Expect.Call(_personPeriod.PersonContract).Return(personContract);
                Expect.Call(personContract.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffDayWorkTime);
                Expect.Call(_part.PersonAssignmentCollection()).Return(readOnlyAssignments).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonDayOffCollection()).Return(readOnlyPersonDayOffs);
                Expect.Call(_personPeriod.RuleSetBag).Return(ruleSetBag);
            }

			var ret = _target.CheckStatus(_part, _finderResult, _schedulingOptions);
            Assert.IsTrue(ret);
        }
    }


  
}
