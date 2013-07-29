using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class NewShiftCategoryLimitationRuleTest
    {
        private MockRepository _mocks;
        private INewBusinessRule _target;
        private IShiftCategoryLimitationChecker _limitationChecker;
        private Dictionary<IPerson, IScheduleRange> _dic;
        private IVirtualSchedulePeriodExtractor _virtualSchedulePeriodExtractor;
        private IShiftCategoryLimitation _limitation;
        IList<IShiftCategoryLimitation> _limits;
        private ReadOnlyCollection<IShiftCategoryLimitation> _limitations;
        private IShiftCategory _shiftCategory;
        private IPermissionInformation _permissionInformation;
        private TimeZoneInfo _timeZone;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _limitationChecker = _mocks.StrictMock<IShiftCategoryLimitationChecker>();
            _dic = _mocks.StrictMock<Dictionary<IPerson, IScheduleRange>>();
            _virtualSchedulePeriodExtractor = _mocks.StrictMock<IVirtualSchedulePeriodExtractor>();
            _target = new NewShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor);

            _limitation = _mocks.StrictMock<IShiftCategoryLimitation>();

            _limits = new List<IShiftCategoryLimitation> { _limitation };
            _limitations = new ReadOnlyCollection<IShiftCategoryLimitation>(_limits);
            _shiftCategory = new ShiftCategory("Dummy");
            _permissionInformation = _mocks.StrictMock<IPermissionInformation>();
            _timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
        }

        [Test]
        public void CanCreateNewShiftCategoryLimitationRule()
        {
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.IsMandatory);
            Assert.IsTrue(_target.HaltModify);
            Assert.AreEqual("", _target.ErrorMessage);
        }

        [Test]
        public void ValidateReturnsEmptyListWhenNotTooManyOfSameCategoryInPeriod()
        {
            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2009,2,2,2009,3,1);
            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            _dic = new Dictionary<IPerson, IScheduleRange>();
            _dic.Add(person,range);
            _target = new NewShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor);

            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

            var lstOfDays = new List<IScheduleDay> {scheduleDay, scheduleDay2};
            
            IVirtualSchedulePeriod vPeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
            IVirtualSchedulePeriod vPeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
            var vPeriods = new List<IVirtualSchedulePeriod> {vPeriod1, vPeriod2};
            var oldResponses = new List<IBusinessRuleResponse>();
		
            using (_mocks.Record())
            {
                Expect.Call(_virtualSchedulePeriodExtractor.CreateVirtualSchedulePeriodsFromScheduleDays(lstOfDays)).
                    Return(vPeriods);
                
                Expect.Call(vPeriod1.IsValid).Return(true);
                Expect.Call(vPeriod2.IsValid).Return(true);

                Expect.Call(vPeriod1.DateOnlyPeriod).Return(dateOnlyPeriod);
                Expect.Call(vPeriod2.DateOnlyPeriod).Return(dateOnlyPeriod);

                Expect.Call(vPeriod1.Person).Return(person);
                Expect.Call(vPeriod2.Person).Return(person);

                Expect.Call(vPeriod1.ShiftCategoryLimitationCollection()).Return(_limitations);
                Expect.Call(vPeriod2.ShiftCategoryLimitationCollection()).Return(_limitations);

                Expect.Call(_limitation.Weekly).Return(false).Repeat.Twice();
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses).Repeat.Twice();

                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

                IList<DateOnly> datesWithCategory;
                Expect.Call(_limitationChecker.IsShiftCategoryOverPeriodLimit(_limitation, dateOnlyPeriod, range, out datesWithCategory)).
                    Return(false).Repeat.Twice();
                
            }
            using (_mocks.Playback())
            {
                IEnumerable<IBusinessRuleResponse> ret = _target.Validate(_dic, lstOfDays);
                Assert.AreEqual(0, ret.Count());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ValidateReturnsListOfResponseWhenTooManyOfSameCategoryInPeriod()
        {
            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2009, 2, 2, 2009, 3, 1);
            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            _dic = new Dictionary<IPerson, IScheduleRange>();
            _dic.Add(person, range);
            _target = new NewShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor);

            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            //var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

            var lstOfDays = new List<IScheduleDay> { scheduleDay };

            IVirtualSchedulePeriod vPeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
            //IVirtualSchedulePeriod vPeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
            var vPeriods = new List<IVirtualSchedulePeriod> { vPeriod1 };
            IList<DateOnly> datesWithCategory = new List<DateOnly>();
            datesWithCategory.Add(new DateOnly(2009,2,5));
            datesWithCategory.Add(new DateOnly(2009,2,10));
            datesWithCategory.Add(new DateOnly(2009,2,12));
            var oldResponses = new List<IBusinessRuleResponse>();
		
            using (_mocks.Record())
            {
                Expect.Call(_virtualSchedulePeriodExtractor.CreateVirtualSchedulePeriodsFromScheduleDays(lstOfDays)).
                    Return(vPeriods);
                
                Expect.Call(vPeriod1.IsValid).Return(true);
                //Expect.Call(vPeriod2.IsValid).Return(true);

                Expect.Call(vPeriod1.DateOnlyPeriod).Return(dateOnlyPeriod);
                //Expect.Call(vPeriod2.DateOnlyPeriod).Return(dateOnlyPeriod);

                Expect.Call(vPeriod1.Person).Return(person).Repeat.AtLeastOnce();
                //Expect.Call(vPeriod2.Person).Return(person).Repeat.AtLeastOnce();

                Expect.Call(vPeriod1.ShiftCategoryLimitationCollection()).Return(_limitations);
                //Expect.Call(vPeriod2.ShiftCategoryLimitationCollection()).Return(_limitations);

                Expect.Call(_limitation.Weekly).Return(false);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(_limitationChecker.IsShiftCategoryOverPeriodLimit(_limitation, dateOnlyPeriod, range, out datesWithCategory)).
                    Return(true).OutRef(datesWithCategory);
                Expect.Call(_limitation.ShiftCategory).Return(_shiftCategory).Repeat.AtLeastOnce();
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();
                //Expect.Call(person.Equals(person)).Return(true).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                IEnumerable<IBusinessRuleResponse> ret = _target.Validate(_dic, lstOfDays);
                Assert.AreNotEqual(0, ret.Count());
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ValidateReturnsListWithTooManyShiftCategoryButNoErrorInList()
        {
            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2009, 2, 2, 2009, 3, 1);
            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            _dic = new Dictionary<IPerson, IScheduleRange>();
            _dic.Add(person, range);
            _target = new NewShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor);

            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

            var lstOfDays = new List<IScheduleDay> { scheduleDay, scheduleDay2 };

            IVirtualSchedulePeriod vPeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
            IVirtualSchedulePeriod vPeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
            var vPeriods = new List<IVirtualSchedulePeriod> { vPeriod1, vPeriod2 };
            IList<DateOnly> datesWithCategory = new List<DateOnly>();
            datesWithCategory.Add(new DateOnly(2009, 2, 5));
            datesWithCategory.Add(new DateOnly(2009, 2, 10));
            datesWithCategory.Add(new DateOnly(2009, 2, 12));
            var oldResponses = new List<IBusinessRuleResponse>();

            using (_mocks.Record())
            {
                Expect.Call(_virtualSchedulePeriodExtractor.CreateVirtualSchedulePeriodsFromScheduleDays(lstOfDays)).
                    Return(vPeriods);

                Expect.Call(vPeriod1.IsValid).Return(true);
                Expect.Call(vPeriod2.IsValid).Return(true);

                Expect.Call(vPeriod1.DateOnlyPeriod).Return(dateOnlyPeriod);
                Expect.Call(vPeriod2.DateOnlyPeriod).Return(dateOnlyPeriod);

                Expect.Call(vPeriod1.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(vPeriod2.Person).Return(person).Repeat.AtLeastOnce();

                Expect.Call(vPeriod1.ShiftCategoryLimitationCollection()).Return(_limitations);
                Expect.Call(vPeriod2.ShiftCategoryLimitationCollection()).Return(_limitations);

                Expect.Call(_limitation.Weekly).Return(false).Repeat.Twice();
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses).Repeat.Twice();
                Expect.Call(_limitationChecker.IsShiftCategoryOverPeriodLimit(_limitation, dateOnlyPeriod, range, out datesWithCategory)).
                    Return(true).Repeat.Twice().OutRef(datesWithCategory);
                Expect.Call(_limitation.ShiftCategory).Return(_shiftCategory).Repeat.AtLeastOnce();
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();
                Expect.Call(person.Equals(person)).Return(true).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                IEnumerable<IBusinessRuleResponse> ret = _target.Validate(_dic, lstOfDays);
                Assert.AreEqual( 0, ret.Count());
            }
        }

        
    }
}
